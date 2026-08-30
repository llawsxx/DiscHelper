using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace DiscHelper
{
    public class Settings
    {
        public long DiskCapacity = 25000000000;
        public long MinDiscRedundant = 52428800;
        public long MaxDiscRedundant = 209715200;
        public string DiscNamePattern = "Bucket_{1}_Disc_{1:50}";
        public int AllocatePolicy = 9;
        public string OutputFolder = "DISC";
        public string ParExePath = @"MultiPar\par2j64.exe";
        public bool isMove = false;
        public bool GeneratePar = false;
        public bool isFirstFit = false;
        public bool isCutFile = false;
        public bool GenerateFileList = false;
        public bool GenerateMp4PlaybackHeaders = false;
        public long ReadBuffer = 1024 * 1024;
        public string ParArgument = "/sn32768";
        public string VirtualDiskDataPath = "data";
        public List<PersistedFileItem> SavedFiles = new List<PersistedFileItem>();
        public List<PersistedDiscItem> SavedDiscs = new List<PersistedDiscItem>();
        public int SavedSelectedDiscIndex = -1;
        public List<ComplexFileTemplate> ComplexFileTemplates = new List<ComplexFileTemplate>();
        // Previous successfully saved configurations, newest entry last.
        public List<SettingsHistoryEntry> ConfigHistory = new List<SettingsHistoryEntry>();
        // Configurations that can be restored after an undo, newest entry last.
        public List<SettingsHistoryEntry> ConfigRedoHistory = new List<SettingsHistoryEntry>();

        public const int MaxConfigHistoryCount = 30;

        /// <summary>
        /// Creates a deep copy of the current configuration without its history.
        /// History entries use this form to avoid recursively copying the history list.
        /// </summary>
        public SettingsSnapshot CreateSnapshot()
        {
            SettingsSnapshot snapshot = new SettingsSnapshot
            {
                DiskCapacity = DiskCapacity,
                MinDiscRedundant = MinDiscRedundant,
                MaxDiscRedundant = MaxDiscRedundant,
                DiscNamePattern = DiscNamePattern,
                AllocatePolicy = AllocatePolicy,
                OutputFolder = OutputFolder,
                ParExePath = ParExePath,
                isMove = isMove,
                GeneratePar = GeneratePar,
                isFirstFit = isFirstFit,
                isCutFile = isCutFile,
                GenerateFileList = GenerateFileList,
                GenerateMp4PlaybackHeaders = GenerateMp4PlaybackHeaders,
                ReadBuffer = ReadBuffer,
                ParArgument = ParArgument,
                VirtualDiskDataPath = VirtualDiskDataPath,
                SavedSelectedDiscIndex = SavedSelectedDiscIndex
            };

            snapshot.SavedFiles = (SavedFiles ?? new List<PersistedFileItem>()).Select(CloneFileItem).ToList();
            snapshot.SavedDiscs = (SavedDiscs ?? new List<PersistedDiscItem>()).Select(CloneDiscItem).ToList();
            snapshot.ComplexFileTemplates = (ComplexFileTemplates ?? new List<ComplexFileTemplate>())
                .Select(CloneTemplate).ToList();
            return snapshot;
        }

        public static List<SettingsHistoryEntry> CloneHistory(IEnumerable<SettingsHistoryEntry> history)
        {
            return (history ?? Enumerable.Empty<SettingsHistoryEntry>())
                .Where(item => item != null && item.Snapshot != null)
                .Select(item => new SettingsHistoryEntry
                {
                    SavedAt = item.SavedAt,
                    Snapshot = item.Snapshot.CreateSnapshot()
                }).ToList();
        }

        public void AddConfigHistory(SettingsSnapshot snapshot)
        {
            if (snapshot == null) return;
            List<SettingsHistoryEntry> updatedHistory = (ConfigHistory ?? new List<SettingsHistoryEntry>()).ToList();
            updatedHistory.Add(new SettingsHistoryEntry
            {
                SavedAt = DateTime.Now,
                Snapshot = snapshot.CreateSnapshot()
            });
            if (updatedHistory.Count > MaxConfigHistoryCount)
                updatedHistory.RemoveRange(0, updatedHistory.Count - MaxConfigHistoryCount);
            ConfigHistory = updatedHistory;
        }

        internal static PersistedFileItem CloneFileItem(PersistedFileItem item)
        {
            if (item == null) return null;
            return new PersistedFileItem
            {
                Name = item.Name,
                DestName = item.DestName,
                Size = item.Size,
                CreateTime = item.CreateTime,
                StartPos = item.StartPos,
                NoCut = item.NoCut,
                Priority = item.Priority,
                Command = item.Command,
                CommandExe = item.CommandExe,
                IsFirstCommand = item.IsFirstCommand,
                FileId = item.FileId
            };
        }

        internal static PersistedDiscItem CloneDiscItem(PersistedDiscItem item)
        {
            if (item == null) return null;
            return new PersistedDiscItem
            {
                Name = item.Name,
                OriginalName = item.OriginalName,
                Capacity = item.Capacity,
                IsAvailable = item.IsAvailable,
                IsGenPar = item.IsGenPar,
                FileItems = (item.FileItems ?? new List<PersistedFileItem>()).Select(CloneFileItem).ToList()
            };
        }

        internal static ComplexFileTemplate CloneTemplate(ComplexFileTemplate template)
        {
            if (template == null) return null;
            return new ComplexFileTemplate
            {
                Name = template.Name,
                FileInputReplaceStr = template.FileInputReplaceStr,
                FileInputListSep = template.FileInputListSep,
                InputOutputSizeRatio = template.InputOutputSizeRatio,
                CommandLine = template.CommandLine,
                CommandLineExe = template.CommandLineExe,
                OutputFileSuffix = template.OutputFileSuffix
            };
        }
        public static Settings LoadSettings(string filename)
        {
            try
            {
                using (FileStream fs = new FileStream(filename, FileMode.Open))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                    Settings settings = (Settings)serializer.Deserialize(fs);
                    if (settings == null) return new Settings();
                    if (settings.ConfigHistory == null) settings.ConfigHistory = new List<SettingsHistoryEntry>();
                    if (settings.ConfigRedoHistory == null) settings.ConfigRedoHistory = new List<SettingsHistoryEntry>();
                    settings.ConfigHistory.RemoveAll(item => item == null || item.Snapshot == null);
                    settings.ConfigRedoHistory.RemoveAll(item => item == null || item.Snapshot == null);
                    if (settings.ConfigHistory.Count > MaxConfigHistoryCount)
                        settings.ConfigHistory.RemoveRange(0, settings.ConfigHistory.Count - MaxConfigHistoryCount);
                    if (settings.ConfigRedoHistory.Count > MaxConfigHistoryCount)
                        settings.ConfigRedoHistory.RemoveRange(0, settings.ConfigRedoHistory.Count - MaxConfigHistoryCount);
                    return settings;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return new Settings();
        }

        public bool SaveSettings(string filename)
        {
            try
            {
                using (TextWriter writer = new StreamWriter(filename))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                    serializer.Serialize(writer, this);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return false;
        }

    }

    public class SettingsHistoryEntry
    {
        public DateTime SavedAt;
        public SettingsSnapshot Snapshot;
    }

    /// <summary>
    /// Serializable configuration snapshot. It intentionally excludes undo/redo history.
    /// </summary>
    public class SettingsSnapshot
    {
        public long DiskCapacity;
        public long MinDiscRedundant;
        public long MaxDiscRedundant;
        public string DiscNamePattern;
        public int AllocatePolicy;
        public string OutputFolder;
        public string ParExePath;
        public bool isMove;
        public bool GeneratePar;
        public bool isFirstFit;
        public bool isCutFile;
        public bool GenerateFileList;
        public bool GenerateMp4PlaybackHeaders;
        public long ReadBuffer;
        public string ParArgument;
        public string VirtualDiskDataPath;
        public List<PersistedFileItem> SavedFiles = new List<PersistedFileItem>();
        public List<PersistedDiscItem> SavedDiscs = new List<PersistedDiscItem>();
        public int SavedSelectedDiscIndex = -1;
        public List<ComplexFileTemplate> ComplexFileTemplates = new List<ComplexFileTemplate>();

        public SettingsSnapshot CreateSnapshot()
        {
            SettingsSnapshot snapshot = (SettingsSnapshot)MemberwiseClone();
            snapshot.SavedFiles = (SavedFiles ?? new List<PersistedFileItem>()).Select(Settings.CloneFileItem).ToList();
            snapshot.SavedDiscs = (SavedDiscs ?? new List<PersistedDiscItem>()).Select(Settings.CloneDiscItem).ToList();
            snapshot.ComplexFileTemplates = (ComplexFileTemplates ?? new List<ComplexFileTemplate>()).Select(Settings.CloneTemplate).ToList();
            return snapshot;
        }

        public bool ContentEquals(SettingsSnapshot other)
        {
            if (other == null) return false;
            return DiskCapacity == other.DiskCapacity &&
                MinDiscRedundant == other.MinDiscRedundant &&
                MaxDiscRedundant == other.MaxDiscRedundant &&
                string.Equals(DiscNamePattern, other.DiscNamePattern, StringComparison.Ordinal) &&
                AllocatePolicy == other.AllocatePolicy &&
                string.Equals(OutputFolder, other.OutputFolder, StringComparison.Ordinal) &&
                string.Equals(ParExePath, other.ParExePath, StringComparison.Ordinal) &&
                isMove == other.isMove && GeneratePar == other.GeneratePar && isFirstFit == other.isFirstFit &&
                isCutFile == other.isCutFile && GenerateFileList == other.GenerateFileList &&
                GenerateMp4PlaybackHeaders == other.GenerateMp4PlaybackHeaders && ReadBuffer == other.ReadBuffer &&
                string.Equals(ParArgument, other.ParArgument, StringComparison.Ordinal) &&
                string.Equals(VirtualDiskDataPath, other.VirtualDiskDataPath, StringComparison.Ordinal) &&
                SavedSelectedDiscIndex == other.SavedSelectedDiscIndex &&
                FileItemsEqual(SavedFiles, other.SavedFiles) &&
                DiscItemsEqual(SavedDiscs, other.SavedDiscs) &&
                TemplateItemsEqual(ComplexFileTemplates, other.ComplexFileTemplates);
        }

        private static bool FileItemsEqual(IList<PersistedFileItem> left, IList<PersistedFileItem> right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left == null || right == null || left.Count != right.Count) return false;
            for (int i = 0; i < left.Count; i++)
            {
                PersistedFileItem a = left[i], b = right[i];
                if (a == null || b == null)
                {
                    if (!ReferenceEquals(a, b)) return false;
                    continue;
                }
                if (!string.Equals(a.Name, b.Name, StringComparison.Ordinal) ||
                    !string.Equals(a.DestName, b.DestName, StringComparison.Ordinal) ||
                    a.Size != b.Size || a.CreateTime != b.CreateTime || a.StartPos != b.StartPos ||
                    a.NoCut != b.NoCut || a.Priority != b.Priority ||
                    !string.Equals(a.Command, b.Command, StringComparison.Ordinal) ||
                    !string.Equals(a.CommandExe, b.CommandExe, StringComparison.Ordinal) ||
                    a.IsFirstCommand != b.IsFirstCommand || a.FileId != b.FileId)
                    return false;
            }
            return true;
        }

        private static bool DiscItemsEqual(IList<PersistedDiscItem> left, IList<PersistedDiscItem> right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left == null || right == null || left.Count != right.Count) return false;
            for (int i = 0; i < left.Count; i++)
            {
                PersistedDiscItem a = left[i], b = right[i];
                if (a == null || b == null)
                {
                    if (!ReferenceEquals(a, b)) return false;
                    continue;
                }
                if (!string.Equals(a.Name, b.Name, StringComparison.Ordinal) ||
                    !string.Equals(a.OriginalName, b.OriginalName, StringComparison.Ordinal) ||
                    a.Capacity != b.Capacity || a.IsAvailable != b.IsAvailable || a.IsGenPar != b.IsGenPar ||
                    !FileItemsEqual(a.FileItems, b.FileItems))
                    return false;
            }
            return true;
        }

        private static bool TemplateItemsEqual(IList<ComplexFileTemplate> left, IList<ComplexFileTemplate> right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left == null || right == null || left.Count != right.Count) return false;
            for (int i = 0; i < left.Count; i++)
            {
                ComplexFileTemplate a = left[i], b = right[i];
                if (a == null || b == null)
                {
                    if (!ReferenceEquals(a, b)) return false;
                    continue;
                }
                if (!string.Equals(a.Name, b.Name, StringComparison.Ordinal) ||
                    !string.Equals(a.FileInputReplaceStr, b.FileInputReplaceStr, StringComparison.Ordinal) ||
                    !string.Equals(a.FileInputListSep, b.FileInputListSep, StringComparison.Ordinal) ||
                    a.InputOutputSizeRatio != b.InputOutputSizeRatio ||
                    !string.Equals(a.CommandLine, b.CommandLine, StringComparison.Ordinal) ||
                    !string.Equals(a.CommandLineExe, b.CommandLineExe, StringComparison.Ordinal) ||
                    !string.Equals(a.OutputFileSuffix, b.OutputFileSuffix, StringComparison.Ordinal))
                    return false;
            }
            return true;
        }

        public Settings ToSettings()
        {
            Settings settings = new Settings
            {
                DiskCapacity = DiskCapacity,
                MinDiscRedundant = MinDiscRedundant,
                MaxDiscRedundant = MaxDiscRedundant,
                DiscNamePattern = DiscNamePattern,
                AllocatePolicy = AllocatePolicy,
                OutputFolder = OutputFolder,
                ParExePath = ParExePath,
                isMove = isMove,
                GeneratePar = GeneratePar,
                isFirstFit = isFirstFit,
                isCutFile = isCutFile,
                GenerateFileList = GenerateFileList,
                GenerateMp4PlaybackHeaders = GenerateMp4PlaybackHeaders,
                ReadBuffer = ReadBuffer,
                ParArgument = ParArgument,
                VirtualDiskDataPath = VirtualDiskDataPath,
                SavedSelectedDiscIndex = SavedSelectedDiscIndex,
                SavedFiles = (SavedFiles ?? new List<PersistedFileItem>()).Select(Settings.CloneFileItem).ToList(),
                SavedDiscs = (SavedDiscs ?? new List<PersistedDiscItem>()).Select(Settings.CloneDiscItem).ToList(),
                ComplexFileTemplates = (ComplexFileTemplates ?? new List<ComplexFileTemplate>()).Select(Settings.CloneTemplate).ToList()
            };
            return settings;
        }
    }

    public class PersistedFileItem
    {
        public string Name;
        public string DestName;
        public long Size;
        public DateTime CreateTime;
        public long StartPos;
        public bool NoCut;
        public int Priority;
        public string Command;
        public string CommandExe;
        public bool IsFirstCommand;
        public int FileId;

        public static PersistedFileItem FromFileItem(FileItem item)
        {
            return new PersistedFileItem { Name = item.Name, DestName = item.DestName, Size = item.Size, CreateTime = item.CreateTime, StartPos = item.StartPos, NoCut = item.NoCut, Priority = item.Priority, Command = item.Command, CommandExe = item.CommandExe, IsFirstCommand = item.isFirstCommand, FileId = item.FileId };
        }

        public FileItem ToFileItem()
        {
            return new FileItem { Name = Name, DestName = DestName, Size = Size, CreateTime = CreateTime, StartPos = StartPos, NoCut = NoCut, Priority = Priority, Command = Command, CommandExe = CommandExe, isFirstCommand = IsFirstCommand, FileId = FileId };
        }
    }

    public class PersistedDiscItem
    {
        public string Name;
        public string OriginalName;
        public long Capacity;
        public bool IsAvailable;
        public bool IsGenPar;
        public List<PersistedFileItem> FileItems = new List<PersistedFileItem>();
    }
}
