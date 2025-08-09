using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI.WebControls;
using System.Windows.Forms;

namespace DiscHelper
{
    public partial class ComplexFile : Form
    {
        List<ComplexFileTemplate> ComplexFileTemplates;
        int TemplateSelectedIndex = -1;
        public FileItem newFileItem;
        public ComplexFile()
        {
            InitializeComponent();
            NumInputOuputRatio.Maximum = long.MaxValue;
            NumPredictedOutputSize.Maximum = long.MaxValue;

        }
        public ComplexFile(List<ComplexFileTemplate> complexFileTemplates,int templateSelectedIndex)
            :this()
        {
            ComplexFileTemplates = complexFileTemplates;
            TemplateSelectedIndex = templateSelectedIndex;
            updateTemplateList();
        }

        public ComplexFile(List<ComplexFileTemplate> complexFileTemplates, int templateSelectedIndex,FileItem fileItem)
            :this(complexFileTemplates,templateSelectedIndex)
        {
            if (fileItem != null)
            {
                TxtOutputCMD.Text = fileItem.Command;
                TxtOuputRelativeName.Text = fileItem.DestName;
                NumPredictedOutputSize.Value = fileItem.Size;
                LblPredictedOutputSizeReadable.Text = ((double)fileItem.Size / 1024 / 1024).ToString("F2") + " MB";
                TxtCommandExe.Text = fileItem.CommandExe;
                newFileItem = fileItem;
            }
        }

        public void LstComplexFile_AddItem(string Name)
        {
            if (Directory.Exists(Name))
            {
                string ParentFolder = new DirectoryInfo(Name)?.Parent?.FullName;
                ParentFolder = ParentFolder == null ? Name : ParentFolder;
                string[] files = Directory.GetFiles(Name, "*.*", SearchOption.AllDirectories);
                foreach (string file in files)
                {
                    string RelativeName = Common.GetRelativePath(file, ParentFolder);
                    LstComplexFile.Items.Add(new FileItem(file, RelativeName));
                }
            }
            else if (File.Exists(Name))
            {
                LstComplexFile.Items.Add(new FileItem(Name));
            }
        }

        public void LstComplexFile_DragEnter(object sender, DragEventArgs e)
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

