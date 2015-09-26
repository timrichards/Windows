using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    class LocalTreeMapFileListNode : LocalTreeNode
    {
        internal override string PathShort { get; set; }

        internal LocalTreeMapFileListNode(string strContent)
            : base()
        {
            PathShort = strContent;
        }

        internal LocalTreeMapFileListNode(LocalTreeNode parent, IReadOnlyList<LocalTreeMapFileNode> lsNodes)
            : this(parent.PathShort)
        {
            Parent = parent;
            PathShort = parent.PathShort;
            Level = parent.Level + 1;       // just for fun: not used

            Nodes = lsNodes;

            LocalTreeMapFileNode nextNode = null;

            foreach (var treeNode in lsNodes.Reverse())
            {
                treeNode.Parent = this;
                treeNode.NextNode = nextNode;
                treeNode.Level = Level + 1;
                nextNode = treeNode;
            }
        }
    }

    class LocalTreeMapFileNode : LocalTreeMapFileListNode
    {
        internal LocalTreeMapFileNode(string strContent)
            : base(strContent) { }
    }
}
