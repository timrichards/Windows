using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using DoubleFile;

namespace Local
{
    delegate void TreeSelectStatusDelegate(
        IReadOnlyList<LocalLVitem> lvItemDetails = null,
        IReadOnlyList<LocalLVitem> itemArray = null,
        IEnumerable<LocalLVitem> lvVolDetails = null,
        bool bSecondComparePane = false,
        LVitemFileTag lvFileItem = null);
    delegate void TreeSelectDoneDelegate(bool bSecondComparePane);

    class TreeSelect : UtilAnalysis_DirList
    {
        internal TreeSelect(LocalTreeNode node,
            Dictionary<string, string> dictDriveInfo,
            string strFile, bool bCompareMode, bool bSecondComparePane,
            TreeSelectStatusDelegate statusCallback, TreeSelectDoneDelegate doneCallback)
        {
            m_treeNode = node;
            m_dictDriveInfo = dictDriveInfo;
            m_strFile = strFile;
            m_bCompareMode = bCompareMode;
            m_bSecondComparePane = bSecondComparePane;
            m_statusCallback = statusCallback;
            m_doneCallback = doneCallback;
        }

        internal static List<string[]> GetFileList(LocalTreeNode parent, List<ulong> listLength = null)
        {
            var nodeDatum = parent.Tag as NodeDatum;
            var rootNodeDatum = parent.Root().Tag as RootNodeDatum;
            var listFiles = new List<string[]>();

            if ((null == nodeDatum) ||
                (nodeDatum.nLineNo == 0) ||
                (null == rootNodeDatum))
            {
                return listFiles;
            }

            var strFile = rootNodeDatum.StrFile;
            var nPrevDir = (int) nodeDatum.nPrevLineNo;
            var nLineNo = (int) nodeDatum.nLineNo;

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
                in File.ReadLines(strFile)
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

            MBoxStatic.Assert(1301.2313, nLengthDebug == nodeDatum.nLength);
            return listFiles;
        }

        internal Thread DoThreadFactory()
        {
            m_thread = new Thread(Go) {IsBackground = true};
            m_thread.Start();
            return m_thread;
        }

        void Go()
        {
            Go_A();

            if (m_bCompareMode == false)
            {
                // Volume detail

                if (m_dictDriveInfo.ContainsKeyA(m_strFile))
                {
                    var arrDriveInfo = m_dictDriveInfo[m_strFile]
                        .Split(new[] { "\r\n", "\n" },
                        StringSplitOptions.None);

                    MBoxStatic.Assert(1301.2314,
                        new[] { 7, 8, 10, kanDIviewOrder.Length }
                        .Contains(arrDriveInfo.Length));

                    var asItems = new string[arrDriveInfo.Length][];

                    for (var i = 0; i < arrDriveInfo.Length; ++i)
                    {
                        var a = arrDriveInfo[i].Split('\t');

                        if (a[1].Trim().Length == 0)
                        {
                            continue;
                        }

                        asItems[i] = new[]
                        {
                            a[0],
                            kabDIsizeType[i] ?
                                FormatSize(a[1], bBytes: true) :
                                a[1]
                        };
                    }

                    var lvItems = new LocalLVitem[arrDriveInfo.Length];

                    for (var ix = 0; ix < arrDriveInfo.Length; ++ix)
                    {
                        if ((asItems[ix] == null) ||
                            (asItems[ix].Length == 0) ||
                            (asItems[ix][1].Trim().Length == 0))
                        {
                            continue;
                        }

                        if ((kanDIoptIfEqTo[ix] != -1) &&
                            (asItems[ix][1] == asItems[kanDIoptIfEqTo[ix]][1]))
                        {
                            continue;
                        }

                        var ixA = (arrDriveInfo.Length == kanDIviewOrder.Length) ?
                            kanDIviewOrder[ix] :
                            ix;

                        lvItems[ixA] = new LocalLVitem(asItems[ix]);
                    }

                    m_statusCallback(lvVolDetails: lvItems.Where(i => i != null));
                }
            }

            m_doneCallback(m_bSecondComparePane);
        }

