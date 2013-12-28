using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO; // Directory
using System.Diagnostics;

using System.Runtime.InteropServices;

namespace CopyOnWrite
{
    public partial class Form1 : Form
    {
        bool m_bIconAction = false;
        bool m_bIconSwitch = false;
        String m_strMonitoring = "";
        int m_timer_ChangeIcon_Interval_orig = 0;
        Color m_BackColor_orig = Color.Empty;
        int m_CowHeight = 0;

        public Form1()
        {
            InitializeComponent();

            //Properties.Settings.Default.Reset();

            //for (int nNumLeft = 0, nNewLeft = -1; nNumLeft != nNewLeft;)
            //{
            //    string[] arrDirs = Directory.GetDirectories(@"C:\_T\QW14 130218 files to", "*.*", SearchOption.AllDirectories);

            //    foreach (string strDir in arrDirs)
            //    {
            //        DirectoryInfo info = new DirectoryInfo(strDir);

            //        if ((info.GetFiles().Length == 0) && (info.GetDirectories().Length == 0))
            //        {
            //            Directory.Delete(strDir);
            //        }
            //    }

            //    nNumLeft = nNewLeft;
            //    nNewLeft = arrDirs.Length;
            //}

           m_label_Monitoring.ForeColor = WarnHighFileCounts();
           m_timer_ChangeIcon_Interval_orig =  m_timer_ChangeIcon.Interval;
           m_BackColor_orig = BackColor;
        }

        private void timer_ChangeIcon_Tick(object sender, EventArgs e)
        {
            if (m_bIconSwitch)
            {
                Icon = SystemIcons.Shield;
            }
            else if (m_bIconAction)
            {
                Icon = SystemIcons.Information;
                m_bIconAction = false;
            }
            else
            {
                Icon = SystemIcons.Warning;
            }

            m_bIconSwitch = !m_bIconSwitch;
        }

        private void ListviewInsert(int nIndex, ListViewItem item)
        {
            if (m_label_Monitoring.BackColor != Color.Green)
            {
                m_label_Monitoring.BackColor = Color.Green;
            }
            else
            {
                m_label_Monitoring.BackColor = Color.Empty;
            }

            m_listView1.Items.Insert(nIndex, item);
        }

        private void timer_ChangeTitle_Tick(object sender, EventArgs e)
        {
            if ((m_label_Monitoring.Text.Length == 0) &&
                (m_checkBox_Disable.Checked == false))
            {
                m_label_Monitoring.Text = m_strMonitoring;
                m_label_Monitoring.BackColor = Color.Empty;

                if (m_timer_reset_long.Enabled == false)
                {
                    // not sure if just setting this resets the timer, so checking first
                    m_timer_reset_long.Enabled = true;
                }
            }
            else
            {
                m_label_Monitoring.Text = "";
            }
        }

        void Label_Folder_Validate()
        {
            if (Directory.Exists(m_label_Folder.Text) == false)
            {
                m_checkBox_Disable.Checked = true;
                m_label_Folder.BackColor = Color.Black;
                m_label_Folder.ForeColor = Color.Red;
            }
            else
            {
                m_timer_ChangeIcon.Interval = m_timer_ChangeIcon_Interval_orig;
                BackColor = m_BackColor_orig;
                m_label_Folder.BackColor = Color.Empty;
                m_label_Folder.ForeColor = Color.Empty;
            }
        }

        private void checkBox_Disable_CheckedChanged(object sender, EventArgs e)
        {
            EnableRaisingEvents(m_checkBox_Disable.Checked == false);

            if (m_checkBox_Disable.Checked)
            {
                m_label_Monitoring.Text = "";
                m_timer_ChangeIcon.Interval = 500;
                BackColor = Color.Red;
            }
            else
            {
                Label_Folder_Validate();
            }
        }

        private void AddTab(bool bAddStrings = true)
        {
            m_tabControl1.TabPages.Add((m_tabControl1.TabCount + 1).ToString());

            TabPage page = m_tabControl1.TabPages[m_tabControl1.TabCount - 1];
            COWtabContents cow = new COWtabContents(this, page);

            if (m_CowHeight == 0)
            {
                m_CowHeight = cow.Height;
            }

//            cow.Dock = DockStyle.Fill;
            cow.Location = new Point(0, 0);
            cow.Size = new Size(page.Width, cow.Size.Height);
            cow.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            page.Controls.Add(cow);

            cow.ChangeIndex(m_tabControl1.TabPages.IndexOf(page), bAddStrings);
            Properties.Settings.Default.Save();
            m_tabControl1.SelectedTab = page;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ListViewHelper.EnableDoubleBuffer(m_listView1);

            try
            {
                m_label_Version.Text = System.Deployment.Application.ApplicationDeployment.CurrentDeployment.CurrentVersion.ToString();
            }
            catch
            {
            }

            if (Properties.Settings.Default.FromFolder.Count == 0)
            {
                AddTab(true);
            }
            else
            {
                foreach (string strPath in Properties.Settings.Default.FromFolder)
                {
                    AddTab(false);
                }
            }

            {
                TabPage page = m_tabControl1.TabPages[0];

                page.Text = "Source Path 1";

                COWtabContents cow = (COWtabContents)page.Controls[0];
                cow.ChangeIndex(0);
                cow.Controls.Remove(cow.m_button_Remove);
            }

            Label_Folder_Validate();
            m_timer_reset_long.Enabled = true;
            Icon = SystemIcons.Shield;
            m_strMonitoring = m_label_Monitoring.Text;
            m_timer_ChangeIcon.Start();
            m_timer_ChangeTitle.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            foreach (TabPage page in m_tabControl1.TabPages)
            {
                ((COWtabContents)page.Controls[0]).SaveFilter();
            }

            Properties.Settings.Default.Col_FileName_Width = m_lvcolFileName.Width;
            Properties.Settings.Default.Col_FullPath_Width = m_lvcolFullPath.Width;
            Properties.Settings.Default.Col_TimeStamp_Width = m_lvcolTimeStamp.Width;
            Properties.Settings.Default.Col_Tab_Width = m_lvcolTab.Width;
            Properties.Settings.Default.Save();
        }

