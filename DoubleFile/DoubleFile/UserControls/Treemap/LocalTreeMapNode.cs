using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    class LocalTreemapFileListNode : LocalTreeNode
    {
        internal override string PathShort { get; set; }

        internal new int
            Level { get { return Datum8bits; } private set { Datum8bits = value; } }

        internal LocalTreemapFileListNode(string strContent)
            : base()
        {
            PathShort = strContent;
        }

        internal LocalTreemapFileListNode(LocalTreeNode parent, IReadOnlyList<LocalTreemapFileNode> lsNodes)
            : this(parent.PathShort)
        {
            Parent = parent;
            PathShort = parent.PathShort;
            Level = parent.Level + 1;       // just for fun: not used

            Nodes = lsNodes;

            LocalTreemapFileNode nextNode = null;

            foreach (var treeNode in lsNodes.Reverse())
            {
                treeNode.Parent = this;
                treeNode.NextNode = nextNode;
                treeNode.Level = Level + 1;
                nextNode = treeNode;
            }
        }
    }

    class LocalTreemapFileNode : LocalTreemapFileListNode
    {
        internal LocalTreemapFileNode(string strContent)
            : base(strContent) { }
    }
}
