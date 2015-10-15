using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    interface ITreeMapFileNode { }

    class LocalTreemapFileListNode : LocalTreeNode, ITreeMapFileNode
    {
        internal bool Start = false;

        internal override IReadOnlyList<LocalTreeNode>
            Nodes => Start ? base.Nodes : null;

        internal override string PathShort => Parent.PathShort + " (files)";

        internal LocalTreemapFileListNode(LocalTreeNode treeNode)
            : base()
        {
            Parent = treeNode;
            ColorcodeFG = UtilColorcode.TreemapFolder;
            NodeDatum = new NodeDatum(lengthTotal: treeNode.NodeDatum.LengthHere);
        }

        new internal IEnumerable<string>        // C# has this overload/override problem: the parameter can be optional here so who do you call?
            GetFileList(bool doNotUseThisMethod_itIsOverridden)
        {
            Util.Assert(99589, false);
            return null;
        }

        internal void
            GetFileList()
        {
            if (0 < (base.Nodes?.Count ?? 0))   // C# also has this base override is null problem: (null != base.Nodes) is always true even when null
                return;

            ulong nLengthHere = 0;

            base.Nodes =
                Parent.GetFileList()
                .Select(s => s
                .Split('\t').Skip(3).ToArray())    // makes this an LV line: knColLengthLV
                .Select(asFileLine =>
            {
                ulong nLength = 0;

                if ((asFileLine.Length > FileParse.knColLengthLV) &&
                    (false == string.IsNullOrWhiteSpace(asFileLine[FileParse.knColLengthLV])))
                {
                    nLengthHere += nLength = ("" + asFileLine[FileParse.knColLengthLV]).ToUlong();
                }

                return                   // from lambda
                    (0 < nLength)
                    ? new LocalTreemapFileNode(this, asFileLine[0], nLength)
                    : null;                               
            })
                .Where(fileNode => null != fileNode)
                .ToList();

            Util.Assert(99650, nLengthHere == Parent.NodeDatum.LengthHere);
        }
    }

    class LocalTreemapFileNode : LocalTreeNode, ITreeMapFileNode
    {
        internal override string PathShort { get; set; }

        internal LocalTreemapFileNode(LocalTreeNode treeNode, string strContent, ulong nLength)
        {
            Parent = treeNode;
            NodeDatum = new NodeDatum(lengthTotal: nLength);

            PathShort = strContent;
            ColorcodeFG = UtilColorcode.TreemapFile;
        }

        internal LocalTreemapFileNode(LocalTreeNode treeNode, ulong nLength, string strContent, int nColorcode)
        {
            Parent = treeNode;
            NodeDatum = new NodeDatum(lengthTotal: nLength);

            PathShort = nLength.FormatSize() + " " + strContent;
            ColorcodeFG = nColorcode;
        }
    }
}
