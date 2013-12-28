using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO; // Directory
using System.Text.RegularExpressions;
using System.Collections.Specialized;   //StringCollection
using System.Diagnostics;

namespace CopyOnWrite
{
    public partial class COWtabContents : UserControl
    {
        Form1 m_Form = null;
        TabPage m_Page = null;
        String m_strLabelExcludeOrig = "";
        String m_strLabelIncludeOrig = "";
        String m_strButtonFolderOrig = "";
        FlatStyle m_buttonFolderFlatOrig = FlatStyle.Standard;

        public COWtabContents(Form1 form, TabPage page)
        {
            InitializeComponent();

            m_Form = form;
            m_Page = page;
        }

        //void Label_Folder_Validate()
        //{
        //    if (Directory.Exists(m_label_Folder.Text) == false)
        //    {
        //        m_checkBox_Disable.Checked = true;
        //        m_label_Folder.BackColor = Color.Black;
        //        m_label_Folder.ForeColor = Color.Red;
        //    }
        //    else
        //    {
        //        m_label_Folder.BackColor = Color.Empty;
        //        m_label_Folder.ForeColor = Color.Empty;
        //    }
        //}

        private void COWtabContents_Load(object sender, EventArgs e)
        {
            m_strLabelExcludeOrig = m_label_Negative.Text;
            m_strLabelIncludeOrig = m_label_IsMatch.Text;
            m_strButtonFolderOrig = m_button_Folder.Text;
            m_buttonFolderFlatOrig = m_button_Folder.FlatStyle;

            FromFolder_set();
        }
        void FromFolder_set()
        {
            String strPath = Properties.Settings.Default.FromFolder[m_nIndex];
            m_label_Folder.Text = strPath;

            bool bValid = Directory.Exists(strPath);

Debug.WriteLine("9. FromFolder_set()");
            CheckBox_Disable_Check(bValid == false);

            if (Directory.Exists(strPath))
            {
                m_fileSystemWatcher1.Path = strPath;
                m_label_Folder.ForeColor = Color.Black;
            }
            else
            {
                // directory used to be here
                m_label_Folder.ForeColor = Color.DarkRed;
            }

            m_fileSystemWatcher1.EnableRaisingEvents = ((m_Form.m_checkBox_Disable.Checked == false) && (m_checkBox_Disable.Checked == false));
        }
        void FromFolder_change(String strPath)
        {
            if (strPath.Length == 0)
            {
                return;
            }

            if (Directory.Exists(strPath) == false)
            {
                return;
            }

            m_Form.FromFolder_setat(m_nIndex, strPath);
            Properties.Settings.Default.Save();
            FromFolder_set();
        }
        private void button_FromFolder_Click(object sender, EventArgs e)
        {
            m_folderBrowserDialog1.ShowNewFolderButton = false;

            if (Directory.Exists(m_label_Folder.Text))
            {
                m_folderBrowserDialog1.SelectedPath = m_label_Folder.Text;
            }
            else
            {
                m_folderBrowserDialog1.SelectedPath = "";
            }

            DialogResult result = m_folderBrowserDialog1.ShowDialog();

            if (result == DialogResult.OK)
            {
                FromFolder_change(m_folderBrowserDialog1.SelectedPath);
            }
        }

