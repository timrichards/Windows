#if WPF
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Markup;
using System.Xml;
using System.Windows.Interop;
using Media = System.Windows.Media;
#else
using System.Windows.Forms;
using System.Threading;         // release mode
#endif

using WPF = System.Windows;
using Forms = System.Windows.Forms;
using Drawing = System.Drawing;

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

namespace SearchDirLists
{
#if (WPF)
    [System.ComponentModel.DesignerCategory("Code")]
    class SDL_Win : WPF.Window { }
    enum MBoxBtns { OK = WPF.MessageBoxButton.OK, YesNo = WPF.MessageBoxButton.YesNo, YesNoCancel = WPF.MessageBoxButton.YesNoCancel }
    enum MBoxRet { None = WPF.MessageBoxResult.None, Yes = WPF.MessageBoxResult.Yes, No = WPF.MessageBoxResult.No }

    [System.ComponentModel.DesignerCategory("Code")]
    class SDL_Control : Control
    {
        internal IntPtr Handle = (IntPtr)0;
        internal bool IsDisposed { get { return false; } }
    }

    class SDL_TreeView
    {
        internal SDL_TreeView()
        {
            Nodes = new SDL_TreeNodeCollection(this);
        }

        readonly internal SDL_TreeNodeCollection Nodes = null;

        internal int GetNodeCount(bool includeSubTrees = false)
        {
            if (includeSubTrees)
            {
                return CountSubnodes(Nodes);
            }
            else
            {
                return Nodes.Count;
            }
        }

        internal void CollapseAll() { }
        internal void Select() { }

        internal SDL_TreeNode SelectedNode = null;
        internal SDL_TreeNode TopNode = null;
        internal Drawing.Font Font = null;
        internal bool CheckBoxes = false;
        internal bool Enabled = false;

        int CountSubnodes(SDL_TreeNodeCollection nodes)
        {
            int nRet = 0;

            foreach (SDL_TreeNode treeNode in nodes)
            {
                nRet += CountSubnodes(treeNode.Nodes);
                ++nRet;
            }

            return nRet;
        }
    }

    class SDL_TreeNodeCollection : UList<SDL_TreeNode>
    {
        readonly SDL_TreeView m_treeView = null;
        String strPrevQuery = null;
        SDL_TreeNode nodePrevQuery = null;

        internal SDL_TreeNodeCollection(SDL_TreeView treeView)
        {
            m_treeView = treeView;
        }

        internal void AddRange(SDL_TreeNode[] arrNodes)
        {
            foreach (SDL_TreeNode treeNode in arrNodes)
            {
                Add(treeNode);
            }

            if ((Count > 0) && (m_treeView != null))
            {
                m_treeView.TopNode = this[0];
                SetLevel(m_treeView, this);
            }
        }

        static void SetLevel(SDL_TreeView treeView, SDL_TreeNodeCollection nodes, SDL_TreeNode nodeParent = null, int nLevel = 0)
        {
            SDL_TreeNode nodePrev = null;

            if ((nodeParent != null) && (nodes.Count > 0))
            {
                nodeParent.FirstNode = nodes[0];
            }

            foreach (SDL_TreeNode treeNode in nodes)
            {
                if (nodePrev != null)
                {
                    nodePrev.NextNode = treeNode;
                }

                // same assert that Forms generates: must remove it from the other tree first.
                Utilities.Assert(0, (treeNode.TreeView == null) || (treeNode.TreeView == treeView));

                nodePrev = treeNode;
                treeNode.DetachFromTree();
                treeNode.TreeView = treeView;
                treeNode.Parent = nodeParent;
                treeNode.Level = nLevel;
                SetLevel(treeView, treeNode.Nodes, treeNode, nLevel + 1);
            }
        }

        internal bool ContainsKey(String s)
        {
            if (s != strPrevQuery)
            {
                strPrevQuery = s;
                nodePrevQuery = this[s];
            }

            return (nodePrevQuery != null);
        }

        internal SDL_TreeNode this[String s]
        {
            get
            {
                if (s == strPrevQuery)
                {
                    return nodePrevQuery;
                }
                else
                {
                    strPrevQuery = s;
                    nodePrevQuery = (SDL_TreeNode)Keys.Where(t => t.Text == s);
                    return nodePrevQuery;                   // TODO: Trim? ignore case? Probably neither.
                }
            }
        }

        internal new void Clear()
        {
            foreach (SDL_TreeNode treeNode in this)
            {
                treeNode.DetachFromTree();
            }

            base.Clear();
        }
    }

    class SDL_TreeNode
    {
        internal void DetachFromTree()
        {
            TreeView = null;
            Level = -1;
            m_strFullPath = null;

            foreach (SDL_TreeNode treeNode in Nodes)
            {
                treeNode.DetachFromTree();
            }
        }

