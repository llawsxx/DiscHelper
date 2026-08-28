using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Text.RegularExpressions;
using Fsp;
using VolumeInfo = Fsp.Interop.VolumeInfo;
using FileInfo = Fsp.Interop.FileInfo;

namespace DiscHelper
{
    internal sealed class VirtualDiskFileSystem : FileSystemBase
    {
        private sealed class Node
        {
            public string Name;
            public bool IsDirectory;
            public bool IsMapping;
            public string SourcePath;
            public long SourceOffset;
            public long Length;
            public string BackendPath;
            public DateTime LastWriteUtc;
            public Node Parent;
            public readonly List<SourceExtent> Extents = new List<SourceExtent>();
            public readonly Dictionary<string, Node> Children = new Dictionary<string, Node>(StringComparer.OrdinalIgnoreCase);
        }

        private sealed class SourceExtent
        {
            public long VirtualOffset;
            public string SourcePath;
            public long SourceOffset;
            public long Length;
        }

        private sealed class Handle
        {
            public Node Node;
            public FileStream Stream;
            public readonly Dictionary<string, FileStream> SourceStreams = new Dictionary<string, FileStream>(StringComparer.OrdinalIgnoreCase);
            public readonly object SyncRoot = new object();
            public bool DeletePending;
            public int IsCounted;
        }

        private sealed class DirectoryContext
        {
            public readonly List<Node> Children;
            public int Index;
            public DirectoryContext(List<Node> children) { Children = children; }
        }

        private readonly Node _root = new Node { IsDirectory = true };
        private readonly string _backendRoot;
        private readonly string _volumePath;
        private readonly bool _readOnlyView;
        private FileSystemHost _host;
        private int _activeFileHandleCount;

        public string BackendRoot { get { return _backendRoot; } }
        public string MountPoint { get { return _host == null ? null : _host.MountPoint(); } }
        public int LastMountStatus { get; private set; }
        public int ActiveFileHandleCount { get { return Volatile.Read(ref _activeFileHandleCount); } }
        public event EventHandler ActiveFileHandleCountChanged;
        public readonly List<string> ScanWarnings = new List<string>();

        public VirtualDiskFileSystem(IEnumerable<DiscItem> discs, string backendRoot)
        {
            _backendRoot = Path.GetFullPath(backendRoot);
            _volumePath = _backendRoot;
            Directory.CreateDirectory(_backendRoot);
            LoadBackendDirectory(new DirectoryInfo(_backendRoot), _root);

            foreach (DiscItem disc in (discs ?? Enumerable.Empty<DiscItem>()).Where(item => item.IsAvailable))
            {
                foreach (FileItem item in disc.FileItems)
                {
                    string name = item.DestName ?? Path.GetFileName(item.Name);
                    AddMapping(disc.Name + "\\" + name, item);
                }
            }
        }

        private VirtualDiskFileSystem(string sourceRoot)
        {
            _readOnlyView = true;
            _volumePath = Path.GetFullPath(sourceRoot);
            if (!Directory.Exists(_volumePath)) throw new DirectoryNotFoundException("Segment 文件夹不存在：" + _volumePath);
            LoadSegmentDirectory(new DirectoryInfo(_volumePath), _root);
        }

        public static VirtualDiskFileSystem CreateSegmentView(string sourceRoot)
        {
            return new VirtualDiskFileSystem(sourceRoot);
        }

        public bool Mount(string mountPoint)
        {
            if (_host != null) return false;
            _host = new FileSystemHost(this) { FileSystemName = "DiscHelperVFS" };
            LastMountStatus = _host.Mount(mountPoint, null, true, 0);
            if (LastMountStatus < 0)
            {
                _host.Dispose();
                _host = null;
                return false;
            }
            return true;
        }

        public void Unmount()
        {
            if (_host == null) return;
            _host.Unmount();
            _host.Dispose();
            _host = null;
        }

