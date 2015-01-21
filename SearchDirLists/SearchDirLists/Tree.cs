using System.Windows.Forms;
using System.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;

namespace SearchDirLists
{
    delegate void TreeStatusDelegate(LVvolStrings volStrings, TreeNode rootNode = null, bool bError = false);
    delegate void TreeSelectStatusDelegate(SDL_ListViewItem[] lvItemDetails = null, SDL_ListViewItem[] itemArray = null, SDL_ListViewItem[] lvVolDetails = null, bool bSecondComparePane = false, LVitemFileTag lvFileItem = null);
    delegate void TreeSelectDoneDelegate(bool bSecondComparePane);

    class DetailsDatum
    {
        internal ulong nTotalLength = 0;
        internal uint nFilesInSubdirs = 0;
        internal uint nSubDirs = 0;
        internal uint nImmediateFiles = 0;
        internal uint nDirsWithFiles = 0;

        internal DetailsDatum() {}
        internal DetailsDatum(DetailsDatum in_datum)
        {
            nTotalLength = in_datum.nTotalLength;
            nFilesInSubdirs = in_datum.nFilesInSubdirs;
            nSubDirs = in_datum.nSubDirs;
            nImmediateFiles = in_datum.nImmediateFiles;
            nDirsWithFiles = in_datum.nDirsWithFiles;
        }

        static public DetailsDatum operator +(DetailsDatum in_datum1, DetailsDatum in_datum2)
        {
            DetailsDatum datum = new DetailsDatum();

            datum.nTotalLength = in_datum1.nTotalLength + in_datum2.nTotalLength;
            datum.nFilesInSubdirs = in_datum1.nFilesInSubdirs + in_datum2.nFilesInSubdirs;
            datum.nSubDirs = in_datum1.nSubDirs + in_datum2.nSubDirs;
            datum.nImmediateFiles = in_datum1.nImmediateFiles + in_datum2.nImmediateFiles;
            datum.nDirsWithFiles = in_datum1.nDirsWithFiles + in_datum2.nDirsWithFiles;
            return datum;
        }
    }

    // One tag at the first item, so the compare listviewer knows what the first listviewer's state is.
    class LVitemFileTag
    {
        internal readonly string StrCompareDir = null;
        internal readonly long nNumFiles = 0;   // equivalent to number of items in the listviewer. Not currently used

        internal LVitemFileTag(string strCompareDir_in, long nNumFiles_in)
        {
            StrCompareDir = strCompareDir_in;
            nNumFiles = nNumFiles_in;
        }
    }

    class Correlate : IComparable
    {
        internal readonly ulong nTotalLength;       //  found   41 bits
        internal readonly uint nFilesInSubdirs;     //          23 bits
        internal readonly uint nDirsWithFiles;      //          16 bits

        internal Correlate(ulong nTotalLength_in, uint nFilesInSubdirs_in, uint nDirsWithFiles_in)
        {
            nTotalLength = nTotalLength_in;
            nFilesInSubdirs = nFilesInSubdirs_in;
            nDirsWithFiles = nDirsWithFiles_in;
        }

        public int CompareTo(object obj)
        {
            Correlate that = (Correlate) obj;

            if (this > that) return -1;             // reverse sort
            if (this == that) return 0;
            return 1;
        }

        public override bool Equals(object obj)
        {
            if ((obj is Correlate) == false) return false;
            return (((Correlate)obj) == this);
        }

        public override int GetHashCode()
        {
            // any overflow mixes the bits a bit better.
            int result = 37;    // prime

            result *= 397;      // also prime
            result += nTotalLength.GetHashCode();
            result *= 397;
            result += nFilesInSubdirs.GetHashCode();
            result *= 397;
            result += nDirsWithFiles.GetHashCode();
            return result;
        }

        public override string ToString()
        {
            return "nTotalLength: " + nTotalLength + "\n" +
                "nFilesInSubdirs: " + nFilesInSubdirs + "\n" +
                "nDirsWithFiles: " + nDirsWithFiles + "\n";
        }

        public static bool operator ==(Correlate x, Correlate y)
        {
            return (x.nTotalLength == y.nTotalLength) && (x.nFilesInSubdirs == y.nFilesInSubdirs) && (x.nDirsWithFiles == y.nDirsWithFiles);
        }