        internal SDL_TreeNode()
        {
            Nodes = new SDL_TreeNodeCollection(TreeView);
        }

        readonly internal SDL_TreeNodeCollection Nodes = null;

        internal SDL_TreeNode(String strContent)
            : this()
        {
            Text = strContent;
        }

        internal SDL_TreeNode(String strContent, SDL_TreeNode[] arrNodes)
            : this(strContent)
        {
            Nodes.AddRange(arrNodes);
        }

        internal void EnsureVisible() { }

        internal String FullPath
        {
            get
            {
                if (m_strFullPath != null)
                {
                    return m_strFullPath;
                }

                Stack<SDL_TreeNode> stack = new Stack<SDL_TreeNode>(8);
                SDL_TreeNode nodeParent = Parent;

                while (nodeParent != null)
                {
                    stack.Push(nodeParent);
                    nodeParent = nodeParent.Parent;
                }

                StringBuilder sb = new StringBuilder();

                nodeParent = stack.Pop();

                while (nodeParent != null)
                {
                    sb.Append(nodeParent.Text + Path.DirectorySeparatorChar);
                    nodeParent = stack.Pop();
                }

                sb.Append(Text);
                m_strFullPath = sb.ToString();
                return m_strFullPath;
            }
        }

        String m_strFullPath = null;
        internal String Text = null;
        internal String ToolTipText = null;
        internal String Name = null;
        internal SDL_TreeView TreeView = null;
        internal SDL_TreeNode FirstNode = null;
        internal SDL_TreeNode NextNode = null;
        internal SDL_TreeNode Parent = null;
        internal int Level = -1;
        internal Drawing.Color BackColor;
        internal Drawing.Color ForeColor;
        internal Drawing.Font NodeFont = null;
        internal bool Checked = false;
        internal int SelectedImageIndex = -1;
        internal object Tag = null;
    }

    class SDL_ListViewItemCollection : UList<SDL_ListViewItem>
    {
        readonly SDL_ListView m_listView = null;
        String strPrevQuery = null;
        SDL_ListViewItem lvItemPrevQuery = null;

        internal SDL_ListViewItemCollection(SDL_ListView listView)
        {
            m_listView = listView;
        }

        internal void AddRange(String[] arrItems)
        {
            foreach (String s in arrItems)
            {
                Add(new SDL_ListViewItem(s, m_listView));
            }
        }

        internal void AddRange(SDL_ListViewItem[] arrItems)
        {
            foreach (SDL_ListViewItem lvItem in arrItems)
            {
                lvItem.ListView = m_listView;
                Add(lvItem);
            }
        }

        internal bool ContainsKey(String s)
        {
            if (s != strPrevQuery)
            {
                strPrevQuery = s;
                lvItemPrevQuery = this[s];
            }

            return (lvItemPrevQuery != null);
        }

        internal SDL_ListViewItem this[String s]
        {
            get
            {
                if (s == strPrevQuery)
                {
                    return lvItemPrevQuery;
                }
                else
                {
                    strPrevQuery = s;
                    lvItemPrevQuery = (SDL_ListViewItem)Keys.Where(t => t.Text == s);
                    return lvItemPrevQuery;                   // TODO: Trim? ignore case? Probably neither.
                }
            }
        }
    }

    class SDL_ListView
    {
        internal SDL_ListView() { Items = new SDL_ListViewItemCollection(this); }

        internal SDL_ListViewItem TopItem = null;
        internal SDL_ListViewItemCollection Items = null;
        internal void Invalidate() { }
    }

    class SDL_ListViewItem
    {
        internal SDL_ListViewItem(SDL_ListView listView = null) { SubItems = new SDL_ListViewItemCollection(ListView); }
        internal SDL_ListViewItem(String strContent, SDL_ListView listView = null) : this(listView) { Text = strContent; }

        internal SDL_ListViewItem(String[] arrString, SDL_ListView listView = null) : this(listView)
        {
            Text = arrString[0];
    
            for (int i = 1; i < arrString.Length; ++i)
            {
                SubItems.Add(new SDL_ListViewItem(arrString[i], listView));
            }
        }
    
