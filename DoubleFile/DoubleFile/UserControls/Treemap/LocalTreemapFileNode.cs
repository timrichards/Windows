using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    interface ITreeMapFileNode { }

    class LocalTreemapFileListNode : LocalTreeNode, ITreeMapFileNode
    {
        internal bool Start = false;
        internal bool Filled = false;

        internal override IReadOnlyList<LocalTreeNode>
            Nodes { get { return Start ? base.Nodes : null; } set { base.Nodes = value; } }

        internal override string PathShort => Parent.PathShort + " (files)";

        internal LocalTreemapFileListNode(LocalTreeNode parent)
            : base()
        {
            Parent = parent;
            ColorcodeFG = UtilColorcode.TreemapFolder;
        }

        internal LocalTreemapFileListNode
            Fill(IReadOnlyList<LocalTreeNode> lsNodes)
        {
            base.Nodes = lsNodes;
            Filled = true;
            return this;
        }
    }

    class LocalTreemapFileNode : LocalTreeNode, ITreeMapFileNode
    {
        internal override string PathShort { get; set; }

        internal LocalTreemapFileNode(LocalTreeNode parent, string strContent)
        {
            Parent = parent;
            PathShort = strContent;
        }
    }
}