        public static bool operator >(Correlate x, Correlate y)
        {
            if (x.nTotalLength < y.nTotalLength) return false;
            if (x.nTotalLength > y.nTotalLength) return true;
            if (x.nFilesInSubdirs < y.nFilesInSubdirs) return false;
            if (x.nFilesInSubdirs > y.nFilesInSubdirs) return true;
            if (x.nDirsWithFiles < y.nDirsWithFiles) return false;
            if (x.nDirsWithFiles > y.nDirsWithFiles) return true;
            return false;
        }

        public static bool operator !=(Correlate x, Correlate y) { return ((x == y) == false); }
        public static bool operator <(Correlate x, Correlate y) { return ((x >= y) == false); }
        public static bool operator >=(Correlate x, Correlate y) { return ((x > y) || (x == y)); }
        public static bool operator <=(Correlate x, Correlate y) { return ((x > y) == false); } 
    }                              

    class NodeDatum : DetailsDatum
    {
        internal readonly uint nPrevLineNo = 0;
        internal readonly uint nLineNo = 0;
        internal readonly ulong nLength = 0;

        class NodeDatumLVitemHolder     // this was a way of setting the listview item in a different node after processing the first. Not used.
        {
            internal SDL_ListViewItem m_lvItem = null;
        }

        internal Correlate Key
        {
            get
            {
                return new Correlate((ulong)nTotalLength, nFilesInSubdirs, nDirsWithFiles);
            }
        }

        internal UList<TreeNode> m_listClones = new UList<TreeNode>();

        internal void SetLVitemHolder(NodeDatum holder) { m_lvItem_ = (holder != null) ? holder.m_lvItem_ : null; }
        NodeDatumLVitemHolder m_lvItem_ = new NodeDatumLVitemHolder();
        internal SDL_ListViewItem m_lvItem
        {
            get { return (m_lvItem_ != null) ? m_lvItem_.m_lvItem : null; }
            set { if (m_lvItem_ != null) m_lvItem_.m_lvItem = value; }
        }

        internal bool m_bDifferentVols = false;

        internal NodeDatum() {}
        internal NodeDatum(uint nPrevLineNo_in, uint nLineNo_in, ulong nLength_in)
        { nPrevLineNo = nPrevLineNo_in; nLineNo = nLineNo_in; nLength = nLength_in; }

        protected NodeDatum(NodeDatum node)
            : base(node)
        {
            nPrevLineNo = node.nPrevLineNo;
            nLineNo = node.nLineNo;
            nLength = node.nLength;
        }

        internal Rectangle TreeMapRect = Rectangle.Empty;
        internal TreeNode TreeMapFiles = null;
    }

    class RootNodeDatum : NodeDatum
    {
        internal string StrVolumeGroup = null;
        internal bool VolumeView = true;

        internal readonly string StrFile = null;
        internal readonly ulong VolumeFree = 0;
        internal readonly ulong VolumeLength = 0;

        internal RootNodeDatum(NodeDatum node, string strFile_in, string strVolGroup_in,
            ulong nVolumeFree_in, ulong nVolumeLength_in)
            : base(node)
        {
            StrFile = strFile_in;
            StrVolumeGroup = strVolGroup_in;
            VolumeLength = nVolumeLength_in;
            VolumeFree = nVolumeFree_in;
        }

        internal RootNodeDatum(NodeDatum node, RootNodeDatum rootNode)
            : base(node)
        {
            StrFile = rootNode.StrFile;
            StrVolumeGroup = rootNode.StrVolumeGroup;
            VolumeLength = rootNode.VolumeLength;
            VolumeFree = rootNode.VolumeFree;
        }
    }

    class LVitemNameComparer : IEqualityComparer<SDL_ListViewItem>
    {
        public bool Equals(SDL_ListViewItem x, SDL_ListViewItem y)
        {
            return (x != null) && (y != null) && (false == string.IsNullOrWhiteSpace(x.Name)) && x.Name.Equals(y.Name);
        }

        public int GetHashCode(SDL_ListViewItem obj)
        {
            return obj.Name == null ? 0 : obj.Name.GetHashCode();
        }

        internal static void NameItems(ListView.ListViewItemCollection list)
        {
            foreach (SDL_ListViewItem item in list)
            {
                item.Name = item.Text;

                if (item.SubItems.Count > Utilities.mNcolLengthLV)
                {
                    item.Name += item.SubItems[Utilities.mNcolLengthLV].Text;         // name + size
                }
            }
        }

