using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace DiscHelper
{
    public sealed class Mp4PlaybackMetadata
    {
        public const string MetadataSuffix = ".dhmp4.xml";
        public const string HeaderSuffix = ".dhmp4.header";

        public int Version = 1;
        public string VirtualFileName;
        public long OriginalLength;
        public string HeaderFileName;
        public List<Mp4PlaybackSegment> Segments = new List<Mp4PlaybackSegment>();
    }

    public sealed class Mp4PlaybackSegment
    {
        public string FileName;
        public long Offset;
        public long Length;
        public int Index;
        public int Total;
    }

    internal sealed class Mp4HeaderExtent
    {
        public long VirtualOffset;
        public long SourceOffset;
        public long Length;
    }

    internal static class Mp4PlaybackPackage
    {
        private static readonly byte[] HeaderMagic = Encoding.ASCII.GetBytes("DHMP4H1\0");

        private sealed class BoxSpan
        {
            public long Offset;
            public long StoredLength;
            public string Type;
        }

        public static bool IsSupportedExtension(string path)
        {
            string extension = Path.GetExtension(path) ?? string.Empty;
            return extension.Equals(".mp4", StringComparison.OrdinalIgnoreCase) ||
                   extension.Equals(".mov", StringComparison.OrdinalIgnoreCase);
        }

        public static void Write(string sourcePath, string virtualFileName,
            IEnumerable<Mp4PlaybackSegment> segments, IEnumerable<string> targetDirectories)
        {
            var segmentList = segments.OrderBy(item => item.Offset).ToList();
            if (segmentList.Count < 2) throw new InvalidDataException("文件没有被分割为多个 Segment");

            long sourceLength = new System.IO.FileInfo(sourcePath).Length;
            ValidateSegments(segmentList, sourceLength);

            var targets = targetDirectories
                .Select(Path.GetFullPath)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (targets.Count == 0) return;

            string simpleName = Path.GetFileName(virtualFileName);
            if (string.IsNullOrEmpty(simpleName)) throw new InvalidDataException("虚拟文件名称无效");

            foreach (string target in targets) Directory.CreateDirectory(target);
            string firstHeaderPath = Path.Combine(targets[0], simpleName + Mp4PlaybackMetadata.HeaderSuffix);
            WriteHeader(sourcePath, firstHeaderPath, sourceLength);

            foreach (string target in targets.Skip(1))
            {
                string headerPath = Path.Combine(target, simpleName + Mp4PlaybackMetadata.HeaderSuffix);
                File.Copy(firstHeaderPath, headerPath, true);
            }

            var metadata = new Mp4PlaybackMetadata
            {
                VirtualFileName = simpleName,
                OriginalLength = sourceLength,
                HeaderFileName = simpleName + Mp4PlaybackMetadata.HeaderSuffix,
                Segments = segmentList
            };
            var serializer = new XmlSerializer(typeof(Mp4PlaybackMetadata));
            foreach (string target in targets)
            {
                string metadataPath = Path.Combine(target, simpleName + Mp4PlaybackMetadata.MetadataSuffix);
                using (var stream = new FileStream(metadataPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                    serializer.Serialize(stream, metadata);
            }
        }

        public static Mp4PlaybackMetadata ReadMetadata(string path)
        {
            var serializer = new XmlSerializer(typeof(Mp4PlaybackMetadata));
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            {
                var metadata = (Mp4PlaybackMetadata)serializer.Deserialize(stream);
                if (metadata == null || metadata.Version != 1 || metadata.OriginalLength < 0 ||
                    string.IsNullOrEmpty(metadata.VirtualFileName) || string.IsNullOrEmpty(metadata.HeaderFileName))
                    throw new InvalidDataException("MP4 播放描述格式无效");
                return metadata;
            }
        }

        public static List<Mp4HeaderExtent> ReadHeaderExtents(string path, long expectedLength)
        {
            var result = new List<Mp4HeaderExtent>();
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
            {
                byte[] magic = reader.ReadBytes(HeaderMagic.Length);
                if (!magic.SequenceEqual(HeaderMagic)) throw new InvalidDataException("MP4 头文件标识无效");
                long originalLength = reader.ReadInt64();
                int count = reader.ReadInt32();
                if (originalLength != expectedLength || count < 1 || count > 1000000)
                    throw new InvalidDataException("MP4 头文件与描述不匹配");

                for (int i = 0; i < count; i++)
                {
                    long virtualOffset = reader.ReadInt64();
                    long length = reader.ReadInt64();
                    long sourceOffset = stream.Position;
                    if (virtualOffset < 0 || length < 0 || virtualOffset > originalLength - length ||
                        sourceOffset > stream.Length - length)
                        throw new InvalidDataException("MP4 头文件区间无效");
                    result.Add(new Mp4HeaderExtent
                    {
                        VirtualOffset = virtualOffset,
                        SourceOffset = sourceOffset,
                        Length = length
                    });
                    stream.Seek(length, SeekOrigin.Current);
                }
                if (stream.Position != stream.Length) throw new InvalidDataException("MP4 头文件包含多余数据");
            }
            return result;
        }

        private static void ValidateSegments(IList<Mp4PlaybackSegment> segments, long sourceLength)
        {
            long offset = 0;
            for (int i = 0; i < segments.Count; i++)
            {
                Mp4PlaybackSegment segment = segments[i];
                if (segment == null || string.IsNullOrEmpty(segment.FileName) ||
                    Path.GetFileName(segment.FileName) != segment.FileName || segment.Offset != offset || segment.Length <= 0)
                    throw new InvalidDataException("Segment 描述不连续或文件名无效");
                offset += segment.Length;
            }
            if (offset != sourceLength) throw new InvalidDataException("Segment 总长度与源文件长度不一致");
        }

        private static void WriteHeader(string sourcePath, string outputPath, long sourceLength)
        {
            List<BoxSpan> spans;
            using (var source = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                spans = ScanTopLevelBoxes(source);

            if (!spans.Any(item => item.Type == "moov") || !spans.Any(item => item.Type == "mdat"))
                throw new InvalidDataException("不是受支持的 MP4/MOV：缺少 moov 或 mdat box");

            string temporaryPath = outputPath + ".tmp";
            try
            {
                using (var source = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var output = new FileStream(temporaryPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                using (var writer = new BinaryWriter(output, Encoding.UTF8, true))
                {
                    writer.Write(HeaderMagic);
                    writer.Write(sourceLength);
                    writer.Write(spans.Count);
                    byte[] buffer = new byte[1024 * 1024];
                    foreach (BoxSpan span in spans)
                    {
                        writer.Write(span.Offset);
                        writer.Write(span.StoredLength);
                        source.Seek(span.Offset, SeekOrigin.Begin);
                        CopyExactly(source, output, span.StoredLength, buffer);
                    }
                }
                File.Copy(temporaryPath, outputPath, true);
            }
            finally
            {
                if (File.Exists(temporaryPath)) File.Delete(temporaryPath);
            }
        }

        private static List<BoxSpan> ScanTopLevelBoxes(FileStream stream)
        {
            var result = new List<BoxSpan>();
            long length = stream.Length;
            long offset = 0;
            byte[] header = new byte[16];
            while (offset < length)
            {
                if (length - offset < 8) throw new InvalidDataException("MP4 顶层 box 头不完整");
                stream.Seek(offset, SeekOrigin.Begin);
                ReadExactly(stream, header, 0, 8);
                uint size32 = ReadUInt32BigEndian(header, 0);
                string type = Encoding.ASCII.GetString(header, 4, 4);
                long headerLength = 8;
                long boxLength;
                if (size32 == 1)
                {
                    ReadExactly(stream, header, 8, 8);
                    ulong size64 = ReadUInt64BigEndian(header, 8);
                    if (size64 > long.MaxValue) throw new InvalidDataException("MP4 box 过大");
                    boxLength = (long)size64;
                    headerLength = 16;
                }
                else
                {
                    boxLength = size32 == 0 ? length - offset : size32;
                }
                if (boxLength < headerLength || boxLength > length - offset)
                    throw new InvalidDataException("MP4 顶层 box 长度无效");

                // Keep padding box headers so a parser can still skip their zero-filled payload.
                long storedLength = type == "mdat" || type == "free" || type == "skip"
                    ? headerLength
                    : boxLength;
                if (storedLength > 0)
                    result.Add(new BoxSpan { Offset = offset, StoredLength = storedLength, Type = type });
                offset += boxLength;
            }
            return result;
        }

        private static void CopyExactly(Stream source, Stream destination, long count, byte[] buffer)
        {
            while (count > 0)
            {
                int requested = (int)Math.Min(count, buffer.Length);
                int read = source.Read(buffer, 0, requested);
                if (read <= 0) throw new EndOfStreamException();
                destination.Write(buffer, 0, read);
                count -= read;
            }
        }

        private static void ReadExactly(Stream stream, byte[] buffer, int offset, int count)
        {
            while (count > 0)
            {
                int read = stream.Read(buffer, offset, count);
                if (read <= 0) throw new EndOfStreamException();
                offset += read;
                count -= read;
            }
        }

        private static uint ReadUInt32BigEndian(byte[] data, int offset)
        {
            return ((uint)data[offset] << 24) | ((uint)data[offset + 1] << 16) |
                   ((uint)data[offset + 2] << 8) | data[offset + 3];
        }

        private static ulong ReadUInt64BigEndian(byte[] data, int offset)
        {
            return ((ulong)ReadUInt32BigEndian(data, offset) << 32) | ReadUInt32BigEndian(data, offset + 4);
        }
    }
}
