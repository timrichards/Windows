using System;
using System.Drawing;
using System.Windows.Forms;

namespace DoubleFile
{
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
            readonly ListViewItem m_obj = null;
            internal ListViewItemHolder(ListViewItem obj) { m_obj = obj; }
            internal override Color BackColor { get { return m_obj.BackColor; } set { m_obj.BackColor = value; } }
            internal override void ResetHolder() { m_obj.Selected = true; }
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
            MBox.Assert(1303.4301, m_timer.IsEnabled == false, bTraceOnly: true);
            MBox.Assert(1303.4302, m_nBlink == 0, bTraceOnly: true);
            MBox.Assert(1303.4303, (m_holder is NullHolder) == false, bTraceOnly: true);
            MBox.Assert(1303.4304, m_bProgress == false, bTraceOnly: true);

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
}
