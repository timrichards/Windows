using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Drawing;
using System.Threading;
using System.Diagnostics;

namespace SearchDirLists
{
    class Blinky
    {
        static bool m_bTreeSelect = false;
        internal static bool TreeSelect { get { return m_bTreeSelect; } }

        readonly Control m_defaultControl = null;
        readonly System.Windows.Forms.Timer m_timer = null;

        Holder m_holder = new NullHolder();
        Color m_clrBlink = Color.DarkTurquoise;
        int m_nBlink = 0;
        int m_nNumBlinks = 10;
        bool m_bProgress = false;

        abstract class Holder
        {
            internal Color ClrOrig = Color.Empty;
            internal virtual Color BackColor { get; set; }
            internal virtual void ResetHolder() { }
        }
        class NullHolder : Holder { }
        class TreeNodeHolder : Holder
        {
            readonly TreeNode m_obj = null;
            internal TreeNodeHolder(TreeNode obj) { m_obj = obj; m_bTreeSelect = true; }
            internal override Color BackColor { get { return m_obj.BackColor; } set { m_obj.BackColor = value; } }
            internal override void ResetHolder() { m_bTreeSelect = false; m_obj.TreeView.SelectedNode = m_obj; }
        }
        class ListViewItemHolder : Holder
        {
            readonly ListViewItem m_obj = null;
            internal ListViewItemHolder(ListViewItem obj) { m_obj = obj; }
            internal override Color BackColor { get { return m_obj.BackColor; } set { m_obj.BackColor = value; } }
            internal override void ResetHolder() { m_obj.Selected = true; }
        }
        class ControlHolder : Holder
        {
            protected readonly Control m_obj = null;
            internal ControlHolder(Control obj) { m_obj = obj; }
            internal override Color BackColor { get { return m_obj.BackColor; } set { m_obj.BackColor = value; } }
        }

        internal Blinky(System.Windows.Forms.Timer timer, Control defaultControl)
        {
            m_timer = timer;
            m_defaultControl = defaultControl;
        }

        internal void SelectTreeNode(TreeNode treeNode, bool Once = true)
        {
            Reset();
            m_holder = new TreeNodeHolder(treeNode);
            treeNode.TreeView.Select();
            treeNode.EnsureVisible();
            treeNode.TreeView.SelectedNode = null;
            Go_A(Once: Once);
            m_defaultControl.Select();      // search results UX, and selected treeview
        }

        internal void SelectLVitem(ListViewItem lvItem)
        {
            Reset();
            m_holder = new ListViewItemHolder(lvItem);
            lvItem.EnsureVisible();
            lvItem.Selected = false;
            Go_A(Once: true);
            m_defaultControl.Select();      // search results UX
        }

        internal void Go(Control ctl, Color? clr = null, bool Once = false)
        {
            Reset();
            m_holder = new ControlHolder(ctl);
            Go_A(clr, Once);
        }

        internal void Go(Color? clr = null, bool Once = false, bool bProgress = false)
        {
            Reset();
            m_holder = new ControlHolder(m_defaultControl);
            Go_A(clr, Once, bProgress);
        }

        void Go_A(Color? clr = null, bool Once = false, bool bProgress = false)
        {
            Utilities.Assert(1303.431013, m_timer.Enabled == false, bTraceOnly: true);
            Utilities.Assert(1303.431015, m_nBlink == 0, bTraceOnly: true);
            Utilities.Assert(1303.431017, (m_holder is NullHolder) == false, bTraceOnly: true);
            Utilities.Assert(1303.431019, m_bProgress == false, bTraceOnly: true);

            m_holder.ClrOrig = m_holder.BackColor;
            m_bProgress = bProgress;
            m_clrBlink = clr ?? (bProgress ? Color.LightSalmon : Color.Turquoise);
            m_nBlink = 0;
            m_nNumBlinks = Once ? 2 : 10;
            m_timer.Interval = bProgress ? 500 : (Once ? 100 : 50);
            m_timer.Enabled = true;
        }

        internal void Tick()
        {
            if (m_bProgress || (++m_nBlink < m_nNumBlinks))
            {
                m_holder.BackColor = (m_holder.BackColor == m_clrBlink) ? m_holder.ClrOrig : m_clrBlink;
            }
            else
            {
                Reset();
            }
        }