        class File_CopyOnWrite
        {
            public File_CopyOnWrite(FileInfo fi, String strToFolder, String strEvent, int nTab)
            {
                m_dt = DateTime.Now;
                m_strNow = m_dt.ToString("yyMMdd.HHmmss");

                m_fi = fi;

                m_strNewFile = strToFolder + Path.DirectorySeparatorChar + Path.GetFileNameWithoutExtension(m_fi.FullName) +
                    "_" + m_strNow + Path.GetExtension(m_fi.FullName);
                m_nAttempt = 0;
                m_strErr = "";
                m_strEvent = strEvent;
                m_nTab = nTab;
            }

            FileInfo m_fi;
            public FileInfo fi { get { return m_fi; } }

            String m_strNewFile = "";
            public String StrNewFile { get { return m_strNewFile; } }

            String m_strErr = "";
            public String StrErr { get { return m_strErr; } set { m_strErr = value; } }

            int m_nAttempt = 0;
            public int Attempt { get { return m_nAttempt; } set { m_nAttempt = value; } }

            String m_strNow = "";
            public String StrNow { get { return m_strNow; } }

            DateTime m_dt = DateTime.Now;
            public DateTime DateStamp { get { return m_dt; } }

            String m_strEvent = "";
            public String StrEvent { get { return m_strEvent; } }

            int m_nTab = 0;
            public String Tab { get { return m_nTab.ToString(); } }
        }
        List<File_CopyOnWrite> m_list_Files = new List<File_CopyOnWrite>();
        int m_nErrors = 0;
        Dictionary<String, bool> m_dictionary_FileAttempted = new Dictionary<string, bool>();

        private void SetTimers(bool bCopyAttempt = true)
        {
            // make sure the timer gets reset - not sure if setting Enabled to true alone will accomplish this
            m_timer_CopyAttempt.Enabled = false;
            m_timer_CopyAttempt.Enabled = bCopyAttempt;

            m_timer_reset_short.Enabled = (bCopyAttempt == false);
            m_timer_reset_long.Enabled = (bCopyAttempt == false);
        }

