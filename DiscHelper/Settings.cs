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
}
