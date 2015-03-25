using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    class LocalTreeMapNode : LocalTreeNode
    {
        internal override string Text { get; set; }

        internal LocalTreeMapNode(string strContent)
            : base()
        {
            Text = strContent;
        }

        internal LocalTreeMapNode(string strContent, IReadOnlyList<LocalTreeNode> lsNodes)
            : this(strContent)
        {
            Nodes = lsNodes.ToArray();
        }
    }
}
