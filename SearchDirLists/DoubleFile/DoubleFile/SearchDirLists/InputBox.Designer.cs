namespace SearchDirLists
{
    partial class InputBox
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
            this.form_lbl_Prompt = new System.Windows.Forms.Label();
            this.form_cb_Entry = new System.Windows.Forms.ComboBox();
            this.form_btn_OK = new System.Windows.Forms.Button();
            this.form_btn_Cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // form_lbl_Prompt
            // 
            this.form_lbl_Prompt.AutoSize = true;
            this.form_lbl_Prompt.Location = new System.Drawing.Point(141, 50);
            this.form_lbl_Prompt.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.form_lbl_Prompt.Name = "form_lbl_Prompt";
            this.form_lbl_Prompt.Size = new System.Drawing.Size(69, 20);
            this.form_lbl_Prompt.TabIndex = 0;
            this.form_lbl_Prompt.Text = "Prompt?";
            // 
            // form_cb_Entry
            // 
            this.form_cb_Entry.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.form_cb_Entry.FormattingEnabled = true;
            this.form_cb_Entry.Location = new System.Drawing.Point(146, 104);
            this.form_cb_Entry.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.form_cb_Entry.Name = "form_cb_Entry";
            this.form_cb_Entry.Size = new System.Drawing.Size(451, 28);
            this.form_cb_Entry.TabIndex = 1;
            // 
            // form_btn_OK
            // 
            this.form_btn_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.form_btn_OK.AutoSize = true;
            this.form_btn_OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.form_btn_OK.Location = new System.Drawing.Point(446, 183);
            this.form_btn_OK.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.form_btn_OK.Name = "form_btn_OK";
            this.form_btn_OK.Size = new System.Drawing.Size(112, 34);
            this.form_btn_OK.TabIndex = 2;
            this.form_btn_OK.Text = "OK";
            this.form_btn_OK.UseVisualStyleBackColor = true;
            // 
            // form_btn_Cancel
            // 
            this.form_btn_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.form_btn_Cancel.AutoSize = true;
            this.form_btn_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.form_btn_Cancel.Location = new System.Drawing.Point(568, 183);
            this.form_btn_Cancel.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.form_btn_Cancel.Name = "form_btn_Cancel";
            this.form_btn_Cancel.Size = new System.Drawing.Size(112, 34);
            this.form_btn_Cancel.TabIndex = 3;
            this.form_btn_Cancel.Text = "Cancel";
            this.form_btn_Cancel.UseVisualStyleBackColor = true;
            // 
            // InputBox
            // 
            this.AcceptButton = this.form_btn_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.AutoSize = true;
            this.CancelButton = this.form_btn_Cancel;
            this.ClientSize = new System.Drawing.Size(722, 270);
            this.ControlBox = false;
            this.Controls.Add(this.form_btn_Cancel);
            this.Controls.Add(this.form_btn_OK);
            this.Controls.Add(this.form_cb_Entry);
            this.Controls.Add(this.form_lbl_Prompt);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "InputBox";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "InputBox";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.InputBox_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label form_lbl_Prompt;
        private System.Windows.Forms.ComboBox form_cb_Entry;
        private System.Windows.Forms.Button form_btn_OK;
        private System.Windows.Forms.Button form_btn_Cancel;
    }
}