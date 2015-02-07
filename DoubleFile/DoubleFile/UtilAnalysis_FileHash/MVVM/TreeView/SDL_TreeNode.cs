using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace DoubleFile
{
    class SDL_TreeNode
    {
        internal TreeViewItem_FileHashVM TVIVM = null;
        internal LVitem_FileHashVM LVIVM = null;

        internal SDL_TreeNode()
        {
            Nodes = new SDL_TreeNodeCollection(TreeView);
        }

        internal SDL_TreeNode(string strContent)
            : this()
        {
            Text = strContent;
        }

        internal SDL_TreeNode(string strContent, SDL_TreeNode[] arrNodes)
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

                Stack<SDL_TreeNode> stack = new Stack<SDL_TreeNode>(8);
                SDL_TreeNode nodeParent = Parent;

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

            foreach (SDL_TreeNode treeNode in Nodes)
            {
                treeNode.DetachFromTree();
            }
        }

        internal void EnsureVisible() { }

        internal bool IsChildOf(SDL_TreeNode treeNode)
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

        internal SDL_TreeNode Root()
        {
            var nodeParent = this;

            while (nodeParent.Parent != null)
            {
                nodeParent = nodeParent.Parent;
            }

            return nodeParent;
        }

        readonly internal SDL_TreeNodeCollection Nodes = null;
        internal string Text = null;
        internal string ToolTipText = null;
        internal string Name = null;
        internal WPF_TreeView TreeView = null;
        internal SDL_TreeNode FirstNode = null;
        internal SDL_TreeNode NextNode = null;
        internal SDL_TreeNode Parent = null;
        internal int Level = -1;
        internal bool Checked = false;
        internal int SelectedImageIndex = -1;
        internal object Tag = null;

        internal Color BackColor = Color.Empty;
        internal Color ForeColor = Color.Empty;

        string m_strFullPath = null;
    }
}