        // match value is tab index (user base 1) <0 for no match due to negative rule, 0 for match, and >0 for no match entirely
        const String strDefaultWritablePackageFilter = "strDefaultWritablePackageFilter: None";
        public void FileSystemWatcherChanged(String strPath, string strEvent, int nTab, bool bMatch, String strWritablePackageFilter = strDefaultWritablePackageFilter)
        {
            FileInfo fi = new FileInfo(strPath);
            String strError = "";

            if (bMatch == false)
            {
                // no-op, but skip remaining tests
            }
            //else if (strWritablePackageFilter != strDefaultWritablePackageFilter)
            //{
            // key off the unsuccessfulbuild for example
            // qwmain_130531.201035.unsuccessfulbuild
           //    if (strWritablePackageFilter.Length == 0)
            //    {
            //        strError = "No W filter";
            //    }
            //    else
            //    {
                        // need two source paths, one the match path which is there already
                        // the second is the source path to start searching for writable files.
            //        return;
            //    }
            //}
            else if ((fi.Attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
            {
                // This gets rid of Perforce updates.
                strError = "R/O";
            }
            else if ((fi.Attributes & FileAttributes.System) == FileAttributes.System)
            {
                strError = "Sys";
            }
            else if ((fi.Attributes & FileAttributes.Directory) == FileAttributes.Directory)
            {
                strError = "Dir";
            }
            else if ((fi.Attributes & FileAttributes.Temporary) == FileAttributes.Temporary)
            {
                strError = "Tmp";
            }
            else if ((fi.Attributes & FileAttributes.Hidden) == FileAttributes.Hidden)
            {
                strError = "Hid";
            }
            else if (fi.Exists == false)
            {
                // Not sure while FileSystemWatcher posts an event without a proper
                // filter -- for instance in this case it sends in a directory
                // * file gets renamed

                strError = "NoX";
            }
            else if (fi.Name.Length == 0)
            {
                strError = "Nam";
            }
            else if (fi.Length == 0)
            {
                strError = "Emp";
            }
            else if (fi.Length > (1 * 1024 /*k*/ * 1024 /*M*/))
            {
                strError = ">1M";
            }
            else if (DateTime.Now.Subtract(fi.LastWriteTime).Minutes > 5)
            {
                // Getting files from repository would cause a copy on write, but their
                // date stamps are old. Don't want those anyway, so filter them out.

                strError = "Old";
            }
            else if (fi.LastWriteTime < fi.CreationTime)
            {
                // Still doesn't get rid of those pesky Perforce updates.

                strError = "W<C";
            }
            else if (fi.LastWriteTime.Subtract(DateTime.Now).Ticks > 0)
            {
                strError = "Fut";
            }

            if (bMatch && (strError.Length == 0))
            {
                if (m_dictionary_FileAttempted.ContainsKey(fi.FullName) == false)
                {
                    m_list_Files.Add(new File_CopyOnWrite(fi, m_label_Folder.Text, strEvent, nTab));
                    m_dictionary_FileAttempted.Add(fi.FullName, true /* no-op for lookup */);
                    SetTimers();
                }
            }
            else
            {
                String strTab = "";

                if ((bMatch == false) && (nTab > 0))
                {
                    strTab = "+";
                }

                strTab += nTab;

                if (strError.Length > 0)
                {
                    strError += " ";
                }

                strError += strTab;
                
                ListViewItem lvItem = new ListViewItem(new String[] { Path.GetFileName(fi.FullName), Path.GetDirectoryName(fi.FullName), DateTime.Now + " " + strEvent, strError });
                
                lvItem.ForeColor = Color.LimeGreen;
                lvItem.Name = strError;
                m_listView1.BeginUpdate();
                m_listView1.Items.RemoveByKey(strError);
                ListviewInsert(0, lvItem);
                m_listView1.EndUpdate();
            }
        }

        private void timer_CopyAttempt_Tick(object sender, EventArgs e)
        {
            //Action NestedFn1 = () =>
            //{
            //};

            //Action NestedFn2 = () =>
            //{
            //};

            //NestedFn1();
            //NestedFn2();

            while (m_list_Files.Count > 0)
            {
                if (m_checkBox_Disable.Checked)
                {
                    m_list_Files.Clear();
                    m_dictionary_FileAttempted.Clear();
                    SetTimers(false);
                    return;
                }

                if (m_list_Files.Count == 0)
                {
                    ListviewInsert(0, new ListViewItem(new String[] { "ERROR", "", "list empty" }));
                    SetTimers(false);
                    return;
                }

                // pop
                File_CopyOnWrite fileCOW = m_list_Files[0];
                m_list_Files.RemoveAt(0);

                bool bExist = File.Exists(fileCOW.fi.FullName);
                bool bFailed = (bExist == false);
                TimeSpan dtDiff = TimeSpan.Zero;
                String strThrewOpen = "";
                String strThrewCopy = "";

                for (int nTryType = 0; nTryType < 2; ++nTryType)
                {
                    String strThrown = "";

                    try
                    {
                        if (bExist)
                        {
                            switch (nTryType)
                            {
                                case 0:
                                {
                                    DateTime dtWrite = fileCOW.fi.LastWriteTime;

                                    // why doesn't this generate a FileSystemWatcher changed event and thus an infinite loop?

                                    //using (Stream stream = fileCOW.FileInfo_.Open(FileMode.Open, FileAccess.Write, FileShare.None)) { }   // Test to see if file is locked for write - this is an empty block which deletes the object called "stream" and closes the file

                                    // OpenWrite() call affects write time
                                    // The fileCOW.FileInfo_ object is not refreshed by OpenWrite().

                                    FileInfo fi = new FileInfo(fileCOW.fi.FullName);
                                    DateTime dtWrite2 = fi.LastWriteTime;

                                    // This should be about 5 seconds, matching the event timer, right??
                                    // but it isn't --- it's usually .001 sec. Which means something else wrote the file before this stream got opened
                                    // and the stream doesn't appear to have affected the write time, which /implies/ the stream lock test above is safe
                                    dtDiff = dtWrite2 - dtWrite;

                                    if (dtDiff.Ticks > 0)
                                    {
                                        strThrewOpen = "Wrt";
                                    }

                                    if (fileCOW.fi.Length != fi.Length)
                                    {
                                        strThrewOpen = "Siz";
                                    }

                                    break;
                                }

                                case 1:
                                {
                                    File.Copy(fileCOW.fi.FullName, fileCOW.StrNewFile, true);
                                    break;
                                }

                                default:
                                {
                                    Debug.Assert(false);
                                    break;
                                }
                            }
                        }
                    }
                    catch (System.UnauthorizedAccessException)
                    {
                        strThrown = "Aut";
                    }
                    catch (System.ArgumentNullException)
                    {
                        strThrown = "Nul";
                    }
                    catch (System.ArgumentException)
                    {
                        strThrown = "Arg";
                    }
                    catch (System.IO.PathTooLongException)
                    {
                        strThrown = "Lon";
                    }
                    catch (System.IO.DirectoryNotFoundException)
                    {
                        strThrown = "NoD";
                    }
                    catch (System.IO.FileNotFoundException)
                    {
                        strThrown = "NoF";
                    }
                    catch (System.IO.IOException)
                    {
                        strThrown = "IO";
                    }
                    catch (System.NotSupportedException)
                    {
                        strThrown = "Sup";
                    }
                    catch (System.Security.SecurityException)
                    {
                        strThrown = "Sec";
                    }
                    catch (Exception ex)
                    {
                        strThrown = ex.Message;
                    }

                    switch (nTryType)
                    {
                        case 0:
                        {
                            strThrewOpen = strThrown;
                            break;
                        }

                        case 1:
                        {
                            strThrewCopy = strThrown;
                            break;
                        }

                        default:
                        {
                            Debug.Assert(false);
                            break;
                        }
                    }
                }

                if (strThrewOpen.Length > 0)
                {
                    if (fileCOW.StrErr.Contains(strThrewOpen) == false)
                    {
                        fileCOW.StrErr += "(" + strThrewOpen + ")";
                    }
                }

                if (strThrewCopy.Length > 0)
                {
                    if (fileCOW.StrErr.Contains(strThrewCopy) == false)
                    {
                        fileCOW.StrErr += strThrewCopy;
                    }

                    ++fileCOW.Attempt;
                    bFailed = true;
                }

                const int MAX_ATTEMPTS = 5;

                if (bFailed && bExist && (fileCOW.Attempt < MAX_ATTEMPTS))
                {
                    m_list_Files.Add(fileCOW);  // push it to the end
                    ++m_nErrors;

                    if (m_list_Files.Count == m_nErrors)
                    {
                        return;     // wait for the next timer tick
                    }
                }

                if (fileCOW.Attempt > 0)
                {
                    --m_nErrors;
                }

                m_dictionary_FileAttempted.Remove(fileCOW.fi.FullName);

                if (m_list_Files.Count == 0)
                {
                    SetTimers(false);
                }

                ListViewItem lvItem = null;

                if (bFailed == false)
                {
                    lvItem = new ListViewItem(new String[] { Path.GetFileName(fileCOW.fi.FullName), Path.GetDirectoryName(fileCOW.fi.FullName), /* dtDiff.TotalSeconds + " " + fileCOW.StrNow + " " + */ fileCOW.DateStamp.ToString(), (fileCOW.StrEvent + " " + fileCOW.StrErr + " " + fileCOW.Tab).Trim() });
                    lvItem.Name = fileCOW.fi.FullName;
                    lvItem.ForeColor = /* (fileCOW.Attempt > 0) ? Color.Yellow : */ Color.White;

                    ListViewItem[] arrRows = m_listView1.Items.Find(lvItem.Name, false);

                    foreach (ListViewItem row in arrRows)
                    {
                        Byte r = (byte)(row.ForeColor.R - 10);
                        Byte g = (byte)(row.ForeColor.G - 10);
                        Byte b = (byte)(row.ForeColor.B - 20);

                        //if ((r < 0) || (g < 0) || (b < 0))
                        if ((r < 15) || (g < 15) || (b < 15))
                        {
                            continue;
                        }

                        //if ((r > 220) || (g > 220) || (b > 220))
                        if ((r > 255) || (g > 255) || (b > 255))
                        {
                            continue;
                        }

                        row.ForeColor = Color.FromArgb(r, g, b);
                    }
                }
                else
                {
                    if (fileCOW.Attempt < MAX_ATTEMPTS)
                    {
                        if (fileCOW.StrErr.Contains("No2") == false)
                        {
                            fileCOW.StrErr = fileCOW.StrErr.Insert(0, "No2");
                        }
                    }
                    
                    fileCOW.StrErr = fileCOW.StrErr.Trim();
                    string strError = fileCOW.StrErr.Replace("\n", " ") + " " + fileCOW.Tab;

                    lvItem = new ListViewItem(new String[] { Path.GetFileName(fileCOW.fi.FullName), Path.GetDirectoryName(fileCOW.fi.FullName), fileCOW.DateStamp.ToString() + " " + fileCOW.StrEvent, strError });
                    lvItem.ForeColor = Color.LimeGreen;
                    lvItem.ToolTipText = strError;

                    string[] strErrs = strError.Split(' ');

                    if (strErrs.Length > 0)
                    {
                        lvItem.Name = strErrs[strErrs.Length - 1];
                    }
                    else
                    {
                        lvItem.Name = strError;
                    }

                    m_listView1.BeginUpdate();
                    m_listView1.Items.RemoveByKey(lvItem.Name);
                    m_listView1.EndUpdate();
                }

                ListviewInsert(0, lvItem);
                Icon = SystemIcons.Information;
                m_timer_ChangeIcon.Enabled = true;  // make the info icon stick for the full duration - if this doesn't work set Enabled = false first
            }
        }
        
        private void EnableRaisingEvents(bool bRaise)
        {
            String strStatusCol1 = "Path" + ((m_tabControl1.TabPages.Count > 1) ? "s:" : "") + " ";
            String strStatusCol2 = "", strStatusCol3 = "";

            String strOff = "";
            String strMonitoring = "";
            String strDisable = "";

            int nHaveAnOffStatus = 0;
            int nHaveAMonitoringStatus = 0;
            int nHaveADisabledStatus = 0;

            foreach (TabPage page in m_tabControl1.TabPages)
            {
                COWtabContents cow = ((COWtabContents)page.Controls[0]);

                switch (cow.EnableRaisingEvents(bRaise))
                {
                    case COWtabContents.enum_Status.Off:
                    {
                        strOff += cow.Index_User + " ";
                        ++nHaveAnOffStatus;
                        break;
                    }

                    case COWtabContents.enum_Status.Disable:
                    {
                        strDisable += cow.Index_User + " ";
                        ++nHaveADisabledStatus;
                        break;
                    }

                    case COWtabContents.enum_Status.Monitoring:
                    {
                        strMonitoring += cow.Index_User + " ";
                        ++nHaveAMonitoringStatus;
                        break;
                    }

                    default:
                    {
                        throw new Exception();
                    }
                }
            }

            if (nHaveAnOffStatus == 1)
            {
                strOff += "is off";
            }
            else if (nHaveAnOffStatus > 1)
            {
                strOff = strOff.Insert(0, "Off: ");
            }

            if (nHaveADisabledStatus == 1)
            {
                strDisable += "is disabled";
            }
            else if (nHaveADisabledStatus > 1)
            {
                strDisable = strDisable.Insert(0, "Disabled: ");
            }

            if (nHaveAMonitoringStatus == 1)
            {
                strMonitoring += "is being monitored";
            }
            else if (nHaveAMonitoringStatus > 1)
            {
                strMonitoring = strMonitoring.Insert(0, "Monitoring: ");
            }

            if (nHaveAMonitoringStatus > 0)
            {
                strStatusCol1 += strMonitoring;

                if (nHaveADisabledStatus > 0)
                {
                    strStatusCol2 += strDisable;

                    if (nHaveAnOffStatus > 0)
                    {
                        strStatusCol3 += strOff + "(" + DateTime.Now.ToString() + ")";
                    }
                    else
                    {
                        strStatusCol3 += DateTime.Now.ToString();
                    }
                }
                else if (nHaveAnOffStatus > 0)
                {
                    strStatusCol2 += strOff;
                    strStatusCol3 += DateTime.Now.ToString();
                }
                else
                {
                    strStatusCol3 += DateTime.Now.ToString();
                }
            }
            else if (nHaveADisabledStatus > 0)
            {
                strStatusCol1 += strDisable;

                if ((nHaveAnOffStatus > 0))
                {
                    strStatusCol2 += strOff;
                }

                strStatusCol3 += DateTime.Now.ToString();
            }
            else if ((nHaveAnOffStatus > 0))
            {
                strStatusCol1 += strOff;
                strStatusCol3 += DateTime.Now.ToString();
            }

            if ((nHaveAMonitoringStatus > 0) || (nHaveADisabledStatus > 0) || (nHaveAnOffStatus > 0))
            {
                ListViewItem lvItem = new ListViewItem(new String[] {strStatusCol1, strStatusCol2, strStatusCol3});

                if ((nHaveADisabledStatus > 0) || (nHaveAnOffStatus > 0))
                {
                    lvItem.ForeColor = Color.LightBlue;
                }
                else
                {
                    lvItem.ForeColor = Color.Gray;
                }

                lvItem.Font = SystemFonts.SmallCaptionFont;

                String strName = "EnableRaisingEvents";
                lvItem.Name = strName;
                m_listView1.Items.RemoveByKey(strName);
                ListviewInsert(0, lvItem);
            }
        }

#region utility checks run by timer

        /*recycle bin*/
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1)]
        public struct SHQUERYRBINFO
        {
            public Int32 cbSize;
            public UInt64 i64Size;
            public UInt64 i64NumItems;
        }

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern int SHQueryRecycleBin(string pszRootPath, ref SHQUERYRBINFO pSHQueryRBInfo);

