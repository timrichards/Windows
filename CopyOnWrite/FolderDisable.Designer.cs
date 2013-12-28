namespace CopyOnWrite
{
    partial class FolderDisable
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
            this.checkBox_Disable = new System.Windows.Forms.CheckBox();
            this.button_Folder = new System.Windows.Forms.Button();
            this.label_Folder = new System.Windows.Forms.Label();
            this.textBox_Folder = new System.Windows.Forms.TextBox();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.SuspendLayout();
            // 
            // checkBox_Disable
            // 
            this.checkBox_Disable.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.checkBox_Disable.AutoSize = true;
            this.checkBox_Disable.Checked = true;
            this.checkBox_Disable.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_Disable.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBox_Disable.Location = new System.Drawing.Point(253, 3);
            this.checkBox_Disable.Name = "checkBox_Disable";
            this.checkBox_Disable.Size = new System.Drawing.Size(57, 17);
            this.checkBox_Disable.TabIndex = 3;
            this.checkBox_Disable.Text = "Disable";
            this.checkBox_Disable.UseVisualStyleBackColor = true;
            this.checkBox_Disable.CheckedChanged += new System.EventHandler(this.checkBox_Disable_CheckedChanged);
            // 
            // button_Folder
            // 
            this.button_Folder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.button_Folder.Location = new System.Drawing.Point(0, 0);
            this.button_Folder.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button_Folder.Name = "button_Folder";
            this.button_Folder.Size = new System.Drawing.Size(77, 20);
            this.button_Folder.TabIndex = 0;
            this.button_Folder.Text = "From Folder";
            this.button_Folder.UseVisualStyleBackColor = true;
            this.button_Folder.Click += new System.EventHandler(this.button_FromFolder_Click);
            // 
            // label_Folder
            // 
            this.label_Folder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.label_Folder.AutoSize = true;
            this.label_Folder.Location = new System.Drawing.Point(83, 4);
            this.label_Folder.Name = "label_Folder";
            this.label_Folder.Size = new System.Drawing.Size(32, 13);
            this.label_Folder.TabIndex = 1;
            this.label_Folder.Text = "None";
            this.label_Folder.DoubleClick += new System.EventHandler(this.label_FromFolder_DoubleClick);
            // 
            // textBox_Folder
            // 
            this.textBox_Folder.AllowDrop = true;
            this.textBox_Folder.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_Folder.Enabled = false;
            this.textBox_Folder.Location = new System.Drawing.Point(83, 0);
            this.textBox_Folder.Name = "textBox_Folder";
            this.textBox_Folder.Size = new System.Drawing.Size(164, 20);
            this.textBox_Folder.TabIndex = 2;
            this.textBox_Folder.Visible = false;
            this.textBox_Folder.TextChanged += new System.EventHandler(this.textBox_FromFolder_TextChanged);
            this.textBox_Folder.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_FromFolder_KeyDown);
            this.textBox_Folder.Validating += new System.ComponentModel.CancelEventHandler(this.textBox_FromFolder_Validating);
            // 
            // folderBrowserDialog1
            // 
            this.folderBrowserDialog1.RootFolder = System.Environment.SpecialFolder.DesktopDirectory;
            this.folderBrowserDialog1.ShowNewFolderButton = false;
            // 
            // FolderDisable
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.textBox_Folder);
            this.Controls.Add(this.checkBox_Disable);
            this.Controls.Add(this.label_Folder);
            this.Controls.Add(this.button_Folder);
            this.Font = new System.Drawing.Font("Tahoma", 8F);
            this.Name = "FolderDisable";
            this.Size = new System.Drawing.Size(313, 21);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.CheckBox checkBox_Disable;
        private System.Windows.Forms.Button button_Folder;
        private System.Windows.Forms.Label label_Folder;
        private System.Windows.Forms.TextBox textBox_Folder;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;

        #endregion
    }
}
