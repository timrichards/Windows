using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    class LocalTreeMapFileListNode : LocalTreeNode
    {
        internal override string Text { get; set; }

        internal LocalTreeMapFileListNode(string strContent)
            : base()
        {
            Text = strContent;
        }

        internal LocalTreeMapFileListNode(LocalTreeNode parent, IReadOnlyList<LocalTreeMapFileNode> lsNodes)
            : this(parent.Text)
        {
            Parent = parent;
            Nodes = lsNodes.ToArray();

            LocalTreeMapFileNode nextNode = null;

            foreach (var treeNode in lsNodes.Reverse())
            {
                treeNode.Parent = this;
                treeNode.NextNode = nextNode;
                nextNode = treeNode;
                Level = parent.Level + 1;       // just for fun: not used
            }
        }
    }

    class LocalTreeMapFileNode : LocalTreeMapFileListNode
    {
        internal LocalTreeMapFileNode(string strContent)
            : base(strContent) { }
    }
}