        Color WarnHighFileCounts(String strPath = "" /*recycle bin*/)
        {
            // Environment.SpecialFolder

            int nNumFiles = 0;

            if (strPath.Length > 0)
            {
                if (Directory.Exists(strPath))
                {
                    nNumFiles = (new DirectoryInfo(strPath)).GetFiles().Length;
                }
                else
                {
                    return Color.Red;
                }
            }
            else
            {
                /*recycle bin*/

                SHQUERYRBINFO query = new SHQUERYRBINFO();
                query.cbSize = Marshal.SizeOf(typeof(SHQUERYRBINFO));

                if (SHQueryRecycleBin(null, ref query) == 0)
                {
                    nNumFiles = (int)query.i64NumItems;
                }
            }

            Byte r = (Byte)(nNumFiles / 5000.0 * 255);

            if (r < 0)
            {
                return Color.Empty;
            }

            if (r > 255)
            {
                return Color.Red;
            }

            return Color.FromArgb(r, 0, 0);
        }

        protected bool IsVisibleOnAnyScreen(Rectangle rect)
        {
            foreach (Screen screen in Screen.AllScreens)
            {
                if (screen.WorkingArea.IntersectsWith(rect))
                {
                    return true;
                }
            }

            return false;
        }

#endregion utility checks run by timer

#region timers