        internal static void MarkItemsFrom1notIn2(SDL_ListView lv1, SDL_ListView lv2)
        {
            if ((lv1.Items.Count <= 0) || (lv2.Items.Count <= 0)) { return; }

            List<SDL_ListViewItem> list = lv1.Items.Cast<SDL_ListViewItem>().Except(lv2.Items.Cast<SDL_ListViewItem>(), new LVitemNameComparer()).ToList();

            if (list.Count > 0)
            {
                lv1.TopItem = list[0];
            }

            foreach (SDL_ListViewItem item in list)
            {
                item.ForeColor = Color.Red;
            }
        }

        internal static void SetTopItem(SDL_ListView lv1, SDL_ListView lv2)
        {
            if (lv1.TopItem == null) { return; }
            if (lv1.TopItem.Index > 0) { return; }
            if (lv2.TopItem == null) { return; }

            int nIx = lv2.TopItem.Index - Math.Abs(lv2.Items.Count - lv1.Items.Count);

            if (nIx < 0)
            {
                return;
            }

            if (lv1.Items.Count > nIx)
            {
                lv1.TopItem = lv1.Items[nIx];
            }
        }
    }

    class TreeBase : Utilities
    {
        protected readonly SortedDictionary<Correlate, UList<TreeNode>> m_dictNodes = null;
        protected readonly Dictionary<string, string> m_dictDriveInfo = null;
        protected static TreeStatusDelegate m_statusCallback = null;

        internal TreeBase(SortedDictionary<Correlate, UList<TreeNode>> dictNodes, Dictionary<string, string> dictDriveInfo,
            TreeStatusDelegate statusCallback)
        {
            m_dictNodes = dictNodes;
            m_dictDriveInfo = dictDriveInfo;
            m_statusCallback = statusCallback;
        }

        internal TreeBase(TreeBase base_in)
        {
            m_dictNodes = base_in.m_dictNodes;
            m_dictDriveInfo = base_in.m_dictDriveInfo;
            Utilities.Assert(1301.2301, m_statusCallback != null);
        }
    }

    class Tree : TreeBase
    {
        readonly UList<LVvolStrings> m_list_lvVolStrings = null;
        readonly Action m_doneCallback = null;
        ConcurrentBag<TreeRootNodeBuilder> m_cbagWorkers = new ConcurrentBag<TreeRootNodeBuilder>();
        Thread m_thread = null;
        bool m_bThreadAbort = false;

        class TreeRootNodeBuilder : TreeBase
        {
            Thread m_thread = null;
            bool m_bThreadAbort = false;
            LVvolStrings m_volStrings = null;

            class DirData
            {
                RootNode m_rootNode = null;

                internal DirData(RootNode rootNode)
                {
                    m_rootNode = rootNode;
                }

                internal void AddToTree(string str_in, uint nLineNo, ulong nLength)
                {
                    if (m_rootNode.Nodes.ContainsKey(str_in))
                    {
                        Node node = m_rootNode.Nodes[str_in];

                        Utilities.Assert(1301.2302, false);
                    }

                    string str = str_in.TrimEnd('\\');

                    m_rootNode.Nodes.Add(str, new Node(str, nLineNo, nLength, m_rootNode));
                }

                internal TreeNode AddToTree(string strVolumeName)
                {
                    var nodes = m_rootNode.Nodes.Values;

                    if (nodes.Count <= 0)
                    {
                        return null;
                    }

                    return m_rootNode.Nodes.Values.First().AddToTree(strVolumeName);
                }
            }

            class Node : Utilities
            {
                readonly RootNode m_rootNode = null;
                readonly SortedDictionary<string, Node> subNodes = new SortedDictionary<string, Node>();
                readonly string m_strPath = null;
                uint m_nPrevLineNo = 0;
                uint m_nLineNo = 0;
                ulong m_nLength = 0;
                bool bUseShortPath = true;

