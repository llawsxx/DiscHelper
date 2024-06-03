using DiscHelper.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace DiscHelper
{
    public partial class MainUI : Form
    {
        private BackgroundWorker DiscWorker = new BackgroundWorker();
        DiscItem CurrentDiscItem = null;
        Settings AllSettings;
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern int StrCmpLogicalW(string psz1, string psz2);

        public MainUI()
        {
            InitializeComponent();
            this.FormClosing += DiskHelper_FormClosing;

            NumDiscCapacity.Maximum = long.MaxValue;
            NumDiscRedundant.Maximum = long.MaxValue;
            NumDiscMaxRedundant.Maximum = long.MaxValue;

            DiscWorker.WorkerReportsProgress = true;
            DiscWorker.WorkerSupportsCancellation = true;
            DiscWorker.DoWork += DiscWorker_DoWork;
            DiscWorker.ProgressChanged += DiscWorker_ProgressChanged;
            DiscWorker.RunWorkerCompleted += DiscWorker_RunWorkerCompleted;

            var settings = Settings.LoadSettings("Settings.xml");
            NumDiscCapacity.Value = settings.DiskCapacity;
            NumDiscRedundant.Value = settings.MinDiscRedundant;
            NumDiscMaxRedundant.Value = settings.MaxDiscRedundant;
            TxtDiscPrefix.Text = settings.DiscPrefix;
            NumDiscBucket.Value = settings.DiscBucketNum;
            NumDiscInBucket.Value = settings.DiscInBucketNum;
            NumMaxDiscInBucket.Value = settings.MaxDiscInBucketNum;
            CBoxAllocatePolicy.SelectedIndex = settings.AllocatePolicy;
            TxtOutputPath.Text = settings.OutputFolder;
            CBoxMoveFile.Checked = settings.isMove;
            CBoxGenPar.Checked = settings.GeneratePar;
            CBoxFirstFit.Checked = settings.isFirstFit;
            CboxCutFile.Checked = settings.isCutFile;
            AllSettings = settings;
        }

        private void DiskHelper_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DiscWorker.IsBusy)
            {
                MessageBox.Show("正在输出文件 请先停止", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                e.Cancel = true;
                return;
            }

            AllSettings.DiskCapacity = (long)NumDiscCapacity.Value;
            AllSettings.MinDiscRedundant = (long)NumDiscRedundant.Value;
            AllSettings.MaxDiscRedundant = (long)NumDiscMaxRedundant.Value;
            AllSettings.DiscPrefix = TxtDiscPrefix.Text;
            AllSettings.DiscBucketNum = (long)NumDiscBucket.Value;
            AllSettings.DiscInBucketNum = (long)NumDiscInBucket.Value;
            AllSettings.MaxDiscInBucketNum = (long)NumMaxDiscInBucket.Value;
            AllSettings.AllocatePolicy = CBoxAllocatePolicy.SelectedIndex;
            AllSettings.OutputFolder = TxtOutputPath.Text;
            AllSettings.isMove = CBoxMoveFile.Checked;
            AllSettings.GeneratePar = CBoxGenPar.Checked;
            AllSettings.isFirstFit = CBoxFirstFit.Checked;
            AllSettings.isCutFile = CboxCutFile.Checked;
            AllSettings.SaveSettings("Settings.xml");
            e.Cancel = false;
        }

        private void DiscWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BtnOutputFile.Text = "开始输出";
            //if (e.Cancelled)
            //{
            //    Text = "DiscHelper";
            //}
            Text = "DiscHelper";

            if (e.Error != null)
            {
                MessageBox.Show(e.Error.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void DiscWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string UserState = e.UserState as string;
            if(e.ProgressPercentage>=0)
                Text = $"{e.ProgressPercentage}% " + UserState;
            else
                Text = UserState;
        }

        private void DiscWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            Dictionary<string, object> Args = e.Argument as Dictionary<string, object>;
            List<DiscItem> discItems = Args["Disc"] as List<DiscItem>;
            bool isMove = (bool)Args["isMove"];
            bool GenPar = (bool)Args["GenPar"];
            long MaxRedundancySize = (long)Args["MaxRedundancySize"];
            string OutputPath = Args["OutputPath"] as string;
            long TotalFileSize = 0;
            long TotalFileCount = 0;
            long FinishedFileCount = 0;
            ProcessStartInfo processStartInfo = null;

            if (GenPar)
            {
                processStartInfo = new ProcessStartInfo(@"MultiPar\par2j64.exe");
                processStartInfo.UseShellExecute = false;
                processStartInfo.ErrorDialog = false;
                processStartInfo.CreateNoWindow = true;

                //  Specify redirection.
                processStartInfo.RedirectStandardError = true;
                processStartInfo.RedirectStandardInput = true;
                processStartInfo.RedirectStandardOutput = true;
            }

            foreach (DiscItem discItem in discItems)
            {
                if (!discItem.IsAvailable) continue;
                foreach (FileItem fileitem in discItem.FileItems)
                {
                    TotalFileCount++;
                    TotalFileSize += fileitem.Size;
                }
            }
            Directory.CreateDirectory(OutputPath);

            foreach (DiscItem discItem in discItems)
            {
                if (!discItem.IsAvailable) continue;
                string DiscPath = Path.Combine(OutputPath, discItem.Name);
                Directory.CreateDirectory(DiscPath);
                foreach (FileItem fileitem in discItem.FileItems)
                {
                    if (worker.CancellationPending == true)
                    {
                        e.Cancel = true;
                        return;
                    }
                    string DestFileName = Path.Combine(DiscPath, fileitem.DestName);

                    if (!File.Exists(DestFileName))
                    {
                        string DestFolder = Path.GetDirectoryName(DestFileName);
                        if (DestFolder != null && DestFolder != String.Empty && !Directory.Exists(DestFolder))
                        {
                            Directory.CreateDirectory(DestFolder);
                        }

                        bool MoveFailed = false;
                        if (isMove)
                        {
                            try
                            {
                                if (Path.GetPathRoot(fileitem.Name) == Path.GetPathRoot(DestFileName) && fileitem.StartPos == -1)
                                {
                                    File.Move(fileitem.Name, DestFileName);
                                    worker.ReportProgress((int)100, $"[{FinishedFileCount + 1}/{TotalFileCount}] 已移动 {DestFileName}");
                                }
                                else
                                {
                                    MoveFailed = true;
                                }
                            }
                            catch (Exception)
                            {
                                MoveFailed = true;
                            }
                        }

                        if (!isMove || MoveFailed)
                        {
                            bool cancelFlag = false;
                            byte[] buffer = new byte[1024 * 1024]; // 1MB buffer

                            using (FileStream source = new FileStream(fileitem.Name, FileMode.Open, FileAccess.Read))
                            {
                                long fileLength = fileitem.Size;
                                using (FileStream dest = new FileStream(DestFileName, FileMode.CreateNew, FileAccess.Write))
                                {
                                    long totalBytes = 0;
                                    long RemainSize = fileitem.Size; 
                                    if(fileitem.StartPos!= -1)
                                    {
                                        source.Seek(fileitem.StartPos, SeekOrigin.Begin);
                                    }
                                    while (true)
                                    {
                                        int currentBlockSize = source.Read(buffer, 0, (int)Math.Min(RemainSize, buffer.Length));
                                        if (currentBlockSize <= 0) break;
                                        totalBytes += currentBlockSize;
                                        RemainSize -= currentBlockSize;
                                        double percentage = (double)totalBytes * 100.0 / fileLength;
                                        dest.Write(buffer, 0, currentBlockSize);
                                        worker.ReportProgress((int)percentage, $"[{FinishedFileCount + 1}/{TotalFileCount}] 正在{(isMove ? "移动" : "复制")} [{((totalBytes / 1024) / 1024.0).ToString("F1")} / {((fileLength / 1024) / 1024.0).ToString("F1")} MB] {DestFileName}");
                                        if (RemainSize == 0) break;
                                        if (worker.CancellationPending == true)
                                        {
                                            cancelFlag = true;
                                            break;
                                        }
                                    }
                                }
                            }

                            if (cancelFlag)
                            {
                                e.Cancel = true;
                                File.Delete(DestFileName);
                                return;
                            }

                            if (isMove && fileitem.StartPos == -1)
                            {
                                File.Delete(fileitem.Name);
                            }
                        }
                    }
                    FinishedFileCount++;
                }

                if (processStartInfo != null)
                {
                    long RedundancySize = discItem.Capacity - discItem.Size;
                    RedundancySize = RedundancySize > MaxRedundancySize ? MaxRedundancySize : RedundancySize;
                    double RedundancyPercent = (double)RedundancySize / discItem.Size * 100;

                    var process = new Process();
                    StringBuilder CMDArgs = new StringBuilder();
                    PasteArguments.AppendArgument(CMDArgs, "create");
                    PasteArguments.AppendArgument(CMDArgs, $"/rr{RedundancyPercent}");
                    PasteArguments.AppendArgument(CMDArgs, Path.Combine(DiscPath, discItem.Name + ".par2"));
                    PasteArguments.AppendArgument(CMDArgs, Path.Combine(DiscPath, "*"));
                    processStartInfo.Arguments = CMDArgs.ToString();
                    worker.ReportProgress((int)-1, $"{discItem.Name} 正在生成冗余 {CMDArgs}");

                    process.StartInfo = processStartInfo;
                    process.Start();
                    var outputReader = process.StandardOutput;
                    var inputWriter = process.StandardInput;

                    while (true)
                    {
                        if (worker.CancellationPending == true)
                        {
                            try
                            {
                                process.Kill();
                            }
                            catch { }
                            e.Cancel = true;
                            return;
                        }

                        var line = outputReader.ReadLine();
                        if (line == null) { break; }

                        if (line.Contains("%"))
                        {
                            worker.ReportProgress((int)-1, $"{line} {discItem.Name} 正在生成冗余 {CMDArgs}");
                        }
                        TxtCMDOutput.BeginInvoke(new MethodInvoker(() =>
                        {
                            line = line + Environment.NewLine;
                            TxtCMDOutput.AppendText(line);
                        }));
                    }

                    try
                    {
                        process.Kill();
                    }
                    catch { }
                }
            }
        }

        private string ToGigaByte(long bytes)
        {
            return ((double)bytes / 1024 / 1024 / 1024).ToString("F2")+" GB";
        }

        private void BinPacking()
        {
            List<FileItem> FileItems = new List<FileItem>();
            List<FileItem> NotOKFileItems = new List<FileItem>();
            List<DiscItem> DiscItems = new List<DiscItem>();
            long OKFileSize = 0;
            long NoOKFileSize = 0;
            var MaxDiscInBucket = NumMaxDiscInBucket.Value;
            var DiscPrefix = TxtDiscPrefix.Text;
            StringBuilder FileDuplicatePrompt = new StringBuilder();
            LstDiscs.Items.Clear();
            LstDiscFiles.Items.Clear();
            long DiscCapacity = (long)(NumDiscCapacity.Value - NumDiscRedundant.Value);
            long MinLastDiscOccupy = DiscCapacity;
            int MinDiscCount = int.MaxValue;
            int IterCount = 1;
            foreach (FileItem item in LstFiles.Items)
            {
                if (item.Size > DiscCapacity && !CboxCutFile.Checked)
                {
                    NoOKFileSize += item.Size;
                    NotOKFileItems.Add(item);
                }
                else
                {
                    OKFileSize += item.Size;
                    FileItems.Add(item);
                }
            }

            int _Index = CBoxAllocatePolicy.SelectedIndex;

            switch (_Index)
            {
                case 0://按文件大小倒序
                    FileItems.Sort((x, y) => y.Size.CompareTo(x.Size));
                    break;
                case 1://按文件大小顺序
                    FileItems.Sort((x, y) => x.Size.CompareTo(y.Size));
                    break;
                case 2://按文件名顺序
                    FileItems.Sort((x, y) => StrCmpLogicalW(x.Name, y.Name));
                    break;
                case 3://按文件名倒序
                    FileItems.Sort((x, y) => StrCmpLogicalW(y.Name, x.Name));
                    break;
                case 4://按创建时间顺序
                    FileItems.Sort((x, y) => x.CreateTime.CompareTo(y.CreateTime));
                    break;
                case 5://按创建时间倒序
                    FileItems.Sort((x, y) => y.CreateTime.CompareTo(x.CreateTime));
                    break;
            }

            if(_Index >= 6 && _Index <= 8)//随机排序
            {
                Random rng = new Random();
                int n = FileItems.Count;
                while (n > 1)
                {
                    n--;
                    int k = rng.Next(n + 1);
                    FileItem value = FileItems[k];
                    FileItems[k] = FileItems[n];
                    FileItems[n] = value;
                }
                switch (_Index)
                {
                    case 7:
                        IterCount = 10000;
                        break;
                    case 8:
                        IterCount = 100000;
                        break;
                }
            }

            if(CboxCutFile.Checked)
            {
                List<FileItem> TmpFileItems = new List<FileItem>();
                long DiscSize = 0;
                for (int i = 0; i < FileItems.Count; i++)
                {
                    FileItem item = FileItems[i];
                    if (DiscSize + item.Size <= DiscCapacity)
                    {
                        DiscSize += item.Size;
                        if (DiscSize == DiscCapacity)
                        {
                            DiscSize = 0;
                        }
                        TmpFileItems.Add(item);
                    }
                    else
                    {
                        long StartPos = 0;
                        long RemainSize = item.Size;
                        int Segment = 1;
                        do
                        {
                            FileItem SplitFileItem = new FileItem();
                            SplitFileItem.Name = item.Name;
                            SplitFileItem.StartPos = StartPos;
                            SplitFileItem.Size = Math.Min(DiscCapacity - DiscSize, RemainSize);
                            SplitFileItem.CreateTime = item.CreateTime;
                            SplitFileItem.DestName = item.DestName + $".Segment_{Segment.ToString("00")}";
                            Segment++;
                            StartPos += SplitFileItem.Size;
                            DiscSize += SplitFileItem.Size;
                            RemainSize -= SplitFileItem.Size;
                            if (DiscSize == DiscCapacity)
                            {
                                DiscSize = 0;
                            }
                            TmpFileItems.Add(SplitFileItem);
                        } while (RemainSize>0);
                    }
                }

                FileItems = TmpFileItems;
            }

            if (FileItems.Count > 0)
            {
                var Tick = Environment.TickCount;
                for (int c = 0; c < IterCount; c++)
                {
                    var DiscBucket = NumDiscBucket.Value;
                    var DiscInBucket = NumDiscInBucket.Value;
                    if (c > 0)
                    {
                        Random rng = new Random(Tick + IterCount);
                        int n = FileItems.Count;
                        while (n > 1)
                        {
                            n--;
                            int k = rng.Next(n + 1);
                            FileItem value = FileItems[k];
                            FileItems[k] = FileItems[n];
                            FileItems[n] = value;
                        }
                    }

                    List<DiscItem> TempDiscItems = new List<DiscItem>();
                    for (int i = 0; i < FileItems.Count; ++i)
                    {
                        int j = 0, choose = -1;
                        for (; j < TempDiscItems.Count; ++j)
                        {
                            long Remain = DiscCapacity - TempDiscItems[j].Size;
                            if (Remain >= FileItems[i].Size && (choose == -1 || Remain < DiscCapacity - TempDiscItems[choose].Size))
                            {
                                choose = j;
                                if (CBoxFirstFit.Checked)
                                    break;
                            }
                        }

                        if (choose == -1)
                        {
                            TempDiscItems.Add(new DiscItem($"{DiscPrefix}Bucket_{DiscBucket}_Disc_{DiscInBucket}", DiscBucket, DiscInBucket, (long)NumDiscCapacity.Value));
                            if (++DiscInBucket > MaxDiscInBucket)
                            {
                                DiscInBucket = 1;
                                DiscBucket++;
                            }
                            choose = TempDiscItems.Count - 1;
                        }
                        TempDiscItems[choose].AddFileItem(FileItems[i]);
                    }

                    if (TempDiscItems.Count < MinDiscCount)
                    {
                        DiscItems = TempDiscItems;
                        MinDiscCount = TempDiscItems.Count;

                    }
                    else if (TempDiscItems.Count == MinDiscCount && TempDiscItems[TempDiscItems.Count - 1].Size < MinLastDiscOccupy)
                    {
                        DiscItems = TempDiscItems;
                        MinLastDiscOccupy = TempDiscItems[TempDiscItems.Count - 1].Size;
                    }
                }
            }

            if (NotOKFileItems.Count > 0)
            {
                var NotOKDisc = new DiscItem($"INVALID DISC",-1,-1, (long)NumDiscCapacity.Value);
                NotOKDisc.IsAvailable = false;
                foreach (var item in NotOKFileItems)
                {
                    NotOKDisc.AddFileItem(item);
                }
                DiscItems.Add(NotOKDisc);
            }

            long TotalDiscRemain = 0;
            long TotalDiscSize = 0;
            long TotalDiscAvailable = 0;

            foreach (var item in DiscItems)
            {
                if (item.IsAvailable)
                {
                    TotalDiscRemain += DiscCapacity - item.Size;
                    TotalDiscSize += item.Size;
                    TotalDiscAvailable++;
                    bool PrintDiscName = true;
                    var DuplicateFileItems = item.FileItems.GroupBy(x => x.DestName).Where(g => g.Count() > 1);
                    foreach (var item2 in DuplicateFileItems)
                    {
                        if (PrintDiscName)
                        {
                            FileDuplicatePrompt.AppendLine($"[{item.Name}]");
                            PrintDiscName = false;
                        }
                        foreach (var item3 in item2)
                        {
                            FileDuplicatePrompt.AppendLine(item3.DestName);
                            break;
                        }
                    }
                }
                LstDiscs.Items.Add(item);
            }


            TxtCMDOutput.AppendText($"总数 {FileItems.Count + NotOKFileItems.Count} 分配 {FileItems.Count} ({ToGigaByte(OKFileSize)}) 个 失败 {NotOKFileItems.Count} ({ToGigaByte(NoOKFileSize)}) 个" +
                $" 空间浪费 {ToGigaByte(TotalDiscRemain)} 利用率 {((double)TotalDiscSize / (DiscCapacity * TotalDiscAvailable)).ToString("F4")} 光盘用量 {TotalDiscAvailable}{Environment.NewLine}");

            if (FileDuplicatePrompt.Length > 0)
            {
                string PromptText = "以下分配的光盘有重复文件，请检查，否则文件输出将缺少文件" +
                Environment.NewLine + FileDuplicatePrompt.ToString();
                TxtCMDOutput.AppendText(PromptText);
                MessageBox.Show(PromptText, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        string GetRelativePath(string filespec, string folder)
        {
            Uri pathUri = new Uri(filespec);
            // Folders must end in a slash
            if (!folder.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                folder += Path.DirectorySeparatorChar;
            }
            Uri folderUri = new Uri(folder);
            return Uri.UnescapeDataString(folderUri.MakeRelativeUri(pathUri).ToString().Replace('/', Path.DirectorySeparatorChar));
        }
        private void LstFiles_AddItem(string Name)
        {
            if (Directory.Exists(Name))
            {
                string ParentFolder = new DirectoryInfo(Name)?.Parent?.FullName;
                ParentFolder = ParentFolder == null ? Name : ParentFolder;
                string[] files = Directory.GetFiles(Name, "*.*", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    string RelativeName = GetRelativePath(file, ParentFolder);
                    LstFiles.Items.Add(new FileItem(file, RelativeName));
                }
            }
            else if (File.Exists(Name))
            {
                LstFiles.Items.Add(new FileItem(Name));
            }
        }

        private void LstFiles_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void LstFiles_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string strNames in files)
                {
                    LstFiles_AddItem(strNames);
                }
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            LstFiles.Items.Clear();
        }


        private void BtnAddFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择文件夹";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string FolderName = dialog.SelectedPath;
                LstFiles_AddItem(FolderName);
            }
        }

        private void BtnAddFiles_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true; //是否可以多选true=ok/false=no
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string[] strNames = openFileDialog.FileNames;
                for (int i = 0; i < strNames.Length; i++)
                {
                    LstFiles_AddItem(strNames[i]);
                }
            }
        }

        private void BtnTempFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择文件夹";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                TxtOutputPath.Text = dialog.SelectedPath;
            }
        }

        private void BtnAllocateDisc_Click(object sender, EventArgs e)
        {
            BinPacking();
        }

        private void LstDiscs_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (LstDiscs.SelectedItem == null) return;

            DiscItem item = (DiscItem)LstDiscs.SelectedItem;
            CurrentDiscItem = item;
            LstDiscFiles.Items.Clear();
            foreach (var fileItem in item.FileItems)
            {
                LstDiscFiles.Items.Add(fileItem);
            }
        }


        private void BtnOutputFile_Click(object sender, EventArgs e)
        {
            if (DiscWorker.IsBusy)
            {
                DiscWorker.CancelAsync();
                return;
            }

            if(LstDiscs.Items.Count == 0)
            {
                MessageBox.Show("没有文件可输出", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Dictionary<string,object> Args = new Dictionary<string,object>();
            Args["isMove"] = CBoxMoveFile.Checked;
            Args["OutputPath"] = TxtOutputPath.Text;
            Args["MaxRedundancySize"] = (long)NumDiscMaxRedundant.Value;
            List<DiscItem> discItems = new List<DiscItem>();

            foreach(DiscItem item in LstDiscs.Items)
            {
                discItems.Add(item);
            }

            Args["Disc"] = discItems;
            Args["GenPar"] = CBoxGenPar.Checked;

            DiscWorker.RunWorkerAsync(Args);
            BtnOutputFile.Text = "停止输出";
        }

        private void RefreshDiscItem(DiscItem item)
        {
            for (int i = 0;i< LstDiscs.Items.Count; i++)
            {
                if (LstDiscs.Items[i] == item)
                {
                    LstDiscs.Items.RemoveAt(i);
                    LstDiscs.Items.Insert(i, item);
                    break;
                }
            }
        }

        private DiscItem NewDisc()
        {
            DiscItem LastDiscItem = LstDiscs.Items.Count > 0 ? LstDiscs.Items[LstDiscs.Items.Count - 1] as DiscItem : null;
            decimal DiscBucket;
            decimal DiscInBucket;
            if (LastDiscItem != null)
            {
                DiscBucket = LastDiscItem.BucketNum;
                DiscInBucket = LastDiscItem.DiscInBucketNum;
                if (++DiscInBucket > NumMaxDiscInBucket.Value)
                {
                    DiscInBucket = 1;
                    DiscBucket++;
                }
            }
            else
            {
                DiscBucket = NumDiscBucket.Value;
                DiscInBucket = NumDiscInBucket.Value;
            }
            var newDisc = new DiscItem($"{TxtDiscPrefix.Text}Bucket_{DiscBucket}_Disc_{DiscInBucket}", DiscBucket, DiscInBucket, (long)NumDiscCapacity.Value);
            LstDiscs.Items.Add(newDisc);
            return newDisc;
        }

        private void LstDiscFilesDeleteItem_Click(object sender, EventArgs e)
        {
            var item = (ToolStripItem)sender;
            var fileItemIndexes = item.Tag as List<int>;
            fileItemIndexes.Sort((x,y) => y.CompareTo(x));
            foreach (var fileItemIndex in fileItemIndexes)
            {
                CurrentDiscItem.RemoveFileItem(fileItemIndex);
            }
            RefreshDiscItem(CurrentDiscItem);
            LstDiscs.SelectedItem = CurrentDiscItem;
        }


        private void LstDiscFilesMove_Click(object sender, EventArgs e)
        {
            var item = (ToolStripItem)sender;
            var itemTuple = item.Tag as Tuple<DiscItem, List<int>>;
            DiscItem srcDisc = CurrentDiscItem;
            DiscItem dstDisc = itemTuple.Item1;
            List<int> fileItemIndexes = itemTuple.Item2;
            List<FileItem> fileItems = new List<FileItem>();
            foreach (var fileItemIndex in fileItemIndexes)
            {
                fileItems.Add(srcDisc.FileItems[fileItemIndex]);
            }
            fileItemIndexes.Sort((x, y) => y.CompareTo(x));

            if (dstDisc == null)
            {
                dstDisc = NewDisc();
            }

            foreach (var fileItemIndex in fileItemIndexes)
            {
                srcDisc.RemoveFileItem(fileItemIndex);
            }

            foreach (var fileItem in fileItems)
            {
                dstDisc.AddFileItem(fileItem);
            }
            RefreshDiscItem(srcDisc);
            RefreshDiscItem(dstDisc);
            LstDiscs.SelectedItem = srcDisc;
        }
        private void LstDiscFiles_MouseDown(object sender, MouseEventArgs e)
        {
            if(DiscWorker.IsBusy) return;//DiscWorker运行中，DiscItem里面的内容不可修改
            if (e.Button == MouseButtons.Right)
            {
                if (LstDiscFiles.SelectedItems.Count > 0)
                {
                    DiscHelperMenuStrip.Items.Clear();
                    List<int> fileItemIndexes = new List<int>();
                    long fileSize = 0;
                    foreach (int selectedIndex in LstDiscFiles.SelectedIndices)
                    {
                        fileItemIndexes.Add(selectedIndex);
                        fileSize += (LstDiscFiles.Items[selectedIndex] as FileItem).Size;
                    }

                    foreach (DiscItem discItem in LstDiscs.Items)
                    {
                        if (discItem != CurrentDiscItem && discItem.Remain >= fileSize)
                        {
                            var menuItem = DiscHelperMenuStrip.Items.Add("移动到 " + discItem.Name);
                            menuItem.Tag = new Tuple<DiscItem, List<int>>( discItem, fileItemIndexes);
                            menuItem.Click += LstDiscFilesMove_Click;
                        }
                    }

                    if (fileSize <= NumDiscCapacity.Value)
                    {
                        var menuItem = DiscHelperMenuStrip.Items.Add("移动到新光盘");
                        menuItem.Tag = new Tuple< DiscItem, List<int>>( null, fileItemIndexes);
                        menuItem.Click += LstDiscFilesMove_Click;
                    }


                    var deleteMenuItem = DiscHelperMenuStrip.Items.Add("移除文件");
                    deleteMenuItem.Tag = fileItemIndexes;
                    deleteMenuItem.Click += LstDiscFilesDeleteItem_Click;

                    DiscHelperMenuStrip.Show(LstDiscFiles, e.Location);
                }
            }
        }


        private void LstFilesItem_Click(object sender, EventArgs e)
        {
            var item = (ToolStripItem)sender;
            var itemTuple = item.Tag as Tuple<DiscItem, List<FileItem>>;
            DiscItem currentDisc = null;
            if (itemTuple.Item1 != null)
            {
                currentDisc = itemTuple.Item1;
            }
            else
            {
                currentDisc = NewDisc();
            }
            foreach (FileItem fileItem in itemTuple.Item2)
            {
                currentDisc.AddFileItem(fileItem);
            }
            RefreshDiscItem(currentDisc);
            if(currentDisc == CurrentDiscItem)
            {
                LstDiscs.SelectedItem = currentDisc;
            }
        }

        private void LstFilesDeleteItem_Click(object sender, EventArgs e)
        {
            var item = (ToolStripItem)sender;
            var fileItems = item.Tag as  List<FileItem>;
            foreach(var fileItem in fileItems)
            {
                LstFiles.Items.Remove(fileItem);
            }
        }

        private void LstFiles_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                //select the item under the mouse pointer
                if (LstFiles.SelectedItems.Count > 0)
                {
                    DiscHelperMenuStrip.Items.Clear();
                    List<FileItem> fileItems = new List<FileItem>();
                    long fileSize = 0;
                    foreach(FileItem selectedItem in LstFiles.SelectedItems) {
                        fileItems.Add(selectedItem);
                        fileSize += selectedItem.Size;
                    }
                    foreach(DiscItem discItem in LstDiscs.Items)
                    {
                        if(discItem.Remain >= fileSize)
                        {
                            var menuItem = DiscHelperMenuStrip.Items.Add("添加到 "+ discItem.Name);
                            menuItem.Tag = new Tuple<DiscItem, List<FileItem>>(discItem, fileItems);
                            menuItem.Click += LstFilesItem_Click;
                        }
                    }

                    if(fileSize <= NumDiscCapacity.Value)
                    {
                        var menuItem = DiscHelperMenuStrip.Items.Add("添加到新光盘");
                        menuItem.Tag = new Tuple<DiscItem, List<FileItem>>(null, fileItems);
                        menuItem.Click += LstFilesItem_Click;
                    }

                    var deleteMenuItem = DiscHelperMenuStrip.Items.Add("移除文件");
                    deleteMenuItem.Tag = fileItems;
                    deleteMenuItem.Click += LstFilesDeleteItem_Click;
                    DiscHelperMenuStrip.Show(LstFiles,e.Location);
                }
            }
        }


        private void LstDiscsItem_Click(object sender, EventArgs e)
        {
            var item = (ToolStripItem)sender;
            var discItems = item.Tag as List<DiscItem>;
            foreach(var discItem in discItems)
            {
                LstDiscs.Items.Remove(discItem);
            }
            LstDiscFiles.Items.Clear();
        }

        private void LstDiscs_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (LstDiscs.SelectedItems.Count > 0)
                {
                    DiscHelperMenuStrip.Items.Clear();
                    List<DiscItem> discItem = new List<DiscItem>();
                    long Remain = 0;
                    foreach (DiscItem selectedItem in LstDiscs.SelectedItems)
                    {
                        discItem.Add(selectedItem);
                        Remain += selectedItem.Remain;
                    }

                    DiscHelperMenuStrip.Items.Add($"剩余空间 {((double)Remain / 1024 / 1024).ToString("F2")} MB").Enabled = false;
                    var menuItem = DiscHelperMenuStrip.Items.Add("删除光盘");
                    menuItem.Tag = discItem;
                    menuItem.Click += LstDiscsItem_Click;
                    DiscHelperMenuStrip.Show(LstDiscs, e.Location);
                }
            }
        }


    }

    class FileItem
    {
        public string Name;
        public string DestName;
        public long Size;
        public DateTime CreateTime;
        public long StartPos = -1;

        public FileItem(string Name,string DestName = null)
        {
            FileInfo file = new FileInfo(Name);
            this.Name = Name;
            Size = file.Length;
            CreateTime = file.CreationTime;
            if (DestName == null)
                this.DestName = Path.GetFileName(Name);
            else this.DestName = DestName;
        }

        public FileItem()
        {
            
        }

        public override string ToString()
        {
            return $"[{((double)Size / 1024 / 1024).ToString("F2")} MB] {(DestName == null ? Name : DestName)}";
        }
    }

    class DiscItem
    {
        public string Name;
        public List<FileItem> FileItems = new List<FileItem>();
        public long Size;
        public long Capacity;
        public decimal BucketNum;
        public decimal DiscInBucketNum;
        public bool IsAvailable = true;

        public long Remain
        {
            get
            {
                return Capacity - Size;
            }
        }

        public DiscItem(string Name,decimal BucketNum,decimal DiscInBucketNum,long Capacity)
        {
            this.Name = Name;
            this.Capacity = Capacity;
            this.BucketNum = BucketNum;
            this.DiscInBucketNum = DiscInBucketNum;
        }

        public void AddFileItem(FileItem item)
        {
            this.Size += item.Size;
            FileItems.Add(item);
        }

        public void RemoveFileItem(int index)
        {
            if (FileItems.Count > index)
            {
                Size -= FileItems[index].Size;
                FileItems.RemoveAt(index);
            }
        }


        public override string ToString()
        {

            if (FileItems.Count > 0)
                return $"[{((double)Size / 1024 / 1024 / 1024).ToString("F3")} GB] {Name}";
            else
                return $"{Name}";
        }
    }
}