        private void timer_reset_long_tick(object sender, EventArgs e)
        {
            Label_Folder_Validate();
            m_label_Folder.ForeColor = WarnHighFileCounts(m_label_Folder.Text);
            m_label_Monitoring.ForeColor = WarnHighFileCounts(/*recycle bin*/);

            if (IsVisibleOnAnyScreen(new Rectangle(Location, Size)) == false)
            {
                Location = new Point(0, 0);
            }

            // FileSystemWatcher falls asleep on the job? so reset it every 5 minutes

            if (m_checkBox_Disable.Checked)
            {
                EnableRaisingEvents(false);
                m_timer_reset_long.Enabled = false;
            }
            else if (m_timer_CopyAttempt.Enabled)
            {
                m_timer_reset_long.Enabled = false;
            }
            else
            {
                m_timer_reset.Enabled = true;
                m_bResetPass2 = false;
            }
        }

        private void timer_reset_short_tick(object sender, EventArgs e)
        {
            // FileSystemWatcher seems to have to be reset.

            m_timer_reset_short.Enabled = false;

            if (m_checkBox_Disable.Checked)
            {
                EnableRaisingEvents(false);
            }
            else if (m_timer_CopyAttempt.Enabled == false)
            {
                m_timer_reset.Enabled = true;
                m_bResetPass2 = false;
            }
        }

