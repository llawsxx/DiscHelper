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
        public int AllocatePolicy = 0;
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
        public static Settings LoadSettings(string filename)
        {
            try
            {
                using (FileStream fs = new FileStream(filename, FileMode.Open))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Settings));
                    return (Settings)serializer.Deserialize(fs);
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
        public long Capacity;
        public bool IsAvailable;
        public bool IsGenPar;
        public List<PersistedFileItem> FileItems = new List<PersistedFileItem>();
    }
}