        internal String Text = null;
        internal String Name = null;
        internal object Tag = null;
        internal Drawing.Color ForeColor;
        internal Drawing.Color BackColor;
        internal void Select(bool bSel = true) {}
        internal bool Focused;
        internal Drawing.Font Font = new Drawing.Font("Microsoft Sans Serif", 8.25F, Drawing.FontStyle.Regular, Drawing.GraphicsUnit.Point, ((byte)(0)));
        internal int Index = -1;
        internal UList<SDL_ListViewItem> SubItems = null;
        internal object Clone() { return MemberwiseClone(); }
        internal void EnsureVisible() { }
        internal SDL_ListView ListView = null;
    }
    
/**/    static class SDLWPF
/**/    {
/**/
/**/        // SDL_Control
/**/        static Drawing.Color _BrushToClr(Media.Brush brush) { Media.Color c = ((SolidColorBrush)brush).Color; return Drawing.Color.FromArgb(c.A, c.R, c.G, c.B); }
/**/        static SolidColorBrush _ClrToBrush(Drawing.Color c) { return new SolidColorBrush(Media.Color.FromArgb(c.A, c.R, c.G, c.B)); }
/**/        internal static Drawing.Color GetBackColor(this Control ctl) { return _BrushToClr(ctl.Background); }
/**/        internal static void SetBackColor(this Control ctl, Drawing.Color c) { ctl.Background = _ClrToBrush(c); }
/**/        internal static Drawing.Color GetForeColor(this Control ctl) { return _BrushToClr(ctl.Foreground); }
/**/        internal static void SetForeColor(this Control ctl, Drawing.Color c) { ctl.Foreground = _ClrToBrush(c); }
/**/        internal static object Clone(this Control ctl) { return XamlReader.Load(XmlReader.Create(new StringReader(XamlWriter.Save(ctl)))); }
/**/        internal static void EnsureVisible(this Control ctl) { ctl.BringIntoView(); }
            internal static void Invalidate(this Control ctl) { }
            internal static bool IsDisposed(this Control ctl) { return false; }
/**/
/**/        // LV
/**/        internal static bool ContainsKey(this ItemCollection c, String str) { return false; }
            internal static object GetAt(this ItemCollection c, String s) { return c[c.IndexOf(s)]; }
//https://stackoverflow.com/questions/1077397/scroll-listviewitem-to-be-at-the-top-of-a-listview

            internal static bool InvokeRequired(this WPF.Window w) { return (w.Dispatcher.CheckAccess() == false); }
            internal static object Invoke(this WPF.Window w, Delegate m, params object[] a) { return w.Dispatcher.Invoke(m, a); }
            internal static void TitleSet(this WPF.Window w, String s) { w.Title = s; }
            internal static String TitleGet(this WPF.Window w) { return w.Title; }

            internal static void Select(this WPF.Controls.Control c) { c.Focus(); }
/**/
/**/        internal static SDL_TreeView treeViewMain = new SDL_TreeView();
/**/        internal static SDL_TreeView treeViewCompare1 = null; //Form1.static_form.form_treeCompare1;
/**/        internal static SDL_TreeView treeViewCompare2 = null; //Form1.static_form.form_treeCompare2;
/**/    }
#else
    [System.ComponentModel.DesignerCategory("Code")]
    class SDL_Win : Form {}
    enum MBoxBtns { OK = MessageBoxButtons.OK, YesNo = MessageBoxButtons.YesNo, YesNoCancel = MessageBoxButtons.YesNoCancel }
    enum MBoxRet { None = DialogResult.None, Yes = DialogResult.Yes, No = DialogResult.No }

    class SDL_Control : Control
    {
        public SDL_Control() : base() { }
    }

    class SDL_ListView : ListViewEmbeddedControls.ListViewEx
    {
        internal SDL_ListView() : base() { }
    }

    class SDL_ListViewItem : ListViewItem
    {
        public SDL_ListViewItem() : base() { }
        internal SDL_ListViewItem(String strContent) : base(strContent) { }
        internal SDL_ListViewItem(String[] arrString) : base(arrString) { }
        internal void Select(bool bSel = true) { Selected = bSel; }
    }

    class SDL_ListViewSubitem : ListViewItem.ListViewSubItem
    {
    }

    class SDL_LVItemCollection: ListView.ListViewItemCollection
    {
        internal SDL_LVItemCollection(ListView lv) : base(lv) { }
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

    class SDL_TreeNode : TreeNode
    {
        public SDL_TreeNode() : base() { }
        internal SDL_TreeNode(String strContent) : base(strContent) { }
        internal SDL_TreeNode(String strContent, SDL_TreeNode[] arrNodes) : base(strContent, arrNodes) { }
    }

    static class SDLWPF_Ext
    {
        internal static object GetTag(this TreeNode node) { return node.Tag; }
        internal static void SetTag(this TreeNode node, object o) { node.Tag = o; }
  //      internal static Color Clr(Drawing.Color i) { return i; }
  //      internal static Color ClrA(Drawing.Color i) { return i; }

