using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using DoubleFile;

namespace DoubleFile
{
    partial class TreeSelect : UtilDirList
    {
        internal TreeSelect(LocalTreeNode node,
            bool bCompareMode = false, bool bSecondComparePane = false)
        {
            _treeNode = node;

            if (null != LocalTreeNode.TreeView)
                _dictVolumeInfo = LocalTreeNode.TreeView._dictVolumeInfo;

            _bCompareMode = bCompareMode;
            _bSecondComparePane = bSecondComparePane;
        }

        // This is the original GetFileList() that returns multiple info columns
        // Static and is used by the treemap.
        // However it deprecatedly splits each line and formats it:
        // unused and too far upstream
        internal static List<string[]> GetFileList(
            LocalTreeNode parent,
            List<ulong> listLength = null)
        {
            var nodeDatum = parent.NodeDatum;
            var rootNodeDatum = parent.Root().NodeDatum as RootNodeDatum;
            var listFiles = new List<string[]>();

            if ((null == nodeDatum) ||
                (nodeDatum.LineNo == 0) ||
                (null == rootNodeDatum))
            {
                return listFiles;
            }

            var strListingFile = rootNodeDatum.ListingFile;

            var nPrevDir = (int) nodeDatum.PrevLineNo;
            var nLineNo = (int) nodeDatum.LineNo;

            if (nPrevDir == 0)
            {
                return listFiles;
            }

            if ((nLineNo - nPrevDir) <= 1)  // dir has no files
            {
                return listFiles;
            }

            ulong nLengthDebug = 0;

            foreach (var strFileLine
                in File.ReadLines(strListingFile)
                .Skip(nPrevDir)
                .Take((nLineNo - nPrevDir - 1)))
            {
                var strArrayFiles = strFileLine.Split('\t')
                    .Skip(3)
                    .ToArray();
                ulong nLength = 0;

                strArrayFiles[3] = DecodeAttributes(strArrayFiles[3]);

                if ((strArrayFiles.Length > knColLengthLV) &&
                    (false == string.IsNullOrWhiteSpace(strArrayFiles[knColLengthLV])))
                {
                    nLengthDebug += nLength = ulong.Parse(strArrayFiles[knColLengthLV]);
                    strArrayFiles[knColLengthLV] = FormatSize(strArrayFiles[knColLengthLV]);
                }

                listFiles.Add(strArrayFiles);

                if (listLength != null)
                {
                    listLength.Add(nLength);
                }
            }

            MBoxStatic.Assert(1301.2313, nLengthDebug == nodeDatum.Length);
            return listFiles;
        }

        internal Thread DoThreadFactory()
        {
            if (_treeNode is LocalTreeMapNode)     // does not support immediate file fake nodes
                return null;

            _thread = new Thread(Go) { IsBackground = true };
            _thread.Start();
            return _thread;
        }

        readonly LocalTreeNode
            _treeNode = null;
        readonly Dictionary<string, string>
            _dictVolumeInfo = null;
        Thread
            _thread = null;
        readonly bool
            _bCompareMode = false;
        readonly bool
            _bSecondComparePane = false;
    }
}
