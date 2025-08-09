namespace DiscHelper
{
    partial class ComplexFile
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.LstComplexFile = new System.Windows.Forms.ListBox();
            this.TxtCMD = new System.Windows.Forms.RichTextBox();
            this.LblCMD = new System.Windows.Forms.Label();
            this.BtnAddFiles = new System.Windows.Forms.Button();
            this.BtnAddFolder = new System.Windows.Forms.Button();
            this.LblInputReplace = new System.Windows.Forms.Label();
            this.LblListSep = new System.Windows.Forms.Label();
            this.TxtInputReplace = new System.Windows.Forms.TextBox();
            this.TxtListSep = new System.Windows.Forms.TextBox();
            this.LblInputTotalSize = new System.Windows.Forms.Label();
            this.LblPredictedOutputSize = new System.Windows.Forms.Label();
            this.LblInputTotalSizeNum = new System.Windows.Forms.Label();
            this.LblInputOuputRatio = new System.Windows.Forms.Label();
            this.NumInputOuputRatio = new System.Windows.Forms.NumericUpDown();
            this.LblInputOuputRatioTxt = new System.Windows.Forms.Label();
            this.NumPredictedOutputSize = new System.Windows.Forms.NumericUpDown();
            this.LblPredictedOutputSizeReadable = new System.Windows.Forms.Label();
            this.BtnConfirm = new System.Windows.Forms.Button();
            this.LblTemplate = new System.Windows.Forms.Label();
            this.CBoxTemplate = new System.Windows.Forms.ComboBox();
            this.BtnRemoveTemplate = new System.Windows.Forms.Button();
            this.BtnAddTemplate = new System.Windows.Forms.Button();
            this.TxtNewTemplateName = new System.Windows.Forms.TextBox();
            this.BtnRemoveFile = new System.Windows.Forms.Button();
            this.TxtOutputSuffix = new System.Windows.Forms.TextBox();
            this.LblOutputSuffix = new System.Windows.Forms.Label();
            this.LblOuputRelativeName = new System.Windows.Forms.Label();
            this.TxtOuputRelativeName = new System.Windows.Forms.TextBox();
            this.BtnGenerate = new System.Windows.Forms.Button();
            this.TxtOutputCMD = new System.Windows.Forms.RichTextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.TxtCommandExe = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.NumInputOuputRatio)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumPredictedOutputSize)).BeginInit();
            this.SuspendLayout();
            // 
            // LstComplexFile
            // 
            this.LstComplexFile.AllowDrop = true;
            this.LstComplexFile.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.LstComplexFile.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.LstComplexFile.FormattingEnabled = true;
            this.LstComplexFile.ItemHeight = 17;
            this.LstComplexFile.Location = new System.Drawing.Point(12, 42);
            this.LstComplexFile.Name = "LstComplexFile";
            this.LstComplexFile.Size = new System.Drawing.Size(563, 106);
            this.LstComplexFile.TabIndex = 0;
            this.LstComplexFile.DragDrop += new System.Windows.Forms.DragEventHandler(this.LstComplexFile_DragDrop);
            this.LstComplexFile.DragEnter += new System.Windows.Forms.DragEventHandler(this.LstComplexFile_DragEnter);
            // 
            // TxtCMD
            // 
            this.TxtCMD.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TxtCMD.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.TxtCMD.Location = new System.Drawing.Point(12, 213);
            this.TxtCMD.Name = "TxtCMD";
            this.TxtCMD.Size = new System.Drawing.Size(563, 59);
            this.TxtCMD.TabIndex = 1;
            this.TxtCMD.Text = "";
            // 
            // LblCMD
            // 
            this.LblCMD.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LblCMD.AutoSize = true;
            this.LblCMD.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.LblCMD.Location = new System.Drawing.Point(170, 187);
            this.LblCMD.Name = "LblCMD";
            this.LblCMD.Size = new System.Drawing.Size(104, 17);
            this.LblCMD.TabIndex = 2;
            this.LblCMD.Text = "以下输入处理命令";
            // 
            // BtnAddFiles
            // 
            this.BtnAddFiles.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BtnAddFiles.Location = new System.Drawing.Point(98, 7);
            this.BtnAddFiles.Name = "BtnAddFiles";
            this.BtnAddFiles.Size = new System.Drawing.Size(75, 29);
            this.BtnAddFiles.TabIndex = 3;
            this.BtnAddFiles.Text = "添加文件";
            this.BtnAddFiles.UseVisualStyleBackColor = true;
            this.BtnAddFiles.Click += new System.EventHandler(this.BtnAddFiles_Click);
            // 
            // BtnAddFolder
            // 
            this.BtnAddFolder.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BtnAddFolder.Location = new System.Drawing.Point(12, 7);
            this.BtnAddFolder.Name = "BtnAddFolder";
            this.BtnAddFolder.Size = new System.Drawing.Size(80, 29);
            this.BtnAddFolder.TabIndex = 4;
            this.BtnAddFolder.Text = "添加文件夹";
            this.BtnAddFolder.UseVisualStyleBackColor = true;
            this.BtnAddFolder.Click += new System.EventHandler(this.BtnAddFolder_Click);
            // 
            // LblInputReplace
            // 
            this.LblInputReplace.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LblInputReplace.AutoSize = true;
            this.LblInputReplace.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.LblInputReplace.Location = new System.Drawing.Point(288, 187);
            this.LblInputReplace.Name = "LblInputReplace";
            this.LblInputReplace.Size = new System.Drawing.Size(92, 17);
            this.LblInputReplace.TabIndex = 5;
            this.LblInputReplace.Text = "文件输入替代符";
            // 
            // LblListSep
            // 
            this.LblListSep.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LblListSep.AutoSize = true;
            this.LblListSep.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.LblListSep.Location = new System.Drawing.Point(464, 187);
            this.LblListSep.Name = "LblListSep";
            this.LblListSep.Size = new System.Drawing.Size(68, 17);
            this.LblListSep.TabIndex = 5;
            this.LblListSep.Text = "列表分隔符";
            // 
            // TxtInputReplace
            // 
            this.TxtInputReplace.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.TxtInputReplace.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.TxtInputReplace.Location = new System.Drawing.Point(386, 184);
            this.TxtInputReplace.Name = "TxtInputReplace";
            this.TxtInputReplace.Size = new System.Drawing.Size(60, 23);
            this.TxtInputReplace.TabIndex = 6;
            this.TxtInputReplace.Text = "#{input}";
            // 
            // TxtListSep
            // 
            this.TxtListSep.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.TxtListSep.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.TxtListSep.Location = new System.Drawing.Point(538, 184);
            this.TxtListSep.Name = "TxtListSep";
            this.TxtListSep.Size = new System.Drawing.Size(37, 23);
            this.TxtListSep.TabIndex = 6;
            this.TxtListSep.Text = "|";
            // 
            // LblInputTotalSize
            // 
            this.LblInputTotalSize.AutoSize = true;
            this.LblInputTotalSize.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.LblInputTotalSize.Location = new System.Drawing.Point(264, 13);
            this.LblInputTotalSize.Name = "LblInputTotalSize";
            this.LblInputTotalSize.Size = new System.Drawing.Size(92, 17);
            this.LblInputTotalSize.TabIndex = 7;
            this.LblInputTotalSize.Text = "输入文件总大小";
            // 
            // LblPredictedOutputSize
            // 
            this.LblPredictedOutputSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LblPredictedOutputSize.AutoSize = true;
            this.LblPredictedOutputSize.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.LblPredictedOutputSize.Location = new System.Drawing.Point(10, 411);
            this.LblPredictedOutputSize.Name = "LblPredictedOutputSize";
            this.LblPredictedOutputSize.Size = new System.Drawing.Size(104, 17);
            this.LblPredictedOutputSize.TabIndex = 8;
            this.LblPredictedOutputSize.Text = "预计输出文件大小";
            // 
            // LblInputTotalSizeNum
            // 
            this.LblInputTotalSizeNum.AutoSize = true;
            this.LblInputTotalSizeNum.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.LblInputTotalSizeNum.Location = new System.Drawing.Point(362, 13);
            this.LblInputTotalSizeNum.Name = "LblInputTotalSizeNum";
            this.LblInputTotalSizeNum.Size = new System.Drawing.Size(28, 17);
            this.LblInputTotalSizeNum.TabIndex = 9;
            this.LblInputTotalSizeNum.Text = "(空)";
            // 
            // LblInputOuputRatio
            // 
            this.LblInputOuputRatio.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LblInputOuputRatio.AutoSize = true;
            this.LblInputOuputRatio.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.LblInputOuputRatio.Location = new System.Drawing.Point(9, 442);
            this.LblInputOuputRatio.Name = "LblInputOuputRatio";
            this.LblInputOuputRatio.Size = new System.Drawing.Size(164, 17);
            this.LblInputOuputRatio.TabIndex = 11;
            this.LblInputOuputRatio.Text = "预计输出文件大小为原文件的";
            // 
            // NumInputOuputRatio
            // 
            this.NumInputOuputRatio.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.NumInputOuputRatio.DecimalPlaces = 6;
            this.NumInputOuputRatio.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.NumInputOuputRatio.Location = new System.Drawing.Point(182, 438);
            this.NumInputOuputRatio.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NumInputOuputRatio.Name = "NumInputOuputRatio";
            this.NumInputOuputRatio.Size = new System.Drawing.Size(76, 23);
            this.NumInputOuputRatio.TabIndex = 12;
            this.NumInputOuputRatio.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // LblInputOuputRatioTxt
            // 
            this.LblInputOuputRatioTxt.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LblInputOuputRatioTxt.AutoSize = true;
            this.LblInputOuputRatioTxt.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.LblInputOuputRatioTxt.Location = new System.Drawing.Point(264, 440);
            this.LblInputOuputRatioTxt.Name = "LblInputOuputRatioTxt";
            this.LblInputOuputRatioTxt.Size = new System.Drawing.Size(20, 17);
            this.LblInputOuputRatioTxt.TabIndex = 13;
            this.LblInputOuputRatioTxt.Text = "倍";
            // 
            // NumPredictedOutputSize
            // 
            this.NumPredictedOutputSize.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.NumPredictedOutputSize.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.NumPredictedOutputSize.Location = new System.Drawing.Point(120, 409);
            this.NumPredictedOutputSize.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.NumPredictedOutputSize.Name = "NumPredictedOutputSize";
            this.NumPredictedOutputSize.Size = new System.Drawing.Size(138, 23);
            this.NumPredictedOutputSize.TabIndex = 14;
            this.NumPredictedOutputSize.ValueChanged += new System.EventHandler(this.NumPredictedOutputSize_ValueChanged);
            // 
            // LblPredictedOutputSizeReadable
            // 
            this.LblPredictedOutputSizeReadable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LblPredictedOutputSizeReadable.AutoSize = true;
            this.LblPredictedOutputSizeReadable.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.LblPredictedOutputSizeReadable.Location = new System.Drawing.Point(264, 411);
            this.LblPredictedOutputSizeReadable.Name = "LblPredictedOutputSizeReadable";
            this.LblPredictedOutputSizeReadable.Size = new System.Drawing.Size(28, 17);
            this.LblPredictedOutputSizeReadable.TabIndex = 15;
            this.LblPredictedOutputSizeReadable.Text = "(空)";
            // 
            // BtnConfirm
            // 
            this.BtnConfirm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnConfirm.Font = new System.Drawing.Font("微软雅黑", 11F);
            this.BtnConfirm.Location = new System.Drawing.Point(500, 424);
            this.BtnConfirm.Name = "BtnConfirm";
            this.BtnConfirm.Size = new System.Drawing.Size(75, 35);
            this.BtnConfirm.TabIndex = 16;
            this.BtnConfirm.Text = "确定";
            this.BtnConfirm.UseVisualStyleBackColor = true;
            this.BtnConfirm.Click += new System.EventHandler(this.BtnConfirm_Click);
            // 
            // LblTemplate
            // 
            this.LblTemplate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LblTemplate.AutoSize = true;
            this.LblTemplate.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.LblTemplate.Location = new System.Drawing.Point(12, 159);
            this.LblTemplate.Name = "LblTemplate";
            this.LblTemplate.Size = new System.Drawing.Size(32, 17);
            this.LblTemplate.TabIndex = 17;
            this.LblTemplate.Text = "模板";
            // 
            // CBoxTemplate
            // 
            this.CBoxTemplate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CBoxTemplate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CBoxTemplate.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.CBoxTemplate.FormattingEnabled = true;
            this.CBoxTemplate.Location = new System.Drawing.Point(50, 156);
            this.CBoxTemplate.Name = "CBoxTemplate";
            this.CBoxTemplate.Size = new System.Drawing.Size(208, 25);
            this.CBoxTemplate.TabIndex = 18;
            this.CBoxTemplate.SelectedIndexChanged += new System.EventHandler(this.CBoxTemplate_SelectedIndexChanged);
            // 
            // BtnRemoveTemplate
            // 
            this.BtnRemoveTemplate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.BtnRemoveTemplate.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BtnRemoveTemplate.Location = new System.Drawing.Point(264, 153);
            this.BtnRemoveTemplate.Name = "BtnRemoveTemplate";
            this.BtnRemoveTemplate.Size = new System.Drawing.Size(75, 29);
            this.BtnRemoveTemplate.TabIndex = 19;
            this.BtnRemoveTemplate.Text = "删除模板";
            this.BtnRemoveTemplate.UseVisualStyleBackColor = true;
            this.BtnRemoveTemplate.Click += new System.EventHandler(this.BtnRemoveTemplate_Click);
            // 
            // BtnAddTemplate
            // 
            this.BtnAddTemplate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.BtnAddTemplate.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BtnAddTemplate.Location = new System.Drawing.Point(467, 153);
            this.BtnAddTemplate.Name = "BtnAddTemplate";
            this.BtnAddTemplate.Size = new System.Drawing.Size(108, 29);
            this.BtnAddTemplate.TabIndex = 20;
            this.BtnAddTemplate.Text = "添加/保存模板";
            this.BtnAddTemplate.UseVisualStyleBackColor = true;
            this.BtnAddTemplate.Click += new System.EventHandler(this.BtnAddTemplate_Click);
            // 
            // TxtNewTemplateName
            // 
            this.TxtNewTemplateName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.TxtNewTemplateName.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.TxtNewTemplateName.Location = new System.Drawing.Point(359, 158);
            this.TxtNewTemplateName.Name = "TxtNewTemplateName";
            this.TxtNewTemplateName.Size = new System.Drawing.Size(102, 23);
            this.TxtNewTemplateName.TabIndex = 21;
            // 
            // BtnRemoveFile
            // 
            this.BtnRemoveFile.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.BtnRemoveFile.Location = new System.Drawing.Point(179, 7);
            this.BtnRemoveFile.Name = "BtnRemoveFile";
            this.BtnRemoveFile.Size = new System.Drawing.Size(75, 29);
            this.BtnRemoveFile.TabIndex = 22;
            this.BtnRemoveFile.Text = "清空";
            this.BtnRemoveFile.UseVisualStyleBackColor = true;
            this.BtnRemoveFile.Click += new System.EventHandler(this.LstComplexFile_Clear);
            // 
            // TxtOutputSuffix
            // 
            this.TxtOutputSuffix.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.TxtOutputSuffix.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.TxtOutputSuffix.Location = new System.Drawing.Point(110, 377);
            this.TxtOutputSuffix.Name = "TxtOutputSuffix";
            this.TxtOutputSuffix.Size = new System.Drawing.Size(63, 23);
            this.TxtOutputSuffix.TabIndex = 24;
            // 
            // LblOutputSuffix
            // 
            this.LblOutputSuffix.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LblOutputSuffix.AutoSize = true;
            this.LblOutputSuffix.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.LblOutputSuffix.Location = new System.Drawing.Point(10, 380);
            this.LblOutputSuffix.Name = "LblOutputSuffix";
            this.LblOutputSuffix.Size = new System.Drawing.Size(92, 17);
            this.LblOutputSuffix.TabIndex = 23;
            this.LblOutputSuffix.Text = "输出文件名后缀";
            // 
            // LblOuputRelativeName
            // 
            this.LblOuputRelativeName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.LblOuputRelativeName.AutoSize = true;
            this.LblOuputRelativeName.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.LblOuputRelativeName.Location = new System.Drawing.Point(180, 380);
            this.LblOuputRelativeName.Name = "LblOuputRelativeName";
            this.LblOuputRelativeName.Size = new System.Drawing.Size(104, 17);
            this.LblOuputRelativeName.TabIndex = 25;
            this.LblOuputRelativeName.Text = "输出的相对文件名";
            // 
            // TxtOuputRelativeName
            // 
            this.TxtOuputRelativeName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TxtOuputRelativeName.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.TxtOuputRelativeName.Location = new System.Drawing.Point(290, 377);
            this.TxtOuputRelativeName.Name = "TxtOuputRelativeName";
            this.TxtOuputRelativeName.Size = new System.Drawing.Size(285, 23);
            this.TxtOuputRelativeName.TabIndex = 26;
            // 
            // BtnGenerate
            // 
            this.BtnGenerate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.BtnGenerate.Font = new System.Drawing.Font("微软雅黑", 11F);
            this.BtnGenerate.Location = new System.Drawing.Point(419, 424);
            this.BtnGenerate.Name = "BtnGenerate";
            this.BtnGenerate.Size = new System.Drawing.Size(75, 35);
            this.BtnGenerate.TabIndex = 27;
            this.BtnGenerate.Text = "生成";
            this.BtnGenerate.UseVisualStyleBackColor = true;
            this.BtnGenerate.Click += new System.EventHandler(this.BtnGenerate_Click);
            // 
            // TxtOutputCMD
            // 
            this.TxtOutputCMD.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TxtOutputCMD.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.TxtOutputCMD.Location = new System.Drawing.Point(12, 297);
            this.TxtOutputCMD.Name = "TxtOutputCMD";
            this.TxtOutputCMD.Size = new System.Drawing.Size(563, 74);
            this.TxtOutputCMD.TabIndex = 28;
            this.TxtOutputCMD.Text = "";
            // 
            // label13
            // 
            this.label13.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label13.Location = new System.Drawing.Point(12, 275);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(92, 17);
            this.label13.TabIndex = 29;
            this.label13.Text = "生成的处理命令";
            // 
            // TxtCommandExe
            // 
            this.TxtCommandExe.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.TxtCommandExe.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.TxtCommandExe.Location = new System.Drawing.Point(86, 184);
            this.TxtCommandExe.Name = "TxtCommandExe";
            this.TxtCommandExe.Size = new System.Drawing.Size(78, 23);
            this.TxtCommandExe.TabIndex = 30;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label1.Location = new System.Drawing.Point(12, 187);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 17);
            this.label1.TabIndex = 31;
            this.label1.Text = "可执行文件";
            // 
            // ComplexFile
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(587, 472);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.TxtCommandExe);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.TxtOutputCMD);
            this.Controls.Add(this.BtnGenerate);
            this.Controls.Add(this.TxtOuputRelativeName);
            this.Controls.Add(this.LblOuputRelativeName);
            this.Controls.Add(this.TxtOutputSuffix);
            this.Controls.Add(this.LblOutputSuffix);
            this.Controls.Add(this.BtnRemoveFile);
            this.Controls.Add(this.TxtNewTemplateName);
            this.Controls.Add(this.BtnAddTemplate);
            this.Controls.Add(this.BtnRemoveTemplate);
            this.Controls.Add(this.CBoxTemplate);
            this.Controls.Add(this.LblTemplate);
            this.Controls.Add(this.BtnConfirm);
            this.Controls.Add(this.LblPredictedOutputSizeReadable);
            this.Controls.Add(this.NumPredictedOutputSize);
            this.Controls.Add(this.LblInputOuputRatioTxt);
            this.Controls.Add(this.NumInputOuputRatio);
            this.Controls.Add(this.LblInputOuputRatio);
            this.Controls.Add(this.LblInputTotalSizeNum);
            this.Controls.Add(this.LblPredictedOutputSize);
            this.Controls.Add(this.LblInputTotalSize);
            this.Controls.Add(this.TxtListSep);
            this.Controls.Add(this.TxtInputReplace);
            this.Controls.Add(this.LblListSep);
            this.Controls.Add(this.LblInputReplace);
            this.Controls.Add(this.BtnAddFiles);
            this.Controls.Add(this.BtnAddFolder);
            this.Controls.Add(this.LblCMD);
            this.Controls.Add(this.TxtCMD);
            this.Controls.Add(this.LstComplexFile);
            this.Name = "ComplexFile";
            this.ShowIcon = false;
            this.Text = "高级文件（一个或多个文件生成一个处理后文件，执行的命令需将文件数据输出到stdout）";
            ((System.ComponentModel.ISupportInitialize)(this.NumInputOuputRatio)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.NumPredictedOutputSize)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox LstComplexFile;
        private System.Windows.Forms.RichTextBox TxtCMD;
        private System.Windows.Forms.Label LblCMD;
        private System.Windows.Forms.Button BtnAddFiles;
        private System.Windows.Forms.Button BtnAddFolder;
        private System.Windows.Forms.Label LblInputReplace;
        private System.Windows.Forms.Label LblListSep;
        private System.Windows.Forms.TextBox TxtInputReplace;
        private System.Windows.Forms.TextBox TxtListSep;
        private System.Windows.Forms.Label LblInputTotalSize;
        private System.Windows.Forms.Label LblPredictedOutputSize;
        private System.Windows.Forms.Label LblInputTotalSizeNum;
        private System.Windows.Forms.Label LblInputOuputRatio;
        private System.Windows.Forms.NumericUpDown NumInputOuputRatio;
        private System.Windows.Forms.Label LblInputOuputRatioTxt;
        private System.Windows.Forms.NumericUpDown NumPredictedOutputSize;
        private System.Windows.Forms.Label LblPredictedOutputSizeReadable;
        private System.Windows.Forms.Button BtnConfirm;
        private System.Windows.Forms.Label LblTemplate;
        private System.Windows.Forms.ComboBox CBoxTemplate;
        private System.Windows.Forms.Button BtnRemoveTemplate;
        private System.Windows.Forms.Button BtnAddTemplate;
        private System.Windows.Forms.TextBox TxtNewTemplateName;
        private System.Windows.Forms.Button BtnRemoveFile;
        private System.Windows.Forms.TextBox TxtOutputSuffix;
        private System.Windows.Forms.Label LblOutputSuffix;
        private System.Windows.Forms.Label LblOuputRelativeName;
        private System.Windows.Forms.TextBox TxtOuputRelativeName;
        private System.Windows.Forms.Button BtnGenerate;
        private System.Windows.Forms.RichTextBox TxtOutputCMD;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox TxtCommandExe;
        private System.Windows.Forms.Label label1;
    }
}