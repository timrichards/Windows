using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;    // Directory

namespace CopyOnWrite
{
    public partial class FolderDisable : UserControl
    {
        public FolderDisable()
        {
            InitializeComponent();
        }

        private void checkBox_Disable_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void button_FromFolder_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowNewFolderButton = false;

            if (Directory.Exists(label_Folder.Text))
            {
                folderBrowserDialog1.SelectedPath = label_Folder.Text;
            }
            else
            {
                folderBrowserDialog1.SelectedPath = "";
            }

            DialogResult result = folderBrowserDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
 //               FromFolder_change(folderBrowserDialog1.SelectedPath);
            }
        }

        private void label_FromFolder_DoubleClick(object sender, EventArgs e)
        {
            label_Folder.Hide();
            textBox_Folder.Text = label_Folder.Text;
            textBox_Folder.Enabled = true;
            textBox_Folder.Show();
            textBox_Folder.Focus();
   //         CheckBox_Disable_Check(true);
        }

        void textBox_FromFolder_Cancel()
        {
            textBox_Folder.Enabled = false;
            textBox_Folder.Text = "";
            textBox_Folder.Hide();
            label_Folder.Show();
        }
        private void textBox_FromFolder_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Directory.Exists(textBox_Folder.Text) == false)
            {
                e.Cancel = true;
                textBox_Folder.ForeColor = Color.DarkRed;
                return;
            }

   //         FromFolder_change(textBox_Folder.Text);
            textBox_FromFolder_Cancel();
        }
        private void textBox_FromFolder_TextChanged(object sender, EventArgs e)
        {
            textBox_Folder.ForeColor = Color.Black;
        }
        private void textBox_FromFolder_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                {
                    textBox_FromFolder_Cancel();
                    break;
                }

                default:
                {
                    break;
                }
            }
        }
    }
}
