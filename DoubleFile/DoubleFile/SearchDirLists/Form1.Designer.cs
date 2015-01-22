﻿using ListViewEmbeddedControls;

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
            System.Windows.Forms.Label label3;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.form_chkSpacer = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel3 = new SearchDirLists.Form1.Form1LayoutPanel(this.components);
            this.form_splitFiles = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.form_splitTreeFind = new System.Windows.Forms.SplitContainer();
            this.form_splitCompare = new System.Windows.Forms.SplitContainer();
            this.form_treeCompare1 = new SearchDirLists.SDL_TreeView();
            this.form_treeCompare2 = new SearchDirLists.SDL_TreeView();
            this.form_treeViewBrowse = new SearchDirLists.SDL_TreeView();
            this.form_tabControlCopyIgnore = new System.Windows.Forms.TabControl();
            this.form_tabPageCopy = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel4 = new SearchDirLists.Form1.Form1LayoutPanel(this.components);
            this.form_lvCopyScratchpad = new SearchDirLists.SDL_ListView();
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tableLayoutPanel5 = new SearchDirLists.Form1.Form1LayoutPanel(this.components);
            this.form_btnCopyClear = new System.Windows.Forms.Button();
            this.form_btnSaveCopyDirs = new System.Windows.Forms.Button();
            this.form_btnCopyGen = new System.Windows.Forms.Button();
            this.form_btnLoadCopyDirs = new System.Windows.Forms.Button();
            this.form_tabPageIgnore = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel6 = new SearchDirLists.Form1.Form1LayoutPanel(this.components);
            this.form_lvIgnoreList = new SearchDirLists.SDL_ListView();
            this.columnHeader11 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader14 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tableLayoutPanel7 = new SearchDirLists.Form1.Form1LayoutPanel(this.components);
            this.form_btnClearIgnoreList = new System.Windows.Forms.Button();
            this.form_btnIgnoreAdd = new System.Windows.Forms.Button();
            this.form_btnIgnoreDel = new System.Windows.Forms.Button();
            this.form_btnSaveIgnoreList = new System.Windows.Forms.Button();
            this.form_btnLoadIgnoreList = new System.Windows.Forms.Button();
            this.form_chkLoose = new System.Windows.Forms.CheckBox();
            this.form_splitClones = new System.Windows.Forms.SplitContainer();
            this.form_splitDetail = new System.Windows.Forms.SplitContainer();
            this.form_tabControlFileList = new System.Windows.Forms.TabControl();
            this.form_tabPageDiskUsage = new System.Windows.Forms.TabPage();
            this.form_tmapUserCtl = new SearchDirLists.TreeMapUserControl();
            this.form_tabPageFileList = new System.Windows.Forms.TabPage();
            this.form_splitCompareFiles = new System.Windows.Forms.SplitContainer();
            this.form_lvFiles = new SearchDirLists.SDL_ListView();
            this.form_colFilename = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader10 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lvFileCompare = new SearchDirLists.SDL_ListView();
            this.form_colFileCompare = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader19 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader20 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader21 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader22 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader23 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader24 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_splitDetailVols = new System.Windows.Forms.SplitContainer();
            this.form_lvDetail = new SearchDirLists.SDL_ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_colDirDetail = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lvDetailVol = new SearchDirLists.SDL_ListView();
            this.form_colDirDetailCompare = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_colVolDetail = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_splitNonClones = new System.Windows.Forms.SplitContainer();
            this.form_lvUnique = new SearchDirLists.SDL_ListView();
            this.columnHeader16 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_splitUnique = new System.Windows.Forms.SplitContainer();
            this.form_lvSameVol = new SearchDirLists.SDL_ListView();
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader15 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.form_lvClones = new SearchDirLists.SDL_ListView();
            this.columnHeader12 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader13 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tableLayoutPanel2 = new SearchDirLists.Form1.Form1LayoutPanel(this.components);
            this.form_btnCompare = new System.Windows.Forms.Button();
            this.form_btnCollapse = new System.Windows.Forms.Button();
            this.form_lblVolGroup = new System.Windows.Forms.Label();
            this.form_btnFiles = new System.Windows.Forms.Button();
            this.form_btnForward = new System.Windows.Forms.Button();
            this.form_btnFoldersAndFiles = new System.Windows.Forms.Button();
            this.form_btnBack = new System.Windows.Forms.Button();
            this.form_btnFolder = new System.Windows.Forms.Button();
            this.form_btnUp = new System.Windows.Forms.Button();
            this.form_btnCopyToClipboard = new System.Windows.Forms.Button();
            this.form_cbFindbox = new System.Windows.Forms.ComboBox();
            this.form_chkCompare1 = new System.Windows.Forms.CheckBox();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            label3 = new System.Windows.Forms.Label();
            this.tableLayoutPanel3.SuspendLayout();
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
            this.form_tabControlCopyIgnore.SuspendLayout();
            this.form_tabPageCopy.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.tableLayoutPanel5.SuspendLayout();
            this.form_tabPageIgnore.SuspendLayout();
            this.tableLayoutPanel6.SuspendLayout();
            this.tableLayoutPanel7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.form_splitClones)).BeginInit();
            this.form_splitClones.Panel1.SuspendLayout();
            this.form_splitClones.Panel2.SuspendLayout();
            this.form_splitClones.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.form_splitDetail)).BeginInit();
            this.form_splitDetail.Panel1.SuspendLayout();
            this.form_splitDetail.Panel2.SuspendLayout();
            this.form_splitDetail.SuspendLayout();
            this.form_tabControlFileList.SuspendLayout();
            this.form_tabPageDiskUsage.SuspendLayout();
            this.form_tabPageFileList.SuspendLayout();
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
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.BackColor = System.Drawing.Color.Transparent;
            label3.Dock = System.Windows.Forms.DockStyle.Fill;
            label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            label3.Location = new System.Drawing.Point(864, 0);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(49, 30);
            label3.TabIndex = 8;
            label3.Text = "Search:";
            label3.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // form_chkSpacer
            // 
            this.form_chkSpacer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.form_chkSpacer.AutoSize = true;
            this.form_chkSpacer.BackColor = System.Drawing.Color.Transparent;
            this.form_chkSpacer.CausesValidation = false;
            this.form_chkSpacer.Enabled = false;
            this.form_chkSpacer.ForeColor = System.Drawing.Color.Tomato;
            this.form_chkSpacer.Location = new System.Drawing.Point(3, 90);
            this.form_chkSpacer.Name = "form_chkSpacer";
            this.form_chkSpacer.Size = new System.Drawing.Size(55, 17);
            this.form_chkSpacer.TabIndex = 2;
            this.form_chkSpacer.Text = "Loose";
            this.form_chkSpacer.UseVisualStyleBackColor = false;
            this.form_chkSpacer.Paint += new System.Windows.Forms.PaintEventHandler(this.form_chkSpacer_Paint);
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.AutoSize = true;
            this.tableLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel3.ColumnCount = 1;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Controls.Add(this.form_splitFiles, 0, 1);
            this.tableLayoutPanel3.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 2;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(1188, 592);
            this.tableLayoutPanel3.TabIndex = 8;
            // 
            // form_splitFiles
            // 
            this.form_splitFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_splitFiles.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.form_splitFiles.Location = new System.Drawing.Point(3, 39);
            this.form_splitFiles.Name = "form_splitFiles";
            // 
            // form_splitFiles.Panel1
            // 
            this.form_splitFiles.Panel1.Controls.Add(this.splitContainer2);
            // 
            // form_splitFiles.Panel2
            // 
            this.form_splitFiles.Panel2.Controls.Add(this.form_splitClones);
            this.form_splitFiles.Size = new System.Drawing.Size(1182, 550);
            this.form_splitFiles.SplitterDistance = 360;
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
            this.splitContainer2.Panel2.Controls.Add(this.form_tabControlCopyIgnore);
            this.splitContainer2.Size = new System.Drawing.Size(360, 550);
            this.splitContainer2.SplitterDistance = 282;
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
            this.form_splitTreeFind.Panel2.Controls.Add(this.form_treeViewBrowse);
            this.form_splitTreeFind.Size = new System.Drawing.Size(360, 282);
            this.form_splitTreeFind.SplitterDistance = 101;
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
            this.form_splitCompare.Size = new System.Drawing.Size(150, 101);
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
            this.form_treeCompare1.Size = new System.Drawing.Size(72, 101);
            this.form_treeCompare1.TabIndex = 1;
            this.form_treeCompare1.Tag = "COMPARE 1";
            this.form_treeCompare1.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.form_treeViewBrowse_AfterCheck);
            this.form_treeCompare1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.form_treeView_AfterSelect);
            this.form_treeCompare1.Enter += new System.EventHandler(this.form_treeCompare_Enter);
            this.form_treeCompare1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.form_treeViewBrowse_MouseClick);
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
            this.form_treeCompare2.Size = new System.Drawing.Size(74, 101);
            this.form_treeCompare2.TabIndex = 2;
            this.form_treeCompare2.Tag = "COMPARE 2";
            this.form_treeCompare2.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.form_treeViewBrowse_AfterCheck);
            this.form_treeCompare2.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.form_treeView_AfterSelect);
            this.form_treeCompare2.Enter += new System.EventHandler(this.form_treeCompare_Enter);
            this.form_treeCompare2.MouseClick += new System.Windows.Forms.MouseEventHandler(this.form_treeViewBrowse_MouseClick);
            // 
            // form_treeViewBrowse
            // 
            this.form_treeViewBrowse.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.form_treeViewBrowse.CheckBoxes = true;
            this.form_treeViewBrowse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_treeViewBrowse.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.form_treeViewBrowse.FullRowSelect = true;
            this.form_treeViewBrowse.HideSelection = false;
            this.form_treeViewBrowse.Location = new System.Drawing.Point(0, 0);
            this.form_treeViewBrowse.Name = "form_treeViewBrowse";
            this.form_treeViewBrowse.ShowLines = false;
            this.form_treeViewBrowse.Size = new System.Drawing.Size(360, 282);
            this.form_treeViewBrowse.TabIndex = 0;
            this.form_treeViewBrowse.Tag = "MAIN TREEVIEW";
            this.form_treeViewBrowse.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.form_treeViewBrowse_AfterCheck);
            this.form_treeViewBrowse.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.form_treeView_AfterSelect);
            this.form_treeViewBrowse.Enter += new System.EventHandler(this.form_SetCopyToClipboardTree);
            this.form_treeViewBrowse.MouseClick += new System.Windows.Forms.MouseEventHandler(this.form_treeViewBrowse_MouseClick);
            // 
            // form_tabControlCopyIgnore
            // 
            this.form_tabControlCopyIgnore.Alignment = System.Windows.Forms.TabAlignment.Bottom;
            this.form_tabControlCopyIgnore.Controls.Add(this.form_tabPageCopy);
            this.form_tabControlCopyIgnore.Controls.Add(this.form_tabPageIgnore);
            this.form_tabControlCopyIgnore.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_tabControlCopyIgnore.Location = new System.Drawing.Point(0, 0);
            this.form_tabControlCopyIgnore.Multiline = true;
            this.form_tabControlCopyIgnore.Name = "form_tabControlCopyIgnore";
            this.form_tabControlCopyIgnore.SelectedIndex = 0;
            this.form_tabControlCopyIgnore.Size = new System.Drawing.Size(360, 264);
            this.form_tabControlCopyIgnore.TabIndex = 0;
            // 
            // form_tabPageCopy
            // 
            this.form_tabPageCopy.Controls.Add(this.tableLayoutPanel4);
            this.form_tabPageCopy.Location = new System.Drawing.Point(4, 4);
            this.form_tabPageCopy.Name = "form_tabPageCopy";
            this.form_tabPageCopy.Padding = new System.Windows.Forms.Padding(3);
            this.form_tabPageCopy.Size = new System.Drawing.Size(352, 238);
            this.form_tabPageCopy.TabIndex = 0;
            this.form_tabPageCopy.Text = "Copy scratchpad";
            this.form_tabPageCopy.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Controls.Add(this.form_lvCopyScratchpad, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.tableLayoutPanel5, 0, 0);
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 1;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(346, 232);
            this.tableLayoutPanel4.TabIndex = 3;
            // 
            // form_lvCopyScratchpad
            // 
            this.form_lvCopyScratchpad.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.form_lvCopyScratchpad.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3,
            this.columnHeader4});
            this.form_lvCopyScratchpad.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_lvCopyScratchpad.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.form_lvCopyScratchpad.FullRowSelect = true;
            this.form_lvCopyScratchpad.HideSelection = false;
            this.form_lvCopyScratchpad.Location = new System.Drawing.Point(70, 3);
            this.form_lvCopyScratchpad.MultiSelect = false;
            this.form_lvCopyScratchpad.Name = "form_lvCopyScratchpad";
            this.form_lvCopyScratchpad.Size = new System.Drawing.Size(273, 226);
            this.form_lvCopyScratchpad.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.form_lvCopyScratchpad.TabIndex = 1;
            this.form_lvCopyScratchpad.UseCompatibleStateImageBehavior = false;
            this.form_lvCopyScratchpad.View = System.Windows.Forms.View.Details;
            this.form_lvCopyScratchpad.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.form_lvTreeNodes_ColumnClick);
            this.form_lvCopyScratchpad.SelectedIndexChanged += new System.EventHandler(this.SelectedIndexChanged);
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
            // tableLayoutPanel5
            // 
            this.tableLayoutPanel5.AutoSize = true;
            this.tableLayoutPanel5.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel5.ColumnCount = 1;
            this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel5.Controls.Add(this.form_chkSpacer, 0, 0);
            this.tableLayoutPanel5.Controls.Add(this.form_btnCopyClear, 0, 4);
            this.tableLayoutPanel5.Controls.Add(this.form_btnSaveCopyDirs, 0, 3);
            this.tableLayoutPanel5.Controls.Add(this.form_btnCopyGen, 0, 1);
            this.tableLayoutPanel5.Controls.Add(this.form_btnLoadCopyDirs, 0, 2);
            this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel5.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel5.Name = "tableLayoutPanel5";
            this.tableLayoutPanel5.RowCount = 5;
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel5.Size = new System.Drawing.Size(61, 226);
            this.tableLayoutPanel5.TabIndex = 0;
            // 
            // form_btnCopyClear
            // 
            this.form_btnCopyClear.AutoSize = true;
            this.form_btnCopyClear.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.form_btnCopyClear.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_btnCopyClear.Location = new System.Drawing.Point(3, 200);
            this.form_btnCopyClear.Name = "form_btnCopyClear";
            this.form_btnCopyClear.Size = new System.Drawing.Size(55, 23);
            this.form_btnCopyClear.TabIndex = 3;
            this.form_btnCopyClear.Text = "Clear";
            this.form_btnCopyClear.UseVisualStyleBackColor = true;
            this.form_btnCopyClear.Click += new System.EventHandler(this.form_btnCopyScratchpadClear_Click);
            // 
            // form_btnSaveCopyDirs
            // 
            this.form_btnSaveCopyDirs.AutoSize = true;
            this.form_btnSaveCopyDirs.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.form_btnSaveCopyDirs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_btnSaveCopyDirs.Location = new System.Drawing.Point(3, 171);
            this.form_btnSaveCopyDirs.Name = "form_btnSaveCopyDirs";
            this.form_btnSaveCopyDirs.Size = new System.Drawing.Size(55, 23);
            this.form_btnSaveCopyDirs.TabIndex = 2;
            this.form_btnSaveCopyDirs.Text = "Save";
            this.form_btnSaveCopyDirs.UseVisualStyleBackColor = true;
            this.form_btnSaveCopyDirs.Click += new System.EventHandler(this.form_btnSaveCopyDirs_Click);
            // 
            // form_btnCopyGen
            // 
            this.form_btnCopyGen.AutoSize = true;
            this.form_btnCopyGen.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.form_btnCopyGen.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_btnCopyGen.Location = new System.Drawing.Point(3, 113);
            this.form_btnCopyGen.Name = "form_btnCopyGen";
            this.form_btnCopyGen.Size = new System.Drawing.Size(55, 23);
            this.form_btnCopyGen.TabIndex = 0;
            this.form_btnCopyGen.Text = "Script";
            this.form_btnCopyGen.UseVisualStyleBackColor = true;
            // 
            // form_btnLoadCopyDirs
            // 
            this.form_btnLoadCopyDirs.AutoSize = true;
            this.form_btnLoadCopyDirs.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.form_btnLoadCopyDirs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_btnLoadCopyDirs.Location = new System.Drawing.Point(3, 142);
            this.form_btnLoadCopyDirs.Name = "form_btnLoadCopyDirs";
            this.form_btnLoadCopyDirs.Size = new System.Drawing.Size(55, 23);
            this.form_btnLoadCopyDirs.TabIndex = 1;
            this.form_btnLoadCopyDirs.Text = "Load";
            this.form_btnLoadCopyDirs.UseVisualStyleBackColor = true;
            this.form_btnLoadCopyDirs.Click += new System.EventHandler(this.form_btnLoadCopyScratchpad_Click);
            // 
            // form_tabPageIgnore
            // 
            this.form_tabPageIgnore.Controls.Add(this.tableLayoutPanel6);
            this.form_tabPageIgnore.Location = new System.Drawing.Point(4, 4);
            this.form_tabPageIgnore.Name = "form_tabPageIgnore";
            this.form_tabPageIgnore.Padding = new System.Windows.Forms.Padding(3);
            this.form_tabPageIgnore.Size = new System.Drawing.Size(352, 238);
            this.form_tabPageIgnore.TabIndex = 1;
            this.form_tabPageIgnore.Text = "Ignore list";
            this.form_tabPageIgnore.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel6
            // 
            this.tableLayoutPanel6.ColumnCount = 2;
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.Controls.Add(this.form_lvIgnoreList, 1, 0);
            this.tableLayoutPanel6.Controls.Add(this.tableLayoutPanel7, 0, 0);
            this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel6.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel6.Name = "tableLayoutPanel6";
            this.tableLayoutPanel6.RowCount = 1;
            this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel6.Size = new System.Drawing.Size(346, 232);
            this.tableLayoutPanel6.TabIndex = 4;
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
            this.form_lvIgnoreList.Location = new System.Drawing.Point(70, 3);
            this.form_lvIgnoreList.Name = "form_lvIgnoreList";
            this.form_lvIgnoreList.Size = new System.Drawing.Size(273, 226);
            this.form_lvIgnoreList.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.form_lvIgnoreList.TabIndex = 0;
            this.form_lvIgnoreList.UseCompatibleStateImageBehavior = false;
            this.form_lvIgnoreList.View = System.Windows.Forms.View.Details;
            this.form_lvIgnoreList.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.form_lvTreeNodes_ColumnClick);
            // 
            // columnHeader11
            // 
            this.columnHeader11.Text = "Common folders to ignore";
            this.columnHeader11.Width = 200;
            // 
            // columnHeader14
            // 
            this.columnHeader14.Text = "Level";
            this.columnHeader14.Width = 50;
            // 
            // tableLayoutPanel7
            // 
            this.tableLayoutPanel7.AutoSize = true;
            this.tableLayoutPanel7.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel7.ColumnCount = 1;
            this.tableLayoutPanel7.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel7.Controls.Add(this.form_btnClearIgnoreList, 0, 7);
            this.tableLayoutPanel7.Controls.Add(this.form_btnIgnoreAdd, 0, 2);
            this.tableLayoutPanel7.Controls.Add(this.form_btnIgnoreDel, 0, 3);
            this.tableLayoutPanel7.Controls.Add(this.form_btnSaveIgnoreList, 0, 6);
            this.tableLayoutPanel7.Controls.Add(this.form_btnLoadIgnoreList, 0, 5);
            this.tableLayoutPanel7.Controls.Add(this.form_chkLoose, 0, 1);
            this.tableLayoutPanel7.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel7.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel7.Name = "tableLayoutPanel7";
            this.tableLayoutPanel7.RowCount = 8;
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel7.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel7.Size = new System.Drawing.Size(61, 226);
            this.tableLayoutPanel7.TabIndex = 0;
            // 
            // form_btnClearIgnoreList
            // 
            this.form_btnClearIgnoreList.AutoSize = true;
            this.form_btnClearIgnoreList.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.form_btnClearIgnoreList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_btnClearIgnoreList.Location = new System.Drawing.Point(3, 200);
            this.form_btnClearIgnoreList.Name = "form_btnClearIgnoreList";
            this.form_btnClearIgnoreList.Size = new System.Drawing.Size(55, 23);
            this.form_btnClearIgnoreList.TabIndex = 6;
            this.form_btnClearIgnoreList.Text = "Clear";
            this.form_btnClearIgnoreList.UseVisualStyleBackColor = true;
            this.form_btnClearIgnoreList.Click += new System.EventHandler(this.form_btnClearIgnoreList_Click);
            // 
            // form_btnIgnoreAdd
            // 
            this.form_btnIgnoreAdd.AutoSize = true;
            this.form_btnIgnoreAdd.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.form_btnIgnoreAdd.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_btnIgnoreAdd.Location = new System.Drawing.Point(3, 84);
            this.form_btnIgnoreAdd.Name = "form_btnIgnoreAdd";
            this.form_btnIgnoreAdd.Size = new System.Drawing.Size(55, 23);
            this.form_btnIgnoreAdd.TabIndex = 1;
            this.form_btnIgnoreAdd.Text = "Add";
            this.form_btnIgnoreAdd.UseVisualStyleBackColor = true;
            this.form_btnIgnoreAdd.Click += new System.EventHandler(this.form_btnIgnoreAdd_Click);
            // 
            // form_btnIgnoreDel
            // 
            this.form_btnIgnoreDel.AutoSize = true;
            this.form_btnIgnoreDel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.form_btnIgnoreDel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_btnIgnoreDel.Location = new System.Drawing.Point(3, 113);
            this.form_btnIgnoreDel.Name = "form_btnIgnoreDel";
            this.form_btnIgnoreDel.Size = new System.Drawing.Size(55, 23);
            this.form_btnIgnoreDel.TabIndex = 2;
            this.form_btnIgnoreDel.Text = "Del";
            this.form_btnIgnoreDel.UseVisualStyleBackColor = true;
            this.form_btnIgnoreDel.Click += new System.EventHandler(this.form_btnIgnoreDel_Click);
            // 
            // form_btnSaveIgnoreList
            // 
            this.form_btnSaveIgnoreList.AutoSize = true;
            this.form_btnSaveIgnoreList.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.form_btnSaveIgnoreList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_btnSaveIgnoreList.Location = new System.Drawing.Point(3, 171);
            this.form_btnSaveIgnoreList.Name = "form_btnSaveIgnoreList";
            this.form_btnSaveIgnoreList.Size = new System.Drawing.Size(55, 23);
            this.form_btnSaveIgnoreList.TabIndex = 5;
            this.form_btnSaveIgnoreList.Text = "Save";
            this.form_btnSaveIgnoreList.UseVisualStyleBackColor = true;
            this.form_btnSaveIgnoreList.Click += new System.EventHandler(this.form_btnSaveIgnoreList_Click);
            // 
            // form_btnLoadIgnoreList
            // 
            this.form_btnLoadIgnoreList.AutoSize = true;
            this.form_btnLoadIgnoreList.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.form_btnLoadIgnoreList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_btnLoadIgnoreList.Location = new System.Drawing.Point(3, 142);
            this.form_btnLoadIgnoreList.Name = "form_btnLoadIgnoreList";
            this.form_btnLoadIgnoreList.Size = new System.Drawing.Size(55, 23);
            this.form_btnLoadIgnoreList.TabIndex = 4;
            this.form_btnLoadIgnoreList.Text = "Load";
            this.form_btnLoadIgnoreList.UseVisualStyleBackColor = true;
            this.form_btnLoadIgnoreList.Click += new System.EventHandler(this.form_btnLoadIgnoreList_Click);
            // 
            // form_chkLoose
            // 
            this.form_chkLoose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.form_chkLoose.AutoSize = true;
            this.form_chkLoose.Location = new System.Drawing.Point(3, 61);
            this.form_chkLoose.Name = "form_chkLoose";
            this.form_chkLoose.Size = new System.Drawing.Size(55, 17);
            this.form_chkLoose.TabIndex = 0;
            this.form_chkLoose.Text = "Loose";
            this.form_chkLoose.UseVisualStyleBackColor = true;
            this.form_chkLoose.CheckedChanged += new System.EventHandler(this.form_chkLoose_CheckedChanged);
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
            this.form_splitClones.Size = new System.Drawing.Size(818, 550);
            this.form_splitClones.SplitterDistance = 557;
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
            this.form_splitDetail.Panel1.Controls.Add(this.form_tabControlFileList);
            // 
            // form_splitDetail.Panel2
            // 
            this.form_splitDetail.Panel2.Controls.Add(this.form_splitDetailVols);
            this.form_splitDetail.Size = new System.Drawing.Size(557, 550);
            this.form_splitDetail.SplitterDistance = 259;
            this.form_splitDetail.TabIndex = 1;
            // 
            // form_tabControlFileList
            // 
            this.form_tabControlFileList.Controls.Add(this.form_tabPageDiskUsage);
            this.form_tabControlFileList.Controls.Add(this.form_tabPageFileList);
            this.form_tabControlFileList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_tabControlFileList.Location = new System.Drawing.Point(0, 0);
            this.form_tabControlFileList.Name = "form_tabControlFileList";
            this.form_tabControlFileList.SelectedIndex = 0;
            this.form_tabControlFileList.Size = new System.Drawing.Size(557, 259);
            this.form_tabControlFileList.TabIndex = 0;
            this.form_tabControlFileList.SelectedIndexChanged += new System.EventHandler(this.ClearToolTip);
            // 
            // form_tabPageDiskUsage
            // 
            this.form_tabPageDiskUsage.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.form_tabPageDiskUsage.Controls.Add(this.form_tmapUserCtl);
            this.form_tabPageDiskUsage.Location = new System.Drawing.Point(4, 22);
            this.form_tabPageDiskUsage.Margin = new System.Windows.Forms.Padding(0);
            this.form_tabPageDiskUsage.Name = "form_tabPageDiskUsage";
            this.form_tabPageDiskUsage.Size = new System.Drawing.Size(549, 233);
            this.form_tabPageDiskUsage.TabIndex = 1;
            this.form_tabPageDiskUsage.Text = "Disk usage";
            this.form_tabPageDiskUsage.UseVisualStyleBackColor = true;
            // 
            // form_tmapUserCtl
            // 
            this.form_tmapUserCtl.BackColor = System.Drawing.Color.Transparent;
            this.form_tmapUserCtl.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.form_tmapUserCtl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_tmapUserCtl.Location = new System.Drawing.Point(0, 0);
            this.form_tmapUserCtl.Name = "form_tmapUserCtl";
            this.form_tmapUserCtl.Size = new System.Drawing.Size(549, 233);
            this.form_tmapUserCtl.TabIndex = 0;
            this.form_tmapUserCtl.Leave += new System.EventHandler(this.form_tmapUserCtl_Leave);
            this.form_tmapUserCtl.MouseDown += new System.Windows.Forms.MouseEventHandler(this.form_tmapUserCtl_MouseDown);
            this.form_tmapUserCtl.MouseUp += new System.Windows.Forms.MouseEventHandler(this.form_tmapUserCtl_MouseUp);
            this.form_tmapUserCtl.Resize += new System.EventHandler(this.ClearToolTip);
            // 
            // form_tabPageFileList
            // 
            this.form_tabPageFileList.Controls.Add(this.form_splitCompareFiles);
            this.form_tabPageFileList.Location = new System.Drawing.Point(4, 22);
            this.form_tabPageFileList.Name = "form_tabPageFileList";
            this.form_tabPageFileList.Padding = new System.Windows.Forms.Padding(3);
            this.form_tabPageFileList.Size = new System.Drawing.Size(549, 233);
            this.form_tabPageFileList.TabIndex = 0;
            this.form_tabPageFileList.Text = "Immediate files";
            this.form_tabPageFileList.UseVisualStyleBackColor = true;
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
            this.form_splitCompareFiles.Size = new System.Drawing.Size(543, 227);
            this.form_splitCompareFiles.SplitterDistance = 300;
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
            this.form_lvFiles.Size = new System.Drawing.Size(543, 227);
            this.form_lvFiles.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.form_lvFiles.TabIndex = 0;
            this.form_lvFiles.UseCompatibleStateImageBehavior = false;
            this.form_lvFiles.View = System.Windows.Forms.View.Details;
            this.form_lvFiles.Enter += new System.EventHandler(this.form_lvFiles_Enter);
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
            this.columnHeader7.Width = 100;
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
            this.form_lvFileCompare.Enter += new System.EventHandler(this.form_lvFiles_Enter);
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
            this.form_splitDetailVols.Size = new System.Drawing.Size(557, 287);
            this.form_splitDetailVols.SplitterDistance = 264;
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
            this.form_lvDetail.Size = new System.Drawing.Size(264, 287);
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
            this.form_lvDetailVol.Size = new System.Drawing.Size(289, 287);
            this.form_lvDetailVol.TabIndex = 0;
            this.form_lvDetailVol.UseCompatibleStateImageBehavior = false;
            this.form_lvDetailVol.View = System.Windows.Forms.View.Details;
            // 
            // form_colDirDetailCompare
            // 
            this.form_colDirDetailCompare.Text = "Volume detail";
            this.form_colDirDetailCompare.Width = 110;
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
            this.form_splitNonClones.Size = new System.Drawing.Size(257, 550);
            this.form_splitNonClones.SplitterDistance = 178;
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
            this.form_lvUnique.Size = new System.Drawing.Size(257, 178);
            this.form_lvUnique.TabIndex = 0;
            this.form_lvUnique.UseCompatibleStateImageBehavior = false;
            this.form_lvUnique.View = System.Windows.Forms.View.Details;
            this.form_lvUnique.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.form_lvTreeNodes_ColumnClick);
            this.form_lvUnique.SelectedIndexChanged += new System.EventHandler(this.SelectedIndexChanged);
            this.form_lvUnique.MouseClick += new System.Windows.Forms.MouseEventHandler(this.form_lvUnique_MouseClick);
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
            this.form_splitUnique.Size = new System.Drawing.Size(257, 368);
            this.form_splitUnique.SplitterDistance = 173;
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
            this.form_lvSameVol.Size = new System.Drawing.Size(257, 173);
            this.form_lvSameVol.TabIndex = 0;
            this.form_lvSameVol.UseCompatibleStateImageBehavior = false;
            this.form_lvSameVol.View = System.Windows.Forms.View.Details;
            this.form_lvSameVol.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.form_lvTreeNodes_ColumnClick);
            this.form_lvSameVol.SelectedIndexChanged += new System.EventHandler(this.form_lvClones_SelectedIndexChanged);
            this.form_lvSameVol.Enter += new System.EventHandler(this.form_lvClones_Enter);
            this.form_lvSameVol.KeyDown += new System.Windows.Forms.KeyEventHandler(this.form_lvClones_KeyDown);
            this.form_lvSameVol.KeyUp += new System.Windows.Forms.KeyEventHandler(this.form_lvClones_KeyUp);
            this.form_lvSameVol.MouseDown += new System.Windows.Forms.MouseEventHandler(this.form_lvClones_MouseDown);
            this.form_lvSameVol.MouseLeave += new System.EventHandler(this.form_lvClones_MouseLeave);
            this.form_lvSameVol.MouseUp += new System.Windows.Forms.MouseEventHandler(this.form_lvClones_MouseUp);
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
            this.form_lvClones.Size = new System.Drawing.Size(257, 191);
            this.form_lvClones.TabIndex = 0;
            this.form_lvClones.UseCompatibleStateImageBehavior = false;
            this.form_lvClones.View = System.Windows.Forms.View.Details;
            this.form_lvClones.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.form_lvTreeNodes_ColumnClick);
            this.form_lvClones.SelectedIndexChanged += new System.EventHandler(this.form_lvClones_SelectedIndexChanged);
            this.form_lvClones.Enter += new System.EventHandler(this.form_lvClones_Enter);
            this.form_lvClones.KeyDown += new System.Windows.Forms.KeyEventHandler(this.form_lvClones_KeyDown);
            this.form_lvClones.KeyUp += new System.Windows.Forms.KeyEventHandler(this.form_lvClones_KeyUp);
            this.form_lvClones.MouseClick += new System.Windows.Forms.MouseEventHandler(this.form_lvClones_MouseUp);
            this.form_lvClones.MouseDown += new System.Windows.Forms.MouseEventHandler(this.form_lvClones_MouseDown);
            this.form_lvClones.MouseLeave += new System.EventHandler(this.form_lvClones_MouseLeave);
            this.form_lvClones.MouseUp += new System.Windows.Forms.MouseEventHandler(this.form_lvClones_MouseUp);
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
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 13;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.form_btnCompare, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.form_btnCollapse, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.form_lblVolGroup, 12, 0);
            this.tableLayoutPanel2.Controls.Add(this.form_btnFiles, 11, 0);
            this.tableLayoutPanel2.Controls.Add(this.form_btnForward, 5, 0);
            this.tableLayoutPanel2.Controls.Add(this.form_btnFoldersAndFiles, 10, 0);
            this.tableLayoutPanel2.Controls.Add(this.form_btnBack, 4, 0);
            this.tableLayoutPanel2.Controls.Add(this.form_btnFolder, 9, 0);
            this.tableLayoutPanel2.Controls.Add(this.form_btnUp, 3, 0);
            this.tableLayoutPanel2.Controls.Add(label3, 8, 0);
            this.tableLayoutPanel2.Controls.Add(this.form_btnCopyToClipboard, 7, 0);
            this.tableLayoutPanel2.Controls.Add(this.form_cbFindbox, 6, 0);
            this.tableLayoutPanel2.Controls.Add(this.form_chkCompare1, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(1182, 30);
            this.tableLayoutPanel2.TabIndex = 13;
            // 
            // form_btnCompare
            // 
            this.form_btnCompare.AutoSize = true;
            this.form_btnCompare.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.form_btnCompare.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_btnCompare.Location = new System.Drawing.Point(149, 3);
            this.form_btnCompare.Name = "form_btnCompare";
            this.form_btnCompare.Size = new System.Drawing.Size(59, 24);
            this.form_btnCompare.TabIndex = 2;
            this.form_btnCompare.TabStop = false;
            this.form_btnCompare.Text = "Compare";
            this.form_btnCompare.UseVisualStyleBackColor = true;
            this.form_btnCompare.Click += new System.EventHandler(this.form_btnCompare_Click);
            // 
            // form_btnCollapse
            // 
            this.form_btnCollapse.AutoSize = true;
            this.form_btnCollapse.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.form_btnCollapse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_btnCollapse.Location = new System.Drawing.Point(3, 3);
            this.form_btnCollapse.Name = "form_btnCollapse";
            this.form_btnCollapse.Size = new System.Drawing.Size(57, 24);
            this.form_btnCollapse.TabIndex = 0;
            this.form_btnCollapse.TabStop = false;
            this.form_btnCollapse.Text = "Collapse";
            this.form_btnCollapse.UseVisualStyleBackColor = true;
            this.form_btnCollapse.Click += new System.EventHandler(this.form_btnTreeCollapse_Click);
            // 
            // form_lblVolGroup
            // 
            this.form_lblVolGroup.AutoSize = true;
            this.form_lblVolGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_lblVolGroup.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.form_lblVolGroup.Location = new System.Drawing.Point(1107, 0);
            this.form_lblVolGroup.Name = "form_lblVolGroup";
            this.form_lblVolGroup.Size = new System.Drawing.Size(72, 30);
            this.form_lblVolGroup.TabIndex = 12;
            this.form_lblVolGroup.Text = "Volume group";
            this.form_lblVolGroup.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // form_btnFiles
            // 
            this.form_btnFiles.AutoSize = true;
            this.form_btnFiles.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.form_btnFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_btnFiles.Location = new System.Drawing.Point(1063, 3);
            this.form_btnFiles.Name = "form_btnFiles";
            this.form_btnFiles.Size = new System.Drawing.Size(38, 24);
            this.form_btnFiles.TabIndex = 11;
            this.form_btnFiles.Text = "Files";
            this.form_btnFiles.UseVisualStyleBackColor = true;
            this.form_btnFiles.Click += new System.EventHandler(this.form_btnFind_Click);
            // 
            // form_btnForward
            // 
            this.form_btnForward.AutoSize = true;
            this.form_btnForward.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.form_btnForward.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_btnForward.Location = new System.Drawing.Point(314, 3);
            this.form_btnForward.Name = "form_btnForward";
            this.form_btnForward.Size = new System.Drawing.Size(44, 24);
            this.form_btnForward.TabIndex = 5;
            this.form_btnForward.TabStop = false;
            this.form_btnForward.Text = "-->";
            this.form_btnForward.UseVisualStyleBackColor = true;
            this.form_btnForward.Click += new System.EventHandler(this.form_btnForward_Click);
            // 
            // form_btnFoldersAndFiles
            // 
            this.form_btnFoldersAndFiles.AutoSize = true;
            this.form_btnFoldersAndFiles.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.form_btnFoldersAndFiles.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_btnFoldersAndFiles.Location = new System.Drawing.Point(976, 3);
            this.form_btnFoldersAndFiles.Name = "form_btnFoldersAndFiles";
            this.form_btnFoldersAndFiles.Size = new System.Drawing.Size(81, 24);
            this.form_btnFoldersAndFiles.TabIndex = 10;
            this.form_btnFoldersAndFiles.Text = "Folders && files";
            this.form_btnFoldersAndFiles.UseVisualStyleBackColor = true;
            this.form_btnFoldersAndFiles.Click += new System.EventHandler(this.form_btnFind_Click);
            // 
            // form_btnBack
            // 
            this.form_btnBack.AutoSize = true;
            this.form_btnBack.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.form_btnBack.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_btnBack.Location = new System.Drawing.Point(264, 3);
            this.form_btnBack.Name = "form_btnBack";
            this.form_btnBack.Size = new System.Drawing.Size(44, 24);
            this.form_btnBack.TabIndex = 4;
            this.form_btnBack.TabStop = false;
            this.form_btnBack.Text = "<--";
            this.form_btnBack.UseVisualStyleBackColor = true;
            this.form_btnBack.Click += new System.EventHandler(this.form_btnBack_Click);
            // 
            // form_btnFolder
            // 
            this.form_btnFolder.AutoSize = true;
            this.form_btnFolder.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.form_btnFolder.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_btnFolder.Location = new System.Drawing.Point(919, 3);
            this.form_btnFolder.Name = "form_btnFolder";
            this.form_btnFolder.Size = new System.Drawing.Size(51, 24);
            this.form_btnFolder.TabIndex = 9;
            this.form_btnFolder.Text = "Folders";
            this.form_btnFolder.UseVisualStyleBackColor = true;
            this.form_btnFolder.Click += new System.EventHandler(this.form_btnFind_Click);
            // 
            // form_btnUp
            // 
            this.form_btnUp.AutoSize = true;
            this.form_btnUp.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.form_btnUp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_btnUp.Location = new System.Drawing.Point(214, 3);
            this.form_btnUp.Name = "form_btnUp";
            this.form_btnUp.Size = new System.Drawing.Size(44, 24);
            this.form_btnUp.TabIndex = 3;
            this.form_btnUp.TabStop = false;
            this.form_btnUp.Text = "Up";
            this.form_btnUp.UseVisualStyleBackColor = true;
            this.form_btnUp.Click += new System.EventHandler(this.form_btnUp_Click);
            // 
            // form_btnCopyToClipboard
            // 
            this.form_btnCopyToClipboard.AutoSize = true;
            this.form_btnCopyToClipboard.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.form_btnCopyToClipboard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_btnCopyToClipboard.Location = new System.Drawing.Point(817, 3);
            this.form_btnCopyToClipboard.Name = "form_btnCopyToClipboard";
            this.form_btnCopyToClipboard.Size = new System.Drawing.Size(41, 24);
            this.form_btnCopyToClipboard.TabIndex = 7;
            this.form_btnCopyToClipboard.Text = "Copy";
            this.form_btnCopyToClipboard.UseVisualStyleBackColor = true;
            this.form_btnCopyToClipboard.Click += new System.EventHandler(this.form_btnCopyToClipBoard_Click);
            // 
            // form_cbFindbox
            // 
            this.form_cbFindbox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_cbFindbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.form_cbFindbox.FormattingEnabled = true;
            this.form_cbFindbox.Location = new System.Drawing.Point(364, 3);
            this.form_cbFindbox.Name = "form_cbFindbox";
            this.form_cbFindbox.Size = new System.Drawing.Size(447, 24);
            this.form_cbFindbox.TabIndex = 6;
            this.form_cbFindbox.DropDown += new System.EventHandler(this.form_cbFindbox_DropDown);
            this.form_cbFindbox.SelectedIndexChanged += new System.EventHandler(this.form_cbFindbox_SelectedIndexChanged);
            this.form_cbFindbox.TextChanged += new System.EventHandler(this.form_cbFindbox_TextChanged);
            this.form_cbFindbox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.form_cbFindbox_KeyUp);
            this.form_cbFindbox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.form_cbFindbox_MouseUp);
            // 
            // form_chkCompare1
            // 
            this.form_chkCompare1.AutoSize = true;
            this.form_chkCompare1.BackColor = System.Drawing.Color.Transparent;
            this.form_chkCompare1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.form_chkCompare1.Location = new System.Drawing.Point(66, 3);
            this.form_chkCompare1.Name = "form_chkCompare1";
            this.form_chkCompare1.Size = new System.Drawing.Size(77, 24);
            this.form_chkCompare1.TabIndex = 1;
            this.form_chkCompare1.TabStop = false;
            this.form_chkCompare1.Text = "Compare 1";
            this.form_chkCompare1.UseVisualStyleBackColor = true;
            this.form_chkCompare1.CheckedChanged += new System.EventHandler(this.form_chk_Compare1_CheckedChanged);
            // 
            // Form1
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(1196, 621);
            this.Controls.Add(this.tableLayoutPanel3);
            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(875, 420);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Search Directory Listings";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Activated += new System.EventHandler(this.ClearToolTip);
            this.Deactivate += new System.EventHandler(this.ClearToolTip);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Enter += new System.EventHandler(this.ClearToolTip);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyDown);
            this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.Form1_KeyPress);
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyUp);
            this.Leave += new System.EventHandler(this.ClearToolTip);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
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
            this.form_tabControlCopyIgnore.ResumeLayout(false);
            this.form_tabPageCopy.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.tableLayoutPanel4.PerformLayout();
            this.tableLayoutPanel5.ResumeLayout(false);
            this.tableLayoutPanel5.PerformLayout();
            this.form_tabPageIgnore.ResumeLayout(false);
            this.tableLayoutPanel6.ResumeLayout(false);
            this.tableLayoutPanel6.PerformLayout();
            this.tableLayoutPanel7.ResumeLayout(false);
            this.tableLayoutPanel7.PerformLayout();
            this.form_splitClones.Panel1.ResumeLayout(false);
            this.form_splitClones.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.form_splitClones)).EndInit();
            this.form_splitClones.ResumeLayout(false);
            this.form_splitDetail.Panel1.ResumeLayout(false);
            this.form_splitDetail.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.form_splitDetail)).EndInit();
            this.form_splitDetail.ResumeLayout(false);
            this.form_tabControlFileList.ResumeLayout(false);
            this.form_tabPageDiskUsage.ResumeLayout(false);
            this.form_tabPageFileList.ResumeLayout(false);
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
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        public SDL_TreeView form_treeViewBrowse;
        private System.Windows.Forms.SplitContainer form_splitFiles;
        private SDL_ListView form_lvDetail;
        private System.Windows.Forms.SplitContainer form_splitDetail;
        private SDL_ListView form_lvFiles;
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
        private SDL_ListView form_lvClones;
        private System.Windows.Forms.ColumnHeader columnHeader12;
        private System.Windows.Forms.ColumnHeader columnHeader13;
        private System.Windows.Forms.SplitContainer form_splitDetailVols;
        private SDL_ListView form_lvDetailVol;
        private System.Windows.Forms.ColumnHeader form_colDirDetailCompare;
        private System.Windows.Forms.ColumnHeader form_colVolDetail;
        private System.Windows.Forms.Button form_btnFolder;
        private System.Windows.Forms.Button form_btnCollapse;
        private System.Windows.Forms.ComboBox form_cbFindbox;
        private System.Windows.Forms.SplitContainer form_splitUnique;
        private SDL_ListView form_lvUnique;
        private System.Windows.Forms.ColumnHeader columnHeader16;
        private System.Windows.Forms.CheckBox form_chkCompare1;
        private System.Windows.Forms.Button form_btnCompare;
        private System.Windows.Forms.SplitContainer form_splitTreeFind;
        public SDL_TreeView form_treeCompare1;
        private System.Windows.Forms.SplitContainer form_splitCompare;
        public SDL_TreeView form_treeCompare2;
        private System.Windows.Forms.SplitContainer form_splitCompareFiles;
        private SDL_ListView form_lvFileCompare;
        private System.Windows.Forms.ColumnHeader form_colFileCompare;
        private System.Windows.Forms.ColumnHeader columnHeader19;
        private System.Windows.Forms.ColumnHeader columnHeader20;
        private System.Windows.Forms.ColumnHeader columnHeader21;
        private System.Windows.Forms.ColumnHeader columnHeader22;
        private System.Windows.Forms.ColumnHeader columnHeader23;
        private System.Windows.Forms.ColumnHeader columnHeader24;
        private System.Windows.Forms.Button form_btnCopyToClipboard;
        private System.Windows.Forms.Label form_lblVolGroup;
        private System.Windows.Forms.SplitContainer form_splitNonClones;
        private SDL_ListView form_lvSameVol;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private SDL_ListView form_lvCopyScratchpad;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Button form_btnLoadCopyDirs;
        private System.Windows.Forms.Button form_btnSaveCopyDirs;
        private System.Windows.Forms.Button form_btnCopyGen;
        private System.Windows.Forms.Button form_btnCopyClear;
        private System.Windows.Forms.Button form_btnFoldersAndFiles;
        private System.Windows.Forms.Button form_btnFiles;
        private System.Windows.Forms.TabControl form_tabControlCopyIgnore;
        private System.Windows.Forms.TabPage form_tabPageCopy;
        private System.Windows.Forms.TabPage form_tabPageIgnore;
        private System.Windows.Forms.Button form_btnSaveIgnoreList;
        private System.Windows.Forms.Button form_btnLoadIgnoreList;
        private SDL_ListView form_lvIgnoreList;
        private System.Windows.Forms.ColumnHeader columnHeader11;
        private System.Windows.Forms.ColumnHeader columnHeader14;
        private System.Windows.Forms.Button form_btnIgnoreAdd;
        private System.Windows.Forms.Button form_btnIgnoreDel;
        private System.Windows.Forms.CheckBox form_chkLoose;
        private System.Windows.Forms.ColumnHeader columnHeader15;
        private System.Windows.Forms.TabControl form_tabControlFileList;
        private System.Windows.Forms.TabPage form_tabPageFileList;
        private System.Windows.Forms.TabPage form_tabPageDiskUsage;
        private TreeMapUserControl form_tmapUserCtl;
        private System.Windows.Forms.Button form_btnUp;
        private System.Windows.Forms.Button form_btnForward;
        private System.Windows.Forms.Button form_btnBack;
        private Form1.Form1LayoutPanel tableLayoutPanel2;
        private Form1.Form1LayoutPanel tableLayoutPanel3;
        private Form1.Form1LayoutPanel tableLayoutPanel4;
        private Form1.Form1LayoutPanel tableLayoutPanel5;
        private Form1.Form1LayoutPanel tableLayoutPanel6;
        private Form1.Form1LayoutPanel tableLayoutPanel7;
        private System.Windows.Forms.Button form_btnClearIgnoreList;
        private System.Windows.Forms.CheckBox form_chkSpacer;
    }
}
