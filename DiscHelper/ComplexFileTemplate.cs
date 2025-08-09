using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiscHelper
{
    public class ComplexFileTemplate
    {
        public string Name;
        public string FileInputReplaceStr;
        public string FileInputListSep;
        public decimal InputOutputSizeRatio = 1;
        public string CommandLine;
        public string CommandLineExe;
        public string OutputFileSuffix;
        public override string ToString()
        {
            return Name;
        }

        public static long ComputeInputTotalSize(List<FileItem> fileItems)
        {
            long totalSize = 0;
            foreach (FileItem fileItem in fileItems)
            {
                totalSize += fileItem.Size;
            }
            return totalSize;
        }
        public static FileItem GenerateComplexFileItem(List<FileItem> inputFileItems, string commandExe, string command, string inputReplace, string sep, string suffix, double inputOuputRatio)
        {
            if (commandExe == null || commandExe == "") return null;
            string inputs = String.Join(sep, inputFileItems.Select(x => x.Name).ToArray());
            string outputCommand = command.Replace(inputReplace, inputs);
            string outputRelativeName;
            long totalSize = ComputeInputTotalSize(inputFileItems);
            long predictedTotalSize = (long)(inputOuputRatio * totalSize);
            if (inputFileItems.Count > 0)
            {
                string FirstFileItemName = inputFileItems[0].DestName;
                if (inputFileItems.Count == 1)
                {
                    string FirstFileItemFolder = Path.GetDirectoryName(FirstFileItemName);
                    string FirstFileItemNameNoExt = Path.GetFileNameWithoutExtension(FirstFileItemName);
                    outputRelativeName = Path.Combine(FirstFileItemFolder, String.Format("{0}{1}", FirstFileItemNameNoExt, suffix));
                }
                else
                {
                    string LastFileItemName = inputFileItems[inputFileItems.Count - 1].DestName;
                    string FirstFileItemFolder = Path.GetDirectoryName(FirstFileItemName);
                    string FirstFileItemNameNoExt = Path.GetFileNameWithoutExtension(FirstFileItemName);
                    string LastFileItemNameNoExt = Path.GetFileNameWithoutExtension(LastFileItemName);
                    outputRelativeName = Path.Combine(FirstFileItemFolder, String.Format("{0}-{1}{2}", FirstFileItemNameNoExt, LastFileItemNameNoExt, suffix));
                }
                FileItem NewFileItem = new FileItem();
                NewFileItem.Name = outputRelativeName;
                NewFileItem.DestName = outputRelativeName;
                NewFileItem.Size = predictedTotalSize;
                NewFileItem.CreateTime = DateTime.Now.ToLocalTime();
                NewFileItem.Command = outputCommand;
                NewFileItem.CommandExe = commandExe;
                NewFileItem.isFirstCommand = true;
                return NewFileItem;
            }
            return null;
        }
    }


}
