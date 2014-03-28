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
            this.form_btnVolGroup = new System.Windows.Forms.Button();
            this.form_btn_LoadVolumeList = new System.Windows.Forms.Button();
            this.form_btn_SaveVolumeList = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.form_btn_RemoveVolume = new System.Windows.Forms.Button();
            this.form_btnToggleInclude = new System.Windows.Forms.Button();
            this.form_btnAddVolume = new System.Windows.Forms.Button();
            this.form_cbVolumeName = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.form_lvVolumesMain = new ListViewEmbeddedControls.ListViewEx();
            this.form_lv_Volumes_col_Volume = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_Volumes_col_Path = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_Volumes_col_SaveToFile = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_Volumes_col_Status = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_Volumes_col_IncludeInSearch = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader28 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_btnSavePathInfo = new System.Windows.Forms.Button();
            this.form_btnSaveAs = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.form_btnPath = new System.Windows.Forms.Button();
            this.form_cbSaveAs = new System.Windows.Forms.ComboBox();
            this.form_cbPath = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.form_tabPageSearch = new System.Windows.Forms.TabPage();
            this.form_btnSearchCopy = new System.Windows.Forms.Button();
            this.form_chkSearchCase = new System.Windows.Forms.CheckBox();
            this.form_btnSearchFillPaths = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.form_radSearchDirHandlingInnermost = new System.Windows.Forms.RadioButton();
            this.form_radSearchDirHandlingOutermost = new System.Windows.Forms.RadioButton();
            this.form_radSearchDirHandlingNone = new System.Windows.Forms.RadioButton();
            this.form_splitVolumes = new System.Windows.Forms.SplitContainer();
            this.form_lvSearchResults = new ListViewEmbeddedControls.ListViewEx();
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
            this.form_lvPathErrors = new ListViewEmbeddedControls.ListViewEx();
            this.form_lv_Errors_col_Volume = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_Errors_col_FileOrPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_Errors_col_Error1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_Errors_col_Error2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label3 = new System.Windows.Forms.Label();
            this.form_tabPage_Browse = new System.Windows.Forms.TabPage();
            this.form_lblVolGroup = new System.Windows.Forms.Label();
            this.form_btnTreeCopy = new System.Windows.Forms.Button();
            this.form_chkCompare1 = new System.Windows.Forms.CheckBox();
            this.form_btnCompare = new System.Windows.Forms.Button();
            this.form_cb_TreeFind = new System.Windows.Forms.ComboBox();
            this.form_btn_TreeCollapse = new System.Windows.Forms.Button();
            this.form_splitFiles = new System.Windows.Forms.SplitContainer();
            this.form_splitTreeFind = new System.Windows.Forms.SplitContainer();
            this.form_splitCompare = new System.Windows.Forms.SplitContainer();
            this.form_treeCompare1 = new System.Windows.Forms.TreeView();
            this.form_treeCompare2 = new System.Windows.Forms.TreeView();
            this.form_treeView_Browse = new System.Windows.Forms.TreeView();
            this.form_splitClones = new System.Windows.Forms.SplitContainer();
            this.form_splitDetail = new System.Windows.Forms.SplitContainer();
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
            this.form_splitUnique = new System.Windows.Forms.SplitContainer();
            this.form_lvUnique = new ListViewEmbeddedControls.ListViewEx();
            this.columnHeader16 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lvClones = new ListViewEmbeddedControls.ListViewEx();
            this.columnHeader12 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader13 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_btn_TreeFind = new System.Windows.Forms.Button();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.timer_killRed = new System.Windows.Forms.Timer(this.components);
            this.timer_blink = new System.Windows.Forms.Timer(this.components);
            this.timer_DoTree = new System.Windows.Forms.Timer(this.components);
            this.form_tabControl.SuspendLayout();
            this.form_tabPageVolumes.SuspendLayout();
            this.form_tabPageSearch.SuspendLayout();
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
            ((System.ComponentModel.ISupportInitialize)(this.form_splitUnique)).BeginInit();
            this.form_splitUnique.Panel1.SuspendLayout();
            this.form_splitUnique.Panel2.SuspendLayout();
            this.form_splitUnique.SuspendLayout();
            this.SuspendLayout();
            // 
            // form_btnSearch
            // 
            this.form_btnSearch.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.form_btnSearch.Location = new System.Drawing.Point(892, 31);
            this.form_btnSearch.Name = "form_btnSearch";
            this.form_btnSearch.Size = new System.Drawing.Size(75, 23);
            this.form_btnSearch.TabIndex = 7;
            this.form_btnSearch.Text = "Search String";
            this.form_btnSearch.UseVisualStyleBackColor = true;
            this.form_btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // form_cbSearch
            // 
            this.form_cbSearch.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.form_cbSearch.FormattingEnabled = true;
            this.form_cbSearch.Location = new System.Drawing.Point(0, 6);
            this.form_cbSearch.Name = "form_cbSearch";
            this.form_cbSearch.Size = new System.Drawing.Size(920, 21);
            this.form_cbSearch.TabIndex = 0;
            this.form_cbSearch.SelectedIndexChanged += new System.EventHandler(this.cb_Search_SelectedIndexChanged);
            this.form_cbSearch.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.form_cb_Search_KeyPress);
            // 
            // form_tabControl
            // 
            this.form_tabControl.Controls.Add(this.form_tabPageVolumes);
            this.form_tabControl.Controls.Add(this.form_tabPageSearch);
            this.form_tabControl.Controls.Add(this.form_tabPage_Browse);
            this.form_tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_tabControl.Location = new System.Drawing.Point(0, 0);
            this.form_tabControl.Name = "form_tabControl";
            this.form_tabControl.SelectedIndex = 0;
            this.form_tabControl.Size = new System.Drawing.Size(981, 431);
            this.form_tabControl.TabIndex = 0;
            // 
            // form_tabPageVolumes
            // 
            this.form_tabPageVolumes.Controls.Add(this.form_btnVolGroup);
            this.form_tabPageVolumes.Controls.Add(this.form_btn_LoadVolumeList);
            this.form_tabPageVolumes.Controls.Add(this.form_btn_SaveVolumeList);
            this.form_tabPageVolumes.Controls.Add(this.label5);
            this.form_tabPageVolumes.Controls.Add(this.form_btn_RemoveVolume);
            this.form_tabPageVolumes.Controls.Add(this.form_btnToggleInclude);
            this.form_tabPageVolumes.Controls.Add(this.form_btnAddVolume);
            this.form_tabPageVolumes.Controls.Add(this.form_cbVolumeName);
            this.form_tabPageVolumes.Controls.Add(this.label4);
            this.form_tabPageVolumes.Controls.Add(this.form_lvVolumesMain);
            this.form_tabPageVolumes.Controls.Add(this.form_btnSavePathInfo);
            this.form_tabPageVolumes.Controls.Add(this.form_btnSaveAs);
            this.form_tabPageVolumes.Controls.Add(this.label2);
            this.form_tabPageVolumes.Controls.Add(this.form_btnPath);
            this.form_tabPageVolumes.Controls.Add(this.form_cbSaveAs);
            this.form_tabPageVolumes.Controls.Add(this.form_cbPath);
            this.form_tabPageVolumes.Controls.Add(this.label1);
            this.form_tabPageVolumes.Location = new System.Drawing.Point(4, 22);
            this.form_tabPageVolumes.Name = "form_tabPageVolumes";
            this.form_tabPageVolumes.Padding = new System.Windows.Forms.Padding(3);
            this.form_tabPageVolumes.Size = new System.Drawing.Size(973, 405);
            this.form_tabPageVolumes.TabIndex = 0;
            this.form_tabPageVolumes.Text = "Volumes";
            this.form_tabPageVolumes.UseVisualStyleBackColor = true;
            // 
            // form_btnVolGroup
            // 
            this.form_btnVolGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.form_btnVolGroup.Enabled = false;
            this.form_btnVolGroup.Location = new System.Drawing.Point(874, 143);
            this.form_btnVolGroup.Name = "form_btnVolGroup";
            this.form_btnVolGroup.Size = new System.Drawing.Size(93, 23);
            this.form_btnVolGroup.TabIndex = 14;
            this.form_btnVolGroup.Text = "Volume Group";
            this.form_btnVolGroup.UseVisualStyleBackColor = true;
            this.form_btnVolGroup.Click += new System.EventHandler(this.form_btn_VolGroup_Click);
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
            // form_btnToggleInclude
            // 
            this.form_btnToggleInclude.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.form_btnToggleInclude.Enabled = false;
            this.form_btnToggleInclude.Location = new System.Drawing.Point(776, 142);
            this.form_btnToggleInclude.Name = "form_btnToggleInclude";
            this.form_btnToggleInclude.Size = new System.Drawing.Size(92, 23);
            this.form_btnToggleInclude.TabIndex = 13;
            this.form_btnToggleInclude.Text = "Toggle Include";
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
            this.form_btnAddVolume.Text = "Add Volume";
            this.form_btnAddVolume.UseVisualStyleBackColor = true;
            this.form_btnAddVolume.Click += new System.EventHandler(this.form_btn_AddVolume_Click);
            // 
            // form_cbVolumeName
            // 
            this.form_cbVolumeName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.form_cbVolumeName.FormattingEnabled = true;
            this.form_cbVolumeName.Location = new System.Drawing.Point(111, 16);
            this.form_cbVolumeName.Name = "form_cbVolumeName";
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
            // form_lvVolumesMain
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
            this.form_lvVolumesMain.Name = "form_lvVolumesMain";
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
            // form_btnSavePathInfo
            // 
            this.form_btnSavePathInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.form_btnSavePathInfo.Enabled = false;
            this.form_btnSavePathInfo.Location = new System.Drawing.Point(14, 357);
            this.form_btnSavePathInfo.Name = "form_btnSavePathInfo";
            this.form_btnSavePathInfo.Size = new System.Drawing.Size(947, 40);
            this.form_btnSavePathInfo.TabIndex = 0;
            this.form_btnSavePathInfo.Text = "Save Directory Listings";
            this.form_btnSavePathInfo.UseVisualStyleBackColor = true;
            this.form_btnSavePathInfo.EnabledChanged += new System.EventHandler(this.form_btn_SavePathInfo_EnabledChanged);
            this.form_btnSavePathInfo.Click += new System.EventHandler(this.form_btn_SavePathInfo_Click);
            // 
            // form_btnSaveAs
            // 
            this.form_btnSaveAs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.form_btnSaveAs.Location = new System.Drawing.Point(933, 70);
            this.form_btnSaveAs.Name = "form_btnSaveAs";
            this.form_btnSaveAs.Size = new System.Drawing.Size(28, 23);
            this.form_btnSaveAs.TabIndex = 8;
            this.form_btnSaveAs.Text = "...";
            this.form_btnSaveAs.UseVisualStyleBackColor = true;
            this.form_btnSaveAs.Click += new System.EventHandler(this.form_btn_SaveAs_Click);
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
            // form_btnPath
            // 
            this.form_btnPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.form_btnPath.Location = new System.Drawing.Point(933, 43);
            this.form_btnPath.Name = "form_btnPath";
            this.form_btnPath.Size = new System.Drawing.Size(28, 23);
            this.form_btnPath.TabIndex = 4;
            this.form_btnPath.Text = "...";
            this.form_btnPath.UseVisualStyleBackColor = true;
            this.form_btnPath.Click += new System.EventHandler(this.form_btn_Path_Click);
            // 
            // form_cbSaveAs
            // 
            this.form_cbSaveAs.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.form_cbSaveAs.FormattingEnabled = true;
            this.form_cbSaveAs.Location = new System.Drawing.Point(114, 72);
            this.form_cbSaveAs.Name = "form_cbSaveAs";
            this.form_cbSaveAs.Size = new System.Drawing.Size(813, 21);
            this.form_cbSaveAs.TabIndex = 7;
            // 
            // form_cbPath
            // 
            this.form_cbPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.form_cbPath.FormattingEnabled = true;
            this.form_cbPath.Location = new System.Drawing.Point(49, 43);
            this.form_cbPath.Name = "form_cbPath";
            this.form_cbPath.Size = new System.Drawing.Size(878, 21);
            this.form_cbPath.TabIndex = 3;
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
            // form_tabPageSearch
            // 
            this.form_tabPageSearch.Controls.Add(this.form_btnSearchCopy);
            this.form_tabPageSearch.Controls.Add(this.form_chkSearchCase);
            this.form_tabPageSearch.Controls.Add(this.form_btnSearchFillPaths);
            this.form_tabPageSearch.Controls.Add(this.label6);
            this.form_tabPageSearch.Controls.Add(this.form_radSearchDirHandlingInnermost);
            this.form_tabPageSearch.Controls.Add(this.form_radSearchDirHandlingOutermost);
            this.form_tabPageSearch.Controls.Add(this.form_radSearchDirHandlingNone);
            this.form_tabPageSearch.Controls.Add(this.form_splitVolumes);
            this.form_tabPageSearch.Controls.Add(this.form_cbSearch);
            this.form_tabPageSearch.Controls.Add(this.form_btnSearch);
            this.form_tabPageSearch.Location = new System.Drawing.Point(4, 22);
            this.form_tabPageSearch.Name = "form_tabPageSearch";
            this.form_tabPageSearch.Padding = new System.Windows.Forms.Padding(3);
            this.form_tabPageSearch.Size = new System.Drawing.Size(973, 405);
            this.form_tabPageSearch.TabIndex = 1;
            this.form_tabPageSearch.Text = "Search";
            this.form_tabPageSearch.UseVisualStyleBackColor = true;
            // 
            // form_btnSearchCopy
            // 
            this.form_btnSearchCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.form_btnSearchCopy.Location = new System.Drawing.Point(926, 6);
            this.form_btnSearchCopy.Name = "form_btnSearchCopy";
            this.form_btnSearchCopy.Size = new System.Drawing.Size(41, 23);
            this.form_btnSearchCopy.TabIndex = 8;
            this.form_btnSearchCopy.Text = "Copy";
            this.form_btnSearchCopy.UseVisualStyleBackColor = true;
            this.form_btnSearchCopy.Click += new System.EventHandler(this.form_btnSearchCopy_Click);
            // 
            // form_chkSearchCase
            // 
            this.form_chkSearchCase.AutoSize = true;
            this.form_chkSearchCase.Checked = true;
            this.form_chkSearchCase.CheckState = System.Windows.Forms.CheckState.Checked;
            this.form_chkSearchCase.Location = new System.Drawing.Point(122, 35);
            this.form_chkSearchCase.Name = "form_chkSearchCase";
            this.form_chkSearchCase.Size = new System.Drawing.Size(96, 17);
            this.form_chkSearchCase.TabIndex = 2;
            this.form_chkSearchCase.Text = "Case Sensitive";
            this.form_chkSearchCase.UseVisualStyleBackColor = true;
            this.form_chkSearchCase.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.form_cb_Search_KeyPress);
            // 
            // form_btnSearchFillPaths
            // 
            this.form_btnSearchFillPaths.Enabled = false;
            this.form_btnSearchFillPaths.Location = new System.Drawing.Point(8, 31);
            this.form_btnSearchFillPaths.Name = "form_btnSearchFillPaths";
            this.form_btnSearchFillPaths.Size = new System.Drawing.Size(75, 23);
            this.form_btnSearchFillPaths.TabIndex = 1;
            this.form_btnSearchFillPaths.Text = "Fill Paths";
            this.form_btnSearchFillPaths.UseVisualStyleBackColor = true;
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
            // form_radSearchDirHandlingInnermost
            // 
            this.form_radSearchDirHandlingInnermost.AutoSize = true;
            this.form_radSearchDirHandlingInnermost.Location = new System.Drawing.Point(484, 34);
            this.form_radSearchDirHandlingInnermost.Name = "form_radSearchDirHandlingInnermost";
            this.form_radSearchDirHandlingInnermost.Size = new System.Drawing.Size(151, 17);
            this.form_radSearchDirHandlingInnermost.TabIndex = 6;
            this.form_radSearchDirHandlingInnermost.Text = "Innermost that have length";
            this.form_radSearchDirHandlingInnermost.UseVisualStyleBackColor = true;
            this.form_radSearchDirHandlingInnermost.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.form_cb_Search_KeyPress);
            // 
            // form_radSearchDirHandlingOutermost
            // 
            this.form_radSearchDirHandlingOutermost.AutoSize = true;
            this.form_radSearchDirHandlingOutermost.Checked = true;
            this.form_radSearchDirHandlingOutermost.Location = new System.Drawing.Point(405, 34);
            this.form_radSearchDirHandlingOutermost.Name = "form_radSearchDirHandlingOutermost";
            this.form_radSearchDirHandlingOutermost.Size = new System.Drawing.Size(73, 17);
            this.form_radSearchDirHandlingOutermost.TabIndex = 5;
            this.form_radSearchDirHandlingOutermost.TabStop = true;
            this.form_radSearchDirHandlingOutermost.Text = "Outermost";
            this.form_radSearchDirHandlingOutermost.UseVisualStyleBackColor = true;
            this.form_radSearchDirHandlingOutermost.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.form_cb_Search_KeyPress);
            // 
            // form_radSearchDirHandlingNone
            // 
            this.form_radSearchDirHandlingNone.AutoSize = true;
            this.form_radSearchDirHandlingNone.Location = new System.Drawing.Point(348, 34);
            this.form_radSearchDirHandlingNone.Name = "form_radSearchDirHandlingNone";
            this.form_radSearchDirHandlingNone.Size = new System.Drawing.Size(51, 17);
            this.form_radSearchDirHandlingNone.TabIndex = 4;
            this.form_radSearchDirHandlingNone.Text = "None";
            this.form_radSearchDirHandlingNone.UseVisualStyleBackColor = true;
            this.form_radSearchDirHandlingNone.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.form_cb_Search_KeyPress);
            // 
            // form_splitVolumes
            // 
            this.form_splitVolumes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.form_splitVolumes.Location = new System.Drawing.Point(-4, 60);
            this.form_splitVolumes.Name = "form_splitVolumes";
            this.form_splitVolumes.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // form_splitVolumes.Panel1
            // 
            this.form_splitVolumes.Panel1.Controls.Add(this.form_lvSearchResults);
            // 
            // form_splitVolumes.Panel2
            // 
            this.form_splitVolumes.Panel2.Controls.Add(this.form_lvPathErrors);
            this.form_splitVolumes.Panel2.Controls.Add(this.label3);
            this.form_splitVolumes.Size = new System.Drawing.Size(981, 349);
            this.form_splitVolumes.SplitterDistance = 260;
            this.form_splitVolumes.TabIndex = 6;
            // 
            // form_lvSearchResults
            // 
            this.form_lvSearchResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
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
            this.form_lvSearchResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_lvSearchResults.FullRowSelect = true;
            this.form_lvSearchResults.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.form_lvSearchResults.Location = new System.Drawing.Point(0, 0);
            this.form_lvSearchResults.MultiSelect = false;
            this.form_lvSearchResults.Name = "form_lvSearchResults";
            this.form_lvSearchResults.Size = new System.Drawing.Size(981, 260);
            this.form_lvSearchResults.TabIndex = 0;
            this.form_lvSearchResults.UseCompatibleStateImageBehavior = false;
            this.form_lvSearchResults.View = System.Windows.Forms.View.Details;
            this.form_lvSearchResults.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.form_lv_SearchResults_KeyPress);
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
            this.form_lv_SearchResults_col_Path.Width = 293;
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
            // form_lvPathErrors
            // 
            this.form_lvPathErrors.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.form_lvPathErrors.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.form_lv_Errors_col_Volume,
            this.form_lv_Errors_col_FileOrPath,
            this.form_lv_Errors_col_Error1,
            this.form_lv_Errors_col_Error2});
            this.form_lvPathErrors.FullRowSelect = true;
            this.form_lvPathErrors.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.form_lvPathErrors.Location = new System.Drawing.Point(0, 16);
            this.form_lvPathErrors.MultiSelect = false;
            this.form_lvPathErrors.Name = "form_lvPathErrors";
            this.form_lvPathErrors.Size = new System.Drawing.Size(981, 69);
            this.form_lvPathErrors.TabIndex = 1;
            this.form_lvPathErrors.UseCompatibleStateImageBehavior = false;
            this.form_lvPathErrors.View = System.Windows.Forms.View.Details;
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
            this.form_tabPage_Browse.Controls.Add(this.form_lblVolGroup);
            this.form_tabPage_Browse.Controls.Add(this.form_btnTreeCopy);
            this.form_tabPage_Browse.Controls.Add(this.form_chkCompare1);
            this.form_tabPage_Browse.Controls.Add(this.form_btnCompare);
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
            // form_lblVolGroup
            // 
            this.form_lblVolGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.form_lblVolGroup.AutoSize = true;
            this.form_lblVolGroup.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.form_lblVolGroup.Location = new System.Drawing.Point(831, 8);
            this.form_lblVolGroup.Name = "form_lblVolGroup";
            this.form_lblVolGroup.Size = new System.Drawing.Size(74, 13);
            this.form_lblVolGroup.TabIndex = 6;
            this.form_lblVolGroup.Text = "Volume Group";
            this.form_lblVolGroup.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // form_btnTreeCopy
            // 
            this.form_btnTreeCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.form_btnTreeCopy.Location = new System.Drawing.Point(784, 3);
            this.form_btnTreeCopy.Name = "form_btnTreeCopy";
            this.form_btnTreeCopy.Size = new System.Drawing.Size(41, 23);
            this.form_btnTreeCopy.TabIndex = 5;
            this.form_btnTreeCopy.Text = "Copy";
            this.form_btnTreeCopy.UseVisualStyleBackColor = true;
            this.form_btnTreeCopy.Click += new System.EventHandler(this.form_btn_Copy_Click);
            this.form_btnTreeCopy.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CompareModeButtonKeyPress);
            // 
            // form_chkCompare1
            // 
            this.form_chkCompare1.AutoSize = true;
            this.form_chkCompare1.Location = new System.Drawing.Point(70, 7);
            this.form_chkCompare1.Name = "form_chkCompare1";
            this.form_chkCompare1.Size = new System.Drawing.Size(77, 17);
            this.form_chkCompare1.TabIndex = 1;
            this.form_chkCompare1.Text = "Compare 1";
            this.form_chkCompare1.UseVisualStyleBackColor = true;
            this.form_chkCompare1.CheckedChanged += new System.EventHandler(this.form_chk_Compare1_CheckedChanged);
            // 
            // form_btnCompare
            // 
            this.form_btnCompare.Enabled = false;
            this.form_btnCompare.Location = new System.Drawing.Point(153, 3);
            this.form_btnCompare.Name = "form_btnCompare";
            this.form_btnCompare.Size = new System.Drawing.Size(62, 23);
            this.form_btnCompare.TabIndex = 2;
            this.form_btnCompare.Text = "Compare";
            this.form_btnCompare.UseVisualStyleBackColor = true;
            this.form_btnCompare.Click += new System.EventHandler(this.form_btn_Compare_Click);
            this.form_btnCompare.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.CompareModeButtonKeyPress);
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
            // form_splitFiles
            // 
            this.form_splitFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.form_splitFiles.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.form_splitFiles.Location = new System.Drawing.Point(0, 31);
            this.form_splitFiles.Name = "form_splitFiles";
            // 
            // form_splitFiles.Panel1
            // 
            this.form_splitFiles.Panel1.Controls.Add(this.form_splitTreeFind);
            // 
            // form_splitFiles.Panel2
            // 
            this.form_splitFiles.Panel2.Controls.Add(this.form_splitClones);
            this.form_splitFiles.Size = new System.Drawing.Size(970, 374);
            this.form_splitFiles.SplitterDistance = 570;
            this.form_splitFiles.TabIndex = 1;
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
            this.form_splitTreeFind.Size = new System.Drawing.Size(570, 374);
            this.form_splitTreeFind.SplitterDistance = 200;
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
            this.form_splitCompare.Size = new System.Drawing.Size(150, 200);
            this.form_splitCompare.SplitterDistance = 72;
            this.form_splitCompare.TabIndex = 2;
            // 
            // form_treeCompare1
            // 
            this.form_treeCompare1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_treeCompare1.FullRowSelect = true;
            this.form_treeCompare1.HideSelection = false;
            this.form_treeCompare1.Location = new System.Drawing.Point(0, 0);
            this.form_treeCompare1.Name = "form_treeCompare1";
            this.form_treeCompare1.ShowLines = false;
            this.form_treeCompare1.Size = new System.Drawing.Size(72, 200);
            this.form_treeCompare1.TabIndex = 1;
            this.form_treeCompare1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.form_treeView_Browse_AfterSelect);
            this.form_treeCompare1.Enter += new System.EventHandler(this.formCtl_EnterForCopyButton);
            this.form_treeCompare1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.form_tree_compare_KeyPress);
            this.form_treeCompare1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.form_treeView_Browse_MouseClick);
            // 
            // form_treeCompare2
            // 
            this.form_treeCompare2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_treeCompare2.FullRowSelect = true;
            this.form_treeCompare2.HideSelection = false;
            this.form_treeCompare2.Location = new System.Drawing.Point(0, 0);
            this.form_treeCompare2.Name = "form_treeCompare2";
            this.form_treeCompare2.ShowLines = false;
            this.form_treeCompare2.Size = new System.Drawing.Size(74, 200);
            this.form_treeCompare2.TabIndex = 2;
            this.form_treeCompare2.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.form_treeView_Browse_AfterSelect);
            this.form_treeCompare2.Enter += new System.EventHandler(this.formCtl_EnterForCopyButton);
            this.form_treeCompare2.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.form_tree_compare_KeyPress);
            this.form_treeCompare2.MouseClick += new System.Windows.Forms.MouseEventHandler(this.form_treeView_Browse_MouseClick);
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
            this.form_splitClones.Panel2.Controls.Add(this.form_splitUnique);
            this.form_splitClones.Size = new System.Drawing.Size(396, 374);
            this.form_splitClones.SplitterDistance = 140;
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
            this.form_splitDetail.Panel1.Controls.Add(this.form_splitCompareFiles);
            // 
            // form_splitDetail.Panel2
            // 
            this.form_splitDetail.Panel2.Controls.Add(this.form_splitDetailVols);
            this.form_splitDetail.Size = new System.Drawing.Size(140, 374);
            this.form_splitDetail.SplitterDistance = 172;
            this.form_splitDetail.TabIndex = 1;
            // 
            // form_splitCompareFiles
            // 
            this.form_splitCompareFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_splitCompareFiles.Location = new System.Drawing.Point(0, 0);
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
            this.form_splitCompareFiles.Size = new System.Drawing.Size(140, 172);
            this.form_splitCompareFiles.SplitterDistance = 71;
            this.form_splitCompareFiles.TabIndex = 1;
            // 
            // form_lvFiles
            // 
            this.form_lvFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.form_colFilename,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader8,
            this.columnHeader9,
            this.columnHeader10});
            this.form_lvFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_lvFiles.FullRowSelect = true;
            this.form_lvFiles.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.form_lvFiles.HideSelection = false;
            this.form_lvFiles.Location = new System.Drawing.Point(0, 0);
            this.form_lvFiles.MultiSelect = false;
            this.form_lvFiles.Name = "form_lvFiles";
            this.form_lvFiles.Size = new System.Drawing.Size(140, 172);
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
            this.form_splitDetailVols.Size = new System.Drawing.Size(140, 198);
            this.form_splitDetailVols.SplitterDistance = 67;
            this.form_splitDetailVols.TabIndex = 1;
            // 
            // form_lvDetail
            // 
            this.form_lvDetail.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.form_colDirDetail});
            this.form_lvDetail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_lvDetail.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.form_lvDetail.Location = new System.Drawing.Point(0, 0);
            this.form_lvDetail.MultiSelect = false;
            this.form_lvDetail.Name = "form_lvDetail";
            this.form_lvDetail.Scrollable = false;
            this.form_lvDetail.Size = new System.Drawing.Size(67, 198);
            this.form_lvDetail.TabIndex = 0;
            this.form_lvDetail.UseCompatibleStateImageBehavior = false;
            this.form_lvDetail.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Directory detail";
            this.columnHeader1.Width = 120;
            // 
            // form_colDirDetail
            // 
            this.form_colDirDetail.Text = " ";
            this.form_colDirDetail.Width = 9999;
            // 
            // form_lvDetailVol
            // 
            this.form_lvDetailVol.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.form_colDirDetailCompare,
            this.form_colVolDetail});
            this.form_lvDetailVol.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_lvDetailVol.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.form_lvDetailVol.Location = new System.Drawing.Point(0, 0);
            this.form_lvDetailVol.MultiSelect = false;
            this.form_lvDetailVol.Name = "form_lvDetailVol";
            this.form_lvDetailVol.Scrollable = false;
            this.form_lvDetailVol.Size = new System.Drawing.Size(69, 198);
            this.form_lvDetailVol.TabIndex = 0;
            this.form_lvDetailVol.UseCompatibleStateImageBehavior = false;
            this.form_lvDetailVol.View = System.Windows.Forms.View.Details;
            // 
            // form_colDirDetailCompare
            // 
            this.form_colDirDetailCompare.Text = "Volume detail";
            this.form_colDirDetailCompare.Width = 120;
            // 
            // form_colVolDetail
            // 
            this.form_colVolDetail.Text = " ";
            this.form_colVolDetail.Width = 9999;
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
            this.form_splitUnique.Panel1.Controls.Add(this.form_lvUnique);
            // 
            // form_splitUnique.Panel2
            // 
            this.form_splitUnique.Panel2.Controls.Add(this.form_lvClones);
            this.form_splitUnique.Size = new System.Drawing.Size(252, 374);
            this.form_splitUnique.SplitterDistance = 187;
            this.form_splitUnique.TabIndex = 2;
            // 
            // form_lvUnique
            // 
            this.form_lvUnique.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader16});
            this.form_lvUnique.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_lvUnique.FullRowSelect = true;
            this.form_lvUnique.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.form_lvUnique.HideSelection = false;
            this.form_lvUnique.Location = new System.Drawing.Point(0, 0);
            this.form_lvUnique.MultiSelect = false;
            this.form_lvUnique.Name = "form_lvUnique";
            this.form_lvUnique.Size = new System.Drawing.Size(252, 187);
            this.form_lvUnique.TabIndex = 0;
            this.form_lvUnique.UseCompatibleStateImageBehavior = false;
            this.form_lvUnique.View = System.Windows.Forms.View.Details;
            this.form_lvUnique.SelectedIndexChanged += new System.EventHandler(this.form_lv_Unique_SelectedIndexChanged);
            this.form_lvUnique.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.form_lv_Unique_KeyPress);
            this.form_lvUnique.MouseClick += new System.Windows.Forms.MouseEventHandler(this.form_lv_Unique_MouseClick);
            // 
            // columnHeader16
            // 
            this.columnHeader16.Text = "Solitary";
            this.columnHeader16.Width = 230;
            // 
            // form_lvClones
            // 
            this.form_lvClones.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader12,
            this.columnHeader13});
            this.form_lvClones.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_lvClones.FullRowSelect = true;
            this.form_lvClones.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.form_lvClones.HideSelection = false;
            this.form_lvClones.Location = new System.Drawing.Point(0, 0);
            this.form_lvClones.MultiSelect = false;
            this.form_lvClones.Name = "form_lvClones";
            this.form_lvClones.Size = new System.Drawing.Size(252, 183);
            this.form_lvClones.TabIndex = 0;
            this.form_lvClones.UseCompatibleStateImageBehavior = false;
            this.form_lvClones.View = System.Windows.Forms.View.Details;
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
            this.form_tabPageSearch.ResumeLayout(false);
            this.form_tabPageSearch.PerformLayout();
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
            this.form_splitUnique.Panel1.ResumeLayout(false);
            this.form_splitUnique.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.form_splitUnique)).EndInit();
            this.form_splitUnique.ResumeLayout(false);
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
        private System.Windows.Forms.Button form_btnSavePathInfo;
        private System.Windows.Forms.Button form_btnSaveAs;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button form_btnPath;
        private System.Windows.Forms.ComboBox form_cbSaveAs;
        private System.Windows.Forms.ComboBox form_cbPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage form_tabPageSearch;
        private System.Windows.Forms.SplitContainer form_splitVolumes;
        private ListViewEx form_lvSearchResults;
        private System.Windows.Forms.ColumnHeader form_lv_SearchResults_col_Filename;
        private System.Windows.Forms.ColumnHeader form_lv_SearchResults_col_Length;
        private System.Windows.Forms.ColumnHeader form_lv_SearchResults_col_Modified;
        private System.Windows.Forms.ColumnHeader form_lv_SearchResults_col_Created;
        private ListViewEx form_lvPathErrors;
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
        private System.Windows.Forms.RadioButton form_radSearchDirHandlingInnermost;
        private System.Windows.Forms.RadioButton form_radSearchDirHandlingOutermost;
        private System.Windows.Forms.RadioButton form_radSearchDirHandlingNone;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.TabPage form_tabPage_Browse;
        private System.Windows.Forms.TreeView form_treeView_Browse;
        private System.Windows.Forms.Timer timer_killRed;
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
        private System.Windows.Forms.ColumnHeader columnHeader11;
        private System.Windows.Forms.Button form_btnSearchFillPaths;
        private System.Windows.Forms.CheckBox form_chkSearchCase;
        private System.Windows.Forms.SplitContainer form_splitClones;
        private ListViewEx form_lvClones;
        private System.Windows.Forms.ColumnHeader columnHeader12;
        private System.Windows.Forms.ColumnHeader columnHeader13;
        private System.Windows.Forms.SplitContainer form_splitDetailVols;
        private ListViewEx form_lvDetailVol;
        private System.Windows.Forms.ColumnHeader form_colDirDetailCompare;
        private System.Windows.Forms.ColumnHeader form_colVolDetail;
        private System.Windows.Forms.Button form_btn_TreeFind;
        private System.Windows.Forms.Button form_btn_TreeCollapse;
        private System.Windows.Forms.ComboBox form_cb_TreeFind;
        private System.Windows.Forms.SplitContainer form_splitUnique;
        private ListViewEx form_lvUnique;
        private System.Windows.Forms.ColumnHeader columnHeader16;
        private System.Windows.Forms.CheckBox form_chkCompare1;
        private System.Windows.Forms.Button form_btnCompare;
        private System.Windows.Forms.Timer timer_blink;
        private System.Windows.Forms.SplitContainer form_splitTreeFind;
        private System.Windows.Forms.TreeView form_treeCompare1;
        private System.Windows.Forms.SplitContainer form_splitCompare;
        private System.Windows.Forms.TreeView form_treeCompare2;
        private System.Windows.Forms.SplitContainer form_splitCompareFiles;
        private ListViewEx form_lvFileCompare;
        private System.Windows.Forms.ColumnHeader form_colFileCompare;
        private System.Windows.Forms.ColumnHeader columnHeader19;
        private System.Windows.Forms.ColumnHeader columnHeader20;
        private System.Windows.Forms.ColumnHeader columnHeader21;
        private System.Windows.Forms.ColumnHeader columnHeader22;
        private System.Windows.Forms.ColumnHeader columnHeader23;
        private System.Windows.Forms.ColumnHeader columnHeader24;
        private System.Windows.Forms.ColumnHeader columnHeader26;
        private System.Windows.Forms.ColumnHeader columnHeader27;
        private System.Windows.Forms.Button form_btnTreeCopy;
        private System.Windows.Forms.Button form_btnVolGroup;
        private System.Windows.Forms.ColumnHeader columnHeader28;
        private System.Windows.Forms.Timer timer_DoTree;
        private System.Windows.Forms.Label form_lblVolGroup;
        private System.Windows.Forms.Button form_btnSearchCopy;

    }
}

