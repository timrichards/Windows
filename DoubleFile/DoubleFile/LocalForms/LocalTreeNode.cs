using System.Collections.Generic;
using System.Text;
using System.Windows.Media;

namespace DoubleFile
{
    class LocalTreeNode
    {
        internal TreeViewItem_FileHashVM TVIVM = null;
        internal LVitem_FileHashVM LVIVM = null;

        internal LocalTreeNode()
        {
            Nodes = new LocalTreeNodeCollection(TreeView);
        }

        internal LocalTreeNode(string strContent)
            : this()
        {
            Text = strContent;
        }

        internal LocalTreeNode(string strContent, LocalTreeNode[] arrNodes)
            : this(strContent)
        {
            Nodes.AddRange(arrNodes);
        }

        internal string FullPath
        {
            get
            {
                if (m_strFullPath != null)
                {
                    return m_strFullPath;
                }

                Stack<LocalTreeNode> stack = new Stack<LocalTreeNode>(8);
                LocalTreeNode nodeParent = Parent;

                while (nodeParent != null)
                {
                    stack.Push(nodeParent);
                    nodeParent = nodeParent.Parent;
                }

                StringBuilder sb = new StringBuilder();

                while (stack.Count > 0)
                {
                    nodeParent = stack.Pop();
                    sb.Append(nodeParent.Text + '\\');
                }

                sb.Append(Text);
                m_strFullPath = sb.ToString();
                return m_strFullPath;
            }
        }

        internal void DetachFromTree()
        {
            TVIVM = null;
            TreeView = null;
            Level = -1;
            m_strFullPath = null;

            foreach (LocalTreeNode treeNode in Nodes)
            {
                treeNode.DetachFromTree();
            }
        }

        internal void EnsureVisible() { }

        internal bool IsChildOf(LocalTreeNode treeNode)
        {
            if (Level <= treeNode.Level)
            {
                return false;
            }

            var parentNode = Parent;

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

        internal LocalTreeNode Root()
        {
            var nodeParent = this;

            while (nodeParent.Parent != null)
            {
                nodeParent = nodeParent.Parent;
            }

            return nodeParent;
        }

        readonly internal LocalTreeNodeCollection Nodes = null;
        internal string Text = null;
        internal string ToolTipText = null;
        internal string Name = null;
        internal LocalTV TreeView = null;
        internal LocalTreeNode FirstNode = null;
        internal LocalTreeNode NextNode = null;
        internal LocalTreeNode Parent = null;
        internal int Level = -1;
        internal bool Checked = false;
        internal int SelectedImageIndex = -1;
        internal object Tag = null;

        internal int BackColor = UtilColor.Empty;
        internal int ForeColor = UtilColor.Empty;

        string m_strFullPath = null;
    }
}