        public override int Init(object host0)
        {
            FileSystemHost host = (FileSystemHost)host0;
            host.SectorSize = 4096;
            host.SectorsPerAllocationUnit = 1;
            host.MaxComponentLength = 255;
            host.CaseSensitiveSearch = false;
            host.CasePreservedNames = true;
            host.UnicodeOnDisk = true;
            host.PersistentAcls = false;
            host.PostCleanupWhenModifiedOnly = true;
            // Let WinFsp apply directory wildcard filtering before callbacks.
            host.PassQueryDirectoryPattern = false;
            host.FlushAndPurgeOnCleanup = true;
            return STATUS_SUCCESS;
        }

        public override int GetVolumeInfo(out VolumeInfo volumeInfo)
        {
            volumeInfo = new VolumeInfo();
            try
            {
                DriveInfo drive = new DriveInfo(Path.GetPathRoot(_volumePath));
                volumeInfo.TotalSize = (ulong)drive.TotalSize;
                volumeInfo.FreeSize = (ulong)drive.AvailableFreeSpace;
            }
            catch { }
            return STATUS_SUCCESS;
        }

        public override int GetSecurityByName(string fileName, out uint attributes, ref byte[] securityDescriptor)
        {
            Node node = Find(fileName);
            if (node == null) { attributes = 0; return STATUS_OBJECT_NAME_NOT_FOUND; }
            attributes = Attributes(node);
            return STATUS_SUCCESS;
        }

        public override int Open(string fileName, uint createOptions, uint grantedAccess, out object fileNode, out object fileDesc, out FileInfo fileInfo, out string normalizedName)
        {
            fileNode = fileDesc = null;
            normalizedName = null;
            Node node = Find(fileName);
            if (node == null) { fileInfo = new FileInfo(); return STATUS_OBJECT_NAME_NOT_FOUND; }
            Handle handle = new Handle { Node = node };
            try
            {
                if (!node.IsDirectory && !node.IsMapping)
                    handle.Stream = new FileStream(node.BackendPath, FileMode.Open, FileAccess.ReadWrite,
                        FileShare.ReadWrite | FileShare.Delete);
                fileNode = node;
                fileDesc = handle;
                normalizedName = CanonicalPath(node);
                FillInfo(node, out fileInfo);
                TrackHandle(handle);
                return STATUS_SUCCESS;
            }
            catch
            {
                fileInfo = new FileInfo();
                return STATUS_OBJECT_NAME_NOT_FOUND;
            }
        }

        public override int Overwrite(object fileNode, object fileDesc, uint fileAttributes,
            bool replaceFileAttributes, ulong allocationSize, out FileInfo fileInfo)
        {
            Node node = (Node)fileNode;
            Handle handle = fileDesc as Handle;
            fileInfo = new FileInfo();
            if (node == null || node.IsDirectory || node.IsMapping || handle == null || handle.Stream == null)
                return STATUS_ACCESS_DENIED;

            try
            {
                handle.Stream.SetLength((long)allocationSize);
                handle.Stream.Position = 0;
                node.Length = (long)allocationSize;
                node.LastWriteUtc = DateTime.UtcNow;
                if (replaceFileAttributes)
                {
                    File.SetAttributes(node.BackendPath, (FileAttributes)fileAttributes);
                }
                File.SetLastWriteTimeUtc(node.BackendPath, node.LastWriteUtc);
                FillInfo(node, out fileInfo);
                return STATUS_SUCCESS;
            }
            catch (UnauthorizedAccessException)
            {
                return STATUS_ACCESS_DENIED;
            }
            catch (IOException)
            {
                return STATUS_ACCESS_DENIED;
            }
        }

        public override int Create(string fileName, uint createOptions, uint grantedAccess, uint fileAttributes,
            byte[] securityDescriptor, ulong allocationSize, out object fileNode, out object fileDesc,
            out FileInfo fileInfo, out string normalizedName)
        {
            fileNode = fileDesc = null;
            fileInfo = new FileInfo();
            normalizedName = null;
            if (_readOnlyView) return STATUS_ACCESS_DENIED;
            string relative = Normalize(fileName);
            if (string.IsNullOrEmpty(relative) || Find(relative) != null) return STATUS_OBJECT_NAME_COLLISION;
            Node parent = Find(ParentPath(relative));
            if (parent == null || !parent.IsDirectory) return STATUS_OBJECT_PATH_NOT_FOUND;

            bool directory = 0 != (createOptions & FILE_DIRECTORY_FILE);
            string backendPath = GetBackendPath(relative);
            Directory.CreateDirectory(Path.GetDirectoryName(backendPath));
            if (directory)
                Directory.CreateDirectory(backendPath);
            else
                using (FileStream stream = new FileStream(backendPath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete))
                    if (allocationSize > 0) stream.SetLength((long)allocationSize);

            Node node = AddNode(relative, directory, false, backendPath, null, 0, (long)allocationSize, DateTime.UtcNow);
            Handle handle = new Handle
            {
                Node = node,
                Stream = directory ? null : new FileStream(backendPath, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete)
            };
            fileNode = node;
            fileDesc = handle;
            normalizedName = CanonicalPath(node);
            FillInfo(node, out fileInfo);
            TrackHandle(handle);
            return STATUS_SUCCESS;
        }

