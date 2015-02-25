using System.Collections.Generic;
using System.Drawing;

namespace DoubleFile
{
    class LocalTV
    {
        readonly internal LocalTreeNodeCollection
            _Nodes = null;
        internal LocalTreeNode
            _SelectedNode = null;
        internal LocalTreeNode
            _TopNode = null;
        internal Font
            _Font = null;
        internal bool
            _CheckBoxes = false;
        internal bool
            _Enabled = false;
        
        internal LocalTV()
        {
            _Nodes = new LocalTreeNodeCollection(this);
        }

        internal int GetNodeCount(bool includeSubTrees = false)
        {
            return includeSubTrees ? CountSubnodes(_Nodes) : _Nodes.Count;
        }

        internal void Select() { }

        static int CountSubnodes(IEnumerable<LocalTreeNode> nodes)
        {
            var nRet = 0;

            foreach (var treeNode in nodes)
            {
                nRet += CountSubnodes(treeNode._Nodes);
                ++nRet;
            }

            return nRet;
        }
    }
}