        bool m_bCheckBox_Disable_programmatically = false;
        bool m_bCheckBox_Disable_user = false;
        void CheckBox_Disable_Check(bool bCheck)
        {
            m_bCheckBox_Disable_programmatically = true;

            if ((bCheck == false) && (m_checkBox_Disable.Checked == false) && (m_bCheckBox_Disable_user == false))
            {
                Debug.WriteLine("2. CheckBox_Disable_Check\t\t\tcheckBox_Disable.Checked = false; bCheck==" + bCheck);
                m_checkBox_Disable.Checked = false;
                m_checkBox_Disable.Enabled = true;
            }
            else
            {
                Debug.WriteLine("3. CheckBox_Disable_Check\t\t\tcheckBox_Disable.Checked = bCheck==" + bCheck + "; checkBox_Disable.Checked==" + m_checkBox_Disable.Checked + "; m_bCheckBox_Disable_user==" + m_bCheckBox_Disable_user);
                m_checkBox_Disable.Checked = bCheck;
                m_checkBox_Disable.Enabled = (m_Form.m_checkBox_Disable.Checked == false);
            }

            m_bCheckBox_Disable_programmatically = false;
        }
        private void checkBox_Disable_CheckedChanged(object sender, EventArgs e)
        {
            Debug.WriteLine("4. checkBox_Disable_CheckedChanged\tm_bCheckBox_Disable_user==" + m_bCheckBox_Disable_user + "; checkBox_Disable.Checked==" + m_checkBox_Disable.Checked);
            if (m_bCheckBox_Disable_programmatically)
            {
                Debug.WriteLine("5. checkBox_Disable_CheckedChanged\tm_bCheckBox_Disable_programmatically");
                if ((m_bCheckBox_Disable_user) && (m_checkBox_Disable.Checked == false))
                {
                    // user wants this tab disabled
                    Debug.WriteLine("6. checkBox_Disable_CheckedChanged\tcheckBox_Disable.Checked == false");
                    CheckBox_Disable_Check(true);
                }
            }
            else
            {
                m_bCheckBox_Disable_user = m_checkBox_Disable.Checked;
            }

            if (Directory.Exists(m_fileSystemWatcher1.Path))
            {
                m_fileSystemWatcher1.EnableRaisingEvents = ((m_Form.m_checkBox_Disable.Checked == false) && (m_checkBox_Disable.Checked == false));
            }
            else
            {
                Debug.WriteLine("7. checkBox_Disable_CheckedChanged\tDirectory.Exists(fileSystemWatcher1.Path) == false");
                CheckBox_Disable_Check(true);
            }
        }

        private void button_Remove_Click(object sender, EventArgs e)
        {
            m_Form.RemoveTab(m_Page);
        }

        private void button_AddTab_Click(object sender, EventArgs e)
        {
            m_Form.button_AddTab_Click(sender, e);
        }

        public enum enum_Status
        {
            Off, Disable, Monitoring
        };
        public enum_Status EnableRaisingEvents(bool bRaise)
        {
            enum_Status eStatus = enum_Status.Off;

            if (m_Form.m_checkBox_Disable.Checked)
            {
Debug.WriteLine("8. EnableRaisingEvents m_Form.m_checkBox_Disable.Checked == true");
                CheckBox_Disable_Check(true);
            }
            else
            {
Debug.WriteLine("8. EnableRaisingEvents m_Form.m_checkBox_Disable.Checked == false");
                FromFolder_set();

                if (m_checkBox_Disable.Checked)
                {
                    eStatus = enum_Status.Disable;
                }
                else
                {
                    eStatus = enum_Status.Monitoring;
                }
            }

            return eStatus;
        }

