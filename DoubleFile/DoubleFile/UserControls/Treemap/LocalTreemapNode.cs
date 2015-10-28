using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    interface ITreemapNode { }

    class LocalTreemapFileListNode : LocalTreeNode, ITreemapNode
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

        internal new void
            GetFileList()
        {
            if (0 < (base.Nodes?.Count ?? 0))   // C# has this base override is null problem: (null != base.Nodes) is always true even when null
                return;

            ulong nLengthHere = 0;
            var nHashColumn = Statics.DupeFileDictionary.HashColumn - 3;

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

                if (0 == nLength)
                    return null;        // from lambda

                var isDuplicate =
                    (nHashColumn < asFileLine.Length)
                    ? Statics.DupeFileDictionary.IsDupeSepVolume(HashTuple.FileIndexedIDfromString(asFileLine[nHashColumn], nLength))
                    : false;

                return new LocalTreemapNode(this, asFileLine[0], nLength, isDuplicate);     // from lambda                        
            })
                .Where(fileNode => null != fileNode)
                .ToList();

            Util.Assert(99650, nLengthHere == Parent.NodeDatum.LengthHere);
        }
    }

    class LocalTreemapNode : LocalTreeNode, ITreemapNode
    {
        internal override string PathShort { get; set; }
        internal bool IsFile = false;

        // File
        internal LocalTreemapNode(LocalTreeNode treeNode, string strContent, ulong nLength, bool? isDuplicate)
        {
            Parent = treeNode;
            NodeDatum = new NodeDatum(lengthTotal: nLength);
            PathShort = strContent;

            ColorcodeFG =
                (null == isDuplicate)
                ? UtilColorcode.TreemapUniqueFile
                : isDuplicate.Value
                ? UtilColorcode.TreemapDupeSepVol
                : UtilColorcode.TreemapDupeOneVol;

            IsFile = true;
        }

        // Volume free; volume unread
        internal LocalTreemapNode(LocalTreeNode treeNode, ulong nLength, string strContent, int nColorcode)
        {
            Parent = treeNode;
            NodeDatum = new NodeDatum(lengthTotal: nLength);
            PathShort = strContent;

            ColorcodeFG = nColorcode;
        }
    }
}