        internal void Reset()
        {
            m_timer.Enabled = false;
            m_nBlink = 0;
            m_bProgress = false;
            m_holder.BackColor = m_holder.ClrOrig;
            m_holder.ResetHolder();   // has to be last before deleting object.
            m_holder = new NullHolder();
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

        internal static void Go(Control ctl_in = null, bool Once = false)
        {
            Control ctl = ctl_in ?? Form1.static_form;

            Utilities.CheckAndInvoke(ctl, new Action(() =>
            {
                FLASHWINFO fInfo = new FLASHWINFO();

                fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));
                fInfo.hwnd = ctl.Handle;
                fInfo.dwFlags = FLASHW_ALL;
                fInfo.uCount = (uint) (Once ? 1 : 3);
                fInfo.dwTimeout = 0;
                FlashWindowEx(ref fInfo);
            }));
        }
    }

    class LVvolStrings : Utilities
    {
        readonly int m_nIndex = -1;
        readonly string m_strVolumeName = null;
        readonly string m_strPath = null;
        readonly string m_strSaveAs = null;
        string m_strStatus = null;
        readonly string m_strInclude = null;
        readonly string m_strVolumeGroup = null;
        internal int Index { get { return m_nIndex; } }
        internal string VolumeName { get { return m_strVolumeName; } }
        internal string StrPath { get { return m_strPath; } }
        internal string SaveAs { get { return m_strSaveAs; } }
        internal string Status { get { return m_strStatus; } }
        internal string Include { get { return m_strInclude; } }
        internal string VolumeGroup { get { return m_strVolumeGroup; } }

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
                    ((Utilities.mSTRusingFile + Utilities.mSTRsaved).Contains(Status)));
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
        public static string BaseFilter = null;
        public static string FileAndDirListFileFilter = "SearchDirLists listing file|*." + mSTRfileExt_Listing;

        public readonly string Header = null;

        string m_strDescription = null;
        public string Description { get { return m_strDescription + " list file"; } }

        string m_strExt = null;
        public string Filter { get { return "SearchDirLists " + Description + "|*." + m_strExt; } }

        protected static OpenFileDialog static_OpenFileDialog = null;
        protected static SaveFileDialog static_SaveFileDialog = null;

        protected string m_strPrevFile = null;
        protected string m_strFileNotDialog = null;

        internal static void SetFileDialogs(OpenFileDialog ofd_in, SaveFileDialog sfd_in)
        {
            static_OpenFileDialog = ofd_in;
            static_SaveFileDialog = sfd_in;
            BaseFilter = static_OpenFileDialog.Filter;
        }

        protected SDL_File(string strHeader, string strExt, string strDescription)
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

        protected virtual void ReadListItem(UList<ListViewItem> listItems, string[] strArray) { listItems.Add(new ListViewItem(strArray)); }

        internal bool ReadList(ListView lv)
        {
            if ((m_strFileNotDialog == null) && (ShowDialog(static_OpenFileDialog) == false))
            {
                return false;
            }

            UList<ListViewItem> listItems = new UList<ListViewItem>();

            using (StreamReader sr = File.OpenText(m_strFileNotDialog))
            {
                string strLine = sr.ReadLine();

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
                m_MessageboxCallback("Not a valid " + Description + ".", "Load " + Description);
            }

            return (listItems.Count > 0);
        }

        protected virtual string WriteListItem(int nIndex, string str) { return str; }

        internal bool WriteList(ListView.ListViewItemCollection lvItems)
        {
            if (ShowDialog(static_SaveFileDialog) == false)
            {
                return false;
            }

            if ((File.Exists(m_strPrevFile))
                && (m_MessageboxCallback(m_strPrevFile + " already exists. Overwrite?", Description, MessageBoxButtons.YesNo)
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
        internal SDL_VolumeFile(string strFile = null) : base(mSTRvolListHeader, mSTRfileExt_Volume, "volume") { m_strFileNotDialog = strFile; }

        protected override void ReadListItem(UList<ListViewItem> listItems, string[] strArray)
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

        protected override string WriteListItem(int nIndex, string str)
        {
            return (nIndex == 1) ? str.TrimEnd(Path.DirectorySeparatorChar) : str;
        }
    }

    class SDL_CopyFile : SDL_File { internal SDL_CopyFile() : base(mSTRcopyScratchpadHeader, mSTRfileExt_Copy, "copy") { } }

    class SDL_IgnoreFile : SDL_File { internal SDL_IgnoreFile(string strFile = null) : base(mSTRignoreListHeader, mSTRfileExt_Ignore, "ignore") { m_strFileNotDialog = strFile; } }

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
        internal const string mSTRheader01 = "SearchDirLists 0.1";
        internal const string mSTRstart01 = mSTRheader01 + " START";
        internal const string mSTRend01 = mSTRheader01 + " END";
        internal const string mSTRerrorsLoc01 = mSTRheader01 + " ERRORS";
        internal const string mSTRtotalLengthLoc01 = mSTRheader01 + " LENGTH";
        internal const string mSTRdrive01 = mSTRheader01 + " DRIVE";
        internal const string mSTRvolListHeader01 = mSTRheader01 + " VOLUME LIST";

        internal const string mSTRheader = "SearchDirLists 0.2";
        internal const string mSTRstart = mSTRheader + " START";
        internal const string mSTRend = mSTRheader + " END";
        internal const string mSTRerrorsLoc = mSTRheader + " ERRORS";
        internal const string mSTRtotalLengthLoc = mSTRheader + " LENGTH";
        internal const string mSTRdrive = mSTRheader + " DRIVE";
        internal const string mSTRvolListHeader = mSTRheader + " VOLUME LIST";
        internal const string mSTRcopyScratchpadHeader = mSTRheader + " COPYDIRS LIST";
        internal const string mSTRignoreListHeader = mSTRheader + " IGNORE LIST";
        internal const string mSTRusingFile = "Using file.";
        internal const string mSTRsaved = "Saved.";
        internal const string mSTRnotSaved = "Not saved.";

        internal const int mNcolLength = 7;
        internal const int mNcolLength01 = 5;
        internal const int mNcolLengthLV = 4;

        internal const string mSTRlineType_Version = "V";
        internal const string mSTRlineType_Nickname = "N";
        internal const string mSTRlineType_Path = "P";
        internal const string mSTRlineType_DriveInfo = "I";
        internal const string mSTRlineType_Comment = "C";
        internal const string mSTRlineType_Start = "S";
        internal const string mSTRlineType_Directory = "D";
        internal const string mSTRlineType_File = "F";
        internal const string mSTRlineType_End = "E";
        internal const string mSTRlineType_Blank = "B";
        internal const string mSTRlineType_ErrorDir = "R";
        internal const string mSTRlineType_ErrorFile = "r";
        internal const string mSTRlineType_Length = "L";

        internal const string mSTRfileExt_Listing = "sdl_list";
        internal const string mSTRfileExt_Volume = "sdl_vol";
        internal const string mSTRfileExt_Copy = "sdl_copy";
        internal const string mSTRfileExt_Ignore = "sdl_ignore";

        static MessageBoxDelegate static_MessageboxCallback = null;
        protected MessageBoxDelegate m_MessageboxCallback = null;
        static double static_nLastAssertLoc = -1;
        static DateTime static_dtLastAssert = DateTime.MinValue;

#if (DEBUG == false)
        static bool static_bAssertUp = false;
#endif

        protected Utilities()
        {
            m_MessageboxCallback = static_MessageboxCallback;
        }

        internal static bool Assert(double nLocation, bool bCondition, string strError_in = null, bool bTraceOnly = false)
        {
            if (bCondition) return true;

            if ((static_nLastAssertLoc == nLocation) && ((DateTime.Now - static_dtLastAssert).Seconds < 1))
            {
                return false;
            }

            string strError = "Assertion failed at location " + nLocation + ".";

            if (StrValid(strError_in))
            {
                strError += "\n\nAdditional information: " + strError_in;
            }

            Utilities.WriteLine(strError);
#if (DEBUG)
            Debug.Assert(false, strError);
#else
            if (static_bAssertUp == false)
            {
                bool bTrace = false; // Trace.Listeners.Cast<TraceListener>().Any(i => i is DefaultTraceListener);

                Action messageBox = new Action(() =>
                {
                    static_MessageboxCallback(strError + "\n\nPlease discuss this bug at http://sourceforge.net/projects/searchdirlists/.".PadRight(100), "SearchDirLists Assertion Failure");
                    static_bAssertUp = false;
                });

                if (bTrace)
                {
                    messageBox();
                }
                else if (bTraceOnly == false)
                {
                    static_nLastAssertLoc = nLocation;
                    static_dtLastAssert = DateTime.Now;
                    static_bAssertUp = true;
                    new Thread(new ThreadStart(messageBox)).Start();
                }
            }
#endif
            return false;
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

        internal static string CheckNTFS_chars(ref string strFile, bool bFile = false)
        {
            char[] arrChar = bFile ? Path.GetInvalidFileNameChars() : Path.GetInvalidPathChars();
            int nIx = -1;

            if ((nIx = strFile.IndexOfAny(arrChar)) > -1)
            {
                string strRet = "NTFS ASCII " + ((int)strFile[nIx]).ToString();

                strFile = strFile.Replace("\n", "").Replace("\r", "").Replace("\t", "");    // program-incompatible
                return strRet;
            }
            else
            {
                return null;
            }
        }

        internal static void ConvertFile(string strFile)
        {
            string strFile_01 = StrFile_01(strFile);

            if (File.Exists(strFile_01))
            {
                File.Delete(strFile_01);
            }

            File.Move(strFile, strFile_01);

            using (StreamWriter file_out = new StreamWriter(strFile))
            {
                using (StreamReader file_in = new StreamReader(strFile_01))
                {
                    string strLine = null;
                    long nLineNo = 0;       // lines number from one
                    bool bAtErrors = false;

                    while ((strLine = file_in.ReadLine()) != null)
                    {
                        ++nLineNo;

                        if (strLine == mSTRheader01)
                        {
                            Utilities.Assert(1303.4302, nLineNo == 1);
                            file_out.WriteLine(FormatLine(mSTRlineType_Version, nLineNo, mSTRheader));
                            continue;
                        }
                        else if (nLineNo == 2)
                        {
                            file_out.WriteLine(FormatLine(mSTRlineType_Nickname, nLineNo, strLine));
                            continue;
                        }
                        else if (nLineNo == 3)
                        {
                            file_out.WriteLine(FormatLine(mSTRlineType_Path, nLineNo, strLine));
                            continue;
                        }
                        else if (strLine == mSTRdrive01)
                        {
                            Utilities.Assert(1303.4303, nLineNo == 4);
                            file_out.WriteLine(FormatLine(mSTRlineType_Comment, nLineNo, mSTRdrive));

                            for (int i = 0; i < 8; ++i)
                            {
                                strLine = file_in.ReadLine();
                                file_out.WriteLine(FormatLine(mSTRlineType_DriveInfo, nLineNo, strLine));
                                ++nLineNo;
                            }

                            continue;
                        }
                        else if (strLine.Length <= 0)
                        {
                            file_out.WriteLine(FormatLine(mSTRlineType_Blank, nLineNo));
                            continue;
                        }
                        else if (nLineNo == 14)
                        {
                            file_out.WriteLine(FormatLine(mSTRlineType_Comment, nLineNo, FormatString(nHeader: 0)));
                            continue;
                        }
                        else if (nLineNo == 15)
                        {
                            file_out.WriteLine(FormatLine(mSTRlineType_Comment, nLineNo, FormatString(nHeader: 1)));
                            continue;
                        }
                        else if (strLine.StartsWith(mSTRstart01))
                        {
                            Utilities.Assert(1303.4304, nLineNo == 16);
                            file_out.WriteLine(FormatLine(mSTRlineType_Start, nLineNo, mSTRstart));
                            continue;
                        }
                        else if (strLine.StartsWith(mSTRend01))
                        {
                            file_out.WriteLine(FormatLine(mSTRlineType_End, nLineNo, mSTRend));
                            continue;
                        }
                        else if (strLine == mSTRerrorsLoc01)
                        {
                            file_out.WriteLine(FormatLine(mSTRlineType_Comment, nLineNo, mSTRerrorsLoc));
                            bAtErrors = true;
                            continue;
                        }
                        else if (strLine.StartsWith(mSTRtotalLengthLoc01))
                        {
                            string[] arrLine = strLine.Split('\t');

                            file_out.WriteLine(FormatLine(mSTRlineType_Length, nLineNo, FormatString(strDir: mSTRtotalLengthLoc, nLength: long.Parse(arrLine[mNcolLength01]))));
                            continue;
                        }

                        string[] strArray = strLine.Split('\t');
                        string strDir = strArray[0];

                        if (StrValid(strDir) == false)
                        {
                            DateTime dtParse;
                            string strTab = null;

                            if ((strArray.Length > 5) && strArray[5].Contains("Trailing whitespace") && DateTime.TryParse(strArray[1], out dtParse))
                            {
                                strTab = "\t";
                            }

                            file_out.WriteLine(FormatLine(bAtErrors ? mSTRlineType_ErrorFile : mSTRlineType_File, nLineNo, strTab + strLine));
                            continue;
                        }
                        else if (strDir.Contains(":" + Path.DirectorySeparatorChar) == false)
                        {
                            Utilities.Assert(1303.4305, false);        // all that's left is directories
                            continue;
                        }

                        // directory
                        string P = Path.DirectorySeparatorChar.ToString();
                        string PP = P + P;
                        string str = strLine.Replace(PP, P);

                        file_out.WriteLine(FormatLine(bAtErrors ? mSTRlineType_ErrorDir : mSTRlineType_Directory, nLineNo, str));
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

        internal static bool FormatPath(ref string strPath, bool bFailOnDirectory = true)
        {
            while (strPath.Contains(@"\\"))
            {
                strPath = strPath.Replace(@"\\", @"\");
            }

            string strDirName = Path.GetDirectoryName(strPath);

            if ((StrValid(strDirName) == false) || Directory.Exists(strDirName))
            {
                string strCapDrive = strPath.Substring(0, strPath.IndexOf(":" + Path.DirectorySeparatorChar) + 2);

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

        internal static bool FormatPath(ref string strPath, ref string strSaveAs, bool bFailOnDirectory = true)
        {
            if (StrValid(strPath))
            {
                strPath += Path.DirectorySeparatorChar;

                if (FormatPath(ref strPath, bFailOnDirectory) == false)
                {
                    static_MessageboxCallback("Error in Source path.", "Save Directory Listing");
                    return false;
                }
            }

            if (StrValid(strSaveAs))
            {
                strSaveAs = Path.GetFullPath(strSaveAs.Trim());

                if (FormatPath(ref strSaveAs, bFailOnDirectory) == false)
                {
                    static_MessageboxCallback("Error in Save filename.", "Save Directory Listing");
                    return false;
                }
            }

            return true;
        }

        static string FormatLine(string strLineType, long nLineNo, string strLine_in = null)
        {
            string strLine_out = strLineType + "\t" + nLineNo;

            if (StrValid(strLine_in))
            {
                strLine_out += '\t' + strLine_in;
            }

            return strLine_out;
        }

        internal static string FormatSize(string in_str, bool bBytes = false)
        {
            return FormatSize(ulong.Parse(in_str), bBytes);
        }

        internal static string FormatSize(long nLength, bool bBytes = false, bool bNoDecimal = false)
        {
            return FormatSize((ulong)nLength, bBytes, bNoDecimal);
        }

        internal static string FormatSize(ulong nLength, bool bBytes = false, bool bNoDecimal = false)
        {
            double nT = nLength / 1024.0 / 1024.0 / 1024 / 1024 - .05;
            double nG = nLength / 1024.0 / 1024 / 1024 - .05;
            double nM = nLength / 1024.0 / 1024 - .05;
            double nK = nLength / 1024.0 - .05;     // Windows Explorer seems to not round
            string strFmt_big = "###,##0.0";
            string strFormat = bNoDecimal ? "###,###" : strFmt_big;
            string strSz = null;

            if (((int)nT) > 0) strSz = nT.ToString(strFmt_big) + " TB";
            else if (((int)nG) > 0) strSz = nG.ToString(strFmt_big) + " GB";
            else if (((int)nM) > 0) strSz = nM.ToString(strFormat) + " MB";
            else if (((int)nK) > 0) strSz = nK.ToString(strFormat) + " KB";
            else strSz = "1 KB";                    // Windows Explorer mins at 1K

            if (nLength > 0)
            {
                return strSz + (bBytes ? (" (" + nLength.ToString("###,###,###,###,###") + " bytes)") : string.Empty);
            }
            else
            {
                return "0 bytes";
            }
        }

        internal static string FormatString(string strDir = null, string strFile = null, DateTime? dtCreated = null, DateTime? dtModified = null, string strAttributes = null, long nLength = -1, string strError1 = null, string strError2 = null, int? nHeader = null)
        {
            string strLength = null;
            string strCreated = null;
            string strModified = null;

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

            string strRet = (strDir + '\t' + strFile + '\t' + strCreated + '\t' + strModified + '\t' + strAttributes + '\t' + strLength + '\t' + strError1 + '\t' + strError2).TrimEnd();

            if (bDbgCheck)
            {
#if (DEBUG)
                string[] strArray = strRet.Split('\t');
                DateTime dtParse = DateTime.MinValue;

                if (strArray[mNcolLength01].Contains("Trailing whitespace") && DateTime.TryParse(strArray[1], out dtParse))
                {
                    Utilities.Assert(1303.43101, false);
                }
#endif
            }

            return strRet;
        }

        internal static string NotNull(string str)
        {
            return str ?? string.Empty;
        }

        internal static void SetMessageBoxDelegate(MessageBoxDelegate messageBoxCallback)
        {
            static_MessageboxCallback = messageBoxCallback;
        }

        protected static string StrFile_01(string strFile)
        {
            return Path.Combine(Path.GetDirectoryName(strFile),
                Path.GetFileNameWithoutExtension(strFile) + "_01" + Path.GetExtension(strFile));
        }

        internal static bool StrValid(string str)
        {
            return ((str != null) && (str.Length > 0));
        }

        internal static bool ValidateFile(string strSaveAs)
        {
            if (File.Exists(strSaveAs) == false) return false;

            string[] arrLine = File.ReadLines(strSaveAs).Take(1).ToArray();

            if (arrLine.Length <= 0) return false;

            bool bConvertFile = false;

            if (arrLine[0] == mSTRheader01)
            {
                Utilities.WriteLine("Converting " + strSaveAs);
                ConvertFile(strSaveAs);
                Utilities.WriteLine("File converted to " + mSTRheader);
                bConvertFile = true;
            }

            string[] arrToken = File.ReadLines(strSaveAs).Take(1).ToArray()[0].Split('\t');

            if (arrToken.Length < 3) return false;
            if (arrToken[2] != mSTRheader) return false;

            string[] arrLine_A = File.ReadLines(strSaveAs).Where(s => s.StartsWith(mSTRlineType_Length)).ToArray();

            if (arrLine_A.Length <= 0) return false;

            string[] arrToken_A = arrLine_A[0].Split('\t');

            if (arrToken_A.Length < 3) return false;
            if (arrToken_A[2] != mSTRtotalLengthLoc) return false;

            string strFile_01 = StrFile_01(strSaveAs);

            if (bConvertFile && File.Exists(strFile_01))
            {
                File.Delete(strFile_01);
            }

            return true;
        }

        static internal void Write(string str)
        {
#if (DEBUG)
            Console.Write(str);
#endif
        }

        static internal void WriteLine(string str = null)
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

            internal static bool WinFile(string strFile, out DATUM winFindData)
            {
                string P = Path.DirectorySeparatorChar.ToString();
                string PP = P + P;
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

        internal static bool GetDirectory(string strDir, ref List<DATUM> listDirs, ref List<DATUM> listFiles)
        {
            string P = Path.DirectorySeparatorChar.ToString();
            string PP = P + P;
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

    class WPF_Form
    {
        internal static DialogResult WPFMessageBox(string strMessage, string strTitle = null, MessageBoxButtons? buttons = null)
        {
            DialogResult dlgRet = DialogResult.None;

            if (buttons == null)
            {
                dlgRet = MessageBox.Show(strMessage.PadRight(100), strTitle);
            }
            else
            {
                dlgRet = MessageBox.Show(strMessage.PadRight(100), strTitle, buttons.Value);
            }

            return dlgRet;
        }
    }
}
