using ListViewEmbeddedControls;

namespace SearchDirLists
{
    partial class Form1
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

        #region Windows Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.form_tabControl = new System.Windows.Forms.TabControl();
            this.form_tabPageVolumes = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.form_cbVolumeName = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.form_cbPath = new System.Windows.Forms.ComboBox();
            this.form_cbSaveAs = new System.Windows.Forms.ComboBox();
            this.form_btnPath = new System.Windows.Forms.Button();
            this.form_btnSaveAs = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.btnFontClear = new System.Windows.Forms.Button();
            this.form_btnFontDown = new System.Windows.Forms.Button();
            this.form_btnFontUp = new System.Windows.Forms.Button();
            this.form_btnModifyFile = new System.Windows.Forms.Button();
            this.form_btnVolGroup = new System.Windows.Forms.Button();
            this.form_btnLoadVolumeList = new System.Windows.Forms.Button();
            this.form_btnSaveVolumeList = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.form_btnRemoveVolume = new System.Windows.Forms.Button();
            this.form_btnToggleInclude = new System.Windows.Forms.Button();
            this.form_btnAddVolume = new System.Windows.Forms.Button();
            this.form_lvVolumesMain = new ListViewEmbeddedControls.ListViewEx();
            this.form_lv_Volumes_col_Volume = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_Volumes_col_Path = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_Volumes_col_SaveToFile = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_Volumes_col_Status = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_Volumes_col_IncludeInSearch = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader28 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_btnSaveDirList = new System.Windows.Forms.Button();
            this.form_tabPageBrowse = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.form_btnForward = new System.Windows.Forms.Button();
            this.form_btnBack = new System.Windows.Forms.Button();
            this.form_btnUp = new System.Windows.Forms.Button();
            this.form_btnSearchFiles = new System.Windows.Forms.Button();
            this.form_btnSearchFoldersAndFiles = new System.Windows.Forms.Button();
            this.form_btn_TreeCollapse = new System.Windows.Forms.Button();
            this.form_lblVolGroup = new System.Windows.Forms.Label();
            this.form_btnNavigate = new System.Windows.Forms.Button();
            this.form_btnTreeCopy = new System.Windows.Forms.Button();
            this.form_cbNavigate = new System.Windows.Forms.ComboBox();
            this.form_btnCompare = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.form_chkCompare1 = new System.Windows.Forms.CheckBox();
            this.form_splitFiles = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.form_splitTreeFind = new System.Windows.Forms.SplitContainer();
            this.form_splitCompare = new System.Windows.Forms.SplitContainer();
            this.form_treeCompare1 = new SearchDirLists.Form1.Form1TreeView();
            this.form_treeCompare2 = new SearchDirLists.Form1.Form1TreeView();
            this.form_treeView_Browse = new SearchDirLists.Form1.Form1TreeView();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.splitCopyList = new System.Windows.Forms.SplitContainer();
            this.form_btnSaveCopyDirs = new System.Windows.Forms.Button();
            this.form_btnCopyClear = new System.Windows.Forms.Button();
            this.form_btnLoadCopyDirs = new System.Windows.Forms.Button();
            this.form_btnCopyGen = new System.Windows.Forms.Button();
            this.form_lvCopyList = new ListViewEmbeddedControls.ListViewEx();
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.splitIgnoreList = new System.Windows.Forms.SplitContainer();
            this.form_chkLoose = new System.Windows.Forms.CheckBox();
            this.form_btnIgnoreTree = new System.Windows.Forms.Button();
            this.form_btnSaveIgnoreList = new System.Windows.Forms.Button();
            this.form_btnClearIgnoreList = new System.Windows.Forms.Button();
            this.form_btnIgnoreDel = new System.Windows.Forms.Button();
            this.form_btnIgnoreAdd = new System.Windows.Forms.Button();
            this.form_btnLoadIgnoreList = new System.Windows.Forms.Button();
            this.form_lvIgnoreList = new ListViewEmbeddedControls.ListViewEx();
            this.columnHeader11 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader14 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_splitClones = new System.Windows.Forms.SplitContainer();
            this.form_splitDetail = new System.Windows.Forms.SplitContainer();
            this.tabControl_FileList = new System.Windows.Forms.TabControl();
            this.tabPage_DiskUsage = new System.Windows.Forms.TabPage();
            this.form_tmapUserCtl = new SearchDirLists.TreeMapUserControl();
            this.tabPage_FileList = new System.Windows.Forms.TabPage();
            this.form_splitCompareFiles = new System.Windows.Forms.SplitContainer();
            this.form_lvFiles = new ListViewEmbeddedControls.ListViewEx();
            this.form_colFilename = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader10 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lvFileCompare = new ListViewEmbeddedControls.ListViewEx();
            this.form_colFileCompare = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader19 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader20 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader21 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader22 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader23 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader24 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_splitDetailVols = new System.Windows.Forms.SplitContainer();
            this.form_lvDetail = new ListViewEmbeddedControls.ListViewEx();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_colDirDetail = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lvDetailVol = new ListViewEmbeddedControls.ListViewEx();
            this.form_colDirDetailCompare = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_colVolDetail = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_splitNonClones = new System.Windows.Forms.SplitContainer();
            this.form_lvUnique = new ListViewEmbeddedControls.ListViewEx();
            this.columnHeader16 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_splitUnique = new System.Windows.Forms.SplitContainer();
            this.form_lvSameVol = new ListViewEmbeddedControls.ListViewEx();
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader15 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lvClones = new ListViewEmbeddedControls.ListViewEx();
            this.columnHeader12 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader13 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.timer_blink = new System.Windows.Forms.Timer(this.components);
            this.timer_DoTree = new System.Windows.Forms.Timer(this.components);
            this.label_About = new System.Windows.Forms.Label();
            this.form_tabControl.SuspendLayout();
            this.form_tabPageVolumes.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.form_tabPageBrowse.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.form_splitFiles)).BeginInit();
            this.form_splitFiles.Panel1.SuspendLayout();
            this.form_splitFiles.Panel2.SuspendLayout();
            this.form_splitFiles.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.form_splitTreeFind)).BeginInit();
            this.form_splitTreeFind.Panel1.SuspendLayout();
            this.form_splitTreeFind.Panel2.SuspendLayout();
            this.form_splitTreeFind.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.form_splitCompare)).BeginInit();
            this.form_splitCompare.Panel1.SuspendLayout();
            this.form_splitCompare.Panel2.SuspendLayout();
            this.form_splitCompare.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitCopyList)).BeginInit();
            this.splitCopyList.Panel1.SuspendLayout();
            this.splitCopyList.Panel2.SuspendLayout();
            this.splitCopyList.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitIgnoreList)).BeginInit();
            this.splitIgnoreList.Panel1.SuspendLayout();
            this.splitIgnoreList.Panel2.SuspendLayout();
            this.splitIgnoreList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.form_splitClones)).BeginInit();
            this.form_splitClones.Panel1.SuspendLayout();
            this.form_splitClones.Panel2.SuspendLayout();
            this.form_splitClones.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.form_splitDetail)).BeginInit();
            this.form_splitDetail.Panel1.SuspendLayout();
            this.form_splitDetail.Panel2.SuspendLayout();
            this.form_splitDetail.SuspendLayout();
            this.tabControl_FileList.SuspendLayout();
            this.tabPage_DiskUsage.SuspendLayout();
            this.tabPage_FileList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.form_splitCompareFiles)).BeginInit();
            this.form_splitCompareFiles.Panel1.SuspendLayout();
            this.form_splitCompareFiles.Panel2.SuspendLayout();
            this.form_splitCompareFiles.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.form_splitDetailVols)).BeginInit();
            this.form_splitDetailVols.Panel1.SuspendLayout();
            this.form_splitDetailVols.Panel2.SuspendLayout();
            this.form_splitDetailVols.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.form_splitNonClones)).BeginInit();
            this.form_splitNonClones.Panel1.SuspendLayout();
            this.form_splitNonClones.Panel2.SuspendLayout();
            this.form_splitNonClones.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.form_splitUnique)).BeginInit();
            this.form_splitUnique.Panel1.SuspendLayout();
            this.form_splitUnique.Panel2.SuspendLayout();
            this.form_splitUnique.SuspendLayout();
            this.SuspendLayout();
            // 
            // form_tabControl
            // 
            this.form_tabControl.Controls.Add(this.form_tabPageVolumes);
            this.form_tabControl.Controls.Add(this.form_tabPageBrowse);
            this.form_tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_tabControl.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.form_tabControl.Location = new System.Drawing.Point(0, 0);
            this.form_tabControl.Name = "form_tabControl";
            this.form_tabControl.SelectedIndex = 0;
            this.form_tabControl.Size = new System.Drawing.Size(981, 431);
            this.form_tabControl.TabIndex = 0;
            // 
            // form_tabPageVolumes
            // 
            this.form_tabPageVolumes.Controls.Add(this.tableLayoutPanel1);
            this.form_tabPageVolumes.Controls.Add(this.btnFontClear);
            this.form_tabPageVolumes.Controls.Add(this.form_btnFontDown);
            this.form_tabPageVolumes.Controls.Add(this.form_btnFontUp);
            this.form_tabPageVolumes.Controls.Add(this.form_btnModifyFile);
            this.form_tabPageVolumes.Controls.Add(this.form_btnVolGroup);
            this.form_tabPageVolumes.Controls.Add(this.form_btnLoadVolumeList);
            this.form_tabPageVolumes.Controls.Add(this.form_btnSaveVolumeList);
            this.form_tabPageVolumes.Controls.Add(this.label5);
            this.form_tabPageVolumes.Controls.Add(this.form_btnRemoveVolume);
            this.form_tabPageVolumes.Controls.Add(this.form_btnToggleInclude);
            this.form_tabPageVolumes.Controls.Add(this.form_btnAddVolume);
            this.form_tabPageVolumes.Controls.Add(this.form_lvVolumesMain);
            this.form_tabPageVolumes.Controls.Add(this.form_btnSaveDirList);
            this.form_tabPageVolumes.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.form_tabPageVolumes.Location = new System.Drawing.Point(4, 25);
            this.form_tabPageVolumes.Name = "form_tabPageVolumes";
            this.form_tabPageVolumes.Padding = new System.Windows.Forms.Padding(3);
            this.form_tabPageVolumes.Size = new System.Drawing.Size(973, 402);
            this.form_tabPageVolumes.TabIndex = 0;
            this.form_tabPageVolumes.Text = "Volumes";
            this.form_tabPageVolumes.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 35F));
            this.tableLayoutPanel1.Controls.Add(this.form_cbVolumeName, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label4, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label1, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.form_cbPath, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.form_cbSaveAs, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.form_btnPath, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.form_btnSaveAs, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 2);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(6, 6);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(954, 90);
            this.tableLayoutPanel1.TabIndex = 21;
            // 
            // form_cbVolumeName
            // 
            this.form_cbVolumeName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_cbVolumeName.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.form_cbVolumeName.FormattingEnabled = true;
            this.form_cbVolumeName.Location = new System.Drawing.Point(103, 3);
            this.form_cbVolumeName.Name = "form_cbVolumeName";
            this.form_cbVolumeName.Size = new System.Drawing.Size(813, 24);
            this.form_cbVolumeName.TabIndex = 1;
            this.form_cbVolumeName.SelectedIndexChanged += new System.EventHandler(this.form_cb_VolumeName_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(3, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(94, 30);
            this.label4.TabIndex = 0;
            this.label4.Text = "Volume nickname";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Location = new System.Drawing.Point(3, 30);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(94, 30);
            this.label1.TabIndex = 2;
            this.label1.Text = "Path";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // form_cbPath
            // 
            this.form_cbPath.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_cbPath.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.form_cbPath.FormattingEnabled = true;
            this.form_cbPath.Location = new System.Drawing.Point(103, 33);
            this.form_cbPath.Name = "form_cbPath";
            this.form_cbPath.Size = new System.Drawing.Size(813, 24);
            this.form_cbPath.TabIndex = 3;
            this.form_cbPath.SelectedIndexChanged += new System.EventHandler(this.form_cb_Path_SelectedIndexChanged);
            // 
            // form_cbSaveAs
            // 
            this.form_cbSaveAs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_cbSaveAs.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.form_cbSaveAs.FormattingEnabled = true;
            this.form_cbSaveAs.Location = new System.Drawing.Point(103, 63);
            this.form_cbSaveAs.Name = "form_cbSaveAs";
            this.form_cbSaveAs.Size = new System.Drawing.Size(813, 24);
            this.form_cbSaveAs.TabIndex = 7;
            this.form_cbSaveAs.SelectedIndexChanged += new System.EventHandler(this.form_cb_SaveAs_SelectedIndexChanged);
            // 
            // form_btnPath
            // 
            this.form_btnPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.form_btnPath.Location = new System.Drawing.Point(923, 33);
            this.form_btnPath.Name = "form_btnPath";
            this.form_btnPath.Size = new System.Drawing.Size(28, 22);
            this.form_btnPath.TabIndex = 4;
            this.form_btnPath.Text = "...";
            this.form_btnPath.UseVisualStyleBackColor = true;
            this.form_btnPath.Click += new System.EventHandler(this.form_btnPath_Click);
            // 
            // form_btnSaveAs
            // 
            this.form_btnSaveAs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.form_btnSaveAs.Location = new System.Drawing.Point(923, 63);
            this.form_btnSaveAs.Name = "form_btnSaveAs";
            this.form_btnSaveAs.Size = new System.Drawing.Size(28, 23);
            this.form_btnSaveAs.TabIndex = 8;
            this.form_btnSaveAs.Text = "...";
            this.form_btnSaveAs.UseVisualStyleBackColor = true;
            this.form_btnSaveAs.Click += new System.EventHandler(this.form_btnSaveAs_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(3, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 30);
            this.label2.TabIndex = 6;
            this.label2.Text = "Directory listing file";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnFontClear
            // 
            this.btnFontClear.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnFontClear.Location = new System.Drawing.Point(692, 99);
            this.btnFontClear.Name = "btnFontClear";
            this.btnFontClear.Size = new System.Drawing.Size(41, 23);
            this.btnFontClear.TabIndex = 20;
            this.btnFontClear.Text = "Clear";
            this.btnFontClear.UseVisualStyleBackColor = true;
            this.btnFontClear.Click += new System.EventHandler(this.btnFontClear_Click);
            // 
            // form_btnFontDown
            // 
            this.form_btnFontDown.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.form_btnFontDown.Location = new System.Drawing.Point(586, 99);
            this.form_btnFontDown.Name = "form_btnFontDown";
            this.form_btnFontDown.Size = new System.Drawing.Size(47, 23);
            this.form_btnFontDown.TabIndex = 19;
            this.form_btnFontDown.Text = "Font -";
            this.form_btnFontDown.UseVisualStyleBackColor = true;
            this.form_btnFontDown.Click += new System.EventHandler(this.form_btnFontDown_Click);
            // 
            // form_btnFontUp
            // 
            this.form_btnFontUp.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.form_btnFontUp.Location = new System.Drawing.Point(639, 99);
            this.form_btnFontUp.Name = "form_btnFontUp";
            this.form_btnFontUp.Size = new System.Drawing.Size(47, 23);
            this.form_btnFontUp.TabIndex = 18;
            this.form_btnFontUp.Text = "Font +";
            this.form_btnFontUp.UseVisualStyleBackColor = true;
            this.form_btnFontUp.Click += new System.EventHandler(this.form_btnFontUp_Click);
            // 
            // form_btnModifyFile
            // 
            this.form_btnModifyFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.form_btnModifyFile.Enabled = false;
            this.form_btnModifyFile.Location = new System.Drawing.Point(892, 143);
            this.form_btnModifyFile.Name = "form_btnModifyFile";
            this.form_btnModifyFile.Size = new System.Drawing.Size(75, 23);
            this.form_btnModifyFile.TabIndex = 17;
            this.form_btnModifyFile.Text = "Modify file";
            this.form_btnModifyFile.UseVisualStyleBackColor = true;
            this.form_btnModifyFile.Click += new System.EventHandler(this.form_btnModifyFile_Click);
            // 
            // form_btnVolGroup
            // 
            this.form_btnVolGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.form_btnVolGroup.Enabled = false;
            this.form_btnVolGroup.Location = new System.Drawing.Point(793, 143);
            this.form_btnVolGroup.Name = "form_btnVolGroup";
            this.form_btnVolGroup.Size = new System.Drawing.Size(93, 23);
            this.form_btnVolGroup.TabIndex = 14;
            this.form_btnVolGroup.Text = "Volume group";
            this.form_btnVolGroup.UseVisualStyleBackColor = true;
            this.form_btnVolGroup.Click += new System.EventHandler(this.form_btn_VolGroup_Click);
            // 
            // form_btnLoadVolumeList
            // 
            this.form_btnLoadVolumeList.Location = new System.Drawing.Point(121, 143);
            this.form_btnLoadVolumeList.Name = "form_btnLoadVolumeList";
            this.form_btnLoadVolumeList.Size = new System.Drawing.Size(51, 23);
            this.form_btnLoadVolumeList.TabIndex = 11;
            this.form_btnLoadVolumeList.Text = "Load";
            this.form_btnLoadVolumeList.UseVisualStyleBackColor = true;
            this.form_btnLoadVolumeList.Click += new System.EventHandler(this.form_btn_LoadVolumeList_Click);
            // 
            // form_btnSaveVolumeList
            // 
            this.form_btnSaveVolumeList.Enabled = false;
            this.form_btnSaveVolumeList.Location = new System.Drawing.Point(69, 143);
            this.form_btnSaveVolumeList.Name = "form_btnSaveVolumeList";
            this.form_btnSaveVolumeList.Size = new System.Drawing.Size(46, 23);
            this.form_btnSaveVolumeList.TabIndex = 10;
            this.form_btnSaveVolumeList.Text = "Save";
            this.form_btnSaveVolumeList.UseVisualStyleBackColor = true;
            this.form_btnSaveVolumeList.EnabledChanged += new System.EventHandler(this.form_btnSaveDirLists_EnabledChanged);
            this.form_btnSaveVolumeList.Click += new System.EventHandler(this.form_btnSaveVolumeList_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 148);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(57, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Volume list";
            // 
            // form_btnRemoveVolume
            // 
            this.form_btnRemoveVolume.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.form_btnRemoveVolume.Enabled = false;
            this.form_btnRemoveVolume.Location = new System.Drawing.Point(586, 143);
            this.form_btnRemoveVolume.Name = "form_btnRemoveVolume";
            this.form_btnRemoveVolume.Size = new System.Drawing.Size(103, 23);
            this.form_btnRemoveVolume.TabIndex = 15;
            this.form_btnRemoveVolume.Text = "Remove volume";
            this.form_btnRemoveVolume.UseVisualStyleBackColor = true;
            this.form_btnRemoveVolume.Click += new System.EventHandler(this.form_btnRemoveVolume_Click);
            // 
            // form_btnToggleInclude
            // 
            this.form_btnToggleInclude.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.form_btnToggleInclude.Enabled = false;
            this.form_btnToggleInclude.Location = new System.Drawing.Point(695, 143);
            this.form_btnToggleInclude.Name = "form_btnToggleInclude";
            this.form_btnToggleInclude.Size = new System.Drawing.Size(92, 23);
            this.form_btnToggleInclude.TabIndex = 13;
            this.form_btnToggleInclude.Text = "Toggle include";
            this.form_btnToggleInclude.UseVisualStyleBackColor = true;
            this.form_btnToggleInclude.Click += new System.EventHandler(this.form_btn_ToggleInclude_Click);
            // 
            // form_btnAddVolume
            // 
            this.form_btnAddVolume.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.form_btnAddVolume.Location = new System.Drawing.Point(444, 99);
            this.form_btnAddVolume.Name = "form_btnAddVolume";
            this.form_btnAddVolume.Size = new System.Drawing.Size(85, 66);
            this.form_btnAddVolume.TabIndex = 12;
            this.form_btnAddVolume.Text = "Add volume";
            this.form_btnAddVolume.UseVisualStyleBackColor = true;
            this.form_btnAddVolume.Click += new System.EventHandler(this.form_btn_AddVolume_Click);
            // 
            // form_lvVolumesMain
            // 
            this.form_lvVolumesMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.form_lvVolumesMain.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.form_lvVolumesMain.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.form_lv_Volumes_col_Volume,
            this.form_lv_Volumes_col_Path,
            this.form_lv_Volumes_col_SaveToFile,
            this.form_lv_Volumes_col_Status,
            this.form_lv_Volumes_col_IncludeInSearch,
            this.columnHeader28});
            this.form_lvVolumesMain.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.form_lvVolumesMain.FullRowSelect = true;
            this.form_lvVolumesMain.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.form_lvVolumesMain.HideSelection = false;
            this.form_lvVolumesMain.Location = new System.Drawing.Point(0, 172);
            this.form_lvVolumesMain.Name = "form_lvVolumesMain";
            this.form_lvVolumesMain.Size = new System.Drawing.Size(973, 176);
            this.form_lvVolumesMain.TabIndex = 16;
            this.form_lvVolumesMain.UseCompatibleStateImageBehavior = false;
            this.form_lvVolumesMain.View = System.Windows.Forms.View.Details;
            this.form_lvVolumesMain.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.form_lv_Volumes_ItemSelectionChanged);
            // 
            // form_lv_Volumes_col_Volume
            // 
            this.form_lv_Volumes_col_Volume.Text = "Volume name";
            this.form_lv_Volumes_col_Volume.Width = 300;
            // 
            // form_lv_Volumes_col_Path
            // 
            this.form_lv_Volumes_col_Path.Text = "Path";
            this.form_lv_Volumes_col_Path.Width = 200;
            // 
            // form_lv_Volumes_col_SaveToFile
            // 
            this.form_lv_Volumes_col_SaveToFile.Text = "Save to file";
            this.form_lv_Volumes_col_SaveToFile.Width = 400;
            // 
            // form_lv_Volumes_col_Status
            // 
            this.form_lv_Volumes_col_Status.Text = "Status";
            this.form_lv_Volumes_col_Status.Width = 170;
            // 
            // form_lv_Volumes_col_IncludeInSearch
            // 
            this.form_lv_Volumes_col_IncludeInSearch.Text = "Include in search";
            this.form_lv_Volumes_col_IncludeInSearch.Width = 120;
            // 
            // columnHeader28
            // 
            this.columnHeader28.Text = "Volume group";
            this.columnHeader28.Width = 100;
            // 
            // form_btnSaveDirList
            // 
            this.form_btnSaveDirList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.form_btnSaveDirList.Enabled = false;
            this.form_btnSaveDirList.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.form_btnSaveDirList.Location = new System.Drawing.Point(14, 354);
            this.form_btnSaveDirList.Name = "form_btnSaveDirList";
            this.form_btnSaveDirList.Size = new System.Drawing.Size(947, 40);
            this.form_btnSaveDirList.TabIndex = 0;
            this.form_btnSaveDirList.Text = "Save directory listings";
            this.form_btnSaveDirList.UseVisualStyleBackColor = true;
            this.form_btnSaveDirList.EnabledChanged += new System.EventHandler(this.form_btnSaveDirLists_EnabledChanged);
            this.form_btnSaveDirList.Click += new System.EventHandler(this.form_btnSaveDirLists_Click);
            // 
            // form_tabPageBrowse
            // 
            this.form_tabPageBrowse.Controls.Add(this.splitContainer1);
            this.form_tabPageBrowse.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.form_tabPageBrowse.Location = new System.Drawing.Point(4, 25);
            this.form_tabPageBrowse.Name = "form_tabPageBrowse";
            this.form_tabPageBrowse.Size = new System.Drawing.Size(973, 402);
            this.form_tabPageBrowse.TabIndex = 2;
            this.form_tabPageBrowse.Text = "Browse";
            this.form_tabPageBrowse.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.IsSplitterFixed = true;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.form_btnForward);
            this.splitContainer1.Panel1.Controls.Add(this.form_btnBack);
            this.splitContainer1.Panel1.Controls.Add(this.form_btnUp);
            this.splitContainer1.Panel1.Controls.Add(this.form_btnSearchFiles);
            this.splitContainer1.Panel1.Controls.Add(this.form_btnSearchFoldersAndFiles);
            this.splitContainer1.Panel1.Controls.Add(this.form_btn_TreeCollapse);
            this.splitContainer1.Panel1.Controls.Add(this.form_lblVolGroup);
            this.splitContainer1.Panel1.Controls.Add(this.form_btnNavigate);
            this.splitContainer1.Panel1.Controls.Add(this.form_btnTreeCopy);
            this.splitContainer1.Panel1.Controls.Add(this.form_cbNavigate);
            this.splitContainer1.Panel1.Controls.Add(this.form_btnCompare);
            this.splitContainer1.Panel1.Controls.Add(this.label3);
            this.splitContainer1.Panel1.Controls.Add(this.form_chkCompare1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.form_splitFiles);
            this.splitContainer1.Size = new System.Drawing.Size(973, 402);
            this.splitContainer1.SplitterDistance = 29;
            this.splitContainer1.TabIndex = 7;
            // 
            // form_btnForward
            // 
            this.form_btnForward.Location = new System.Drawing.Point(292, 3);
            this.form_btnForward.Name = "form_btnForward";
            this.form_btnForward.Size = new System.Drawing.Size(29, 23);
            this.form_btnForward.TabIndex = 12;
            this.form_btnForward.Text = "-->";
            this.form_btnForward.UseVisualStyleBackColor = true;
            this.form_btnForward.Click += new System.EventHandler(this.form_btnForward_Click);
            // 
            // form_btnBack
            // 
            this.form_btnBack.Location = new System.Drawing.Point(257, 3);
            this.form_btnBack.Name = "form_btnBack";
            this.form_btnBack.Size = new System.Drawing.Size(29, 23);
            this.form_btnBack.TabIndex = 11;
            this.form_btnBack.Text = "<--";
            this.form_btnBack.UseVisualStyleBackColor = true;
            this.form_btnBack.Click += new System.EventHandler(this.form_btnBack_Click);
            // 
            // form_btnUp
            // 
            this.form_btnUp.Location = new System.Drawing.Point(221, 3);
            this.form_btnUp.Name = "form_btnUp";
            this.form_btnUp.Size = new System.Drawing.Size(30, 23);
            this.form_btnUp.TabIndex = 10;
            this.form_btnUp.Text = "Up";
            this.form_btnUp.UseVisualStyleBackColor = true;
            this.form_btnUp.Click += new System.EventHandler(this.form_btnUp_Click);
            // 
            // form_btnSearchFiles
            // 
            this.form_btnSearchFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.form_btnSearchFiles.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.form_btnSearchFiles.Location = new System.Drawing.Point(815, 3);
            this.form_btnSearchFiles.Name = "form_btnSearchFiles";
            this.form_btnSearchFiles.Size = new System.Drawing.Size(41, 23);
            this.form_btnSearchFiles.TabIndex = 8;
            this.form_btnSearchFiles.Text = "Files";
            this.form_btnSearchFiles.UseVisualStyleBackColor = true;
            this.form_btnSearchFiles.Click += new System.EventHandler(this.form_btnSearchFoldersAndFiles_Click);
            // 
            // form_btnSearchFoldersAndFiles
            // 
            this.form_btnSearchFoldersAndFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.form_btnSearchFoldersAndFiles.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.form_btnSearchFoldersAndFiles.Location = new System.Drawing.Point(725, 3);
            this.form_btnSearchFoldersAndFiles.Name = "form_btnSearchFoldersAndFiles";
            this.form_btnSearchFoldersAndFiles.Size = new System.Drawing.Size(84, 23);
            this.form_btnSearchFoldersAndFiles.TabIndex = 7;
            this.form_btnSearchFoldersAndFiles.Text = "Folders && files";
            this.form_btnSearchFoldersAndFiles.UseVisualStyleBackColor = true;
            this.form_btnSearchFoldersAndFiles.Click += new System.EventHandler(this.form_btnSearchFoldersAndFiles_Click);
            // 
            // form_btn_TreeCollapse
            // 
            this.form_btn_TreeCollapse.Location = new System.Drawing.Point(3, 3);
            this.form_btn_TreeCollapse.Name = "form_btn_TreeCollapse";
            this.form_btn_TreeCollapse.Size = new System.Drawing.Size(61, 23);
            this.form_btn_TreeCollapse.TabIndex = 0;
            this.form_btn_TreeCollapse.Text = "Collapse";
            this.form_btn_TreeCollapse.UseVisualStyleBackColor = true;
            this.form_btn_TreeCollapse.Click += new System.EventHandler(this.form_btn_TreeCollapse_Click);
            this.form_btn_TreeCollapse.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CompareModeButtonKeyPress);
            // 
            // form_lblVolGroup
            // 
            this.form_lblVolGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.form_lblVolGroup.AutoSize = true;
            this.form_lblVolGroup.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.form_lblVolGroup.Location = new System.Drawing.Point(862, 8);
            this.form_lblVolGroup.Name = "form_lblVolGroup";
            this.form_lblVolGroup.Size = new System.Drawing.Size(72, 13);
            this.form_lblVolGroup.TabIndex = 6;
            this.form_lblVolGroup.Text = "Volume group";
            this.form_lblVolGroup.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // form_btnNavigate
            // 
            this.form_btnNavigate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.form_btnNavigate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.form_btnNavigate.Location = new System.Drawing.Point(661, 3);
            this.form_btnNavigate.Name = "form_btnNavigate";
            this.form_btnNavigate.Size = new System.Drawing.Size(58, 23);
            this.form_btnNavigate.TabIndex = 4;
            this.form_btnNavigate.Text = "Folders";
            this.form_btnNavigate.UseVisualStyleBackColor = true;
            this.form_btnNavigate.Click += new System.EventHandler(this.form_btnNavigate_Click);
            this.form_btnNavigate.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CompareModeButtonKeyPress);
            // 
            // form_btnTreeCopy
            // 
            this.form_btnTreeCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.form_btnTreeCopy.Location = new System.Drawing.Point(567, 3);
            this.form_btnTreeCopy.Name = "form_btnTreeCopy";
            this.form_btnTreeCopy.Size = new System.Drawing.Size(41, 23);
            this.form_btnTreeCopy.TabIndex = 5;
            this.form_btnTreeCopy.Text = "Copy";
            this.form_btnTreeCopy.UseVisualStyleBackColor = true;
            this.form_btnTreeCopy.Click += new System.EventHandler(this.form_btn_Copy_Click);
            this.form_btnTreeCopy.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CompareModeButtonKeyPress);
            // 
            // form_cbNavigate
            // 
            this.form_cbNavigate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.form_cbNavigate.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.form_cbNavigate.FormattingEnabled = true;
            this.form_cbNavigate.Location = new System.Drawing.Point(327, 3);
            this.form_cbNavigate.Name = "form_cbNavigate";
            this.form_cbNavigate.Size = new System.Drawing.Size(234, 24);
            this.form_cbNavigate.TabIndex = 3;
            this.form_cbNavigate.SelectedIndexChanged += new System.EventHandler(this.form_cbNavigate_SelectedIndexChanged);
            this.form_cbNavigate.TextChanged += new System.EventHandler(this.form_cbNavigate_TextChanged);
            this.form_cbNavigate.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.form_cbNavigate_KeyPress);
            this.form_cbNavigate.MouseUp += new System.Windows.Forms.MouseEventHandler(this.form_cbNavigate_MouseUp);
            // 
            // form_btnCompare
            // 
            this.form_btnCompare.Enabled = false;
            this.form_btnCompare.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.form_btnCompare.Location = new System.Drawing.Point(153, 3);
            this.form_btnCompare.Name = "form_btnCompare";
            this.form_btnCompare.Size = new System.Drawing.Size(62, 23);
            this.form_btnCompare.TabIndex = 2;
            this.form_btnCompare.Text = "Compare";
            this.form_btnCompare.UseVisualStyleBackColor = true;
            this.form_btnCompare.Click += new System.EventHandler(this.form_btn_Compare_Click);
            this.form_btnCompare.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CompareModeButtonKeyPress);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.Color.Transparent;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(614, 8);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(239, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Search                                                                  ";
            // 
            // form_chkCompare1
            // 
            this.form_chkCompare1.AutoSize = true;
            this.form_chkCompare1.BackColor = System.Drawing.Color.Transparent;
            this.form_chkCompare1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Underline, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.form_chkCompare1.Location = new System.Drawing.Point(70, 7);
            this.form_chkCompare1.Name = "form_chkCompare1";
            this.form_chkCompare1.Size = new System.Drawing.Size(140, 17);
            this.form_chkCompare1.TabIndex = 1;
            this.form_chkCompare1.Text = "Compare 1                     ";
            this.form_chkCompare1.UseVisualStyleBackColor = false;
            this.form_chkCompare1.CheckedChanged += new System.EventHandler(this.form_chk_Compare1_CheckedChanged);
            // 
            // form_splitFiles
            // 
            this.form_splitFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_splitFiles.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.form_splitFiles.Location = new System.Drawing.Point(0, 0);
            this.form_splitFiles.Name = "form_splitFiles";
            // 
            // form_splitFiles.Panel1
            // 
            this.form_splitFiles.Panel1.Controls.Add(this.splitContainer2);
            // 
            // form_splitFiles.Panel2
            // 
            this.form_splitFiles.Panel2.Controls.Add(this.form_splitClones);
            this.form_splitFiles.Size = new System.Drawing.Size(973, 369);
            this.form_splitFiles.SplitterDistance = 570;
            this.form_splitFiles.TabIndex = 1;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.form_splitTreeFind);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer2.Size = new System.Drawing.Size(570, 369);
            this.splitContainer2.SplitterDistance = 135;
            this.splitContainer2.TabIndex = 2;
            // 
            // form_splitTreeFind
            // 
            this.form_splitTreeFind.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_splitTreeFind.Location = new System.Drawing.Point(0, 0);
            this.form_splitTreeFind.Name = "form_splitTreeFind";
            this.form_splitTreeFind.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // form_splitTreeFind.Panel1
            // 
            this.form_splitTreeFind.Panel1.Controls.Add(this.form_splitCompare);
            this.form_splitTreeFind.Panel1Collapsed = true;
            // 
            // form_splitTreeFind.Panel2
            // 
            this.form_splitTreeFind.Panel2.Controls.Add(this.form_treeView_Browse);
            this.form_splitTreeFind.Size = new System.Drawing.Size(570, 135);
            this.form_splitTreeFind.SplitterDistance = 109;
            this.form_splitTreeFind.SplitterWidth = 1;
            this.form_splitTreeFind.TabIndex = 1;
            // 
            // form_splitCompare
            // 
            this.form_splitCompare.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_splitCompare.Location = new System.Drawing.Point(0, 0);
            this.form_splitCompare.Name = "form_splitCompare";
            // 
            // form_splitCompare.Panel1
            // 
            this.form_splitCompare.Panel1.Controls.Add(this.form_treeCompare1);
            // 
            // form_splitCompare.Panel2
            // 
            this.form_splitCompare.Panel2.Controls.Add(this.form_treeCompare2);
            this.form_splitCompare.Size = new System.Drawing.Size(150, 109);
            this.form_splitCompare.SplitterDistance = 72;
            this.form_splitCompare.TabIndex = 2;
            // 
            // form_treeCompare1
            // 
            this.form_treeCompare1.CheckBoxes = true;
            this.form_treeCompare1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_treeCompare1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.form_treeCompare1.FullRowSelect = true;
            this.form_treeCompare1.HideSelection = false;
            this.form_treeCompare1.Location = new System.Drawing.Point(0, 0);
            this.form_treeCompare1.Name = "form_treeCompare1";
            this.form_treeCompare1.ShowLines = false;
            this.form_treeCompare1.ShowNodeToolTips = true;
            this.form_treeCompare1.Size = new System.Drawing.Size(72, 109);
            this.form_treeCompare1.TabIndex = 1;
            this.form_treeCompare1.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.form_treeView_Browse_AfterCheck);
            this.form_treeCompare1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.form_treeView_Browse_AfterSelect);
            this.form_treeCompare1.Enter += new System.EventHandler(this.formCtl_EnterForCopyButton);
            this.form_treeCompare1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.form_tree_compare_KeyPress);
            this.form_treeCompare1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.form_treeView_Browse_MouseClick);
            // 
            // form_treeCompare2
            // 
            this.form_treeCompare2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_treeCompare2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.form_treeCompare2.FullRowSelect = true;
            this.form_treeCompare2.HideSelection = false;
            this.form_treeCompare2.Location = new System.Drawing.Point(0, 0);
            this.form_treeCompare2.Name = "form_treeCompare2";
            this.form_treeCompare2.ShowLines = false;
            this.form_treeCompare2.ShowNodeToolTips = true;
            this.form_treeCompare2.Size = new System.Drawing.Size(74, 109);
            this.form_treeCompare2.TabIndex = 2;
            this.form_treeCompare2.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.form_treeView_Browse_AfterCheck);
            this.form_treeCompare2.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.form_treeView_Browse_AfterSelect);
            this.form_treeCompare2.Enter += new System.EventHandler(this.formCtl_EnterForCopyButton);
            this.form_treeCompare2.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.form_tree_compare_KeyPress);
            this.form_treeCompare2.MouseClick += new System.Windows.Forms.MouseEventHandler(this.form_treeView_Browse_MouseClick);
            // 
            // form_treeView_Browse
            // 
            this.form_treeView_Browse.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.form_treeView_Browse.CheckBoxes = true;
            this.form_treeView_Browse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_treeView_Browse.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.form_treeView_Browse.FullRowSelect = true;
            this.form_treeView_Browse.HideSelection = false;
            this.form_treeView_Browse.Location = new System.Drawing.Point(0, 0);
            this.form_treeView_Browse.Name = "form_treeView_Browse";
            this.form_treeView_Browse.ShowLines = false;
            this.form_treeView_Browse.Size = new System.Drawing.Size(570, 135);
            this.form_treeView_Browse.TabIndex = 0;
            this.form_treeView_Browse.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.form_treeView_Browse_AfterCheck);
            this.form_treeView_Browse.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.form_treeView_Browse_AfterSelect);
            this.form_treeView_Browse.Enter += new System.EventHandler(this.formCtl_EnterForCopyButton);
            this.form_treeView_Browse.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.form_treeView_Browse_KeyPress);
            this.form_treeView_Browse.MouseClick += new System.Windows.Forms.MouseEventHandler(this.form_treeView_Browse_MouseClick);
            // 
            // tabControl1
            // 
            this.tabControl1.Alignment = System.Windows.Forms.TabAlignment.Bottom;
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Multiline = true;
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(570, 230);
            this.tabControl1.TabIndex = 3;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.splitCopyList);
            this.tabPage1.Location = new System.Drawing.Point(4, 4);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(562, 204);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Copy list";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // splitCopyList
            // 
            this.splitCopyList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitCopyList.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitCopyList.IsSplitterFixed = true;
            this.splitCopyList.Location = new System.Drawing.Point(3, 3);
            this.splitCopyList.Name = "splitCopyList";
            // 
            // splitCopyList.Panel1
            // 
            this.splitCopyList.Panel1.Controls.Add(this.form_btnSaveCopyDirs);
            this.splitCopyList.Panel1.Controls.Add(this.form_btnCopyClear);
            this.splitCopyList.Panel1.Controls.Add(this.form_btnLoadCopyDirs);
            this.splitCopyList.Panel1.Controls.Add(this.form_btnCopyGen);
            // 
            // splitCopyList.Panel2
            // 
            this.splitCopyList.Panel2.Controls.Add(this.form_lvCopyList);
            this.splitCopyList.Size = new System.Drawing.Size(556, 198);
            this.splitCopyList.SplitterDistance = 51;
            this.splitCopyList.TabIndex = 2;
            // 
            // form_btnSaveCopyDirs
            // 
            this.form_btnSaveCopyDirs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.form_btnSaveCopyDirs.Location = new System.Drawing.Point(3, 143);
            this.form_btnSaveCopyDirs.Name = "form_btnSaveCopyDirs";
            this.form_btnSaveCopyDirs.Size = new System.Drawing.Size(43, 23);
            this.form_btnSaveCopyDirs.TabIndex = 0;
            this.form_btnSaveCopyDirs.Text = "Save";
            this.form_btnSaveCopyDirs.UseVisualStyleBackColor = true;
            this.form_btnSaveCopyDirs.Click += new System.EventHandler(this.form_btnSaveCopyDirs_Click);
            // 
            // form_btnCopyClear
            // 
            this.form_btnCopyClear.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.form_btnCopyClear.Location = new System.Drawing.Point(3, 172);
            this.form_btnCopyClear.Name = "form_btnCopyClear";
            this.form_btnCopyClear.Size = new System.Drawing.Size(43, 23);
            this.form_btnCopyClear.TabIndex = 3;
            this.form_btnCopyClear.Text = "Clear";
            this.form_btnCopyClear.UseVisualStyleBackColor = true;
            this.form_btnCopyClear.Click += new System.EventHandler(this.form_btnCopyClear_Click);
            // 
            // form_btnLoadCopyDirs
            // 
            this.form_btnLoadCopyDirs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.form_btnLoadCopyDirs.Location = new System.Drawing.Point(4, 114);
            this.form_btnLoadCopyDirs.Name = "form_btnLoadCopyDirs";
            this.form_btnLoadCopyDirs.Size = new System.Drawing.Size(42, 23);
            this.form_btnLoadCopyDirs.TabIndex = 1;
            this.form_btnLoadCopyDirs.Text = "Load";
            this.form_btnLoadCopyDirs.UseVisualStyleBackColor = true;
            this.form_btnLoadCopyDirs.Click += new System.EventHandler(this.form_btnLoadCopyDirs_Click);
            // 
            // form_btnCopyGen
            // 
            this.form_btnCopyGen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.form_btnCopyGen.Location = new System.Drawing.Point(4, 85);
            this.form_btnCopyGen.Name = "form_btnCopyGen";
            this.form_btnCopyGen.Size = new System.Drawing.Size(42, 23);
            this.form_btnCopyGen.TabIndex = 2;
            this.form_btnCopyGen.Text = "Script";
            this.form_btnCopyGen.UseVisualStyleBackColor = true;
            // 
            // form_lvCopyList
            // 
            this.form_lvCopyList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.form_lvCopyList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3,
            this.columnHeader4});
            this.form_lvCopyList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_lvCopyList.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.form_lvCopyList.FullRowSelect = true;
            this.form_lvCopyList.HideSelection = false;
            this.form_lvCopyList.Location = new System.Drawing.Point(0, 0);
            this.form_lvCopyList.MultiSelect = false;
            this.form_lvCopyList.Name = "form_lvCopyList";
            this.form_lvCopyList.Size = new System.Drawing.Size(501, 198);
            this.form_lvCopyList.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.form_lvCopyList.TabIndex = 1;
            this.form_lvCopyList.UseCompatibleStateImageBehavior = false;
            this.form_lvCopyList.View = System.Windows.Forms.View.Details;
            this.form_lvCopyList.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.form_lvTreeNodes_ColumnClick);
            this.form_lvCopyList.SelectedIndexChanged += new System.EventHandler(this.SelectedIndexChanged);
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Folders selected for copy";
            this.columnHeader3.Width = 210;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = " ";
            this.columnHeader4.Width = 277;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.splitIgnoreList);
            this.tabPage2.Location = new System.Drawing.Point(4, 4);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(562, 204);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Ignore list";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // splitIgnoreList
            // 
            this.splitIgnoreList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitIgnoreList.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitIgnoreList.IsSplitterFixed = true;
            this.splitIgnoreList.Location = new System.Drawing.Point(3, 3);
            this.splitIgnoreList.Name = "splitIgnoreList";
            // 
            // splitIgnoreList.Panel1
            // 
            this.splitIgnoreList.Panel1.Controls.Add(this.form_chkLoose);
            this.splitIgnoreList.Panel1.Controls.Add(this.form_btnIgnoreTree);
            this.splitIgnoreList.Panel1.Controls.Add(this.form_btnSaveIgnoreList);
            this.splitIgnoreList.Panel1.Controls.Add(this.form_btnClearIgnoreList);
            this.splitIgnoreList.Panel1.Controls.Add(this.form_btnIgnoreDel);
            this.splitIgnoreList.Panel1.Controls.Add(this.form_btnIgnoreAdd);
            this.splitIgnoreList.Panel1.Controls.Add(this.form_btnLoadIgnoreList);
            // 
            // splitIgnoreList.Panel2
            // 
            this.splitIgnoreList.Panel2.Controls.Add(this.form_lvIgnoreList);
            this.splitIgnoreList.Size = new System.Drawing.Size(556, 198);
            this.splitIgnoreList.SplitterDistance = 51;
            this.splitIgnoreList.TabIndex = 3;
            // 
            // form_chkLoose
            // 
            this.form_chkLoose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.form_chkLoose.AutoSize = true;
            this.form_chkLoose.Location = new System.Drawing.Point(4, 4);
            this.form_chkLoose.Name = "form_chkLoose";
            this.form_chkLoose.Size = new System.Drawing.Size(55, 17);
            this.form_chkLoose.TabIndex = 5;
            this.form_chkLoose.Text = "Loose";
            this.form_chkLoose.UseVisualStyleBackColor = true;
            this.form_chkLoose.CheckedChanged += new System.EventHandler(this.form_chkLoose_CheckedChanged);
            // 
            // form_btnIgnoreTree
            // 
            this.form_btnIgnoreTree.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.form_btnIgnoreTree.Location = new System.Drawing.Point(3, 85);
            this.form_btnIgnoreTree.Name = "form_btnIgnoreTree";
            this.form_btnIgnoreTree.Size = new System.Drawing.Size(42, 23);
            this.form_btnIgnoreTree.TabIndex = 4;
            this.form_btnIgnoreTree.Text = "Tree";
            this.form_btnIgnoreTree.UseVisualStyleBackColor = true;
            this.form_btnIgnoreTree.Click += new System.EventHandler(this.form_btnIgnoreTree_Click);
            // 
            // form_btnSaveIgnoreList
            // 
            this.form_btnSaveIgnoreList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.form_btnSaveIgnoreList.Location = new System.Drawing.Point(3, 143);
            this.form_btnSaveIgnoreList.Name = "form_btnSaveIgnoreList";
            this.form_btnSaveIgnoreList.Size = new System.Drawing.Size(43, 23);
            this.form_btnSaveIgnoreList.TabIndex = 0;
            this.form_btnSaveIgnoreList.Text = "Save";
            this.form_btnSaveIgnoreList.UseVisualStyleBackColor = true;
            this.form_btnSaveIgnoreList.Click += new System.EventHandler(this.form_btnSaveIgnoreList_Click);
            // 
            // form_btnClearIgnoreList
            // 
            this.form_btnClearIgnoreList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.form_btnClearIgnoreList.Location = new System.Drawing.Point(3, 172);
            this.form_btnClearIgnoreList.Name = "form_btnClearIgnoreList";
            this.form_btnClearIgnoreList.Size = new System.Drawing.Size(43, 23);
            this.form_btnClearIgnoreList.TabIndex = 3;
            this.form_btnClearIgnoreList.Text = "Clear";
            this.form_btnClearIgnoreList.UseVisualStyleBackColor = true;
            this.form_btnClearIgnoreList.Click += new System.EventHandler(this.form_btnClearIgnoreList_Click);
            // 
            // form_btnIgnoreDel
            // 
            this.form_btnIgnoreDel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.form_btnIgnoreDel.Location = new System.Drawing.Point(3, 56);
            this.form_btnIgnoreDel.Name = "form_btnIgnoreDel";
            this.form_btnIgnoreDel.Size = new System.Drawing.Size(42, 23);
            this.form_btnIgnoreDel.TabIndex = 1;
            this.form_btnIgnoreDel.Text = "Del";
            this.form_btnIgnoreDel.UseVisualStyleBackColor = true;
            this.form_btnIgnoreDel.Click += new System.EventHandler(this.form_btnIgnoreDel_Click);
            // 
            // form_btnIgnoreAdd
            // 
            this.form_btnIgnoreAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.form_btnIgnoreAdd.Location = new System.Drawing.Point(4, 27);
            this.form_btnIgnoreAdd.Name = "form_btnIgnoreAdd";
            this.form_btnIgnoreAdd.Size = new System.Drawing.Size(42, 23);
            this.form_btnIgnoreAdd.TabIndex = 1;
            this.form_btnIgnoreAdd.Text = "Add";
            this.form_btnIgnoreAdd.UseVisualStyleBackColor = true;
            this.form_btnIgnoreAdd.Click += new System.EventHandler(this.form_btnIgnoreAdd_Click);
            // 
            // form_btnLoadIgnoreList
            // 
            this.form_btnLoadIgnoreList.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.form_btnLoadIgnoreList.Location = new System.Drawing.Point(4, 114);
            this.form_btnLoadIgnoreList.Name = "form_btnLoadIgnoreList";
            this.form_btnLoadIgnoreList.Size = new System.Drawing.Size(42, 23);
            this.form_btnLoadIgnoreList.TabIndex = 1;
            this.form_btnLoadIgnoreList.Text = "Load";
            this.form_btnLoadIgnoreList.UseVisualStyleBackColor = true;
            this.form_btnLoadIgnoreList.Click += new System.EventHandler(this.form_btnLoadIgnoreList_Click);
            // 
            // form_lvIgnoreList
            // 
            this.form_lvIgnoreList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.form_lvIgnoreList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader11,
            this.columnHeader14});
            this.form_lvIgnoreList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_lvIgnoreList.FullRowSelect = true;
            this.form_lvIgnoreList.HideSelection = false;
            this.form_lvIgnoreList.Location = new System.Drawing.Point(0, 0);
            this.form_lvIgnoreList.MultiSelect = false;
            this.form_lvIgnoreList.Name = "form_lvIgnoreList";
            this.form_lvIgnoreList.Size = new System.Drawing.Size(501, 198);
            this.form_lvIgnoreList.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.form_lvIgnoreList.TabIndex = 1;
            this.form_lvIgnoreList.UseCompatibleStateImageBehavior = false;
            this.form_lvIgnoreList.View = System.Windows.Forms.View.Details;
            this.form_lvIgnoreList.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.form_lvTreeNodes_ColumnClick);
            // 
            // columnHeader11
            // 
            this.columnHeader11.Text = "Common folders to ignore";
            this.columnHeader11.Width = 369;
            // 
            // columnHeader14
            // 
            this.columnHeader14.Text = "Level";
            this.columnHeader14.Width = 95;
            // 
            // form_splitClones
            // 
            this.form_splitClones.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_splitClones.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.form_splitClones.Location = new System.Drawing.Point(0, 0);
            this.form_splitClones.Name = "form_splitClones";
            // 
            // form_splitClones.Panel1
            // 
            this.form_splitClones.Panel1.Controls.Add(this.form_splitDetail);
            // 
            // form_splitClones.Panel2
            // 
            this.form_splitClones.Panel2.Controls.Add(this.form_splitNonClones);
            this.form_splitClones.Size = new System.Drawing.Size(399, 369);
            this.form_splitClones.SplitterDistance = 138;
            this.form_splitClones.TabIndex = 1;
            // 
            // form_splitDetail
            // 
            this.form_splitDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_splitDetail.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.form_splitDetail.Location = new System.Drawing.Point(0, 0);
            this.form_splitDetail.Name = "form_splitDetail";
            this.form_splitDetail.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // form_splitDetail.Panel1
            // 
            this.form_splitDetail.Panel1.Controls.Add(this.tabControl_FileList);
            // 
            // form_splitDetail.Panel2
            // 
            this.form_splitDetail.Panel2.Controls.Add(this.form_splitDetailVols);
            this.form_splitDetail.Size = new System.Drawing.Size(138, 369);
            this.form_splitDetail.SplitterDistance = 147;
            this.form_splitDetail.TabIndex = 1;
            // 
            // tabControl_FileList
            // 
            this.tabControl_FileList.Controls.Add(this.tabPage_DiskUsage);
            this.tabControl_FileList.Controls.Add(this.tabPage_FileList);
            this.tabControl_FileList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_FileList.Location = new System.Drawing.Point(0, 0);
            this.tabControl_FileList.Name = "tabControl_FileList";
            this.tabControl_FileList.SelectedIndex = 0;
            this.tabControl_FileList.Size = new System.Drawing.Size(138, 147);
            this.tabControl_FileList.TabIndex = 1;
            this.tabControl_FileList.SelectedIndexChanged += new System.EventHandler(this.ClearToolTip);
            // 
            // tabPage_DiskUsage
            // 
            this.tabPage_DiskUsage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.tabPage_DiskUsage.Controls.Add(this.form_tmapUserCtl);
            this.tabPage_DiskUsage.Location = new System.Drawing.Point(4, 22);
            this.tabPage_DiskUsage.Margin = new System.Windows.Forms.Padding(0);
            this.tabPage_DiskUsage.Name = "tabPage_DiskUsage";
            this.tabPage_DiskUsage.Size = new System.Drawing.Size(130, 121);
            this.tabPage_DiskUsage.TabIndex = 1;
            this.tabPage_DiskUsage.Text = "Disk usage";
            this.tabPage_DiskUsage.UseVisualStyleBackColor = true;
            // 
            // form_tmapUserCtl
            // 
            this.form_tmapUserCtl.BackColor = System.Drawing.Color.Turquoise;
            this.form_tmapUserCtl.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.form_tmapUserCtl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_tmapUserCtl.Location = new System.Drawing.Point(0, 0);
            this.form_tmapUserCtl.Name = "form_tmapUserCtl";
            this.form_tmapUserCtl.Size = new System.Drawing.Size(130, 121);
            this.form_tmapUserCtl.TabIndex = 0;
            this.form_tmapUserCtl.Leave += new System.EventHandler(this.form_tmapUserCtl_Leave);
            this.form_tmapUserCtl.MouseUp += new System.Windows.Forms.MouseEventHandler(this.form_tmapUserCtl_MouseUp);
            this.form_tmapUserCtl.Resize += new System.EventHandler(this.ClearToolTip);
            // 
            // tabPage_FileList
            // 
            this.tabPage_FileList.Controls.Add(this.form_splitCompareFiles);
            this.tabPage_FileList.Location = new System.Drawing.Point(4, 22);
            this.tabPage_FileList.Name = "tabPage_FileList";
            this.tabPage_FileList.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_FileList.Size = new System.Drawing.Size(130, 121);
            this.tabPage_FileList.TabIndex = 0;
            this.tabPage_FileList.Text = "Immediate files";
            this.tabPage_FileList.UseVisualStyleBackColor = true;
            // 
            // form_splitCompareFiles
            // 
            this.form_splitCompareFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_splitCompareFiles.Location = new System.Drawing.Point(3, 3);
            this.form_splitCompareFiles.Name = "form_splitCompareFiles";
            // 
            // form_splitCompareFiles.Panel1
            // 
            this.form_splitCompareFiles.Panel1.Controls.Add(this.form_lvFiles);
            // 
            // form_splitCompareFiles.Panel2
            // 
            this.form_splitCompareFiles.Panel2.Controls.Add(this.form_lvFileCompare);
            this.form_splitCompareFiles.Panel2Collapsed = true;
            this.form_splitCompareFiles.Size = new System.Drawing.Size(124, 115);
            this.form_splitCompareFiles.SplitterDistance = 71;
            this.form_splitCompareFiles.TabIndex = 1;
            // 
            // form_lvFiles
            // 
            this.form_lvFiles.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.form_lvFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.form_colFilename,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader8,
            this.columnHeader9,
            this.columnHeader10});
            this.form_lvFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_lvFiles.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.form_lvFiles.FullRowSelect = true;
            this.form_lvFiles.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.form_lvFiles.HideSelection = false;
            this.form_lvFiles.Location = new System.Drawing.Point(0, 0);
            this.form_lvFiles.MultiSelect = false;
            this.form_lvFiles.Name = "form_lvFiles";
            this.form_lvFiles.Size = new System.Drawing.Size(124, 115);
            this.form_lvFiles.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.form_lvFiles.TabIndex = 0;
            this.form_lvFiles.UseCompatibleStateImageBehavior = false;
            this.form_lvFiles.View = System.Windows.Forms.View.Details;
            this.form_lvFiles.Enter += new System.EventHandler(this.formCtl_EnterForCopyButton);
            this.form_lvFiles.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.form_LV_Files_KeyPress);
            // 
            // form_colFilename
            // 
            this.form_colFilename.Text = "Filename";
            this.form_colFilename.Width = 350;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Created";
            this.columnHeader5.Width = 150;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Modified";
            this.columnHeader6.Width = 150;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Attributes";
            this.columnHeader7.Width = 80;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "Length";
            this.columnHeader8.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader8.Width = 80;
            // 
            // columnHeader9
            // 
            this.columnHeader9.Text = "Error 1";
            // 
            // columnHeader10
            // 
            this.columnHeader10.Text = "Error 2";
            // 
            // form_lvFileCompare
            // 
            this.form_lvFileCompare.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.form_colFileCompare,
            this.columnHeader19,
            this.columnHeader20,
            this.columnHeader21,
            this.columnHeader22,
            this.columnHeader23,
            this.columnHeader24});
            this.form_lvFileCompare.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_lvFileCompare.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.form_lvFileCompare.FullRowSelect = true;
            this.form_lvFileCompare.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.form_lvFileCompare.HideSelection = false;
            this.form_lvFileCompare.Location = new System.Drawing.Point(0, 0);
            this.form_lvFileCompare.MultiSelect = false;
            this.form_lvFileCompare.Name = "form_lvFileCompare";
            this.form_lvFileCompare.Size = new System.Drawing.Size(96, 100);
            this.form_lvFileCompare.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.form_lvFileCompare.TabIndex = 1;
            this.form_lvFileCompare.UseCompatibleStateImageBehavior = false;
            this.form_lvFileCompare.View = System.Windows.Forms.View.Details;
            this.form_lvFileCompare.Enter += new System.EventHandler(this.formCtl_EnterForCopyButton);
            this.form_lvFileCompare.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.form_lv_FileCompare_KeyPress);
            // 
            // form_colFileCompare
            // 
            this.form_colFileCompare.Text = "Filename";
            this.form_colFileCompare.Width = 250;
            // 
            // columnHeader19
            // 
            this.columnHeader19.Text = "Created";
            this.columnHeader19.Width = 130;
            // 
            // columnHeader20
            // 
            this.columnHeader20.Text = "Modified";
            this.columnHeader20.Width = 130;
            // 
            // columnHeader21
            // 
            this.columnHeader21.Text = "Attributes";
            // 
            // columnHeader22
            // 
            this.columnHeader22.Text = "Length";
            this.columnHeader22.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader22.Width = 80;
            // 
            // columnHeader23
            // 
            this.columnHeader23.Text = "Error 1";
            // 
            // columnHeader24
            // 
            this.columnHeader24.Text = "Error 2";
            // 
            // form_splitDetailVols
            // 
            this.form_splitDetailVols.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_splitDetailVols.Location = new System.Drawing.Point(0, 0);
            this.form_splitDetailVols.Name = "form_splitDetailVols";
            // 
            // form_splitDetailVols.Panel1
            // 
            this.form_splitDetailVols.Panel1.Controls.Add(this.form_lvDetail);
            // 
            // form_splitDetailVols.Panel2
            // 
            this.form_splitDetailVols.Panel2.Controls.Add(this.form_lvDetailVol);
            this.form_splitDetailVols.Size = new System.Drawing.Size(138, 218);
            this.form_splitDetailVols.SplitterDistance = 66;
            this.form_splitDetailVols.TabIndex = 1;
            // 
            // form_lvDetail
            // 
            this.form_lvDetail.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.form_lvDetail.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.form_colDirDetail});
            this.form_lvDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_lvDetail.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.form_lvDetail.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.form_lvDetail.Location = new System.Drawing.Point(0, 0);
            this.form_lvDetail.MultiSelect = false;
            this.form_lvDetail.Name = "form_lvDetail";
            this.form_lvDetail.Scrollable = false;
            this.form_lvDetail.Size = new System.Drawing.Size(66, 218);
            this.form_lvDetail.TabIndex = 0;
            this.form_lvDetail.UseCompatibleStateImageBehavior = false;
            this.form_lvDetail.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Directory detail";
            this.columnHeader1.Width = 150;
            // 
            // form_colDirDetail
            // 
            this.form_colDirDetail.Text = " ";
            this.form_colDirDetail.Width = 9999;
            // 
            // form_lvDetailVol
            // 
            this.form_lvDetailVol.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.form_lvDetailVol.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.form_colDirDetailCompare,
            this.form_colVolDetail});
            this.form_lvDetailVol.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_lvDetailVol.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.form_lvDetailVol.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.form_lvDetailVol.Location = new System.Drawing.Point(0, 0);
            this.form_lvDetailVol.MultiSelect = false;
            this.form_lvDetailVol.Name = "form_lvDetailVol";
            this.form_lvDetailVol.Scrollable = false;
            this.form_lvDetailVol.Size = new System.Drawing.Size(68, 218);
            this.form_lvDetailVol.TabIndex = 0;
            this.form_lvDetailVol.UseCompatibleStateImageBehavior = false;
            this.form_lvDetailVol.View = System.Windows.Forms.View.Details;
            // 
            // form_colDirDetailCompare
            // 
            this.form_colDirDetailCompare.Text = "Volume detail";
            this.form_colDirDetailCompare.Width = 150;
            // 
            // form_colVolDetail
            // 
            this.form_colVolDetail.Text = " ";
            this.form_colVolDetail.Width = 9999;
            // 
            // form_splitNonClones
            // 
            this.form_splitNonClones.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_splitNonClones.Location = new System.Drawing.Point(0, 0);
            this.form_splitNonClones.Name = "form_splitNonClones";
            this.form_splitNonClones.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // form_splitNonClones.Panel1
            // 
            this.form_splitNonClones.Panel1.Controls.Add(this.form_lvUnique);
            // 
            // form_splitNonClones.Panel2
            // 
            this.form_splitNonClones.Panel2.Controls.Add(this.form_splitUnique);
            this.form_splitNonClones.Size = new System.Drawing.Size(257, 369);
            this.form_splitNonClones.SplitterDistance = 126;
            this.form_splitNonClones.TabIndex = 3;
            // 
            // form_lvUnique
            // 
            this.form_lvUnique.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.form_lvUnique.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader16});
            this.form_lvUnique.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_lvUnique.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.form_lvUnique.FullRowSelect = true;
            this.form_lvUnique.HideSelection = false;
            this.form_lvUnique.LabelWrap = false;
            this.form_lvUnique.Location = new System.Drawing.Point(0, 0);
            this.form_lvUnique.MultiSelect = false;
            this.form_lvUnique.Name = "form_lvUnique";
            this.form_lvUnique.ShowGroups = false;
            this.form_lvUnique.Size = new System.Drawing.Size(257, 126);
            this.form_lvUnique.TabIndex = 0;
            this.form_lvUnique.UseCompatibleStateImageBehavior = false;
            this.form_lvUnique.View = System.Windows.Forms.View.Details;
            this.form_lvUnique.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.form_lvTreeNodes_ColumnClick);
            this.form_lvUnique.SelectedIndexChanged += new System.EventHandler(this.SelectedIndexChanged);
            this.form_lvUnique.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.form_lv_Unique_KeyPress);
            this.form_lvUnique.MouseClick += new System.Windows.Forms.MouseEventHandler(this.form_lv_Unique_MouseClick);
            // 
            // columnHeader16
            // 
            this.columnHeader16.Text = "Solitary";
            this.columnHeader16.Width = 230;
            // 
            // form_splitUnique
            // 
            this.form_splitUnique.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_splitUnique.Location = new System.Drawing.Point(0, 0);
            this.form_splitUnique.Name = "form_splitUnique";
            this.form_splitUnique.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // form_splitUnique.Panel1
            // 
            this.form_splitUnique.Panel1.Controls.Add(this.form_lvSameVol);
            // 
            // form_splitUnique.Panel2
            // 
            this.form_splitUnique.Panel2.Controls.Add(this.form_lvClones);
            this.form_splitUnique.Size = new System.Drawing.Size(257, 239);
            this.form_splitUnique.SplitterDistance = 118;
            this.form_splitUnique.TabIndex = 2;
            // 
            // form_lvSameVol
            // 
            this.form_lvSameVol.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.form_lvSameVol.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2,
            this.columnHeader15});
            this.form_lvSameVol.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_lvSameVol.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.form_lvSameVol.FullRowSelect = true;
            this.form_lvSameVol.HideSelection = false;
            this.form_lvSameVol.LabelWrap = false;
            this.form_lvSameVol.Location = new System.Drawing.Point(0, 0);
            this.form_lvSameVol.MultiSelect = false;
            this.form_lvSameVol.Name = "form_lvSameVol";
            this.form_lvSameVol.ShowGroups = false;
            this.form_lvSameVol.Size = new System.Drawing.Size(257, 118);
            this.form_lvSameVol.TabIndex = 1;
            this.form_lvSameVol.UseCompatibleStateImageBehavior = false;
            this.form_lvSameVol.View = System.Windows.Forms.View.Details;
            this.form_lvSameVol.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.form_lvTreeNodes_ColumnClick);
            this.form_lvSameVol.SelectedIndexChanged += new System.EventHandler(this.form_lvSameVol_SelectedIndexChanged);
            this.form_lvSameVol.MouseClick += new System.Windows.Forms.MouseEventHandler(this.form_lvSameVol_MouseClick);
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Same volume group";
            this.columnHeader2.Width = 180;
            // 
            // columnHeader15
            // 
            this.columnHeader15.Text = " ";
            this.columnHeader15.Width = 50;
            // 
            // form_lvClones
            // 
            this.form_lvClones.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.form_lvClones.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader12,
            this.columnHeader13});
            this.form_lvClones.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_lvClones.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.form_lvClones.FullRowSelect = true;
            this.form_lvClones.HideSelection = false;
            this.form_lvClones.LabelWrap = false;
            this.form_lvClones.Location = new System.Drawing.Point(0, 0);
            this.form_lvClones.MultiSelect = false;
            this.form_lvClones.Name = "form_lvClones";
            this.form_lvClones.ShowGroups = false;
            this.form_lvClones.Size = new System.Drawing.Size(257, 117);
            this.form_lvClones.TabIndex = 0;
            this.form_lvClones.UseCompatibleStateImageBehavior = false;
            this.form_lvClones.View = System.Windows.Forms.View.Details;
            this.form_lvClones.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.form_lvTreeNodes_ColumnClick);
            this.form_lvClones.SelectedIndexChanged += new System.EventHandler(this.form_lvClones_SelectedIndexChanged);
            this.form_lvClones.MouseClick += new System.Windows.Forms.MouseEventHandler(this.form_lvClones_MouseClick);
            // 
            // columnHeader12
            // 
            this.columnHeader12.Text = "Clones";
            this.columnHeader12.Width = 180;
            // 
            // columnHeader13
            // 
            this.columnHeader13.Text = " ";
            this.columnHeader13.Width = 50;
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "txt";
            this.saveFileDialog1.Filter = "Text files|*.txt|All files|*.*";
            this.saveFileDialog1.OverwritePrompt = false;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "Text files|*.txt|All files|*.*";
            // 
            // timer_blink
            // 
            this.timer_blink.Interval = 50;
            this.timer_blink.Tick += new System.EventHandler(this.timer_blink_Tick);
            // 
            // timer_DoTree
            // 
            this.timer_DoTree.Interval = 3000;
            this.timer_DoTree.Tick += new System.EventHandler(this.timer_DoTree_Tick);
            // 
            // label_About
            // 
            this.label_About.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label_About.AutoSize = true;
            this.label_About.Location = new System.Drawing.Point(884, 6);
            this.label_About.Name = "label_About";
            this.label_About.Size = new System.Drawing.Size(81, 13);
            this.label_About.TabIndex = 1;
            this.label_About.Text = "Hit F1 for About";
            this.label_About.Click += new System.EventHandler(this.label_About_Click);
            // 
            // Form1
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(981, 431);
            this.Controls.Add(this.label_About);
            this.Controls.Add(this.form_tabControl);
            this.DoubleBuffered = true;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(746, 420);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SearchDirLists";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Activated += new System.EventHandler(this.ClearToolTip);
            this.Deactivate += new System.EventHandler(this.ClearToolTip);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.Form1_HelpRequested);
            this.Enter += new System.EventHandler(this.ClearToolTip);
            this.Leave += new System.EventHandler(this.ClearToolTip);
            this.form_tabControl.ResumeLayout(false);
            this.form_tabPageVolumes.ResumeLayout(false);
            this.form_tabPageVolumes.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.form_tabPageBrowse.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.form_splitFiles.Panel1.ResumeLayout(false);
            this.form_splitFiles.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.form_splitFiles)).EndInit();
            this.form_splitFiles.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.form_splitTreeFind.Panel1.ResumeLayout(false);
            this.form_splitTreeFind.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.form_splitTreeFind)).EndInit();
            this.form_splitTreeFind.ResumeLayout(false);
            this.form_splitCompare.Panel1.ResumeLayout(false);
            this.form_splitCompare.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.form_splitCompare)).EndInit();
            this.form_splitCompare.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.splitCopyList.Panel1.ResumeLayout(false);
            this.splitCopyList.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitCopyList)).EndInit();
            this.splitCopyList.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.splitIgnoreList.Panel1.ResumeLayout(false);
            this.splitIgnoreList.Panel1.PerformLayout();
            this.splitIgnoreList.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitIgnoreList)).EndInit();
            this.splitIgnoreList.ResumeLayout(false);
            this.form_splitClones.Panel1.ResumeLayout(false);
            this.form_splitClones.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.form_splitClones)).EndInit();
            this.form_splitClones.ResumeLayout(false);
            this.form_splitDetail.Panel1.ResumeLayout(false);
            this.form_splitDetail.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.form_splitDetail)).EndInit();
            this.form_splitDetail.ResumeLayout(false);
            this.tabControl_FileList.ResumeLayout(false);
            this.tabPage_DiskUsage.ResumeLayout(false);
            this.tabPage_FileList.ResumeLayout(false);
            this.form_splitCompareFiles.Panel1.ResumeLayout(false);
            this.form_splitCompareFiles.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.form_splitCompareFiles)).EndInit();
            this.form_splitCompareFiles.ResumeLayout(false);
            this.form_splitDetailVols.Panel1.ResumeLayout(false);
            this.form_splitDetailVols.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.form_splitDetailVols)).EndInit();
            this.form_splitDetailVols.ResumeLayout(false);
            this.form_splitNonClones.Panel1.ResumeLayout(false);
            this.form_splitNonClones.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.form_splitNonClones)).EndInit();
            this.form_splitNonClones.ResumeLayout(false);
            this.form_splitUnique.Panel1.ResumeLayout(false);
            this.form_splitUnique.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.form_splitUnique)).EndInit();
            this.form_splitUnique.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl form_tabControl;
        private System.Windows.Forms.TabPage form_tabPageVolumes;
        private System.Windows.Forms.Button form_btnToggleInclude;
        private System.Windows.Forms.Button form_btnAddVolume;
        private System.Windows.Forms.ComboBox form_cbVolumeName;
        private System.Windows.Forms.Label label4;
        private ListViewEx form_lvVolumesMain;
        private System.Windows.Forms.ColumnHeader form_lv_Volumes_col_Volume;
        private System.Windows.Forms.ColumnHeader form_lv_Volumes_col_Path;
        private System.Windows.Forms.ColumnHeader form_lv_Volumes_col_SaveToFile;
        private System.Windows.Forms.ColumnHeader form_lv_Volumes_col_Status;
        private System.Windows.Forms.ColumnHeader form_lv_Volumes_col_IncludeInSearch;
        private System.Windows.Forms.Button form_btnSaveDirList;
        private System.Windows.Forms.Button form_btnSaveAs;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button form_btnPath;
        private System.Windows.Forms.ComboBox form_cbSaveAs;
        private System.Windows.Forms.ComboBox form_cbPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Button form_btnRemoveVolume;
        private System.Windows.Forms.Button form_btnLoadVolumeList;
        private System.Windows.Forms.Button form_btnSaveVolumeList;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.TabPage form_tabPageBrowse;
        private Form1TreeView form_treeView_Browse;
        private System.Windows.Forms.SplitContainer form_splitFiles;
        private ListViewEx form_lvDetail;
        private System.Windows.Forms.SplitContainer form_splitDetail;
        private ListViewEx form_lvFiles;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader form_colDirDetail;
        private System.Windows.Forms.ColumnHeader form_colFilename;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.ColumnHeader columnHeader10;
        private System.Windows.Forms.SplitContainer form_splitClones;
        private ListViewEx form_lvClones;
        private System.Windows.Forms.ColumnHeader columnHeader12;
        private System.Windows.Forms.ColumnHeader columnHeader13;
        private System.Windows.Forms.SplitContainer form_splitDetailVols;
        private ListViewEx form_lvDetailVol;
        private System.Windows.Forms.ColumnHeader form_colDirDetailCompare;
        private System.Windows.Forms.ColumnHeader form_colVolDetail;
        private System.Windows.Forms.Button form_btnNavigate;
        private System.Windows.Forms.Button form_btn_TreeCollapse;
        private System.Windows.Forms.ComboBox form_cbNavigate;
        private System.Windows.Forms.SplitContainer form_splitUnique;
        private ListViewEx form_lvUnique;
        private System.Windows.Forms.ColumnHeader columnHeader16;
        private System.Windows.Forms.CheckBox form_chkCompare1;
        private System.Windows.Forms.Button form_btnCompare;
        private System.Windows.Forms.Timer timer_blink;
        private System.Windows.Forms.SplitContainer form_splitTreeFind;
        private Form1TreeView form_treeCompare1;
        private System.Windows.Forms.SplitContainer form_splitCompare;
        private Form1TreeView form_treeCompare2;
        private System.Windows.Forms.SplitContainer form_splitCompareFiles;
        private ListViewEx form_lvFileCompare;
        private System.Windows.Forms.ColumnHeader form_colFileCompare;
        private System.Windows.Forms.ColumnHeader columnHeader19;
        private System.Windows.Forms.ColumnHeader columnHeader20;
        private System.Windows.Forms.ColumnHeader columnHeader21;
        private System.Windows.Forms.ColumnHeader columnHeader22;
        private System.Windows.Forms.ColumnHeader columnHeader23;
        private System.Windows.Forms.ColumnHeader columnHeader24;
        private System.Windows.Forms.Button form_btnTreeCopy;
        private System.Windows.Forms.Button form_btnVolGroup;
        private System.Windows.Forms.ColumnHeader columnHeader28;
        private System.Windows.Forms.Timer timer_DoTree;
        private System.Windows.Forms.Label form_lblVolGroup;
        private System.Windows.Forms.SplitContainer form_splitNonClones;
        private ListViewEx form_lvSameVol;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private ListViewEx form_lvCopyList;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.SplitContainer splitCopyList;
        private System.Windows.Forms.Button form_btnLoadCopyDirs;
        private System.Windows.Forms.Button form_btnSaveCopyDirs;
        private System.Windows.Forms.Button form_btnCopyGen;
        private System.Windows.Forms.Button form_btnCopyClear;
        private System.Windows.Forms.Button form_btnSearchFoldersAndFiles;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button form_btnSearchFiles;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.SplitContainer splitIgnoreList;
        private System.Windows.Forms.Button form_btnSaveIgnoreList;
        private System.Windows.Forms.Button form_btnClearIgnoreList;
        private System.Windows.Forms.Button form_btnLoadIgnoreList;
        private ListViewEx form_lvIgnoreList;
        private System.Windows.Forms.ColumnHeader columnHeader11;
        private System.Windows.Forms.ColumnHeader columnHeader14;
        private System.Windows.Forms.Button form_btnModifyFile;
        private System.Windows.Forms.Button form_btnIgnoreAdd;
        private System.Windows.Forms.Button form_btnIgnoreDel;
        private System.Windows.Forms.Button form_btnIgnoreTree;
        private System.Windows.Forms.CheckBox form_chkLoose;
        private System.Windows.Forms.ColumnHeader columnHeader15;
        private System.Windows.Forms.TabControl tabControl_FileList;
        private System.Windows.Forms.TabPage tabPage_FileList;
        private System.Windows.Forms.TabPage tabPage_DiskUsage;
        private TreeMapUserControl form_tmapUserCtl;
        private System.Windows.Forms.Button form_btnUp;
        private System.Windows.Forms.Button form_btnForward;
        private System.Windows.Forms.Button form_btnBack;
        private System.Windows.Forms.Label label_About;
        private System.Windows.Forms.Button form_btnFontDown;
        private System.Windows.Forms.Button form_btnFontUp;
        private System.Windows.Forms.Button btnFontClear;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;

    }
}
