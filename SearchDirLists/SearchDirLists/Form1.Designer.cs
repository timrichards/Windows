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
            this.form_btn_Search = new System.Windows.Forms.Button();
            this.form_cb_Search = new System.Windows.Forms.ComboBox();
            this.form_tabControl = new System.Windows.Forms.TabControl();
            this.form_tabPage_Volumes = new System.Windows.Forms.TabPage();
            this.form_btn_LoadVolumeList = new System.Windows.Forms.Button();
            this.form_btn_SaveVolumeList = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.form_btn_RemoveVolume = new System.Windows.Forms.Button();
            this.form_btn_ToggleInclude = new System.Windows.Forms.Button();
            this.form_btn_AddVolume = new System.Windows.Forms.Button();
            this.form_cb_VolumeName = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.form_lv_Volumes = new ListViewEx();
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
            this.form_tabPage_Search = new System.Windows.Forms.TabPage();
            this.form_btn_Search_FillPaths = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.form_rad_Folder_Innermost = new System.Windows.Forms.RadioButton();
            this.form_rad_Folder_Outermost = new System.Windows.Forms.RadioButton();
            this.form_rad_Folder_None = new System.Windows.Forms.RadioButton();
            this.split_Volumes = new System.Windows.Forms.SplitContainer();
            this.form_lv_SearchResults = new ListViewEx();
            this.form_lv_SearchResults_col_Path = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_SearchResults_col_Filename = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_SearchResults_col_Created = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_SearchResults_col_Modified = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader11 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_SearchResults_col_Length = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_SearchResults_col_Error1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_SearchResults_col_Error2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_PathErrors = new ListViewEx();
            this.form_lv_Errors_col_Volume = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_Errors_col_FileOrPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_Errors_col_Error1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lv_Errors_col_Error2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label3 = new System.Windows.Forms.Label();
            this.form_tabPage_Browse = new System.Windows.Forms.TabPage();
            this.split_Files = new System.Windows.Forms.SplitContainer();
            this.form_treeView_Browse = new System.Windows.Forms.TreeView();
            this.split_Detail = new System.Windows.Forms.SplitContainer();
            this.form_LV_Files = new ListViewEx();
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader10 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_LV_Detail = new ListViewEx();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.timer_killRed = new System.Windows.Forms.Timer(this.components);
            this.form_chk_SearchCase = new System.Windows.Forms.CheckBox();
            this.form_tabControl.SuspendLayout();
            this.form_tabPage_Volumes.SuspendLayout();
            this.form_tabPage_Search.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.split_Volumes)).BeginInit();
            this.split_Volumes.Panel1.SuspendLayout();
            this.split_Volumes.Panel2.SuspendLayout();
            this.split_Volumes.SuspendLayout();
            this.form_tabPage_Browse.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.split_Files)).BeginInit();
            this.split_Files.Panel1.SuspendLayout();
            this.split_Files.Panel2.SuspendLayout();
            this.split_Files.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.split_Detail)).BeginInit();
            this.split_Detail.Panel1.SuspendLayout();
            this.split_Detail.Panel2.SuspendLayout();
            this.split_Detail.SuspendLayout();
            this.SuspendLayout();
            // 
            // form_btn_Search
            // 
            this.form_btn_Search.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.form_btn_Search.Location = new System.Drawing.Point(641, 31);
            this.form_btn_Search.Name = "form_btn_Search";
            this.form_btn_Search.Size = new System.Drawing.Size(75, 23);
            this.form_btn_Search.TabIndex = 1;
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
            this.form_cb_Search.TabIndex = 0;
            this.form_cb_Search.SelectedIndexChanged += new System.EventHandler(this.cb_Search_SelectedIndexChanged);
            this.form_cb_Search.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.form_cb_Search_KeyPress);
            // 
            // form_tabControl
            // 
            this.form_tabControl.Controls.Add(this.form_tabPage_Volumes);
            this.form_tabControl.Controls.Add(this.form_tabPage_Search);
            this.form_tabControl.Controls.Add(this.form_tabPage_Browse);
            this.form_tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_tabControl.Location = new System.Drawing.Point(0, 0);
            this.form_tabControl.Name = "form_tabControl";
            this.form_tabControl.SelectedIndex = 0;
            this.form_tabControl.Size = new System.Drawing.Size(730, 382);
            this.form_tabControl.TabIndex = 0;
            // 
            // form_tabPage_Volumes
            // 
            this.form_tabPage_Volumes.Controls.Add(this.form_btn_LoadVolumeList);
            this.form_tabPage_Volumes.Controls.Add(this.form_btn_SaveVolumeList);
            this.form_tabPage_Volumes.Controls.Add(this.label5);
            this.form_tabPage_Volumes.Controls.Add(this.form_btn_RemoveVolume);
            this.form_tabPage_Volumes.Controls.Add(this.form_btn_ToggleInclude);
            this.form_tabPage_Volumes.Controls.Add(this.form_btn_AddVolume);
            this.form_tabPage_Volumes.Controls.Add(this.form_cb_VolumeName);
            this.form_tabPage_Volumes.Controls.Add(this.label4);
            this.form_tabPage_Volumes.Controls.Add(this.form_lv_Volumes);
            this.form_tabPage_Volumes.Controls.Add(this.form_btn_SavePathInfo);
            this.form_tabPage_Volumes.Controls.Add(this.form_btn_SaveAs);
            this.form_tabPage_Volumes.Controls.Add(this.label2);
            this.form_tabPage_Volumes.Controls.Add(this.form_btn_Path);
            this.form_tabPage_Volumes.Controls.Add(this.form_cb_SaveAs);
            this.form_tabPage_Volumes.Controls.Add(this.form_cb_Path);
            this.form_tabPage_Volumes.Controls.Add(this.label1);
            this.form_tabPage_Volumes.Location = new System.Drawing.Point(4, 22);
            this.form_tabPage_Volumes.Name = "form_tabPage_Volumes";
            this.form_tabPage_Volumes.Padding = new System.Windows.Forms.Padding(3);
            this.form_tabPage_Volumes.Size = new System.Drawing.Size(722, 356);
            this.form_tabPage_Volumes.TabIndex = 0;
            this.form_tabPage_Volumes.Text = "Volumes";
            this.form_tabPage_Volumes.UseVisualStyleBackColor = true;
            // 
            // form_btn_LoadVolumeList
            // 
            this.form_btn_LoadVolumeList.Location = new System.Drawing.Point(126, 143);
            this.form_btn_LoadVolumeList.Name = "form_btn_LoadVolumeList";
            this.form_btn_LoadVolumeList.Size = new System.Drawing.Size(51, 23);
            this.form_btn_LoadVolumeList.TabIndex = 7;
            this.form_btn_LoadVolumeList.Text = "Load";
            this.form_btn_LoadVolumeList.UseVisualStyleBackColor = true;
            this.form_btn_LoadVolumeList.Click += new System.EventHandler(this.form_btn_LoadVolumeList_Click);
            // 
            // form_btn_SaveVolumeList
            // 
            this.form_btn_SaveVolumeList.Location = new System.Drawing.Point(74, 143);
            this.form_btn_SaveVolumeList.Name = "form_btn_SaveVolumeList";
            this.form_btn_SaveVolumeList.Size = new System.Drawing.Size(46, 23);
            this.form_btn_SaveVolumeList.TabIndex = 6;
            this.form_btn_SaveVolumeList.Text = "Save";
            this.form_btn_SaveVolumeList.UseVisualStyleBackColor = true;
            this.form_btn_SaveVolumeList.Click += new System.EventHandler(this.form_btn_SaveVolumeList_Click);
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
            this.form_cb_VolumeName.Location = new System.Drawing.Point(111, 16);
            this.form_cb_VolumeName.Name = "form_cb_VolumeName";
            this.form_cb_VolumeName.Size = new System.Drawing.Size(599, 21);
            this.form_cb_VolumeName.TabIndex = 0;
            this.form_cb_VolumeName.SelectedIndexChanged += new System.EventHandler(this.form_cb_VolumeName_SelectedIndexChanged);
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
            this.form_lv_Volumes_col_Volume.Text = "Volume name";
            this.form_lv_Volumes_col_Volume.Width = 200;
            // 
            // form_lv_Volumes_col_Path
            // 
            this.form_lv_Volumes_col_Path.Text = "Path";
            this.form_lv_Volumes_col_Path.Width = 134;
            // 
            // form_lv_Volumes_col_SaveToFile
            // 
            this.form_lv_Volumes_col_SaveToFile.Text = "Save to file";
            this.form_lv_Volumes_col_SaveToFile.Width = 183;
            // 
            // form_lv_Volumes_col_Status
            // 
            this.form_lv_Volumes_col_Status.Text = "Status";
            this.form_lv_Volumes_col_Status.Width = 94;
            // 
            // form_lv_Volumes_col_IncludeInSearch
            // 
            this.form_lv_Volumes_col_IncludeInSearch.Text = "Include in search";
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
            this.label2.Size = new System.Drawing.Size(94, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Directory listing file";
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
            this.form_cb_SaveAs.Location = new System.Drawing.Point(114, 72);
            this.form_cb_SaveAs.Name = "form_cb_SaveAs";
            this.form_cb_SaveAs.Size = new System.Drawing.Size(562, 21);
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
            // form_tabPage_Search
            // 
            this.form_tabPage_Search.Controls.Add(this.form_chk_SearchCase);
            this.form_tabPage_Search.Controls.Add(this.form_btn_Search_FillPaths);
            this.form_tabPage_Search.Controls.Add(this.label6);
            this.form_tabPage_Search.Controls.Add(this.form_rad_Folder_Innermost);
            this.form_tabPage_Search.Controls.Add(this.form_rad_Folder_Outermost);
            this.form_tabPage_Search.Controls.Add(this.form_rad_Folder_None);
            this.form_tabPage_Search.Controls.Add(this.split_Volumes);
            this.form_tabPage_Search.Controls.Add(this.form_cb_Search);
            this.form_tabPage_Search.Controls.Add(this.form_btn_Search);
            this.form_tabPage_Search.Location = new System.Drawing.Point(4, 22);
            this.form_tabPage_Search.Name = "form_tabPage_Search";
            this.form_tabPage_Search.Padding = new System.Windows.Forms.Padding(3);
            this.form_tabPage_Search.Size = new System.Drawing.Size(722, 356);
            this.form_tabPage_Search.TabIndex = 1;
            this.form_tabPage_Search.Text = "Search";
            this.form_tabPage_Search.UseVisualStyleBackColor = true;
            // 
            // form_btn_Search_FillPaths
            // 
            this.form_btn_Search_FillPaths.Enabled = false;
            this.form_btn_Search_FillPaths.Location = new System.Drawing.Point(8, 31);
            this.form_btn_Search_FillPaths.Name = "form_btn_Search_FillPaths";
            this.form_btn_Search_FillPaths.Size = new System.Drawing.Size(75, 23);
            this.form_btn_Search_FillPaths.TabIndex = 7;
            this.form_btn_Search_FillPaths.Text = "Fill Paths";
            this.form_btn_Search_FillPaths.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(224, 36);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(118, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "Folder special handling:";
            // 
            // form_rad_Folder_Innermost
            // 
            this.form_rad_Folder_Innermost.AutoSize = true;
            this.form_rad_Folder_Innermost.Location = new System.Drawing.Point(484, 34);
            this.form_rad_Folder_Innermost.Name = "form_rad_Folder_Innermost";
            this.form_rad_Folder_Innermost.Size = new System.Drawing.Size(151, 17);
            this.form_rad_Folder_Innermost.TabIndex = 5;
            this.form_rad_Folder_Innermost.Text = "Innermost that have length";
            this.form_rad_Folder_Innermost.UseVisualStyleBackColor = true;
            // 
            // form_rad_Folder_Outermost
            // 
            this.form_rad_Folder_Outermost.AutoSize = true;
            this.form_rad_Folder_Outermost.Location = new System.Drawing.Point(405, 34);
            this.form_rad_Folder_Outermost.Name = "form_rad_Folder_Outermost";
            this.form_rad_Folder_Outermost.Size = new System.Drawing.Size(73, 17);
            this.form_rad_Folder_Outermost.TabIndex = 4;
            this.form_rad_Folder_Outermost.Text = "Outermost";
            this.form_rad_Folder_Outermost.UseVisualStyleBackColor = true;
            // 
            // form_rad_Folder_None
            // 
            this.form_rad_Folder_None.AutoSize = true;
            this.form_rad_Folder_None.Checked = true;
            this.form_rad_Folder_None.Location = new System.Drawing.Point(348, 34);
            this.form_rad_Folder_None.Name = "form_rad_Folder_None";
            this.form_rad_Folder_None.Size = new System.Drawing.Size(51, 17);
            this.form_rad_Folder_None.TabIndex = 3;
            this.form_rad_Folder_None.TabStop = true;
            this.form_rad_Folder_None.Text = "None";
            this.form_rad_Folder_None.UseVisualStyleBackColor = true;
            // 
            // split_Volumes
            // 
            this.split_Volumes.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.split_Volumes.Location = new System.Drawing.Point(-4, 60);
            this.split_Volumes.Name = "split_Volumes";
            this.split_Volumes.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // split_Volumes.Panel1
            // 
            this.split_Volumes.Panel1.Controls.Add(this.form_lv_SearchResults);
            // 
            // split_Volumes.Panel2
            // 
            this.split_Volumes.Panel2.Controls.Add(this.form_lv_PathErrors);
            this.split_Volumes.Panel2.Controls.Add(this.label3);
            this.split_Volumes.Size = new System.Drawing.Size(730, 300);
            this.split_Volumes.SplitterDistance = 224;
            this.split_Volumes.TabIndex = 6;
            // 
            // form_lv_SearchResults
            // 
            this.form_lv_SearchResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
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
            this.form_lv_SearchResults.Location = new System.Drawing.Point(0, 0);
            this.form_lv_SearchResults.MultiSelect = false;
            this.form_lv_SearchResults.Name = "form_lv_SearchResults";
            this.form_lv_SearchResults.Size = new System.Drawing.Size(730, 224);
            this.form_lv_SearchResults.TabIndex = 0;
            this.form_lv_SearchResults.UseCompatibleStateImageBehavior = false;
            this.form_lv_SearchResults.View = System.Windows.Forms.View.Details;
            // 
            // form_lv_SearchResults_col_Path
            // 
            this.form_lv_SearchResults_col_Path.Text = "Path";
            // 
            // form_lv_SearchResults_col_Filename
            // 
            this.form_lv_SearchResults_col_Filename.Text = "Filename";
            this.form_lv_SearchResults_col_Filename.Width = 101;
            // 
            // form_lv_SearchResults_col_Created
            // 
            this.form_lv_SearchResults_col_Created.Text = "Created";
            this.form_lv_SearchResults_col_Created.Width = 100;
            // 
            // form_lv_SearchResults_col_Modified
            // 
            this.form_lv_SearchResults_col_Modified.Text = "Modified";
            this.form_lv_SearchResults_col_Modified.Width = 100;
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
            this.form_lv_PathErrors.Location = new System.Drawing.Point(0, 16);
            this.form_lv_PathErrors.MultiSelect = false;
            this.form_lv_PathErrors.Name = "form_lv_PathErrors";
            this.form_lv_PathErrors.Size = new System.Drawing.Size(730, 56);
            this.form_lv_PathErrors.TabIndex = 0;
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
            this.label3.TabIndex = 1;
            this.label3.Text = "Path Errors";
            // 
            // form_tabPage_Browse
            // 
            this.form_tabPage_Browse.Controls.Add(this.split_Files);
            this.form_tabPage_Browse.Location = new System.Drawing.Point(4, 22);
            this.form_tabPage_Browse.Name = "form_tabPage_Browse";
            this.form_tabPage_Browse.Size = new System.Drawing.Size(722, 356);
            this.form_tabPage_Browse.TabIndex = 2;
            this.form_tabPage_Browse.Text = "Browse";
            this.form_tabPage_Browse.UseVisualStyleBackColor = true;
            this.form_tabPage_Browse.Paint += new System.Windows.Forms.PaintEventHandler(this.form_tabPage_Browse_Paint);
            // 
            // split_Files
            // 
            this.split_Files.Dock = System.Windows.Forms.DockStyle.Fill;
            this.split_Files.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.split_Files.Location = new System.Drawing.Point(0, 0);
            this.split_Files.Name = "split_Files";
            // 
            // split_Files.Panel1
            // 
            this.split_Files.Panel1.Controls.Add(this.form_treeView_Browse);
            // 
            // split_Files.Panel2
            // 
            this.split_Files.Panel2.Controls.Add(this.split_Detail);
            this.split_Files.Size = new System.Drawing.Size(722, 356);
            this.split_Files.SplitterDistance = 334;
            this.split_Files.TabIndex = 1;
            // 
            // form_treeView_Browse
            // 
            this.form_treeView_Browse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_treeView_Browse.FullRowSelect = true;
            this.form_treeView_Browse.HideSelection = false;
            this.form_treeView_Browse.Location = new System.Drawing.Point(0, 0);
            this.form_treeView_Browse.Name = "form_treeView_Browse";
            this.form_treeView_Browse.Size = new System.Drawing.Size(334, 356);
            this.form_treeView_Browse.TabIndex = 0;
            this.form_treeView_Browse.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.form_treeView_Browse_AfterSelect);
            // 
            // split_Detail
            // 
            this.split_Detail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.split_Detail.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.split_Detail.Location = new System.Drawing.Point(0, 0);
            this.split_Detail.Name = "split_Detail";
            this.split_Detail.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // split_Detail.Panel1
            // 
            this.split_Detail.Panel1.Controls.Add(this.form_LV_Files);
            // 
            // split_Detail.Panel2
            // 
            this.split_Detail.Panel2.Controls.Add(this.form_LV_Detail);
            this.split_Detail.Size = new System.Drawing.Size(384, 356);
            this.split_Detail.SplitterDistance = 169;
            this.split_Detail.TabIndex = 1;
            // 
            // form_LV_Files
            // 
            this.form_LV_Files.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader8,
            this.columnHeader9,
            this.columnHeader10});
            this.form_LV_Files.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_LV_Files.FullRowSelect = true;
            this.form_LV_Files.HideSelection = false;
            this.form_LV_Files.Location = new System.Drawing.Point(0, 0);
            this.form_LV_Files.MultiSelect = false;
            this.form_LV_Files.Name = "form_LV_Files";
            this.form_LV_Files.Size = new System.Drawing.Size(384, 169);
            this.form_LV_Files.TabIndex = 0;
            this.form_LV_Files.UseCompatibleStateImageBehavior = false;
            this.form_LV_Files.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Path";
            this.columnHeader3.Width = 0;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Filename";
            this.columnHeader4.Width = 200;
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
            // 
            // columnHeader9
            // 
            this.columnHeader9.Text = "Error 1";
            // 
            // columnHeader10
            // 
            this.columnHeader10.Text = "Error 2";
            // 
            // form_LV_Detail
            // 
            this.form_LV_Detail.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.form_LV_Detail.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_LV_Detail.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.form_LV_Detail.Location = new System.Drawing.Point(0, 0);
            this.form_LV_Detail.MultiSelect = false;
            this.form_LV_Detail.Name = "form_LV_Detail";
            this.form_LV_Detail.Scrollable = false;
            this.form_LV_Detail.Size = new System.Drawing.Size(384, 183);
            this.form_LV_Detail.TabIndex = 0;
            this.form_LV_Detail.UseCompatibleStateImageBehavior = false;
            this.form_LV_Detail.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Directory detail";
            this.columnHeader1.Width = 120;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "";
            this.columnHeader2.Width = 9999;
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
            // form_chk_SearchCase
            // 
            this.form_chk_SearchCase.AutoSize = true;
            this.form_chk_SearchCase.Checked = true;
            this.form_chk_SearchCase.CheckState = System.Windows.Forms.CheckState.Checked;
            this.form_chk_SearchCase.Location = new System.Drawing.Point(122, 35);
            this.form_chk_SearchCase.Name = "form_chk_SearchCase";
            this.form_chk_SearchCase.Size = new System.Drawing.Size(96, 17);
            this.form_chk_SearchCase.TabIndex = 8;
            this.form_chk_SearchCase.Text = "Case Sensitive";
            this.form_chk_SearchCase.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(730, 382);
            this.Controls.Add(this.form_tabControl);
            this.MinimumSize = new System.Drawing.Size(746, 420);
            this.Name = "Form1";
            this.Text = "SearchDirLists";
            this.form_tabControl.ResumeLayout(false);
            this.form_tabPage_Volumes.ResumeLayout(false);
            this.form_tabPage_Volumes.PerformLayout();
            this.form_tabPage_Search.ResumeLayout(false);
            this.form_tabPage_Search.PerformLayout();
            this.split_Volumes.Panel1.ResumeLayout(false);
            this.split_Volumes.Panel2.ResumeLayout(false);
            this.split_Volumes.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.split_Volumes)).EndInit();
            this.split_Volumes.ResumeLayout(false);
            this.form_tabPage_Browse.ResumeLayout(false);
            this.split_Files.Panel1.ResumeLayout(false);
            this.split_Files.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.split_Files)).EndInit();
            this.split_Files.ResumeLayout(false);
            this.split_Detail.Panel1.ResumeLayout(false);
            this.split_Detail.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.split_Detail)).EndInit();
            this.split_Detail.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button form_btn_Search;
        private System.Windows.Forms.ComboBox form_cb_Search;
        private System.Windows.Forms.TabControl form_tabControl;
        private System.Windows.Forms.TabPage form_tabPage_Volumes;
        private System.Windows.Forms.Button form_btn_ToggleInclude;
        private System.Windows.Forms.Button form_btn_AddVolume;
        private System.Windows.Forms.ComboBox form_cb_VolumeName;
        private System.Windows.Forms.Label label4;
        private ListViewEx form_lv_Volumes;
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
        private System.Windows.Forms.SplitContainer split_Volumes;
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
        private System.Windows.Forms.SplitContainer split_Files;
        private ListViewEx form_LV_Detail;
        private System.Windows.Forms.SplitContainer split_Detail;
        private ListViewEx form_LV_Files;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.ColumnHeader columnHeader10;
        private System.Windows.Forms.ColumnHeader columnHeader11;
        private System.Windows.Forms.Button form_btn_Search_FillPaths;
        private System.Windows.Forms.CheckBox form_chk_SearchCase;

    }
}

