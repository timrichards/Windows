using System;
using System.Drawing;
using System.Windows.Forms;

namespace DoubleFile
{
    struct BlinkyStruct : IDisposable
    {
        static bool m_bTreeSelect = false;
        internal static bool TreeSelect { get { return m_bTreeSelect; } }

        readonly Control m_defaultControl;
        readonly SDL_Timer m_timer;

        Holder m_holder;
        Color m_clrBlink;
        int m_nBlink;
        int m_nNumBlinks;
        bool m_bProgress;   

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
            internal override void ResetHolder()
            {
                m_bTreeSelect = false;

                if (m_obj == null)
                    return;

                if (m_obj.TreeView == null)
                    return;

                m_obj.TreeView.SelectedNode = m_obj; 
            }
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

        internal BlinkyStruct(Control defaultControl)
        {
            m_holder = new NullHolder();
            m_clrBlink = Color.DarkTurquoise;
            m_nBlink = 0;
            m_nNumBlinks = 10;
            m_bProgress = false;
            m_defaultControl = defaultControl;

            m_timer = null;     // bootstrap
            BlinkyStruct local = this;
            m_timer = new SDL_Timer(33.0, () =>
            {
                if (local.m_bProgress || (++local.m_nBlink < local.m_nNumBlinks))
                {
                    local.m_holder.BackColor = (local.m_nBlink % 2 == 0) ? local.m_holder.ClrOrig : local.m_clrBlink;
                }
                else
                {
                    local.Reset();
                }
            });
        }

        public void Dispose()
        {
            if (m_timer != null)
            {
                m_timer.Dispose();
            }
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
            MBoxStatic.Assert(1303.4301, m_timer.Enabled == false, bTraceOnly: true);
            MBoxStatic.Assert(1303.4302, m_nBlink == 0, bTraceOnly: true);
            MBoxStatic.Assert(1303.4303, (m_holder is NullHolder) == false, bTraceOnly: true);
            MBoxStatic.Assert(1303.4304, m_bProgress == false, bTraceOnly: true);

            m_holder.ClrOrig = m_holder.BackColor;
            m_bProgress = bProgress;
            m_clrBlink = clr ?? (bProgress ? Color.LightSalmon : Color.Turquoise);
            m_nBlink = 0;
            m_nNumBlinks = Once ? 2 : 10;
            m_timer.Interval = bProgress ? 500 : (Once ? 100 : 50);
            m_timer.Start();
        }

        internal void Reset()
        {
            if (m_timer != null)
                m_timer.Stop();

            m_nBlink = 0;
            m_bProgress = false;
            m_holder.BackColor = m_holder.ClrOrig;
            m_holder.ResetHolder();   // has to be last before deleting object.
            m_holder = new NullHolder();
        }
    }
}
