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
                    ? "" + rootNodeDatum.LVitemProjectVM.RootText   // if this is a root treenode return nickname text
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
            NextNode = null;
            Parent = null;
            Level = -1;

            Nodes?.ForEach(treeNode =>
                treeNode.DetachFromTree());

            Nodes = null;
            return this;
        }

        internal string
            FullPath => FullPathGet(true);
        internal string
            FullPathGet(bool UseNickname)
        {
            var sbPath = new StringBuilder();

            for (var treeNode = this; ; treeNode = treeNode.Parent)
            {
                if (null == treeNode.Parent)
                    return ("" + sbPath.Insert(0, UseNickname ? treeNode.Text : "" + treeNode._text));

                sbPath
                    .Insert(0, treeNode._text)
                    .Insert(0, '\\');
            }
        }

        internal bool IsChildOf(LocalTreeNode treeNode)
        {
            if (Level <= treeNode.Level)
                return false;

            var parentNode = Parent;

            while (null != parentNode)
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

        public override string ToString() => FullPath;      // for debug

        internal NodeDatum
            NodeDatum;
    }
}