        // extension method as property
        internal static Drawing.Color GetBackColor(this Control ctl) { return ctl.BackColor; }
        internal static void SetBackColor(this Control ctl, Drawing.Color c) { ctl.BackColor = c; }
        internal static String Text(this ListViewItem l) { return l.Text; }
        internal static void SetText(this ListViewItem l, String s) { l.Text = s; }
        internal static String Text(this ListViewItem.ListViewSubItem l) { return l.Text; }
        internal static void SetText(this ListViewItem.ListViewSubItem l, String s) { l.Text = s; }
        internal static int GetCount(this ListViewItem.ListViewSubItemCollection a) { return a.Count; }
        internal static void TopItemSet(this ListView lv, int n) { lv.TopItem = lv.Items[n]; }
        internal static void TopItemSet(this ListView lv, ListViewItem l) { lv.TopItem = l; }
        internal static SDL_ListViewItem TopItem(this ListView lv) { return (SDL_ListViewItem)lv.TopItem; }
        internal static bool InvokeRequired(this Control c) { return c.InvokeRequired; }
        internal static void TitleSet(this Control c, String s) { c.Text = s; }
        internal static String TitleGet(this Control c) { return c.Text; }
    }

    class SDLWPF
    {
        internal static SDL_TreeView treeViewMain = (SDL_TreeView)GlobalData.static_form.form_treeViewBrowse;
        internal static SDL_TreeView treeViewCompare1 = (SDL_TreeView)GlobalData.static_form.form_treeCompare1;
        internal static SDL_TreeView treeViewCompare2 = (SDL_TreeView)GlobalData.static_form.form_treeCompare2;
    }
#endif

class Blinky
    {
        static bool m_bTreeSelect = false;
        internal static bool TreeSelect { get { return m_bTreeSelect; } }

        readonly Control m_defaultControl = null;
        readonly DispatcherTimer m_timer = new DispatcherTimer();

        Holder m_holder = new NullHolder();
        Drawing.Color m_clrBlink = Drawing.Color.DarkTurquoise;
        int m_nBlink = 0;
        int m_nNumBlinks = 10;
        bool m_bProgress = false;

        abstract class Holder
        {
            internal Drawing.Color ClrOrig = Drawing.Color.Empty;
            internal virtual Drawing.Color BackColor { get; set; }
            internal virtual void ResetHolder() { }
        }
        class NullHolder : Holder { }
        class TreeNodeHolder : Holder
        {
            readonly SDL_TreeNode m_obj = null;
            internal TreeNodeHolder(SDL_TreeNode obj) { m_obj = obj; m_bTreeSelect = true; }
            internal override Drawing.Color BackColor { get { return m_obj.BackColor; } set { m_obj.BackColor = value; } }
            internal override void ResetHolder() { m_bTreeSelect = false; m_obj.TreeView.SelectedNode = m_obj; }
        }
        class ListViewItemHolder : Holder
        {
            readonly SDL_ListViewItem m_obj = null;
            internal ListViewItemHolder(SDL_ListViewItem obj) { m_obj = obj; }
            internal override Drawing.Color BackColor { get { return m_obj.BackColor; } set { m_obj.BackColor = value; } }
            internal override void ResetHolder() { m_obj.Select(); }
        }
        class ControlHolder : Holder
        {
            protected readonly Control m_obj = null;
            internal ControlHolder(Control obj) { m_obj = obj; }
            internal override Drawing.Color BackColor { get { return m_obj.GetBackColor(); } set { m_obj.SetBackColor(value); } }
        }

#if (WPF)
        internal Blinky(Forms.Control defaultControl){}
#else
        internal Blinky(WPF.Controls.Control defaultControl){}
#endif
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

        internal void SelectTreeNode(SDL_TreeNode treeNode, bool Once = true)
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

        internal void Go(Forms.Control ctl, Drawing.Color? clr = null, bool Once = false)
        {
#if (WPF == false)
            Reset();
            m_holder = new ControlHolder(ctl);
            Go_A(clr, Once);
#endif
        }

        internal void Go(WPF.Controls.Control ctl, Drawing.Color? clr = null, bool Once = false)
        {
#if (WPF) 
            if (ctl.Background is SolidColorBrush)      // TODO: Fix the gradient brush issue on buttons
            {
                Reset();
                m_holder = new ControlHolder(ctl);
                Go_A(clr, Once);
            }
#endif
        }

        internal void Go(Drawing.Color? clr = null, bool Once = false, bool bProgress = false)
        {
            Reset();
            m_holder = new ControlHolder(m_defaultControl);
            Go_A(clr, Once, bProgress);
        }

