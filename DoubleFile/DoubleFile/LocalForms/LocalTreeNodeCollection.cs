using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    class LocalTreeNodeCollection : KeyList<LocalTreeNode>
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
                _treeView.TopNode = this[0];
                LocalTreeNode.SetLevel(_treeView, this);
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
                        .Where(t => t.Text == s)
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

        readonly LocalTV
            _treeView = null;
        string
            _strPrevQuery = null;
        LocalTreeNode
            _nodePrevQuery = null;
    }
}
