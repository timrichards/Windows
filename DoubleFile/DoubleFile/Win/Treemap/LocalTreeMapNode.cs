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

        internal LocalTreeMapFileListNode(LocalTreeNode parent, IEnumerable<LocalTreeMapFileNode> lsNodes)
            : this(parent.Text)
        {
            Parent = parent;
            Text = parent.Text;
            Level = parent.Level + 1;       // just for fun: not used

            Nodes = lsNodes.ToArray();

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
