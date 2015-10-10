using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace DoubleFile
{
    [DebuggerDisplay("{_strPathShort} {Nodes?.Count}")]
    class LocalTreeNode : LocalColorItemBase
    {
        internal NodeDatum
            NodeDatum;
        internal RootNodeDatum
            RootNodeDatum => (RootNodeDatum)Root.NodeDatum;

        public IReadOnlyList<LocalTreeNode>
            Nodes { get; protected set; }

        internal LocalTreeNode
            FirstNode => Nodes?.First();
        public virtual LocalTreeNode
            Parent { get; protected set; }

        internal int
            Level { get { return Datum8bits; } private set { Datum8bits = value; } }

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
                    var strRet = "" + sbPath.Insert(0, UseNickname ? treeNode.PathShort : "" + treeNode._strPathShort).Replace(@"\\", @"\");

                    if (2 == strRet.Length)
                        strRet += '\\';

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

        static internal int SetLevel(IEnumerable<LocalTreeNode> nodes, LocalTreeNode nodeParent = null, int nLevel = 0)
        {
            var nCount = 0;

            foreach (var treeNode in nodes)
            {
                treeNode.Parent = nodeParent;
                treeNode.Level = nLevel;

                if (null != treeNode.Nodes)
                    nCount += SetLevel(treeNode.Nodes, treeNode, nLevel + 1);

                ++nCount;
            }

            return nCount;
        }

        internal LocalTreeNode GoToFile(string strFile)
        {
            TreeSelect.DoThreadFactory(this, 0 /* UI Initiator */, strFile);
            return this;
        }

        internal IEnumerable<string> GetFileList(bool bReadAllLines = false)
        {
            var nPrevDir = (int)NodeDatum.PrevLineNo;

            if (0 == nPrevDir)
                return new string[0];

            if (0 == NodeDatum.FileCountHere)
                return new string[0];

            if (bReadAllLines)
            {
                if (1 != _currentIterator?.state)
                {
                    _currentIterator =
                        RootNodeDatum.LVitemProjectVM.ListingFile
                        .ReadLines(99596).As<ReadLinesIterator>();

                    _currentPos = 0;
                }

                if (null != _currentIterator)   // null if not isolated storage file
                {
                    var ieRet =
                        _currentIterator
                        .Skip(nPrevDir - _currentPos)
                        .Take(NodeDatum.FileCountHere);

                    _currentPos = nPrevDir + NodeDatum.FileCountHere;
                    return ieRet;
                }
            }

            _currentIterator = null;    // jic

            return
                RootNodeDatum.LVitemProjectVM.ListingFile
                .ReadLines(99643)
                .Skip(nPrevDir)
                .Take(NodeDatum.FileCountHere)
                .ToArray();
        }

        static ReadLinesIterator
            _currentIterator;
        static int
            _currentPos;
    }
}