                internal Node(string in_str, uint nLineNo, ulong nLength, RootNode rootNode)
                {
                    if (GlobalData.AppExit)
                    {
                        return;
                    }

                    Utilities.Assert(1301.2303, nLineNo != 0);
                    m_rootNode = rootNode;

                    if (in_str.EndsWith(@":\") == false)
                    {
                        Utilities.Assert(1301.2304, in_str.Trim().EndsWith(@"\") == false);
                    }

                    m_strPath = in_str;
                    m_nPrevLineNo = m_rootNode.FirstLineNo;
                    m_rootNode.FirstLineNo = m_nLineNo = nLineNo;
                    m_nLength = nLength;

                    // Path.GetDirectoryName() does not preserve filesystem root

                    string strParent = m_strPath;
                    int nIndex = strParent.LastIndexOf('\\');

                    if (nIndex < 0)
                    {
                        return;
                    }

                    strParent = strParent.Remove(nIndex).TrimEnd('\\');

                    if (m_rootNode.Nodes.ContainsKey(strParent) == false)
                    {
                        m_rootNode.Nodes.Add(strParent, new Node(strParent, m_rootNode.FirstLineNo, 0, m_rootNode));
                    }

                    if (m_rootNode.Nodes[strParent].subNodes.ContainsKey(m_strPath) == false)
                    {
                        m_rootNode.Nodes[strParent].subNodes.Add(m_strPath, this);
                    }
                }

                internal TreeNode AddToTree(string strVolumeName = null)
                {
                    if (GlobalData.AppExit)
                    {
                        return new TreeNode();
                    }

                    int nIndex = m_strPath.LastIndexOf('\\');
                    string strShortPath = bUseShortPath ? m_strPath.Substring(nIndex + 1) : m_strPath;
                    TreeNode treeNode = null;

                    if (subNodes.Count == 1)
                    {
                        Node subNode = subNodes.Values.First();

                        if (this == m_rootNode.Nodes.Values.First())
                        {
                            // cull all root node single-chains.
                            m_rootNode.Nodes = subNodes;
                            subNode.m_strPath.Insert(0, m_strPath + '\\');
                            subNode.bUseShortPath = false;
                            treeNode = subNode.AddToTree(strVolumeName);

                            // further down at new NodeDatum...
                            m_nPrevLineNo = subNode.m_nPrevLineNo;
                            m_nLength = subNode.m_nLength;
                            m_nLineNo = subNode.m_nLineNo;
                        }
                        else
                        {
                            treeNode = new TreeNode(strShortPath, new TreeNode[] { subNode.AddToTree() });
                        }
                    }
                    else if (subNodes.Count > 1)
                    {
                        UList<TreeNode> treeList = new UList<TreeNode>();

                        foreach (Node node in subNodes.Values)
                        {
                            treeList.Add(node.AddToTree());
                        }

                        treeNode = new TreeNode(strShortPath, treeList.ToArray());
                    }
                    else
                    {
                        treeNode = new TreeNode(strShortPath);
                    }

                    //Utilities.Assert(1301.2305, treeNode.Text == strShortPath, "\"" + treeNode.Text + "\" != \"" + strShortPath + "\""); not true for non-root
                    Utilities.Assert(1301.2306, treeNode.SelectedImageIndex == -1);     // sets the bitmap size
                    treeNode.SelectedImageIndex = -1;
                    treeNode.Tag = new NodeDatum(m_nPrevLineNo, m_nLineNo, m_nLength);  // this is almost but not quite always newly assigned here.

                    if (this == m_rootNode.Nodes.Values.First())
                    {
                        treeNode.Name = treeNode.Text;

                        if (false == string.IsNullOrWhiteSpace(strVolumeName))
                        {
                            if (strVolumeName.EndsWith(treeNode.Text))
                            {
                                treeNode.Text = strVolumeName;
                            }
                            else
                            {
                                treeNode.Text = strVolumeName + " (" + treeNode.Text + ")";
                            }
                        }
                    }

                    return treeNode;
                }
            }

            class RootNode
            {
                internal SortedDictionary<string, Node> Nodes = new SortedDictionary<string, Node>();
                internal uint FirstLineNo = 0;
            }

            internal TreeRootNodeBuilder(LVvolStrings volStrings, TreeBase base_in) : base(base_in)
            {
                m_volStrings = volStrings;
                Utilities.Assert(1301.2301, m_statusCallback != null);
            }

            DetailsDatum TreeSubnodeDetails(TreeNode treeNode)
            {
                DetailsDatum datum = new DetailsDatum();

                foreach (TreeNode node in treeNode.Nodes)
                {
                    if (m_bThreadAbort || GlobalData.AppExit)
                    {
                        return datum;
                    }

                    datum += TreeSubnodeDetails(node);
                }

                if ((treeNode.Tag is NodeDatum) == false)
                {
                    return datum;
                }

                NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;

                if (nodeDatum.nLineNo <= 0)
                {
                    return datum;
                }

                nodeDatum.nTotalLength = (datum.nTotalLength += nodeDatum.nLength);
                nodeDatum.nImmediateFiles = (nodeDatum.nLineNo - nodeDatum.nPrevLineNo - 1);
                nodeDatum.nFilesInSubdirs = (datum.nFilesInSubdirs += nodeDatum.nImmediateFiles);
                nodeDatum.nSubDirs = (datum.nSubDirs += (uint)treeNode.Nodes.Count);

                if (nodeDatum.nImmediateFiles > 0)
                {
                    ++datum.nDirsWithFiles;
                }

                nodeDatum.nDirsWithFiles = datum.nDirsWithFiles;

                Correlate nKey = nodeDatum.Key;

                lock (m_dictNodes)
                {
                    if (m_dictNodes.ContainsKey(nKey))
                    {
                        m_dictNodes[nKey].Add(treeNode);
                    }
                    else if (nodeDatum.nTotalLength > 100 * 1024)
                    {
                        UList<TreeNode> listNodes = new UList<TreeNode>();

                        listNodes.Add(treeNode);
                        m_dictNodes.Add(nKey, listNodes);
                    }
                }

                return datum;
            }

            internal void Go()
            {
                DateTime dtStart = DateTime.Now;

                if (m_volStrings.CanLoad == false)
                {
                    Utilities.Assert(1301.2307, false);    // guaranteed by caller
                    return;
                }

                string strVolumeName = m_volStrings.VolumeName;
                string strPath = m_volStrings.StrPath;
                string strSaveAs = m_volStrings.SaveAs;

                if (FormatPath(ref strPath, ref strSaveAs, false) == false)
                {
                    return;
                }

                {
                    bool bValid = false;
                    bool bAttemptConvert = false;

                    while (true)
                    {
                        bValid = ValidateFile(strSaveAs);

                        if (bValid || bAttemptConvert)
                        {
                            break;
                        }

                        if (File.Exists(StrFile_01(strSaveAs)) == false)
                        {
                            break;
                        }

                        try
                        {
                            File.Delete(StrFile_01(strSaveAs));
                        }
                        catch { }

                        bAttemptConvert = true;
                    }

                    if (bValid == false)
                    {
                        MBox("Bad file: " + strSaveAs, "Tree");
                        m_statusCallback(m_volStrings, bError: true);
                        return;
                    }
                }

                ulong nVolFree = 0;
                ulong nVolLength = 0;

                {
                    string[] arrDriveInfo = File.ReadLines(strSaveAs).Where(s => s.StartsWith(mSTRlineType_DriveInfo)).ToArray();
                    StringBuilder strBuilder = new StringBuilder();
                    int nIx = -1;

                    foreach (string strLine in arrDriveInfo)
                    {
                        string[] strArray = strLine.Split('\t');
                        ++nIx;

                        if (strArray.Length > 3)
                        {
                            strBuilder.Append(strArray[2]);
                        }
                        else if (strArray.Length > 2)
                        {
                            strBuilder.Append(Utilities.mAstrDIlabels[nIx]);
                        }
                        else
                        {
                            continue;
                        }

                        string s = strArray[strArray.Length - 1];
                        strBuilder.AppendLine('\t' + s);

                        if ((nIx == 5) && (false == string.IsNullOrWhiteSpace(s)))
                        {
                            nVolFree = ulong.Parse(s);
                        }
                        else if ((nIx == 6) && (false == string.IsNullOrWhiteSpace(s)))
                        {
                            nVolLength = ulong.Parse(s);
                        }
                    }

                    lock (m_dictDriveInfo)
                    {
                        if (m_dictDriveInfo.ContainsKey(strSaveAs))
                        {
                            Utilities.Assert(1301.2308, false);
                            m_dictDriveInfo.Remove(strSaveAs);
                        }

                        m_dictDriveInfo.Add(strSaveAs, strBuilder.ToString().Trim(new char[] { '\r', '\n' }));
                    }
                }

                DirData dirData = null;

                {
                    RootNode rootNode = new RootNode();
                    string strStart = File.ReadLines(strSaveAs).Where(s => s.StartsWith(mSTRlineType_Start)).ToArray()[0];

                    rootNode.FirstLineNo = uint.Parse(strStart.Split('\t')[1]);
                    dirData = new DirData(rootNode);
                }

                bool bZeroLengthsWritten = true;
                List<string> listLines = File.ReadLines(strSaveAs).Where(s => s.StartsWith(mSTRlineType_Directory)).ToList();

                foreach (string strLine in listLines)
                {
                    if (GlobalData.AppExit)
                    {
                        return;
                    }

                    string[] strArray = strLine.Split('\t');
                    uint nLineNo = uint.Parse(strArray[1]);
                    int nIx = mNcolLength;
                    ulong nLength = 0;

                    if ((strArray.Length > nIx) && (false == string.IsNullOrWhiteSpace(strArray[nIx])))
                    {
                        nLength = ulong.Parse(strArray[nIx]);
                    }
                    else
                    {
                        bZeroLengthsWritten = false;     // files created before 140509 Fri drop zeroes from the end of the line
                    }

                    string strDir = strArray[2];

                    dirData.AddToTree(strDir, nLineNo, nLength);
                }

                TreeNode rootTreeNode = dirData.AddToTree(strVolumeName);

                if (rootTreeNode != null)
                {
                    rootTreeNode.Tag = new RootNodeDatum((NodeDatum)rootTreeNode.Tag, strSaveAs, m_volStrings.VolumeGroup, nVolFree, nVolLength);
                    TreeSubnodeDetails(rootTreeNode);
                }

                m_statusCallback(m_volStrings, rootTreeNode);

                if (bZeroLengthsWritten)
                {
#if (DEBUG)
                    Utilities.WriteLine(File.ReadLines(strSaveAs).Where(s => s.StartsWith(mSTRlineType_File)).Sum(s => decimal.Parse(s.Split('\t')[mNcolLength])).ToString());
                    Utilities.WriteLine(File.ReadLines(strSaveAs).Where(s => s.StartsWith(mSTRlineType_Directory)).Sum(s => decimal.Parse(s.Split('\t')[mNcolLength])).ToString());
#endif
                }

                ulong nScannedLength = ulong.Parse(
                    File.ReadLines(strSaveAs).Where(s => s.StartsWith(mSTRlineType_Length)).ToArray()[0]
                    .Split('\t')[mNcolLength]);

                Utilities.WriteLine(nScannedLength.ToString());

                ulong nTotalLength = 0;

                if (rootTreeNode != null)
                {
                    nTotalLength = ((RootNodeDatum)rootTreeNode.Tag).nTotalLength;
                }

                if (nScannedLength != nTotalLength)
                {
                    Utilities.WriteLine(nTotalLength.ToString());
                    Utilities.Assert(1301.23101, false, "nScannedLength != nTotalLength\n" + strSaveAs, bTraceOnly:true);
                }

                Utilities.WriteLine(strSaveAs + " tree took " + (DateTime.Now - dtStart).TotalMilliseconds / 1000.0 + " seconds.");
            }

            internal TreeRootNodeBuilder DoThreadFactory()
            {
                m_thread = new Thread(new ThreadStart(Go));
                m_thread.IsBackground = true;
                m_thread.Start();
                return this;
            }

            internal void Join()
            {
                m_thread.Join();
            }

            internal void Abort()
            {
                m_bThreadAbort = true;
                m_thread.Abort();
            }
        }

        internal Tree(UList<LVvolStrings> listLVvolStrings,
            SortedDictionary<Correlate, UList<TreeNode>> dictNodes, Dictionary<string, string> dictDriveInfo,
            TreeStatusDelegate statusCallback, Action doneCallback)
            : base(dictNodes, dictDriveInfo, statusCallback)
        {
            m_list_lvVolStrings = listLVvolStrings;
            m_doneCallback = doneCallback;
            Utilities.Assert(1301.2301, m_statusCallback != null);
        }

        void Go()
        {
            Utilities.WriteLine();
            Utilities.WriteLine("Creating tree.");

            DateTime dtStart = DateTime.Now;

            foreach (LVvolStrings volStrings in m_list_lvVolStrings)
            {
                if (volStrings.CanLoad == false)
                {
                    continue;
                }

                TreeRootNodeBuilder treeRoot = new TreeRootNodeBuilder(volStrings, this);

                m_cbagWorkers.Add(treeRoot.DoThreadFactory());
            }

            foreach (TreeRootNodeBuilder worker in m_cbagWorkers)
            {
                worker.Join();
            }

            Utilities.WriteLine(string.Format("Completed tree in {0} seconds.", ((int)(DateTime.Now - dtStart).TotalMilliseconds / 10) / 100.0));

            if (m_bThreadAbort || GlobalData.AppExit)
            {
                return;
            }

            m_doneCallback();
        }

        internal void EndThread(bool bJoin = false)     // bJoin is not used because it induces lag.
        {
            m_bThreadAbort = true;

            if (m_thread != null)
            {
                m_thread.Abort();
                m_thread = null;
            }

            foreach (TreeRootNodeBuilder worker in m_cbagWorkers)
            {
                worker.Abort();
            }

            m_cbagWorkers = new ConcurrentBag<TreeRootNodeBuilder>();
            Collate.Abort();
            m_dictNodes.Clear();
        }

        internal void DoThreadFactory()
        {
            m_thread = new Thread(new ThreadStart(Go));
            m_thread.IsBackground = true;
            m_thread.Start();
        }

        internal bool IsAborted { get { return m_bThreadAbort; } }
    }

    class TreeSelect : Utilities
    {
        readonly TreeNode m_treeNode = null;
        readonly SortedDictionary<Correlate, UList<TreeNode>> m_dictNodes = null;
        readonly Dictionary<string, string> m_dictDriveInfo = null;
        readonly TreeSelectStatusDelegate m_statusCallback = null;
        readonly TreeSelectDoneDelegate m_doneCallback = null;
        Thread m_thread = null;
        readonly string m_strFile = null;
        readonly bool m_bCompareMode = false;
        readonly bool m_bSecondComparePane = false;

        internal TreeSelect(TreeNode node, SortedDictionary<Correlate, UList<TreeNode>> dictNodes, Dictionary<string, string> dictDriveInfo,
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

        void Go_A()
        {
            if (File.Exists(m_strFile) == false)
            {
                Utilities.Assert(1301.2311, false);
                return;
            }

            if ((m_treeNode.Tag is NodeDatum) == false)
            {
                return;
            }

            NodeDatum nodeDatum = (NodeDatum)m_treeNode.Tag;

            if (nodeDatum.nLineNo <= 0)
            {
                return;
            }

            long nPrevDir = nodeDatum.nPrevLineNo;
            long nLineNo = nodeDatum.nLineNo;
            string strLine = File.ReadLines(m_strFile).Skip((int)nLineNo - 1).Take(1).ToArray()[0];
            string[] strArray = strLine.Split('\t');

            Utilities.Assert(1301.2312, (false == string.IsNullOrWhiteSpace(strArray[2])));

            long nIx = 0;
            DateTime dt;

            // Directory detail

            UList<SDL_ListViewItem> listItems = new UList<SDL_ListViewItem>();

            nIx = 4; if ((strArray.Length > nIx) && (false == string.IsNullOrWhiteSpace(strArray[nIx]))) listItems.Add(new SDL_ListViewItem(new string[]{ "Created\t", (dt = DateTime.Parse(strArray[nIx])).ToLongDateString() + ", " + dt.ToLongTimeString() }));
            nIx = 5; if ((strArray.Length > nIx) && (false == string.IsNullOrWhiteSpace(strArray[nIx]))) listItems.Add(new SDL_ListViewItem(new string[] { "Modified\t", (dt = DateTime.Parse(strArray[nIx])).ToLongDateString() + ", " + dt.ToLongTimeString() }));
            nIx = 6; if ((strArray.Length > nIx) && (false == string.IsNullOrWhiteSpace(strArray[nIx]))) listItems.Add(new SDL_ListViewItem(new string[] { "Attributes\t", DecodeAttributes(strArray[nIx]) }));
            listItems.Add(new SDL_ListViewItem(new string[] { "Immediate Size\t", FormatSize(nodeDatum.nLength, bBytes: true) }));
            nIx = 8; if ((strArray.Length > nIx) && (false == string.IsNullOrWhiteSpace(strArray[nIx]))) listItems.Add(new SDL_ListViewItem(new string[] { "Error 1\t", strArray[nIx] }));
            nIx = 9; if ((strArray.Length > nIx) && (false == string.IsNullOrWhiteSpace(strArray[nIx]))) listItems.Add(new SDL_ListViewItem(new string[] { "Error 2\t", strArray[nIx] }));
            listItems.Add(new SDL_ListViewItem(new string[] { "# Immediate Files", (nLineNo - nPrevDir - 1).ToString() }));

            // Tree subnode detail

            string NUMFMT = "###,###,###,##0";

            listItems.Add(new SDL_ListViewItem(new string[] { "# Immediate Folders", m_treeNode.Nodes.Count.ToString(NUMFMT) }));
            listItems.Add(new SDL_ListViewItem(new string[] { "Total # Files", nodeDatum.nFilesInSubdirs.ToString(NUMFMT) }));

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

                listItems.Add(new SDL_ListViewItem(new string[] { "# Subfolders", strItem }));
            }

            listItems.Add(new SDL_ListViewItem(new string[] { "Total Size", FormatSize(nodeDatum.nTotalLength, bBytes: true) }));
            m_statusCallback(lvItemDetails: listItems.ToArray(), bSecondComparePane: m_bSecondComparePane);
            Utilities.WriteLine(strLine);

            List<string[]> listFiles_A = GetFileList(m_treeNode);

            if (listFiles_A != null)
            {
                UList<SDL_ListViewItem> listFiles = new UList<SDL_ListViewItem>();

                foreach (string[] arrLine in listFiles_A)
                {
                    listFiles.Add(new SDL_ListViewItem(arrLine));
                }

                m_statusCallback(itemArray: listFiles.ToArray(), bSecondComparePane: m_bSecondComparePane, lvFileItem: new LVitemFileTag(m_treeNode.Text, listFiles.Count));
            }
        }

        internal static List<string[]> GetFileList(TreeNode parent, List<ulong> listLength = null)
        {
            string strFile = ((RootNodeDatum)parent.Root().Tag).StrFile;

            if ((parent.Tag is NodeDatum) == false)
            {
                return null;
            }

            NodeDatum nodeDatum = (NodeDatum)parent.Tag;

            if (nodeDatum.nLineNo <= 0)
            {
                return null;
            }

            long nPrevDir = nodeDatum.nPrevLineNo;
            long nLineNo = nodeDatum.nLineNo;

            if (nPrevDir <= 0)
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

            if (listLines.Count <= 0)
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

                if ((strArrayFiles.Length > mNcolLengthLV) && (false == string.IsNullOrWhiteSpace(strArrayFiles[mNcolLengthLV])))
                {
                    nLengthDebug += nLength = ulong.Parse(strArrayFiles[mNcolLengthLV]);
                    strArrayFiles[mNcolLengthLV] = FormatSize(strArrayFiles[mNcolLengthLV]);
                }

                listFiles.Add(strArrayFiles);

                if (listLength != null)
                {
                    listLength.Add(nLength);
                }
            }

            Utilities.Assert(1301.2313, nLengthDebug == nodeDatum.nLength);
            return listFiles;
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

                    Utilities.Assert(1301.2314, new int[] { 7, 8, mAnDIviewOrder.Length }.Contains(arrDriveInfo.Length));

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
                            mAbDIsizeType[i] ? FormatSize(a[1], bBytes: true) : a[1]
                        };
                    }

                    SDL_ListViewItem[] lvItems = new SDL_ListViewItem[arrDriveInfo.Length];

                    for (int ix = 0; ix < arrDriveInfo.Length; ++ix)
                    {
                        if ((asItems[ix] == null) || (asItems[ix].Length == 0) || (asItems[ix][1].Trim().Length == 0))
                        {
                            continue;
                        }

                        if ((mAnDIoptIfEqTo[ix] != -1) && (asItems[ix][1] == asItems[mAnDIoptIfEqTo[ix]][1]))
                        {
                            continue;
                        }

                        int ixA = (arrDriveInfo.Length == mAnDIviewOrder.Length) ? mAnDIviewOrder[ix] : ix;

                        lvItems[ixA] = new SDL_ListViewItem(asItems[ix]);
                    }

                    m_statusCallback(lvVolDetails: lvItems.Where(i => i != null).ToArray());
                }
            }

            m_doneCallback(m_bSecondComparePane);
        }

        internal Thread DoThreadFactory()
        {
            m_thread = new Thread(new ThreadStart(Go));
            m_thread.IsBackground = true;
            m_thread.Start();
            return m_thread;
        }
    }
}