        bool m_bResetPass2 = false;
        private void timer_reset_Tick(object sender, EventArgs e)
        {
            // ticks twice: first to switch off, then to switch back on

            if (m_checkBox_Disable.Checked)
            {
                EnableRaisingEvents(false);
                m_timer_reset.Enabled = false;
                m_bResetPass2 = false;
            }
            else if (m_timer_CopyAttempt.Enabled)
            {
                m_timer_reset.Enabled = false;
                m_bResetPass2 = false;
            }
            else if (m_bResetPass2 == false)
            {
                EnableRaisingEvents(false);
                m_timer_reset.Enabled = true;
                m_bResetPass2 = true;
            }
            else
            {
                EnableRaisingEvents(true);
                m_timer_reset.Enabled = false;
                m_bResetPass2 = false;
            }
        }

#endregion timers

        public void button_AddTab_Click(object sender, EventArgs e)
        {
            AddTab();
        }

        public void RemoveTab(TabPage removePage)
        {
            TabControl.TabPageCollection pages = m_tabControl1.TabPages;

            m_tabControl1.SelectedTab = pages[pages.IndexOf(removePage) - 1];
            pages.Remove(removePage);

            foreach (TabPage page in pages)
            {
                int nIndex = pages.IndexOf(page);

                if (nIndex > 0) // the first tab is fixed
                {
                    page.Text = (nIndex + 1).ToString();
                    ((COWtabContents)page.Controls[0]).ChangeIndex(nIndex);
                }
            }

            Properties.Settings.Default.FromFolder.RemoveAt(pages.Count);
            Properties.Settings.Default.Negative.RemoveAt(pages.Count);
            Properties.Settings.Default.IsMatch.RemoveAt(pages.Count);

            {
                //Debug.Assert(Properties.Settings.Default.SplitterPos.Length == pages.Count + 1);
                int[] arrPos = Properties.Settings.Default.SplitterPos;
                Array.Resize(ref arrPos, pages.Count);
                Properties.Settings.Default.SplitterPos = arrPos;
            }

            {
                //Debug.Assert(Properties.Settings.Default.Disable.Length == pages.Count + 1);
                bool[] arrPos = Properties.Settings.Default.Disable;
                Array.Resize(ref arrPos, pages.Count);
                Properties.Settings.Default.Disable = arrPos;
            }

            Properties.Settings.Default.Save();
        }

#region serialization

        public const string m_csPathDefault = @"C:\_qdev";
        public void FromFolder_setat(int nIndex, string str = m_csPathDefault)
        {
            while (Properties.Settings.Default.FromFolder.Count <= nIndex)
            {
                Properties.Settings.Default.FromFolder.Add(m_csPathDefault);
            }

            Properties.Settings.Default.FromFolder[nIndex] = str;
        }

        const string m_csNegativeDefault = @"qwapp\.c$|dlldata\.c$|(_p|_i|_h)\.(c|h)$|_manifest.rc$";
        public void Negative_setat(int nIndex, string strNegative = m_csNegativeDefault)
        {
            while (Properties.Settings.Default.Negative.Count <= nIndex)
            {
                Properties.Settings.Default.Negative.Add(strNegative);
            }

            Properties.Settings.Default.Negative[nIndex] = strNegative;
        }

        const string m_csIsMatchDefault = @"\.sln$|\.vcxproj$|\.c$|\.cpp$|\\.h$|\\.rc$|\\.dlg$|\\.htm$|\\.html$";
        public void IsMatch_setat(int nIndex, string strIsMatch = m_csIsMatchDefault)
        {
            while (Properties.Settings.Default.IsMatch.Count <= nIndex)
            {
                Properties.Settings.Default.IsMatch.Add(strIsMatch);
            }

            Properties.Settings.Default.IsMatch[nIndex] = strIsMatch;
        }

        public void SplitterPos_setat(int nIndex, int nPos)
        {
            if (Properties.Settings.Default.SplitterPos.Length <= nIndex)
            {
                int[] arrPos = Properties.Settings.Default.SplitterPos;

                Array.Resize(ref arrPos, nIndex + 1);
                Properties.Settings.Default.SplitterPos = arrPos;
            }

            Properties.Settings.Default.SplitterPos[nIndex] = nPos;
        }

        public void Disable_setat(int nIndex, bool bDisable)
        {
            if (Properties.Settings.Default.Disable.Length <= nIndex)
            {
                Boolean[] arrPos = Properties.Settings.Default.Disable;

                Array.Resize(ref arrPos, nIndex + 1);
                Properties.Settings.Default.Disable = arrPos;
            }

            Properties.Settings.Default.Disable[nIndex] = bDisable;
        }

#endregion serialization

#region button_Folder