        private void fileSystemWatcher1_Changed(object sender, FileSystemEventArgs e)
        {
            fileSystemWatcher1_Changed_(sender, e);
        }
        private void fileSystemWatcher1_Created(object sender, FileSystemEventArgs e)
        {
            fileSystemWatcher1_Changed_(sender, e, "Created");
        }
        private void fileSystemWatcher1_Renamed(object sender, FileSystemEventArgs e)
        {
            fileSystemWatcher1_Changed_(sender, e, "Renamed");
        }
        private void fileSystemWatcher1_Changed_(object sender, FileSystemEventArgs e, String strEvent = "")
        {
            //if (checkBox_AllWritable.Checked)
            //{
            //    // m_textBox_Negative is spoofing m_textBox_IsMatch in this case
            //    bool bMatch = ((m_textBox_Negative.Text.Length == 0) || Regex.IsMatch(e.FullPath, m_textBox_IsMatch.Text, RegexOptions.IgnoreCase));

            //    m_label_IsMatch.ForeColor = Color.Black;

            //    if (m_textBox_IsMatch.Text.Length == 0)
            //    {
            //        m_label_IsMatch.ForeColor = Color.Red;
            //    }

            //    // m_textBox_IsMatch.Text is in this case a filter for packaging all writable files
            //    m_Form.FileSystemWatcherChanged(e.FullPath, strEvent, Index_User, bMatch, m_textBox_IsMatch.Text);
            //}
            //else
            {
                bool bCopyFile = true;

                {
                    String strNeg = m_textBox_Negative.Text;

                    if (strNeg.Length > 0)
                    {
                        bCopyFile = (Regex.IsMatch(e.FullPath, strNeg, RegexOptions.IgnoreCase) == false);
                    }
                }

                int nNegativeRuleEncodeMultiplier = bCopyFile ? 1 : -1;

                if (bCopyFile)
                {
                    String strPos = m_textBox_IsMatch.Text;

                    if (strPos.Length > 0)
                    {
                        bCopyFile = (Regex.IsMatch(e.FullPath, strPos, RegexOptions.IgnoreCase));
                    }
                }

                // tab value is tab index (user base 1). <0 for no match due to negative rule
                m_Form.FileSystemWatcherChanged(e.FullPath, strEvent, Index_User * nNegativeRuleEncodeMultiplier, bCopyFile);
            }
        }

        int m_nIndex = 0;
        public int Index_User { get { return m_nIndex + 1; } }
        public void ChangeIndex(int nIndex, bool bAddStrings = true)
        {
            bool bNew = (bAddStrings == false);

            m_nIndex = nIndex;

            if (bAddStrings)
            {
                if (Properties.Settings.Default.FromFolder.Count <= nIndex)
                {
                    m_Form.FromFolder_setat(m_nIndex);
                    bNew = true;
                }

                if (Properties.Settings.Default.Negative.Count <= nIndex)
                {
                    m_Form.Negative_setat(m_nIndex);
                    bNew = true;
                }

                if (Properties.Settings.Default.IsMatch.Count <= nIndex)
                {
                    m_Form.IsMatch_setat(m_nIndex);
                    bNew = true;
                }

                // splitter pos should default to designer position
                // Disable checkbox should default to designer true/false
            }

            if (bNew == false)
            {
                // the new index will always be one less than the old
                // always because the one to the left was deleted. So just move
                // the filter into the settings at the new index.
                SaveFilter();
            }
            else
            {
                m_label_Folder.Text = Properties.Settings.Default.FromFolder[nIndex];
                m_textBox_Negative.Text = Properties.Settings.Default.Negative[nIndex];
                m_textBox_IsMatch.Text = Properties.Settings.Default.IsMatch[nIndex];

                if (Properties.Settings.Default.SplitterPos.Length > nIndex)
                {
                    m_splitContainer1.SplitterDistance = Properties.Settings.Default.SplitterPos[nIndex];
                }

                if (Properties.Settings.Default.Disable.Length > nIndex)
                {
                    // non-programmatic checkstate since this is the user setting
                    bool bChecked = Properties.Settings.Default.Disable[nIndex];
                    Debug.WriteLine("1. ChangeIndex\t\t\t\t\tbChecked==" + bChecked);
                    m_checkBox_Disable.Checked = bChecked;
                }
            }
        }
        public void SaveFilter()
        {
            m_Form.FromFolder_setat(m_nIndex, m_label_Folder.Text);
            m_Form.Negative_setat(m_nIndex, m_textBox_Negative.Text);
            m_Form.IsMatch_setat(m_nIndex, m_textBox_IsMatch.Text);
            m_Form.SplitterPos_setat(m_nIndex, m_splitContainer1.SplitterDistance);
            m_Form.Disable_setat(m_nIndex, m_bCheckBox_Disable_user);
        }

