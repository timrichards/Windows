namespace FileInfoSerializer
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
            this.form_btn_Search = new System.Windows.Forms.Button();
            this.form_cb_Search = new System.Windows.Forms.ComboBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.form_btn_LoadVolumeList = new System.Windows.Forms.Button();
            this.form_btn_SaveVolumeList = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.form_btn_RemoveVolume = new System.Windows.Forms.Button();
            this.form_btn_ToggleInclude = new System.Windows.Forms.Button();
            this.form_btn_AddVolume = new System.Windows.Forms.Button();
            this.form_cb_VolumeName = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.form_lv_Volumes = new System.Windows.Forms.ListView();
            this.form_lv_Volumes_col_Volume = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_Volumes_col_Path = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_Volumes_col_SaveToFile = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_Volumes_col_Status = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_Volumes_col_IncludeInSearch = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_btn_SavePathInfo = new System.Windows.Forms.Button();
            this.form_btn_SaveAs = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.form_btn_Path = new System.Windows.Forms.Button();
            this.form_cb_SaveAs = new System.Windows.Forms.ComboBox();
            this.form_cb_Path = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.form_lv_SearchResults = new System.Windows.Forms.ListView();
            this.form_lv_SearchResults_col_Volume = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_SearchResults_col_Filename = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_SearchResults_col_Length = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_SearchResults_col_ContainingDir = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_SearchResults_col_Modified = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_SearchResults_col_Created = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_SearchResults_col_Path = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_PathErrors = new System.Windows.Forms.ListView();
            this.form_lv_Errors_col_Volume = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_Errors_col_FileOrPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_Errors_col_Error1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_Errors_col_Error2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label3 = new System.Windows.Forms.Label();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.form_chk_FoldersWithLength = new System.Windows.Forms.CheckBox();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // form_btn_Search
            // 
            this.form_btn_Search.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.form_btn_Search.Location = new System.Drawing.Point(641, 31);
            this.form_btn_Search.Name = "form_btn_Search";
            this.form_btn_Search.Size = new System.Drawing.Size(75, 23);
            this.form_btn_Search.TabIndex = 2;
            this.form_btn_Search.Text = "Search String";
            this.form_btn_Search.UseVisualStyleBackColor = true;
            this.form_btn_Search.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // form_cb_Search
            // 
            this.form_cb_Search.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.form_cb_Search.FormattingEnabled = true;
            this.form_cb_Search.Location = new System.Drawing.Point(0, 6);
            this.form_cb_Search.Name = "form_cb_Search";
            this.form_cb_Search.Size = new System.Drawing.Size(722, 21);
            this.form_cb_Search.TabIndex = 1;
            this.form_cb_Search.SelectedIndexChanged += new System.EventHandler(this.cb_Search_SelectedIndexChanged);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(730, 382);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.form_btn_LoadVolumeList);
            this.tabPage1.Controls.Add(this.form_btn_SaveVolumeList);
            this.tabPage1.Controls.Add(this.label5);
            this.tabPage1.Controls.Add(this.form_btn_RemoveVolume);
            this.tabPage1.Controls.Add(this.form_btn_ToggleInclude);
            this.tabPage1.Controls.Add(this.form_btn_AddVolume);
            this.tabPage1.Controls.Add(this.form_cb_VolumeName);
            this.tabPage1.Controls.Add(this.label4);
            this.tabPage1.Controls.Add(this.form_lv_Volumes);
            this.tabPage1.Controls.Add(this.form_btn_SavePathInfo);
            this.tabPage1.Controls.Add(this.form_btn_SaveAs);
            this.tabPage1.Controls.Add(this.label2);
            this.tabPage1.Controls.Add(this.form_btn_Path);
            this.tabPage1.Controls.Add(this.form_cb_SaveAs);
            this.tabPage1.Controls.Add(this.form_cb_Path);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(722, 356);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Volumes";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // form_btn_LoadVolumeList
            // 
            this.form_btn_LoadVolumeList.Location = new System.Drawing.Point(126, 143);
            this.form_btn_LoadVolumeList.Name = "form_btn_LoadVolumeList";
            this.form_btn_LoadVolumeList.Size = new System.Drawing.Size(51, 23);
            this.form_btn_LoadVolumeList.TabIndex = 7;
            this.form_btn_LoadVolumeList.Text = "Load";
            this.form_btn_LoadVolumeList.UseVisualStyleBackColor = true;
            // 
            // form_btn_SaveVolumeList
            // 
            this.form_btn_SaveVolumeList.Location = new System.Drawing.Point(74, 143);
            this.form_btn_SaveVolumeList.Name = "form_btn_SaveVolumeList";
            this.form_btn_SaveVolumeList.Size = new System.Drawing.Size(46, 23);
            this.form_btn_SaveVolumeList.TabIndex = 6;
            this.form_btn_SaveVolumeList.Text = "Save";
            this.form_btn_SaveVolumeList.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(11, 147);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(57, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Volume list";
            // 
            // form_btn_RemoveVolume
            // 
            this.form_btn_RemoveVolume.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.form_btn_RemoveVolume.Enabled = false;
            this.form_btn_RemoveVolume.Location = new System.Drawing.Point(501, 142);
            this.form_btn_RemoveVolume.Name = "form_btn_RemoveVolume";
            this.form_btn_RemoveVolume.Size = new System.Drawing.Size(103, 23);
            this.form_btn_RemoveVolume.TabIndex = 8;
            this.form_btn_RemoveVolume.Text = "Remove Volume";
            this.form_btn_RemoveVolume.UseVisualStyleBackColor = true;
            this.form_btn_RemoveVolume.Click += new System.EventHandler(this.form_btn_RemoveVolume_Click);
            // 
            // form_btn_ToggleInclude
            // 
            this.form_btn_ToggleInclude.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.form_btn_ToggleInclude.Enabled = false;
            this.form_btn_ToggleInclude.Location = new System.Drawing.Point(610, 142);
            this.form_btn_ToggleInclude.Name = "form_btn_ToggleInclude";
            this.form_btn_ToggleInclude.Size = new System.Drawing.Size(100, 23);
            this.form_btn_ToggleInclude.TabIndex = 9;
            this.form_btn_ToggleInclude.Text = "Toggle Include";
            this.form_btn_ToggleInclude.UseVisualStyleBackColor = true;
            this.form_btn_ToggleInclude.Click += new System.EventHandler(this.form_btn_ToggleInclude_Click);
            // 
            // form_btn_AddVolume
            // 
            this.form_btn_AddVolume.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.form_btn_AddVolume.Location = new System.Drawing.Point(319, 99);
            this.form_btn_AddVolume.Name = "form_btn_AddVolume";
            this.form_btn_AddVolume.Size = new System.Drawing.Size(85, 66);
            this.form_btn_AddVolume.TabIndex = 3;
            this.form_btn_AddVolume.Text = "Add Volume";
            this.form_btn_AddVolume.UseVisualStyleBackColor = true;
            this.form_btn_AddVolume.Click += new System.EventHandler(this.form_btn_AddVolume_Click);
            // 
            // form_cb_VolumeName
            // 
            this.form_cb_VolumeName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.form_cb_VolumeName.FormattingEnabled = true;
            this.form_cb_VolumeName.Location = new System.Drawing.Point(113, 16);
            this.form_cb_VolumeName.Name = "form_cb_VolumeName";
            this.form_cb_VolumeName.Size = new System.Drawing.Size(597, 21);
            this.form_cb_VolumeName.TabIndex = 0;
            this.form_cb_VolumeName.SelectedIndexChanged += new System.EventHandler(this.form_cb_VolumeName_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 19);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(93, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "Volume Nickname";
            // 
            // form_lv_Volumes
            // 
            this.form_lv_Volumes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.form_lv_Volumes.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.form_lv_Volumes_col_Volume,
            this.form_lv_Volumes_col_Path,
            this.form_lv_Volumes_col_SaveToFile,
            this.form_lv_Volumes_col_Status,
            this.form_lv_Volumes_col_IncludeInSearch});
            this.form_lv_Volumes.FullRowSelect = true;
            this.form_lv_Volumes.HideSelection = false;
            this.form_lv_Volumes.Location = new System.Drawing.Point(0, 172);
            this.form_lv_Volumes.MultiSelect = false;
            this.form_lv_Volumes.Name = "form_lv_Volumes";
            this.form_lv_Volumes.Size = new System.Drawing.Size(722, 130);
            this.form_lv_Volumes.TabIndex = 10;
            this.form_lv_Volumes.UseCompatibleStateImageBehavior = false;
            this.form_lv_Volumes.View = System.Windows.Forms.View.Details;
            this.form_lv_Volumes.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.form_lv_Volumes_ItemSelectionChanged);
            // 
            // form_lv_Volumes_col_Volume
            // 
            this.form_lv_Volumes_col_Volume.Text = "Volume Name";
            this.form_lv_Volumes_col_Volume.Width = 200;
            // 
            // form_lv_Volumes_col_Path
            // 
            this.form_lv_Volumes_col_Path.Text = "Path";
            this.form_lv_Volumes_col_Path.Width = 134;
            // 
            // form_lv_Volumes_col_SaveToFile
            // 
            this.form_lv_Volumes_col_SaveToFile.Text = "Save to File";
            this.form_lv_Volumes_col_SaveToFile.Width = 183;
            // 
            // form_lv_Volumes_col_Status
            // 
            this.form_lv_Volumes_col_Status.Text = "Status";
            this.form_lv_Volumes_col_Status.Width = 94;
            // 
            // form_lv_Volumes_col_IncludeInSearch
            // 
            this.form_lv_Volumes_col_IncludeInSearch.Text = "Include in Search";
            this.form_lv_Volumes_col_IncludeInSearch.Width = 97;
            // 
            // form_btn_SavePathInfo
            // 
            this.form_btn_SavePathInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.form_btn_SavePathInfo.Enabled = false;
            this.form_btn_SavePathInfo.Location = new System.Drawing.Point(14, 308);
            this.form_btn_SavePathInfo.Name = "form_btn_SavePathInfo";
            this.form_btn_SavePathInfo.Size = new System.Drawing.Size(696, 40);
            this.form_btn_SavePathInfo.TabIndex = 11;
            this.form_btn_SavePathInfo.Text = "Save Directory Listings";
            this.form_btn_SavePathInfo.UseVisualStyleBackColor = true;
            this.form_btn_SavePathInfo.Click += new System.EventHandler(this.form_btn_SavePathInfo_Click);
            // 
            // form_btn_SaveAs
            // 
            this.form_btn_SaveAs.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.form_btn_SaveAs.Location = new System.Drawing.Point(682, 70);
            this.form_btn_SaveAs.Name = "form_btn_SaveAs";
            this.form_btn_SaveAs.Size = new System.Drawing.Size(28, 23);
            this.form_btn_SaveAs.TabIndex = 5;
            this.form_btn_SaveAs.Text = "...";
            this.form_btn_SaveAs.UseVisualStyleBackColor = true;
            this.form_btn_SaveAs.Click += new System.EventHandler(this.form_btn_SaveAs_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 75);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Directory Listing File";
            // 
            // form_btn_Path
            // 
            this.form_btn_Path.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.form_btn_Path.Location = new System.Drawing.Point(682, 43);
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
            this.form_cb_SaveAs.Location = new System.Drawing.Point(121, 72);
            this.form_cb_SaveAs.Name = "form_cb_SaveAs";
            this.form_cb_SaveAs.Size = new System.Drawing.Size(555, 21);
            this.form_cb_SaveAs.TabIndex = 2;
            // 
            // form_cb_Path
            // 
            this.form_cb_Path.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.form_cb_Path.FormattingEnabled = true;
            this.form_cb_Path.Location = new System.Drawing.Point(49, 43);
            this.form_cb_Path.Name = "form_cb_Path";
            this.form_cb_Path.Size = new System.Drawing.Size(627, 21);
            this.form_cb_Path.TabIndex = 1;
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
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.form_chk_FoldersWithLength);
            this.tabPage2.Controls.Add(this.splitContainer1);
            this.tabPage2.Controls.Add(this.form_cb_Search);
            this.tabPage2.Controls.Add(this.form_btn_Search);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(722, 356);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Search";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(-4, 60);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.form_lv_SearchResults);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.form_lv_PathErrors);
            this.splitContainer1.Panel2.Controls.Add(this.label3);
            this.splitContainer1.Size = new System.Drawing.Size(730, 300);
            this.splitContainer1.SplitterDistance = 224;
            this.splitContainer1.TabIndex = 9;
            // 
            // form_lv_SearchResults
            // 
            this.form_lv_SearchResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.form_lv_SearchResults_col_Volume,
            this.form_lv_SearchResults_col_Filename,
            this.form_lv_SearchResults_col_Length,
            this.form_lv_SearchResults_col_ContainingDir,
            this.form_lv_SearchResults_col_Modified,
            this.form_lv_SearchResults_col_Created,
            this.form_lv_SearchResults_col_Path});
            this.form_lv_SearchResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_lv_SearchResults.FullRowSelect = true;
            this.form_lv_SearchResults.Location = new System.Drawing.Point(0, 0);
            this.form_lv_SearchResults.MultiSelect = false;
            this.form_lv_SearchResults.Name = "form_lv_SearchResults";
            this.form_lv_SearchResults.Size = new System.Drawing.Size(730, 224);
            this.form_lv_SearchResults.TabIndex = 0;
            this.form_lv_SearchResults.UseCompatibleStateImageBehavior = false;
            this.form_lv_SearchResults.View = System.Windows.Forms.View.Details;
            // 
            // form_lv_SearchResults_col_Volume
            // 
            this.form_lv_SearchResults_col_Volume.Text = "Volume";
            // 
            // form_lv_SearchResults_col_Filename
            // 
            this.form_lv_SearchResults_col_Filename.Text = "Filename";
            this.form_lv_SearchResults_col_Filename.Width = 101;
            // 
            // form_lv_SearchResults_col_Length
            // 
            this.form_lv_SearchResults_col_Length.Text = "Length";
            this.form_lv_SearchResults_col_Length.Width = 73;
            // 
            // form_lv_SearchResults_col_ContainingDir
            // 
            this.form_lv_SearchResults_col_ContainingDir.Text = "Containing Dir";
            this.form_lv_SearchResults_col_ContainingDir.Width = 99;
            // 
            // form_lv_SearchResults_col_Modified
            // 
            this.form_lv_SearchResults_col_Modified.Text = "Modified";
            this.form_lv_SearchResults_col_Modified.Width = 86;
            // 
            // form_lv_SearchResults_col_Created
            // 
            this.form_lv_SearchResults_col_Created.Text = "Created";
            this.form_lv_SearchResults_col_Created.Width = 100;
            // 
            // form_lv_SearchResults_col_Path
            // 
            this.form_lv_SearchResults_col_Path.Text = "Path";
            this.form_lv_SearchResults_col_Path.Width = 223;
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
            this.form_lv_PathErrors.Location = new System.Drawing.Point(0, 16);
            this.form_lv_PathErrors.MultiSelect = false;
            this.form_lv_PathErrors.Name = "form_lv_PathErrors";
            this.form_lv_PathErrors.Size = new System.Drawing.Size(730, 56);
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
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(0, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 13);
            this.label3.TabIndex = 0;
            this.label3.Text = "Path Errors";
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "txt";
            this.saveFileDialog1.Filter = "Text files|*.txt|All files|*.*";
            this.saveFileDialog1.OverwritePrompt = false;
            // 
            // form_chk_FoldersWithLength
            // 
            this.form_chk_FoldersWithLength.AutoSize = true;
            this.form_chk_FoldersWithLength.Location = new System.Drawing.Point(425, 35);
            this.form_chk_FoldersWithLength.Name = "form_chk_FoldersWithLength";
            this.form_chk_FoldersWithLength.Size = new System.Drawing.Size(210, 17);
            this.form_chk_FoldersWithLength.TabIndex = 10;
            this.form_chk_FoldersWithLength.Text = "List only (inner) folders that have length";
            this.form_chk_FoldersWithLength.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(730, 382);
            this.Controls.Add(this.tabControl1);
            this.MinimumSize = new System.Drawing.Size(746, 420);
            this.Name = "Form1";
            this.Text = "FileInfoSerializer";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button form_btn_Search;
        private System.Windows.Forms.ComboBox form_cb_Search;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.Button form_btn_ToggleInclude;
        private System.Windows.Forms.Button form_btn_AddVolume;
        private System.Windows.Forms.ComboBox form_cb_VolumeName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListView form_lv_Volumes;
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
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListView form_lv_SearchResults;
        private System.Windows.Forms.ColumnHeader form_lv_SearchResults_col_Filename;
        private System.Windows.Forms.ColumnHeader form_lv_SearchResults_col_Length;
        private System.Windows.Forms.ColumnHeader form_lv_SearchResults_col_ContainingDir;
        private System.Windows.Forms.ColumnHeader form_lv_SearchResults_col_Modified;
        private System.Windows.Forms.ColumnHeader form_lv_SearchResults_col_Created;
        private System.Windows.Forms.ColumnHeader form_lv_SearchResults_col_Path;
        private System.Windows.Forms.ListView form_lv_PathErrors;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.ColumnHeader form_lv_SearchResults_col_Volume;
        private System.Windows.Forms.ColumnHeader form_lv_Errors_col_Volume;
        private System.Windows.Forms.ColumnHeader form_lv_Errors_col_FileOrPath;
        private System.Windows.Forms.ColumnHeader form_lv_Errors_col_Error1;
        private System.Windows.Forms.ColumnHeader form_lv_Errors_col_Error2;
        private System.Windows.Forms.Button form_btn_RemoveVolume;
        private System.Windows.Forms.Button form_btn_LoadVolumeList;
        private System.Windows.Forms.Button form_btn_SaveVolumeList;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox form_chk_FoldersWithLength;

    }
}

