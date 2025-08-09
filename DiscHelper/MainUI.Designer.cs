namespace DiscHelper
{
    partial class MainUI
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.BtnAddFolder = new System.Windows.Forms.Button();
            this.BtnAddFiles = new System.Windows.Forms.Button();
            this.LstFiles = new System.Windows.Forms.ListBox();
            this.NumDiscCapacity = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.BtnAllocateDisc = new System.Windows.Forms.Button();
            this.LstDiscs = new System.Windows.Forms.ListBox();
            this.LstDiscFiles = new System.Windows.Forms.ListBox();
            this.BtnTempFolder = new System.Windows.Forms.Button();
            this.BtnOutputFile = new System.Windows.Forms.Button();
            this.NumDiscRedundant = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.CBoxGenPar = new System.Windows.Forms.CheckBox();
            this.BtnClearLstFiles = new System.Windows.Forms.Button();
            this.NumDiscMaxRedundant = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.TxtDiscNamePattern = new System.Windows.Forms.TextBox();
            this.CBoxMoveFile = new System.Windows.Forms.CheckBox();
            this.TxtOutputPath = new System.Windows.Forms.TextBox();
            this.TxtCMDOutput = new System.Windows.Forms.RichTextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.CBoxAllocatePolicy = new System.Windows.Forms.ComboBox();
            this.CBoxFirstFit = new System.Windows.Forms.CheckBox();
            this.DiscHelperMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.CboxCutFile = new System.Windows.Forms.CheckBox();
            this.CBoxGenFileList = new System.Windows.Forms.CheckBox();
            this.NumBuffer = new System.Windows.Forms.NumericUpDown();
            this.LblBufferSize = new System.Windows.Forms.Label();
            this.LblParArgument = new System.Windows.Forms.Label();
            this.TxtParArgument = new System.Windows.Forms.TextBox();
            this.BtnAddComplexFile = new System.Windows.Forms.Button();
            this.LblComplexFileTemplate = new System.Windows.Forms.Label();
            this.CBoxTemplate = new System.Windows.Forms.ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.NumDiscCapacity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumDiscRedundant)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumDiscMaxRedundant)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumBuffer)).BeginInit();
            this.SuspendLayout();
            // 
            // BtnAddFolder
            // 
            this.BtnAddFolder.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BtnAddFolder.Location = new System.Drawing.Point(12, 12);
            this.BtnAddFolder.Name = "BtnAddFolder";
            this.BtnAddFolder.Size = new System.Drawing.Size(80, 29);
            this.BtnAddFolder.TabIndex = 0;
            this.BtnAddFolder.Text = "添加文件夹";
            this.BtnAddFolder.UseVisualStyleBackColor = true;
            this.BtnAddFolder.Click += new System.EventHandler(this.BtnAddFolder_Click);
            // 
            // BtnAddFiles
            // 
            this.BtnAddFiles.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BtnAddFiles.Location = new System.Drawing.Point(98, 12);
            this.BtnAddFiles.Name = "BtnAddFiles";
            this.BtnAddFiles.Size = new System.Drawing.Size(75, 29);
            this.BtnAddFiles.TabIndex = 0;
            this.BtnAddFiles.Text = "添加文件";
            this.BtnAddFiles.UseVisualStyleBackColor = true;
            this.BtnAddFiles.Click += new System.EventHandler(this.BtnAddFiles_Click);
            // 
            // LstFiles
            // 
            this.LstFiles.AllowDrop = true;
            this.LstFiles.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LstFiles.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.LstFiles.FormattingEnabled = true;
            this.LstFiles.ItemHeight = 17;
            this.LstFiles.Location = new System.Drawing.Point(12, 116);
            this.LstFiles.Name = "LstFiles";
            this.LstFiles.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.LstFiles.Size = new System.Drawing.Size(517, 123);
            this.LstFiles.TabIndex = 1;
            this.LstFiles.DragDrop += new System.Windows.Forms.DragEventHandler(this.LstFiles_DragDrop);
            this.LstFiles.DragEnter += new System.Windows.Forms.DragEventHandler(this.LstFiles_DragEnter);
            this.LstFiles.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LstFiles_MouseDown);
            // 
            // NumDiscCapacity
            // 
            this.NumDiscCapacity.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.NumDiscCapacity.Location = new System.Drawing.Point(72, 51);
            this.NumDiscCapacity.Maximum = new decimal(new int[] {
            -1530494977,
            232830,
            0,
            0});
            this.NumDiscCapacity.Name = "NumDiscCapacity";
            this.NumDiscCapacity.Size = new System.Drawing.Size(114, 23);
            this.NumDiscCapacity.TabIndex = 2;
            this.NumDiscCapacity.Value = new decimal(new int[] {
            -769803776,
            5,
            0,
            0});
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(12, 53);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 17);
            this.label1.TabIndex = 3;
            this.label1.Text = "光盘容量";
            // 
            // BtnAllocateDisc
            // 
            this.BtnAllocateDisc.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BtnAllocateDisc.Location = new System.Drawing.Point(477, 80);
            this.BtnAllocateDisc.Name = "BtnAllocateDisc";
            this.BtnAllocateDisc.Size = new System.Drawing.Size(53, 29);
            this.BtnAllocateDisc.TabIndex = 0;
            this.BtnAllocateDisc.Text = "分配";
            this.BtnAllocateDisc.UseVisualStyleBackColor = true;
            this.BtnAllocateDisc.Click += new System.EventHandler(this.BtnAllocateDisc_Click);
            // 
            // LstDiscs
            // 
            this.LstDiscs.AllowDrop = true;
            this.LstDiscs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.LstDiscs.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.LstDiscs.FormattingEnabled = true;
            this.LstDiscs.ItemHeight = 17;
            this.LstDiscs.Location = new System.Drawing.Point(14, 280);
            this.LstDiscs.Name = "LstDiscs";
            this.LstDiscs.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.LstDiscs.Size = new System.Drawing.Size(247, 140);
            this.LstDiscs.TabIndex = 1;
            this.LstDiscs.SelectedIndexChanged += new System.EventHandler(this.LstDiscs_SelectedIndexChanged);
            this.LstDiscs.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LstDiscs_MouseDown);
            // 
            // LstDiscFiles
            // 
            this.LstDiscFiles.AllowDrop = true;
            this.LstDiscFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LstDiscFiles.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.LstDiscFiles.FormattingEnabled = true;
            this.LstDiscFiles.ItemHeight = 17;
            this.LstDiscFiles.Location = new System.Drawing.Point(267, 246);
            this.LstDiscFiles.Name = "LstDiscFiles";
            this.LstDiscFiles.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.LstDiscFiles.Size = new System.Drawing.Size(263, 174);
            this.LstDiscFiles.TabIndex = 1;
            this.LstDiscFiles.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LstDiscFiles_MouseDown);
            // 
            // BtnTempFolder
            // 
            this.BtnTempFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.BtnTempFolder.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BtnTempFolder.Location = new System.Drawing.Point(14, 428);
            this.BtnTempFolder.Name = "BtnTempFolder";
            this.BtnTempFolder.Size = new System.Drawing.Size(106, 29);
            this.BtnTempFolder.TabIndex = 0;
            this.BtnTempFolder.Text = "设置输出文件夹";
            this.BtnTempFolder.UseVisualStyleBackColor = true;
            this.BtnTempFolder.Click += new System.EventHandler(this.BtnTempFolder_Click);
            // 
            // BtnOutputFile
            // 
            this.BtnOutputFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnOutputFile.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BtnOutputFile.Location = new System.Drawing.Point(453, 428);
            this.BtnOutputFile.Name = "BtnOutputFile";
            this.BtnOutputFile.Size = new System.Drawing.Size(76, 29);
            this.BtnOutputFile.TabIndex = 0;
            this.BtnOutputFile.Text = "开始输出";
            this.BtnOutputFile.UseVisualStyleBackColor = true;
            this.BtnOutputFile.Click += new System.EventHandler(this.BtnOutputFile_Click);
            // 
            // NumDiscRedundant
            // 
            this.NumDiscRedundant.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.NumDiscRedundant.Location = new System.Drawing.Point(74, 84);
            this.NumDiscRedundant.Maximum = new decimal(new int[] {
            -1530494977,
            232830,
            0,
            0});
            this.NumDiscRedundant.Name = "NumDiscRedundant";
            this.NumDiscRedundant.Size = new System.Drawing.Size(90, 23);
            this.NumDiscRedundant.TabIndex = 2;
            this.NumDiscRedundant.Value = new decimal(new int[] {
            52428800,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label3.Location = new System.Drawing.Point(12, 86);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 17);
            this.label3.TabIndex = 3;
            this.label3.Text = "最小冗余";
            // 
            // CBoxGenPar
            // 
            this.CBoxGenPar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CBoxGenPar.AutoSize = true;
            this.CBoxGenPar.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.CBoxGenPar.Location = new System.Drawing.Point(296, 433);
            this.CBoxGenPar.Name = "CBoxGenPar";
            this.CBoxGenPar.Size = new System.Drawing.Size(70, 21);
            this.CBoxGenPar.TabIndex = 5;
            this.CBoxGenPar.Text = "生成Par";
            this.CBoxGenPar.UseVisualStyleBackColor = true;
            // 
            // BtnClearLstFiles
            // 
            this.BtnClearLstFiles.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BtnClearLstFiles.Location = new System.Drawing.Point(12, 245);
            this.BtnClearLstFiles.Name = "BtnClearLstFiles";
            this.BtnClearLstFiles.Size = new System.Drawing.Size(47, 29);
            this.BtnClearLstFiles.TabIndex = 0;
            this.BtnClearLstFiles.Text = "清空";
            this.BtnClearLstFiles.UseVisualStyleBackColor = true;
            this.BtnClearLstFiles.Click += new System.EventHandler(this.LstFiles_Clear);
            // 
            // NumDiscMaxRedundant
            // 
            this.NumDiscMaxRedundant.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.NumDiscMaxRedundant.Location = new System.Drawing.Point(232, 84);
            this.NumDiscMaxRedundant.Maximum = new decimal(new int[] {
            -1530494977,
            232830,
            0,
            0});
            this.NumDiscMaxRedundant.Name = "NumDiscMaxRedundant";
            this.NumDiscMaxRedundant.Size = new System.Drawing.Size(90, 23);
            this.NumDiscMaxRedundant.TabIndex = 2;
            this.NumDiscMaxRedundant.Value = new decimal(new int[] {
            209715200,
            0,
            0,
            0});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label4.Location = new System.Drawing.Point(170, 86);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 17);
            this.label4.TabIndex = 3;
            this.label4.Text = "最大冗余";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label6.Location = new System.Drawing.Point(64, 251);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(56, 17);
            this.label6.TabIndex = 3;
            this.label6.Text = "名称模版";
            // 
            // TxtDiscNamePattern
            // 
            this.TxtDiscNamePattern.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.TxtDiscNamePattern.Location = new System.Drawing.Point(122, 248);
            this.TxtDiscNamePattern.Name = "TxtDiscNamePattern";
            this.TxtDiscNamePattern.Size = new System.Drawing.Size(139, 23);
            this.TxtDiscNamePattern.TabIndex = 7;
            // 
            // CBoxMoveFile
            // 
            this.CBoxMoveFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CBoxMoveFile.AutoSize = true;
            this.CBoxMoveFile.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.CBoxMoveFile.Location = new System.Drawing.Point(372, 433);
            this.CBoxMoveFile.Name = "CBoxMoveFile";
            this.CBoxMoveFile.Size = new System.Drawing.Size(75, 21);
            this.CBoxMoveFile.TabIndex = 5;
            this.CBoxMoveFile.Text = "移动文件";
            this.CBoxMoveFile.UseVisualStyleBackColor = true;
            // 
            // TxtOutputPath
            // 
            this.TxtOutputPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TxtOutputPath.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.TxtOutputPath.Location = new System.Drawing.Point(126, 431);
            this.TxtOutputPath.Name = "TxtOutputPath";
            this.TxtOutputPath.Size = new System.Drawing.Size(83, 23);
            this.TxtOutputPath.TabIndex = 7;
            this.TxtOutputPath.Text = "DISC";
            // 
            // TxtCMDOutput
            // 
            this.TxtCMDOutput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TxtCMDOutput.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TxtCMDOutput.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.TxtCMDOutput.Location = new System.Drawing.Point(15, 463);
            this.TxtCMDOutput.Name = "TxtCMDOutput";
            this.TxtCMDOutput.Size = new System.Drawing.Size(517, 100);
            this.TxtCMDOutput.TabIndex = 8;
            this.TxtCMDOutput.Text = "";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label7.Location = new System.Drawing.Point(194, 53);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(56, 17);
            this.label7.TabIndex = 9;
            this.label7.Text = "分配规则";
            // 
            // CBoxAllocatePolicy
            // 
            this.CBoxAllocatePolicy.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CBoxAllocatePolicy.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.CBoxAllocatePolicy.FormattingEnabled = true;
            this.CBoxAllocatePolicy.Items.AddRange(new object[] {
            "按文件大小倒序",
            "按文件大小顺序",
            "按文件名称顺序",
            "按文件名称倒序",
            "按创建时间顺序",
            "按创建时间倒序",
            "随机顺序",
            "随机迭代10K",
            "随机迭代100K",
            "不排序"});
            this.CBoxAllocatePolicy.Location = new System.Drawing.Point(256, 50);
            this.CBoxAllocatePolicy.Name = "CBoxAllocatePolicy";
            this.CBoxAllocatePolicy.Size = new System.Drawing.Size(119, 25);
            this.CBoxAllocatePolicy.TabIndex = 10;
            // 
            // CBoxFirstFit
            // 
            this.CBoxFirstFit.AutoSize = true;
            this.CBoxFirstFit.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.CBoxFirstFit.Location = new System.Drawing.Point(409, 84);
            this.CBoxFirstFit.Name = "CBoxFirstFit";
            this.CBoxFirstFit.Size = new System.Drawing.Size(68, 21);
            this.CBoxFirstFit.TabIndex = 5;
            this.CBoxFirstFit.Text = "First Fit";
            this.CBoxFirstFit.UseVisualStyleBackColor = true;
            // 
            // DiscHelperMenuStrip
            // 
            this.DiscHelperMenuStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.DiscHelperMenuStrip.Name = "LstDiscFilesMenuStrip";
            this.DiscHelperMenuStrip.Size = new System.Drawing.Size(61, 4);
            // 
            // CboxCutFile
            // 
            this.CboxCutFile.AutoSize = true;
            this.CboxCutFile.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.CboxCutFile.Location = new System.Drawing.Point(328, 84);
            this.CboxCutFile.Name = "CboxCutFile";
            this.CboxCutFile.Size = new System.Drawing.Size(75, 21);
            this.CboxCutFile.TabIndex = 5;
            this.CboxCutFile.Text = "分割文件";
            this.CboxCutFile.UseVisualStyleBackColor = true;
            // 
            // CBoxGenFileList
            // 
            this.CBoxGenFileList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CBoxGenFileList.AutoSize = true;
            this.CBoxGenFileList.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.CBoxGenFileList.Location = new System.Drawing.Point(215, 433);
            this.CBoxGenFileList.Name = "CBoxGenFileList";
            this.CBoxGenFileList.Size = new System.Drawing.Size(75, 21);
            this.CBoxGenFileList.TabIndex = 5;
            this.CBoxGenFileList.Text = "生成列表";
            this.CBoxGenFileList.UseVisualStyleBackColor = true;
            // 
            // NumBuffer
            // 
            this.NumBuffer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.NumBuffer.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.NumBuffer.Location = new System.Drawing.Point(77, 569);
            this.NumBuffer.Maximum = new decimal(new int[] {
            -1530494977,
            232830,
            0,
            0});
            this.NumBuffer.Minimum = new decimal(new int[] {
            1048576,
            0,
            0,
            0});
            this.NumBuffer.Name = "NumBuffer";
            this.NumBuffer.Size = new System.Drawing.Size(96, 23);
            this.NumBuffer.TabIndex = 2;
            this.NumBuffer.Value = new decimal(new int[] {
            1048576,
            0,
            0,
            0});
            // 
            // LblBufferSize
            // 
            this.LblBufferSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LblBufferSize.AutoSize = true;
            this.LblBufferSize.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.LblBufferSize.Location = new System.Drawing.Point(15, 571);
            this.LblBufferSize.Name = "LblBufferSize";
            this.LblBufferSize.Size = new System.Drawing.Size(56, 17);
            this.LblBufferSize.TabIndex = 3;
            this.LblBufferSize.Text = "缓存大小";
            // 
            // LblParArgument
            // 
            this.LblParArgument.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LblParArgument.AutoSize = true;
            this.LblParArgument.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.LblParArgument.Location = new System.Drawing.Point(194, 571);
            this.LblParArgument.Name = "LblParArgument";
            this.LblParArgument.Size = new System.Drawing.Size(51, 17);
            this.LblParArgument.TabIndex = 3;
            this.LblParArgument.Text = "Par参数";
            // 
            // TxtParArgument
            // 
            this.TxtParArgument.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TxtParArgument.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.TxtParArgument.Location = new System.Drawing.Point(251, 569);
            this.TxtParArgument.Name = "TxtParArgument";
            this.TxtParArgument.Size = new System.Drawing.Size(278, 23);
            this.TxtParArgument.TabIndex = 7;
            // 
            // BtnAddComplexFile
            // 
            this.BtnAddComplexFile.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BtnAddComplexFile.Location = new System.Drawing.Point(179, 12);
            this.BtnAddComplexFile.Name = "BtnAddComplexFile";
            this.BtnAddComplexFile.Size = new System.Drawing.Size(99, 29);
            this.BtnAddComplexFile.TabIndex = 11;
            this.BtnAddComplexFile.Text = "添加高级文件";
            this.BtnAddComplexFile.UseVisualStyleBackColor = true;
            this.BtnAddComplexFile.Click += new System.EventHandler(this.BtnAddComplexFile_Click);
            // 
            // LblComplexFileTemplate
            // 
            this.LblComplexFileTemplate.AutoSize = true;
            this.LblComplexFileTemplate.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.LblComplexFileTemplate.Location = new System.Drawing.Point(293, 18);
            this.LblComplexFileTemplate.Name = "LblComplexFileTemplate";
            this.LblComplexFileTemplate.Size = new System.Drawing.Size(80, 17);
            this.LblComplexFileTemplate.TabIndex = 12;
            this.LblComplexFileTemplate.Text = "高级文件模版";
            // 
            // CBoxTemplate
            // 
            this.CBoxTemplate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CBoxTemplate.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.CBoxTemplate.FormattingEnabled = true;
            this.CBoxTemplate.Location = new System.Drawing.Point(379, 15);
            this.CBoxTemplate.Name = "CBoxTemplate";
            this.CBoxTemplate.Size = new System.Drawing.Size(150, 25);
            this.CBoxTemplate.TabIndex = 19;
            // 
            // MainUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(544, 601);
            this.Controls.Add(this.CBoxTemplate);
            this.Controls.Add(this.LblComplexFileTemplate);
            this.Controls.Add(this.BtnAddComplexFile);
            this.Controls.Add(this.CBoxAllocatePolicy);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.TxtCMDOutput);
            this.Controls.Add(this.TxtOutputPath);
            this.Controls.Add(this.TxtParArgument);
            this.Controls.Add(this.TxtDiscNamePattern);
            this.Controls.Add(this.CBoxMoveFile);
            this.Controls.Add(this.CboxCutFile);
            this.Controls.Add(this.CBoxFirstFit);
            this.Controls.Add(this.CBoxGenFileList);
            this.Controls.Add(this.CBoxGenPar);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.LblParArgument);
            this.Controls.Add(this.LblBufferSize);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.NumBuffer);
            this.Controls.Add(this.NumDiscMaxRedundant);
            this.Controls.Add(this.NumDiscRedundant);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.NumDiscCapacity);
            this.Controls.Add(this.LstDiscFiles);
            this.Controls.Add(this.LstDiscs);
            this.Controls.Add(this.LstFiles);
            this.Controls.Add(this.BtnOutputFile);
            this.Controls.Add(this.BtnTempFolder);
            this.Controls.Add(this.BtnClearLstFiles);
            this.Controls.Add(this.BtnAllocateDisc);
            this.Controls.Add(this.BtnAddFiles);
            this.Controls.Add(this.BtnAddFolder);
            this.Name = "MainUI";
            this.ShowIcon = false;
            this.Text = "DiscHelper";
            ((System.ComponentModel.ISupportInitialize)(this.NumDiscCapacity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumDiscRedundant)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumDiscMaxRedundant)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumBuffer)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button BtnAddFolder;
        private System.Windows.Forms.Button BtnAddFiles;
        private System.Windows.Forms.ListBox LstFiles;
        private System.Windows.Forms.NumericUpDown NumDiscCapacity;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button BtnAllocateDisc;
        private System.Windows.Forms.ListBox LstDiscs;
        private System.Windows.Forms.ListBox LstDiscFiles;
        private System.Windows.Forms.Button BtnTempFolder;
        private System.Windows.Forms.Button BtnOutputFile;
        private System.Windows.Forms.NumericUpDown NumDiscRedundant;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox CBoxGenPar;
        private System.Windows.Forms.Button BtnClearLstFiles;
        private System.Windows.Forms.NumericUpDown NumDiscMaxRedundant;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox TxtDiscNamePattern;
        private System.Windows.Forms.CheckBox CBoxMoveFile;
        private System.Windows.Forms.TextBox TxtOutputPath;
        private System.Windows.Forms.RichTextBox TxtCMDOutput;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox CBoxAllocatePolicy;
        private System.Windows.Forms.CheckBox CBoxFirstFit;
        private System.Windows.Forms.ContextMenuStrip DiscHelperMenuStrip;
        private System.Windows.Forms.CheckBox CboxCutFile;
        private System.Windows.Forms.CheckBox CBoxGenFileList;
        private System.Windows.Forms.NumericUpDown NumBuffer;
        private System.Windows.Forms.Label LblBufferSize;
        private System.Windows.Forms.Label LblParArgument;
        private System.Windows.Forms.TextBox TxtParArgument;
        private System.Windows.Forms.Button BtnAddComplexFile;
        private System.Windows.Forms.Label LblComplexFileTemplate;
        private System.Windows.Forms.ComboBox CBoxTemplate;
    }
}