        public override int Read(object fileNode, object fileDesc, IntPtr buffer, ulong offset, uint length, out uint bytesTransferred)
        {
            Node node = (Node)fileNode;
            Handle handle = (Handle)fileDesc;
            bytesTransferred = 0;
            if (node == null || node.IsDirectory || handle == null) return STATUS_ACCESS_DENIED;
            if (offset >= (ulong)node.Length) return STATUS_END_OF_FILE;
            int count = (int)Math.Min((ulong)int.MaxValue, Math.Min((ulong)length, (ulong)node.Length - offset));
            byte[] bytes = new byte[count];
            if (node.IsMapping)
            {
                try
                {
                    long requestStart = (long)offset;
                    long requestEnd = requestStart + count;
                    lock (handle.SyncRoot)
                    {
                        foreach (SourceExtent extent in node.Extents)
                        {
                            long extentEnd = extent.VirtualOffset + extent.Length;
                            long copyStart = Math.Max(requestStart, extent.VirtualOffset);
                            long copyEnd = Math.Min(requestEnd, extentEnd);
                            if (copyStart >= copyEnd) continue;
                            FileStream stream;
                            if (!handle.SourceStreams.TryGetValue(extent.SourcePath, out stream))
                            {
                                stream = new FileStream(extent.SourcePath, FileMode.Open, FileAccess.Read,
                                    FileShare.ReadWrite | FileShare.Delete);
                                handle.SourceStreams[extent.SourcePath] = stream;
                            }
                            stream.Seek(extent.SourceOffset + copyStart - extent.VirtualOffset, SeekOrigin.Begin);
                            int destinationOffset = (int)(copyStart - requestStart);
                            int remaining = (int)(copyEnd - copyStart);
                            while (remaining > 0)
                            {
                                int current = stream.Read(bytes, destinationOffset, remaining);
                                if (current <= 0) return STATUS_END_OF_FILE;
                                destinationOffset += current;
                                remaining -= current;
                            }
                        }
                    }
                }
                catch (FileNotFoundException) { return STATUS_OBJECT_NAME_NOT_FOUND; }
                catch (DirectoryNotFoundException) { return STATUS_OBJECT_PATH_NOT_FOUND; }
                catch (IOException) { return STATUS_ACCESS_DENIED; }
                Marshal.Copy(bytes, 0, buffer, count);
                bytesTransferred = (uint)count;
                return STATUS_SUCCESS;
            }

            int read = 0;
            lock (handle.SyncRoot)
            {
                handle.Stream.Seek((long)offset, SeekOrigin.Begin);
                while (read < count)
                {
                    int current = handle.Stream.Read(bytes, read, count - read);
                    if (current <= 0) break;
                    read += current;
                }
            }
            Marshal.Copy(bytes, 0, buffer, read);
            bytesTransferred = (uint)read;
            return read == count ? STATUS_SUCCESS : STATUS_END_OF_FILE;
        }

