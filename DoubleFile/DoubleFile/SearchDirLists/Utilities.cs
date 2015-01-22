﻿using System.Windows.Forms;
using System.Threading;         // release mode
using System.Diagnostics;       // debug mode
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Threading;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Input;
using System.Collections;
using System.Text;
using System.Drawing;
using DoubleFile;

namespace SearchDirLists
{
    delegate bool BoolAction();
    delegate MBoxRet MBoxDelegate(string strMessage, string strTitle = null, MBoxBtns? buttons = null);

    [System.ComponentModel.DesignerCategory("Code")]
    class SDL_Win : Form { }
    enum MBoxBtns { OK = MessageBoxButtons.OK, YesNo = MessageBoxButtons.YesNo, YesNoCancel = MessageBoxButtons.YesNoCancel }
    enum MBoxRet { None = DialogResult.None, Yes = DialogResult.Yes, No = DialogResult.No }

    class SDL_ListView : ListViewEmbeddedControls.ListViewEx { }

    class SDL_LVItemCollection : ListView.ListViewItemCollection
    {
        internal SDL_LVItemCollection(ListView lv) : base(lv) { }
    }

    class SDL_ListViewItem : ListViewItem
    {
        public SDL_ListViewItem() : base() { }
        internal SDL_ListViewItem(string strContent) : base(strContent) { }
        internal SDL_ListViewItem(string[] arrString) : base(arrString) { }
        internal void Select(bool bSel = true) { Selected = bSel; }
    }

    [System.ComponentModel.DesignerCategory("Code")]
    public class SDL_TreeView : TreeView
    {
        // enable double buffer

        public SDL_TreeView()
        {
            DoubleBuffered = true;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            SendMessage(this.Handle, TVM_SETEXTENDEDSTYLE, (IntPtr)TVS_EX_DOUBLEBUFFER, (IntPtr)TVS_EX_DOUBLEBUFFER);
            base.OnHandleCreated(e);
        }
        private const int TVM_SETEXTENDEDSTYLE = 0x1100 + 44;
        private const int TVM_GETEXTENDEDSTYLE = 0x1100 + 45;
        private const int TVS_EX_DOUBLEBUFFER = 0x0004;
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wp, IntPtr lp);

        // suppress double click bug in treeview. Affects checkboxes

