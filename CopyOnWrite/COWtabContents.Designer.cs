namespace CopyOnWrite
{
    partial class COWtabContents
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.m_button_Remove = new System.Windows.Forms.Button();
            this.m_button_AddTab = new System.Windows.Forms.Button();
            this.m_label_Negative = new System.Windows.Forms.Label();
            this.m_button_Folder = new System.Windows.Forms.Button();
            this.m_checkBox_Disable = new System.Windows.Forms.CheckBox();
            this.m_fileSystemWatcher1 = new System.IO.FileSystemWatcher();
            this.m_folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.m_label_Folder = new System.Windows.Forms.Label();
            this.m_textBox_IsMatch = new System.Windows.Forms.TextBox();
            this.m_textBox_Negative = new System.Windows.Forms.TextBox();
            this.m_label_IsMatch = new System.Windows.Forms.Label();
            this.m_splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.m_textBox_Folder = new System.Windows.Forms.TextBox();
            this.button_AllWritable = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.m_fileSystemWatcher1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_splitContainer1)).BeginInit();
            this.m_splitContainer1.Panel1.SuspendLayout();
            this.m_splitContainer1.Panel2.SuspendLayout();
            this.m_splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_button_Remove
            // 
            this.m_button_Remove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_button_Remove.FlatAppearance.BorderSize = 0;
            this.m_button_Remove.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.m_button_Remove.Location = new System.Drawing.Point(517, 2);
            this.m_button_Remove.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.m_button_Remove.Name = "m_button_Remove";
            this.m_button_Remove.Size = new System.Drawing.Size(62, 22);
            this.m_button_Remove.TabIndex = 6;
            this.m_button_Remove.Text = "Remove";
            this.m_button_Remove.UseVisualStyleBackColor = true;
            this.m_button_Remove.Click += new System.EventHandler(this.button_Remove_Click);
            // 
            // m_button_AddTab
            // 
            this.m_button_AddTab.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_button_AddTab.FlatAppearance.BorderSize = 0;
            this.m_button_AddTab.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.m_button_AddTab.Location = new System.Drawing.Point(469, 2);
            this.m_button_AddTab.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.m_button_AddTab.Name = "m_button_AddTab";
            this.m_button_AddTab.Size = new System.Drawing.Size(59, 22);
            this.m_button_AddTab.TabIndex = 5;
            this.m_button_AddTab.Text = "Add Tab";
            this.m_button_AddTab.UseVisualStyleBackColor = true;
            this.m_button_AddTab.Click += new System.EventHandler(this.button_AddTab_Click);
            // 
            // m_label_Negative
            // 
            this.m_label_Negative.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_label_Negative.AutoSize = true;
            this.m_label_Negative.Location = new System.Drawing.Point(3, 3);
            this.m_label_Negative.Name = "m_label_Negative";
            this.m_label_Negative.Size = new System.Drawing.Size(11, 13);
            this.m_label_Negative.TabIndex = 1;
            this.m_label_Negative.Text = "-";
            // 
            // m_button_Folder
            // 
            this.m_button_Folder.Location = new System.Drawing.Point(13, 1);
            this.m_button_Folder.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.m_button_Folder.Name = "m_button_Folder";
            this.m_button_Folder.Size = new System.Drawing.Size(75, 22);
            this.m_button_Folder.TabIndex = 1;
            this.m_button_Folder.Text = "From Folder";
            this.m_button_Folder.UseVisualStyleBackColor = true;
            this.m_button_Folder.Click += new System.EventHandler(this.button_FromFolder_Click);
            // 
            // m_checkBox_Disable
            // 
            this.m_checkBox_Disable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_checkBox_Disable.AutoSize = true;
            this.m_checkBox_Disable.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.m_checkBox_Disable.Location = new System.Drawing.Point(406, 4);
            this.m_checkBox_Disable.Name = "m_checkBox_Disable";
            this.m_checkBox_Disable.Size = new System.Drawing.Size(57, 17);
            this.m_checkBox_Disable.TabIndex = 4;
            this.m_checkBox_Disable.Text = "Disable";
            this.m_checkBox_Disable.UseVisualStyleBackColor = true;
            this.m_checkBox_Disable.CheckedChanged += new System.EventHandler(this.checkBox_Disable_CheckedChanged);
            // 
            // m_fileSystemWatcher1
            // 
            this.m_fileSystemWatcher1.EnableRaisingEvents = true;
            this.m_fileSystemWatcher1.IncludeSubdirectories = true;
            this.m_fileSystemWatcher1.NotifyFilter = ((System.IO.NotifyFilters)((System.IO.NotifyFilters.LastWrite | System.IO.NotifyFilters.CreationTime)));
            this.m_fileSystemWatcher1.SynchronizingObject = this;
            this.m_fileSystemWatcher1.Changed += new System.IO.FileSystemEventHandler(this.fileSystemWatcher1_Changed);
            this.m_fileSystemWatcher1.Created += new System.IO.FileSystemEventHandler(this.fileSystemWatcher1_Created);
            this.m_fileSystemWatcher1.Renamed += new System.IO.RenamedEventHandler(this.fileSystemWatcher1_Renamed);
            // 
            // m_folderBrowserDialog1
            // 
            this.m_folderBrowserDialog1.RootFolder = System.Environment.SpecialFolder.DesktopDirectory;
            this.m_folderBrowserDialog1.ShowNewFolderButton = false;
            // 
            // m_label_Folder
            // 
            this.m_label_Folder.AutoSize = true;
            this.m_label_Folder.Location = new System.Drawing.Point(97, 6);
            this.m_label_Folder.Name = "m_label_Folder";
            this.m_label_Folder.Size = new System.Drawing.Size(0, 13);
            this.m_label_Folder.TabIndex = 3;
            this.m_label_Folder.DoubleClick += new System.EventHandler(this.label_FromFolder_DoubleClick);
            // 
            // m_textBox_IsMatch
            // 
            this.m_textBox_IsMatch.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_textBox_IsMatch.CausesValidation = false;
            this.m_textBox_IsMatch.Location = new System.Drawing.Point(20, 0);
            this.m_textBox_IsMatch.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.m_textBox_IsMatch.Name = "m_textBox_IsMatch";
            this.m_textBox_IsMatch.Size = new System.Drawing.Size(286, 20);
            this.m_textBox_IsMatch.TabIndex = 1;
            this.m_textBox_IsMatch.Leave += new System.EventHandler(this.m_textBox_IsMatch_Leave);
            // 
            // m_textBox_Negative
            // 
            this.m_textBox_Negative.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_textBox_Negative.CausesValidation = false;
            this.m_textBox_Negative.Location = new System.Drawing.Point(20, 0);
            this.m_textBox_Negative.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.m_textBox_Negative.Name = "m_textBox_Negative";
            this.m_textBox_Negative.Size = new System.Drawing.Size(241, 20);
            this.m_textBox_Negative.TabIndex = 0;
            // 
            // m_label_IsMatch
            // 
            this.m_label_IsMatch.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_label_IsMatch.AutoSize = true;
            this.m_label_IsMatch.Location = new System.Drawing.Point(-1, 3);
            this.m_label_IsMatch.Name = "m_label_IsMatch";
            this.m_label_IsMatch.Size = new System.Drawing.Size(15, 13);
            this.m_label_IsMatch.TabIndex = 0;
            this.m_label_IsMatch.Text = "+";
            // 
            // m_splitContainer1
            // 
            this.m_splitContainer1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.m_splitContainer1.Location = new System.Drawing.Point(0, 25);
            this.m_splitContainer1.Margin = new System.Windows.Forms.Padding(0);
            this.m_splitContainer1.Name = "m_splitContainer1";
            // 
            // m_splitContainer1.Panel1
            // 
            this.m_splitContainer1.Panel1.Controls.Add(this.m_label_Negative);
            this.m_splitContainer1.Panel1.Controls.Add(this.m_textBox_Negative);
            this.m_splitContainer1.Panel1MinSize = 15;
            // 
            // m_splitContainer1.Panel2
            // 
            this.m_splitContainer1.Panel2.Controls.Add(this.m_label_IsMatch);
            this.m_splitContainer1.Panel2.Controls.Add(this.m_textBox_IsMatch);
            this.m_splitContainer1.Panel2MinSize = 15;
            this.m_splitContainer1.Size = new System.Drawing.Size(574, 20);
            this.m_splitContainer1.SplitterDistance = 264;
            this.m_splitContainer1.TabIndex = 7;
            this.m_splitContainer1.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer1_SplitterMoved);
            this.m_splitContainer1.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer1_Paint);
            this.m_splitContainer1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.splitContainer1_MouseDown);
            this.m_splitContainer1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.splitContainer1_MouseMove);
            this.m_splitContainer1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.splitContainer1_MouseUp);
            // 
            // m_textBox_Folder
            // 
            this.m_textBox_Folder.AllowDrop = true;
            this.m_textBox_Folder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_textBox_Folder.Enabled = false;
            this.m_textBox_Folder.Location = new System.Drawing.Point(94, 2);
            this.m_textBox_Folder.Name = "m_textBox_Folder";
            this.m_textBox_Folder.Size = new System.Drawing.Size(306, 20);
            this.m_textBox_Folder.TabIndex = 2;
            this.m_textBox_Folder.Visible = false;
            this.m_textBox_Folder.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_FromFolder_KeyDown);
            this.m_textBox_Folder.Validated += new System.EventHandler(this.textBox_Folder_Validated);
            // 
            // button_AllWritable
            // 
            this.button_AllWritable.FlatAppearance.BorderSize = 0;
            this.button_AllWritable.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_AllWritable.Location = new System.Drawing.Point(0, 1);
            this.button_AllWritable.Name = "button_AllWritable";
            this.button_AllWritable.Size = new System.Drawing.Size(14, 22);
            this.button_AllWritable.TabIndex = 0;
            this.button_AllWritable.Text = "v";
            this.button_AllWritable.UseVisualStyleBackColor = true;
            this.button_AllWritable.Click += new System.EventHandler(this.button_AllWritable_Click);
            // 
            // COWtabContents
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
            this.Controls.Add(this.m_button_AddTab);
            this.Controls.Add(this.button_AllWritable);
            this.Controls.Add(this.m_textBox_Folder);
            this.Controls.Add(this.m_splitContainer1);
            this.Controls.Add(this.m_checkBox_Disable);
            this.Controls.Add(this.m_button_Remove);
            this.Controls.Add(this.m_label_Folder);
            this.Controls.Add(this.m_button_Folder);
            this.Font = new System.Drawing.Font("Tahoma", 8F);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.MinimumSize = new System.Drawing.Size(472, 45);
            this.Name = "COWtabContents";
            this.Size = new System.Drawing.Size(574, 45);
            this.Load += new System.EventHandler(this.COWtabContents_Load);
            ((System.ComponentModel.ISupportInitialize)(this.m_fileSystemWatcher1)).EndInit();
            this.m_splitContainer1.Panel1.ResumeLayout(false);
            this.m_splitContainer1.Panel1.PerformLayout();
            this.m_splitContainer1.Panel2.ResumeLayout(false);
            this.m_splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_splitContainer1)).EndInit();
            this.m_splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.Button m_button_Remove;
        private System.Windows.Forms.Button m_button_AddTab;
        private System.Windows.Forms.Label m_label_Negative;
        private System.Windows.Forms.TextBox m_textBox_IsMatch;
        private System.IO.FileSystemWatcher m_fileSystemWatcher1;
        private System.Windows.Forms.Label m_label_IsMatch;
        private System.Windows.Forms.TextBox m_textBox_Negative;
        private System.Windows.Forms.SplitContainer m_splitContainer1;

        private System.Windows.Forms.CheckBox m_checkBox_Disable;
        private System.Windows.Forms.Button m_button_Folder;
        private System.Windows.Forms.Label m_label_Folder;
        private System.Windows.Forms.TextBox m_textBox_Folder;
        private System.Windows.Forms.FolderBrowserDialog m_folderBrowserDialog1;
        private System.Windows.Forms.Button button_AllWritable;
    }
}
