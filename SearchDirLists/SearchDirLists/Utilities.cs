using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using System.Runtime.InteropServices;
using System.Security;
using System.Drawing;
using System.Threading;

namespace SearchDirLists
{
    class Blinky
    {
        static bool m_bTreeSelect = false;
        internal static bool TreeSelect { get { return m_bTreeSelect; } }

        readonly Control m_defaultControl = null;
        readonly System.Windows.Forms.Timer m_timer = null;

        Holder m_blinky = new NullHolder();
        Color m_clrOrig = Color.Empty;
        Color m_clrBlink = Color.DarkTurquoise;
        int m_nBlink = 0;
        int m_nNumBlinks = 10;
        bool m_bProgress = false;

        abstract class Holder
        {
            internal virtual Color BackColor { get; set; }
            internal virtual void Reset() { }
        }
        class NullHolder : Holder { }
        class TreeNodeHolder : Holder
        {
            readonly TreeNode m_obj = null;
            internal TreeNodeHolder(TreeNode obj) { m_obj = obj; m_bTreeSelect = true; }
            internal override Color BackColor { get { return m_obj.BackColor; } set { m_obj.BackColor = value; } }
            internal override void Reset() { m_bTreeSelect = false; m_obj.TreeView.SelectedNode = m_obj; }
        }
        class ListViewItemHolder : Holder
        {
            readonly ListViewItem m_obj = null;
            internal ListViewItemHolder(ListViewItem obj) { m_obj = obj; }
            internal override Color BackColor { get { return m_obj.BackColor; } set { m_obj.BackColor = value; } }
            internal override void Reset() { m_obj.Selected = true; }
        }
        class ControlHolder : Holder
        {
            readonly Control m_obj = null;
            internal ControlHolder(Control obj) { m_obj = obj; }
            internal override Color BackColor { get { return m_obj.BackColor; } set { m_obj.BackColor = value; } }
        }

        internal Blinky(System.Windows.Forms.Timer timer, Control defaultControl)
        {
            m_timer = timer;
            m_defaultControl = defaultControl;
        }

        internal void SelectTreeNode(TreeNode treeNode)
        {
            Reset();
            m_blinky = new TreeNodeHolder(treeNode);
            treeNode.TreeView.Select();
            treeNode.EnsureVisible();
            treeNode.TreeView.SelectedNode = null;
            Go(Once: true);
        }

        internal void SelectLVitem(ListViewItem lvItem)
        {
            Reset();
            m_blinky = new ListViewItemHolder(lvItem);
            lvItem.EnsureVisible();
            lvItem.Selected = false;
            Go(Once: true);
        }

        internal void Go(Control ctl, Color? clr = null, bool Once = false)
        {
            Reset();
            m_blinky = new ControlHolder(ctl);
            Go(clr, Once);
        }

        internal void Go(Color? clr = null, bool Once = false, bool bProgress = false)
        {
            if (m_blinky is NullHolder)
            {
                m_blinky = new ControlHolder(m_defaultControl);
            }

            m_clrOrig = m_blinky.BackColor;
            m_bProgress = bProgress;
            m_clrBlink = clr ?? (bProgress ? Color.LightSalmon : Color.Turquoise);
            m_nBlink = 0;
            m_nNumBlinks = Once ? 2 : 10;
            m_timer.Interval = bProgress ? 500 : (Once ? 100 : 50);
            m_timer.Enabled = true;
        }

        internal void Tick()
        {
            if (++m_nBlink < m_nNumBlinks)
            {
                SetCtlBackColor((m_blinky.BackColor == m_clrBlink) ? m_clrOrig : m_clrBlink);
            }
            else
            {
                Reset();
            }
        }

        void Reset()
        {
            m_timer.Enabled = false;
            m_nBlink = 0;
            SetCtlBackColor(m_clrOrig);
            m_blinky.Reset();
            m_defaultControl.Select();
            m_blinky = new NullHolder();
        }

        void SetCtlBackColor(Color clr)
        {
            m_blinky.BackColor = clr;
        }
    }

    static class ExtensionMethods
    {
        internal static bool IsChildOf(this TreeNode child, TreeNode treeNode)
        {
            if (child.Level <= treeNode.Level)
            {
                return false;
            }

            TreeNode parentNode = child.Parent;

            while (parentNode != null)
            {
                if (parentNode == treeNode)
                {
                    return true;
                }

                parentNode = parentNode.Parent;
            }

            return false;
        }

        internal static TreeNode Root(this TreeNode treeNode)
        {
            TreeNode nodeParent = treeNode;

            while (nodeParent.Parent != null)
            {
                nodeParent = nodeParent.Parent;
            }

            return nodeParent;
        }

        internal static Rectangle Scale(this Rectangle rc_in, SizeF scale)
        {
            RectangleF rc = rc_in;

            rc.X *= scale.Width;
            rc.Y *= scale.Height;
            rc.Width *= scale.Width;
            rc.Height *= scale.Height;
            return Rectangle.Ceiling(rc);
        }
    }

