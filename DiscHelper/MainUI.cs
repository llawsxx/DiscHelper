using DiscHelper.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace DiscHelper
{
    public partial class MainUI : Form
    {
        private BackgroundWorker DiscWorker = new BackgroundWorker();
        DiscItem CurrentDiscItem = null;
        NameGenerator DiscNameGenerator = null;
        Settings AllSettings;
        SettingsSnapshot LastSavedSettings;
        List<DiscItem> LastAllDiscItems = new List<DiscItem>();
        private VirtualDiskFileSystem _virtualDisk;
        private static readonly Regex SegmentSuffixPattern = new Regex(@"\.Segment_(\d+)(?:_of_(\d+))?$",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode)]
        private static extern int StrCmpLogicalW(string psz1, string psz2);

        public MainUI()
        {
            InitializeComponent();
            UpdateFileMoveButtons();
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
            TxtDiscNamePattern.Text = settings.DiscNamePattern;
            CBoxAllocatePolicy.SelectedIndex = settings.AllocatePolicy;
            TxtOutputPath.Text = settings.OutputFolder;
            CBoxMoveFile.Checked = settings.isMove;
            CBoxGenPar.Checked = settings.GeneratePar;
            CBoxFirstFit.Checked = settings.isFirstFit;
            CboxCutFile.Checked = settings.isCutFile;
            CBoxGenFileList.Checked = settings.GenerateFileList;
            CBoxGenMp4Headers.Checked = settings.GenerateMp4PlaybackHeaders;
            NumBuffer.Maximum = int.MaxValue;
            NumBuffer.Value = settings.ReadBuffer > int.MaxValue ? int.MaxValue : settings.ReadBuffer;
            TxtParArgument.Text = settings.ParArgument;
            TxtVirtualDiskDataPath.Text = string.IsNullOrWhiteSpace(settings.VirtualDiskDataPath) ? "data" : settings.VirtualDiskDataPath;
            AllSettings = settings;
            RestoreWorkspace(settings);
            LastSavedSettings = settings.CreateSnapshot();
            UpdateVirtualDiskButton();
            updateTemplateList();
            UpdateUndoSettingsButton();
        }

        private string GetDiscName()
        {
            if(DiscNameGenerator == null)
            {
                DiscNameGenerator = new NameGenerator(TxtDiscNamePattern.Text);
            }

            return DiscNameGenerator.Next();
        }

        private bool SaveCurrentSettings(bool showMessage)
        {
            SettingsSnapshot previousSettings = LastSavedSettings ?? AllSettings.CreateSnapshot();
            List<SettingsHistoryEntry> previousHistory = AllSettings.ConfigHistory;
            List<SettingsHistoryEntry> previousRedoHistory = AllSettings.ConfigRedoHistory;
            AllSettings.DiskCapacity = (long)NumDiscCapacity.Value;
            AllSettings.MinDiscRedundant = (long)NumDiscRedundant.Value;
            AllSettings.MaxDiscRedundant = (long)NumDiscMaxRedundant.Value;
            AllSettings.DiscNamePattern = TxtDiscNamePattern.Text;
            AllSettings.AllocatePolicy = CBoxAllocatePolicy.SelectedIndex;
            AllSettings.OutputFolder = TxtOutputPath.Text;
            AllSettings.isMove = CBoxMoveFile.Checked;
            AllSettings.GeneratePar = CBoxGenPar.Checked;
            AllSettings.isFirstFit = CBoxFirstFit.Checked;
            AllSettings.isCutFile = CboxCutFile.Checked;
            AllSettings.GenerateFileList = CBoxGenFileList.Checked;
            AllSettings.GenerateMp4PlaybackHeaders = CBoxGenMp4Headers.Checked;
            AllSettings.ReadBuffer = (long)NumBuffer.Value;
            AllSettings.ParArgument = TxtParArgument.Text;
            AllSettings.VirtualDiskDataPath = TxtVirtualDiskDataPath.Text.Trim();
            SaveWorkspace();
            SettingsSnapshot currentSettings = AllSettings.CreateSnapshot();
            if (previousSettings != null && previousSettings.ContentEquals(currentSettings))
            {
                if (showMessage)
                    MessageBox.Show("配置未变化，无需保存。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return true;
            }
            AllSettings.AddConfigHistory(previousSettings);
            AllSettings.ConfigRedoHistory = new List<SettingsHistoryEntry>();
            bool saved = AllSettings.SaveSettings("Settings.xml");
            if (!saved)
            {
                AllSettings.ConfigHistory = previousHistory;
                AllSettings.ConfigRedoHistory = previousRedoHistory;
            }
            else
            {
                LastSavedSettings = AllSettings.CreateSnapshot();
            }
            UpdateUndoSettingsButton();
            if (showMessage)
            {
                MessageBox.Show(saved ? "配置已保存" : "配置保存失败，请检查文件权限。", saved ? "提示" : "错误",
                    MessageBoxButtons.OK, saved ? MessageBoxIcon.Information : MessageBoxIcon.Error);
            }
            return saved;
        }

        private void BtnSaveSettings_Click(object sender, EventArgs e)
        {
            SaveCurrentSettings(true);
        }

        private void BtnUndoSettings_Click(object sender, EventArgs e)
        {
            if (DiscWorker.IsBusy)
            {
                MessageBox.Show("正在输出文件，请先停止。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (_virtualDisk != null)
            {
                MessageBox.Show("请先卸载虚拟磁盘，再撤回配置。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            List<SettingsHistoryEntry> history = AllSettings.ConfigHistory;
            if (history == null || history.Count == 0)
            {
                MessageBox.Show("没有可撤回的配置。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                UpdateUndoSettingsButton();
                return;
            }

            SettingsHistoryEntry entry = history[history.Count - 1];
            string timeText = entry.SavedAt == default(DateTime) ? "最近一次保存" : entry.SavedAt.ToString("yyyy-MM-dd HH:mm:ss");
            if (MessageBox.Show("将撤回到 " + timeText + " 保存的配置，继续吗？", "撤回配置",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            Settings currentSettings = AllSettings;
            Settings restoredSettings = entry.Snapshot == null ? null : entry.Snapshot.ToSettings();
            if (restoredSettings == null)
            {
                history.RemoveAt(history.Count - 1);
                UpdateUndoSettingsButton();
                return;
            }

            restoredSettings.ConfigHistory = Settings.CloneHistory(history.Take(history.Count - 1));
            restoredSettings.ConfigRedoHistory = Settings.CloneHistory(currentSettings.ConfigRedoHistory);
            restoredSettings.ConfigRedoHistory.Add(new SettingsHistoryEntry { SavedAt = DateTime.Now, Snapshot = currentSettings.CreateSnapshot() });
            if (restoredSettings.ConfigRedoHistory.Count > Settings.MaxConfigHistoryCount)
                restoredSettings.ConfigRedoHistory.RemoveRange(0, restoredSettings.ConfigRedoHistory.Count - Settings.MaxConfigHistoryCount);
            AllSettings = restoredSettings;
            if (!AllSettings.SaveSettings("Settings.xml"))
            {
                AllSettings = currentSettings;
                MessageBox.Show("配置撤回失败，请检查文件权限。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ApplySettingsToControls(AllSettings);
            LastSavedSettings = AllSettings.CreateSnapshot();
            MessageBox.Show("配置已撤回。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnRedoSettings_Click(object sender, EventArgs e)
        {
            if (DiscWorker.IsBusy)
            {
                MessageBox.Show("正在输出文件，请先停止。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (_virtualDisk != null)
            {
                MessageBox.Show("请先卸载虚拟磁盘，再恢复配置。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            List<SettingsHistoryEntry> redoHistory = AllSettings.ConfigRedoHistory;
            if (redoHistory == null || redoHistory.Count == 0)
            {
                MessageBox.Show("没有可恢复的配置。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                UpdateUndoSettingsButton();
                return;
            }
            if (MessageBox.Show("恢复最近一次撤回的配置吗？", "恢复配置", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            Settings currentSettings = AllSettings;
            SettingsHistoryEntry entry = redoHistory[redoHistory.Count - 1];
            Settings restoredSettings = entry.Snapshot == null ? null : entry.Snapshot.ToSettings();
            if (restoredSettings == null)
            {
                redoHistory.RemoveAt(redoHistory.Count - 1);
                UpdateUndoSettingsButton();
                return;
            }

            restoredSettings.ConfigHistory = Settings.CloneHistory(currentSettings.ConfigHistory);
            restoredSettings.ConfigHistory.Add(new SettingsHistoryEntry { SavedAt = DateTime.Now, Snapshot = currentSettings.CreateSnapshot() });
            if (restoredSettings.ConfigHistory.Count > Settings.MaxConfigHistoryCount)
                restoredSettings.ConfigHistory.RemoveRange(0, restoredSettings.ConfigHistory.Count - Settings.MaxConfigHistoryCount);
            restoredSettings.ConfigRedoHistory = Settings.CloneHistory(redoHistory.Take(redoHistory.Count - 1));
            AllSettings = restoredSettings;
            if (!AllSettings.SaveSettings("Settings.xml"))
            {
                AllSettings = currentSettings;
                MessageBox.Show("配置恢复失败，请检查文件权限。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ApplySettingsToControls(AllSettings);
            LastSavedSettings = AllSettings.CreateSnapshot();
            MessageBox.Show("配置已恢复。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void UpdateUndoSettingsButton()
        {
            BtnUndoSettings.Enabled = AllSettings != null && AllSettings.ConfigHistory != null && AllSettings.ConfigHistory.Count > 0;
            if (BtnUndoSettings.Enabled)
                BtnUndoSettings.Text = "撤回配置 (" + AllSettings.ConfigHistory.Count + ")";
            else
                BtnUndoSettings.Text = "撤回配置";
            BtnRedoSettings.Enabled = AllSettings != null && AllSettings.ConfigRedoHistory != null && AllSettings.ConfigRedoHistory.Count > 0;
            if (BtnRedoSettings.Enabled)
                BtnRedoSettings.Text = "恢复配置 (" + AllSettings.ConfigRedoHistory.Count + ")";
            else
                BtnRedoSettings.Text = "恢复配置";
        }

        private void ApplySettingsToControls(Settings settings)
        {
            NumDiscCapacity.Value = Math.Min(NumDiscCapacity.Maximum, Math.Max(NumDiscCapacity.Minimum, settings.DiskCapacity));
            NumDiscRedundant.Value = Math.Min(NumDiscRedundant.Maximum, Math.Max(NumDiscRedundant.Minimum, settings.MinDiscRedundant));
            NumDiscMaxRedundant.Value = Math.Min(NumDiscMaxRedundant.Maximum, Math.Max(NumDiscMaxRedundant.Minimum, settings.MaxDiscRedundant));
            TxtDiscNamePattern.Text = settings.DiscNamePattern ?? "";
            CBoxAllocatePolicy.SelectedIndex = settings.AllocatePolicy >= 0 && settings.AllocatePolicy < CBoxAllocatePolicy.Items.Count ? settings.AllocatePolicy : -1;
            TxtOutputPath.Text = settings.OutputFolder ?? "";
            CBoxMoveFile.Checked = settings.isMove;
            CBoxGenPar.Checked = settings.GeneratePar;
            CBoxFirstFit.Checked = settings.isFirstFit;
            CboxCutFile.Checked = settings.isCutFile;
            CBoxGenFileList.Checked = settings.GenerateFileList;
            CBoxGenMp4Headers.Checked = settings.GenerateMp4PlaybackHeaders;
            NumBuffer.Value = Math.Min(NumBuffer.Maximum, Math.Max(NumBuffer.Minimum, settings.ReadBuffer));
            TxtParArgument.Text = settings.ParArgument ?? "";
            TxtVirtualDiskDataPath.Text = string.IsNullOrWhiteSpace(settings.VirtualDiskDataPath) ? "data" : settings.VirtualDiskDataPath;
            CurrentDiscItem = null;
            LstFiles.Items.Clear();
            LstDiscs.Items.Clear();
            LstDiscFiles.Items.Clear();
            DiscNameGenerator = null;
            RestoreWorkspace(settings);
            updateTemplateList();
            UpdateFileMoveButtons();
            UpdateUndoSettingsButton();
        }

        private void DiskHelper_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DiscWorker.IsBusy)
            {
                MessageBox.Show("正在输出文件 请先停止", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                e.Cancel = true;
                return;
            }

            DialogResult saveResult = MessageBox.Show("保存配置、文件列表、光盘列表信息吗？", "关闭软件", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (saveResult == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }

            if (saveResult == DialogResult.Yes && !SaveCurrentSettings(false))
            {
                MessageBox.Show("配置保存失败，已取消关闭。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.Cancel = true;
                return;
            }
            if (!TryUnmountVirtualDisk(true))
            {
                e.Cancel = true;
                return;
            }
            e.Cancel = false;
        }

        private void DiscWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BtnOutputFile.Text = "开始输出";
            UpdateVirtualDiskButton();
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
            if (e.ProgressPercentage >= 0)
                Text = $"{e.ProgressPercentage}% " + UserState;
            else
                Text = UserState;
        }

        private void OutputFileListTxt(List<DiscItem> discItems)
        {
            string OutputPath = NormalizeOutputPath(TxtOutputPath.Text);
            Directory.CreateDirectory(OutputPath);
            foreach (var discItem in discItems)
            {
                string OutputName = Path.Combine(OutputPath, discItem.Name);
                using (FileStream fs = new FileStream(OutputName + ".txt", FileMode.Create))
                {
                    using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                    {
                        sw.WriteLine($"{discItem.Name}");
                        sw.WriteLine($"{ToGigaByte(discItem.Size)} ({discItem.Size})");
                        sw.WriteLine($"{DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssK")}");
                        foreach (var fileItem in discItem.FileItems)
                        {
                            sw.WriteLine(fileItem.ToStringSimple());
                        }
                    }
                }
            }
        }
        private List<Tuple<FileItem,DiscItem>> findAllSameFile(List<DiscItem> discItems,int FileId)
        {
            List<Tuple<FileItem, DiscItem>> resultFileItems = new List<Tuple<FileItem, DiscItem>>();
            foreach (DiscItem discItem in discItems)
            {
                if (!discItem.IsAvailable) continue;
                foreach (FileItem fileitem in discItem.FileItems)
                {
                    if(FileId == fileitem.FileId)
                    {
                        resultFileItems.Add(new Tuple<FileItem, DiscItem>(fileitem,discItem));
                    }
                }
            }
            resultFileItems = resultFileItems.OrderBy(e => { return e.Item1.StartPos; }).ToList();
            return resultFileItems;
        }


        private void GenerateComplexFileItem_Click(object sender, EventArgs e)
        {
            var item = (ToolStripItem)sender;
            var fileItems = item.Tag as List<FileItem>;

            ComplexFileTemplate template = CBoxTemplate.SelectedItem as ComplexFileTemplate;
            if (template == null)
            {
                MessageBox.Show("请先选择模版", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;

            }
            FileItem newFileItem = ComplexFileTemplate.GenerateComplexFileItem(fileItems, template.CommandLineExe, template.CommandLine,
                template.FileInputReplaceStr, template.FileInputListSep, template.OutputFileSuffix, (double)template.InputOutputSizeRatio);
            if (newFileItem == null)
            {
                MessageBox.Show("高级文件生成失败，请检查模版是否正确", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            int firstIndex = -1;
            for (int i = 0; i < fileItems.Count; i++)
            {
                for (int j = 0; j < LstFiles.Items.Count; j++)
                {
                    if (fileItems[i] == LstFiles.Items[j]) {
                        if(firstIndex == -1)
                        {
                            firstIndex = j;
                        }
                        LstFiles.Items.RemoveAt(j);
                        break;
                    }
                }
            }

            if (firstIndex != -1) {
                LstFiles.Items.Insert(firstIndex, newFileItem);
            }
        }

        private void GenerateComplexFileItemSingle_Click(object sender, EventArgs e)
        {
            var item = (ToolStripItem)sender;
            var fileItems = item.Tag as List<FileItem>;

            ComplexFileTemplate template = CBoxTemplate.SelectedItem as ComplexFileTemplate;
            if (template == null)
            {
                MessageBox.Show("请先选择模版", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;

            }
            foreach (FileItem fileItem in fileItems)
            {
                var oneFileItem = new List<FileItem>();
                oneFileItem.Add(fileItem);
                FileItem newFileItem = ComplexFileTemplate.GenerateComplexFileItem(oneFileItem, template.CommandLineExe, template.CommandLine,
                    template.FileInputReplaceStr, template.FileInputListSep, template.OutputFileSuffix, (double)template.InputOutputSizeRatio);
                if (newFileItem == null)
                {
                    MessageBox.Show("高级文件生成失败，请检查模版是否正确", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                for (int i = 0; i < LstFiles.Items.Count; i++)
                {
                    if (fileItem == LstFiles.Items[i])
                    {
                        LstFiles.Items.RemoveAt(i);
                        LstFiles.Items.Insert(i, newFileItem);
                        break;
                    }
                }
            }
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
            OutputPath = Path.GetFullPath(NormalizeOutputPath(OutputPath));
            string ParExePath = Args["ParExePath"] as string;
            string ParArgument = Args["ParArgument"] as string;
            long ReadSize = (long)Args["Buffer"];
            bool generateMp4Headers = (bool)Args["GenerateMp4Headers"];
            List<DiscItem> allDiscItems = Args["AllDiscs"] as List<DiscItem>;
            long TotalFileCount = 0;
            long FinishedFileCount = 0;
            ProcessStartInfo processStartInfo = null;
            ProcessStartInfo processStartInfoComplexFile = null;

            if (GenPar)
            {
                processStartInfo = new ProcessStartInfo(ParExePath);
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
                        if (!string.IsNullOrEmpty(fileitem.CommandExe))
                        {
                            if (fileitem.isFirstCommand)
                            {
                                List<Tuple<FileItem,DiscItem>> allSameFileItems = findAllSameFile(discItems, fileitem.FileId);

                                string exeName = fileitem.CommandExe;
                                string arguments = fileitem.Command;

                                processStartInfoComplexFile = new ProcessStartInfo(exeName, arguments);
                                processStartInfoComplexFile.UseShellExecute = false;
                                processStartInfoComplexFile.ErrorDialog = false;
                                processStartInfoComplexFile.CreateNoWindow = true;

                                //  Specify redirection.
                                processStartInfoComplexFile.RedirectStandardError = true;
                                processStartInfoComplexFile.RedirectStandardInput = true;
                                processStartInfoComplexFile.RedirectStandardOutput = true;
                                var process = new Process();
                                process.StartInfo = processStartInfoComplexFile;
                                process.ErrorDataReceived += new DataReceivedEventHandler((sender2, e2) =>
                                {
                                    // Prepend line numbers to each line of the output.
                                    if (!String.IsNullOrEmpty(e2.Data))
                                    {
                                        TxtCMDOutput.BeginInvoke(new MethodInvoker(() =>
                                        {
                                            TxtCMDOutput.AppendText(e2.Data + Environment.NewLine);
                                        }));
                                    }
                                });
                                process.Start();
                                process.BeginErrorReadLine();
                                var stdin = process.StandardInput.BaseStream;
                                var stdout = process.StandardOutput.BaseStream;
                                byte[] buffer = new byte[ReadSize];
                                long predictedSize = allSameFileItems.Select(item=>item.Item1.Size).Sum();
                                long actualSize = 0;
                                for (int i = 0; i < allSameFileItems.Count; i++)
                                {
                                    FileItem fileitem2 = allSameFileItems[i].Item1;
                                    DiscItem discItem2 = allSameFileItems[i].Item2;
                                    string DiscPath2 = Path.Combine(OutputPath, discItem2.Name);
                                    string DestFileName2 = Path.Combine(DiscPath2, fileitem2.DestName);
                                    string DestFolder2 = Path.GetDirectoryName(DestFileName2);
                                    if (DestFolder2 != null && DestFolder2 != String.Empty && !Directory.Exists(DestFolder2))
                                    {
                                        Directory.CreateDirectory(DestFolder2);
                                    }

                                    long fileLength = fileitem2.Size;
                                    bool cancelFlag = false;
                                    using (FileStream dest = new FileStream(DestFileName2, FileMode.CreateNew, FileAccess.Write))
                                    {
                                        long totalBytes = 0;
                                        long RemainSize = fileitem2.Size;
                                        bool isLast = i == allSameFileItems.Count - 1;
                                        int currentBlockSize;
                                        var watch = Stopwatch.StartNew();

                                        while (true)
                                        {
                                            if (isLast)
                                            {
                                                currentBlockSize = stdout.Read(buffer, 0, buffer.Length);
                                            }
                                            else
                                            {
                                                currentBlockSize = stdout.Read(buffer, 0, (int)Math.Min(RemainSize, buffer.Length));
                                            }
                                            if (currentBlockSize <= 0) break;
                                            totalBytes += currentBlockSize;
                                            actualSize += currentBlockSize;
                                            RemainSize -= currentBlockSize;
                                            dest.Write(buffer, 0, currentBlockSize);
                                            if (watch.ElapsedMilliseconds > 200)
                                            {
                                                if (worker.CancellationPending == true)
                                                {
                                                    try
                                                    {
                                                        process.Kill();
                                                    }
                                                    catch { }
                                                    cancelFlag = true;
                                                    break;
                                                }
                                                double percentage = (double)totalBytes * 100.0 / fileLength;
                                                worker.ReportProgress((int)percentage, $"[{FinishedFileCount + 1}/{TotalFileCount}] 正在处理 [{((totalBytes / 1024) / 1024.0).ToString("F1")} / {((fileLength / 1024) / 1024.0).ToString("F1")} MB] {DestFileName2}");
                                                watch.Restart();
                                            }
                                            if (RemainSize == 0 && !isLast) break;
                                        }
                                    }

                                    if (cancelFlag)
                                    {
                                        e.Cancel = true;
                                        File.Delete(DestFileName2);
                                        return;
                                    }

                                    FinishedFileCount++;
                                }
                                TxtCMDOutput.BeginInvoke(new MethodInvoker(() =>
                                {
                                    TxtCMDOutput.AppendText($"【{allSameFileItems[0].Item1.Name}】预计大小：{predictedSize}({ToGigaByte(predictedSize)}) 实际大小：{actualSize}({ToGigaByte(actualSize)}) 实际大小/预计大小：{(double)actualSize / predictedSize}" + Environment.NewLine);
                                }));
                            }
                        }
                        else
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
                                byte[] buffer = new byte[ReadSize];

                                using (FileStream source = new FileStream(fileitem.Name, FileMode.Open, FileAccess.Read))
                                {
                                    long fileLength = fileitem.Size;
                                    using (FileStream dest = new FileStream(DestFileName, FileMode.CreateNew, FileAccess.Write))
                                    {
                                        long totalBytes = 0;
                                        long RemainSize = fileitem.Size;
                                        if (fileitem.StartPos != -1)
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
                                            worker.ReportProgress((int)percentage, $"[{FinishedFileCount + 1}/{TotalFileCount}] 正在{(isMove && fileitem.StartPos == -1 ? "移动" : "复制")} [{((totalBytes / 1024) / 1024.0).ToString("F1")} / {((fileLength / 1024) / 1024.0).ToString("F1")} MB] {DestFileName}");
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

                            FinishedFileCount++;
                        }
                    }
                    else
                    {
                        FinishedFileCount++;
                    }
                }


            }

            if (generateMp4Headers && !worker.CancellationPending)
                GenerateMp4PlaybackPackages(allDiscItems, discItems, OutputPath, worker);

            //由于高级文件的存在，如果一个discItem有不是isFirstCommand的FileItem，就需要其它discItem生成完才会有这个isFirstCommand为false的FileItem的生成
            //所以GenPar移到这里
            foreach (DiscItem discItem in discItems)
            {
                if (!discItem.IsAvailable) continue;
                string DiscPath = Path.Combine(OutputPath, discItem.Name);
                if (discItem.IsGenPar && processStartInfo != null)
                {
                    long RedundancySize = discItem.Capacity - discItem.Size;
                    RedundancySize = RedundancySize > MaxRedundancySize ? MaxRedundancySize : RedundancySize;
                    double RedundancyPercent = (double)RedundancySize / discItem.Size * 100;
                    if (Double.IsInfinity(RedundancyPercent))
                        RedundancyPercent = 0;
                    var process = new Process();
                    StringBuilder CMDArgs = new StringBuilder();
                    PasteArguments.AppendArgument(CMDArgs, "create");
                    PasteArguments.AppendArgument(CMDArgs, $"/rr{RedundancyPercent}");
                    if (ParArgument.Length > 0)
                        CMDArgs.Append(" " + ParArgument);
                    PasteArguments.AppendArgument(CMDArgs, Path.Combine(DiscPath, discItem.Name + ".par2"));
                    PasteArguments.AppendArgument(CMDArgs, Path.Combine(DiscPath, "*"));
                    processStartInfo.Arguments = CMDArgs.ToString();
                    string ReportText = $"{discItem.Name} 正在生成冗余 {processStartInfo.FileName} {CMDArgs}";
                    TxtCMDOutput.BeginInvoke(new MethodInvoker(() =>
                    {
                        TxtCMDOutput.AppendText(ReportText + Environment.NewLine);
                    }));
                    worker.ReportProgress((int)-1, ReportText);

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
                            worker.ReportProgress((int)-1, $"{line} {discItem.Name} 正在生成冗余 {processStartInfo.FileName} {CMDArgs}");
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
            return ((double)bytes / 1024 / 1024 / 1024).ToString("F2") + " GB";
        }

        private static string NormalizeOutputPath(string path)
        {
            path = (path ?? string.Empty).Trim();
            if (path.Length == 2 && char.IsLetter(path[0]) && path[1] == ':')
                return path + Path.DirectorySeparatorChar;
            return path;
        }

        private int SortFileItemsForPacking(List<FileItem> fileItems, List<FileItem> priorityUpFileItems,
            List<FileItem> priorityDownFileItems)
        {
            priorityUpFileItems.Sort((x, y) => x.Priority.CompareTo(y.Priority));
            priorityDownFileItems.Sort((x, y) => x.Priority.CompareTo(y.Priority));

            int policyIndex = CBoxAllocatePolicy.SelectedIndex;
            switch (policyIndex)
            {
                case 0:
                    fileItems.Sort((x, y) => y.Size.CompareTo(x.Size));
                    break;
                case 1:
                    fileItems.Sort((x, y) => x.Size.CompareTo(y.Size));
                    break;
                case 2:
                    fileItems.Sort((x, y) => StrCmpLogicalW(x.Name, y.Name));
                    break;
                case 3:
                    fileItems.Sort((x, y) => StrCmpLogicalW(y.Name, x.Name));
                    break;
                case 4:
                    fileItems.Sort((x, y) => x.CreateTime.CompareTo(y.CreateTime));
                    break;
                case 5:
                    fileItems.Sort((x, y) => y.CreateTime.CompareTo(x.CreateTime));
                    break;
            }

            int iterationCount = 1;
            if (policyIndex >= 6 && policyIndex <= 8)
            {
                Random rng = new Random();
                int count = fileItems.Count;
                while (count > 1)
                {
                    count--;
                    int index = rng.Next(count + 1);
                    FileItem value = fileItems[index];
                    fileItems[index] = fileItems[count];
                    fileItems[count] = value;
                }
                if (policyIndex == 7) iterationCount = 10000;
                else if (policyIndex == 8) iterationCount = 100000;
            }

            fileItems.InsertRange(0, priorityUpFileItems);
            fileItems.AddRange(priorityDownFileItems);
            return iterationCount;
        }

        private DiscItem FindPackingTarget(IEnumerable<DiscItem> discs, FileItem fileItem, long discCapacity)
        {
            DiscItem target = null;
            foreach (DiscItem disc in discs)
            {
                long remaining = discCapacity - disc.Size;
                if (remaining >= fileItem.Size &&
                    (target == null || remaining < discCapacity - target.Size))
                {
                    target = disc;
                    if (CBoxFirstFit.Checked) break;
                }
            }
            return target;
        }

        private void BinPacking()
        {
            long DiscCapacity = (long)(NumDiscCapacity.Value - NumDiscRedundant.Value);
            if (DiscCapacity <= 0)
            {
                MessageBox.Show("光盘可用容量小于或等于0", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            List<FileItem> FileItems = new List<FileItem>();
            List<FileItem> PriorityUpFileItems = new List<FileItem>();
            List<FileItem> PriorityDownFileItems = new List<FileItem>();
            List<FileItem> NotOKFileItems = new List<FileItem>();
            List<DiscItem> DiscItems = new List<DiscItem>();
            long OKFileSize = 0;
            long NoOKFileSize = 0;
            var DiscPrefix = TxtDiscNamePattern.Text;
            StringBuilder FileDuplicatePrompt = new StringBuilder();
            StringBuilder DiscDuplicatePrompt = new StringBuilder();
            LstDiscs.Items.Clear();
            LstDiscFiles.Items.Clear();

            long MinLastDiscOccupy = DiscCapacity;
            int MinDiscCount = int.MaxValue;
            int IterCount = 1;
            int CurFileId = 0;
            foreach (FileItem item in LstFiles.Items)
            {
                item.FileId = CurFileId++;
                if (item.Size > DiscCapacity && (!CboxCutFile.Checked || item.NoCut))
                {
                    NoOKFileSize += item.Size;
                    NotOKFileItems.Add(item);
                }
                else
                {
                    OKFileSize += item.Size;
                    if (item.Priority > 0)
                    {
                        PriorityUpFileItems.Add(item);
                    }else if(item.Priority < 0)
                    {
                        PriorityDownFileItems.Add(item);
                    }
                    else
                    {
                        FileItems.Add(item);
                    }
                }
            }

            IterCount = SortFileItemsForPacking(FileItems, PriorityUpFileItems, PriorityDownFileItems);

            if (CboxCutFile.Checked)
            {
                List<FileItem> TmpFileItems = new List<FileItem>();
                long DiscSize = 0;
                int i = 0;
                while(i < FileItems.Count)
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
                        if (item.NoCut)
                        {
                            bool FoundOKFile = false;
                            for(int j = i + 1; j < FileItems.Count;j++) //目前暂时只能从后面找符合条件的FileItem
                            {
                                if (!FileItems[j].NoCut || DiscSize + FileItems[j].Size <= DiscCapacity)
                                {
                                    var temp = FileItems[i];
                                    FileItems[i] = FileItems[j];
                                    FileItems[j] = temp;
                                    FoundOKFile = true;
                                    break;
                                }
                            }
                            if (FoundOKFile)
                            {
                                continue;
                            }
                            else
                            {
                                for (int j = i; j < FileItems.Count; j++)//后面已经没有可以Cut或DiscSize + FileItems[j].Size <= DiscCapacity的文件，做不到刚好塞满DiscCapacity，只能全部原样添加进去了
                                {
                                    TmpFileItems.Add(FileItems[j]);
                                }
                                break;
                            }
                        }

                        long MaxSegmentNum = 1 + ((item.Size - (DiscCapacity - DiscSize) - 1) / DiscCapacity) + 1;
                        long StartPos = 0;
                        long RemainSize = item.Size;
                        int Segment = 1;
                        do
                        {
                            FileItem SplitFileItem = CreateSplitFileItem(item, StartPos,
                                Math.Min(DiscCapacity - DiscSize, RemainSize), Segment, MaxSegmentNum);
                            Segment++;
                            StartPos += SplitFileItem.Size;
                            DiscSize += SplitFileItem.Size;
                            RemainSize -= SplitFileItem.Size;
                            if (DiscSize == DiscCapacity)
                            {
                                DiscSize = 0;
                            }
                            TmpFileItems.Add(SplitFileItem);
                        } while (RemainSize > 0);
                    }
                    i++;
                }

                FileItems = TmpFileItems;
            }

            if (FileItems.Count > 0)
            {
                var Tick = Environment.TickCount;
                for (int c = 0; c < IterCount; c++)
                {
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
                        DiscItem target = FindPackingTarget(TempDiscItems, FileItems[i], DiscCapacity);
                        if (target == null)
                        {
                            target = new DiscItem(GetDiscName(), (long)NumDiscCapacity.Value);
                            TempDiscItems.Add(target);
                        }
                        target.AddFileItem(FileItems[i]);
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
                var NotOKDisc = new DiscItem($"[INVALID DISC]", (long)NumDiscCapacity.Value);
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

            LastAllDiscItems.Clear();
            foreach (var item in DiscItems)
            {
                if (item.IsAvailable) {
                    LastAllDiscItems.Add(item);
                }
            }

            TxtCMDOutput.AppendText($"总数 {FileItems.Count + NotOKFileItems.Count} 分配 {FileItems.Count} ({ToGigaByte(OKFileSize)}) 个 失败 {NotOKFileItems.Count} ({ToGigaByte(NoOKFileSize)}) 个" +
                $" 空间浪费 {ToGigaByte(TotalDiscRemain)} 利用率 {((double)TotalDiscSize / (DiscCapacity * TotalDiscAvailable)).ToString("F4")} 光盘用量 {TotalDiscAvailable}{Environment.NewLine}");

            var DuplicateDiscItems = DiscItems.GroupBy(x => x.Name).Where(g => g.Count() > 1);
            foreach (var item2 in DuplicateDiscItems)
            {
                foreach (var item3 in item2)
                {
                    DiscDuplicatePrompt.AppendLine(item3.Name);
                    break;
                }
            }

            if (DiscDuplicatePrompt.Length > 0)
            {
                string PromptText = "以下光盘名称重复，请检查名称模版" +
                Environment.NewLine + DiscDuplicatePrompt.ToString();
                TxtCMDOutput.AppendText(PromptText);
                MessageBox.Show(PromptText, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }


            if (FileDuplicatePrompt.Length > 0)
            {
                string PromptText = "以下分配的光盘有重复文件，请检查" +
                Environment.NewLine + FileDuplicatePrompt.ToString();
                TxtCMDOutput.AppendText(PromptText);
                MessageBox.Show("分配的光盘有重复文件，请检查", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        public void LstFiles_AddItem(string Name)
        {
            if (Directory.Exists(Name))
            {
                string ParentFolder = new DirectoryInfo(Name)?.Parent?.FullName;
                ParentFolder = ParentFolder == null ? Name : ParentFolder;
                string[] files = Directory.GetFiles(Name, "*.*", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    string RelativeName = Common.GetRelativePath(file, ParentFolder);
                    LstFiles.Items.Add(new FileItem(file, RelativeName));
                }
            }
            else if (File.Exists(Name))
            {
                LstFiles.Items.Add(new FileItem(Name));
            }

            if (_virtualDisk != null)
            {
                TryUnmountVirtualDisk(true);
            }
            UpdateFileMoveButtons();
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

        private void LstFiles_Clear(object sender, EventArgs e)
        {
            LstFiles.Items.Clear();
            UpdateFileMoveButtons();
        }

        private void LstFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateFileMoveButtons();
        }

        private void UpdateFileMoveButtons()
        {
            bool hasSelection = LstFiles.SelectedIndices.Count > 0;
            int firstSelectedIndex = hasSelection ? LstFiles.SelectedIndices.Cast<int>().Min() : -1;
            int lastSelectedIndex = hasSelection ? LstFiles.SelectedIndices.Cast<int>().Max() : -1;

            BtnMoveFilesFirst.Enabled = hasSelection && firstSelectedIndex > 0;
            BtnMoveFilesLast.Enabled = hasSelection && lastSelectedIndex < LstFiles.Items.Count - 1;
            BtnMoveFilesUp.Enabled = hasSelection && firstSelectedIndex > 0;
            BtnMoveFilesDown.Enabled = hasSelection && lastSelectedIndex < LstFiles.Items.Count - 1;
        }

        private enum FileMoveOperation
        {
            First,
            Last,
            Up,
            Down
        }

        private void MoveSelectedFiles(FileMoveOperation operation)
        {
            List<int> selectedIndices = LstFiles.SelectedIndices.Cast<int>().ToList();
            if (selectedIndices.Count == 0)
            {
                return;
            }

            List<FileItem> items = LstFiles.Items.Cast<FileItem>().ToList();
            HashSet<FileItem> selectedItems = new HashSet<FileItem>(selectedIndices.Select(index => items[index]));
            List<FileItem> reorderedItems;

            if (operation == FileMoveOperation.First)
            {
                reorderedItems = items.Where(item => selectedItems.Contains(item))
                    .Concat(items.Where(item => !selectedItems.Contains(item))).ToList();
            }
            else if (operation == FileMoveOperation.Last)
            {
                reorderedItems = items.Where(item => !selectedItems.Contains(item))
                    .Concat(items.Where(item => selectedItems.Contains(item))).ToList();
            }
            else if (operation == FileMoveOperation.Up)
            {
                reorderedItems = items.ToList();
                for (int i = 1; i < reorderedItems.Count; i++)
                {
                    if (selectedItems.Contains(reorderedItems[i]) && !selectedItems.Contains(reorderedItems[i - 1]))
                    {
                        FileItem movedItem = reorderedItems[i];
                        reorderedItems[i] = reorderedItems[i - 1];
                        reorderedItems[i - 1] = movedItem;
                    }
                }
            }
            else
            {
                reorderedItems = items.ToList();
                for (int i = reorderedItems.Count - 2; i >= 0; i--)
                {
                    if (selectedItems.Contains(reorderedItems[i]) && !selectedItems.Contains(reorderedItems[i + 1]))
                    {
                        FileItem movedItem = reorderedItems[i];
                        reorderedItems[i] = reorderedItems[i + 1];
                        reorderedItems[i + 1] = movedItem;
                    }
                }
            }

            LstFiles.BeginUpdate();
            try
            {
                LstFiles.Items.Clear();
                LstFiles.Items.AddRange(reorderedItems.ToArray());
                for (int i = 0; i < reorderedItems.Count; i++)
                {
                    if (selectedItems.Contains(reorderedItems[i]))
                    {
                        LstFiles.SetSelected(i, true);
                    }
                }
            }
            finally
            {
                LstFiles.EndUpdate();
            }
            UpdateFileMoveButtons();
        }

        private void BtnMoveFilesFirst_Click(object sender, EventArgs e)
        {
            MoveSelectedFiles(FileMoveOperation.First);
        }

        private void BtnMoveFilesLast_Click(object sender, EventArgs e)
        {
            MoveSelectedFiles(FileMoveOperation.Last);
        }

        private void BtnMoveFilesUp_Click(object sender, EventArgs e)
        {
            MoveSelectedFiles(FileMoveOperation.Up);
        }

        private void BtnMoveFilesDown_Click(object sender, EventArgs e)
        {
            MoveSelectedFiles(FileMoveOperation.Down);
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

        public void BtnAddFiles_Click(object sender, EventArgs e)
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
            if (DiscWorker.IsBusy) return;
            if (LstDiscs.Items.Count == 0)
            {
                AllocateDiscMenuItem_Click(sender, e);
                return;
            }
            DiscHelperMenuStrip.Items.Clear();
            ToolStripItem allocate = DiscHelperMenuStrip.Items.Add("重新分配");
            allocate.Click += AllocateDiscMenuItem_Click;
            ToolStripItem lastDisc = DiscHelperMenuStrip.Items.Add("追加到最后一张光盘（空间不足时新建）");
            lastDisc.Click += AppendDiscMenuItem_Click;
            DiscHelperMenuStrip.Show(BtnAllocateDisc, new Point(0, BtnAllocateDisc.Height));
        }

        private void AllocateDiscMenuItem_Click(object sender, EventArgs e)
        {
            DiscNameGenerator = new NameGenerator(TxtDiscNamePattern.Text);
            BinPacking();
        }

        private void AppendDiscMenuItem_Click(object sender, EventArgs e)
        {
            AppendFiles();
        }

        private bool IsFileAlreadyAssigned(FileItem item)
        {
            foreach (DiscItem disc in LstDiscs.Items)
            {
                foreach (FileItem assigned in disc.FileItems)
                {
                    if (ReferenceEquals(item, assigned)) return true;
                    if (item.StartPos < 0 && assigned.StartPos >= 0 && assigned.FileId == item.FileId &&
                        string.Equals(assigned.Name, item.Name, StringComparison.OrdinalIgnoreCase)) return true;
                    if (item.StartPos == assigned.StartPos && item.Size == assigned.Size &&
                        string.Equals(item.Name, assigned.Name, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(item.DestName, assigned.DestName, StringComparison.OrdinalIgnoreCase)) return true;
                }
            }
            return false;
        }

        private void PrepareDiscNameGeneratorForAppend()
        {
            DiscItem lastDisc = LstDiscs.Items.Cast<DiscItem>().LastOrDefault(disc => disc.IsAvailable);
            if (lastDisc == null) return;

            // Recreate the generator at the position following the current last name.
            NameGenerator candidateGenerator = new NameGenerator(TxtDiscNamePattern.Text);
            string lastGeneratedName = string.IsNullOrEmpty(lastDisc.OriginalName) ? lastDisc.Name : lastDisc.OriginalName;
            for (int i = 0; i < 100000; i++)
            {
                if (string.Equals(candidateGenerator.Next(), lastGeneratedName, StringComparison.Ordinal))
                {
                    DiscNameGenerator = candidateGenerator;
                    return;
                }
            }
            // The pattern may have been edited since allocation; start it from its current value.
            DiscNameGenerator = new NameGenerator(TxtDiscNamePattern.Text);
        }

        private DiscItem NewDiscForAppend()
        {
            PrepareDiscNameGeneratorForAppend();
            string name = GetDiscName();
            for (int i = 0; i < 100 && LstDiscs.Items.Cast<DiscItem>().Any(disc => disc.Name == name); i++)
                name = GetDiscName();
            var newDisc = new DiscItem(name, (long)NumDiscCapacity.Value);
            LstDiscs.Items.Add(newDisc);
            return newDisc;
        }

        private static FileItem CreateSplitFileItem(FileItem source, long startPos, long size, int index, long total)
        {
            int padWidth = total.ToString().Length;
            string segmentNumber = index.ToString().PadLeft(padWidth, '0');
            FileItem segment = new FileItem
            {
                Name = source.Name,
                StartPos = startPos,
                Size = size,
                CreateTime = source.CreateTime,
                Priority = source.Priority,
                Command = source.Command,
                CommandExe = source.CommandExe,
                isFirstCommand = startPos == 0,
                FileId = source.FileId
            };
            if (string.IsNullOrEmpty(source.CommandExe))
            {
                string totalSegmentNumber = total.ToString().PadLeft(padWidth, '0');
                segment.DestName = source.DestName + string.Format(".Segment_{0}_of_{1}", segmentNumber, totalSegmentNumber);
            }
            else
            {
                segment.DestName = source.DestName + ".Segment_" + segmentNumber;
            }
            return segment;
        }

        private DiscItem GetInvalidAppendDisc()
        {
            DiscItem invalidDisc = LstDiscs.Items.Cast<DiscItem>().FirstOrDefault(disc => !disc.IsAvailable);
            if (invalidDisc == null)
            {
                invalidDisc = new DiscItem("[INVALID DISC]", (long)NumDiscCapacity.Value) { IsAvailable = false };
                LstDiscs.Items.Add(invalidDisc);
            }
            return invalidDisc;
        }

        private void AppendFiles()
        {
            List<FileItem> files = (LstFiles.SelectedItems.Count > 0
                ? LstFiles.SelectedItems.Cast<FileItem>()
                : LstFiles.Items.Cast<FileItem>())
                .Where(item => !IsFileAlreadyAssigned(item)).ToList();
            if (files.Count == 0)
            {
                MessageBox.Show("没有可追加的文件", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            List<FileItem> priorityUpFiles = files.Where(item => item.Priority > 0).ToList();
            List<FileItem> priorityDownFiles = files.Where(item => item.Priority < 0).ToList();
            files = files.Where(item => item.Priority == 0).ToList();
            SortFileItemsForPacking(files, priorityUpFiles, priorityDownFiles);

            long discCapacity = (long)(NumDiscCapacity.Value - NumDiscRedundant.Value);
            if (discCapacity <= 0)
            {
                MessageBox.Show("光盘可用容量小于或等于0", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            List<DiscItem> appendTargets = LstDiscs.Items.Cast<DiscItem>().Where(disc => disc.IsAvailable).ToList();
            if (appendTargets.Count > 1)
                appendTargets = new List<DiscItem> { appendTargets[appendTargets.Count - 1] };

            int nextFileId = LstDiscs.Items.Cast<DiscItem>().SelectMany(disc => disc.FileItems)
                .Concat(LstFiles.Items.Cast<FileItem>()).Select(item => item.FileId).DefaultIfEmpty(-1).Max() + 1;
            int sourceFileCount = 0;
            int addedItemCount = 0;
            int newDiscCount = 0;
            int invalidFileCount = 0;
            HashSet<DiscItem> changedDiscs = new HashSet<DiscItem>();

            foreach (FileItem file in files)
            {
                file.FileId = nextFileId++;
                DiscItem fullTarget = FindPackingTarget(appendTargets, file, discCapacity);
                if (fullTarget != null)
                {
                    fullTarget.AddFileItem(file);
                    changedDiscs.Add(fullTarget);
                    sourceFileCount++;
                    addedItemCount++;
                    continue;
                }

                if (!CboxCutFile.Checked || file.NoCut)
                {
                    if (file.Size > discCapacity)
                    {
                        DiscItem invalidDisc = GetInvalidAppendDisc();
                        invalidDisc.AddFileItem(file);
                        changedDiscs.Add(invalidDisc);
                        invalidFileCount++;
                        addedItemCount++;
                        continue;
                    }

                    DiscItem newDisc = NewDiscForAppend();
                    newDiscCount++;
                    newDisc.AddFileItem(file);
                    changedDiscs.Add(newDisc);
                    appendTargets.Add(newDisc);
                    sourceFileCount++;
                    addedItemCount++;
                    continue;
                }

                var placements = new List<Tuple<DiscItem, long, long>>();
                long startPos = 0;
                long remaining = file.Size;
                foreach (DiscItem disc in appendTargets.ToList())
                {
                    long available = Math.Max(0, discCapacity - disc.Size);
                    if (available == 0) continue;
                    long segmentSize = Math.Min(available, remaining);
                    placements.Add(Tuple.Create(disc, startPos, segmentSize));
                    startPos += segmentSize;
                    remaining -= segmentSize;
                    if (remaining == 0) break;
                }
                while (remaining > 0)
                {
                    long segmentSize = Math.Min(discCapacity, remaining);
                    placements.Add(Tuple.Create<DiscItem, long, long>(null, startPos, segmentSize));
                    startPos += segmentSize;
                    remaining -= segmentSize;
                }

                for (int i = 0; i < placements.Count; i++)
                {
                    DiscItem target = placements[i].Item1;
                    if (target == null)
                    {
                        target = NewDiscForAppend();
                        newDiscCount++;
                        appendTargets.Add(target);
                    }
                    target.AddFileItem(CreateSplitFileItem(file, placements[i].Item2, placements[i].Item3, i + 1, placements.Count));
                    changedDiscs.Add(target);
                    addedItemCount++;
                }
                sourceFileCount++;
            }

            foreach (DiscItem changedDisc in changedDiscs) RefreshDiscItem(changedDisc);
            LastAllDiscItems = LstDiscs.Items.Cast<DiscItem>().Where(disc => disc.IsAvailable).ToList();
            if (CurrentDiscItem != null)
            {
                LstDiscs.SelectedItem = CurrentDiscItem;
            }
            string summary = string.Format("已追加 {0} 个源文件（{1} 个文件项），新增 {2} 张光盘，失败 {3} 个",
                sourceFileCount, addedItemCount, newDiscCount, invalidFileCount);
            TxtCMDOutput.AppendText(summary + Environment.NewLine);
            if (invalidFileCount > 0)
                MessageBox.Show(summary + Environment.NewLine + "无法装入单张光盘且禁止分割的文件已放入 [INVALID DISC]。",
                    "追加结果", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void OutputFileDoWork(List<DiscItem> discItems)
        {
            if (DiscWorker.IsBusy)
            {
                return;
            }
            Dictionary<string, object> Args = new Dictionary<string, object>();
            Args["isMove"] = CBoxMoveFile.Checked;
            Args["OutputPath"] = TxtOutputPath.Text;
            Args["ParExePath"] = AllSettings.ParExePath;
            Args["MaxRedundancySize"] = (long)NumDiscMaxRedundant.Value;
            Args["Disc"] = discItems;
            Args["GenPar"] = CBoxGenPar.Checked;
            Args["Buffer"] = (long)NumBuffer.Value;
            Args["ParArgument"] = TxtParArgument.Text;
            Args["GenerateMp4Headers"] = CBoxGenMp4Headers.Checked;
            Args["AllDiscs"] = LstDiscs.Items.Cast<DiscItem>().ToList();
            DiscWorker.RunWorkerAsync(Args);
            BtnOutputFile.Text = "停止输出";
            UpdateVirtualDiskButton();
        }

        private bool CheckDuplicateFileItems(List<DiscItem> discItems)
        {
            StringBuilder FileDuplicatePrompt = new StringBuilder();
            foreach (var item in discItems)
            {
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
            if (FileDuplicatePrompt.Length > 0)
            {
                TxtCMDOutput.AppendText("以下分配的光盘有重复文件，请检查" + Environment.NewLine);
                TxtCMDOutput.AppendText(FileDuplicatePrompt.ToString());
                MessageBox.Show("分配的光盘有重复文件，请检查", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        private bool ValidateOutputSources(IEnumerable<DiscItem> discs)
        {
            List<string> errors = new List<string>();
            HashSet<string> checkedItems = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (DiscItem disc in discs ?? Enumerable.Empty<DiscItem>())
            {
                if (!disc.IsAvailable) continue;
                foreach (FileItem item in disc.FileItems)
                {
                    if (!string.IsNullOrEmpty(item.CommandExe))
                    {
                        string commandKey = "COMMAND\0" + item.CommandExe;
                        if (checkedItems.Add(commandKey) && !ExecutableExists(item.CommandExe))
                        {
                            errors.Add(string.Format("[{0}] {1}\n  高级文件生成程序不存在：{2}",
                                disc.Name, item.DestName, item.CommandExe));
                        }
                        continue;
                    }
                    string sourcePath = item.Name ?? string.Empty;
                    string checkKey = sourcePath + "\0" + item.FileId + "\0" + item.StartPos + "\0" + item.Size;
                    if (!checkedItems.Add(checkKey)) continue;
                    try
                    {
                        FileInfo source = new FileInfo(sourcePath);
                        if (!source.Exists)
                        {
                            errors.Add(string.Format("[{0}] {1}\n  来源不存在：{2}", disc.Name, item.DestName, GetFullSourcePath(item)));
                            continue;
                        }

                        bool isSegment = item.StartPos >= 0;
                        long requiredLength = Math.Max(0, item.StartPos) + item.Size;
                        bool sizeMatches = isSegment ? source.Length >= requiredLength : source.Length == item.Size;
                        if (!sizeMatches)
                        {
                            string expected = isSegment ? ">= " + requiredLength : item.Size.ToString();
                            errors.Add(string.Format("[{0}] {1}\n  来源：{2}\n  要求大小：{3} 字节，实际大小：{4} 字节",
                                disc.Name, item.DestName, source.FullName, expected, source.Length));
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add(string.Format("[{0}] {1}\n  来源无法读取：{2}\n  {3}", disc.Name, item.DestName, sourcePath, ex.Message));
                    }
                }
            }

            if (errors.Count == 0) return true;
            string message = "以下文件来源不存在或大小不符合要求，无法开始输出：" + Environment.NewLine +
                string.Join(Environment.NewLine, errors);
            TxtCMDOutput.AppendText(message + Environment.NewLine);
            MessageBox.Show(message, "输出检查失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        private static bool ExecutableExists(string executable)
        {
            string value = (executable ?? string.Empty).Trim().Trim('"');
            if (string.IsNullOrEmpty(value)) return false;
            if (Path.IsPathRooted(value) || value.IndexOf(Path.DirectorySeparatorChar) >= 0 ||
                value.IndexOf(Path.AltDirectorySeparatorChar) >= 0)
            {
                try { return File.Exists(Path.GetFullPath(value)); }
                catch { return false; }
            }

            string[] extensions = Path.HasExtension(value)
                ? new[] { string.Empty }
                : (Environment.GetEnvironmentVariable("PATHEXT") ?? ".EXE;.COM;.BAT;.CMD")
                    .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            IEnumerable<string> directories = new[] { Environment.CurrentDirectory }
                .Concat((Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
                    .Split(new[] { Path.PathSeparator }, StringSplitOptions.RemoveEmptyEntries));
            foreach (string directory in directories)
            {
                foreach (string extension in extensions)
                {
                    try
                    {
                        if (File.Exists(Path.Combine(directory.Trim().Trim('"'), value + extension))) return true;
                    }
                    catch { }
                }
            }
            return false;
        }

        private void BtnOutputFile_Click(object sender, EventArgs e)
        {
            if (DiscWorker.IsBusy)
            {
                DiscWorker.CancelAsync();
                return;
            }

            List<DiscItem> discItems = new List<DiscItem>();

            if (LstDiscs.Items.Count > 0)
            {
                foreach (DiscItem item in LstDiscs.Items)
                {
                    discItems.Add(item);
                }
            }
            else
            {
                MessageBox.Show("没有文件可输出", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!ValidateOutputSources(discItems)) return;
            if (CBoxGenFileList.Checked)
            {
                OutputFileListTxt(discItems);
            }
            if (CheckDiscItems(discItems) && CheckDuplicateFileItems(discItems))
            {
                OutputFileDoWork(discItems);
            }
        }

        private void RefreshDiscItem(DiscItem item)
        {
            for (int i = 0; i < LstDiscs.Items.Count; i++)
            {
                if (LstDiscs.Items[i] == item)
                {
                    LstDiscs.Items.RemoveAt(i);
                    LstDiscs.Items.Insert(i, item);
                    break;
                }
            }
        }


        private void RefreshFileLstItem(FileItem item)
        {
            for (int i = 0; i < LstFiles.Items.Count; i++)
            {
                if (LstFiles.Items[i] == item)
                {
                    LstFiles.Items.RemoveAt(i);
                    LstFiles.Items.Insert(i, item);
                    break;
                }
            }
        }

        private DiscItem NewDisc()
        {
            var newDisc = new DiscItem(GetDiscName(), (long)NumDiscCapacity.Value);
            LstDiscs.Items.Add(newDisc);
            return newDisc;
        }

        private void LstDiscFilesDeleteItem_Click(object sender, EventArgs e)
        {
            var item = (ToolStripItem)sender;
            var fileItemIndexes = item.Tag as List<int>;
            fileItemIndexes.Sort((x, y) => y.CompareTo(x));
            foreach (var fileItemIndex in fileItemIndexes)
            {
                CurrentDiscItem.RemoveFileItem(fileItemIndex);
            }
            RefreshDiscItem(CurrentDiscItem);
            LstDiscs.SelectedItem = CurrentDiscItem;
        }

        private void LstDiscFilesShowSource_Click(object sender, EventArgs e)
        {
            List<FileItem> files = ((ToolStripItem)sender).Tag as List<FileItem>;
            if (files == null || files.Count == 0) return;
            string message = string.Join(Environment.NewLine, files.Select(file =>
            {
                string sourcePath;
                try { sourcePath = Path.GetFullPath(file.Name); }
                catch { sourcePath = file.Name ?? string.Empty; }
                return string.Format("{0} <- {1}", file.DestName, sourcePath);
            }));
            MessageBox.Show(message, "文件来源完整路径", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static string GetFullSourcePath(FileItem file)
        {
            try { return Path.GetFullPath(file == null ? null : file.Name); }
            catch { return file == null ? string.Empty : (file.Name ?? string.Empty); }
        }

        private void LstDiscFilesChangeSource_Click(object sender, EventArgs e)
        {
            FileItem fileItem = ((ToolStripItem)sender).Tag as FileItem;
            if (fileItem == null) return;
            if (_virtualDisk != null && !TryUnmountVirtualDisk(true)) return;
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Multiselect = false;
                dialog.Title = "选择新的文件来源";
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                string selectedPath = Path.GetFullPath(dialog.FileName);
                FileInfo sourceInfo;
                try { sourceInfo = new FileInfo(selectedPath); }
                catch (Exception ex)
                {
                    MessageBox.Show("无法读取文件：" + ex.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                string expectedName = Path.GetFileName(fileItem.Name);
                long requiredLength = Math.Max(0, fileItem.StartPos) + fileItem.Size;
                if (fileItem.StartPos >= 0)
                {
                    requiredLength = LstDiscs.Items.Cast<DiscItem>()
                        .SelectMany(disc => disc.FileItems)
                        .Where(item => item.FileId == fileItem.FileId &&
                            string.Equals(Path.GetFileName(item.Name), expectedName, StringComparison.OrdinalIgnoreCase))
                        .Select(item => Math.Max(0, item.StartPos) + item.Size)
                        .DefaultIfEmpty(requiredLength)
                        .Max();
                }
                bool nameMatches = string.Equals(sourceInfo.Name, expectedName, StringComparison.OrdinalIgnoreCase);
                bool isSegment = fileItem.StartPos >= 0;
                bool sizeMatches = isSegment ? sourceInfo.Length >= requiredLength : sourceInfo.Length == requiredLength;
                if (!nameMatches || !sizeMatches)
                {
                    string expectedSize = isSegment ? ">= " + requiredLength : requiredLength.ToString();
                    string sizeDescription = isSegment ? "要求最小长度" : "要求大小";
                    MessageBox.Show(string.Format("文件名和大小不符合要求。\n要求文件名：{0}\n{1}：{2}\n实际大小：{3}", expectedName, sizeDescription, expectedSize, sourceInfo.Length),
                        "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                string oldSourcePath = fileItem.Name;
                List<FileItem> relatedItems = LstDiscs.Items.Cast<DiscItem>()
                    .SelectMany(disc => disc.FileItems)
                    .Where(item => ReferenceEquals(item, fileItem) ||
                        (fileItem.StartPos >= 0 && item.FileId == fileItem.FileId &&
                         string.Equals(item.Name, oldSourcePath, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
                foreach (FileItem related in relatedItems)
                {
                    related.Name = selectedPath;
                    related.CreateTime = sourceInfo.CreationTime;
                }
                foreach (FileItem listed in LstFiles.Items.Cast<FileItem>())
                {
                    if (ReferenceEquals(listed, fileItem) ||
                        (fileItem.StartPos >= 0 && listed.FileId == fileItem.FileId &&
                         string.Equals(listed.Name, oldSourcePath, StringComparison.OrdinalIgnoreCase)))
                    {
                        listed.Name = selectedPath;
                        listed.CreateTime = sourceInfo.CreationTime;
                    }
                }
                RefreshDiscItem(CurrentDiscItem);
                if (CurrentDiscItem != null) LstDiscs.SelectedItem = CurrentDiscItem;
            }
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
            if (DiscWorker.IsBusy) return;//DiscWorker运行中，不可修改DiscItem中的FileItems
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

                    List<FileItem> selectedFiles = fileItemIndexes.Select(index => CurrentDiscItem.FileItems[index]).ToList();
                    foreach (FileItem selectedFile in selectedFiles)
                    {
                        var pathMenuItem = DiscHelperMenuStrip.Items.Add("来源：" + GetFullSourcePath(selectedFile));
                        pathMenuItem.Enabled = false;
                    }
                    var sourceMenuItem = DiscHelperMenuStrip.Items.Add("查看文件来源完整路径");
                    sourceMenuItem.Tag = selectedFiles;
                    sourceMenuItem.Click += LstDiscFilesShowSource_Click;
                    if (selectedFiles.Count == 1)
                    {
                        sourceMenuItem = DiscHelperMenuStrip.Items.Add("更改文件来源...");
                        sourceMenuItem.Tag = selectedFiles[0];
                        sourceMenuItem.Click += LstDiscFilesChangeSource_Click;
                    }

                    foreach (DiscItem discItem in LstDiscs.Items)
                    {
                        if (discItem != CurrentDiscItem && discItem.Remain >= fileSize)
                        {
                            var menuItem = DiscHelperMenuStrip.Items.Add("移动到 " + discItem.Name);
                            menuItem.Tag = new Tuple<DiscItem, List<int>>(discItem, fileItemIndexes);
                            menuItem.Click += LstDiscFilesMove_Click;
                        }
                    }

                    if (fileSize <= NumDiscCapacity.Value)
                    {
                        var menuItem = DiscHelperMenuStrip.Items.Add("移动到新光盘");
                        menuItem.Tag = new Tuple<DiscItem, List<int>>(null, fileItemIndexes);
                        menuItem.Click += LstDiscFilesMove_Click;
                    }


                    var deleteMenuItem = DiscHelperMenuStrip.Items.Add("移除文件");
                    deleteMenuItem.Tag = fileItemIndexes;
                    deleteMenuItem.Click += LstDiscFilesDeleteItem_Click;

                    DiscHelperMenuStrip.Show(LstDiscFiles, e.Location);
                }
            }
        }

        private void LstFilesItemEditComplex_Click(object sender, EventArgs e)
        {
            var item = (ToolStripItem)sender;
            var fileItem = item.Tag as FileItem;
            ComplexFile complexFileDialog =  new ComplexFile(AllSettings.ComplexFileTemplates, CBoxTemplate.SelectedIndex, fileItem);
            if (complexFileDialog.ShowDialog() == DialogResult.OK)
            {
                for (int i = 0; i < LstFiles.Items.Count; i++)
                {
                    if (LstFiles.Items[i] == fileItem)
                    {
                        LstFiles.Items.RemoveAt(i);
                        LstFiles.Items.Insert(i, complexFileDialog.newFileItem);
                        break;
                    }
                }
            }

            updateTemplateList();
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
            if (currentDisc == CurrentDiscItem)
            {
                LstDiscs.SelectedItem = currentDisc;
            }
        }

        private void LstFilesDeleteItem_Click(object sender, EventArgs e)
        {
            var item = (ToolStripItem)sender;
            var fileItems = item.Tag as List<FileItem>;
            foreach (var fileItem in fileItems)
            {
                LstFiles.Items.Remove(fileItem);
            }
            UpdateFileMoveButtons();
        }

        private void LstFilesSetPriorityUp_Click(object sender, EventArgs e)
        {
            var item = (ToolStripItem)sender;
            var fileItems = item.Tag as List<FileItem>;
            int LastPriority = 0;
            foreach (FileItem fileItem in LstFiles.Items)
            {
                if (fileItem.Priority > LastPriority)
                {
                    LastPriority = fileItem.Priority;
                }
            }

            foreach (var fileItem in fileItems)
            {
                fileItem.Priority = ++LastPriority;
                RefreshFileLstItem(fileItem);
            }
        }

        private void LstFilesSetPriorityDown_Click(object sender, EventArgs e)
        {
            var item = (ToolStripItem)sender;
            var fileItems = item.Tag as List<FileItem>;
            int LastPriority = 0;
            foreach (FileItem fileItem in LstFiles.Items)
            {
                if (fileItem.Priority < LastPriority)
                {
                    LastPriority = fileItem.Priority;
                }
            }

            foreach (var fileItem in fileItems)
            {
                fileItem.Priority = --LastPriority;
                RefreshFileLstItem(fileItem);
            }
        }


        private void LstFilesUnsetPriority_Click(object sender, EventArgs e)
        {
            var item = (ToolStripItem)sender;
            var fileItems = item.Tag as List<FileItem>;
            foreach (var fileItem in fileItems)
            {
                fileItem.Priority = 0;
                RefreshFileLstItem(fileItem);
            }
        }

        private void LstFilesTriggerCut_Click(object sender, EventArgs e)
        {
            var item = (ToolStripItem)sender;
            var fileItems = item.Tag as List<FileItem>;
            foreach (var fileItem in fileItems)
            {
                fileItem.NoCut = !fileItem.NoCut;
                RefreshFileLstItem(fileItem);
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
                    foreach (FileItem selectedItem in LstFiles.SelectedItems)
                    {
                        fileItems.Add(selectedItem);
                        fileSize += selectedItem.Size;
                    }
                    ToolStripItem menuItem;

                    if (!DiscWorker.IsBusy)//DiscWorker运行中，不可修改DiscItem中的FileItems
                    {
                        foreach (DiscItem discItem in LstDiscs.Items)
                        {
                            if (discItem.Remain >= fileSize)
                            {
                                menuItem = DiscHelperMenuStrip.Items.Add("添加到 " + discItem.Name);
                                menuItem.Tag = new Tuple<DiscItem, List<FileItem>>(discItem, fileItems);
                                menuItem.Click += LstFilesItem_Click;
                            }
                        }
                    }

                    if (!string.IsNullOrEmpty(fileItems[0].CommandExe))
                    {
                        menuItem = DiscHelperMenuStrip.Items.Add("编辑高级文件");
                        menuItem.Tag = fileItems[0];
                        menuItem.Click += LstFilesItemEditComplex_Click;
                    }
                    else
                    {
                        if (fileItems.All(item => item.Command == null))
                        {
                            if (fileItems.Count > 1)
                            {
                                menuItem = DiscHelperMenuStrip.Items.Add("逐个转为高级文件");
                                menuItem.Tag = fileItems;
                                menuItem.Click += GenerateComplexFileItemSingle_Click;
                            }
                            menuItem = DiscHelperMenuStrip.Items.Add("转为高级文件");
                            menuItem.Tag = fileItems;
                            menuItem.Click += GenerateComplexFileItem_Click;
                        }
                    }

                    if (fileSize <= NumDiscCapacity.Value)
                    {
                        menuItem = DiscHelperMenuStrip.Items.Add("添加到新光盘");
                        menuItem.Tag = new Tuple<DiscItem, List<FileItem>>(null, fileItems);
                        menuItem.Click += LstFilesItem_Click;
                    }

                    menuItem = DiscHelperMenuStrip.Items.Add("移除文件");
                    menuItem.Tag = fileItems;
                    menuItem.Click += LstFilesDeleteItem_Click;

                    menuItem = DiscHelperMenuStrip.Items.Add("优先分配");
                    menuItem.Tag = fileItems;
                    menuItem.Click += LstFilesSetPriorityUp_Click;

                    menuItem = DiscHelperMenuStrip.Items.Add("最后分配");
                    menuItem.Tag = fileItems;
                    menuItem.Click += LstFilesSetPriorityDown_Click;

                    menuItem = DiscHelperMenuStrip.Items.Add("取消优先/最后分配");
                    menuItem.Tag = fileItems;
                    menuItem.Click += LstFilesUnsetPriority_Click;

                    menuItem = DiscHelperMenuStrip.Items.Add("允许/禁止分割");
                    menuItem.Tag = fileItems;
                    menuItem.Click += LstFilesTriggerCut_Click;
                    DiscHelperMenuStrip.Show(LstFiles, e.Location);
                }
            }
        }


        private void LstDiscsItem_Click(object sender, EventArgs e)
        {
            var item = (ToolStripItem)sender;
            var discItems = item.Tag as List<DiscItem>;
            foreach (var discItem in discItems)
            {
                LstDiscs.Items.Remove(discItem);
            }
            LstDiscFiles.Items.Clear();
        }


        private void LstDiscsItemOutput_Click(object sender, EventArgs e)
        {
            var item = (ToolStripItem)sender;
            List<DiscItem> discItems = item.Tag as List<DiscItem>;
            if (!ValidateOutputSources(discItems)) return;
            if (CBoxGenFileList.Checked)
            {
                OutputFileListTxt(discItems);
            }
            if (CheckDiscItems(discItems) && CheckDuplicateFileItems(discItems))
            {
                OutputFileDoWork(discItems);
            }
        }

        private void LstDiscsItemOutputFileList_Click(object sender, EventArgs e)
        {
            var item = (ToolStripItem)sender;
            OutputFileListTxt(item.Tag as List<DiscItem>);
            MessageBox.Show("输出成功", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        private void LstDiscsItemTriggerPar_Click(object sender, EventArgs e)
        {
            var item = (ToolStripItem)sender;
            var discItems = item.Tag as List<DiscItem>;
            foreach (var discItem in discItems)
            {
                discItem.IsGenPar = !discItem.IsGenPar;
                RefreshDiscItem(discItem);
            }
        }

        private void LstDiscsRenameItem_Click(object sender, EventArgs e)
        {
            var menuItem = (ToolStripItem)sender;
            var discItem = menuItem.Tag as DiscItem;
            if (discItem == null)
            {
                return;
            }

            string newName;
            if (!TryGetDiscName(discItem.Name, out newName))
            {
                return;
            }

            discItem.Name = newName;
            RefreshDiscItem(discItem);
            if (CurrentDiscItem == discItem)
            {
                LstDiscs.SelectedItem = discItem;
            }
        }

        private bool TryGetDiscName(string currentName, out string newName)
        {
            newName = null;
            using (Form dialog = new Form())
            using (Label label = new Label())
            using (TextBox textBox = new TextBox())
            using (Button okButton = new Button())
            using (Button cancelButton = new Button())
            {
                dialog.Text = "重命名光盘";
                dialog.FormBorderStyle = FormBorderStyle.FixedDialog;
                dialog.StartPosition = FormStartPosition.CenterParent;
                dialog.MinimizeBox = false;
                dialog.MaximizeBox = false;
                dialog.ShowInTaskbar = false;
                dialog.ClientSize = new System.Drawing.Size(360, 112);

                label.AutoSize = true;
                label.Text = "光盘名称：";
                label.Location = new System.Drawing.Point(12, 17);

                textBox.Text = currentName;
                textBox.Location = new System.Drawing.Point(82, 14);
                textBox.Size = new System.Drawing.Size(265, 23);

                okButton.Text = "确定";
                okButton.DialogResult = DialogResult.OK;
                okButton.Location = new System.Drawing.Point(191, 61);
                okButton.Size = new System.Drawing.Size(75, 29);

                cancelButton.Text = "取消";
                cancelButton.DialogResult = DialogResult.Cancel;
                cancelButton.Location = new System.Drawing.Point(272, 61);
                cancelButton.Size = new System.Drawing.Size(75, 29);

                dialog.Controls.Add(label);
                dialog.Controls.Add(textBox);
                dialog.Controls.Add(okButton);
                dialog.Controls.Add(cancelButton);
                dialog.AcceptButton = okButton;
                dialog.CancelButton = cancelButton;

                if (dialog.ShowDialog(this) != DialogResult.OK)
                {
                    return false;
                }

                newName = textBox.Text.Trim();
                if (string.IsNullOrEmpty(newName))
                {
                    MessageBox.Show("光盘名称不能为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    newName = null;
                    return false;
                }

                if (newName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                {
                    MessageBox.Show("光盘名称包含文件名不允许的字符", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    newName = null;
                    return false;
                }

                return true;
            }
        }

        private void LstDiscsMountVirtualDisk_Click(object sender, EventArgs e)
        {
            var menuItem = (ToolStripItem)sender;
            ToggleVirtualDisk(menuItem.Tag as List<DiscItem>);
        }

        private void BtnVirtualDisk_Click(object sender, EventArgs e)
        {
            ToggleVirtualDisk(null);
        }

        private void BtnSegmentVirtualDisk_Click(object sender, EventArgs e)
        {
            using (var dialog = new SegmentVirtualDiskDialog(NormalizeOutputPath(TxtOutputPath.Text)))
                dialog.ShowDialog(this);
        }

        private void GenerateMp4PlaybackPackages(List<DiscItem> allDiscs, List<DiscItem> outputDiscs,
            string outputPath, BackgroundWorker worker)
        {
            var allEntries = (allDiscs ?? new List<DiscItem>())
                .Where(disc => disc.IsAvailable)
                .SelectMany(disc => disc.FileItems.Select(file => new { Disc = disc, File = file }))
                .Where(entry => entry.File.StartPos >= 0 && string.IsNullOrEmpty(entry.File.CommandExe) &&
                    Mp4PlaybackPackage.IsSupportedExtension(StripSegmentSuffix(entry.File.DestName)))
                .GroupBy(entry => Path.GetFullPath(entry.File.Name) + "\0" + StripSegmentSuffix(entry.File.DestName),
                    StringComparer.OrdinalIgnoreCase);
            var outputSet = new HashSet<DiscItem>(outputDiscs ?? new List<DiscItem>());

            foreach (var group in allEntries)
            {
                var ordered = group.OrderBy(entry => entry.File.StartPos).ToList();
                if (ordered.Count < 2 || !ordered.Any(entry => outputSet.Contains(entry.Disc))) continue;
                string sourcePath = ordered[0].File.Name;
                string baseDestination = StripSegmentSuffix(ordered[0].File.DestName);
                try
                {
                    var segments = ordered.Select((entry, index) => new Mp4PlaybackSegment
                    {
                        FileName = Path.GetFileName(entry.File.DestName),
                        Offset = entry.File.StartPos,
                        Length = entry.File.Size,
                        Index = index + 1,
                        Total = ordered.Count
                    }).ToList();
                    string relativeDirectory = Path.GetDirectoryName(baseDestination) ?? string.Empty;
                    var targetDirectories = ordered
                        .Where(entry => outputSet.Contains(entry.Disc))
                        .Select(entry => Path.Combine(outputPath, entry.Disc.Name, relativeDirectory));
                    Mp4PlaybackPackage.Write(sourcePath, Path.GetFileName(baseDestination), segments, targetDirectories);
                    worker.ReportProgress(-1, "已生成 MP4/MOV 播放头：" + baseDestination);
                }
                catch (Exception ex)
                {
                    string message = "MP4/MOV 播放头生成失败 [" + baseDestination + "]：" + ex.Message;
                    worker.ReportProgress(-1, message);
                    TxtCMDOutput.BeginInvoke(new MethodInvoker(() => TxtCMDOutput.AppendText(message + Environment.NewLine)));
                }
            }
        }

        private static string StripSegmentSuffix(string name)
        {
            return SegmentSuffixPattern.Replace(name ?? string.Empty, string.Empty);
        }

        private bool ValidateVirtualDiskSources(IEnumerable<DiscItem> discs)
        {
            List<string> missing = new List<string>();
            foreach (DiscItem disc in discs ?? Enumerable.Empty<DiscItem>())
            {
                if (!disc.IsAvailable) continue;
                foreach (FileItem item in disc.FileItems)
                {
                    string sourcePath = item.Name;
                    try
                    {
                        FileInfo info = new FileInfo(sourcePath);
                        long requiredLength = Math.Max(0, item.StartPos) + item.Size;
                        if (!info.Exists || info.Length < requiredLength)
                            missing.Add(string.Format("[{0}] {1}（需要 {2} 字节）", disc.Name, Path.GetFullPath(sourcePath), requiredLength));
                    }
                    catch
                    {
                        missing.Add(string.Format("[{0}] {1}", disc.Name, sourcePath));
                    }
                }
            }
            if (missing.Count == 0) return true;
            string message = "以下文件来源不存在或长度不足，无法挂载虚拟磁盘：" + Environment.NewLine + string.Join(Environment.NewLine, missing);
            MessageBox.Show(message, "挂载检查失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        private void ToggleVirtualDisk(List<DiscItem> requestedDiscs)
        {
            if (DiscWorker.IsBusy) return;
            if (_virtualDisk != null)
            {
                TryUnmountVirtualDisk(true);
                return;
            }
            var discs = requestedDiscs;
            if (discs == null)
            {
                discs = LstDiscs.Items.Cast<DiscItem>().ToList();
            }
            if (discs.Any(disc => disc.FileItems.Any(item => !string.IsNullOrEmpty(item.CommandExe))))
            {
                MessageBox.Show("包含高级文件的光盘不支持虚拟磁盘", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!ValidateVirtualDiskSources(discs)) return;
            try
            {
                string configuredPath = TxtVirtualDiskDataPath.Text.Trim();
                if (string.IsNullOrEmpty(configuredPath))
                {
                    MessageBox.Show("虚拟磁盘数据目录不能为空", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                string settingsDirectory = Path.GetDirectoryName(Path.GetFullPath("Settings.xml"));
                string dataPath = Path.GetFullPath(Path.IsPathRooted(configuredPath)
                    ? configuredPath
                    : Path.Combine(settingsDirectory, configuredPath));
                if (discs.Count == 0 && !Directory.Exists(dataPath))
                {
                    MessageBox.Show("请先在光盘列表中添加要挂载的光盘", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                Directory.CreateDirectory(dataPath);
                _virtualDisk = new VirtualDiskFileSystem(discs, dataPath);
                _virtualDisk.ActiveFileHandleCountChanged += VirtualDisk_ActiveFileHandleCountChanged;
                AllSettings.VirtualDiskDataPath = configuredPath;
                if (!_virtualDisk.Mount(null))
                {
                    int status = _virtualDisk.LastMountStatus;
                    _virtualDisk.ActiveFileHandleCountChanged -= VirtualDisk_ActiveFileHandleCountChanged;
                    _virtualDisk = null;
                    UpdateVirtualDiskButton();
                    MessageBox.Show(string.Format("虚拟磁盘挂载失败：0x{0:X8} ({1})", status, status), "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    UpdateVirtualDiskButton();
                    MessageBox.Show("虚拟磁盘已挂载到 " + _virtualDisk.MountPoint + "。映射文件为只读，关闭软件时会自动卸载。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                if (_virtualDisk != null)
                {
                    _virtualDisk.ActiveFileHandleCountChanged -= VirtualDisk_ActiveFileHandleCountChanged;
                    _virtualDisk.Unmount();
                }
                _virtualDisk = null;
                UpdateVirtualDiskButton();
                Exception detail = ex;
                while (detail.InnerException != null) detail = detail.InnerException;
                MessageBox.Show("虚拟磁盘挂载失败：" + detail.Message, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private bool TryUnmountVirtualDisk(bool confirmActiveHandles)
        {
            if (_virtualDisk == null) return true;

            int activeHandles = _virtualDisk.ActiveFileHandleCount;
            if (confirmActiveHandles && activeHandles > 0)
            {
                string message = string.Format("虚拟磁盘仍有 {0} 个活动文件句柄。强制卸载会中断正在进行的读取或写入，是否继续？", activeHandles);
                if (MessageBox.Show(message, "确认卸载", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                    return false;
            }

            _virtualDisk.ActiveFileHandleCountChanged -= VirtualDisk_ActiveFileHandleCountChanged;
            _virtualDisk.Unmount();
            _virtualDisk = null;
            UpdateVirtualDiskButton();
            return true;
        }

        private void VirtualDisk_ActiveFileHandleCountChanged(object sender, EventArgs e)
        {
            if (IsDisposed || !IsHandleCreated) return;
            if (InvokeRequired)
            {
                try
                {
                    BeginInvoke(new EventHandler(VirtualDisk_ActiveFileHandleCountChanged), sender, e);
                }
                catch (InvalidOperationException) { }
                return;
            }
            if (ReferenceEquals(sender, _virtualDisk)) UpdateVirtualDiskButton();
        }

        private void UpdateVirtualDiskButton()
        {
            BtnVirtualDisk.Text = _virtualDisk == null ? "挂载虚拟磁盘" : "卸载虚拟磁盘";
            BtnVirtualDisk.Enabled = !DiscWorker.IsBusy;
            BtnSegmentVirtualDisk.Enabled = !DiscWorker.IsBusy;
            TxtVirtualDiskDataPath.Enabled = _virtualDisk == null && !DiscWorker.IsBusy;
            LblVirtualDiskHandles.Visible = _virtualDisk != null;
            LblVirtualDiskHandles.Text = "虚拟磁盘句柄：" + (_virtualDisk == null ? 0 : _virtualDisk.ActiveFileHandleCount);
        }

        private void RestoreWorkspace(Settings settings)
        {
            foreach (PersistedFileItem item in settings.SavedFiles ?? new List<PersistedFileItem>())
                LstFiles.Items.Add(item.ToFileItem());

            foreach (PersistedDiscItem savedDisc in settings.SavedDiscs ?? new List<PersistedDiscItem>())
            {
                DiscItem disc = new DiscItem(savedDisc.Name, savedDisc.Capacity)
                {
                    OriginalName = string.IsNullOrEmpty(savedDisc.OriginalName) ? savedDisc.Name : savedDisc.OriginalName,
                    IsAvailable = savedDisc.IsAvailable,
                    IsGenPar = savedDisc.IsGenPar
                };
                foreach (PersistedFileItem item in savedDisc.FileItems ?? new List<PersistedFileItem>()) disc.AddFileItem(item.ToFileItem());
                LstDiscs.Items.Add(disc);
            }
            LastAllDiscItems = LstDiscs.Items.Cast<DiscItem>().Where(disc => disc.IsAvailable).ToList();
            if (LstDiscs.Items.Count > 0)
                LstDiscs.SelectedIndex = settings.SavedSelectedDiscIndex >= 0 && settings.SavedSelectedDiscIndex < LstDiscs.Items.Count ? settings.SavedSelectedDiscIndex : 0;
            UpdateFileMoveButtons();
        }

        private void SaveWorkspace()
        {
            AllSettings.SavedFiles = LstFiles.Items.Cast<FileItem>().Select(PersistedFileItem.FromFileItem).ToList();
            AllSettings.SavedSelectedDiscIndex = LstDiscs.SelectedIndex;
            AllSettings.SavedDiscs = LstDiscs.Items.Cast<DiscItem>().Select(disc => new PersistedDiscItem
            {
                Name = disc.Name,
                OriginalName = disc.OriginalName,
                Capacity = disc.Capacity,
                IsAvailable = disc.IsAvailable,
                IsGenPar = disc.IsGenPar,
                FileItems = disc.FileItems.Select(PersistedFileItem.FromFileItem).ToList()
            }).ToList();
        }


        private void LstDiscs_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (LstDiscs.SelectedItems.Count > 0)
                {
                    DiscHelperMenuStrip.Items.Clear();
                    List<DiscItem> discItem = new List<DiscItem>();
                    long Remain = 0,Size = 0;
                    foreach (DiscItem selectedItem in LstDiscs.SelectedItems)
                    {
                        discItem.Add(selectedItem);
                        Remain += selectedItem.Remain;
                        Size += selectedItem.Size;
                    }

                    DiscHelperMenuStrip.Items.Add($"总大小 {((double)Size / 1024 / 1024).ToString("F2")} MB 剩余空间 {((double)Remain / 1024 / 1024).ToString("F2")} MB [共选中{discItem.Count}个]").Enabled = false;
                    ToolStripItem menuItem;
                    if (!DiscWorker.IsBusy && _virtualDisk == null && discItem.All(disc => disc.IsAvailable))
                    {
                        menuItem = DiscHelperMenuStrip.Items.Add("挂载只读虚拟磁盘");
                        menuItem.Tag = discItem;
                        menuItem.Click += LstDiscsMountVirtualDisk_Click;
                    }
                    if (!DiscWorker.IsBusy && discItem.Count == 1)
                    {
                        menuItem = DiscHelperMenuStrip.Items.Add("重命名光盘");
                        menuItem.Tag = discItem[0];
                        menuItem.Click += LstDiscsRenameItem_Click;
                    }
                    if (!DiscWorker.IsBusy)
                    {
                        menuItem = DiscHelperMenuStrip.Items.Add($"输出选中光盘");
                        menuItem.Tag = discItem;
                        menuItem.Click += LstDiscsItemOutput_Click;
                    }

                    menuItem = DiscHelperMenuStrip.Items.Add($"输出选中光盘文件列表");
                    menuItem.Tag = discItem;
                    menuItem.Click += LstDiscsItemOutputFileList_Click;

                    menuItem = DiscHelperMenuStrip.Items.Add("删除光盘");
                    menuItem.Tag = discItem;
                    menuItem.Click += LstDiscsItem_Click;

                    menuItem = DiscHelperMenuStrip.Items.Add("开启/关闭PAR冗余");
                    menuItem.Tag = discItem;
                    menuItem.Click += LstDiscsItemTriggerPar_Click;


                    DiscHelperMenuStrip.Show(LstDiscs, e.Location);
                }
            }
        }

        private void BtnAddComplexFile_Click(object sender, EventArgs e)
        {
            ComplexFile ComplexFileDialog = new ComplexFile(AllSettings.ComplexFileTemplates,CBoxTemplate.SelectedIndex);
            if(ComplexFileDialog.ShowDialog() == DialogResult.OK)
            {
                LstFiles.Items.Add(ComplexFileDialog.newFileItem);
            }
            UpdateFileMoveButtons();
            updateTemplateList();
        }

        private bool CheckDiscItems(List<DiscItem> discItems)
        {
            StringBuilder checkMessage = new StringBuilder();
            
            for (int i = 0; i < discItems.Count; i++)
            {
                var discItem = discItems[i];
                for (int j = 0; j < discItem.FileItems.Count; j++){
                    var fileItem = discItem.FileItems[j];
                    if(!string.IsNullOrEmpty(fileItem.CommandExe))
                    {
                        var result = findAllSameFile(discItems,fileItem.FileId);
                        if(result.Count > 0)
                        {
                            var result2 = findAllSameFile(LastAllDiscItems, fileItem.FileId);
                            if (result2.Count > result.Count)
                                checkMessage.AppendLine($"【{discItem.Name}】中的【{fileItem.DestName}】依赖于【{string.Join(" / ", result2.Select(e => e.Item2.Name))}】");
                        }
                    }
                }
            }
            if (checkMessage.Length > 0)
            {
                TxtCMDOutput.AppendText(checkMessage.ToString());
                MessageBox.Show("部分文件依赖于其它文件，无法输出", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true; 
        }
        private void updateTemplateList()
        {
            int selectedIndex = CBoxTemplate.SelectedIndex;
            CBoxTemplate.Items.Clear();
            foreach (var template in AllSettings.ComplexFileTemplates)
            {
                CBoxTemplate.Items.Add(template);
            }
            if(CBoxTemplate.Items.Count > selectedIndex)
            {
                CBoxTemplate.SelectedIndex = selectedIndex;
            }
        }


    }

    public class FileItem
    {
        public string Name;
        public string DestName;
        public long Size;
        public DateTime CreateTime;
        public long StartPos = -1;
        public bool NoCut = false;
        public int Priority = 0;
        public string Command;
        public string CommandExe;
        public bool isFirstCommand;
        //必要时使用ID
        public int FileId;
        public FileItem(string Name, string DestName = null)
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
            string ExtraStr = "";
            string CommandStr = string.IsNullOrEmpty(CommandExe) ? "" : $"[{CommandExe}]";
            if (Priority > 0) ExtraStr = $"{Priority}▲ ";
            else if(Priority < 0) ExtraStr = $"{-Priority}▼ ";

            if (NoCut) ExtraStr += "NO CUT ";
            ExtraStr += CommandStr;
            return $"[{ExtraStr}{((double)Size / 1024 / 1024).ToString("F2")} MB] {(DestName == null ? Name : DestName)}";
        }

        public string ToStringSimple()
        {
            string CommandStr = string.IsNullOrEmpty(CommandExe) ? "" : $"[{CommandExe}]";
            return $"[{((double)Size / 1024 / 1024).ToString("F2")} MB ({Size})]{CommandStr} {(DestName == null ? Name : DestName)}";
        }
    }

    class DiscItem
    {
        public string Name;
        // Name generated from the disc-name template; remains stable when Name is manually renamed.
        public string OriginalName;
        public List<FileItem> FileItems = new List<FileItem>();
        public long Size;
        public long Capacity;
        public bool IsAvailable = true;
        public bool IsGenPar = true;

        public long Remain
        {
            get
            {
                return Capacity - Size;
            }
        }

        public DiscItem(string Name, long Capacity)
        {
            this.Name = Name;
            this.OriginalName = Name;
            this.Capacity = Capacity;
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
            string GenParStr = "";
            if (!IsGenPar) GenParStr = " W/O PAR";
            if (FileItems.Count > 0)
                return $"[{((double)Size / 1024 / 1024 / 1024).ToString("F3")} GB] {Name}{GenParStr}";
            else
                return $"{Name}{GenParStr}";
        }
    }

    
    public class NameGenerator
    {
        public class NumInfo
        {
            public int Current;
            public int Max;
            public int IndexInPattern;
            public int LengthInPattern;
        }
        public string NamePattern;
        public List<NumInfo> NumInfos = new List<NumInfo>();
        public NameGenerator(string NamePattern)
        {
            this.NamePattern = NamePattern;
            var rgx1 = new Regex(@"\{(\d+:\d+|\d+)\}");
            var matches = rgx1.Matches(NamePattern);
            foreach (Match match in matches)
            {
                string SubString = match.Groups[1].Value;
                int IndexInPattern = match.Index;
                int LengthInPattern = match.Length;
                var NumI = new NumInfo();
                NumI.IndexInPattern = IndexInPattern;
                NumI.LengthInPattern = LengthInPattern;
                if (SubString.Contains(":"))
                {
                    string[] SubStringSplit = SubString.Split(':');
                    NumI.Current = Int32.Parse(SubStringSplit[0]);
                    NumI.Max = Int32.Parse(SubStringSplit[1]);
                }
                else
                {
                    NumI.Current = Int32.Parse(SubString);
                    NumI.Max = int.MaxValue;
                }
                NumInfos.Add(NumI);
            }
        }

        public string Next()
        {
            StringBuilder Result = new StringBuilder();
            int NamePatternNextIndex = 0;
            foreach (var NumI in NumInfos) {
                Result.Append(NamePattern.Substring(NamePatternNextIndex, NumI.IndexInPattern - NamePatternNextIndex));
                Result.Append(NumI.Current.ToString());
                NamePatternNextIndex = NumI.IndexInPattern + NumI.LengthInPattern;
            }

            Result.Append(NamePattern.Substring(NamePatternNextIndex));
            for (var i = NumInfos.Count - 1; i >= 0; i--)
            {
                var NumI = NumInfos[i];
                NumI.Current++;
                if (NumI.Current > NumI.Max)
                {
                    NumI.Current = 1;
                }else break;
            }
            return Result.ToString();
        }
    }

}
