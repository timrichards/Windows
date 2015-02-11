using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace DoubleFile
{
    delegate void TreeSelectStatusDelegate(ListViewItem[] lvItemDetails = null, ListViewItem[] itemArray = null, ListViewItem[] lvVolDetails = null, bool bSecondComparePane = false, LVitemFileTag lvFileItem = null);
    delegate void TreeSelectDoneDelegate(bool bSecondComparePane);

    class TreeSelect : UtilAnalysis_DirList
    {
        internal TreeSelect(TreeNode node, SortedDictionary<FolderKeyStruct, UList<TreeNode>> dictNodes, Dictionary<string, string> dictDriveInfo,
            string strFile, bool bCompareMode, bool bSecondComparePane,
            TreeSelectStatusDelegate statusCallback, TreeSelectDoneDelegate doneCallback)
        {
            m_treeNode = node;
            m_dictNodes = dictNodes;
            m_dictDriveInfo = dictDriveInfo;
            m_strFile = strFile;
            m_bCompareMode = bCompareMode;
            m_bSecondComparePane = bSecondComparePane;
            m_statusCallback = statusCallback;
            m_doneCallback = doneCallback;
        }

        internal static List<string[]> GetFileList(TreeNode parent, List<ulong> listLength = null)
        {
            string strFile = ((RootNodeDatum)parent.Root().Tag).StrFile;

            if ((parent.Tag is NodeDatum) == false)
            {
                return null;
            }

            NodeDatum nodeDatum = (NodeDatum)parent.Tag;

            if (nodeDatum.nLineNo == 0)
            {
                return null;
            }

            long nPrevDir = nodeDatum.nPrevLineNo;
            long nLineNo = nodeDatum.nLineNo;

            if (nPrevDir == 0)
            {
                return null;
            }

            if ((nLineNo - nPrevDir) <= 1)  // dir has no files
            {
                return null;
            }

            DateTime dtStart = DateTime.Now;
            List<string> listLines = File.ReadLines(strFile)
                .Skip((int)nPrevDir)
                .Take((int)(nLineNo - nPrevDir - 1))
                .ToList();

            if (listLines.IsEmpty())
            {
                return null;
            }

            List<string[]> listFiles = new List<string[]>();
            ulong nLengthDebug = 0;

            foreach (string strFileLine in listLines)
            {
                string[] strArrayFiles = strFileLine.Split('\t').Skip(3).ToArray();
                ulong nLength = 0;

                strArrayFiles[3] = DecodeAttributes(strArrayFiles[3]);

                if ((strArrayFiles.Length > knColLengthLV) && (false == string.IsNullOrWhiteSpace(strArrayFiles[knColLengthLV])))
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
            m_thread = new Thread(Go);
            m_thread.IsBackground = true;
            m_thread.Start();
            return m_thread;
        }

        void Go()
        {
            Go_A();

            if (m_bCompareMode == false)
            {
                // Volume detail

                if (m_dictDriveInfo.ContainsKey(m_strFile))
                {
                    string[] arrDriveInfo = m_dictDriveInfo[m_strFile].Split(new string[] { "\r\n", "\n" }, StringSplitOptions.None);

                    MBoxStatic.Assert(1301.2314, new int[] { 7, 8, 10, kanDIviewOrder.Length }.Contains(arrDriveInfo.Length));

                    string[][] asItems = new string[arrDriveInfo.Length][];

                    for (int i = 0; i < arrDriveInfo.Length; ++i)
                    {
                        string[] a = arrDriveInfo[i].Split('\t');

                        if (a[1].Trim().Length == 0)
                        {
                            continue;
                        }

                        asItems[i] = new string[]
                        {
                            a[0],
                            kabDIsizeType[i] ? FormatSize(a[1], bBytes: true) : a[1]
                        };
                    }

                    ListViewItem[] lvItems = new ListViewItem[arrDriveInfo.Length];

                    for (int ix = 0; ix < arrDriveInfo.Length; ++ix)
                    {
                        if ((asItems[ix] == null) || (asItems[ix].Length == 0) || (asItems[ix][1].Trim().Length == 0))
                        {
                            continue;
                        }

                        if ((kanDIoptIfEqTo[ix] != -1) && (asItems[ix][1] == asItems[kanDIoptIfEqTo[ix]][1]))
                        {
                            continue;
                        }

                        int ixA = (arrDriveInfo.Length == kanDIviewOrder.Length) ? kanDIviewOrder[ix] : ix;

                        lvItems[ixA] = new ListViewItem(asItems[ix]);
                    }

                    m_statusCallback(lvVolDetails: lvItems.Where(i => i != null).ToArray());
                }
            }

            m_doneCallback(m_bSecondComparePane);
        }

        void Go_A()
        {
            if (File.Exists(m_strFile) == false)
            {
                MBoxStatic.Assert(1301.2311, false);
                return;
            }

            if ((m_treeNode.Tag is NodeDatum) == false)
            {
                return;
            }

            var nodeDatum = (NodeDatum)m_treeNode.Tag;

            if (nodeDatum.nLineNo == 0)
            {
                return;
            }

            var nPrevDir = nodeDatum.nPrevLineNo;
            var nLineNo = nodeDatum.nLineNo;
            string[] strArray = null;

            File
                .ReadLines(m_strFile)
                .Skip((int)nLineNo - 1)
                .Take(1)
                .FirstOnlyAssert(strLine =>
                {
                    strArray = strLine.Split('\t');
                    UtilProject.WriteLine(strLine);
                });

            MBoxStatic.Assert(1301.2312, (false == string.IsNullOrWhiteSpace(strArray[2])));

            long nIx = 0;
            DateTime dt;

            // Directory detail

            UList<ListViewItem> listItems = new UList<ListViewItem>();

            nIx = 4; if ((strArray.Length > nIx) && (false == string.IsNullOrWhiteSpace(strArray[nIx]))) listItems.Add(new ListViewItem(new string[] { "Created\t", (dt = DateTime.Parse(strArray[nIx])).ToLongDateString() + ", " + dt.ToLongTimeString() }));
            nIx = 5; if ((strArray.Length > nIx) && (false == string.IsNullOrWhiteSpace(strArray[nIx]))) listItems.Add(new ListViewItem(new string[] { "Modified\t", (dt = DateTime.Parse(strArray[nIx])).ToLongDateString() + ", " + dt.ToLongTimeString() }));
            nIx = 6; if ((strArray.Length > nIx) && (false == string.IsNullOrWhiteSpace(strArray[nIx]))) listItems.Add(new ListViewItem(new string[] { "Attributes\t", DecodeAttributes(strArray[nIx]) }));
            listItems.Add(new ListViewItem(new string[] { "Immediate Size\t", FormatSize(nodeDatum.nLength, bBytes: true) }));
            nIx = 8; if ((strArray.Length > nIx) && (false == string.IsNullOrWhiteSpace(strArray[nIx]))) listItems.Add(new ListViewItem(new string[] { "Error 1\t", strArray[nIx] }));
            nIx = 9; if ((strArray.Length > nIx) && (false == string.IsNullOrWhiteSpace(strArray[nIx]))) listItems.Add(new ListViewItem(new string[] { "Error 2\t", strArray[nIx] }));
            listItems.Add(new ListViewItem(new string[] { "# Immediate Files", (nLineNo - nPrevDir - 1).ToString() }));

            // Tree subnode detail

            string NUMFMT = "###,###,###,##0";

            listItems.Add(new ListViewItem(new string[] { "# Immediate Folders", m_treeNode.Nodes.Count.ToString(NUMFMT) }));
            listItems.Add(new ListViewItem(new string[] { "Total # Files", nodeDatum.nFilesInSubdirs.ToString(NUMFMT) }));

            if (nodeDatum.nSubDirs > 0)
            {
                string strItem = nodeDatum.nSubDirs.ToString(NUMFMT);

                if (nodeDatum.nDirsWithFiles > 0)
                {
                    long nDirsWithFiles = nodeDatum.nDirsWithFiles;

                    if (nodeDatum.nImmediateFiles > 0)
                    {
                        --nDirsWithFiles;
                    }

                    if (nDirsWithFiles > 0)
                    {
                        strItem += " (" + nDirsWithFiles.ToString(NUMFMT) + " with files)";
                    }
                }

                listItems.Add(new ListViewItem(new string[] { "# Subfolders", strItem }));
            }

            listItems.Add(new ListViewItem(new string[] { "Total Size", FormatSize(nodeDatum.nTotalLength, bBytes: true) }));
            m_statusCallback(lvItemDetails: listItems.ToArray(), bSecondComparePane: m_bSecondComparePane);

            List<string[]> listFiles_A = GetFileList(m_treeNode);

            if (listFiles_A != null)
            {
                UList<ListViewItem> listFiles = new UList<ListViewItem>();

                foreach (string[] arrLine in listFiles_A)
                {
                    listFiles.Add(new ListViewItem(arrLine));
                }

                m_statusCallback(itemArray: listFiles.ToArray(), bSecondComparePane: m_bSecondComparePane, lvFileItem: new LVitemFileTag(m_treeNode.Text, listFiles.Count));
            }
        }

        readonly TreeNode m_treeNode = null;
        readonly SortedDictionary<FolderKeyStruct, UList<TreeNode>> m_dictNodes = null;
        readonly Dictionary<string, string> m_dictDriveInfo = null;
        readonly TreeSelectStatusDelegate m_statusCallback = null;
        readonly TreeSelectDoneDelegate m_doneCallback = null;
        Thread m_thread = null;
        readonly string m_strFile = null;
        readonly bool m_bCompareMode = false;
        readonly bool m_bSecondComparePane = false;
    }
}