        private void splitContainer1_SplitterMoved(object sender, SplitterEventArgs e)
        {
            if (m_splitContainer1.SplitterDistance < (m_splitContainer1.Panel1MinSize + 20))
            {
                m_label_Negative.ForeColor = (m_textBox_Negative.Text.Length > 0) ? Color.Red : Color.Gray;
                m_label_IsMatch.ForeColor = Color.Black;
            }
            else if (m_splitContainer1.SplitterDistance > (m_splitContainer1.Width - m_splitContainer1.Panel2MinSize - 20))
            {
                m_label_Negative.ForeColor = Color.Black;
                m_label_IsMatch.ForeColor = (m_textBox_IsMatch.Text.Length > 0) ? Color.Red : Color.Gray;
            }
            else
            {
                m_label_Negative.ForeColor = Color.Black;
                m_label_IsMatch.ForeColor = Color.Black;
            }
        }

        private void splitContainer1_Paint(object sender, PaintEventArgs e)
        {
            var control = sender as SplitContainer;
            //paint the three dots'
            Point[] points = new Point[3];
            var w = control.Width;
            var h = control.Height;
            var d = control.SplitterDistance;
            var sW = control.SplitterWidth;

            //calculate the position of the points'
            if (control.Orientation == Orientation.Horizontal)
            {
                points[0] = new Point((w / 2), d + (sW / 2));
                points[1] = new Point(points[0].X - 10, points[0].Y);
                points[2] = new Point(points[0].X + 10, points[0].Y);
            }
            else
            {
                points[0] = new Point(d + (sW / 2), (h / 2));
                points[1] = new Point(points[0].X, points[0].Y - 10);
                points[2] = new Point(points[0].X, points[0].Y + 10);
            }

            foreach (Point p in points)
            {
                p.Offset(-2, -2);
                e.Graphics.FillEllipse(SystemBrushes.ControlDark,
                    new Rectangle(p, new Size(3, 3)));

                p.Offset(1, 1);
                e.Graphics.FillEllipse(SystemBrushes.ControlLight,
                    new Rectangle(p, new Size(3, 3)));
            }
        }

        // Temp variable to store a previously focused control
        private Control focused = null;
        private void splitContainer1_MouseDown(object sender, MouseEventArgs e)
        {
            // Get the focused control before the splitter is focused
            focused = getFocused(m_Form.Controls);
            m_splitContainer1.IsSplitterFixed = true;
        }
        private Control getFocused(Control.ControlCollection controls)
        {
            foreach (Control c in controls)
            {
                if (c.Focused)
                {
                    // Return the focused control
                    return c;
                }
                else if (c.ContainsFocus)
                {
                    // If the focus is contained inside a control's children
                    // return the child
                    return getFocused(c.Controls);
                }
            }
            // No control on the form has focus
            return null;
        }
        private void splitContainer1_MouseUp(object sender, MouseEventArgs e)
        {
            // If a previous control had focus
            if (focused != null)
            {
                // Return focus and clear the temp variable for 
                // garbage collection
                focused.Focus();
                focused = null;
            }

            m_splitContainer1.IsSplitterFixed = false;
            m_splitContainer1.Invalidate();
        }

        private void splitContainer1_MouseMove(object sender, MouseEventArgs e)
        {
            // Check to make sure the splitter won't be updated by the
            // normal move behavior also
            if (((SplitContainer)sender).IsSplitterFixed)
            {
                // Make sure that the button used to move the splitter
                // is the left mouse button
                if (e.Button.Equals(MouseButtons.Left))
                {
                    // Checks to see if the splitter is aligned Vertically
                    if (((SplitContainer)sender).Orientation.Equals(Orientation.Vertical))
                    {
                        // Only move the splitter if the mouse is within
                        // the appropriate bounds
                        if (e.X > 0 && e.X < ((SplitContainer)sender).Width)
                        {
                            // Move the splitter
                            ((SplitContainer)sender).SplitterDistance = e.X;
                        }
                    }
                    // If it isn't aligned vertically then it must be
                    // horizontal
                    else
                    {
                        // Only move the splitter if the mouse is within
                        // the appropriate bounds
                        if (e.Y > 0 && e.Y < ((SplitContainer)sender).Height)
                        {
                            // Move the splitter
                            ((SplitContainer)sender).SplitterDistance = e.Y;
                        }
                    }
                }
                // If a button other than left is pressed or no button
                // at all
                else
                {
                    // This allows the splitter to be moved normally again
                    ((SplitContainer)sender).IsSplitterFixed = false;
                }
            }
        }