        public override int Write(object fileNode, object fileDesc, IntPtr buffer, ulong offset, uint length,
            bool writeToEndOfFile, bool constrainedIo, out uint bytesTransferred, out FileInfo fileInfo)
        {
            Node node = (Node)fileNode;
            Handle handle = (Handle)fileDesc;
            bytesTransferred = 0;
            fileInfo = new FileInfo();
            if (node == null || node.IsDirectory || node.IsMapping || handle == null || handle.Stream == null) return STATUS_ACCESS_DENIED;
            byte[] bytes = new byte[length];
            Marshal.Copy(buffer, bytes, 0, bytes.Length);
            handle.Stream.Seek(writeToEndOfFile ? 0 : (long)offset, writeToEndOfFile ? SeekOrigin.End : SeekOrigin.Begin);
            handle.Stream.Write(bytes, 0, bytes.Length);
            handle.Stream.Flush();
            node.Length = handle.Stream.Length;
            node.LastWriteUtc = DateTime.UtcNow;
            bytesTransferred = length;
            FillInfo(node, out fileInfo);
            return STATUS_SUCCESS;
        }

        public override int Flush(object fileNode, object fileDesc, out FileInfo fileInfo)
        {
            Node node = (Node)fileNode;
            Handle handle = fileDesc as Handle;
            if (handle != null && handle.Stream != null)
            {
                try
                {
                    handle.Stream.Flush();
                }
                catch (IOException)
                {
                    fileInfo = new FileInfo();
                    return STATUS_ACCESS_DENIED;
                }
            }
            if (node == null)
            {
                fileInfo = new FileInfo();
                return STATUS_SUCCESS;
            }
            FillInfo(node, out fileInfo);
            return STATUS_SUCCESS;
        }

        public override int GetFileInfo(object fileNode, object fileDesc, out FileInfo fileInfo)
        {
            FillInfo((Node)fileNode, out fileInfo);
            return STATUS_SUCCESS;
        }

        public override void Cleanup(object fileNode, object fileDesc, string fileName, uint flags)
        {
            Handle handle = fileDesc as Handle;
            if (handle != null && 0 != (flags & CleanupDelete)) DeleteNode(handle.Node);
        }

        public override void Close(object fileNode, object fileDesc)
        {
            Handle handle = fileDesc as Handle;
            if (handle == null) return;
            try
            {
                lock (handle.SyncRoot)
                {
                    if (handle.Stream != null) handle.Stream.Dispose();
                    foreach (FileStream stream in handle.SourceStreams.Values) stream.Dispose();
                    handle.SourceStreams.Clear();
                }
            }
            finally
            {
                ReleaseHandle(handle);
            }
        }

        public override bool ReadDirectoryEntry(object fileNode, object fileDesc, string pattern, string marker,
            ref object context, out string fileName, out FileInfo fileInfo)
        {
            Node node = (Node)fileNode;
            DirectoryContext directoryContext = context as DirectoryContext;
            if (directoryContext == null)
            {
                IEnumerable<Node> children = node.Children.Values.OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase);
                if (!string.IsNullOrEmpty(marker))
                    children = children.Where(item => StringComparer.OrdinalIgnoreCase.Compare(item.Name, marker) > 0);
                directoryContext = new DirectoryContext(children.ToList());
                context = directoryContext;
            }
            if (directoryContext.Index >= directoryContext.Children.Count)
            {
                fileName = null;
                fileInfo = new FileInfo();
                return false;
            }
            Node entry = directoryContext.Children[directoryContext.Index++];
            fileName = entry.Name;
            FillInfo(entry, out fileInfo);
            return true;
        }

        public override int SetFileSize(object fileNode, object fileDesc, ulong newSize, bool setAllocationSize, out FileInfo fileInfo)
        {
            Node node = (Node)fileNode;
            Handle handle = (Handle)fileDesc;
            if (node == null || node.IsMapping || handle == null || handle.Stream == null)
            {
                fileInfo = new FileInfo();
                return STATUS_ACCESS_DENIED;
            }
            handle.Stream.SetLength((long)newSize);
            node.Length = (long)newSize;
            FillInfo(node, out fileInfo);
            return STATUS_SUCCESS;
        }

