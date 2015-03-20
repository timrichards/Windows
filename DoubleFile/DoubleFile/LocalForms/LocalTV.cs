using System.Collections.Generic;
using System.Drawing;

namespace DoubleFile
{
    class LocalTV
    {
        internal Dictionary<string, string>
            _dictVolumeInfo = null;
        internal LocalTreeNodeCollection
            Nodes { get; private set; }
        internal LocalTreeNode
            SelectedNode { get; set; }
        internal LocalTreeNode
            TopNode { get; set; }
        internal Font
            Font { get; set; }
        internal bool
            CheckBoxes { get; set; }
        internal bool
            Enabled { get; set; }
        
        internal LocalTV()
        {
            Nodes = new LocalTreeNodeCollection(this);
        }

        internal int GetNodeCount(bool includeSubTrees = false)
        {
            return includeSubTrees ? CountSubnodes(Nodes) : Nodes.Count;
        }

        internal void Select() { }

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