        void Go_A()
        {
            if (false == File.Exists(m_strFile))
            {
                MBoxStatic.Assert(1301.2311, false);
                return;
            }

            var nodeDatum = m_treeNode.Tag as NodeDatum;

            if ((null == nodeDatum) || 
                (nodeDatum.nLineNo == 0))
            {
                return;
            }

            var nPrevDir = nodeDatum.nPrevLineNo;
            var nLineNo = (int)nodeDatum.nLineNo;
            string[] strArray = null;

            File
                .ReadLines(m_strFile)
                .Skip(nLineNo - 1)
                .Take(1)
                .FirstOnlyAssert(strLine =>
                {
                    strArray = strLine.Split('\t');
                    UtilProject.WriteLine(strLine);
                });

            MBoxStatic.Assert(1301.2312, (false == string.IsNullOrWhiteSpace(strArray[2])));

            var nIx = 0;
            DateTime dt;

            // Directory detail

            var listItems = new UList<LocalLVitem>();

            nIx = 4; if ((strArray.Length > nIx) && (false == string.IsNullOrWhiteSpace(strArray[nIx]))) listItems.Add(new LocalLVitem(new[] { "Created\t", (dt = DateTime.Parse(strArray[nIx])).ToLongDateString() + ", " + dt.ToLongTimeString() }));
            nIx = 5; if ((strArray.Length > nIx) && (false == string.IsNullOrWhiteSpace(strArray[nIx]))) listItems.Add(new LocalLVitem(new[] { "Modified\t", (dt = DateTime.Parse(strArray[nIx])).ToLongDateString() + ", " + dt.ToLongTimeString() }));
            nIx = 6; if ((strArray.Length > nIx) && (false == string.IsNullOrWhiteSpace(strArray[nIx]))) listItems.Add(new LocalLVitem(new[] { "Attributes\t", DecodeAttributes(strArray[nIx]) }));
            listItems.Add(new LocalLVitem(new[] { "Immediate Size\t", FormatSize(nodeDatum.nLength, bBytes: true) }));
            nIx = 8; if ((strArray.Length > nIx) && (false == string.IsNullOrWhiteSpace(strArray[nIx]))) listItems.Add(new LocalLVitem(new[] { "Error 1\t", strArray[nIx] }));
            nIx = 9; if ((strArray.Length > nIx) && (false == string.IsNullOrWhiteSpace(strArray[nIx]))) listItems.Add(new LocalLVitem(new[] { "Error 2\t", strArray[nIx] }));
            listItems.Add(new LocalLVitem(new[] { "# Immediate Files", (nLineNo - nPrevDir - 1).ToString() }));

            // Tree subnode detail

            const string NUMFMT = "###,###,###,##0";

            listItems.Add(new LocalLVitem(new[] { "# Immediate Folders", m_treeNode.Nodes.Count.ToString(NUMFMT) }));
            listItems.Add(new LocalLVitem(new[] { "Total # Files", nodeDatum.nFilesInSubdirs.ToString(NUMFMT) }));

            if (nodeDatum.nSubDirs > 0)
            {
                var strItem = nodeDatum.nSubDirs.ToString(NUMFMT);

                if (nodeDatum.nDirsWithFiles > 0)
                {
                    var nDirsWithFiles = nodeDatum.nDirsWithFiles;

                    if (nodeDatum.nImmediateFiles > 0)
                    {
                        --nDirsWithFiles;
                    }

                    if (nDirsWithFiles > 0)
                    {
                        strItem += " (" + nDirsWithFiles.ToString(NUMFMT) + " with files)";
                    }
                }

                listItems.Add(new LocalLVitem(new string[] { "# Subfolders", strItem }));
            }

            listItems.Add(new LocalLVitem(new string[] { "Total Size", FormatSize(nodeDatum.nTotalLength, bBytes: true) }));
            m_statusCallback(lvItemDetails: listItems.ToArray(), bSecondComparePane: m_bSecondComparePane);

            var listFiles_A = GetFileList(m_treeNode);

            if (listFiles_A.IsEmpty())
            {
                return;
            }

            var listFiles = new UList<LocalLVitem>();

            foreach (var arrLine in listFiles_A)
            {
                listFiles.Add(new LocalLVitem(arrLine));
            }

            m_statusCallback(itemArray: listFiles.ToArray(),
                bSecondComparePane: m_bSecondComparePane,
                lvFileItem: new LVitemFileTag(m_treeNode.Text, listFiles.Count));
        }

        readonly LocalTreeNode m_treeNode = null;
        readonly Dictionary<string, string> m_dictDriveInfo = null;
        readonly TreeSelectStatusDelegate m_statusCallback = null;
        readonly TreeSelectDoneDelegate m_doneCallback = null;
        Thread m_thread = null;
        readonly string m_strFile = null;
        readonly bool m_bCompareMode = false;
        readonly bool m_bSecondComparePane = false;
    }
}
