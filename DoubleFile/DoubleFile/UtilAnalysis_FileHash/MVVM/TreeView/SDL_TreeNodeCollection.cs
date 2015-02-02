using System.Linq;

namespace DoubleFile
{
    class SDL_TreeNodeCollection : UList<SDL_TreeNode>
    {
        internal SDL_TreeNodeCollection(TreeViewVMhack treeView)
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

        internal bool ContainsKey(string s)
        {
            if (s != strPrevQuery)
            {
                strPrevQuery = s;
                nodePrevQuery = this[s];
            }

            return (nodePrevQuery != null);
        }

        internal SDL_TreeNode this[string s]
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

        static void SetLevel(TreeViewVMhack treeView, SDL_TreeNodeCollection nodes, SDL_TreeNode nodeParent = null, int nLevel = 0)
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
                MBox.Assert(0, (treeNode.TreeView == null) || (treeNode.TreeView == treeView));

                nodePrev = treeNode;
                treeNode.TreeView = treeView;
                treeNode.Parent = nodeParent;
                treeNode.Level = nLevel;
                SetLevel(treeView, treeNode.Nodes, treeNode, nLevel + 1);
            }
        }

        readonly TreeViewVMhack m_treeView = null;
        string strPrevQuery = null;
        SDL_TreeNode nodePrevQuery = null;
    }
}
