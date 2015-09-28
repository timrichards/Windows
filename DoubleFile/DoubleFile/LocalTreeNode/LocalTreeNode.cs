using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DoubleFile
{
    [DebuggerDisplay("{_strPathShort} {Nodes?.Count}")]
    class LocalTreeNode : LocalColorItemBase
    {
        public IReadOnlyList<LocalTreeNode>
            Nodes { get; protected set; }

        internal LocalTreeNode
            FirstNode => (0 < (Nodes?.Count ?? 0)) ? Nodes[0] : null;
        public virtual LocalTreeNode
            NextNode { get; protected set; }
        public virtual LocalTreeNode
            Parent { get; protected set; }

        internal int
            Level { get { return Datum8bits; } set { Datum8bits = value; } }
        //internal int
        //    SelectedImageIndex { get { return Datum16bits; } set { Datum16bits = value; } }

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
        }

        internal LocalTreeNode(string strContent)
            : this()
        {
            PathShort = strContent;
        }

        internal LocalTreeNode(TabledString<TabledStringType_Folders> strContent)
            : this()
        {
            _strPathShort = strContent;
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

        internal virtual string
            PathShort
        {
            get
            {
                var strText = "" + _strPathShort;
                var rootNodeDatum = NodeDatum.As<RootNodeDatum>();

                return
                    (null != rootNodeDatum)
                    ? "" + rootNodeDatum.LVitemProjectVM.RootText   // if this is a root treenode return nickname text
                    : strText;
            }
            set { _strPathShort = (TabledString<TabledStringType_Folders>)value; }
        }
        TabledString<TabledStringType_Folders> _strPathShort;

        internal string
            PathFull => PathFullGet(true);
        internal string
            PathFullGet(bool UseNickname)
        {
            var sbPath = new StringBuilder();

            for (var treeNode = this; ; treeNode = treeNode.Parent)
            {
                if (null == treeNode.Parent)
                {
                    var strRet = "" + sbPath.Insert(0, UseNickname ? treeNode.PathShort : "" + treeNode._strPathShort);

                    if (2 == strRet.Length)
                        strRet += "\\";

                    return strRet;
                }

                sbPath
                    .Insert(0, treeNode._strPathShort)
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

        internal NodeDatum
            NodeDatum;
    }
}
