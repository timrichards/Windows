namespace CopyOnWrite
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
            this.m_timer_ChangeIcon = new System.Windows.Forms.Timer(this.components);
            this.m_timer_ChangeTitle = new System.Windows.Forms.Timer(this.components);
            this.m_button_Folder = new System.Windows.Forms.Button();
            this.m_listView1 = new System.Windows.Forms.ListView();
            this.m_lvcolFileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.m_lvcolFullPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.m_lvcolTimeStamp = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.m_lvcolTab = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.m_timer_CopyAttempt = new System.Windows.Forms.Timer(this.components);
            this.m_timer_reset_short = new System.Windows.Forms.Timer(this.components);
            this.m_timer_reset_long = new System.Windows.Forms.Timer(this.components);
            this.m_timer_reset = new System.Windows.Forms.Timer(this.components);
            this.m_tabControl1 = new System.Windows.Forms.TabControl();
            this.m_folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.m_label_Folder = new System.Windows.Forms.Label();
            this.m_textBox_Folder = new System.Windows.Forms.TextBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.m_label_Version = new System.Windows.Forms.Label();
            this.m_label2 = new System.Windows.Forms.Label();
            this.m_checkBox_Disable = new System.Windows.Forms.CheckBox();
            this.m_label_Monitoring = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_timer_ChangeIcon
            // 
            this.m_timer_ChangeIcon.Interval = 15000;
            this.m_timer_ChangeIcon.Tick += new System.EventHandler(this.timer_ChangeIcon_Tick);
            // 
            // m_timer_ChangeTitle
            // 
            this.m_timer_ChangeTitle.Interval = 1000;
            this.m_timer_ChangeTitle.Tick += new System.EventHandler(this.timer_ChangeTitle_Tick);
            // 
            // m_button_Folder
            // 
            this.m_button_Folder.Location = new System.Drawing.Point(0, 0);
            this.m_button_Folder.Name = "m_button_Folder";
            this.m_button_Folder.Size = new System.Drawing.Size(60, 19);
            this.m_button_Folder.TabIndex = 1;
            this.m_button_Folder.Text = "To Folder";
            this.m_button_Folder.UseVisualStyleBackColor = true;
            this.m_button_Folder.Click += new System.EventHandler(this.button_ToFolder_Click);
            // 
            // m_listView1
            // 
            this.m_listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_listView1.BackColor = System.Drawing.Color.Black;
            this.m_listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.m_lvcolFileName,
            this.m_lvcolFullPath,
            this.m_lvcolTimeStamp,
            this.m_lvcolTab});
            this.m_listView1.ForeColor = System.Drawing.Color.White;
            this.m_listView1.FullRowSelect = true;
            this.m_listView1.Location = new System.Drawing.Point(0, 18);
            this.m_listView1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.m_listView1.Name = "m_listView1";
            this.m_listView1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.m_listView1.ShowItemToolTips = true;
            this.m_listView1.Size = new System.Drawing.Size(549, 240);
            this.m_listView1.TabIndex = 7;
            this.m_listView1.UseCompatibleStateImageBehavior = false;
            this.m_listView1.View = System.Windows.Forms.View.Details;
            // 
            // m_lvcolFileName
            // 
            this.m_lvcolFileName.Text = "File Name";
            this.m_lvcolFileName.Width = global::CopyOnWrite.Properties.Settings.Default.Col_FileName_Width;
            // 
            // m_lvcolFullPath
            // 
            this.m_lvcolFullPath.Text = "Path";
            this.m_lvcolFullPath.Width = global::CopyOnWrite.Properties.Settings.Default.Col_FullPath_Width;
            // 
            // m_lvcolTimeStamp
            // 
            this.m_lvcolTimeStamp.Text = "Time";
            this.m_lvcolTimeStamp.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.m_lvcolTimeStamp.Width = global::CopyOnWrite.Properties.Settings.Default.Col_TimeStamp_Width;
            // 
            // m_lvcolTab
            // 
            this.m_lvcolTab.Text = "Tab";
            this.m_lvcolTab.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.m_lvcolTab.Width = global::CopyOnWrite.Properties.Settings.Default.Col_Tab_Width;
            // 
            // m_timer_CopyAttempt
            // 
            this.m_timer_CopyAttempt.Interval = 5000;
            this.m_timer_CopyAttempt.Tick += new System.EventHandler(this.timer_CopyAttempt_Tick);
            // 
            // m_timer_reset_short
            // 
            this.m_timer_reset_short.Interval = 5000;
            this.m_timer_reset_short.Tick += new System.EventHandler(this.timer_reset_short_tick);
            // 
            // m_timer_reset_long
            // 
            this.m_timer_reset_long.Interval = 300000;
            this.m_timer_reset_long.Tick += new System.EventHandler(this.timer_reset_long_tick);
            // 
            // m_timer_reset
            // 
            this.m_timer_reset.Tick += new System.EventHandler(this.timer_reset_Tick);
            // 
            // m_tabControl1
            // 
            this.m_tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_tabControl1.Location = new System.Drawing.Point(0, 0);
            this.m_tabControl1.Margin = new System.Windows.Forms.Padding(0);
            this.m_tabControl1.Name = "m_tabControl1";
            this.m_tabControl1.SelectedIndex = 0;
            this.m_tabControl1.Size = new System.Drawing.Size(549, 70);
            this.m_tabControl1.TabIndex = 0;
            this.m_tabControl1.SelectedIndexChanged += new System.EventHandler(this.m_tabControl1_SelectedIndexChanged);
            // 
            // m_label_Folder
            // 
            this.m_label_Folder.AutoSize = true;
            this.m_label_Folder.Location = new System.Drawing.Point(66, 3);
            this.m_label_Folder.Name = "m_label_Folder";
            this.m_label_Folder.Size = new System.Drawing.Size(43, 13);
            this.m_label_Folder.TabIndex = 2;
            this.m_label_Folder.Text = global::CopyOnWrite.Properties.Settings.Default.ToFolder;
            this.m_label_Folder.DoubleClick += new System.EventHandler(this.label_ToFolder_DoubleClick);
            // 
            // m_textBox_Folder
            // 
            this.m_textBox_Folder.AllowDrop = true;
            this.m_textBox_Folder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_textBox_Folder.Enabled = false;
            this.m_textBox_Folder.Location = new System.Drawing.Point(66, 0);
            this.m_textBox_Folder.Name = "m_textBox_Folder";
            this.m_textBox_Folder.Size = new System.Drawing.Size(187, 20);
            this.m_textBox_Folder.TabIndex = 8;
            this.m_textBox_Folder.Visible = false;
            this.m_textBox_Folder.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_ToFolder_KeyDown);
            this.m_textBox_Folder.Validated += new System.EventHandler(this.textBox_ToFolder_Validated);
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
            this.splitContainer1.Panel1.Controls.Add(this.m_tabControl1);
            this.splitContainer1.Panel1MinSize = 70;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.m_label_Version);
            this.splitContainer1.Panel2.Controls.Add(this.m_label2);
            this.splitContainer1.Panel2.Controls.Add(this.m_checkBox_Disable);
            this.splitContainer1.Panel2.Controls.Add(this.m_label_Monitoring);
            this.splitContainer1.Panel2.Controls.Add(this.m_listView1);
            this.splitContainer1.Panel2.Controls.Add(this.m_textBox_Folder);
            this.splitContainer1.Panel2.Controls.Add(this.m_label_Folder);
            this.splitContainer1.Panel2.Controls.Add(this.m_button_Folder);
            this.splitContainer1.Panel2MinSize = 100;
            this.splitContainer1.Size = new System.Drawing.Size(549, 332);
            this.splitContainer1.SplitterDistance = 70;
            this.splitContainer1.TabIndex = 9;
            // 
            // m_label_Version
            // 
            this.m_label_Version.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_label_Version.AutoSize = true;
            this.m_label_Version.Location = new System.Drawing.Point(359, 0);
            this.m_label_Version.Name = "m_label_Version";
            this.m_label_Version.Size = new System.Drawing.Size(53, 13);
            this.m_label_Version.TabIndex = 12;
            this.m_label_Version.Text = "{ debug }";
            // 
            // m_label2
            // 
            this.m_label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_label2.AutoSize = true;
            this.m_label2.Location = new System.Drawing.Point(259, 0);
            this.m_label2.Name = "m_label2";
            this.m_label2.Size = new System.Drawing.Size(94, 13);
            this.m_label2.TabIndex = 11;
            this.m_label2.Text = "(c) \'13 T. Richards";
            // 
            // m_checkBox_Disable
            // 
            this.m_checkBox_Disable.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_checkBox_Disable.AutoSize = true;
            this.m_checkBox_Disable.Location = new System.Drawing.Point(489, -1);
            this.m_checkBox_Disable.Name = "m_checkBox_Disable";
            this.m_checkBox_Disable.Size = new System.Drawing.Size(60, 17);
            this.m_checkBox_Disable.TabIndex = 9;
            this.m_checkBox_Disable.Text = "Disable";
            this.m_checkBox_Disable.UseVisualStyleBackColor = true;
            this.m_checkBox_Disable.CheckedChanged += new System.EventHandler(this.checkBox_Disable_CheckedChanged);
            // 
            // m_label_Monitoring
            // 
            this.m_label_Monitoring.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_label_Monitoring.AutoSize = true;
            this.m_label_Monitoring.Location = new System.Drawing.Point(418, 0);
            this.m_label_Monitoring.Name = "m_label_Monitoring";
            this.m_label_Monitoring.Size = new System.Drawing.Size(65, 13);
            this.m_label_Monitoring.TabIndex = 10;
            this.m_label_Monitoring.Text = "[Monitoring]";
            this.m_label_Monitoring.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(549, 332);
            this.Controls.Add(this.splitContainer1);
            this.Font = new System.Drawing.Font("Tahoma", 8F);
            this.MinimumSize = new System.Drawing.Size(540, 286);
            this.Name = "Form1";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.Text = "Copy On Write";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer m_timer_ChangeIcon;
        private System.Windows.Forms.Timer m_timer_ChangeTitle;
        private System.Windows.Forms.ListView m_listView1;
        private System.Windows.Forms.ColumnHeader m_lvcolFileName;
        private System.Windows.Forms.ColumnHeader m_lvcolFullPath;
        private System.Windows.Forms.ColumnHeader m_lvcolTimeStamp;
        private System.Windows.Forms.Timer m_timer_CopyAttempt;
        private System.Windows.Forms.Timer m_timer_reset_short;
        private System.Windows.Forms.Timer m_timer_reset_long;
        private System.Windows.Forms.Timer m_timer_reset;
        private System.Windows.Forms.TabControl m_tabControl1;
        private System.Windows.Forms.Button m_button_Folder;
        private System.Windows.Forms.Label m_label_Folder;
        private System.Windows.Forms.TextBox m_textBox_Folder;
        private System.Windows.Forms.FolderBrowserDialog m_folderBrowserDialog1;
        private System.Windows.Forms.ColumnHeader m_lvcolTab;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label m_label_Version;
        private System.Windows.Forms.Label m_label2;
        public System.Windows.Forms.CheckBox m_checkBox_Disable;
        private System.Windows.Forms.Label m_label_Monitoring;
    }
}

