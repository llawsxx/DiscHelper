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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainUI));
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
            this.label2 = new System.Windows.Forms.Label();
            this.NumDiscRedundant = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.CBoxGenPar = new System.Windows.Forms.CheckBox();
            this.button4 = new System.Windows.Forms.Button();
            this.NumDiscMaxRedundant = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.NumDiscInBucket = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.TxtDiscPrefix = new System.Windows.Forms.TextBox();
            this.NumDiscBucket = new System.Windows.Forms.NumericUpDown();
            this.CBoxMoveFile = new System.Windows.Forms.CheckBox();
            this.TxtOutputPath = new System.Windows.Forms.TextBox();
            this.TxtCMDOutput = new System.Windows.Forms.RichTextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.CBoxAllocatePolicy = new System.Windows.Forms.ComboBox();
            this.CBoxFirstFit = new System.Windows.Forms.CheckBox();
            this.NumMaxDiscInBucket = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.DiscHelperMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.CboxCutFile = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.NumDiscCapacity)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumDiscRedundant)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumDiscMaxRedundant)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumDiscInBucket)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumDiscBucket)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumMaxDiscInBucket)).BeginInit();
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
            this.LstFiles.Location = new System.Drawing.Point(12, 82);
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
            this.NumDiscCapacity.Location = new System.Drawing.Point(239, 16);
            this.NumDiscCapacity.Maximum = new decimal(new int[] {
            -1530494977,
            232830,
            0,
            0});
            this.NumDiscCapacity.Name = "NumDiscCapacity";
            this.NumDiscCapacity.Size = new System.Drawing.Size(125, 23);
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
            this.label1.Location = new System.Drawing.Point(179, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 17);
            this.label1.TabIndex = 3;
            this.label1.Text = "光盘容量";
            // 
            // BtnAllocateDisc
            // 
            this.BtnAllocateDisc.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BtnAllocateDisc.Location = new System.Drawing.Point(477, 47);
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
            this.LstDiscs.Location = new System.Drawing.Point(14, 246);
            this.LstDiscs.Name = "LstDiscs";
            this.LstDiscs.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.LstDiscs.Size = new System.Drawing.Size(222, 140);
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
            this.LstDiscFiles.Location = new System.Drawing.Point(242, 246);
            this.LstDiscFiles.Name = "LstDiscFiles";
            this.LstDiscFiles.SelectionMode = System.Windows.Forms.SelectionMode.MultiExtended;
            this.LstDiscFiles.Size = new System.Drawing.Size(288, 140);
            this.LstDiscFiles.TabIndex = 1;
            this.LstDiscFiles.MouseDown += new System.Windows.Forms.MouseEventHandler(this.LstDiscFiles_MouseDown);
            // 
            // BtnTempFolder
            // 
            this.BtnTempFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.BtnTempFolder.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BtnTempFolder.Location = new System.Drawing.Point(14, 395);
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
            this.BtnOutputFile.Location = new System.Drawing.Point(424, 395);
            this.BtnOutputFile.Name = "BtnOutputFile";
            this.BtnOutputFile.Size = new System.Drawing.Size(105, 29);
            this.BtnOutputFile.TabIndex = 0;
            this.BtnOutputFile.Text = "开始输出";
            this.BtnOutputFile.UseVisualStyleBackColor = true;
            this.BtnOutputFile.Click += new System.EventHandler(this.BtnOutputFile_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label2.Location = new System.Drawing.Point(174, 217);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 17);
            this.label2.TabIndex = 3;
            this.label2.Text = "桶序号";
            // 
            // NumDiscRedundant
            // 
            this.NumDiscRedundant.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.NumDiscRedundant.Location = new System.Drawing.Point(74, 51);
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
            this.label3.Location = new System.Drawing.Point(12, 53);
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
            this.CBoxGenPar.Location = new System.Drawing.Point(267, 401);
            this.CBoxGenPar.Name = "CBoxGenPar";
            this.CBoxGenPar.Size = new System.Drawing.Size(70, 21);
            this.CBoxGenPar.TabIndex = 5;
            this.CBoxGenPar.Text = "生成Par";
            this.CBoxGenPar.UseVisualStyleBackColor = true;
            // 
            // button4
            // 
            this.button4.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button4.Location = new System.Drawing.Point(12, 211);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(56, 29);
            this.button4.TabIndex = 0;
            this.button4.Text = "清空";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // NumDiscMaxRedundant
            // 
            this.NumDiscMaxRedundant.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.NumDiscMaxRedundant.Location = new System.Drawing.Point(232, 51);
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
            this.label4.Location = new System.Drawing.Point(170, 53);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 17);
            this.label4.TabIndex = 3;
            this.label4.Text = "最大冗余";
            // 
            // NumDiscInBucket
            // 
            this.NumDiscInBucket.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.NumDiscInBucket.Location = new System.Drawing.Point(328, 214);
            this.NumDiscInBucket.Maximum = new decimal(new int[] {
            -1530494977,
            232830,
            0,
            0});
            this.NumDiscInBucket.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NumDiscInBucket.Name = "NumDiscInBucket";
            this.NumDiscInBucket.Size = new System.Drawing.Size(43, 23);
            this.NumDiscInBucket.TabIndex = 2;
            this.NumDiscInBucket.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label5.Location = new System.Drawing.Point(272, 217);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(56, 17);
            this.label5.TabIndex = 3;
            this.label5.Text = "桶内序号";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label6.Location = new System.Drawing.Point(77, 217);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(32, 17);
            this.label6.TabIndex = 3;
            this.label6.Text = "前缀";
            // 
            // TxtDiscPrefix
            // 
            this.TxtDiscPrefix.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.TxtDiscPrefix.Location = new System.Drawing.Point(108, 214);
            this.TxtDiscPrefix.Name = "TxtDiscPrefix";
            this.TxtDiscPrefix.Size = new System.Drawing.Size(58, 23);
            this.TxtDiscPrefix.TabIndex = 7;
            // 
            // NumDiscBucket
            // 
            this.NumDiscBucket.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.NumDiscBucket.Location = new System.Drawing.Point(224, 214);
            this.NumDiscBucket.Maximum = new decimal(new int[] {
            -1530494977,
            232830,
            0,
            0});
            this.NumDiscBucket.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NumDiscBucket.Name = "NumDiscBucket";
            this.NumDiscBucket.Size = new System.Drawing.Size(42, 23);
            this.NumDiscBucket.TabIndex = 2;
            this.NumDiscBucket.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // CBoxMoveFile
            // 
            this.CBoxMoveFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.CBoxMoveFile.AutoSize = true;
            this.CBoxMoveFile.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.CBoxMoveFile.Location = new System.Drawing.Point(342, 401);
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
            this.TxtOutputPath.Location = new System.Drawing.Point(126, 398);
            this.TxtOutputPath.Name = "TxtOutputPath";
            this.TxtOutputPath.Size = new System.Drawing.Size(135, 23);
            this.TxtOutputPath.TabIndex = 7;
            this.TxtOutputPath.Text = "DISC";
            // 
            // TxtCMDOutput
            // 
            this.TxtCMDOutput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TxtCMDOutput.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.TxtCMDOutput.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.TxtCMDOutput.Location = new System.Drawing.Point(15, 430);
            this.TxtCMDOutput.Name = "TxtCMDOutput";
            this.TxtCMDOutput.Size = new System.Drawing.Size(517, 102);
            this.TxtCMDOutput.TabIndex = 8;
            this.TxtCMDOutput.Text = "";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label7.Location = new System.Drawing.Point(370, 18);
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
            this.CBoxAllocatePolicy.Location = new System.Drawing.Point(432, 15);
            this.CBoxAllocatePolicy.Name = "CBoxAllocatePolicy";
            this.CBoxAllocatePolicy.Size = new System.Drawing.Size(98, 25);
            this.CBoxAllocatePolicy.TabIndex = 10;
            // 
            // CBoxFirstFit
            // 
            this.CBoxFirstFit.AutoSize = true;
            this.CBoxFirstFit.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.CBoxFirstFit.Location = new System.Drawing.Point(409, 51);
            this.CBoxFirstFit.Name = "CBoxFirstFit";
            this.CBoxFirstFit.Size = new System.Drawing.Size(68, 21);
            this.CBoxFirstFit.TabIndex = 5;
            this.CBoxFirstFit.Text = "First Fit";
            this.CBoxFirstFit.UseVisualStyleBackColor = true;
            // 
            // NumMaxDiscInBucket
            // 
            this.NumMaxDiscInBucket.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.NumMaxDiscInBucket.Location = new System.Drawing.Point(464, 214);
            this.NumMaxDiscInBucket.Maximum = new decimal(new int[] {
            -1530494977,
            232830,
            0,
            0});
            this.NumMaxDiscInBucket.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NumMaxDiscInBucket.Name = "NumMaxDiscInBucket";
            this.NumMaxDiscInBucket.Size = new System.Drawing.Size(42, 23);
            this.NumMaxDiscInBucket.TabIndex = 2;
            this.NumMaxDiscInBucket.Value = new decimal(new int[] {
            50,
            0,
            0,
            0});
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label8.Location = new System.Drawing.Point(378, 217);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(80, 17);
            this.label8.TabIndex = 3;
            this.label8.Text = "桶内最多光盘";
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
            this.CboxCutFile.Location = new System.Drawing.Point(328, 51);
            this.CboxCutFile.Name = "CboxCutFile";
            this.CboxCutFile.Size = new System.Drawing.Size(75, 21);
            this.CboxCutFile.TabIndex = 5;
            this.CboxCutFile.Text = "分割文件";
            this.CboxCutFile.UseVisualStyleBackColor = true;
            // 
            // MainUI
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(544, 544);
            this.Controls.Add(this.CBoxAllocatePolicy);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.TxtCMDOutput);
            this.Controls.Add(this.TxtOutputPath);
            this.Controls.Add(this.TxtDiscPrefix);
            this.Controls.Add(this.CBoxMoveFile);
            this.Controls.Add(this.CboxCutFile);
            this.Controls.Add(this.CBoxFirstFit);
            this.Controls.Add(this.CBoxGenPar);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.NumMaxDiscInBucket);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.NumDiscInBucket);
            this.Controls.Add(this.NumDiscMaxRedundant);
            this.Controls.Add(this.NumDiscBucket);
            this.Controls.Add(this.NumDiscRedundant);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.NumDiscCapacity);
            this.Controls.Add(this.LstDiscFiles);
            this.Controls.Add(this.LstDiscs);
            this.Controls.Add(this.LstFiles);
            this.Controls.Add(this.BtnOutputFile);
            this.Controls.Add(this.BtnTempFolder);
            this.Controls.Add(this.button4);
            this.Controls.Add(this.BtnAllocateDisc);
            this.Controls.Add(this.BtnAddFiles);
            this.Controls.Add(this.BtnAddFolder);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainUI";
            this.Text = "DiscHelper";
            ((System.ComponentModel.ISupportInitialize)(this.NumDiscCapacity)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumDiscRedundant)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumDiscMaxRedundant)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumDiscInBucket)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumDiscBucket)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumMaxDiscInBucket)).EndInit();
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
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown NumDiscRedundant;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox CBoxGenPar;
        private System.Windows.Forms.Button button4;
        private System.Windows.Forms.NumericUpDown NumDiscMaxRedundant;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.NumericUpDown NumDiscInBucket;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox TxtDiscPrefix;
        private System.Windows.Forms.NumericUpDown NumDiscBucket;
        private System.Windows.Forms.CheckBox CBoxMoveFile;
        private System.Windows.Forms.TextBox TxtOutputPath;
        private System.Windows.Forms.RichTextBox TxtCMDOutput;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox CBoxAllocatePolicy;
        private System.Windows.Forms.CheckBox CBoxFirstFit;
        private System.Windows.Forms.NumericUpDown NumMaxDiscInBucket;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ContextMenuStrip DiscHelperMenuStrip;
        private System.Windows.Forms.CheckBox CboxCutFile;
    }
}

