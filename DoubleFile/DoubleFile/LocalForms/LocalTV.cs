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
            if (includeSubTrees)
            {
                return CountSubnodes(Nodes);
            }
            else
            {
                return Nodes.Count;
            }
        }

        internal void Select() { }

        readonly internal LocalTreeNodeCollection Nodes = null;
        internal LocalTreeNode SelectedNode = null;
        internal LocalTreeNode TopNode = null;
        internal Font Font = null;
        internal bool CheckBoxes = false;
        internal bool Enabled = false;

        int CountSubnodes(LocalTreeNodeCollection nodes)
        {
            int nRet = 0;

            foreach (LocalTreeNode treeNode in nodes)
            {
                nRet += CountSubnodes(treeNode.Nodes);
                ++nRet;
            }

            return nRet;
        }
    }
}