        public override int SetBasicInfo(object fileNode, object fileDesc, uint fileAttributes, ulong creationTime,
            ulong lastAccessTime, ulong lastWriteTime, ulong changeTime, out FileInfo fileInfo)
        {
            Node node = (Node)fileNode;
            if (node == null)
            {
                fileInfo = new FileInfo();
                return STATUS_OBJECT_NAME_NOT_FOUND;
            }
            if (!node.IsMapping)
            {
                try
                {
                    if (fileAttributes != uint.MaxValue)
                    {
                        File.SetAttributes(node.BackendPath, (FileAttributes)fileAttributes);
                    }
                    if (creationTime != 0)
                    {
                        DateTime value = DateTime.FromFileTimeUtc((long)creationTime);
                        if (node.IsDirectory) Directory.SetCreationTimeUtc(node.BackendPath, value);
                        else File.SetCreationTimeUtc(node.BackendPath, value);
                    }
                    if (lastAccessTime != 0)
                    {
                        DateTime value = DateTime.FromFileTimeUtc((long)lastAccessTime);
                        if (node.IsDirectory) Directory.SetLastAccessTimeUtc(node.BackendPath, value);
                        else File.SetLastAccessTimeUtc(node.BackendPath, value);
                    }
                    if (lastWriteTime != 0)
                    {
                        node.LastWriteUtc = DateTime.FromFileTimeUtc((long)lastWriteTime);
                        if (node.IsDirectory) Directory.SetLastWriteTimeUtc(node.BackendPath, node.LastWriteUtc);
                        else File.SetLastWriteTimeUtc(node.BackendPath, node.LastWriteUtc);
                    }
                }
                catch (UnauthorizedAccessException)
                {
                    fileInfo = new FileInfo();
                    return STATUS_ACCESS_DENIED;
                }
                catch (IOException)
                {
                    fileInfo = new FileInfo();
                    return STATUS_ACCESS_DENIED;
                }
            }
            FillInfo(node, out fileInfo);
            return STATUS_SUCCESS;
        }

        public override int CanDelete(object fileNode, object fileDesc, string fileName)
        {
            Node node = (Node)fileNode;
            if (node == null || node == _root) return STATUS_ACCESS_DENIED;
            return node.IsDirectory && node.Children.Count > 0 ? STATUS_DIRECTORY_NOT_EMPTY : STATUS_SUCCESS;
        }

        public override int SetDelete(object fileNode, object fileDesc, string fileName, bool deleteFile)
        {
            int status = deleteFile ? CanDelete(fileNode, fileDesc, fileName) : STATUS_SUCCESS;
            Handle handle = fileDesc as Handle;
            if (status >= 0 && handle != null) handle.DeletePending = deleteFile;
            return status;
        }

        public override int Rename(object fileNode, object fileDesc, string fileName, string newFileName, bool replaceIfExists)
        {
            Node node = (Node)fileNode;
            string relative = Normalize(newFileName);
            Node parent = Find(ParentPath(relative));
            if (node == null || node == _root || parent == null || !parent.IsDirectory) return STATUS_OBJECT_PATH_NOT_FOUND;
            string newName = Path.GetFileName(relative);
            Node existing;
            if (parent.Children.TryGetValue(newName, out existing))
            {
                if (!replaceIfExists) return STATUS_OBJECT_NAME_COLLISION;
                int deleteStatus = CanDelete(existing, null, null);
                if (deleteStatus < 0) return deleteStatus;
                DeleteNode(existing);
            }

            if (!node.IsMapping)
            {
                string newBackendPath = GetBackendPath(relative);
                Directory.CreateDirectory(Path.GetDirectoryName(newBackendPath));
                if (node.IsDirectory) Directory.Move(node.BackendPath, newBackendPath);
                else File.Move(node.BackendPath, newBackendPath);
                UpdateBackendPaths(node, node.BackendPath, newBackendPath);
            }

            node.Parent.Children.Remove(node.Name);
            node.Parent = parent;
            node.Name = newName;
            parent.Children[newName] = node;
            return STATUS_SUCCESS;
        }

        private void LoadBackendDirectory(DirectoryInfo directory, Node parent)
        {
            foreach (DirectoryInfo childDirectory in directory.EnumerateDirectories())
            {
                Node node = new Node { Name = childDirectory.Name, IsDirectory = true, BackendPath = childDirectory.FullName, LastWriteUtc = childDirectory.LastWriteTimeUtc, Parent = parent };
                parent.Children[node.Name] = node;
                LoadBackendDirectory(childDirectory, node);
            }
            foreach (System.IO.FileInfo file in directory.EnumerateFiles())
            {
                parent.Children[file.Name] = new Node { Name = file.Name, BackendPath = file.FullName, Length = file.Length, LastWriteUtc = file.LastWriteTimeUtc, Parent = parent };
            }
        }

