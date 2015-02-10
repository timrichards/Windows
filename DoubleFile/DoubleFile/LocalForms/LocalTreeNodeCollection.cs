using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    class LocalTreeNodeCollection : List<LocalTreeNode>
    {
        internal LocalTreeNodeCollection(LocalTV treeView)
        {
            m_treeView = treeView;
        }

        internal void AddRange(LocalTreeNode[] arrNodes)
        {
            foreach (LocalTreeNode treeNode in arrNodes)
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

        internal LocalTreeNode this[string s]
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
                    nodePrevQuery = (LocalTreeNode)this.Where(t => t.Text == s);
                    return nodePrevQuery;                   // TODO: Trim? ignore case? Probably neither.
                }
            }
        }

        internal new void Clear()
        {
            foreach (LocalTreeNode treeNode in this)
            {
                treeNode.DetachFromTree();
            }

            base.Clear();
        }

        static void SetLevel(LocalTV treeView, LocalTreeNodeCollection nodes, LocalTreeNode nodeParent = null, int nLevel = 0)
        {
            LocalTreeNode nodePrev = null;

            if ((nodeParent != null) && (nodes.Count > 0))
            {
                nodeParent.FirstNode = nodes[0];
            }

            foreach (LocalTreeNode treeNode in nodes)
            {
                if (nodePrev != null)
                {
                    nodePrev.NextNode = treeNode;
                }

                // same assert that Forms generates: must remove it from the other tree first.
                MBoxStatic.Assert(0, (treeNode.TreeView == null) || (treeNode.TreeView == treeView));

                nodePrev = treeNode;
                treeNode.TreeView = treeView;
                treeNode.Parent = nodeParent;
                treeNode.Level = nLevel;
                SetLevel(treeView, treeNode.Nodes, treeNode, nLevel + 1);
            }
        }

        readonly LocalTV m_treeView = null;
        string strPrevQuery = null;
        LocalTreeNode nodePrevQuery = null;
    }
}
