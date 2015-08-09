using System.Collections.Generic;
using System.Text;

namespace DoubleFile
{
    class LocalTreeNode : LocalColorItemBase
    {
        public IReadOnlyList<LocalTreeNode>
            Nodes { get; protected set; }

        internal virtual string
            Text
        {
            get
            {
                var strText = "" + _text;
                var rootNodeDatum = NodeDatum.As<RootNodeDatum>();

                return
                    (null != rootNodeDatum)
                    ? rootNodeDatum.RootText(strText)
                    : strText;
            }
            set { _text = (TabledString<TabledStringType_Folders>)value; }
        }
        TabledString<TabledStringType_Folders> _text;

        internal LocalTreeNode
            FirstNode => (0 < (Nodes?.Count ?? 0)) ? Nodes[0] : null;
        public virtual LocalTreeNode
            NextNode { get; protected set; }
        public virtual LocalTreeNode
            Parent { get; protected set; }

        internal int
            Level { get { return Datum8bits; } set { Datum8bits = value; } }
        internal int
            SelectedImageIndex { get { return Datum16bits; } set { Datum16bits = value; } }

        internal LocalTreeNode Root
        {
            get
            {
                var nodeParent = this;

                while (nodeParent.Parent != null)
                    nodeParent = nodeParent.Parent;

                return nodeParent;
            }
        }

        internal LocalTreeNode()
        {
            Level = -1;
            SelectedImageIndex = -1;
        }

        internal LocalTreeNode(string strContent)
            : this()
        {
            Text = strContent;
        }

        internal LocalTreeNode(TabledString<TabledStringType_Folders> strContent)
            : this()
        {
            _text = strContent;
        }

        internal LocalTreeNode(string strContent, IReadOnlyList<LocalTreeNode> lsNodes)
            : this(strContent)
        {
            Nodes = lsNodes;
        }

        internal LocalTreeNode DetachFromTree()
        {
            Level = -1;

            Nodes?.ForEach(treeNode =>
                treeNode.DetachFromTree());

            return this;
        }

        internal string FullPath
        {
            get
            {
                var sbPath = new StringBuilder();
                var treeNode = this;

                do
                {
                    sbPath
                        .Insert(0, '\\')
                        .Insert(0, treeNode.Text);
                }
                while (null != (treeNode = treeNode.Parent));

                return ("" + sbPath).TrimEnd('\\');
            }
        }

        internal bool IsChildOf(LocalTreeNode treeNode)
        {
            if (Level <= treeNode.Level)
                return false;

            var parentNode = Parent;

            while (parentNode != null)
            {
                if (parentNode == treeNode)
                    return true;

                parentNode = parentNode.Parent;
            }

            return false;
        }

        static internal void SetLevel(IReadOnlyList<LocalTreeNode> nodes, LocalTreeNode nodeParent = null, int nLevel = 0)
        {
            LocalTreeNode nodePrev = null;

            nodes?.ForEach(treeNode =>
            {
                if (null != nodePrev)
                    nodePrev.NextNode = treeNode;

                nodePrev = treeNode;
                treeNode.Parent = nodeParent;
                treeNode.Level = nLevel;
                SetLevel(treeNode.Nodes, treeNode, nLevel + 1);
            });
        }

        internal LocalTreeNode GoToFile(string strFile)
        {
            TreeSelect.DoThreadFactory(this, 0 /* UI Initiator */, strFile);
            return this;
        }

        internal NodeDatum
            NodeDatum;
    }
}