        private void LoadSegmentDirectory(DirectoryInfo directory, Node parent)
        {
            var files = directory.EnumerateFiles().ToList();
            var hidden = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (System.IO.FileInfo metadataFile in files.Where(file =>
                file.Name.EndsWith(Mp4PlaybackMetadata.MetadataSuffix, StringComparison.OrdinalIgnoreCase)))
            {
                hidden.Add(metadataFile.Name);
                try
                {
                    Mp4PlaybackMetadata metadata = Mp4PlaybackPackage.ReadMetadata(metadataFile.FullName);
                    string headerName = Path.GetFileName(metadata.HeaderFileName);
                    if (!string.Equals(headerName, metadata.HeaderFileName, StringComparison.Ordinal))
                        throw new InvalidDataException("头文件名称必须是简单文件名");
                    string headerPath = Path.Combine(directory.FullName, headerName);
                    hidden.Add(headerName);

                    string virtualName = Path.GetFileName(metadata.VirtualFileName);
                    if (!string.Equals(virtualName, metadata.VirtualFileName, StringComparison.Ordinal) ||
                        string.IsNullOrEmpty(virtualName))
                        throw new InvalidDataException("虚拟文件名称无效");

                    var node = new Node
                    {
                        Name = virtualName,
                        Parent = parent,
                        IsMapping = true,
                        Length = metadata.OriginalLength,
                        LastWriteUtc = metadataFile.LastWriteTimeUtc
                    };
                    foreach (Mp4HeaderExtent extent in Mp4PlaybackPackage.ReadHeaderExtents(headerPath, metadata.OriginalLength))
                    {
                        node.Extents.Add(new SourceExtent
                        {
                            VirtualOffset = extent.VirtualOffset,
                            SourcePath = headerPath,
                            SourceOffset = extent.SourceOffset,
                            Length = extent.Length
                        });
                    }
                    foreach (Mp4PlaybackSegment segment in metadata.Segments ?? new List<Mp4PlaybackSegment>())
                    {
                        if (segment == null || segment.Offset < 0 || segment.Length <= 0 ||
                            segment.Offset > metadata.OriginalLength - segment.Length)
                        {
                            ScanWarnings.Add(metadataFile.FullName + " 包含无效的 Segment 区间");
                            continue;
                        }
                        if (string.IsNullOrEmpty(segment.FileName)) continue;
                        string segmentName = Path.GetFileName(segment.FileName);
                        if (!string.Equals(segmentName, segment.FileName, StringComparison.Ordinal)) continue;
                        string segmentPath = Path.Combine(directory.FullName, segmentName);
                        if (!File.Exists(segmentPath)) continue;
                        long available = new System.IO.FileInfo(segmentPath).Length;
                        if (available < segment.Length)
                        {
                            ScanWarnings.Add(segmentPath + " 长度不足，已忽略");
                            continue;
                        }
                        hidden.Add(segmentName);
                        node.Extents.Add(new SourceExtent
                        {
                            VirtualOffset = segment.Offset,
                            SourcePath = segmentPath,
                            SourceOffset = 0,
                            Length = segment.Length
                        });
                    }
                    if (!parent.Children.ContainsKey(node.Name)) parent.Children[node.Name] = node;
                }
                catch (Exception ex)
                {
                    ScanWarnings.Add(metadataFile.FullName + "：" + ex.Message);
                }
            }

            var segmentPattern = new Regex(@"^(?<name>.+)\.Segment_(?<index>\d+)(?:_of_(?<total>\d+))?$",
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            var segmentGroups = files
                .Where(file => !hidden.Contains(file.Name))
                .Select(file => new { File = file, Match = segmentPattern.Match(file.Name) })
                .Where(item => item.Match.Success)
                .GroupBy(item => item.Match.Groups["name"].Value, StringComparer.OrdinalIgnoreCase);

            foreach (var group in segmentGroups)
            {
                var ordered = group.OrderBy(item => ParseSegmentNumber(item.Match.Groups["index"].Value))
                    .ThenBy(item => item.File.Name, StringComparer.OrdinalIgnoreCase).ToList();
                var indices = ordered.Select(item => ParseSegmentNumber(item.Match.Groups["index"].Value)).ToList();
                long? declaredTotal = ordered
                    .Where(item => item.Match.Groups["total"].Success)
                    .Select(item => (long?)ParseSegmentNumber(item.Match.Groups["total"].Value))
                    .FirstOrDefault();
                bool complete = indices.Count > 0 && indices[0] == 1 &&
                    indices.Distinct().Count() == indices.Count &&
                    indices.Select((value, index) => value == index + 1).All(value => value) &&
                    (!declaredTotal.HasValue || declaredTotal.Value == indices.Count) &&
                    ordered.Where(item => item.Match.Groups["total"].Success)
                        .All(item => ParseSegmentNumber(item.Match.Groups["total"].Value) == declaredTotal.Value);
                if (!complete)
                {
                    ScanWarnings.Add(Path.Combine(directory.FullName, group.Key) + " 的 Segment 不完整，已保留原文件");
                    continue;
                }
                foreach (var item in ordered) hidden.Add(item.File.Name);
                if (parent.Children.ContainsKey(group.Key)) continue;
                var node = new Node
                {
                    Name = group.Key,
                    Parent = parent,
                    IsMapping = true,
                    LastWriteUtc = ordered.Max(item => item.File.LastWriteTimeUtc)
                };
                long virtualOffset = 0;
                foreach (var item in ordered)
                {
                    node.Extents.Add(new SourceExtent
                    {
                        VirtualOffset = virtualOffset,
                        SourcePath = item.File.FullName,
                        SourceOffset = 0,
                        Length = item.File.Length
                    });
                    virtualOffset += item.File.Length;
                }
                node.Length = virtualOffset;
                parent.Children[node.Name] = node;
            }

            foreach (System.IO.FileInfo file in files.Where(file => !hidden.Contains(file.Name)))
            {
                if (parent.Children.ContainsKey(file.Name)) continue;
                var node = new Node
                {
                    Name = file.Name,
                    Parent = parent,
                    IsMapping = true,
                    Length = file.Length,
                    LastWriteUtc = file.LastWriteTimeUtc
                };
                node.Extents.Add(new SourceExtent { SourcePath = file.FullName, Length = file.Length });
                parent.Children[node.Name] = node;
            }

            foreach (DirectoryInfo childDirectory in directory.EnumerateDirectories())
            {
                var node = new Node
                {
                    Name = childDirectory.Name,
                    IsDirectory = true,
                    IsMapping = true,
                    LastWriteUtc = childDirectory.LastWriteTimeUtc,
                    Parent = parent
                };
                parent.Children[node.Name] = node;
                LoadSegmentDirectory(childDirectory, node);
            }
        }

        private void AddMapping(string path, FileItem item)
        {
            string relative = Normalize(path);
            string[] parts = relative.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) return;
            Node parent = _root;
            for (int i = 0; i < parts.Length - 1; i++) parent = EnsureDirectory(parent, parts[i], true);
            if (parent.Children.ContainsKey(parts[parts.Length - 1])) return;
            parent.Children[parts[parts.Length - 1]] = new Node
            {
                Name = parts[parts.Length - 1], Parent = parent, IsMapping = true, SourcePath = item.Name,
                SourceOffset = Math.Max(0, item.StartPos), Length = item.Size, LastWriteUtc = item.CreateTime.ToUniversalTime()
            };
            Node node = parent.Children[parts[parts.Length - 1]];
            node.Extents.Add(new SourceExtent
            {
                VirtualOffset = 0,
                SourcePath = item.Name,
                SourceOffset = Math.Max(0, item.StartPos),
                Length = item.Size
            });
        }