        public void LstComplexFile_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string strNames in files)
                {
                    LstComplexFile_AddItem(strNames);
                }
            }
            setInputSize();
        }

        public void LstComplexFile_Clear(object sender, EventArgs e)
        {
            LstComplexFile.Items.Clear();
        }


        public void BtnAddFolder_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.Description = "请选择文件夹";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                string FolderName = dialog.SelectedPath;
                LstComplexFile_AddItem(FolderName);
            }

            setInputSize();
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
                    LstComplexFile_AddItem(strNames[i]);
                }
            }
            setInputSize();
        }



        private void updateTemplateList()
        {
            int selectedIndex = CBoxTemplate.SelectedIndex;
            if(TemplateSelectedIndex != -1)
            {
                selectedIndex = TemplateSelectedIndex;
                TemplateSelectedIndex = -1;
            }
            CBoxTemplate.Items.Clear();
            foreach (var template in ComplexFileTemplates)
            {
                CBoxTemplate.Items.Add(template);
            }
            if (CBoxTemplate.Items.Count > selectedIndex)
            {
                CBoxTemplate.SelectedIndex = selectedIndex;
            }
        }

        private void BtnAddTemplate_Click(object sender, EventArgs e)
        {
            string newTemplateName = TxtNewTemplateName.Text;
            ComplexFileTemplate newTemplate = new ComplexFileTemplate();
            bool isSameName = false;
            if (newTemplateName.Length == 0) {
                MessageBox.Show("请输入模版名称", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            else
            {
                for (int i = 0; i < ComplexFileTemplates.Count; i++)
                {
                    ComplexFileTemplate template = ComplexFileTemplates[i];
                    if (template != null && template.Name == newTemplateName)
                    {
                        newTemplate = template;
                        isSameName = true;
                    }
                }
            }
            newTemplate.Name = newTemplateName;
            newTemplate.FileInputReplaceStr = TxtInputReplace.Text;
            newTemplate.FileInputListSep = TxtListSep.Text;
            newTemplate.CommandLine = TxtCMD.Text;
            newTemplate.OutputFileSuffix = TxtOutputSuffix.Text;
            newTemplate.InputOutputSizeRatio = NumInputOuputRatio.Value;
            newTemplate.CommandLineExe = TxtCommandExe.Text;
            if (!isSameName)
                ComplexFileTemplates.Add(newTemplate);
            updateTemplateList();
        }

        private void CBoxTemplate_SelectedIndexChanged(object sender, EventArgs e)
        {
            ComplexFileTemplate template = CBoxTemplate.SelectedItem as ComplexFileTemplate;
            if (template != null)
            {
                TxtNewTemplateName.Text = template.Name;
                TxtInputReplace.Text = template.FileInputReplaceStr;
                TxtListSep.Text = template.FileInputListSep;
                TxtCMD.Text = template.CommandLine;
                TxtOutputSuffix.Text = template.OutputFileSuffix;
                NumInputOuputRatio.Value = template.InputOutputSizeRatio;
                TxtCommandExe.Text = template.CommandLineExe;
            }
        }

        private void BtnRemoveTemplate_Click(object sender, EventArgs e)
        {
            int templateIndex = CBoxTemplate.SelectedIndex;
            if (templateIndex != -1)
            {
                ComplexFileTemplates.RemoveAt(templateIndex);
            }
            updateTemplateList();
        }

        private void BtnConfirm_Click(object sender, EventArgs e)
        {
            if (newFileItem != null)
            {
                //新建一个 防止修改了在DiscItem里的FileItem
                if (TxtCommandExe.Text != "")
                {
                    FileItem NewFileItem = new FileItem();
                    NewFileItem.Name = TxtOuputRelativeName.Text;
                    NewFileItem.DestName = TxtOuputRelativeName.Text;
                    NewFileItem.Size = (long)NumPredictedOutputSize.Value;
                    NewFileItem.CreateTime = DateTime.Now.ToLocalTime();
                    NewFileItem.Command = TxtOutputCMD.Text;
                    NewFileItem.CommandExe = TxtCommandExe.Text;
                    NewFileItem.isFirstCommand = true;
                    newFileItem = NewFileItem;
                    DialogResult = DialogResult.OK;
                }
            }
            Close();
        }

        private long ComputeInputTotalSize()
        {
            List<FileItem> fileItems = new List<FileItem>();
            foreach (FileItem fileItem in LstComplexFile.Items)
            {
                fileItems.Add(fileItem);
            }
            return ComplexFileTemplate.ComputeInputTotalSize(fileItems);
        }


        private void setInputSize()
        {
            long totalSize = ComputeInputTotalSize();
            LblInputTotalSizeNum.Text = ((double)totalSize / 1024 / 1024).ToString("F2") + " MB";
        }

        private void BtnGenerate_Click(object sender, EventArgs e)
        {
            List<FileItem> fileItems = new List<FileItem>();
            foreach (FileItem fileItem in LstComplexFile.Items)
            {
                fileItems.Add(fileItem);
            }
            FileItem newFileItem = ComplexFileTemplate.GenerateComplexFileItem(fileItems,TxtCommandExe.Text, TxtCMD.Text, TxtInputReplace.Text, TxtListSep.Text, TxtOutputSuffix.Text,(double)NumInputOuputRatio.Value);
            if(newFileItem != null)
            {
                TxtOutputCMD.Text = newFileItem.Command;
                TxtOuputRelativeName.Text = newFileItem.DestName;
                NumPredictedOutputSize.Value = newFileItem.Size;
                LblPredictedOutputSizeReadable.Text = ((double)newFileItem.Size / 1024 / 1024).ToString("F2") + " MB";
                this.newFileItem = newFileItem;
            }
        }

        private void NumPredictedOutputSize_ValueChanged(object sender, EventArgs e)
        {
            LblPredictedOutputSizeReadable.Text = ((double)NumPredictedOutputSize.Value / 1024 / 1024).ToString("F2") + " MB";
        }
    }
}
