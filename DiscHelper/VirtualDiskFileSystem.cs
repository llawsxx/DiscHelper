using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
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
            public readonly Dictionary<string, Node> Children = new Dictionary<string, Node>(StringComparer.OrdinalIgnoreCase);
        }

        private sealed class Handle
        {
            public Node Node;
            public FileStream Stream;
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
        private FileSystemHost _host;
        private int _activeFileHandleCount;

        public string BackendRoot { get { return _backendRoot; } }
        public string MountPoint { get { return _host == null ? null : _host.MountPoint(); } }
        public int LastMountStatus { get; private set; }
        public int ActiveFileHandleCount { get { return Volatile.Read(ref _activeFileHandleCount); } }
        public event EventHandler ActiveFileHandleCountChanged;

        public VirtualDiskFileSystem(IEnumerable<DiscItem> discs, string backendRoot)
        {
            _backendRoot = Path.GetFullPath(backendRoot);
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
            host.PassQueryDirectoryPattern = true;
            host.FlushAndPurgeOnCleanup = true;
            return STATUS_SUCCESS;
        }

        public override int GetVolumeInfo(out VolumeInfo volumeInfo)
        {
            volumeInfo = new VolumeInfo();
            try
            {
                DriveInfo drive = new DriveInfo(Path.GetPathRoot(_backendRoot));
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
                if (!node.IsDirectory)
                    handle.Stream = new FileStream(node.IsMapping ? node.SourcePath : node.BackendPath, FileMode.Open,
                        node.IsMapping ? FileAccess.Read : FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete);
                fileNode = node;
                fileDesc = handle;
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

        public override int Create(string fileName, uint createOptions, uint grantedAccess, uint fileAttributes,
            byte[] securityDescriptor, ulong allocationSize, out object fileNode, out object fileDesc,
            out FileInfo fileInfo, out string normalizedName)
        {
            fileNode = fileDesc = null;
            fileInfo = new FileInfo();
            normalizedName = null;
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
            int count = (int)Math.Min((ulong)length, (ulong)node.Length - offset);
            byte[] bytes = new byte[count];
            handle.Stream.Seek(node.IsMapping ? node.SourceOffset + (long)offset : (long)offset, SeekOrigin.Begin);
            int read = handle.Stream.Read(bytes, 0, count);
            Marshal.Copy(bytes, 0, buffer, read);
            bytesTransferred = (uint)read;
            return STATUS_SUCCESS;
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
                if (handle.Stream != null) handle.Stream.Dispose();
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
            if (!node.IsMapping && lastWriteTime != 0)
            {
                node.LastWriteUtc = DateTime.FromFileTimeUtc((long)lastWriteTime);
                if (node.IsDirectory) Directory.SetLastWriteTimeUtc(node.BackendPath, node.LastWriteUtc);
                else File.SetLastWriteTimeUtc(node.BackendPath, node.LastWriteUtc);
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
            string result = Path.GetFullPath(Path.Combine(_backendRoot, Normalize(relative)));
            string rootWithSeparator = _backendRoot.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!result.StartsWith(rootWithSeparator, StringComparison.OrdinalIgnoreCase)) throw new IOException("虚拟磁盘路径超出 data 目录");
            return result;
        }

        private static string Normalize(string path) { return (path ?? string.Empty).Replace('/', '\\').Trim('\\'); }
        private static string ParentPath(string path) { int index = path.LastIndexOf('\\'); return index < 0 ? string.Empty : path.Substring(0, index); }
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