        public void button_ToFolder_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("0. button_ToFolder_Click");
            m_checkBox_Disable.Checked = true;
            m_folderBrowserDialog1.ShowNewFolderButton = true;

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
                m_label_Folder.Text = m_folderBrowserDialog1.SelectedPath;
                Properties.Settings.Default.ToFolder = m_folderBrowserDialog1.SelectedPath;
                Properties.Settings.Default.Save();
            }
        }
        private void label_ToFolder_DoubleClick(object sender, EventArgs e)
        {
            m_label_Folder.Hide();
            m_textBox_Folder.Text = m_label_Folder.Text;
            m_textBox_Folder.Enabled = true;
            m_textBox_Folder.Show();
            m_textBox_Folder.Focus();
        }
        void textBox_ToFolder_Cancel()
        {
            m_textBox_Folder.Text = "";
            m_textBox_Folder.ForeColor = Color.Empty;
            m_textBox_Folder.BackColor = Color.Empty;
            m_textBox_Folder.Enabled = false;
            m_textBox_Folder.Modified = false;
            m_textBox_Folder.Hide();
            m_label_Folder.Show();
            Label_Folder_Validate();
        }
        private void textBox_ToFolder_Validated(object sender, EventArgs e)
        {
            m_label_Folder.Text = m_textBox_Folder.Text;
            textBox_ToFolder_Cancel();
        }
        private void textBox_ToFolder_KeyDown(object sender, KeyEventArgs e)
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