    class FlashWindow
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [StructLayout(LayoutKind.Sequential)]
        public struct FLASHWINFO
        {
            public UInt32 cbSize;
            public IntPtr hwnd;
            public UInt32 dwFlags;
            public UInt32 uCount;
            public UInt32 dwTimeout;
        }

        public const UInt32 FLASHW_ALL = 3;

        internal static void Go(Control ctl_in = null)
        {
            Control ctl = ctl_in ?? Form1.static_form;

            Utilities.CheckAndInvoke(ctl, new Action(() =>
            {
                FLASHWINFO fInfo = new FLASHWINFO();

                fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));
                fInfo.hwnd = ctl.Handle;
                fInfo.dwFlags = FLASHW_ALL;
                fInfo.uCount = 3;
                fInfo.dwTimeout = 0;
                FlashWindowEx(ref fInfo);
            }));
        }
    }

    class LVvolStrings : Utilities
    {
        int m_nIndex = -1;
        String m_strVolumeName = null;
        String m_strPath = null;
        String m_strSaveAs = null;
        String m_strStatus = null;
        String m_strInclude = null;
        String m_strVolumeGroup = null;
        internal int Index { get { return m_nIndex; } }
        internal String VolumeName { get { return m_strVolumeName; } }
        internal String StrPath { get { return m_strPath; } }
        internal String SaveAs { get { return m_strSaveAs; } }
        internal String Status { get { return m_strStatus; } }
        internal String Include { get { return m_strInclude; } }
        internal String VolumeGroup { get { return m_strVolumeGroup; } }

        internal LVvolStrings(ListViewItem lvItem)
        {
            m_nIndex = lvItem.Index;
            m_strVolumeName = lvItem.SubItems[0].Text;
            m_strPath = lvItem.SubItems[1].Text;
            m_strSaveAs = lvItem.SubItems[2].Text;
            m_strStatus = lvItem.SubItems[3].Text;
            m_strInclude = lvItem.SubItems[4].Text;

            if ((lvItem.SubItems.Count > 5) && StrValid(lvItem.SubItems[5].Text))
            {
                m_strVolumeGroup = lvItem.SubItems[5].Text;
            }
        }

        internal bool CanInclude { get { return Include == "Yes"; } }
        internal bool CanLoad
        {
            get
            {
                return (CanInclude &&
                    ((Utilities.m_str_USING_FILE + Utilities.m_str_SAVED).Contains(Status)));
            }
        }

        internal void SetStatus_BadFile(ListView lv)
        {
            lv.Items[Index].SubItems[3].Text =
                m_strStatus = "Bad file. Will overwrite.";
        }

        internal void SetStatus_Done(ListView lv, TreeNode rootNode)
        {
            lv.Items[Index].Tag = rootNode;
        }
    }

    // SearchDirLists listing file|*.sdl_list|SearchDirLists volume list file|*.sdl_vol|SearchDirLists copy scratchpad file|*.sdl_copy|SearchDirLists ignore list file|*.sdl_ignore
    abstract class SDL_File : Utilities
    {
        public static String BaseFilter = null;
        public static String FileAndDirListFileFilter = "SearchDirLists listing file|*." + m_strFILEEXT_Listing;

        public readonly String Header = null;

        String m_strDescription = null;
        public String Description { get { return m_strDescription + " list file"; } }

        String m_strExt = null;
        public String Filter { get { return "SearchDirLists " + Description + "|*." + m_strExt; } }

        protected static OpenFileDialog static_OpenFileDialog = null;
        protected static SaveFileDialog static_SaveFileDialog = null;

        protected String m_strPrevFile = null;
        protected String m_strFileNotDialog = null;

        internal static void SetFileDialogs(OpenFileDialog ofd_in, SaveFileDialog sfd_in)
        {
            static_OpenFileDialog = ofd_in;
            static_SaveFileDialog = sfd_in;
            BaseFilter = static_OpenFileDialog.Filter;
        }

        protected SDL_File(String strHeader, String strExt, String strDescription)
        {
            Utilities.Assert(1303.4311, static_OpenFileDialog != null);
            Utilities.Assert(1303.4312, static_SaveFileDialog != null);
            Header = strHeader;
            m_strExt = strExt;
            m_strDescription = strDescription;
        }

        bool ShowDialog(FileDialog fd)
        {
            fd.Filter = Filter + "|" + BaseFilter;
            fd.FilterIndex = 0;
            fd.FileName = Path.GetFileNameWithoutExtension(m_strPrevFile);
            fd.InitialDirectory = Path.GetDirectoryName(m_strPrevFile);

            if (fd.ShowDialog() != DialogResult.OK)
            {
                return false;
            }

            m_strFileNotDialog = m_strPrevFile = fd.FileName;
            return true;
        }

        protected virtual void ReadListItem(UList<ListViewItem> listItems, String[] strArray) { listItems.Add(new ListViewItem(strArray)); }

        internal bool ReadList(ListView lv)
        {
            if ((m_strFileNotDialog == null) && (ShowDialog(static_OpenFileDialog) == false))
            {
                return false;
            }

            UList<ListViewItem> listItems = new UList<ListViewItem>();

            using (StreamReader sr = File.OpenText(m_strFileNotDialog))
            {
                String strLine = sr.ReadLine();

                if (strLine == Header)
                {
                    while ((strLine = sr.ReadLine()) != null)
                    {
                        ReadListItem(listItems, strLine.TrimEnd(new char[] { '\t' }).Split('\t').Take(lv.Columns.Count).ToArray());
                    }
                }
            }

            if (listItems.Count > 0)
            {
                lv.Items.Clear();
                lv.Items.AddRange(listItems.ToArray());
                lv.Invalidate();
            }
            else
            {
                MessageBox.Show("Not a valid " + Description + ".".PadRight(100), "Load " + Description);
            }

            return (listItems.Count > 0);
        }

        protected virtual String WriteListItem(int nIndex, String str) { return str; }

        internal bool WriteList(ListView.ListViewItemCollection lvItems)
        {
            if (ShowDialog(static_SaveFileDialog) == false)
            {
                return false;
            }

            if ((File.Exists(m_strPrevFile))
                && (MessageBox.Show((m_strPrevFile + " already exists. Overwrite?").PadRight(100), Description, MessageBoxButtons.YesNo)
                != System.Windows.Forms.DialogResult.Yes))
            {
                return false;
            }

            using (StreamWriter sw = File.CreateText(m_strPrevFile))
            {
                sw.WriteLine(Header);

                foreach (ListViewItem lvItem in lvItems)
                {
                    sw.Write(WriteListItem(0, lvItem.SubItems[0].Text));

                    int nIx = 1;

                    foreach (ListViewItem.ListViewSubItem lvSubitem in lvItem.SubItems.Cast<ListViewItem.ListViewSubItem>().Skip(1))
                    {
                        sw.Write('\t' + WriteListItem(nIx, lvSubitem.Text));
                        ++nIx;
                    }

                    sw.WriteLine();
                }
            }

            return true;
        }
    }

    class SDL_VolumeFile : SDL_File
    {
        internal SDL_VolumeFile(String strFile = null) : base(m_str_VOLUME_LIST_HEADER, m_strFILEEXT_Volume, "volume") { m_strFileNotDialog = strFile; }

        protected override void ReadListItem(UList<ListViewItem> listItems, String[] strArray)
        {
            if (strArray.Length < 4)
            {
                return;
            }

            strArray[3] = "Using file.";

            if (File.Exists(strArray[2]) == false)
            {
                strArray[2] = Path.Combine(Path.GetDirectoryName(m_strPrevFile), Path.GetFileName(strArray[2]));

                if (File.Exists(strArray[2]) == false)
                {
                    strArray[3] = "No file. Will create.";
                }
            }

            strArray[1] = strArray[1].TrimEnd(Path.DirectorySeparatorChar);
            listItems.Add(new ListViewItem(strArray));
        }

        protected override String WriteListItem(int nIndex, String str)
        {
            return (nIndex == 1) ? str.TrimEnd(Path.DirectorySeparatorChar) : str;
        }
    }

    class SDL_CopyFile : SDL_File { internal SDL_CopyFile() : base(m_str_COPYDIRS_LIST_HEADER, m_strFILEEXT_Copy, "copy") { } }

    class SDL_IgnoreFile : SDL_File { internal SDL_IgnoreFile(String strFile = null) : base(m_str_IGNORE_LIST_HEADER, m_strFILEEXT_Ignore, "ignore") { m_strFileNotDialog = strFile; } }

    class UList<T> :