        protected override void WndProc(ref Message m)
        {
            // Suppress WM_LBUTTONDBLCLK
            if (m.Msg == 0x203) { m.Result = IntPtr.Zero; }
            else base.WndProc(ref m);
        }
    }

    class Blinky
    {
        static bool m_bTreeSelect = false;
        internal static bool TreeSelect { get { return m_bTreeSelect; } }

        readonly Control m_defaultControl = null;
        readonly SDL_Timer m_timer = new SDL_Timer();

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
            readonly SDL_ListViewItem m_obj = null;
            internal ListViewItemHolder(SDL_ListViewItem obj) { m_obj = obj; }
            internal override Color BackColor { get { return m_obj.BackColor; } set { m_obj.BackColor = value; } }
            internal override void ResetHolder() { m_obj.Select(); }
        }
        class ControlHolder : Holder
        {
            protected readonly Control m_obj = null;
            internal ControlHolder(Control obj) { m_obj = obj; }
        }

        internal Blinky(Control defaultControl)
        {
            m_defaultControl = defaultControl;
            m_timer.Tick += new EventHandler((object sender, EventArgs e) =>
            {
                if (m_bProgress || (++m_nBlink < m_nNumBlinks))
                {
                    m_holder.BackColor = (m_nBlink % 2 == 0) ? m_holder.ClrOrig : m_clrBlink;
                }
                else
                {
                    Reset();
                }
            });
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

        internal void SelectLVitem(SDL_ListViewItem lvItem)
        {
            Reset();
            m_holder = new ListViewItemHolder(lvItem);
            lvItem.EnsureVisible();
            lvItem.Select(false);
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
            Utilities.Assert(1303.4301, m_timer.IsEnabled == false, bTraceOnly: true);
            Utilities.Assert(1303.4302, m_nBlink == 0, bTraceOnly: true);
            Utilities.Assert(1303.4303, (m_holder is NullHolder) == false, bTraceOnly: true);
            Utilities.Assert(1303.4304, m_bProgress == false, bTraceOnly: true);

            m_holder.ClrOrig = m_holder.BackColor;
            m_bProgress = bProgress;
            m_clrBlink = clr ?? (bProgress ? Color.LightSalmon : Color.Turquoise);
            m_nBlink = 0;
            m_nNumBlinks = Once ? 2 : 10;
            m_timer.Interval = new TimeSpan(0, 0, 0, 0, bProgress ? 500 : (Once ? 100 : 50));
            m_timer.Start();
        }

        internal void Reset()
        {
            m_timer.Stop();
            m_nBlink = 0;
            m_bProgress = false;
            m_holder.BackColor = m_holder.ClrOrig;
            m_holder.ResetHolder();   // has to be last before deleting object.
            m_holder = new NullHolder();
        }
    }

    static class ExtensionMethods
    {
        public static string ToPrintString(this object source)
        {
            if (source == null) return null;

            string s = string.Join("", source.ToString().Cast<char>().Where(c => Char.IsControl(c) == false)).Trim();

            if (s.Length == 0) return null;                             // Returns null if empty

            return s;
        }

        public static int Count<T>(this IEnumerable<T> source)
        {
            ICollection<T> c = source as ICollection<T>;

            if (c != null)
            {
                return c.Count;
            }

            Utilities.WriteLine("Count<" + source + "> is not an ICollection: must GetEnumerator()");

            using (IEnumerator<T> enumerator = source.GetEnumerator())
            {
                int result = 0;

                while (enumerator.MoveNext())
                {
                    result++;
                }

                return result;
            }
        }

        public static void FirstOnly<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action(item);
                break;
            }
        }

        public static void FirstOnlyAssert<T>(this IEnumerable<T> source, Action<T> action)
        {
            Utilities.Assert(0, source.Count() <= 1);
            FirstOnly(source, action);
        }

        internal static bool IsChildOf(this TreeNode child, TreeNode treeNode)
        {
            if (child.Level <= treeNode.Level)
            {
                return false;
            }

            TreeNode parentNode = (TreeNode)child.Parent;

            while (parentNode != null)
            {
                if (parentNode == treeNode)
                {
                    return true;
                }

                parentNode = (TreeNode)parentNode.Parent;
            }

            return false;
        }

        internal static TreeNode Root(this TreeNode treeNode)
        {
            TreeNode nodeParent = treeNode;

            while (nodeParent.Parent != null)
            {
                nodeParent = (TreeNode)nodeParent.Parent;
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
            Control dispatcher = ctl_in ?? GlobalDataSDL.static_form;
            Utilities.CheckAndInvoke(dispatcher, new Action(() =>
            {
                FLASHWINFO fInfo = new FLASHWINFO();

                fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));

                if (ctl_in != null)
                {
                    fInfo.hwnd = ctl_in.Handle;
                }
                else
                {
                    fInfo.hwnd = GlobalDataSDL.static_form.Handle;
                }

                fInfo.dwFlags = FLASHW_ALL;
                fInfo.uCount = (uint)(Once ? 1 : 3);
                fInfo.dwTimeout = 0;
                FlashWindowEx(ref fInfo);
            }));
        }
    }

    // SearchDirLists listing file|*.sdl_list|SearchDirLists volume list file|*.sdl_vol|SearchDirLists copy scratchpad file|*.sdl_copy|SearchDirLists ignore list file|*.sdl_ignore
    abstract class SDL_File : Utilities
    {
        internal const string BaseFilter = "Text files|*.txt|All files|*.*";
        internal const string FileAndDirListFileFilter = "SearchDirLists listing file|*." + mSTRfileExt_Listing;

        internal readonly string Header = null;

        string m_strDescription = null;
        internal string Description { get { return m_strDescription + " list file"; } }

        string m_strExt = null;
        internal string Filter { get { return "SearchDirLists " + Description + "|*." + m_strExt; } }

        internal static SaveFileDialog SFD = null;        // TODO: remove frankenSFD

        protected string m_strPrevFile = null;
        protected string m_strFileNotDialog = null;

        static bool bInited = false;

        internal static void Init()
        {
            if (bInited)
            {
                return;
            }

            SFD = new SaveFileDialog();
            SFD.OverwritePrompt = false;
            bInited = true;
        }

        protected SDL_File(string strHeader, string strExt, string strDescription)
        {
            Init();
            Utilities.Assert(1303.4306, SFD != null);
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

        protected virtual void ReadListItem(ListView lv, string[] strArray) { lv.Items.Add(new SDL_ListViewItem(strArray)); }

        internal bool ReadList(ListView lv_in)
        {
            int nCols = lv_in.Columns.Count;
            ListView lv = new ListView();       // fake
            if ((m_strFileNotDialog == null) && (ShowDialog(new OpenFileDialog()) == false))
            {
                return false;
            }

            if (Keyboard.IsKeyDown(Key.LeftShift) == false)
            {
                lv_in
                .Items.Clear();
            }

            using (StreamReader sr = File.OpenText(m_strFileNotDialog))
            {
                string strLine = sr.ReadLine();

                if (strLine == Header)
                {
                    while ((strLine = sr.ReadLine()) != null)
                    {
                        ReadListItem(lv, strLine.TrimEnd(new char[] { '\t' }).Split('\t').Take(nCols).ToArray());
                    }
                }
            }

            if (lv.Items.Count > 0)
            {
                ListViewItem[] lvItems = lv.Items.Cast<ListViewItem>().ToArray();
                lv.Items.Clear();
                lv_in.Items.AddRange(lvItems);
                lv_in.Invalidate();
                return true;
            }
            else
            {
                MBox("Not a valid " + Description + ".", "Load " + Description);
                return false;
            }
        }

        protected virtual string WriteListItem(int nIndex, string str) { return str; }

        internal bool WriteList(ListView.ListViewItemCollection lvItems)
        {
            if (ShowDialog(SFD) == false)
            {
                return false;
            }

            if ((File.Exists(m_strPrevFile))
                && (MBox(m_strPrevFile + " already exists. Overwrite?", Description, MBoxBtns.YesNo)
                != MBoxRet.Yes))
            {
                return false;
            }

            using (StreamWriter sw = File.CreateText(m_strPrevFile))
            {
                sw.WriteLine(Header);
                foreach (SDL_ListViewItem lvItem in lvItems)
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
        protected override void ReadListItem(ListView lv, string[] strArray)
        {
            if (strArray.Length < 4)
            {
                return;
            }

            strArray[3] = mSTRusingFile;

            if (File.Exists(strArray[2]) == false)
            {
                strArray[2] = Path.Combine(Path.GetDirectoryName(m_strPrevFile), Path.GetFileName(strArray[2]));

                if (File.Exists(strArray[2]) == false)
                {
                    if (Directory.Exists(strArray[1]))
                    {
                        strArray[3] = "No file. Will create.";
                    }
                    else
                    {
                        strArray[3] = mSTRcantSave;
                    }
                }
            }

            strArray[1] = strArray[1].TrimEnd('\\');
            lv.Items.Add(new SDL_ListViewItem(strArray));
        }

        protected override string WriteListItem(int nIndex, string str)
        {
            return (nIndex == 1) ? str.TrimEnd('\\') : str;
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

    class SDL_Timer : DispatcherTimer
    {
        internal SDL_Timer()
            : base()
        { }

        internal SDL_Timer(TimeSpan interval, DispatcherPriority priority, EventHandler callback, Dispatcher dispatcher)
            : base(interval, priority, callback, dispatcher)
        { }

        internal new void Start()
        {
            if (Utilities.Assert(0, Interval.TotalMilliseconds > 0))
            {
                base.Start();
            }
        }
    }

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
        internal const string mSTRcantSave = "Can't save. Not mounted.";

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

        const int knDriveInfoItems = 11;
        internal static readonly string[] mAstrDIlabels = new string[knDriveInfoItems]
        {
            "Volume Free",
            "Volume Format",
            "Drive Type",           // DriveInfo
            "Volume Name",
            "Volume Root",
            "Volume Free 2",
            "Volume Size",
            "Volume Label",
            "Drive Model",          // These last three are CIM items
            "Drive Serial",
            "Drive Size"
        };
        internal static readonly bool[] mAbDIsizeType = new bool[knDriveInfoItems]
        {
            true, false, false, false, false, true, true, false, false, false, true
        };
        internal static readonly int[] mAnDIviewOrder = new int[knDriveInfoItems]
        {
            9, 5, 6, 2, 0, 10, 8, 1, 3, 4, 7
        };
        internal static readonly int[] mAnDIoptIfEqTo = new int[knDriveInfoItems]
        {
            -1, -1, -1, 4, -1, 0, -1, -1, -1, -1, -1
        };

        static SDL_Win m_form1MessageBoxOwner = null;
        static double static_nLastAssertLoc = -1;
        static DateTime static_dtLastAssert = DateTime.MinValue;

#if (DEBUG == false)
        static bool static_bAssertUp = false;
#endif

        internal static bool Assert(double nLocation, bool bCondition, string strError_in = null, bool bTraceOnly = false)
        {
            if (bCondition) return true;

            if ((static_nLastAssertLoc == nLocation) && ((DateTime.Now - static_dtLastAssert).Seconds < 1))
            {
                return false;
            }

            string strError = "Assertion failed at location " + nLocation + ".";

            if (false == string.IsNullOrWhiteSpace(strError_in))
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
                    MBox(strError + "\n\nPlease discuss this bug at http://sourceforge.net/projects/searchdirlists/.".PadRight(100), "SearchDirLists Assertion Failure");
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

        internal static object CheckAndInvoke(Control dispatcher, Delegate action, object[] args = null)
        {
            bool bInvoke = dispatcher.InvokeRequired;
            if (GlobalData.AppExit)
            {
                return null;
            }

            if (bInvoke)
            {
                if (args == null)
                {
                    return dispatcher.Invoke(action);
                }
                else
                {
                    return dispatcher.Invoke(action, (object)args);
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
                            Utilities.Assert(1303.4307, nLineNo == 1);
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
                            Utilities.Assert(1303.4308, nLineNo == 4);
                            file_out.WriteLine(FormatLine(mSTRlineType_Comment, nLineNo, mSTRdrive));

                            int ixDriveInfo = 0;
                            for (; ixDriveInfo < mAstrDIlabels.Length; ++ixDriveInfo)
                            {
                                strLine = file_in.ReadLine();
                                ++nLineNo;

                                if (strLine.Length <= 0)
                                {
                                    break;
                                }

                                file_out.WriteLine(FormatLine(mSTRlineType_DriveInfo, nLineNo, strLine));
                            }

                            if (ixDriveInfo == mAstrDIlabels.Length)
                            {
                                strLine = file_in.ReadLine();
                                ++nLineNo;
                            }
                            
                            file_out.WriteLine(FormatLine(mSTRlineType_Blank, nLineNo));
                            ++nLineNo;
                            strLine = file_in.ReadLine();
                            file_out.WriteLine(FormatLine(mSTRlineType_Comment, nLineNo, FormatString(nHeader: 0)));
                            ++nLineNo;
                            strLine = file_in.ReadLine();
                            file_out.WriteLine(FormatLine(mSTRlineType_Comment, nLineNo, FormatString(nHeader: 1)));
                            continue;
                        }
                        else if (strLine.Length <= 0)
                        {
                            file_out.WriteLine(FormatLine(mSTRlineType_Blank, nLineNo));
                            continue;
                        }
                        else if (strLine.StartsWith(mSTRstart01))
                        {
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

                        string[] arrLine_A = strLine.Split('\t');
                        string strDir = arrLine_A[0];

                        if (string.IsNullOrWhiteSpace(strDir))
                        {
                            DateTime dtParse;
                            string strTab = null;

                            if ((arrLine_A.Length > 5) && arrLine_A[5].Contains("Trailing whitespace") && DateTime.TryParse(arrLine_A[1], out dtParse))
                            {
                                strTab = "\t";
                            }

                            file_out.WriteLine(FormatLine(bAtErrors ? mSTRlineType_ErrorFile : mSTRlineType_File, nLineNo, strTab + strLine));
                            continue;
                        }
                        else if (strDir.Contains(@":\") == false)
                        {
                            Utilities.Assert(1303.4311, false);        // all that's left is directories
                            continue;
                        }

                        // directory
                        file_out.WriteLine(FormatLine(bAtErrors ? mSTRlineType_ErrorDir : mSTRlineType_Directory, nLineNo, strLine.Replace(@"\\", @"\")));
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
                    nCount += CountNodes((TreeNode)treeNode.Nodes[0]);
                }

                ++nCount;
            }
            while (bNextNode && ((treeNode = (TreeNode)treeNode.NextNode) != null));

            return nCount;
        }

        internal static string DecodeAttributes(string strAttr)
        {
            FileAttributes nAttr = (FileAttributes)Convert.ToInt32(strAttr, 16);
            string str = "";

            if ((nAttr & FileAttributes.ReparsePoint) != 0) str += " ReparsePoint";
            if ((nAttr & FileAttributes.Normal) != 0) str += " Normal";
            if ((nAttr & FileAttributes.Hidden) != 0) str += " Hidden";
            if ((nAttr & FileAttributes.ReadOnly) != 0) str += " Readonly";
            if ((nAttr & FileAttributes.Archive) != 0) str += " Archive";
            if ((nAttr & FileAttributes.Compressed) != 0) str += " Compressed";
            if ((nAttr & FileAttributes.System) != 0) str += " System";
            if ((nAttr & FileAttributes.Temporary) != 0) str += " Tempfile";
            if ((nAttr & FileAttributes.Directory) != 0) str += " Directory";

            str = str.TrimStart();

            if (str.Length == 0) str = strAttr;
            else str += " (" + strAttr + ")";

            return str;
        }

        internal static bool FormatPath(ref string strPath, bool bFailOnDirectory = true)
        {
            while (strPath.Contains(@"\\"))
            {
                strPath = strPath.Replace(@"\\", @"\");
            }

            string strDirName = Path.GetDirectoryName(strPath);

            if (string.IsNullOrWhiteSpace(strDirName) || Directory.Exists(strDirName))
            {
                string strCapDrive = strPath.Substring(0, strPath.IndexOf(@":\") + 2);

                strPath = Path.GetFullPath(strPath).Replace(strCapDrive, strCapDrive.ToUpper());

                if (strPath == strCapDrive.ToUpper())
                {
                    Utilities.Assert(1303.4312, strDirName == null);
                }
                else
                {
                    strPath = strPath.TrimEnd('\\');
                    Utilities.Assert(1303.4313, false == string.IsNullOrWhiteSpace(strDirName));
                }
            }
            else if (bFailOnDirectory)
            {
                return false;
            }

            return true;
        }

        static string FormatLine(string strLineType, long nLineNo, string strLine_in = null)
        {
            string strLine_out = strLineType + "\t" + nLineNo;

            if (false == string.IsNullOrWhiteSpace(strLine_in))
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

        internal static string FormatString(string strDir = null, string strFile = null, DateTime? dtCreated = null, DateTime? dtModified = null, string strAttributes = null, long nLength = -1, string strError1 = null, string strError2 = null, int? nHeader = null, string strChecksum = null)
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

            if (string.IsNullOrWhiteSpace(strDir + strFile + strCreated + strModified + strAttributes + strLength + strError1 + strError2 + strChecksum))
            {
                Utilities.Assert(1303.4314, nHeader is int);

                if (nHeader == 0)
                {
                    return "2" + '\t' + "3" + '\t' + "4" + '\t' + "5" + '\t' + "6" + '\t' + "7" + '\t' + "8" + '\t' + "9" + '\t' + "10";
                }
                else if (nHeader == 1)
                {
                    return "Dir" + '\t' + "File" + '\t' + "Created" + '\t' + "Modded" + '\t' + "Attrib" + '\t' + "Length" + '\t' + "Error1" + '\t' + "Error2" + '\t' + "FakeChecksum";
                }
            }

            bool bDbgCheck = false;

            if (((strDir ?? "").TrimEnd() != (strDir ?? "")) || ((strFile ?? "").TrimEnd() != (strFile ?? "")))
            {
                strError1 += " Trailing whitespace";
                strError1.Trim();
                Utilities.Assert(1303.4315, (false == string.IsNullOrWhiteSpace(strDir)) || (false == string.IsNullOrWhiteSpace(strFile)));
                bDbgCheck = true;
            }

            string strRet = (strDir + '\t' + strFile + '\t' + strCreated + '\t' + strModified + '\t' + strAttributes + '\t' + strLength + '\t' + strError1 + '\t' + strError2 + '\t' + strChecksum).TrimEnd();

            if (bDbgCheck)
            {
#if (DEBUG)
                string[] strArray = strRet.Split('\t');
                DateTime dtParse = DateTime.MinValue;

                if (strArray[mNcolLength01].Contains("Trailing whitespace") && DateTime.TryParse(strArray[1], out dtParse))
                {
                    Utilities.Assert(1303.4316, false);
                }
#endif
            }

            return strRet;
        }

        // make MessageBox modal from a worker thread
        internal static MBoxRet MBox(string strMessage, string strTitle = null, MBoxBtns? buttons_in = null)
        {
            if (GlobalData.AppExit)
            {
                return MBoxRet.None;
            }

            if (GlobalDataSDL.static_form.InvokeRequired) { return (MBoxRet)GlobalDataSDL.static_form.Invoke(new MBoxDelegate(MBox), new object[] { strMessage, strTitle, buttons_in }); }

            MessageBoxKill();
            m_form1MessageBoxOwner = new SDL_Win();
            m_form1MessageBoxOwner.Owner = GlobalDataSDL.static_form;
            m_form1MessageBoxOwner.Text = strTitle;
            m_form1MessageBoxOwner.Icon = GlobalDataSDL.static_form.Icon;

            MBoxBtns buttons = (buttons_in != null) ? buttons_in.Value : MBoxBtns.OK;
            MBoxRet msgBoxRet = (MBoxRet)MessageBox.Show(m_form1MessageBoxOwner, strMessage.PadRight(100), strTitle, (MessageBoxButtons)buttons);
            if (m_form1MessageBoxOwner != null)
            {
                MessageBoxKill();
                return msgBoxRet;
            }

            // cancelled externally
            return MBoxRet.None;
        }

        internal static void MessageBoxKill(string strMatch = null)
        {
            if ((m_form1MessageBoxOwner != null) && new string[] { null, m_form1MessageBoxOwner.Text }.Contains(strMatch))
            {
                m_form1MessageBoxOwner.Close();
                m_form1MessageBoxOwner = null;
                GlobalDataSDL.static_form.Activate();
            }
        }

        protected static string StrFile_01(string strFile)
        {
            return Path.Combine(Path.GetDirectoryName(strFile),
                Path.GetFileNameWithoutExtension(strFile) + "_01" + Path.GetExtension(strFile));
        }

        internal static void SetProperty<T>(object input, T outObj, Expression<Func<T, object>> outExpr)
        {
            if (input == null)
            {
                return;
            }

            ((PropertyInfo)((MemberExpression)outExpr.Body).Member).SetValue(outObj, input, null);
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
}