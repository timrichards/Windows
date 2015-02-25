using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    class LocalTreeNodeCollection : UList<LocalTreeNode>
    {
        internal LocalTreeNodeCollection(LocalTV treeView)
        {
            _treeView = treeView;
        }

        internal void AddRange(IReadOnlyList<LocalTreeNode> lsNodes)
        {
            foreach (var treeNode in lsNodes)
            {
                Add(treeNode);
            }

            if ((false == this.IsEmpty()) && (_treeView != null))
            {
                _treeView._TopNode = this[0];
                SetLevel(_treeView, this);
            }
        }

        internal bool ContainsKeyA(string s)
        {
            if (s != _strPrevQuery)
            {
                _strPrevQuery = s;
                _nodePrevQuery = this[s];
            }

            return (_nodePrevQuery != null);
        }

        internal LocalTreeNode this[string s]
        {
            get
            {
                if (s == _strPrevQuery)
                {
                    return _nodePrevQuery;
                }
                else
                {
                    _strPrevQuery = s;
                    Keys
                        .Where(t => t._Text == s)
                        .FirstOnlyAssert(t => _nodePrevQuery = t);
                    return _nodePrevQuery;                   // TODO: Trim? ignore case? Probably neither.
                }
            }
        }

        internal new void Clear()
        {
            foreach (var treeNode in this)
            {
                treeNode.DetachFromTree();
            }

            base.Clear();
        }

        static void SetLevel(LocalTV treeView,
            IReadOnlyList<LocalTreeNode> nodes, LocalTreeNode nodeParent = null, int nLevel = 0)
        {
            LocalTreeNode nodePrev = null;

            if ((nodeParent != null) && (false == nodes.IsEmptyA()))
            {
                nodeParent._FirstNode = nodes[0];
            }

            foreach (var treeNode in nodes)
            {
                if (nodePrev != null)
                {
                    nodePrev._NextNode = treeNode;
                }

                // same assert that Forms generates: must remove it from the other tree first.
                MBoxStatic.Assert(99999, (treeNode._TreeView == null) || (treeNode._TreeView == treeView));

                nodePrev = treeNode;
                treeNode._TreeView = treeView;
                treeNode._Parent = nodeParent;
                treeNode.Level = nLevel;
                SetLevel(treeView, treeNode._Nodes, treeNode, nLevel + 1);
            }
        }

        readonly LocalTV
            _treeView = null;
        string
            _strPrevQuery = null;
        LocalTreeNode
            _nodePrevQuery = null;
    }
}