        private Node AddNode(string path, bool directory, bool mapping, string backendPath, string sourcePath,
            long sourceOffset, long length, DateTime lastWriteUtc)
        {
            string[] parts = Normalize(path).Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
            Node parent = _root;
            for (int i = 0; i < parts.Length - 1; i++) parent = EnsureDirectory(parent, parts[i], true);
            Node node = new Node { Name = parts[parts.Length - 1], Parent = parent, IsDirectory = directory, IsMapping = mapping, BackendPath = backendPath, SourcePath = sourcePath, SourceOffset = sourceOffset, Length = length, LastWriteUtc = lastWriteUtc };
            parent.Children[node.Name] = node;
            return node;
        }

        private static Node EnsureDirectory(Node parent, string name, bool mapping)
        {
            Node node;
            if (!parent.Children.TryGetValue(name, out node))
            {
                node = new Node { Name = name, Parent = parent, IsDirectory = true, IsMapping = mapping };
                parent.Children[name] = node;
            }
            return node;
        }

        private Node Find(string path)
        {
            Node node = _root;
            foreach (string part in Normalize(path).Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries))
            {
                Node next;
                if (!node.Children.TryGetValue(part, out next)) return null;
                node = next;
            }
            return node;
        }

        private static string CanonicalPath(Node node)
        {
            if (node == null || node.Parent == null) return "\\";
            var parts = new Stack<string>();
            Node current = node;
            while (current != null && current.Parent != null)
            {
                parts.Push(current.Name);
                current = current.Parent;
            }
            return "\\" + string.Join("\\", parts.ToArray());
        }

        private void DeleteNode(Node node)
        {
            if (node == null || node.Parent == null) return;
            if (!node.IsMapping)
            {
                if (node.IsDirectory) Directory.Delete(node.BackendPath, false);
                else File.Delete(node.BackendPath);
            }
            node.Parent.Children.Remove(node.Name);
        }

        private static void UpdateBackendPaths(Node node, string oldPrefix, string newPrefix)
        {
            if (!string.IsNullOrEmpty(node.BackendPath) && node.BackendPath.StartsWith(oldPrefix, StringComparison.OrdinalIgnoreCase))
                node.BackendPath = newPrefix + node.BackendPath.Substring(oldPrefix.Length);
            foreach (Node child in node.Children.Values) UpdateBackendPaths(child, oldPrefix, newPrefix);
        }

        private void TrackHandle(Handle handle)
        {
            if (handle == null || handle.Node == null || handle.Node.IsDirectory) return;
            handle.IsCounted = 1;
            Interlocked.Increment(ref _activeFileHandleCount);
            OnActiveFileHandleCountChanged();
        }

        private void ReleaseHandle(Handle handle)
        {
            if (handle == null || Interlocked.Exchange(ref handle.IsCounted, 0) == 0) return;
            Interlocked.Decrement(ref _activeFileHandleCount);
            OnActiveFileHandleCountChanged();
        }

        private void OnActiveFileHandleCountChanged()
        {
            EventHandler handler = ActiveFileHandleCountChanged;
            if (handler != null) handler(this, EventArgs.Empty);
        }

        private string GetBackendPath(string relative)
        {
            if (_readOnlyView || string.IsNullOrEmpty(_backendRoot)) throw new IOException("只读虚拟磁盘不能创建文件");
            string result = Path.GetFullPath(Path.Combine(_backendRoot, Normalize(relative)));
            string rootWithSeparator = _backendRoot.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!result.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase)) throw new IOException("虚拟磁盘路径超出 data 目录");
            return result;
        }

        private static string Normalize(string path) { return (path ?? string.Empty).Replace('/', '\\').Trim('\\'); }
        private static string ParentPath(string path) { int index = path.LastIndexOf('\\'); return index < 0 ? string.Empty : path.Substring(0, index); }
        private static long ParseSegmentNumber(string value) { long result; return long.TryParse(value, out result) ? result : long.MaxValue; }
        private static uint Attributes(Node node) { return (uint)(node.IsDirectory ? FileAttributes.Directory : FileAttributes.Archive); }
        private static void FillInfo(Node node, out FileInfo info)
        {
            info = new FileInfo { FileAttributes = Attributes(node), FileSize = node.IsDirectory ? 0UL : (ulong)Math.Max(0, node.Length) };
            info.AllocationSize = (info.FileSize + 4095) / 4096 * 4096;
            long time = (node.LastWriteUtc == default(DateTime) ? DateTime.UtcNow : node.LastWriteUtc).ToFileTimeUtc();
            info.CreationTime = info.LastAccessTime = info.LastWriteTime = info.ChangeTime = (ulong)time;
            info.HardLinks = 1;
        }
    }
}