#endregion button_Folder

        internal void SetAllWritableTab()
        {
            TabPage page = m_tabControl1.TabPages[m_tabControl1.SelectedIndex];

            if (page.Controls.Count == 1)
            {
                COWtabContents cow = new COWtabContents(this, page);

                cow.Size = new Size(page.Width, cow.Size.Height);
                cow.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                page.Controls.Add(cow);
                ((COWtabContents)page.Controls[0]).SetAllWritableClone(cow);
                ((COWtabContents)page.Controls[0]).Location = new Point(0, m_CowHeight);
            }
            else
            {
                ((COWtabContents)page.Controls[0]).SetAllWritableClone();
                ((COWtabContents)page.Controls[0]).Location = new Point(0, 0);
                Debug.Assert(page.Controls.Count == 2);
                page.Controls.RemoveAt(1);
            }

            splitContainer1.SplitterDistance += m_CowHeight * ((page.Controls.Count == 2) ? 1 : -1);
        }

        private void m_tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            TabPage page = m_tabControl1.TabPages[m_tabControl1.SelectedIndex];
            splitContainer1.SplitterDistance += m_CowHeight * ((page.Controls.Count == 2) ? 1 : -1);
        }
    }

    public enum ListViewExtendedStyles
    {
        /// <summary>
        /// LVS_EX_GRIDLINES
        /// </summary>
        GridLines = 0x00000001,
        /// <summary>
        /// LVS_EX_SUBITEMIMAGES
        /// </summary>
        SubItemImages = 0x00000002,
        /// <summary>
        /// LVS_EX_CHECKBOXES
        /// </summary>
        CheckBoxes = 0x00000004,
        /// <summary>
        /// LVS_EX_TRACKSELECT
        /// </summary>
        TrackSelect = 0x00000008,
        /// <summary>
        /// LVS_EX_HEADERDRAGDROP
        /// </summary>
        HeaderDragDrop = 0x00000010,
        /// <summary>
        /// LVS_EX_FULLROWSELECT
        /// </summary>
        FullRowSelect = 0x00000020,
        /// <summary>
        /// LVS_EX_ONECLICKACTIVATE
        /// </summary>
        OneClickActivate = 0x00000040,
        /// <summary>
        /// LVS_EX_TWOCLICKACTIVATE
        /// </summary>
        TwoClickActivate = 0x00000080,
        /// <summary>
        /// LVS_EX_FLATSB
        /// </summary>
        FlatsB = 0x00000100,
        /// <summary>
        /// LVS_EX_REGIONAL
        /// </summary>
        Regional = 0x00000200,
        /// <summary>
        /// LVS_EX_INFOTIP
        /// </summary>
        InfoTip = 0x00000400,
        /// <summary>
        /// LVS_EX_UNDERLINEHOT
        /// </summary>
        UnderlineHot = 0x00000800,
        /// <summary>
        /// LVS_EX_UNDERLINECOLD
        /// </summary>
        UnderlineCold = 0x00001000,
        /// <summary>
        /// LVS_EX_MULTIWORKAREAS
        /// </summary>
        MultilWorkAreas = 0x00002000,
        /// <summary>
        /// LVS_EX_LABELTIP
        /// </summary>
        LabelTip = 0x00004000,
        /// <summary>
        /// LVS_EX_BORDERSELECT
        /// </summary>
        BorderSelect = 0x00008000,
        /// <summary>
        /// LVS_EX_DOUBLEBUFFER
        /// </summary>
        DoubleBuffer = 0x00010000,
        /// <summary>
        /// LVS_EX_HIDELABELS
        /// </summary>
        HideLabels = 0x00020000,
        /// <summary>
        /// LVS_EX_SINGLEROW
        /// </summary>
        SingleRow = 0x00040000,
        /// <summary>
        /// LVS_EX_SNAPTOGRID
        /// </summary>
        SnapToGrid = 0x00080000,
        /// <summary>
        /// LVS_EX_SIMPLESELECT
        /// </summary>
        SimpleSelect = 0x00100000
    }

    public enum ListViewMessages
    {
        First = 0x1000,
        SetExtendedStyle = (First + 54),
        GetExtendedStyle = (First + 55),
    }

    /// <summary>
    /// Contains helper methods to change extended styles on ListView, including enabling double buffering.
    /// Based on Giovanni Montrone's article on <see cref="http://www.codeproject.com/KB/list/listviewxp.aspx"/>
    /// </summary>
    public class ListViewHelper
    {
        private ListViewHelper()
        {
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessage(IntPtr handle, int messg, int wparam, int lparam);

        public static void SetExtendedStyle(Control control, ListViewExtendedStyles exStyle)
        {
            ListViewExtendedStyles styles;
            styles = (ListViewExtendedStyles)SendMessage(control.Handle, (int)ListViewMessages.GetExtendedStyle, 0, 0);
            styles |= exStyle;
            SendMessage(control.Handle, (int)ListViewMessages.SetExtendedStyle, 0, (int)styles);
        }

        public static void EnableDoubleBuffer(Control control)
        {
            ListViewExtendedStyles styles;
            // read current style
            styles = (ListViewExtendedStyles)SendMessage(control.Handle, (int)ListViewMessages.GetExtendedStyle, 0, 0);
            // enable double buffer and border select
            styles |= ListViewExtendedStyles.DoubleBuffer | ListViewExtendedStyles.BorderSelect;
            // write new style
            SendMessage(control.Handle, (int)ListViewMessages.SetExtendedStyle, 0, (int)styles);
        }
        public static void DisableDoubleBuffer(Control control)
        {
            ListViewExtendedStyles styles;
            // read current style
            styles = (ListViewExtendedStyles)SendMessage(control.Handle, (int)ListViewMessages.GetExtendedStyle, 0, 0);
            // disable double buffer and border select
            styles -= styles & ListViewExtendedStyles.DoubleBuffer;
            styles -= styles & ListViewExtendedStyles.BorderSelect;
            // write new style
            SendMessage(control.Handle, (int)ListViewMessages.SetExtendedStyle, 0, (int)styles);
        }
    }
    
    #region MainForm

    public class MainForm : Form1
    {
        private bool windowInitialized;

        public MainForm()
        {
            // this is the default
            this.WindowState = FormWindowState.Normal;
            this.StartPosition = FormStartPosition.WindowsDefaultBounds;

            // check if the saved bounds are nonzero and visible on any screen
            if (Properties.Settings.Default.WindowPosition != Rectangle.Empty && IsVisibleOnAnyScreen(Properties.Settings.Default.WindowPosition))
            {
                // first set the bounds
                this.StartPosition = FormStartPosition.Manual;
                this.DesktopBounds = Properties.Settings.Default.WindowPosition;

                // afterwards set the window state to the saved value (which could be Maximized)
                this.WindowState = Properties.Settings.Default.WindowState;
            }
            else
            {
                // this resets the upper left corner of the window to windows standards
                this.StartPosition = FormStartPosition.WindowsDefaultLocation;

                // we can still apply the saved size
                // msorens: added gatekeeper, otherwise first time appears as just a title bar!
                if (Properties.Settings.Default.WindowPosition != Rectangle.Empty)
                {
                    this.Size = Properties.Settings.Default.WindowPosition.Size;
                }
            }

            windowInitialized = true;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            // only save the WindowState if Normal or Maximized
            switch (this.WindowState)
            {
                case FormWindowState.Normal:
                case FormWindowState.Maximized:
                    Properties.Settings.Default.WindowState = this.WindowState;
                    break;

                default:
                    Properties.Settings.Default.WindowState = FormWindowState.Normal;
                    break;
            }

            # region msorens: this code does *not* handle minimized/maximized window.

            // reset window state to normal to get the correct bounds
            // also make the form invisible to prevent distracting the user
            //this.Visible = false;
            //this.WindowState = FormWindowState.Normal;
            //Settings.Default.WindowPosition = this.DesktopBounds;

            # endregion

            Properties.Settings.Default.Save();
        }

        # region window size/position
        // msorens: Added region to handle closing when window is minimized or maximized.

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            TrackWindowState();
        }

        protected override void OnMove(EventArgs e)
        {
            base.OnMove(e);
            TrackWindowState();
        }

        // On a move or resize in Normal state, record the new values as they occur.
        // This solves the problem of closing the app when minimized or maximized.
        private void TrackWindowState()
        {
            // Don't record the window setup, otherwise we lose the persistent values!
            if (!windowInitialized) { return; }

            if (WindowState == FormWindowState.Normal)
            {
                Properties.Settings.Default.WindowPosition = this.DesktopBounds;
            }
        }

        # endregion window size/position
    }

    class ListViewNF : System.Windows.Forms.ListView
    {
        public ListViewNF()
        {
            //Activate double buffering
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);

            //Enable the OnNotifyMessage event so we get a chance to filter out 
            // Windows messages before they get to the form's WndProc
            this.SetStyle(ControlStyles.EnableNotifyMessage, true);
        }

        protected override void OnNotifyMessage(Message m)
        {
            //Filter out the WM_ERASEBKGND message
            if (m.Msg != 0x14)
            {
                base.OnNotifyMessage(m);
            }
        }
    }

#endregion MainForm

}