#if (true)
        Dictionary<T, object>       // guarantees uniqueness; faster random seek; removes items fast
    {
        public void Add(T t) { base.Add(t, null); }
        public T this[int i] { get { return base.Keys.ElementAt(i); } }
        public new IEnumerator<T> GetEnumerator() { return base.Keys.GetEnumerator(); }
        public T[] ToArray() { return base.Keys.ToArray(); }
        public List<T> ToList() { return base.Keys.ToList(); }
        public bool Contains(T t) { return base.ContainsKey(t); }
    }
#else
#error Locks up removing items.
        List<T> { }                  // uses less memory; faster iterator; locks up removing items.
#endif

    class Utilities
    {
        internal const String m_str_HEADER_01 = "SearchDirLists 0.1";
        internal const String m_str_START_01 = m_str_HEADER_01 + " START";
        internal const String m_str_END_01 = m_str_HEADER_01 + " END";
        internal const String m_str_ERRORS_LOC_01 = m_str_HEADER_01 + " ERRORS";
        internal const String m_str_TOTAL_LENGTH_LOC_01 = m_str_HEADER_01 + " LENGTH";
        internal const String m_str_DRIVE_01 = m_str_HEADER_01 + " DRIVE";
        internal const String m_str_VOLUME_LIST_HEADER_01 = m_str_HEADER_01 + " VOLUME LIST";
        internal const String m_str_HEADER = "SearchDirLists 0.2";
        internal const String m_str_START = m_str_HEADER + " START";
        internal const String m_str_END = m_str_HEADER + " END";
        internal const String m_str_ERRORS_LOC = m_str_HEADER + " ERRORS";
        internal const String m_str_TOTAL_LENGTH_LOC = m_str_HEADER + " LENGTH";
        internal const String m_str_DRIVE = m_str_HEADER + " DRIVE";
        internal const String m_str_VOLUME_LIST_HEADER = m_str_HEADER + " VOLUME LIST";
        internal const String m_str_COPYDIRS_LIST_HEADER = m_str_HEADER + " COPYDIRS LIST";
        internal const String m_str_IGNORE_LIST_HEADER = m_str_HEADER + " IGNORE LIST";
        internal const String m_str_USING_FILE = "Using file.";
        internal const String m_str_SAVED = "Saved.";
        internal const String m_str_NOTSAVED = "Not saved.";
        internal const int nColLENGTH = 7;
        internal const int nColLENGTH_01 = 5;
        internal const int nColLENGTH_LV = 4;
        internal const String m_strLINETYPE_Version = "V";
        internal const String m_strLINETYPE_Nickname = "N";
        internal const String m_strLINETYPE_Path = "P";
        internal const String m_strLINETYPE_DriveInfo = "I";
        internal const String m_strLINETYPE_Comment = "C";
        internal const String m_strLINETYPE_Start = "S";
        internal const String m_strLINETYPE_Directory = "D";
        internal const String m_strLINETYPE_File = "F";
        internal const String m_strLINETYPE_End = "E";
        internal const String m_strLINETYPE_Blank = "B";
        internal const String m_strLINETYPE_Error_Dir = "R";
        internal const String m_strLINETYPE_Error_File = "r";
        internal const String m_strLINETYPE_Length = "L";
        internal const String m_strFILEEXT_Listing = "sdl_list";
        internal const String m_strFILEEXT_Volume = "sdl_vol";
        internal const String m_strFILEEXT_Copy = "sdl_copy";
        internal const String m_strFILEEXT_Ignore = "sdl_ignore";

        static MessageBoxDelegate static_MessageboxCallback = null;
        protected MessageBoxDelegate m_MessageboxCallback = null;
        static double static_nLastAssertLoc = -1;
        static DateTime static_dtLastAssert = DateTime.MinValue;
        static bool static_bAssertUp = false;

        protected Utilities()
        {
            m_MessageboxCallback = static_MessageboxCallback;
        }

        internal static bool Assert(double nLocation, bool bCondition, String strError_in = null)
        {
            if (bCondition) return true;

            if ((static_nLastAssertLoc == nLocation) && ((DateTime.Now - static_dtLastAssert).Seconds < 1))
            {
                return false;
            }

            String strError = "Assertion failed at location " + nLocation + ".";

            if (StrValid(strError_in))
            {
                strError += "\n\nAdditional information: " + strError_in;
            }

            Utilities.WriteLine(strError);

            if (static_bAssertUp)
            {
#if (DEBUG)
                FlashWindow.Go();
#endif
            }
            else
            {
#if (DEBUG)
                if (Trace.Listeners.Cast<TraceListener>().Any(i => i is DefaultTraceListener))
                {
                    Debug.Assert(false, strError);
                }
                else
#endif
                {
                    static_bAssertUp = true;
                    new Thread(new ParameterizedThreadStart(Assert_A)).Start(strError);
                }

                static_nLastAssertLoc = nLocation;
                static_dtLastAssert = DateTime.Now;
            }

            return false;
        }

        static void Assert_A(object strError)
        {
            static_MessageboxCallback(((String)strError).PadRight(100) + "\n\nPlease discuss this bug at http://sourceforge.net/projects/searchdirlists/.", "SearchDirLists Assertion Failure");
            static_bAssertUp = false;
        }

        internal static void Closure(Action action) { action(); }

        internal static object CheckAndInvoke(Control control, Delegate action, object[] args = null)
        {
            if (Form1.AppExit)
            {
                return null;
            }

            if (control.InvokeRequired)
            {
                if (args == null)
                {
                    return control.Invoke(action);
                }
                else
                {
                    return control.Invoke(action, (object)args);
                }
            }
            else
            {
                if (action is Action)
                {
                    ((Action)action)();
                }
                else if (action is BoolAction)
                {
                    return ((BoolAction)action)();
                }
                else
                {
                    return action.DynamicInvoke(args);     // late-bound and slow
                }
            }

            return null;
        }

        internal static String CheckNTFS_chars(ref String strFile, bool bFile = false)
        {
            char[] arrChar = bFile ? Path.GetInvalidFileNameChars() : Path.GetInvalidPathChars();
            int nIx = -1;

            if ((nIx = strFile.IndexOfAny(arrChar)) > -1)
            {
                String strRet = "NTFS ASCII " + ((int)strFile[nIx]).ToString();

                strFile = strFile.Replace("\n", "").Replace("\r", "").Replace("\t", "");    // program-incompatible
                return strRet;
            }
            else
            {
                return null;
            }
        }

        internal static void ConvertFile(String strFile)
        {
            String strFile_01 = StrFile_01(strFile);

            if (File.Exists(strFile_01))
            {
                File.Delete(strFile_01);
            }

            File.Move(strFile, strFile_01);

            using (StreamWriter file_out = new StreamWriter(strFile))
            {
                using (StreamReader file_in = new StreamReader(strFile_01))
                {
                    String strLine = null;
                    long nLineNo = 0;       // lines number from one
                    bool bAtErrors = false;

                    while ((strLine = file_in.ReadLine()) != null)
                    {
                        ++nLineNo;

                        if (strLine == m_str_HEADER_01)
                        {
                            Utilities.Assert(1303.4302, nLineNo == 1);
                            file_out.WriteLine(FormatLine(m_strLINETYPE_Version, nLineNo, m_str_HEADER));
                            continue;
                        }
                        else if (nLineNo == 2)
                        {
                            file_out.WriteLine(FormatLine(m_strLINETYPE_Nickname, nLineNo, strLine));
                            continue;
                        }
                        else if (nLineNo == 3)
                        {
                            file_out.WriteLine(FormatLine(m_strLINETYPE_Path, nLineNo, strLine));
                            continue;
                        }
                        else if (strLine == m_str_DRIVE_01)
                        {
                            Utilities.Assert(1303.4303, nLineNo == 4);
                            file_out.WriteLine(FormatLine(m_strLINETYPE_Comment, nLineNo, m_str_DRIVE));

                            for (int i = 0; i < 8; ++i)
                            {
                                strLine = file_in.ReadLine();
                                file_out.WriteLine(FormatLine(m_strLINETYPE_DriveInfo, nLineNo, strLine));
                                ++nLineNo;
                            }

                            continue;
                        }
                        else if (strLine.Length == 0)
                        {
                            file_out.WriteLine(FormatLine(m_strLINETYPE_Blank, nLineNo));
                            continue;
                        }
                        else if (nLineNo == 14)
                        {
                            file_out.WriteLine(FormatLine(m_strLINETYPE_Comment, nLineNo, FormatString(nHeader: 0)));
                            continue;
                        }
                        else if (nLineNo == 15)
                        {
                            file_out.WriteLine(FormatLine(m_strLINETYPE_Comment, nLineNo, FormatString(nHeader: 1)));
                            continue;
                        }
                        else if (strLine.StartsWith(m_str_START_01))
                        {
                            Utilities.Assert(1303.4304, nLineNo == 16);
                            file_out.WriteLine(FormatLine(m_strLINETYPE_Start, nLineNo, m_str_START));
                            continue;
                        }
                        else if (strLine.StartsWith(m_str_END_01))
                        {
                            file_out.WriteLine(FormatLine(m_strLINETYPE_End, nLineNo, m_str_END));
                            continue;
                        }
                        else if (strLine == m_str_ERRORS_LOC_01)
                        {
                            file_out.WriteLine(FormatLine(m_strLINETYPE_Comment, nLineNo, m_str_ERRORS_LOC));
                            bAtErrors = true;
                            continue;
                        }
                        else if (strLine.StartsWith(m_str_TOTAL_LENGTH_LOC_01))
                        {
                            String[] arrLine = strLine.Split('\t');

                            file_out.WriteLine(FormatLine(m_strLINETYPE_Length, nLineNo, FormatString(strDir: m_str_TOTAL_LENGTH_LOC, nLength: long.Parse(arrLine[nColLENGTH_01]))));
                            continue;
                        }

                        String[] strArray = strLine.Split('\t');
                        String strDir = strArray[0];

                        if (StrValid(strDir) == false)
                        {
                            DateTime dtParse;
                            String strTab = null;

                            if ((strArray.Length > 5) && strArray[5].Contains("Trailing whitespace") && DateTime.TryParse(strArray[1], out dtParse))
                            {
                                strTab = "\t";
                            }

                            file_out.WriteLine(FormatLine(bAtErrors ? m_strLINETYPE_Error_File : m_strLINETYPE_File, nLineNo, strTab + strLine));
                            continue;
                        }
                        else if (strDir.Contains(":" + Path.DirectorySeparatorChar) == false)
                        {
                            Utilities.Assert(1303.4305, false);        // all that's left is directories
                            continue;
                        }

                        // directory
                        String P = Path.DirectorySeparatorChar.ToString();
                        String PP = P + P;
                        String str = strLine.Replace(PP, P);

                        file_out.WriteLine(FormatLine(bAtErrors ? m_strLINETYPE_Error_Dir : m_strLINETYPE_Directory, nLineNo, str));
                    }
                }
            }
        }

        internal static int CountNodes(List<TreeNode> listNodes)
        {
            int nCount = 0;

            foreach (TreeNode treeNode in listNodes)
            {
                nCount += CountNodes(treeNode, bNextNode: false);
            }

            return nCount;
        }

        internal static int CountNodes(TreeNode treeNode_in, bool bNextNode = true)
        {
            TreeNode treeNode = treeNode_in;
            int nCount = 0;

            do
            {
                if ((treeNode.Nodes != null) && (treeNode.Nodes.Count > 0))
                {
                    nCount += CountNodes(treeNode.Nodes[0]);
                }

                ++nCount;
            }
            while (bNextNode && ((treeNode = treeNode.NextNode) != null));

            return nCount;
        }

        internal static bool FormatPath(ref String strPath, bool bFailOnDirectory = true)
        {
            while (strPath.Contains(@"\\"))
            {
                strPath = strPath.Replace(@"\\", @"\");
            }

            String strDirName = Path.GetDirectoryName(strPath);

            if ((StrValid(strDirName) == false) || Directory.Exists(strDirName))
            {
                String strCapDrive = strPath.Substring(0, strPath.IndexOf(":" + Path.DirectorySeparatorChar) + 2);

                strPath = Path.GetFullPath(strPath).Replace(strCapDrive, strCapDrive.ToUpper());

                if (strPath == strCapDrive.ToUpper())
                {
                    Utilities.Assert(1303.4306, strDirName == null);
                }
                else
                {
                    strPath = strPath.TrimEnd(Path.DirectorySeparatorChar);
                    Utilities.Assert(1303.4307, StrValid(strDirName));
                }
            }
            else if (bFailOnDirectory)
            {
                return false;
            }

            return true;
        }

        internal static bool FormatPath(ref String strPath, ref String strSaveAs, bool bFailOnDirectory = true)
        {
            if (StrValid(strPath))
            {
                strPath += Path.DirectorySeparatorChar;

                if (FormatPath(ref strPath, bFailOnDirectory) == false)
                {
                    static_MessageboxCallback("Error in Source path.".PadRight(100), "Save Directory Listing");
                    return false;
                }
            }

            if (StrValid(strSaveAs))
            {
                strSaveAs = Path.GetFullPath(strSaveAs.Trim());

                if (FormatPath(ref strSaveAs, bFailOnDirectory) == false)
                {
                    static_MessageboxCallback("Error in Save filename.".PadRight(100), "Save Directory Listing");
                    return false;
                }
            }

            return true;
        }

        static String FormatLine(String strLineType, long nLineNo, String strLine_in = null)
        {
            String strLine_out = strLineType + "\t" + nLineNo;

            if (StrValid(strLine_in))
            {
                strLine_out += '\t' + strLine_in;
            }

            return strLine_out;
        }

        internal static String FormatSize(String in_str, bool bBytes = false)
        {
            return FormatSize(ulong.Parse(in_str), bBytes);
        }

        internal static String FormatSize(long nLength, bool bBytes = false, bool bNoDecimal = false)
        {
            return FormatSize((ulong)nLength, bBytes, bNoDecimal);
        }

        internal static String FormatSize(ulong nLength, bool bBytes = false, bool bNoDecimal = false)
        {
            double nT = nLength / 1024.0 / 1024.0 / 1024 / 1024 - .05;
            double nG = nLength / 1024.0 / 1024 / 1024 - .05;
            double nM = nLength / 1024.0 / 1024 - .05;
            double nK = nLength / 1024.0 - .05;     // Windows Explorer seems to not round
            String strFmt_big = "###,##0.0";
            String strFormat = bNoDecimal ? "###,###" : strFmt_big;
            String strSz = null;

            if (((int)nT) > 0) strSz = nT.ToString(strFmt_big) + " TB";
            else if (((int)nG) > 0) strSz = nG.ToString(strFmt_big) + " GB";
            else if (((int)nM) > 0) strSz = nM.ToString(strFormat) + " MB";
            else if (((int)nK) > 0) strSz = nK.ToString(strFormat) + " KB";
            else strSz = "1 KB";                    // Windows Explorer mins at 1K

            if (nLength > 0)
            {
                return strSz + (bBytes ? (" (" + nLength.ToString("###,###,###,###,###") + " bytes)") : String.Empty);
            }
            else
            {
                return "0 bytes";
            }
        }

        internal static String FormatString(String strDir = null, String strFile = null, DateTime? dtCreated = null, DateTime? dtModified = null, String strAttributes = null, long nLength = -1, String strError1 = null, String strError2 = null, int? nHeader = null)
        {
            String strLength = null;
            String strCreated = null;
            String strModified = null;

            if (nLength > -1)
            {
                strLength = nLength.ToString();
            }

            if (dtCreated != null)
            {
                strCreated = dtCreated.ToString();
            }

            if (dtModified != null)
            {
                strModified = dtModified.ToString();
            }

            if (StrValid(strDir + strFile + strCreated + strModified + strAttributes + strLength + strError1 + strError2) == false)
            {
                Utilities.Assert(1303.4308, nHeader is int);

                if (nHeader == 0)
                {
                    return "2" + '\t' + "3" + '\t' + "4" + '\t' + "5" + '\t' + "6" + '\t' + "7" + '\t' + "8" + '\t' + "9";
                }
                else if (nHeader == 1)
                {
                    return "Dir" + '\t' + "File" + '\t' + "Created" + '\t' + "Modded" + '\t' + "Attrib" + '\t' + "Length" + '\t' + "Error1" + '\t' + "Error2";
                }
            }

            bool bDbgCheck = false;

            if ((NotNull(strDir).TrimEnd() != NotNull(strDir)) || (NotNull(strFile).TrimEnd() != NotNull(strFile)))
            {
                strError1 += " Trailing whitespace";
                strError1.Trim();
                Utilities.Assert(1303.4309, StrValid(strDir) || StrValid(strFile));
                bDbgCheck = true;
            }

            String strRet = (strDir + '\t' + strFile + '\t' + strCreated + '\t' + strModified + '\t' + strAttributes + '\t' + strLength + '\t' + strError1 + '\t' + strError2).TrimEnd();

            if (bDbgCheck)
            {
#if (DEBUG)
                String[] strArray = strRet.Split('\t');
                DateTime dtParse = DateTime.MinValue;

                if (strArray[nColLENGTH_01].Contains("Trailing whitespace") && DateTime.TryParse(strArray[1], out dtParse))
                {
                    Utilities.Assert(1303.43101, false);
                }
#endif
            }

            return strRet;
        }

        internal static String NotNull(String str)
        {
            return str ?? String.Empty;
        }

        internal static void SetMessageBoxDelegate(MessageBoxDelegate messageBoxCallback)
        {
            static_MessageboxCallback = messageBoxCallback;
        }

        protected static String StrFile_01(String strFile)
        {
            return Path.Combine(Path.GetDirectoryName(strFile),
                Path.GetFileNameWithoutExtension(strFile) + "_01" + Path.GetExtension(strFile));
        }

        internal static bool StrValid(String str)
        {
            return ((str != null) && (str.Length > 0));
        }

        internal static bool ValidateFile(String strSaveAs)
        {
            if (File.Exists(strSaveAs) == false) return false;

            String[] arrLine = File.ReadLines(strSaveAs).Take(1).ToArray();

            if (arrLine.Length <= 0) return false;

            bool bConvertFile = false;

            if (arrLine[0] == m_str_HEADER_01)
            {
                Utilities.WriteLine("Converting " + strSaveAs);
                ConvertFile(strSaveAs);
                Utilities.WriteLine("File converted to " + m_str_HEADER);
                bConvertFile = true;
            }

            String[] arrToken = File.ReadLines(strSaveAs).Take(1).ToArray()[0].Split('\t');

            if (arrToken.Length < 3) return false;
            if (arrToken[2] != m_str_HEADER) return false;

            String[] arrLine_A = File.ReadLines(strSaveAs).Where(s => s.StartsWith(m_strLINETYPE_Length)).ToArray();

            if (arrLine_A.Length == 0) return false;

            String[] arrToken_A = arrLine_A[0].Split('\t');

            if (arrToken_A.Length < 3) return false;
            if (arrToken_A[2] != m_str_TOTAL_LENGTH_LOC) return false;

            String strFile_01 = StrFile_01(strSaveAs);

            if (bConvertFile && File.Exists(strFile_01))
            {
                File.Delete(strFile_01);
            }

            return true;
        }

        static internal void Write(String str)
        {
#if (DEBUG)
            Console.Write(str);
#endif
        }

        static internal void WriteLine(String str = null)
        {
#if (DEBUG)
            Console.WriteLine(str);
#endif
        }
    }

    class Win32FindFile
    {
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct DATUM
        {
            internal FileAttributes fileAttributes;
            internal uint ftCreationTimeLow;
            internal uint ftCreationTimeHigh;
            internal uint ftLastAccessTimeLow;
            internal uint ftLastAccessTimeHigh;
            internal uint ftLastWriteTimeLow;
            internal uint ftLastWriteTimeHigh;
            internal uint nFileSizeHigh;
            internal uint nFileSizeLow;
            internal int dwReserved0;
            internal int dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            internal string strFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            internal string strAltFileName;
        }

        internal enum IndexInfoLevels
        {
            FindExInfoStandard = 0,
            FindExInfoBasic,
            FindExInfoMaxInfoLevel
        };

        private enum IndexSearchOps
        {
            FindExSearchNameMatch = 0,
            FindExSearchLimitToDirectories,
            FindExSearchLimitToDevices
        };

        private const int FIND_FIRST_EX_LARGE_FETCH = 0x02;

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, BestFitMapping = false), SuppressUnmanagedCodeSecurity]
        private static extern IntPtr FindFirstFileExW(string lpFileName, IndexInfoLevels infoLevel, out DATUM lpFindFileData, IndexSearchOps fSearchOp, IntPtr lpSearchFilter, int dwAdditionalFlag);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, BestFitMapping = false), SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FindNextFileW(IntPtr handle, out DATUM lpFindFileData);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FindClose(IntPtr hFindFile);

        private static readonly IntPtr InvalidHandleValue = new IntPtr(-1);

        [Serializable]
        internal class FileData
        {
            FileAttributes m_Attributes;
            DateTime m_CreationTimeUtc;
            DateTime m_LastAccessTimeUtc;
            DateTime m_LastWriteTimeUtc;
            long m_Size;
            bool m_bValid = false;

            internal FileAttributes Attributes { get { return m_Attributes; } }
            internal DateTime CreationTimeUtc { get { return m_CreationTimeUtc; } }
            internal DateTime LastAccessTimeUtc { get { return m_LastAccessTimeUtc; } }
            internal DateTime LastWriteTimeUtc { get { return m_LastWriteTimeUtc; } }
            internal long Size { get { return m_Size; } }
            internal bool IsValid { get { return m_bValid; } }

            internal static bool WinFile(String strFile, out DATUM winFindData)
            {
                String P = Path.DirectorySeparatorChar.ToString();
                String PP = P + P;
                IntPtr handle = FindFirstFileExW(PP + '?' + P + strFile, IndexInfoLevels.FindExInfoBasic, out winFindData, IndexSearchOps.FindExSearchNameMatch, IntPtr.Zero, FIND_FIRST_EX_LARGE_FETCH);

                winFindData.strAltFileName = strFile.Replace(PP, P);                        // 8.3 not used
                return (handle != InvalidHandleValue);
            }

            internal FileData(DATUM findData)
            {
                m_Attributes = findData.fileAttributes;
                m_CreationTimeUtc = ConvertDateTime(findData.ftCreationTimeHigh, findData.ftCreationTimeLow);
                m_LastAccessTimeUtc = ConvertDateTime(findData.ftLastAccessTimeHigh, findData.ftLastAccessTimeLow);
                m_LastWriteTimeUtc = ConvertDateTime(findData.ftLastWriteTimeHigh, findData.ftLastWriteTimeLow);
                m_Size = CombineHighLowInts(findData.nFileSizeHigh, findData.nFileSizeLow);
                m_bValid = (findData.ftCreationTimeHigh | findData.ftCreationTimeLow) != 0;
            }

            internal DateTime CreationTime
            {
                get { return CreationTimeUtc.ToLocalTime(); }
            }

            internal DateTime LastAccessTime
            {
                get { return LastAccessTimeUtc.ToLocalTime(); }
            }

            internal DateTime LastWriteTime
            {
                get { return LastWriteTimeUtc.ToLocalTime(); }
            }

            private static long CombineHighLowInts(uint high, uint low)
            {
                return (((long)high) << 0x20) | low;
            }

            private static DateTime ConvertDateTime(uint high, uint low)
            {
                long fileTime = CombineHighLowInts(high, low);
                return DateTime.FromFileTimeUtc(fileTime);
            }
        }

        internal static bool GetDirectory(String strDir, ref List<DATUM> listDirs, ref List<DATUM> listFiles)
        {
            String P = Path.DirectorySeparatorChar.ToString();
            String PP = P + P;
            DATUM winFindData;
            IntPtr handle = FindFirstFileExW(PP + '?' + P + strDir + P + '*', IndexInfoLevels.FindExInfoBasic, out winFindData, IndexSearchOps.FindExSearchNameMatch, IntPtr.Zero, FIND_FIRST_EX_LARGE_FETCH);

            if (handle == InvalidHandleValue)
            {
                return false;
            }

            listDirs.Clear();
            listFiles.Clear();

            do
            {
                if ("..".Contains(winFindData.strFileName))
                {
                    continue;
                }

                winFindData.strAltFileName = (strDir + P + winFindData.strFileName).Replace(PP, P);     // 8.3 not used

                if ((winFindData.fileAttributes & FileAttributes.Directory) != 0)
                {
                    if ((winFindData.fileAttributes & FileAttributes.ReparsePoint) != 0)
                    {
                        const uint IO_REPARSE_TAG_MOUNT_POINT = 0xA0000003;
                        const uint IO_REPARSE_TAG_SYMLINK = 0xA000000C;

                        // stay on source drive. Treat mount points and symlinks as files.
                        if (((winFindData.dwReserved0 & IO_REPARSE_TAG_MOUNT_POINT) != 0)
                            || ((winFindData.dwReserved0 & IO_REPARSE_TAG_SYMLINK) != 0))
                        {
                            listFiles.Add(winFindData);
                            continue;
                        }
                    }

                    listDirs.Add(winFindData);
                }
                else
                {
                    listFiles.Add(winFindData);
                }
            }
            while (FindNextFileW(handle, out winFindData));

            FindClose(handle);
            listDirs.Sort((x, y) => x.strFileName.CompareTo(y.strFileName));
            listFiles.Sort((x, y) => x.strFileName.CompareTo(y.strFileName));
            return true;
        }
    }
}
