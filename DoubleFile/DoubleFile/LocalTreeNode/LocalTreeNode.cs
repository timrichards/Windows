using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace DoubleFile
{
    interface ILocalTreeNode_SetLevel
    {
        int Set(IEnumerable<LocalTreeNode> rootNodes);
    }

    [DebuggerDisplay("{PathShort} {Nodes?.Count}")]
    class LocalTreeNode : ILocalTreeNode_SetLevel
    {
        internal NodeDatum
            NodeDatum;
        internal virtual IReadOnlyList<LocalTreeNode>
            Nodes { get; set; }
        internal LocalTreeNode
            Parent;
        internal List<LocalTreeNode>
            Clones;
        internal LVitem_ClonesVM
            LVitem;
        internal LocalTreemapFileListNode
            TreemapFiles;
        internal Rect
            TreemapRect;

        internal LocalTreeNode
            FirstNode => Nodes?.First();
        internal RootNodeDatum
            RootNodeDatum => (RootNodeDatum)Root.NodeDatum;
        internal bool
            IsSolitary => null == Clones;
        internal bool
            IsAllOnOneVolume => UtilColorcode.AllOnOneVolume == Clones?[0].ColorcodeFG;

        internal int ColorcodeFG { get { return UtilColorcode.GetFG_ARGB(Color); } set { int c = Color; Color = UtilColorcode.SetFG_ARGB(ref c, value); } }
        internal int ColorcodeBG { get { return UtilColorcode.GetBG_ARGB(Color); } set { int c = Color; Color = UtilColorcode.SetBG_ARGB(ref c, value); } }

        internal Brush
            Foreground =>
            (UtilColorcode.Transparent == ColorcodeFG)
            ? Brushes.White
            : UtilColorcode.ARGBtoBrush(ColorcodeFG);

        internal Brush
            Background => UtilColorcode.ARGBtoBrush(ColorcodeBG);

        internal int Level
        {
            get { return (int)(_datum_color_level & _knDatum8bitMask) >> UtilColorcode.CLUT_Shift; }
            set { _datum_color_level = (short)((_datum_color_level & (-1 - _knDatum8bitMask)) + (value << UtilColorcode.CLUT_Shift)); }
        }

        internal LocalTreeNode
            Root
        {
            get
            {
                var parent = this;

                for (; null != parent.Parent; parent = parent.Parent)
                    ;

                return parent;
            }
        }

        internal LocalTreeNode()
        {
            Color = UtilColorcode.Set_ARGB(UtilColorcode.Transparent, UtilColorcode.Transparent);
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

        internal virtual string
            PathShort
        {
            get
            {
                var rootNodeDatum = NodeDatum.As<RootNodeDatum>();

                return
                    (null != rootNodeDatum)
                    ? "" + rootNodeDatum.LVitemProjectVM.RootText   // if this is a root treenode return nickname text
                    : "" + _strPathShort;
            }
            set { _strPathShort = (TabledString<TabledStringType_Folders>)value; }
        }

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

        internal bool
            IsChildOf(LocalTreeNode treeNode)
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

        int
            ILocalTreeNode_SetLevel.Set(IEnumerable<LocalTreeNode> rootNodes) => SetLevel(rootNodes, null, 0); 
        static int SetLevel(IEnumerable<LocalTreeNode> nodes, LocalTreeNode nodeParent, int nLevel)
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

        internal LocalTreeNode
            GoToFile(string strFile)
        {
            TreeSelect.DoThreadFactory(this, 0 /* UI Initiator */, strFile);
            return this;
        }

        internal IEnumerable<string>
            GetFileList(bool bReadAllLines = false)
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

        int Color
        {
            get { return _datum_color_level & UtilColorcode.CLUT_Mask; }
            set { _datum_color_level = (short)((_datum_color_level & (-1 - UtilColorcode.CLUT_Mask)) + value); }
        }

        const uint
            _knDatum16bitMask = (uint)((1 << 16) - 1) << 16;
        short
            _datum_color_level = 0;

        TabledString<TabledStringType_Folders>
            _strPathShort;
        static readonly uint
            _knDatum8bitMask = (1 << 16) - 1 - UtilColorcode.CLUT_Mask;
        static ReadLinesIterator
            _currentIterator;
        static int
            _currentPos;
    }
}
