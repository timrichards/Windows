using System.Collections.Generic;
using System.Drawing;

namespace DoubleFile
{
    class LocalTV
    {
        internal LocalTV()
        {
            Nodes = new LocalTreeNodeCollection(this);
        }

        internal int GetNodeCount(bool includeSubTrees = false)
        {
            return includeSubTrees ? CountSubnodes(Nodes) : Nodes.Count;
        }

        internal void Select() { }

        readonly internal LocalTreeNodeCollection Nodes = null;
        internal LocalTreeNode SelectedNode = null;
        internal LocalTreeNode TopNode = null;
        internal Font Font = null;
        internal bool CheckBoxes = false;
        internal bool Enabled = false;

        static int CountSubnodes(IEnumerable<LocalTreeNode> nodes)
        {
            var nRet = 0;

            foreach (var treeNode in nodes)
            {
                nRet += CountSubnodes(treeNode.Nodes);
                ++nRet;
            }

            return nRet;
        }
    }
}
