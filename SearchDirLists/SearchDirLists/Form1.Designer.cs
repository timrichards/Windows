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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.form_btnSearch = new System.Windows.Forms.Button();
            this.form_cbSearch = new System.Windows.Forms.ComboBox();
            this.form_tabControl = new System.Windows.Forms.TabControl();
            this.form_tabPageVolumes = new System.Windows.Forms.TabPage();
            this.form_btn_VolGroup = new System.Windows.Forms.Button();
            this.form_btn_LoadVolumeList = new System.Windows.Forms.Button();
            this.form_btn_SaveVolumeList = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.form_btn_RemoveVolume = new System.Windows.Forms.Button();
            this.form_btnToggleInclude = new System.Windows.Forms.Button();
            this.form_btnAddVolume = new System.Windows.Forms.Button();
            this.form_cbVolumeName = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.form_btn_SavePathInfo = new System.Windows.Forms.Button();
            this.form_btn_SaveAs = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.form_btn_Path = new System.Windows.Forms.Button();
            this.form_cb_SaveAs = new System.Windows.Forms.ComboBox();
            this.form_cb_Path = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.form_tabPage_Search = new System.Windows.Forms.TabPage();
            this.form_chk_SearchCase = new System.Windows.Forms.CheckBox();
            this.form_btn_Search_FillPaths = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.form_rad_Folder_Innermost = new System.Windows.Forms.RadioButton();
            this.form_rad_Folder_Outermost = new System.Windows.Forms.RadioButton();
            this.form_rad_Folder_None = new System.Windows.Forms.RadioButton();
            this.form_splitVolumes = new System.Windows.Forms.SplitContainer();
            this.label3 = new System.Windows.Forms.Label();
            this.form_tabPage_Browse = new System.Windows.Forms.TabPage();
            this.form_lbl_VolGroup = new System.Windows.Forms.Label();
            this.form_btn_Copy = new System.Windows.Forms.Button();
            this.form_chk_Compare1 = new System.Windows.Forms.CheckBox();
            this.form_btn_Compare = new System.Windows.Forms.Button();
            this.form_cb_TreeFind = new System.Windows.Forms.ComboBox();
            this.form_btn_TreeCollapse = new System.Windows.Forms.Button();
            this.form_splitFiles = new System.Windows.Forms.SplitContainer();
            this.form_splitTreeFind = new System.Windows.Forms.SplitContainer();
            this.form_splitCompare = new System.Windows.Forms.SplitContainer();
            this.form_treeView_compare1 = new System.Windows.Forms.TreeView();
            this.form_treeView_compare2 = new System.Windows.Forms.TreeView();
            this.form_treeView_Browse = new System.Windows.Forms.TreeView();
            this.form_splitClones = new System.Windows.Forms.SplitContainer();
            this.form_splitDetail = new System.Windows.Forms.SplitContainer();
            this.form_splitCompareFiles = new System.Windows.Forms.SplitContainer();
            this.form_splitDetailVols = new System.Windows.Forms.SplitContainer();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.form_btn_TreeFind = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.timer_killRed = new System.Windows.Forms.Timer(this.components);
            this.timer_blink = new System.Windows.Forms.Timer(this.components);
            this.timer_DoTree = new System.Windows.Forms.Timer(this.components);
            this.form_lvVolumesMain = new ListViewEmbeddedControls.ListViewEx();
            this.form_lv_Volumes_col_Volume = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_Volumes_col_Path = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_Volumes_col_SaveToFile = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_Volumes_col_Status = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_Volumes_col_IncludeInSearch = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader28 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_SearchResults = new ListViewEmbeddedControls.ListViewEx();
            this.columnHeader26 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader27 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_SearchResults_col_Path = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_SearchResults_col_Filename = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_SearchResults_col_Created = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_SearchResults_col_Modified = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader11 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_SearchResults_col_Length = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_SearchResults_col_Error1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_SearchResults_col_Error2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_PathErrors = new ListViewEmbeddedControls.ListViewEx();
            this.form_lv_Errors_col_Volume = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_Errors_col_FileOrPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_Errors_col_Error1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_Errors_col_Error2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_LV_Files = new ListViewEmbeddedControls.ListViewEx();
            this.form_colFilename = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader10 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_FileCompare = new ListViewEmbeddedControls.ListViewEx();
            this.form_colFileCompare = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader19 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader20 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader21 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader22 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader23 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader24 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_LV_Detail = new ListViewEmbeddedControls.ListViewEx();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_colDirDetail = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_LV_DetailVol = new ListViewEmbeddedControls.ListViewEx();
            this.form_colDirDetailCompare = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_colVolDetail = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_Unique = new ListViewEmbeddedControls.ListViewEx();
            this.columnHeader16 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_LV_Clones = new ListViewEmbeddedControls.ListViewEx();
            this.columnHeader12 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader13 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_tabControl.SuspendLayout();
            this.form_tabPageVolumes.SuspendLayout();
            this.form_tabPage_Search.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.form_splitVolumes)).BeginInit();
            this.form_splitVolumes.Panel1.SuspendLayout();
            this.form_splitVolumes.Panel2.SuspendLayout();
            this.form_splitVolumes.SuspendLayout();
            this.form_tabPage_Browse.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.form_splitFiles)).BeginInit();
            this.form_splitFiles.Panel1.SuspendLayout();
            this.form_splitFiles.Panel2.SuspendLayout();
            this.form_splitFiles.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.form_splitTreeFind)).BeginInit();
            this.form_splitTreeFind.Panel1.SuspendLayout();
            this.form_splitTreeFind.Panel2.SuspendLayout();
            this.form_splitTreeFind.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.form_splitCompare)).BeginInit();
            this.form_splitCompare.Panel1.SuspendLayout();
            this.form_splitCompare.Panel2.SuspendLayout();
            this.form_splitCompare.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.form_splitClones)).BeginInit();
            this.form_splitClones.Panel1.SuspendLayout();
            this.form_splitClones.Panel2.SuspendLayout();
            this.form_splitClones.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.form_splitDetail)).BeginInit();
            this.form_splitDetail.Panel1.SuspendLayout();
            this.form_splitDetail.Panel2.SuspendLayout();
            this.form_splitDetail.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.form_splitCompareFiles)).BeginInit();
            this.form_splitCompareFiles.Panel1.SuspendLayout();
            this.form_splitCompareFiles.Panel2.SuspendLayout();
            this.form_splitCompareFiles.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.form_splitDetailVols)).BeginInit();
            this.form_splitDetailVols.Panel1.SuspendLayout();
            this.form_splitDetailVols.Panel2.SuspendLayout();
            this.form_splitDetailVols.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // form_btn_Search
            // 
            this.form_btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.form_btnSearch.Location = new System.Drawing.Point(892, 31);
            this.form_btnSearch.Name = "form_btn_Search";
            this.form_btnSearch.Size = new System.Drawing.Size(75, 23);
            this.form_btnSearch.TabIndex = 7;
            this.form_btnSearch.Text = "Search String";
            this.form_btnSearch.UseVisualStyleBackColor = true;
            this.form_btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // form_cb_Search
            // 
            this.form_cbSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.form_cbSearch.FormattingEnabled = true;
            this.form_cbSearch.Location = new System.Drawing.Point(0, 6);
            this.form_cbSearch.Name = "form_cb_Search";
            this.form_cbSearch.Size = new System.Drawing.Size(973, 21);
            this.form_cbSearch.TabIndex = 0;
            this.form_cbSearch.SelectedIndexChanged += new System.EventHandler(this.cb_Search_SelectedIndexChanged);
            this.form_cbSearch.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.form_cb_Search_KeyPress);
            // 
            // form_tabControl
            // 
            this.form_tabControl.Controls.Add(this.form_tabPageVolumes);
            this.form_tabControl.Controls.Add(this.form_tabPage_Search);
            this.form_tabControl.Controls.Add(this.form_tabPage_Browse);
            this.form_tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_tabControl.Location = new System.Drawing.Point(0, 0);
            this.form_tabControl.Name = "form_tabControl";
            this.form_tabControl.SelectedIndex = 0;
            this.form_tabControl.Size = new System.Drawing.Size(981, 431);
            this.form_tabControl.TabIndex = 0;
            // 
            // form_tabPage_Volumes
            // 
            this.form_tabPageVolumes.Controls.Add(this.form_btn_VolGroup);
            this.form_tabPageVolumes.Controls.Add(this.form_btn_LoadVolumeList);
            this.form_tabPageVolumes.Controls.Add(this.form_btn_SaveVolumeList);
            this.form_tabPageVolumes.Controls.Add(this.label5);
            this.form_tabPageVolumes.Controls.Add(this.form_btn_RemoveVolume);
            this.form_tabPageVolumes.Controls.Add(this.form_btnToggleInclude);
            this.form_tabPageVolumes.Controls.Add(this.form_btnAddVolume);
            this.form_tabPageVolumes.Controls.Add(this.form_cbVolumeName);
            this.form_tabPageVolumes.Controls.Add(this.label4);
            this.form_tabPageVolumes.Controls.Add(this.form_lvVolumesMain);
            this.form_tabPageVolumes.Controls.Add(this.form_btn_SavePathInfo);
            this.form_tabPageVolumes.Controls.Add(this.form_btn_SaveAs);
            this.form_tabPageVolumes.Controls.Add(this.label2);
            this.form_tabPageVolumes.Controls.Add(this.form_btn_Path);
            this.form_tabPageVolumes.Controls.Add(this.form_cb_SaveAs);
            this.form_tabPageVolumes.Controls.Add(this.form_cb_Path);
            this.form_tabPageVolumes.Controls.Add(this.label1);
            this.form_tabPageVolumes.Location = new System.Drawing.Point(4, 22);
            this.form_tabPageVolumes.Name = "form_tabPage_Volumes";
            this.form_tabPageVolumes.Padding = new System.Windows.Forms.Padding(3);
            this.form_tabPageVolumes.Size = new System.Drawing.Size(973, 405);
            this.form_tabPageVolumes.TabIndex = 0;
            this.form_tabPageVolumes.Text = "Volumes";
            this.form_tabPageVolumes.UseVisualStyleBackColor = true;
            // 
            // form_btn_VolGroup
            // 
            this.form_btn_VolGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.form_btn_VolGroup.Enabled = false;
            this.form_btn_VolGroup.Location = new System.Drawing.Point(874, 143);
            this.form_btn_VolGroup.Name = "form_btn_VolGroup";
            this.form_btn_VolGroup.Size = new System.Drawing.Size(93, 23);
            this.form_btn_VolGroup.TabIndex = 14;
            this.form_btn_VolGroup.Text = "Volume Group";
            this.form_btn_VolGroup.UseVisualStyleBackColor = true;
            this.form_btn_VolGroup.Click += new System.EventHandler(this.form_btn_VolGroup_Click);
            // 
            // form_btn_LoadVolumeList
            // 
            this.form_btn_LoadVolumeList.Location = new System.Drawing.Point(121, 143);
            this.form_btn_LoadVolumeList.Name = "form_btn_LoadVolumeList";
            this.form_btn_LoadVolumeList.Size = new System.Drawing.Size(51, 23);
            this.form_btn_LoadVolumeList.TabIndex = 11;
            this.form_btn_LoadVolumeList.Text = "Load";
            this.form_btn_LoadVolumeList.UseVisualStyleBackColor = true;
            this.form_btn_LoadVolumeList.Click += new System.EventHandler(this.form_btn_LoadVolumeList_Click);
            // 
            // form_btn_SaveVolumeList
            // 
            this.form_btn_SaveVolumeList.Enabled = false;
            this.form_btn_SaveVolumeList.Location = new System.Drawing.Point(69, 143);
            this.form_btn_SaveVolumeList.Name = "form_btn_SaveVolumeList";
            this.form_btn_SaveVolumeList.Size = new System.Drawing.Size(46, 23);
            this.form_btn_SaveVolumeList.TabIndex = 10;
            this.form_btn_SaveVolumeList.Text = "Save";
            this.form_btn_SaveVolumeList.UseVisualStyleBackColor = true;
            this.form_btn_SaveVolumeList.Click += new System.EventHandler(this.form_btn_SaveVolumeList_Click);
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
            // form_btn_RemoveVolume
            // 
            this.form_btn_RemoveVolume.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.form_btn_RemoveVolume.Enabled = false;
            this.form_btn_RemoveVolume.Location = new System.Drawing.Point(667, 142);
            this.form_btn_RemoveVolume.Name = "form_btn_RemoveVolume";
            this.form_btn_RemoveVolume.Size = new System.Drawing.Size(103, 23);
            this.form_btn_RemoveVolume.TabIndex = 15;
            this.form_btn_RemoveVolume.Text = "Remove Volume";
            this.form_btn_RemoveVolume.UseVisualStyleBackColor = true;
            this.form_btn_RemoveVolume.Click += new System.EventHandler(this.form_btn_RemoveVolume_Click);
            // 
            // form_btn_ToggleInclude
            // 
            this.form_btnToggleInclude.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.form_btnToggleInclude.Enabled = false;
            this.form_btnToggleInclude.Location = new System.Drawing.Point(776, 142);
            this.form_btnToggleInclude.Name = "form_btn_ToggleInclude";
            this.form_btnToggleInclude.Size = new System.Drawing.Size(92, 23);
            this.form_btnToggleInclude.TabIndex = 13;
            this.form_btnToggleInclude.Text = "Toggle Include";
            this.form_btnToggleInclude.UseVisualStyleBackColor = true;
            this.form_btnToggleInclude.Click += new System.EventHandler(this.form_btn_ToggleInclude_Click);
            // 
            // form_btn_AddVolume
            // 
            this.form_btnAddVolume.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.form_btnAddVolume.Location = new System.Drawing.Point(444, 99);
            this.form_btnAddVolume.Name = "form_btn_AddVolume";
            this.form_btnAddVolume.Size = new System.Drawing.Size(85, 66);
            this.form_btnAddVolume.TabIndex = 12;
            this.form_btnAddVolume.Text = "Add Volume";
            this.form_btnAddVolume.UseVisualStyleBackColor = true;
            this.form_btnAddVolume.Click += new System.EventHandler(this.form_btn_AddVolume_Click);
            // 
            // form_cb_VolumeName
            // 
            this.form_cbVolumeName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.form_cbVolumeName.FormattingEnabled = true;
            this.form_cbVolumeName.Location = new System.Drawing.Point(111, 16);
            this.form_cbVolumeName.Name = "form_cb_VolumeName";
            this.form_cbVolumeName.Size = new System.Drawing.Size(850, 21);
            this.form_cbVolumeName.TabIndex = 1;
            this.form_cbVolumeName.SelectedIndexChanged += new System.EventHandler(this.form_cb_VolumeName_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 19);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(91, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Volume nickname";
            // 
            // form_btn_SavePathInfo
            // 
            this.form_btn_SavePathInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.form_btn_SavePathInfo.Enabled = false;
            this.form_btn_SavePathInfo.Location = new System.Drawing.Point(14, 357);
            this.form_btn_SavePathInfo.Name = "form_btn_SavePathInfo";
            this.form_btn_SavePathInfo.Size = new System.Drawing.Size(947, 40);
            this.form_btn_SavePathInfo.TabIndex = 0;
            this.form_btn_SavePathInfo.Text = "Save Directory Listings";
            this.form_btn_SavePathInfo.UseVisualStyleBackColor = true;
            this.form_btn_SavePathInfo.EnabledChanged += new System.EventHandler(this.form_btn_SavePathInfo_EnabledChanged);
            this.form_btn_SavePathInfo.Click += new System.EventHandler(this.form_btn_SavePathInfo_Click);
            // 
            // form_btn_SaveAs
            // 
            this.form_btn_SaveAs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.form_btn_SaveAs.Location = new System.Drawing.Point(933, 70);
            this.form_btn_SaveAs.Name = "form_btn_SaveAs";
            this.form_btn_SaveAs.Size = new System.Drawing.Size(28, 23);
            this.form_btn_SaveAs.TabIndex = 8;
            this.form_btn_SaveAs.Text = "...";
            this.form_btn_SaveAs.UseVisualStyleBackColor = true;
            this.form_btn_SaveAs.Click += new System.EventHandler(this.form_btn_SaveAs_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 75);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Directory listing file";
            // 
            // form_btn_Path
            // 
            this.form_btn_Path.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.form_btn_Path.Location = new System.Drawing.Point(933, 43);
            this.form_btn_Path.Name = "form_btn_Path";
            this.form_btn_Path.Size = new System.Drawing.Size(28, 23);
            this.form_btn_Path.TabIndex = 4;
            this.form_btn_Path.Text = "...";
            this.form_btn_Path.UseVisualStyleBackColor = true;
            this.form_btn_Path.Click += new System.EventHandler(this.form_btn_Path_Click);
            // 
            // form_cb_SaveAs
            // 
            this.form_cb_SaveAs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.form_cb_SaveAs.FormattingEnabled = true;
            this.form_cb_SaveAs.Location = new System.Drawing.Point(114, 72);
            this.form_cb_SaveAs.Name = "form_cb_SaveAs";
            this.form_cb_SaveAs.Size = new System.Drawing.Size(813, 21);
            this.form_cb_SaveAs.TabIndex = 7;
            // 
            // form_cb_Path
            // 
            this.form_cb_Path.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.form_cb_Path.FormattingEnabled = true;
            this.form_cb_Path.Location = new System.Drawing.Point(49, 43);
            this.form_cb_Path.Name = "form_cb_Path";
            this.form_cb_Path.Size = new System.Drawing.Size(878, 21);
            this.form_cb_Path.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 46);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Path";
            // 
            // form_tabPage_Search
            // 
            this.form_tabPage_Search.Controls.Add(this.form_chk_SearchCase);
            this.form_tabPage_Search.Controls.Add(this.form_btn_Search_FillPaths);
            this.form_tabPage_Search.Controls.Add(this.label6);
            this.form_tabPage_Search.Controls.Add(this.form_rad_Folder_Innermost);
            this.form_tabPage_Search.Controls.Add(this.form_rad_Folder_Outermost);
            this.form_tabPage_Search.Controls.Add(this.form_rad_Folder_None);
            this.form_tabPage_Search.Controls.Add(this.form_splitVolumes);
            this.form_tabPage_Search.Controls.Add(this.form_cbSearch);
            this.form_tabPage_Search.Controls.Add(this.form_btnSearch);
            this.form_tabPage_Search.Location = new System.Drawing.Point(4, 22);
            this.form_tabPage_Search.Name = "form_tabPage_Search";
            this.form_tabPage_Search.Padding = new System.Windows.Forms.Padding(3);
            this.form_tabPage_Search.Size = new System.Drawing.Size(973, 405);
            this.form_tabPage_Search.TabIndex = 1;
            this.form_tabPage_Search.Text = "Search";
            this.form_tabPage_Search.UseVisualStyleBackColor = true;
            // 
            // form_chk_SearchCase
            // 
            this.form_chk_SearchCase.AutoSize = true;
            this.form_chk_SearchCase.Checked = true;
            this.form_chk_SearchCase.CheckState = System.Windows.Forms.CheckState.Checked;
            this.form_chk_SearchCase.Location = new System.Drawing.Point(122, 35);
            this.form_chk_SearchCase.Name = "form_chk_SearchCase";
            this.form_chk_SearchCase.Size = new System.Drawing.Size(96, 17);
            this.form_chk_SearchCase.TabIndex = 2;
            this.form_chk_SearchCase.Text = "Case Sensitive";
            this.form_chk_SearchCase.UseVisualStyleBackColor = true;
            this.form_chk_SearchCase.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.form_cb_Search_KeyPress);
            // 
            // form_btn_Search_FillPaths
            // 
            this.form_btn_Search_FillPaths.Enabled = false;
            this.form_btn_Search_FillPaths.Location = new System.Drawing.Point(8, 31);
            this.form_btn_Search_FillPaths.Name = "form_btn_Search_FillPaths";
            this.form_btn_Search_FillPaths.Size = new System.Drawing.Size(75, 23);
            this.form_btn_Search_FillPaths.TabIndex = 1;
            this.form_btn_Search_FillPaths.Text = "Fill Paths";
            this.form_btn_Search_FillPaths.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(224, 36);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(118, 13);
            this.label6.TabIndex = 3;
            this.label6.Text = "Folder special handling:";
            // 
            // form_rad_Folder_Innermost
            // 
            this.form_rad_Folder_Innermost.AutoSize = true;
            this.form_rad_Folder_Innermost.Location = new System.Drawing.Point(484, 34);
            this.form_rad_Folder_Innermost.Name = "form_rad_Folder_Innermost";
            this.form_rad_Folder_Innermost.Size = new System.Drawing.Size(151, 17);
            this.form_rad_Folder_Innermost.TabIndex = 6;
            this.form_rad_Folder_Innermost.Text = "Innermost that have length";
            this.form_rad_Folder_Innermost.UseVisualStyleBackColor = true;
            this.form_rad_Folder_Innermost.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.form_cb_Search_KeyPress);
            // 
            // form_rad_Folder_Outermost
            // 
            this.form_rad_Folder_Outermost.AutoSize = true;
            this.form_rad_Folder_Outermost.Location = new System.Drawing.Point(405, 34);
            this.form_rad_Folder_Outermost.Name = "form_rad_Folder_Outermost";
            this.form_rad_Folder_Outermost.Size = new System.Drawing.Size(73, 17);
            this.form_rad_Folder_Outermost.TabIndex = 5;
            this.form_rad_Folder_Outermost.Text = "Outermost";
            this.form_rad_Folder_Outermost.UseVisualStyleBackColor = true;
            this.form_rad_Folder_Outermost.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.form_cb_Search_KeyPress);
            // 
            // form_rad_Folder_None
            // 
            this.form_rad_Folder_None.AutoSize = true;
            this.form_rad_Folder_None.Checked = true;
            this.form_rad_Folder_None.Location = new System.Drawing.Point(348, 34);
            this.form_rad_Folder_None.Name = "form_rad_Folder_None";
            this.form_rad_Folder_None.Size = new System.Drawing.Size(51, 17);
            this.form_rad_Folder_None.TabIndex = 4;
            this.form_rad_Folder_None.TabStop = true;
            this.form_rad_Folder_None.Text = "None";
            this.form_rad_Folder_None.UseVisualStyleBackColor = true;
            this.form_rad_Folder_None.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.form_cb_Search_KeyPress);
            // 
            // split_Volumes
            // 
            this.form_splitVolumes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.form_splitVolumes.Location = new System.Drawing.Point(-4, 60);
            this.form_splitVolumes.Name = "split_Volumes";
            this.form_splitVolumes.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // split_Volumes.Panel1
            // 
            this.form_splitVolumes.Panel1.Controls.Add(this.form_lv_SearchResults);
            // 
            // split_Volumes.Panel2
            // 
            this.form_splitVolumes.Panel2.Controls.Add(this.form_lv_PathErrors);
            this.form_splitVolumes.Panel2.Controls.Add(this.label3);
            this.form_splitVolumes.Size = new System.Drawing.Size(981, 349);
            this.form_splitVolumes.SplitterDistance = 260;
            this.form_splitVolumes.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(0, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Path Errors";
            // 
            // form_tabPage_Browse
            // 
            this.form_tabPage_Browse.Controls.Add(this.form_lbl_VolGroup);
            this.form_tabPage_Browse.Controls.Add(this.form_btn_Copy);
            this.form_tabPage_Browse.Controls.Add(this.form_chk_Compare1);
            this.form_tabPage_Browse.Controls.Add(this.form_btn_Compare);
            this.form_tabPage_Browse.Controls.Add(this.form_cb_TreeFind);
            this.form_tabPage_Browse.Controls.Add(this.form_btn_TreeCollapse);
            this.form_tabPage_Browse.Controls.Add(this.form_splitFiles);
            this.form_tabPage_Browse.Controls.Add(this.form_btn_TreeFind);
            this.form_tabPage_Browse.Location = new System.Drawing.Point(4, 22);
            this.form_tabPage_Browse.Name = "form_tabPage_Browse";
            this.form_tabPage_Browse.Size = new System.Drawing.Size(973, 405);
            this.form_tabPage_Browse.TabIndex = 2;
            this.form_tabPage_Browse.Text = "Browse";
            this.form_tabPage_Browse.UseVisualStyleBackColor = true;
            this.form_tabPage_Browse.Paint += new System.Windows.Forms.PaintEventHandler(this.form_tabPage_Browse_Paint);
            // 
            // form_lbl_VolGroup
            // 
            this.form_lbl_VolGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.form_lbl_VolGroup.AutoSize = true;
            this.form_lbl_VolGroup.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.form_lbl_VolGroup.Location = new System.Drawing.Point(831, 8);
            this.form_lbl_VolGroup.Name = "form_lbl_VolGroup";
            this.form_lbl_VolGroup.Size = new System.Drawing.Size(74, 13);
            this.form_lbl_VolGroup.TabIndex = 6;
            this.form_lbl_VolGroup.Text = "Volume Group";
            this.form_lbl_VolGroup.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // form_btn_Copy
            // 
            this.form_btn_Copy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.form_btn_Copy.Location = new System.Drawing.Point(784, 3);
            this.form_btn_Copy.Name = "form_btn_Copy";
            this.form_btn_Copy.Size = new System.Drawing.Size(41, 23);
            this.form_btn_Copy.TabIndex = 5;
            this.form_btn_Copy.Text = "Copy";
            this.form_btn_Copy.UseVisualStyleBackColor = true;
            this.form_btn_Copy.Click += new System.EventHandler(this.form_btn_Copy_Click);
            this.form_btn_Copy.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CompareModeButtonKeyPress);
            // 
            // form_chk_Compare1
            // 
            this.form_chk_Compare1.AutoSize = true;
            this.form_chk_Compare1.Location = new System.Drawing.Point(70, 7);
            this.form_chk_Compare1.Name = "form_chk_Compare1";
            this.form_chk_Compare1.Size = new System.Drawing.Size(77, 17);
            this.form_chk_Compare1.TabIndex = 1;
            this.form_chk_Compare1.Text = "Compare 1";
            this.form_chk_Compare1.UseVisualStyleBackColor = true;
            this.form_chk_Compare1.CheckedChanged += new System.EventHandler(this.form_chk_Compare1_CheckedChanged);
            // 
            // form_btn_Compare
            // 
            this.form_btn_Compare.Enabled = false;
            this.form_btn_Compare.Location = new System.Drawing.Point(153, 3);
            this.form_btn_Compare.Name = "form_btn_Compare";
            this.form_btn_Compare.Size = new System.Drawing.Size(62, 23);
            this.form_btn_Compare.TabIndex = 2;
            this.form_btn_Compare.Text = "Compare";
            this.form_btn_Compare.UseVisualStyleBackColor = true;
            this.form_btn_Compare.Click += new System.EventHandler(this.form_btn_Compare_Click);
            this.form_btn_Compare.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CompareModeButtonKeyPress);
            // 
            // form_cb_TreeFind
            // 
            this.form_cb_TreeFind.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.form_cb_TreeFind.FormattingEnabled = true;
            this.form_cb_TreeFind.Location = new System.Drawing.Point(221, 5);
            this.form_cb_TreeFind.Name = "form_cb_TreeFind";
            this.form_cb_TreeFind.Size = new System.Drawing.Size(493, 21);
            this.form_cb_TreeFind.TabIndex = 3;
            this.form_cb_TreeFind.SelectedIndexChanged += new System.EventHandler(this.form_cb_TreeFind_SelectedIndexChanged);
            this.form_cb_TreeFind.TextChanged += new System.EventHandler(this.form_edit_TreeFind_TextChanged);
            this.form_cb_TreeFind.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.form_edit_TreeFind_KeyPress);
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
            // split_Files
            // 
            this.form_splitFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.form_splitFiles.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.form_splitFiles.Location = new System.Drawing.Point(0, 31);
            this.form_splitFiles.Name = "split_Files";
            // 
            // split_Files.Panel1
            // 
            this.form_splitFiles.Panel1.Controls.Add(this.form_splitTreeFind);
            // 
            // split_Files.Panel2
            // 
            this.form_splitFiles.Panel2.Controls.Add(this.form_splitClones);
            this.form_splitFiles.Size = new System.Drawing.Size(970, 374);
            this.form_splitFiles.SplitterDistance = 570;
            this.form_splitFiles.TabIndex = 1;
            // 
            // split_TreeFind
            // 
            this.form_splitTreeFind.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_splitTreeFind.Location = new System.Drawing.Point(0, 0);
            this.form_splitTreeFind.Name = "split_TreeFind";
            this.form_splitTreeFind.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // split_TreeFind.Panel1
            // 
            this.form_splitTreeFind.Panel1.Controls.Add(this.form_splitCompare);
            this.form_splitTreeFind.Panel1Collapsed = true;
            // 
            // split_TreeFind.Panel2
            // 
            this.form_splitTreeFind.Panel2.Controls.Add(this.form_treeView_Browse);
            this.form_splitTreeFind.Size = new System.Drawing.Size(570, 374);
            this.form_splitTreeFind.SplitterDistance = 200;
            this.form_splitTreeFind.SplitterWidth = 1;
            this.form_splitTreeFind.TabIndex = 1;
            // 
            // split_Compare
            // 
            this.form_splitCompare.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_splitCompare.Location = new System.Drawing.Point(0, 0);
            this.form_splitCompare.Name = "split_Compare";
            // 
            // split_Compare.Panel1
            // 
            this.form_splitCompare.Panel1.Controls.Add(this.form_treeView_compare1);
            // 
            // split_Compare.Panel2
            // 
            this.form_splitCompare.Panel2.Controls.Add(this.form_treeView_compare2);
            this.form_splitCompare.Size = new System.Drawing.Size(150, 200);
            this.form_splitCompare.SplitterDistance = 72;
            this.form_splitCompare.TabIndex = 2;
            // 
            // form_treeView_compare1
            // 
            this.form_treeView_compare1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_treeView_compare1.FullRowSelect = true;
            this.form_treeView_compare1.HideSelection = false;
            this.form_treeView_compare1.Location = new System.Drawing.Point(0, 0);
            this.form_treeView_compare1.Name = "form_treeView_compare1";
            this.form_treeView_compare1.ShowLines = false;
            this.form_treeView_compare1.Size = new System.Drawing.Size(72, 200);
            this.form_treeView_compare1.TabIndex = 1;
            this.form_treeView_compare1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.form_treeView_Browse_AfterSelect);
            this.form_treeView_compare1.Enter += new System.EventHandler(this.formCtl_EnterForCopyButton);
            this.form_treeView_compare1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.form_tree_compare_KeyPress);
            this.form_treeView_compare1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.form_treeView_Browse_MouseClick);
            // 
            // form_treeView_compare2
            // 
            this.form_treeView_compare2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_treeView_compare2.FullRowSelect = true;
            this.form_treeView_compare2.HideSelection = false;
            this.form_treeView_compare2.Location = new System.Drawing.Point(0, 0);
            this.form_treeView_compare2.Name = "form_treeView_compare2";
            this.form_treeView_compare2.ShowLines = false;
            this.form_treeView_compare2.Size = new System.Drawing.Size(74, 200);
            this.form_treeView_compare2.TabIndex = 2;
            this.form_treeView_compare2.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.form_treeView_Browse_AfterSelect);
            this.form_treeView_compare2.Enter += new System.EventHandler(this.formCtl_EnterForCopyButton);
            this.form_treeView_compare2.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.form_tree_compare_KeyPress);
            this.form_treeView_compare2.MouseClick += new System.Windows.Forms.MouseEventHandler(this.form_treeView_Browse_MouseClick);
            // 
            // form_treeView_Browse
            // 
            this.form_treeView_Browse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_treeView_Browse.FullRowSelect = true;
            this.form_treeView_Browse.HideSelection = false;
            this.form_treeView_Browse.Location = new System.Drawing.Point(0, 0);
            this.form_treeView_Browse.Name = "form_treeView_Browse";
            this.form_treeView_Browse.ShowLines = false;
            this.form_treeView_Browse.Size = new System.Drawing.Size(570, 374);
            this.form_treeView_Browse.TabIndex = 0;
            this.form_treeView_Browse.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.form_treeView_Browse_AfterSelect);
            this.form_treeView_Browse.Enter += new System.EventHandler(this.formCtl_EnterForCopyButton);
            this.form_treeView_Browse.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.form_treeView_Browse_KeyPress);
            this.form_treeView_Browse.MouseClick += new System.Windows.Forms.MouseEventHandler(this.form_treeView_Browse_MouseClick);
            // 
            // split_Clones
            // 
            this.form_splitClones.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_splitClones.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.form_splitClones.Location = new System.Drawing.Point(0, 0);
            this.form_splitClones.Name = "split_Clones";
            // 
            // split_Clones.Panel1
            // 
            this.form_splitClones.Panel1.Controls.Add(this.form_splitDetail);
            // 
            // split_Clones.Panel2
            // 
            this.form_splitClones.Panel2.Controls.Add(this.splitContainer1);
            this.form_splitClones.Size = new System.Drawing.Size(396, 374);
            this.form_splitClones.SplitterDistance = 140;
            this.form_splitClones.TabIndex = 1;
            // 
            // split_Detail
            // 
            this.form_splitDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_splitDetail.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.form_splitDetail.Location = new System.Drawing.Point(0, 0);
            this.form_splitDetail.Name = "split_Detail";
            this.form_splitDetail.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // split_Detail.Panel1
            // 
            this.form_splitDetail.Panel1.Controls.Add(this.form_splitCompareFiles);
            // 
            // split_Detail.Panel2
            // 
            this.form_splitDetail.Panel2.Controls.Add(this.form_splitDetailVols);
            this.form_splitDetail.Size = new System.Drawing.Size(140, 374);
            this.form_splitDetail.SplitterDistance = 172;
            this.form_splitDetail.TabIndex = 1;
            // 
            // split_FileCompare
            // 
            this.form_splitCompareFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_splitCompareFiles.Location = new System.Drawing.Point(0, 0);
            this.form_splitCompareFiles.Name = "split_FileCompare";
            // 
            // split_FileCompare.Panel1
            // 
            this.form_splitCompareFiles.Panel1.Controls.Add(this.form_LV_Files);
            // 
            // split_FileCompare.Panel2
            // 
            this.form_splitCompareFiles.Panel2.Controls.Add(this.form_lv_FileCompare);
            this.form_splitCompareFiles.Panel2Collapsed = true;
            this.form_splitCompareFiles.Size = new System.Drawing.Size(140, 172);
            this.form_splitCompareFiles.SplitterDistance = 71;
            this.form_splitCompareFiles.TabIndex = 1;
            // 
            // split_DetailVols
            // 
            this.form_splitDetailVols.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_splitDetailVols.Location = new System.Drawing.Point(0, 0);
            this.form_splitDetailVols.Name = "split_DetailVols";
            // 
            // split_DetailVols.Panel1
            // 
            this.form_splitDetailVols.Panel1.Controls.Add(this.form_LV_Detail);
            // 
            // split_DetailVols.Panel2
            // 
            this.form_splitDetailVols.Panel2.Controls.Add(this.form_LV_DetailVol);
            this.form_splitDetailVols.Size = new System.Drawing.Size(140, 198);
            this.form_splitDetailVols.SplitterDistance = 67;
            this.form_splitDetailVols.TabIndex = 1;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.form_lv_Unique);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.form_LV_Clones);
            this.splitContainer1.Size = new System.Drawing.Size(252, 374);
            this.splitContainer1.SplitterDistance = 187;
            this.splitContainer1.TabIndex = 2;
            // 
            // form_btn_TreeFind
            // 
            this.form_btn_TreeFind.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.form_btn_TreeFind.Location = new System.Drawing.Point(720, 3);
            this.form_btn_TreeFind.Name = "form_btn_TreeFind";
            this.form_btn_TreeFind.Size = new System.Drawing.Size(58, 23);
            this.form_btn_TreeFind.TabIndex = 4;
            this.form_btn_TreeFind.Text = "Navigate";
            this.form_btn_TreeFind.UseVisualStyleBackColor = true;
            this.form_btn_TreeFind.Click += new System.EventHandler(this.form_btn_TreeFind_Click);
            this.form_btn_TreeFind.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CompareModeButtonKeyPress);
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
            // 
            // timer_killRed
            // 
            this.timer_killRed.Interval = 10000;
            this.timer_killRed.Tick += new System.EventHandler(this.timer_killRed_Tick);
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
            // form_LV_VolumesMain
            // 
            this.form_lvVolumesMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.form_lvVolumesMain.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.form_lv_Volumes_col_Volume,
            this.form_lv_Volumes_col_Path,
            this.form_lv_Volumes_col_SaveToFile,
            this.form_lv_Volumes_col_Status,
            this.form_lv_Volumes_col_IncludeInSearch,
            this.columnHeader28});
            this.form_lvVolumesMain.FullRowSelect = true;
            this.form_lvVolumesMain.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.form_lvVolumesMain.HideSelection = false;
            this.form_lvVolumesMain.Location = new System.Drawing.Point(0, 172);
            this.form_lvVolumesMain.MultiSelect = false;
            this.form_lvVolumesMain.Name = "form_LV_VolumesMain";
            this.form_lvVolumesMain.Size = new System.Drawing.Size(973, 179);
            this.form_lvVolumesMain.TabIndex = 16;
            this.form_lvVolumesMain.UseCompatibleStateImageBehavior = false;
            this.form_lvVolumesMain.View = System.Windows.Forms.View.Details;
            this.form_lvVolumesMain.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.form_lv_Volumes_ItemSelectionChanged);
            // 
            // form_lv_Volumes_col_Volume
            // 
            this.form_lv_Volumes_col_Volume.Text = "Volume name";
            this.form_lv_Volumes_col_Volume.Width = 190;
            // 
            // form_lv_Volumes_col_Path
            // 
            this.form_lv_Volumes_col_Path.Text = "Path";
            this.form_lv_Volumes_col_Path.Width = 190;
            // 
            // form_lv_Volumes_col_SaveToFile
            // 
            this.form_lv_Volumes_col_SaveToFile.Text = "Save to file";
            this.form_lv_Volumes_col_SaveToFile.Width = 250;
            // 
            // form_lv_Volumes_col_Status
            // 
            this.form_lv_Volumes_col_Status.Text = "Status";
            this.form_lv_Volumes_col_Status.Width = 130;
            // 
            // form_lv_Volumes_col_IncludeInSearch
            // 
            this.form_lv_Volumes_col_IncludeInSearch.Text = "Include in search";
            this.form_lv_Volumes_col_IncludeInSearch.Width = 95;
            // 
            // columnHeader28
            // 
            this.columnHeader28.Text = "Volume Group";
            this.columnHeader28.Width = 100;
            // 
            // form_lv_SearchResults
            // 
            this.form_lv_SearchResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader26,
            this.columnHeader27,
            this.form_lv_SearchResults_col_Path,
            this.form_lv_SearchResults_col_Filename,
            this.form_lv_SearchResults_col_Created,
            this.form_lv_SearchResults_col_Modified,
            this.columnHeader11,
            this.form_lv_SearchResults_col_Length,
            this.form_lv_SearchResults_col_Error1,
            this.form_lv_SearchResults_col_Error2});
            this.form_lv_SearchResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_lv_SearchResults.FullRowSelect = true;
            this.form_lv_SearchResults.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.form_lv_SearchResults.Location = new System.Drawing.Point(0, 0);
            this.form_lv_SearchResults.MultiSelect = false;
            this.form_lv_SearchResults.Name = "form_lv_SearchResults";
            this.form_lv_SearchResults.Size = new System.Drawing.Size(981, 260);
            this.form_lv_SearchResults.TabIndex = 0;
            this.form_lv_SearchResults.UseCompatibleStateImageBehavior = false;
            this.form_lv_SearchResults.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader26
            // 
            this.columnHeader26.Text = "Line Type";
            this.columnHeader26.Width = 0;
            // 
            // columnHeader27
            // 
            this.columnHeader27.Text = "Line #";
            this.columnHeader27.Width = 0;
            // 
            // form_lv_SearchResults_col_Path
            // 
            this.form_lv_SearchResults_col_Path.Text = "Path";
            // 
            // form_lv_SearchResults_col_Filename
            // 
            this.form_lv_SearchResults_col_Filename.Text = "Filename";
            this.form_lv_SearchResults_col_Filename.Width = 250;
            // 
            // form_lv_SearchResults_col_Created
            // 
            this.form_lv_SearchResults_col_Created.Text = "Created";
            this.form_lv_SearchResults_col_Created.Width = 130;
            // 
            // form_lv_SearchResults_col_Modified
            // 
            this.form_lv_SearchResults_col_Modified.Text = "Modified";
            this.form_lv_SearchResults_col_Modified.Width = 130;
            // 
            // columnHeader11
            // 
            this.columnHeader11.Text = "Attributes";
            // 
            // form_lv_SearchResults_col_Length
            // 
            this.form_lv_SearchResults_col_Length.Text = "Length";
            this.form_lv_SearchResults_col_Length.Width = 80;
            // 
            // form_lv_SearchResults_col_Error1
            // 
            this.form_lv_SearchResults_col_Error1.Text = "Error 1";
            // 
            // form_lv_SearchResults_col_Error2
            // 
            this.form_lv_SearchResults_col_Error2.Text = "Error 2";
            // 
            // form_lv_PathErrors
            // 
            this.form_lv_PathErrors.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.form_lv_PathErrors.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.form_lv_Errors_col_Volume,
            this.form_lv_Errors_col_FileOrPath,
            this.form_lv_Errors_col_Error1,
            this.form_lv_Errors_col_Error2});
            this.form_lv_PathErrors.FullRowSelect = true;
            this.form_lv_PathErrors.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.form_lv_PathErrors.Location = new System.Drawing.Point(0, 16);
            this.form_lv_PathErrors.MultiSelect = false;
            this.form_lv_PathErrors.Name = "form_lv_PathErrors";
            this.form_lv_PathErrors.Size = new System.Drawing.Size(981, 69);
            this.form_lv_PathErrors.TabIndex = 1;
            this.form_lv_PathErrors.UseCompatibleStateImageBehavior = false;
            this.form_lv_PathErrors.View = System.Windows.Forms.View.Details;
            // 
            // form_lv_Errors_col_Volume
            // 
            this.form_lv_Errors_col_Volume.Text = "Volume";
            this.form_lv_Errors_col_Volume.Width = 140;
            // 
            // form_lv_Errors_col_FileOrPath
            // 
            this.form_lv_Errors_col_FileOrPath.Text = "File/Path";
            this.form_lv_Errors_col_FileOrPath.Width = 218;
            // 
            // form_lv_Errors_col_Error1
            // 
            this.form_lv_Errors_col_Error1.Text = "Error 1";
            this.form_lv_Errors_col_Error1.Width = 261;
            // 
            // form_lv_Errors_col_Error2
            // 
            this.form_lv_Errors_col_Error2.Text = "Error 2";
            this.form_lv_Errors_col_Error2.Width = 252;
            // 
            // form_LV_Files
            // 
            this.form_LV_Files.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.form_colFilename,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader8,
            this.columnHeader9,
            this.columnHeader10});
            this.form_LV_Files.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_LV_Files.FullRowSelect = true;
            this.form_LV_Files.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.form_LV_Files.HideSelection = false;
            this.form_LV_Files.Location = new System.Drawing.Point(0, 0);
            this.form_LV_Files.MultiSelect = false;
            this.form_LV_Files.Name = "form_LV_Files";
            this.form_LV_Files.Size = new System.Drawing.Size(140, 172);
            this.form_LV_Files.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.form_LV_Files.TabIndex = 0;
            this.form_LV_Files.UseCompatibleStateImageBehavior = false;
            this.form_LV_Files.View = System.Windows.Forms.View.Details;
            this.form_LV_Files.Enter += new System.EventHandler(this.formCtl_EnterForCopyButton);
            this.form_LV_Files.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.form_LV_Files_KeyPress);
            // 
            // form_col_Filename
            // 
            this.form_colFilename.Text = "Filename";
            this.form_colFilename.Width = 250;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Created";
            this.columnHeader5.Width = 130;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "Modified";
            this.columnHeader6.Width = 130;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "Attributes";
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
            // form_lv_FileCompare
            // 
            this.form_lv_FileCompare.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.form_colFileCompare,
            this.columnHeader19,
            this.columnHeader20,
            this.columnHeader21,
            this.columnHeader22,
            this.columnHeader23,
            this.columnHeader24});
            this.form_lv_FileCompare.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_lv_FileCompare.FullRowSelect = true;
            this.form_lv_FileCompare.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.form_lv_FileCompare.HideSelection = false;
            this.form_lv_FileCompare.Location = new System.Drawing.Point(0, 0);
            this.form_lv_FileCompare.MultiSelect = false;
            this.form_lv_FileCompare.Name = "form_lv_FileCompare";
            this.form_lv_FileCompare.Size = new System.Drawing.Size(96, 100);
            this.form_lv_FileCompare.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.form_lv_FileCompare.TabIndex = 1;
            this.form_lv_FileCompare.UseCompatibleStateImageBehavior = false;
            this.form_lv_FileCompare.View = System.Windows.Forms.View.Details;
            this.form_lv_FileCompare.Enter += new System.EventHandler(this.formCtl_EnterForCopyButton);
            this.form_lv_FileCompare.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.form_lv_FileCompare_KeyPress);
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
            // form_LV_Detail
            // 
            this.form_LV_Detail.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.form_colDirDetail});
            this.form_LV_Detail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_LV_Detail.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.form_LV_Detail.Location = new System.Drawing.Point(0, 0);
            this.form_LV_Detail.MultiSelect = false;
            this.form_LV_Detail.Name = "form_LV_Detail";
            this.form_LV_Detail.Scrollable = false;
            this.form_LV_Detail.Size = new System.Drawing.Size(67, 198);
            this.form_LV_Detail.TabIndex = 0;
            this.form_LV_Detail.UseCompatibleStateImageBehavior = false;
            this.form_LV_Detail.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Directory detail";
            this.columnHeader1.Width = 120;
            // 
            // form_col_DirDetail
            // 
            this.form_colDirDetail.Text = " ";
            this.form_colDirDetail.Width = 9999;
            // 
            // form_LV_DetailVol
            // 
            this.form_LV_DetailVol.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.form_colDirDetailCompare,
            this.form_colVolDetail});
            this.form_LV_DetailVol.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_LV_DetailVol.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.form_LV_DetailVol.Location = new System.Drawing.Point(0, 0);
            this.form_LV_DetailVol.MultiSelect = false;
            this.form_LV_DetailVol.Name = "form_LV_DetailVol";
            this.form_LV_DetailVol.Scrollable = false;
            this.form_LV_DetailVol.Size = new System.Drawing.Size(69, 198);
            this.form_LV_DetailVol.TabIndex = 0;
            this.form_LV_DetailVol.UseCompatibleStateImageBehavior = false;
            this.form_LV_DetailVol.View = System.Windows.Forms.View.Details;
            // 
            // form_colVolDetail
            // 
            this.form_colDirDetailCompare.Text = "Volume detail";
            this.form_colDirDetailCompare.Width = 120;
            // 
            // form_col_DirDetailCompare
            // 
            this.form_colVolDetail.Text = " ";
            this.form_colVolDetail.Width = 9999;
            // 
            // form_lv_Unique
            // 
            this.form_lv_Unique.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader16});
            this.form_lv_Unique.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_lv_Unique.FullRowSelect = true;
            this.form_lv_Unique.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.form_lv_Unique.HideSelection = false;
            this.form_lv_Unique.Location = new System.Drawing.Point(0, 0);
            this.form_lv_Unique.MultiSelect = false;
            this.form_lv_Unique.Name = "form_lv_Unique";
            this.form_lv_Unique.Size = new System.Drawing.Size(252, 187);
            this.form_lv_Unique.TabIndex = 0;
            this.form_lv_Unique.UseCompatibleStateImageBehavior = false;
            this.form_lv_Unique.View = System.Windows.Forms.View.Details;
            this.form_lv_Unique.SelectedIndexChanged += new System.EventHandler(this.form_lv_Unique_SelectedIndexChanged);
            this.form_lv_Unique.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.form_lv_Unique_KeyPress);
            this.form_lv_Unique.MouseClick += new System.Windows.Forms.MouseEventHandler(this.form_lv_Unique_MouseClick);
            // 
            // columnHeader16
            // 
            this.columnHeader16.Text = "Solitary";
            this.columnHeader16.Width = 230;
            // 
            // form_LV_Clones
            // 
            this.form_LV_Clones.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader12,
            this.columnHeader13});
            this.form_LV_Clones.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_LV_Clones.FullRowSelect = true;
            this.form_LV_Clones.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.form_LV_Clones.HideSelection = false;
            this.form_LV_Clones.Location = new System.Drawing.Point(0, 0);
            this.form_LV_Clones.MultiSelect = false;
            this.form_LV_Clones.Name = "form_LV_Clones";
            this.form_LV_Clones.Size = new System.Drawing.Size(252, 183);
            this.form_LV_Clones.TabIndex = 0;
            this.form_LV_Clones.UseCompatibleStateImageBehavior = false;
            this.form_LV_Clones.View = System.Windows.Forms.View.Details;
            this.form_LV_Clones.SelectedIndexChanged += new System.EventHandler(this.form_lvClones_SelectedIndexChanged);
            this.form_LV_Clones.MouseClick += new System.Windows.Forms.MouseEventHandler(this.form_lvClones_MouseClick);
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
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(981, 431);
            this.Controls.Add(this.form_tabControl);
            this.MinimumSize = new System.Drawing.Size(746, 420);
            this.Name = "Form1";
            this.Text = "SearchDirLists";
            this.form_tabControl.ResumeLayout(false);
            this.form_tabPageVolumes.ResumeLayout(false);
            this.form_tabPageVolumes.PerformLayout();
            this.form_tabPage_Search.ResumeLayout(false);
            this.form_tabPage_Search.PerformLayout();
            this.form_splitVolumes.Panel1.ResumeLayout(false);
            this.form_splitVolumes.Panel2.ResumeLayout(false);
            this.form_splitVolumes.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.form_splitVolumes)).EndInit();
            this.form_splitVolumes.ResumeLayout(false);
            this.form_tabPage_Browse.ResumeLayout(false);
            this.form_tabPage_Browse.PerformLayout();
            this.form_splitFiles.Panel1.ResumeLayout(false);
            this.form_splitFiles.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.form_splitFiles)).EndInit();
            this.form_splitFiles.ResumeLayout(false);
            this.form_splitTreeFind.Panel1.ResumeLayout(false);
            this.form_splitTreeFind.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.form_splitTreeFind)).EndInit();
            this.form_splitTreeFind.ResumeLayout(false);
            this.form_splitCompare.Panel1.ResumeLayout(false);
            this.form_splitCompare.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.form_splitCompare)).EndInit();
            this.form_splitCompare.ResumeLayout(false);
            this.form_splitClones.Panel1.ResumeLayout(false);
            this.form_splitClones.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.form_splitClones)).EndInit();
            this.form_splitClones.ResumeLayout(false);
            this.form_splitDetail.Panel1.ResumeLayout(false);
            this.form_splitDetail.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.form_splitDetail)).EndInit();
            this.form_splitDetail.ResumeLayout(false);
            this.form_splitCompareFiles.Panel1.ResumeLayout(false);
            this.form_splitCompareFiles.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.form_splitCompareFiles)).EndInit();
            this.form_splitCompareFiles.ResumeLayout(false);
            this.form_splitDetailVols.Panel1.ResumeLayout(false);
            this.form_splitDetailVols.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.form_splitDetailVols)).EndInit();
            this.form_splitDetailVols.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button form_btnSearch;
        private System.Windows.Forms.ComboBox form_cbSearch;
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
        private System.Windows.Forms.Button form_btn_SavePathInfo;
        private System.Windows.Forms.Button form_btn_SaveAs;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button form_btn_Path;
        private System.Windows.Forms.ComboBox form_cb_SaveAs;
        private System.Windows.Forms.ComboBox form_cb_Path;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage form_tabPage_Search;
        private System.Windows.Forms.SplitContainer form_splitVolumes;
        private ListViewEx form_lv_SearchResults;
        private System.Windows.Forms.ColumnHeader form_lv_SearchResults_col_Filename;
        private System.Windows.Forms.ColumnHeader form_lv_SearchResults_col_Length;
        private System.Windows.Forms.ColumnHeader form_lv_SearchResults_col_Modified;
        private System.Windows.Forms.ColumnHeader form_lv_SearchResults_col_Created;
        private ListViewEx form_lv_PathErrors;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.ColumnHeader form_lv_SearchResults_col_Path;
        private System.Windows.Forms.ColumnHeader form_lv_Errors_col_Volume;
        private System.Windows.Forms.ColumnHeader form_lv_Errors_col_FileOrPath;
        private System.Windows.Forms.ColumnHeader form_lv_Errors_col_Error1;
        private System.Windows.Forms.ColumnHeader form_lv_Errors_col_Error2;
        private System.Windows.Forms.Button form_btn_RemoveVolume;
        private System.Windows.Forms.Button form_btn_LoadVolumeList;
        private System.Windows.Forms.Button form_btn_SaveVolumeList;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ColumnHeader form_lv_SearchResults_col_Error1;
        private System.Windows.Forms.ColumnHeader form_lv_SearchResults_col_Error2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.RadioButton form_rad_Folder_Innermost;
        private System.Windows.Forms.RadioButton form_rad_Folder_Outermost;
        private System.Windows.Forms.RadioButton form_rad_Folder_None;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.TabPage form_tabPage_Browse;
        private System.Windows.Forms.TreeView form_treeView_Browse;
        private System.Windows.Forms.Timer timer_killRed;
        private System.Windows.Forms.SplitContainer form_splitFiles;
        private ListViewEx form_LV_Detail;
        private System.Windows.Forms.SplitContainer form_splitDetail;
        private ListViewEx form_LV_Files;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader form_colDirDetail;
        private System.Windows.Forms.ColumnHeader form_colFilename;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.ColumnHeader columnHeader10;
        private System.Windows.Forms.ColumnHeader columnHeader11;
        private System.Windows.Forms.Button form_btn_Search_FillPaths;
        private System.Windows.Forms.CheckBox form_chk_SearchCase;
        private System.Windows.Forms.SplitContainer form_splitClones;
        private ListViewEx form_LV_Clones;
        private System.Windows.Forms.ColumnHeader columnHeader12;
        private System.Windows.Forms.ColumnHeader columnHeader13;
        private System.Windows.Forms.SplitContainer form_splitDetailVols;
        private ListViewEx form_LV_DetailVol;
        private System.Windows.Forms.ColumnHeader form_colDirDetailCompare;
        private System.Windows.Forms.ColumnHeader form_colVolDetail;
        private System.Windows.Forms.Button form_btn_TreeFind;
        private System.Windows.Forms.Button form_btn_TreeCollapse;
        private System.Windows.Forms.ComboBox form_cb_TreeFind;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private ListViewEx form_lv_Unique;
        private System.Windows.Forms.ColumnHeader columnHeader16;
        private System.Windows.Forms.CheckBox form_chk_Compare1;
        private System.Windows.Forms.Button form_btn_Compare;
        private System.Windows.Forms.Timer timer_blink;
        private System.Windows.Forms.SplitContainer form_splitTreeFind;
        private System.Windows.Forms.TreeView form_treeView_compare1;
        private System.Windows.Forms.SplitContainer form_splitCompare;
        private System.Windows.Forms.TreeView form_treeView_compare2;
        private System.Windows.Forms.SplitContainer form_splitCompareFiles;
        private ListViewEx form_lv_FileCompare;
        private System.Windows.Forms.ColumnHeader form_colFileCompare;
        private System.Windows.Forms.ColumnHeader columnHeader19;
        private System.Windows.Forms.ColumnHeader columnHeader20;
        private System.Windows.Forms.ColumnHeader columnHeader21;
        private System.Windows.Forms.ColumnHeader columnHeader22;
        private System.Windows.Forms.ColumnHeader columnHeader23;
        private System.Windows.Forms.ColumnHeader columnHeader24;
        private System.Windows.Forms.ColumnHeader columnHeader26;
        private System.Windows.Forms.ColumnHeader columnHeader27;
        private System.Windows.Forms.Button form_btn_Copy;
        private System.Windows.Forms.Button form_btn_VolGroup;
        private System.Windows.Forms.ColumnHeader columnHeader28;
        private System.Windows.Forms.Timer timer_DoTree;
        private System.Windows.Forms.Label form_lbl_VolGroup;

    }
}