        void Go_A(Drawing.Color? clr = null, bool Once = false, bool bProgress = false)
        {
            Utilities.Assert(1303.4301, m_timer.IsEnabled == false, bTraceOnly: true);
            Utilities.Assert(1303.4302, m_nBlink == 0, bTraceOnly: true);
            Utilities.Assert(1303.4303, (m_holder is NullHolder) == false, bTraceOnly: true);
            Utilities.Assert(1303.4304, m_bProgress == false, bTraceOnly: true);

            m_holder.ClrOrig = m_holder.BackColor;
            m_bProgress = bProgress;
            m_clrBlink = clr ?? (bProgress ? Drawing.Color.LightSalmon : Drawing.Color.Turquoise);
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
        internal static bool IsChildOf(this SDL_TreeNode child, SDL_TreeNode treeNode)
        {
            if (child.Level <= treeNode.Level)
            {
                return false;
            }

            SDL_TreeNode parentNode = (SDL_TreeNode)child.Parent;

            while (parentNode != null)
            {
                if (parentNode == treeNode)
                {
                    return true;
                }

                parentNode = (SDL_TreeNode)parentNode.Parent;
            }

            return false;
        }

        internal static SDL_TreeNode Root(this SDL_TreeNode treeNode)
        {
            SDL_TreeNode nodeParent = treeNode;

            while (nodeParent.Parent != null)
            {
                nodeParent = (SDL_TreeNode)nodeParent.Parent;
            }

            return nodeParent;
        }

        internal static Drawing.Rectangle Scale(this Drawing.Rectangle rc_in, Drawing.SizeF scale)
        {
            Drawing.RectangleF rc = rc_in;

            rc.X *= scale.Width;
            rc.Y *= scale.Height;
            rc.Width *= scale.Width;
            rc.Height *= scale.Height;
            return Drawing.Rectangle.Ceiling(rc);
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

        internal static void Go(Forms.Control ctl_in = null, bool Once = false)
        {
#if (WPF)
            Dispatcher dispatcher = GlobalData.static_wpfWin.Dispatcher;
#else
            Forms.Control dispatcher = ctl_in ?? GlobalData.static_form;
#endif
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
#if (WPF)
                    fInfo.hwnd = new WindowInteropHelper(GlobalData.static_wpfWin).Handle;
#else
                    fInfo.hwnd = GlobalData.static_form.Handle;
#endif
                }

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
        readonly String m_strVolumeName = null;
        readonly String m_strPath = null;
        readonly String m_strSaveAs = null;
        String m_strStatus = null;
        readonly String m_strInclude = null;
        readonly String m_strVolumeGroup = null;
        internal int Index { get { return m_nIndex; } }
        internal String VolumeName { get { return m_strVolumeName; } }
        internal String StrPath { get { return m_strPath; } }
        internal String SaveAs { get { return m_strSaveAs; } }
        internal String Status { get { return m_strStatus; } }
        internal String Include { get { return m_strInclude; } }
        internal String VolumeGroup { get { return m_strVolumeGroup; } }

        internal LVvolStrings(VolumeLVitemVM lvItem)
        {
            m_nIndex = lvItem.Index;
            m_strVolumeName = lvItem.VolumeName;
            m_strPath = lvItem.Path;
            m_strSaveAs = lvItem.SaveAs;
            m_strStatus = lvItem.Status;
            m_strInclude = lvItem.IncludeStr;
            m_strVolumeGroup = lvItem.VolumeGroup;
        }

        internal LVvolStrings(SDL_ListViewItem lvItem)
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

        internal void SetStatus_BadFile(Forms.ListView lv)
        {
            lv.Items[Index].SubItems[3].Text =
                m_strStatus = "Bad file. Will overwrite.";
        }

        internal void SetStatus_Done(Forms.ListView lv, SDL_TreeNode rootNode)
        {
            lv.Items[Index].Tag = rootNode;
        }
    }

    // SearchDirLists listing file|*.sdl_list|SearchDirLists volume list file|*.sdl_vol|SearchDirLists copy scratchpad file|*.sdl_copy|SearchDirLists ignore list file|*.sdl_ignore
    abstract class SDL_File : Utilities
    {
        public static String BaseFilter = "Text files|*.txt|All files|*.*";
        public static String FileAndDirListFileFilter = "SearchDirLists listing file|*." + mSTRfileExt_Listing;

        public readonly String Header = null;

        String m_strDescription = null;
        public String Description { get { return m_strDescription + " list file"; } }

        String m_strExt = null;
        public String Filter { get { return "SearchDirLists " + Description + "|*." + m_strExt; } }

        internal static Forms.SaveFileDialog SFD = null;        // TODO: remove frankenSFD

        protected String m_strPrevFile = null;
        protected String m_strFileNotDialog = null;

        static bool bInited = false;

        internal static void Init()
        {
            if (bInited)
            {
                return;
            }

            SFD = new Forms.SaveFileDialog();
            SFD.OverwritePrompt = false;
            bInited = true;
        }

        protected SDL_File(String strHeader, String strExt, String strDescription)
        {
            Init();
            Utilities.Assert(1303.4306, SFD != null);
            Header = strHeader;
            m_strExt = strExt;
            m_strDescription = strDescription;
        }

        bool ShowDialog(Forms.FileDialog fd)
        {
            fd.Filter = Filter + "|" + BaseFilter;
            fd.FilterIndex = 0;
            fd.FileName = Path.GetFileNameWithoutExtension(m_strPrevFile);
            fd.InitialDirectory = Path.GetDirectoryName(m_strPrevFile);

            if (fd.ShowDialog() != Forms.DialogResult.OK)
            {
                return false;
            }

            m_strFileNotDialog = m_strPrevFile = fd.FileName;
            return true;
        }

        protected virtual void ReadListItem(ListView lv, String[] strArray) { lv.Items.Add(new SDL_ListViewItem(strArray)); }
        protected virtual void ReadListItem(ListViewVM lv, String[] strArray) { lv.NewItem(strArray); }

        internal bool ReadList(ListViewVM lv)
#if (WPF)
        {
            int nCols = lv.NumCols;
#else
        { return false; }
        internal bool ReadList(Forms.ListView lv_in)
        {
            int nCols = lv_in.Columns.Count;
            ListView lv = new ListView();       // fake
#endif
            if ((m_strFileNotDialog == null) && (ShowDialog(new Forms.OpenFileDialog()) == false))
            {
                return false;
            }

            if (Keyboard.IsKeyDown(Key.LeftShift) == false)
            {
                lv.Items.Clear();
            }

            using (StreamReader sr = File.OpenText(m_strFileNotDialog))
            {
                String strLine = sr.ReadLine();

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
#if (WPF == false)
                ListViewItem[] lvItems = lv.Items.Cast<ListViewItem>().ToArray();
                lv.Items.Clear();
                lv_in.Items.AddRange(lvItems);
                lv_in.Invalidate();
#endif
                return true;
            }
            else
            {
                MBox("Not a valid " + Description + ".", "Load " + Description);
                return false;
            }
        }

        protected virtual String WriteListItem(int nIndex, String str) { return str; }

        internal bool WriteList(IEnumerable<ListViewItemVM> lvItems)
#if (WPF == false)
        { return false; }
        internal bool WriteList(Forms.ListView.ListViewItemCollection lvItems)
#endif
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
#if (WPF)
                foreach (ListViewItemVM lvItem in lvItems)
                {
                    sw.Write(WriteListItem(0, lvItem[0]));

                    for (int nIx = 1; nIx < lvItem.NumCols; ++nIx)
                    {
                        sw.Write('\t' + WriteListItem(nIx, lvItem[nIx]));
#else
                foreach (SDL_ListViewItem lvItem in lvItems)
                {
                    sw.Write(WriteListItem(0, lvItem.SubItems[0].Text));

                    int nIx = 1;

                    foreach (Forms.ListViewItem.ListViewSubItem lvSubitem in lvItem.SubItems.Cast<Forms.ListViewItem.ListViewSubItem>().Skip(1))
                    {
                        sw.Write('\t' + WriteListItem(nIx, lvSubitem.Text));
                        ++nIx;
#endif
                    }

                    sw.WriteLine();
                }
            }

            return true;
        }
    }

    class SDL_VolumeFile : SDL_File
    {
        internal SDL_VolumeFile(String strFile = null) : base(mSTRvolListHeader, mSTRfileExt_Volume, "volume") { m_strFileNotDialog = strFile; }
#if (WPF)
        protected override void ReadListItem(ListViewVM lv, String[] strArray)
#else
        protected override void ReadListItem(ListView lv, String[] strArray)
#endif
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

            strArray[1] = strArray[1].TrimEnd(Path.DirectorySeparatorChar);
#if (WPF)
            lv.NewItem(strArray);
#else
            lv.Items.Add(new SDL_ListViewItem(strArray));
#endif
        }

        protected override String WriteListItem(int nIndex, String str)
        {
            return (nIndex == 1) ? str.TrimEnd(Path.DirectorySeparatorChar) : str;
        }
    }

    class SDL_CopyFile : SDL_File { internal SDL_CopyFile() : base(mSTRcopyScratchpadHeader, mSTRfileExt_Copy, "copy") { } }

    class SDL_IgnoreFile : SDL_File { internal SDL_IgnoreFile(String strFile = null) : base(mSTRignoreListHeader, mSTRfileExt_Ignore, "ignore") { m_strFileNotDialog = strFile; } }

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
        internal const String mSTRheader01 = "SearchDirLists 0.1";
        internal const String mSTRstart01 = mSTRheader01 + " START";
        internal const String mSTRend01 = mSTRheader01 + " END";
        internal const String mSTRerrorsLoc01 = mSTRheader01 + " ERRORS";
        internal const String mSTRtotalLengthLoc01 = mSTRheader01 + " LENGTH";
        internal const String mSTRdrive01 = mSTRheader01 + " DRIVE";
        internal const String mSTRvolListHeader01 = mSTRheader01 + " VOLUME LIST";

        internal const String mSTRheader = "SearchDirLists 0.2";
        internal const String mSTRstart = mSTRheader + " START";
        internal const String mSTRend = mSTRheader + " END";
        internal const String mSTRerrorsLoc = mSTRheader + " ERRORS";
        internal const String mSTRtotalLengthLoc = mSTRheader + " LENGTH";
        internal const String mSTRdrive = mSTRheader + " DRIVE";
        internal const String mSTRvolListHeader = mSTRheader + " VOLUME LIST";
        internal const String mSTRcopyScratchpadHeader = mSTRheader + " COPYDIRS LIST";
        internal const String mSTRignoreListHeader = mSTRheader + " IGNORE LIST";
        internal const String mSTRusingFile = "Using file.";
        internal const String mSTRsaved = "Saved.";
        internal const String mSTRnotSaved = "Not saved.";
        internal const String mSTRcantSave = "Can't save. Not mounted.";

        internal const int mNcolLength = 7;
        internal const int mNcolLength01 = 5;
        internal const int mNcolLengthLV = 4;

        internal const String mSTRlineType_Version = "V";
        internal const String mSTRlineType_Nickname = "N";
        internal const String mSTRlineType_Path = "P";
        internal const String mSTRlineType_DriveInfo = "I";
        internal const String mSTRlineType_Comment = "C";
        internal const String mSTRlineType_Start = "S";
        internal const String mSTRlineType_Directory = "D";
        internal const String mSTRlineType_File = "F";
        internal const String mSTRlineType_End = "E";
        internal const String mSTRlineType_Blank = "B";
        internal const String mSTRlineType_ErrorDir = "R";
        internal const String mSTRlineType_ErrorFile = "r";
        internal const String mSTRlineType_Length = "L";

        internal const String mSTRfileExt_Listing = "sdl_list";
        internal const String mSTRfileExt_Volume = "sdl_vol";
        internal const String mSTRfileExt_Copy = "sdl_copy";
        internal const String mSTRfileExt_Ignore = "sdl_ignore";

        static SDL_Win m_form1MessageBoxOwner = null;
        static double static_nLastAssertLoc = -1;
        static DateTime static_dtLastAssert = DateTime.MinValue;

#if (DEBUG == false)
        static bool static_bAssertUp = false;
#endif

        internal static bool Assert(double nLocation, bool bCondition, String strError_in = null, bool bTraceOnly = false)
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

        internal static object CheckAndInvoke(Dispatcher dispatcher, Delegate action, object[] args = null)
        {
#if (WPF)
            bool bInvoke = dispatcher.CheckAccess();
#else
            return null;
        }
        internal static object CheckAndInvoke(Control dispatcher, Delegate action, object[] args = null)
        {
            bool bInvoke = dispatcher.InvokeRequired;
#endif
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
                            Utilities.Assert(1303.4309, nLineNo == 16);
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
                            String[] arrLine = strLine.Split('\t');

                            file_out.WriteLine(FormatLine(mSTRlineType_Length, nLineNo, FormatString(strDir: mSTRtotalLengthLoc, nLength: long.Parse(arrLine[mNcolLength01]))));
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

                            file_out.WriteLine(FormatLine(bAtErrors ? mSTRlineType_ErrorFile : mSTRlineType_File, nLineNo, strTab + strLine));
                            continue;
                        }
                        else if (strDir.Contains(":" + Path.DirectorySeparatorChar) == false)
                        {
                            Utilities.Assert(1303.4311, false);        // all that's left is directories
                            continue;
                        }

                        // directory
                        String P = Path.DirectorySeparatorChar.ToString();
                        String PP = P + P;
                        String str = strLine.Replace(PP, P);

                        file_out.WriteLine(FormatLine(bAtErrors ? mSTRlineType_ErrorDir : mSTRlineType_Directory, nLineNo, str));
                    }
                }
            }
        }

        internal static int CountNodes(List<SDL_TreeNode> listNodes)
        {
            int nCount = 0;

            foreach (SDL_TreeNode treeNode in listNodes)
            {
                nCount += CountNodes(treeNode, bNextNode: false);
            }

            return nCount;
        }

        internal static int CountNodes(SDL_TreeNode treeNode_in, bool bNextNode = true)
        {
            SDL_TreeNode treeNode = treeNode_in;
            int nCount = 0;

            do
            {
                if ((treeNode.Nodes != null) && (treeNode.Nodes.Count > 0))
                {
                    nCount += CountNodes((SDL_TreeNode)treeNode.Nodes[0]);
                }

                ++nCount;
            }
            while (bNextNode && ((treeNode = (SDL_TreeNode)treeNode.NextNode) != null));

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
                    Utilities.Assert(1303.4312, strDirName == null);
                }
                else
                {
                    strPath = strPath.TrimEnd(Path.DirectorySeparatorChar);
                    Utilities.Assert(1303.4313, StrValid(strDirName));
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
                    MBox("Error in Source path.", "Save Directory Listing");
                    return false;
                }
            }

            if (StrValid(strSaveAs))
            {
                strSaveAs = Path.GetFullPath(strSaveAs.Trim());

                if (FormatPath(ref strSaveAs, bFailOnDirectory) == false)
                {
                    MBox("Error in Save filename.", "Save Directory Listing");
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
                Utilities.Assert(1303.4314, nHeader is int);

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
                Utilities.Assert(1303.4315, StrValid(strDir) || StrValid(strFile));
                bDbgCheck = true;
            }

            String strRet = (strDir + '\t' + strFile + '\t' + strCreated + '\t' + strModified + '\t' + strAttributes + '\t' + strLength + '\t' + strError1 + '\t' + strError2).TrimEnd();

            if (bDbgCheck)
            {
#if (DEBUG)
                String[] strArray = strRet.Split('\t');
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
        internal static MBoxRet MBox(String strMessage, String strTitle = null, MBoxBtns? buttons_in = null)
        {
            if (GlobalData.AppExit)
            {
                return MBoxRet.None;
            }

            if (GlobalData.static_wpfOrForm.InvokeRequired()) { return (MBoxRet)GlobalData.static_wpfOrForm.Invoke(new MBoxDelegate(MBox), new object[] { strMessage, strTitle, buttons_in }); }

            MessageBoxKill();
            m_form1MessageBoxOwner = new SDL_Win();
            m_form1MessageBoxOwner.Owner = GlobalData.static_wpfOrForm;
            m_form1MessageBoxOwner.TitleSet(strTitle);
            m_form1MessageBoxOwner.Icon = GlobalData.static_wpfOrForm.Icon;

            MBoxBtns buttons = (buttons_in != null) ? buttons_in.Value : MBoxBtns.OK;
#if (WPF)
            MBoxRet msgBoxRet = (MBoxRet)WPF.MessageBox.Show(m_form1MessageBoxOwner, strMessage.PadRight(100), strTitle, (WPF.MessageBoxButton)buttons);
#else
            MBoxRet msgBoxRet = (MBoxRet)MessageBox.Show(m_form1MessageBoxOwner, strMessage.PadRight(100), strTitle, (MessageBoxButtons)buttons);
#endif
            if (m_form1MessageBoxOwner != null)
            {
                MessageBoxKill();
                return msgBoxRet;
            }

            // cancelled externally
            return MBoxRet.None;
        }

        internal static void MessageBoxKill(String strMatch = null)
        {
            if ((m_form1MessageBoxOwner != null) && new String[] { null, m_form1MessageBoxOwner.TitleGet() }.Contains(strMatch))
            {
                m_form1MessageBoxOwner.Close();
                m_form1MessageBoxOwner = null;
                GlobalData.static_wpfOrForm.Activate();
            }
        }

        internal static String NotNull(String str)
        {
            return str ?? String.Empty;
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

        internal static void SetProperty<T>(object input, T outObj, Expression<Func<T, object>> outExpr)
        {
            if (input == null)
            {
                return;
            }

            ((PropertyInfo)((MemberExpression)outExpr.Body).Member).SetValue(outObj, input, null);
        }

        internal static bool ValidateFile(String strSaveAs)
        {
            if (File.Exists(strSaveAs) == false) return false;

            String[] arrLine = File.ReadLines(strSaveAs).Take(1).ToArray();

            if (arrLine.Length <= 0) return false;

            bool bConvertFile = false;

            if (arrLine[0] == mSTRheader01)
            {
                Utilities.WriteLine("Converting " + strSaveAs);
                ConvertFile(strSaveAs);
                Utilities.WriteLine("File converted to " + mSTRheader);
                bConvertFile = true;
            }

            String[] arrToken = File.ReadLines(strSaveAs).Take(1).ToArray()[0].Split('\t');

            if (arrToken.Length < 3) return false;
            if (arrToken[2] != mSTRheader) return false;

            String[] arrLine_A = File.ReadLines(strSaveAs).Where(s => s.StartsWith(mSTRlineType_Length)).ToArray();

            if (arrLine_A.Length <= 0) return false;

            String[] arrToken_A = arrLine_A[0].Split('\t');

            if (arrToken_A.Length < 3) return false;
            if (arrToken_A[2] != mSTRtotalLengthLoc) return false;

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

    class WPF_Form
    {
        internal static Forms.DialogResult WPFMessageBox(String strMessage, String strTitle = null, Forms.MessageBoxButtons? buttons = null)
        {
            Forms.DialogResult dlgRet = Forms.DialogResult.None;

            if (buttons == null)
            {
                dlgRet = Forms.MessageBox.Show(strMessage.PadRight(100), strTitle);
            }
            else
            {
                dlgRet = Forms.MessageBox.Show(strMessage.PadRight(100), strTitle, buttons.Value);
            }

            return dlgRet;
        }
    }
}