        private void textBox_ToFolder_Validated(object sender, EventArgs e)
        {
            m_label_Folder.Text = m_textBox_Folder.Text;
            textBox_FromFolder_Cancel();
        }
        private void label_FromFolder_DoubleClick(object sender, EventArgs e)
        {
            m_label_Folder.Hide();
            m_textBox_Folder.Text = m_label_Folder.Text;
            m_textBox_Folder.Enabled = true;
            m_textBox_Folder.Show();
            m_textBox_Folder.Focus();
        }
        void textBox_FromFolder_Cancel()
        {
            m_textBox_Folder.Text = "";
            m_textBox_Folder.ForeColor = Color.Empty;
            m_textBox_Folder.BackColor = Color.Empty;
            m_textBox_Folder.Enabled = false;
            m_textBox_Folder.Modified = false;
            m_textBox_Folder.Hide();
            m_label_Folder.Show();
        }
        private void textBox_Folder_Validated(object sender, EventArgs e)
        {
            FromFolder_change(m_textBox_Folder.Text);
            textBox_FromFolder_Cancel();
        }
        private void textBox_FromFolder_KeyDown(object sender, KeyEventArgs e)
        {
            m_textBox_Folder.BackColor = Color.Empty;

            switch (e.KeyCode)
            {
                case Keys.Escape:
                {
                    // not my favorite way of handling cancel
                    m_textBox_Folder.Text = m_label_Folder.Text;
                    goto case Keys.Enter;
                }

                case Keys.Enter:    // otherwise it'll close the form
                {
                    m_textBox_Folder.Enabled = false;
                    m_textBox_Folder.Enabled = true;
                    break;
                }

                default:
                {
                    break;
                }
            }
        }

        //private void checkBox_AllWritable_CheckedChanged(object sender, EventArgs e)
        //{
        //    m_Form.MakeRoomForAllWritableUI();

        //    if (checkBox_AllWritable.Checked)
        //    {
        //        m_label_Negative.Text = "+";
        //        m_label_IsMatch.Text = "W";
        //    }
        //    else
        //    {
        //        m_label_Negative.Text = m_strLabelExcludeOrig;
        //        m_label_IsMatch.Text = m_strLabelIncludeOrig;
        //    }
        //}

        private void m_textBox_IsMatch_Leave(object sender, EventArgs e)
        {
            m_label_IsMatch.ForeColor = Color.Black;
        }

        private void button_AllWritable_Click(object sender, EventArgs e)
        {
            m_Form.SetAllWritableTab();
        }

        COWtabContents m_AllWritableClone = null;
        internal void SetAllWritableClone(COWtabContents cow = null)
        {
            m_AllWritableClone = cow;
            button_AllWritable.Text = (m_AllWritableClone == null) ? "v" : "^";

            if (m_AllWritableClone == null)
            {
                return;
            }

            m_AllWritableClone.m_button_Folder.Text = "Watch Dir";
            m_AllWritableClone.m_button_Folder.FlatStyle = FlatStyle.Flat;
            m_AllWritableClone.m_button_Folder.FlatAppearance.BorderSize = 0;
            m_AllWritableClone.button_AllWritable.Visible = false;
            m_AllWritableClone.m_button_AddTab.Visible = false;
            m_AllWritableClone.m_button_Remove.Visible = false;
            m_AllWritableClone.m_checkBox_Disable.Visible = false;
        }
    }
}
