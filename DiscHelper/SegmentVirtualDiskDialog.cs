using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace DiscHelper
{
    internal sealed class SegmentVirtualDiskDialog : Form
    {
        private readonly TextBox _sourcePath = new TextBox();
        private readonly Button _browseButton = new Button();
        private readonly Button _mountButton = new Button();
        private readonly Button _closeButton = new Button();
        private readonly Label _handleLabel = new Label();
        private readonly Label _mountPointLabel = new Label();
        private VirtualDiskFileSystem _fileSystem;

        public SegmentVirtualDiskDialog(string initialPath)
        {
            Text = "Segment 合并虚拟磁盘";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            ClientSize = new Size(620, 126);
            Font = new Font("微软雅黑", 9F, FontStyle.Regular, GraphicsUnit.Point, 134);

            var sourceLabel = new Label
            {
                Text = "文件夹",
                Location = new Point(12, 16),
                Size = new Size(58, 23),
                TextAlign = ContentAlignment.MiddleLeft
            };
            _sourcePath.Location = new Point(72, 16);
            _sourcePath.Size = new Size(444, 23);
            _sourcePath.Text = initialPath ?? string.Empty;
            _browseButton.Location = new Point(522, 13);
            _browseButton.Size = new Size(86, 29);
            _browseButton.Text = "选择...";
            _browseButton.Click += BrowseButton_Click;

            _mountPointLabel.Location = new Point(12, 56);
            _mountPointLabel.Size = new Size(330, 29);
            _mountPointLabel.Text = "挂载点：未挂载";
            _mountPointLabel.TextAlign = ContentAlignment.MiddleLeft;
            _handleLabel.Location = new Point(345, 56);
            _handleLabel.Size = new Size(90, 29);
            _handleLabel.Text = "句柄：0";
            _handleLabel.TextAlign = ContentAlignment.MiddleRight;
            _handleLabel.Visible = false;

            _mountButton.Location = new Point(441, 56);
            _mountButton.Size = new Size(80, 29);
            _mountButton.Text = "挂载";
            _mountButton.Click += MountButton_Click;
            _closeButton.Location = new Point(528, 56);
            _closeButton.Size = new Size(80, 29);
            _closeButton.Text = "关闭";
            _closeButton.Click += (sender, args) => Close();

            Controls.Add(sourceLabel);
            Controls.Add(_sourcePath);
            Controls.Add(_browseButton);
            Controls.Add(_mountPointLabel);
            Controls.Add(_handleLabel);
            Controls.Add(_mountButton);
            Controls.Add(_closeButton);
            FormClosing += SegmentVirtualDiskDialog_FormClosing;
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "选择包含 Segment 文件的文件夹";
                if (Directory.Exists(_sourcePath.Text)) dialog.SelectedPath = _sourcePath.Text;
                if (dialog.ShowDialog(this) == DialogResult.OK) _sourcePath.Text = dialog.SelectedPath;
            }
        }

        private void MountButton_Click(object sender, EventArgs e)
        {
            if (_fileSystem != null)
            {
                TryUnmount(true);
                return;
            }

            try
            {
                string sourcePath = Path.GetFullPath(_sourcePath.Text.Trim());
                if (!Directory.Exists(sourcePath))
                {
                    MessageBox.Show(this, "文件夹不存在", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                _fileSystem = VirtualDiskFileSystem.CreateSegmentView(sourcePath);
                _fileSystem.ActiveFileHandleCountChanged += FileSystem_ActiveFileHandleCountChanged;
                if (!_fileSystem.Mount(null))
                {
                    int status = _fileSystem.LastMountStatus;
                    _fileSystem.ActiveFileHandleCountChanged -= FileSystem_ActiveFileHandleCountChanged;
                    _fileSystem = null;
                    UpdateState();
                    MessageBox.Show(this, string.Format("虚拟磁盘挂载失败：0x{0:X8} ({1})", status, status),
                        "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                UpdateState();
                if (_fileSystem.ScanWarnings.Count > 0)
                {
                    MessageBox.Show(this, string.Join(Environment.NewLine, _fileSystem.ScanWarnings),
                        "扫描警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                if (_fileSystem != null)
                {
                    _fileSystem.ActiveFileHandleCountChanged -= FileSystem_ActiveFileHandleCountChanged;
                    _fileSystem.Unmount();
                }
                _fileSystem = null;
                UpdateState();
                Exception detail = ex;
                while (detail.InnerException != null) detail = detail.InnerException;
                MessageBox.Show(this, "虚拟磁盘挂载失败：" + detail.Message, "错误",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FileSystem_ActiveFileHandleCountChanged(object sender, EventArgs e)
        {
            if (IsDisposed || !IsHandleCreated) return;
            if (InvokeRequired)
            {
                try { BeginInvoke(new EventHandler(FileSystem_ActiveFileHandleCountChanged), sender, e); }
                catch (InvalidOperationException) { }
                return;
            }
            if (ReferenceEquals(sender, _fileSystem)) UpdateState();
        }

        private void SegmentVirtualDiskDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!TryUnmount(true)) e.Cancel = true;
        }

        private bool TryUnmount(bool confirm)
        {
            if (_fileSystem == null) return true;
            int handles = _fileSystem.ActiveFileHandleCount;
            if (confirm && handles > 0 && MessageBox.Show(this,
                string.Format("虚拟磁盘仍有 {0} 个活动文件句柄，是否强制卸载？", handles),
                "确认卸载", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
                return false;

            _fileSystem.ActiveFileHandleCountChanged -= FileSystem_ActiveFileHandleCountChanged;
            _fileSystem.Unmount();
            _fileSystem = null;
            UpdateState();
            return true;
        }

        private void UpdateState()
        {
            bool mounted = _fileSystem != null;
            _sourcePath.Enabled = !mounted;
            _browseButton.Enabled = !mounted;
            _mountButton.Text = mounted ? "卸载" : "挂载";
            _handleLabel.Visible = mounted;
            _handleLabel.Text = "句柄：" + (mounted ? _fileSystem.ActiveFileHandleCount : 0);
            _mountPointLabel.Text = "挂载点：" + (mounted ? _fileSystem.MountPoint : "未挂载");
        }
    }
}
