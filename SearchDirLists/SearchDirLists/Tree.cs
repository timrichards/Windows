using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Drawing;
using System.Collections.Concurrent;

namespace SearchDirLists
{
    delegate void TreeStatusDelegate(TreeNode rootNode = null, LVvolStrings volStrings = null);
    delegate void TreeDoneDelegate();
    delegate void TreeSelectStatusDelegate(ListViewItem[] lvItemDetails = null, ListViewItem[] itemArray = null, ListViewItem lvVol = null, bool bSecondComparePane = false, LVitemFileTag lvFileItem = null);
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
        internal readonly String StrCompareDir = null;
        internal readonly long nNumFiles = 0;   // equivalent to number of items in the listviewer. Not currently used

        internal LVitemFileTag(String strCompareDir_in, long nNumFiles_in)
        {
            StrCompareDir = strCompareDir_in;
            nNumFiles = nNumFiles_in;
        }
    }

    class HashKey : IComparable
    {
        internal readonly ulong nTotalLength;      //  found   41 bits
        internal readonly uint nFilesInSubdirs;    //          23 bits
        internal readonly uint nDirsWithFiles;     //          16 bits

        internal HashKey(ulong nTotalLength_in, uint nFilesInSubdirs_in, uint nDirsWithFiles_in)
        {
            nTotalLength = nTotalLength_in;
            nFilesInSubdirs = nFilesInSubdirs_in;
            nDirsWithFiles = nDirsWithFiles_in;
        }

        public int CompareTo(object obj)
        {
            HashKey that = (HashKey) obj;

            if (this < that) return -1;
            if (this == that) return 0;
            return 1;
        }

        public override bool Equals(object obj)
        {
            if ((obj is HashKey) == false) return false;
            return (((HashKey)obj) == this);
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

        public static bool operator ==(HashKey x, HashKey y)
        {
            return (x.nTotalLength == y.nTotalLength) && (x.nFilesInSubdirs == y.nFilesInSubdirs) && (x.nDirsWithFiles == y.nDirsWithFiles);
        }

        public static bool operator <(HashKey x, HashKey y)
        {
            if (x.nTotalLength > y.nTotalLength) return false;
            if (x.nTotalLength < y.nTotalLength) return true;
            if (x.nFilesInSubdirs > y.nFilesInSubdirs) return false;
            if (x.nFilesInSubdirs < y.nFilesInSubdirs) return true;
            if (x.nDirsWithFiles > y.nDirsWithFiles) return false;
            if (x.nDirsWithFiles < y.nDirsWithFiles) return true;
            return false;
        }

        public static bool operator !=(HashKey x, HashKey y) { return ((x == y) == false); }
        public static bool operator >(HashKey x, HashKey y) { return ((x <= y) == false); }
        public static bool operator <=(HashKey x, HashKey y) { return ((x < y) || (x == y)); }
        public static bool operator >=(HashKey x, HashKey y) { return ((x < y) == false); } 
    }                              

    class NodeDatum : DetailsDatum
    {
        internal readonly uint nPrevLineNo = 0;
        internal readonly uint nLineNo = 0;
        internal readonly ulong nLength = 0;

        class NodeDatumLVitemHolder     // this was a way of setting the listview item in a different node after processing the first. Not used.
        {
            internal ListViewItem m_lvItem = null;
        }

        internal HashKey Key
        {
            get
            {
                return new HashKey((ulong)nTotalLength, nFilesInSubdirs, nDirsWithFiles);
            }
        }

        internal List<TreeNode> m_listClones = null;

        internal void SetLVitemHolder(NodeDatum holder) { m_lvItem_ = (holder != null) ? holder.m_lvItem_ : null; }
        NodeDatumLVitemHolder m_lvItem_ = new NodeDatumLVitemHolder();
        internal ListViewItem m_lvItem
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
        internal readonly String StrFile = null;
        internal readonly String StrVolumeGroup = null;
        internal readonly ulong VolumeFree = 0;
        internal readonly ulong VolumeLength = 0;
        public bool VolumeView = true;

        internal RootNodeDatum(NodeDatum node, String strFile_in, String strVolGroup_in,
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

    class LVitemNameComparer : IEqualityComparer<ListViewItem>
    {
        public bool Equals(ListViewItem x, ListViewItem y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            return (x != null) && (y != null) && Utilities.StrValid(x.Name) && x.Name.Equals(y.Name);
        }

        public int GetHashCode(ListViewItem obj)
        {
            return obj.Name == null ? 0 : obj.Name.GetHashCode();
        }

        internal static void NameItems(ListView.ListViewItemCollection list)
        {
            foreach (ListViewItem item in list)
            {
                item.Name = item.Text;

                if (item.SubItems.Count > Utilities.nColLENGTH_LV)
                {
                    item.Name += item.SubItems[Utilities.nColLENGTH_LV].Text;      // name + size
                }
            }
        }

        internal static void MarkItemsFrom1notIn2(ListView lv1, ListView lv2)
        {
            if ((lv1.Items.Count <= 0) || (lv2.Items.Count <= 0)) { return; }

            List<ListViewItem> list = lv1.Items.Cast<ListViewItem>().Except(lv2.Items.Cast<ListViewItem>(), new LVitemNameComparer()).ToList();

            if (list.Count > 0)
            {
                lv1.TopItem = list[0];
            }

            foreach (ListViewItem item in list)
            {
                item.ForeColor = Color.Red;
            }
        }

        internal static void SetTopItem(ListView lv1, ListView lv2)
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

    class Tree : Utilities
    {
        List<LVvolStrings> m_list_lvVolStrings = new List<LVvolStrings>();
        Hashtable m_hashCache = null;
        TreeStatusDelegate m_statusCallback = null;
        TreeDoneDelegate m_doneCallback = null;
        ConcurrentBag<TreeRootNodeBuilder> m_cbagWorkers = new ConcurrentBag<TreeRootNodeBuilder>();
        Thread m_thread = null;
        bool m_bThreadAbort = false;

        class TreeRootNodeBuilder : Utilities
        {
            static TreeStatusDelegate m_statusCallback = null;
            Thread m_thread = null;
            bool m_bThreadAbort = false;
            LVvolStrings m_volStrings = null;
            Hashtable m_hashCache = null;

            class DirData
            {
                RootNode m_rootNode = null;
                static TreeStatusDelegate m_statusCallback = null;

                internal DirData(TreeStatusDelegate statusCallback, RootNode rootNode)
                {
                    m_statusCallback = statusCallback;
                    m_rootNode = rootNode;
                }

                internal void AddToTree(String in_str, uint nLineNo, ulong nLength)
                {
                    if (m_rootNode.Nodes.ContainsKey(in_str))
                    {
                        Node node = m_rootNode.Nodes[in_str];
                        Utilities.Assert(1301.2309, false);
                    }

                    in_str = in_str.TrimEnd(Path.DirectorySeparatorChar);
                    m_rootNode.Nodes.Add(in_str, new Node(in_str, nLineNo, nLength, m_rootNode));
                }

                internal TreeNode AddToTree(String strVolumeName)
                {
                    TreeNode rootNode = m_rootNode.Nodes.Values.First().AddToTree(strVolumeName);

                    m_statusCallback(rootNode);
                    return rootNode;
                }
            }

            class Node : Utilities
            {
                readonly RootNode m_rootNode = null;
                SortedDictionary<String, Node> subNodes = new SortedDictionary<String, Node>();
                readonly String m_strPath = null;
                uint m_nPrevLineNo = 0;
                uint m_nLineNo = 0;
                ulong m_nLength = 0;
                bool bUseShortPath = true;

                internal Node(String in_str, uint nLineNo, ulong nLength, RootNode rootNode)
                {
                    if (Form1.AppExit)
                    {
                        return;
                    }

                    Utilities.Assert(1301.2301, nLineNo != 0);
                    m_rootNode = rootNode;

                    if (in_str.EndsWith(":" + Path.DirectorySeparatorChar) == false)
                    {
                        Utilities.Assert(1301.2302, in_str.Trim().EndsWith(Path.DirectorySeparatorChar.ToString()) == false);
                    }

                    m_strPath = in_str;
                    m_nPrevLineNo = m_rootNode.FirstLineNo;
                    m_rootNode.FirstLineNo = m_nLineNo = nLineNo;
                    m_nLength = nLength;

                    // Path.GetDirectoryName() does not preserve filesystem root

                    String strParent = m_strPath;
                    int nIndex = strParent.LastIndexOf(Path.DirectorySeparatorChar);

                    if (nIndex < 0)
                    {
                        return;
                    }

                    strParent = strParent.Remove(nIndex).TrimEnd(Path.DirectorySeparatorChar);

                    if (m_rootNode.Nodes.ContainsKey(strParent) == false)
                    {
                        m_rootNode.Nodes.Add(strParent, new Node(strParent, m_rootNode.FirstLineNo, 0, m_rootNode));
                    }

                    if (m_rootNode.Nodes[strParent].subNodes.ContainsKey(m_strPath) == false)
                    {
                        m_rootNode.Nodes[strParent].subNodes.Add(m_strPath, this);
                    }
                }

                internal TreeNode AddToTree(String strVolumeName = null)
                {
                    if (Form1.AppExit)
                    {
                        return new TreeNode();
                    }

                    int nIndex = m_strPath.LastIndexOf(Path.DirectorySeparatorChar);
                    String strShortPath = bUseShortPath ? m_strPath.Substring(nIndex + 1) : m_strPath;
                    TreeNode treeNode = null;

                    if (subNodes.Count == 1)
                    {
                        Node subNode = subNodes.Values.First();

                        if (this == m_rootNode.Nodes.Values.First())
                        {
                            // cull all root node single-chains.
                            m_rootNode.Nodes = subNodes;
                            subNode.m_strPath.Insert(0, m_strPath + Path.DirectorySeparatorChar);
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
                        List<TreeNode> treeList = new List<TreeNode>();

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

                    Utilities.Assert(1302.3318, treeNode.SelectedImageIndex == -1);        // sets the bitmap size
                    treeNode.Tag = new NodeDatum(m_nPrevLineNo, m_nLineNo, m_nLength);  // this is almost but not quite always newly assigned here.

                    if (this == m_rootNode.Nodes.Values.First())
                    {
                        treeNode.Name = treeNode.Text;

                        if (StrValid(strVolumeName))
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
                SortedDictionary<String, Node> m_nodes = new SortedDictionary<String, Node>();
                uint m_firstLineNo = 0;

                internal SortedDictionary<String, Node> Nodes { get { return m_nodes; } set { m_nodes = value; } }
                internal uint FirstLineNo { get { return m_firstLineNo; } set { m_firstLineNo = value; } }
            }

            internal TreeRootNodeBuilder(LVvolStrings volStrings, Hashtable hashCache, TreeStatusDelegate statusCallback)
            {
                m_volStrings = volStrings;
                m_hashCache = hashCache;
                m_statusCallback = statusCallback;
            }

            DetailsDatum TreeSubnodeDetails(TreeNode treeNode)
            {
                DetailsDatum datum = new DetailsDatum();

                foreach (TreeNode node in treeNode.Nodes)
                {
                    if (m_bThreadAbort || Form1.AppExit)
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

                HashKey nKey = nodeDatum.Key;

                lock (m_hashCache)
                {
                    if (m_hashCache.ContainsKey(nKey))
                    {
                        ((List<TreeNode>)m_hashCache[nKey]).Add(treeNode);
                    }
                    else if (nodeDatum.nTotalLength > 100 * 1024)
                    {
                        List<TreeNode> listNodes = new List<TreeNode>();

                        listNodes.Add(treeNode);
                        m_hashCache.Add(nKey, listNodes);
                    }
                }

                return datum;
            }

            internal void Go()
            {
                DateTime dtStart = DateTime.Now;

                if (LV_VolumesItemCanLoad(m_volStrings) == false)
                {
                    Utilities.Assert(1301.2303, false);    // guaranteed by caller
                    return;
                }

                String strVolumeName = m_volStrings.VolumeName;
                String strPath = m_volStrings.StrPath;
                String strSaveAs = m_volStrings.SaveAs;

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

                        File.Delete(StrFile_01(strSaveAs));
                        bAttemptConvert = true;
                    }

                    if (bValid == false)
                    {
                        m_MessageboxCallback(("Bad file: " + strSaveAs).PadRight(100), "Tree");
                        m_statusCallback(volStrings: m_volStrings);
                        return;
                    }
                }

                ulong nVolFree = 0;
                ulong nVolLength = 0;

                {
                    String[] arrDriveInfo = File.ReadLines(strSaveAs).Where(s => s.StartsWith(m_strLINETYPE_DriveInfo)).ToArray();
                    StringBuilder strBuilder = new StringBuilder();
                    int nIx = -1;

                    foreach (String strLine in arrDriveInfo)
                    {
                        String[] strArray = strLine.Split('\t');
                        ++nIx;

                        if (strArray.Length > 2)
                        {
                            strBuilder.AppendLine(strArray[2]);

                            if ((nIx == 5) && StrValid(strArray[2]))
                            {
                                nVolFree = ulong.Parse(strArray[2]);
                            }
                            else if ((nIx == 6) && StrValid(strArray[2]))
                            {
                                nVolLength = ulong.Parse(strArray[2]);
                            }
                        }
                    }

                    lock (m_hashCache)
                    {
                        if (m_hashCache.Contains("driveInfo" + strSaveAs))
                        {
                            Utilities.Assert(1301.23035, false);
                            m_hashCache.Remove("driveInfo" + strSaveAs);
                        }

                        m_hashCache.Add("driveInfo" + strSaveAs, strBuilder.ToString().Trim());
                    }
                }

                RootNode rootNode = new RootNode();

                {
                    String strStart = File.ReadLines(strSaveAs).Where(s => s.StartsWith(m_strLINETYPE_Start)).ToArray()[0];
                    rootNode.FirstLineNo = uint.Parse(strStart.Split('\t')[1]);
                }

                List<String> listLines = File.ReadLines(strSaveAs).Where(s => s.StartsWith(m_strLINETYPE_Directory)).ToList();
                DirData dirData = new DirData(m_statusCallback, rootNode);

                foreach (String strLine in listLines)
                {
                    if (Form1.AppExit)
                    {
                        return;
                    }

                    String[] strArray = strLine.Split('\t');
                    uint nLineNo = uint.Parse(strArray[1]);
                    int nIx = nColLENGTH;
                    ulong nLength = 0;

                    if ((strArray.Length > nIx) && StrValid(strArray[nIx]))
                    {
                        nLength = ulong.Parse(strArray[nIx]);
                    }

                    String strDir = strArray[2];

                    dirData.AddToTree(strDir, nLineNo, nLength);
                }

                TreeNode rootTreeNode = dirData.AddToTree(strVolumeName);
                ulong nScannedLength = ulong.Parse(
                    File.ReadLines(strSaveAs).Where(s => s.StartsWith(m_strLINETYPE_Length)).ToArray()[0]
                    .Split('\t')[nColLENGTH]);

                rootTreeNode.Tag = new RootNodeDatum((NodeDatum)rootTreeNode.Tag, strSaveAs, m_volStrings.VolumeGroup, nVolFree, nVolLength);
                TreeSubnodeDetails(rootTreeNode);
                Utilities.Assert(1301.2304, nScannedLength == ((RootNodeDatum)rootTreeNode.Tag).nTotalLength);
                Console.WriteLine(strSaveAs + " tree took " + (DateTime.Now - dtStart).TotalMilliseconds / 1000.0 + " seconds.");
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

        internal Tree(ListView.ListViewItemCollection lvVolItems, Hashtable hashCache,
            TreeStatusDelegate statusCallback, TreeDoneDelegate doneCallback)
        {
            foreach (ListViewItem lvItem in lvVolItems)
            {
                m_list_lvVolStrings.Add(new LVvolStrings(lvItem));
            }

            m_hashCache = hashCache;
            m_statusCallback = statusCallback;
            m_doneCallback = doneCallback;
        }

        void Go()
        {
            Console.WriteLine();
            Console.WriteLine("Creating tree.");

            DateTime dtStart = DateTime.Now;

            foreach (LVvolStrings volStrings in m_list_lvVolStrings)
            {
                if (LV_VolumesItemCanLoad(volStrings) == false)
                {
                    continue;
                }

                TreeRootNodeBuilder treeRoot = new TreeRootNodeBuilder(volStrings, m_hashCache, m_statusCallback);
                m_cbagWorkers.Add(treeRoot.DoThreadFactory());
            }

            foreach (TreeRootNodeBuilder worker in m_cbagWorkers)
            {
                worker.Join();
            }

            Console.WriteLine(String.Format("Completed tree in {0} seconds.", ((int)(DateTime.Now - dtStart).TotalMilliseconds / 10) / 100.0));

            if (m_cbagWorkers.Count <= 0)
            {
                return;
            }

            if (m_bThreadAbort || Form1.AppExit)
            {
                return;
            }

            m_doneCallback();
        }

        internal void EndThread(bool bJoin = false)
        {
            foreach (TreeRootNodeBuilder worker in m_cbagWorkers)
            {
				worker.Abort();
            }

            TreeDone.Abort();
            m_bThreadAbort = true;
            m_thread = null;
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
        TreeNode m_treeNode = null;
        Hashtable m_hashCache = null;
        static TreeSelectStatusDelegate m_statusCallback = null;
        static TreeSelectDoneDelegate m_doneCallback = null;
        Thread m_thread = null;
        String m_strFile = null;
        bool m_bCompareMode = false;
        bool m_bSecondComparePane = false;

        internal TreeSelect(TreeNode node, Hashtable hashCache, String strFile, bool bCompareMode, bool bSecondComparePane,
            TreeSelectStatusDelegate statusCallback, TreeSelectDoneDelegate doneCallback)
        {
            m_treeNode = node;
            m_hashCache = hashCache;
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
                Utilities.Assert(1301.2305, false);
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
            String strLine = File.ReadLines(m_strFile).Skip((int)nLineNo - 1).Take(1).ToArray()[0];
            String[] strArray = strLine.Split('\t');

            Utilities.Assert(1301.2306, StrValid(strArray[2]));

            long nIx = 0;
            DateTime dt;


            // Directory detail

            List<ListViewItem> listItems = new List<ListViewItem>();

            nIx = 4; if ((strArray.Length > nIx) && StrValid(strArray[nIx])) { listItems.Add(new ListViewItem(new String[] { "Created\t", (dt = DateTime.Parse(strArray[nIx])).ToLongDateString() + ", " + dt.ToLongTimeString() })); }
            nIx = 5; if ((strArray.Length > nIx) && StrValid(strArray[nIx])) listItems.Add(new ListViewItem(new String[] { "Modified\t", (dt = DateTime.Parse(strArray[nIx])).ToLongDateString() + ", " + dt.ToLongTimeString() }));
            nIx = 6; if ((strArray.Length > nIx) && StrValid(strArray[nIx])) listItems.Add(new ListViewItem(new String[] { "Attributes\t", strArray[nIx] }));
            listItems.Add(new ListViewItem(new String[] { "Immediate Size\t", FormatSize(nodeDatum.nLength, bBytes: true) }));
            nIx = 8; if ((strArray.Length > nIx) && StrValid(strArray[nIx])) listItems.Add(new ListViewItem(new String[] { "Error 1\t", strArray[nIx] }));
            nIx = 9; if ((strArray.Length > nIx) && StrValid(strArray[nIx])) listItems.Add(new ListViewItem(new String[] { "Error 2\t", strArray[nIx] }));
            listItems.Add(new ListViewItem(new String[] { "# Immediate Files", (nLineNo - nPrevDir - 1).ToString() }));

            // Tree subnode detail

            String NUMFMT = "###,###,###,##0";

            listItems.Add(new ListViewItem(new String[] { "# Immediate Folders", m_treeNode.Nodes.Count.ToString(NUMFMT) }));
            listItems.Add(new ListViewItem(new String[] { "Total # Files", nodeDatum.nFilesInSubdirs.ToString(NUMFMT) }));

            if (nodeDatum.nSubDirs > 0)
            {
                String strItem = nodeDatum.nSubDirs.ToString(NUMFMT);

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

                listItems.Add(new ListViewItem(new String[] { "# Subfolders", strItem }));
            }

            listItems.Add(new ListViewItem(new String[] { "Total Size", FormatSize(nodeDatum.nTotalLength, bBytes: true) }));
            m_statusCallback(lvItemDetails: listItems.ToArray(), bSecondComparePane: m_bSecondComparePane);
            Console.WriteLine(strLine);

            List<String[]> listFiles_A = GetFileList(m_treeNode);

            if (listFiles_A != null)
            {
                List<ListViewItem> listFiles = new List<ListViewItem>();

                foreach (String[] arrLine in listFiles_A)
                {
                    listFiles.Add(new ListViewItem(arrLine));
                }

                m_statusCallback(itemArray: listFiles.ToArray(), bSecondComparePane: m_bSecondComparePane, lvFileItem: new LVitemFileTag(m_treeNode.Text, listFiles.Count));
            }
        }

        internal static List<String[]> GetFileList(TreeNode parent, List<ulong> listLength = null)
        {
            String strFile = (String)((RootNodeDatum)parent.Root().Tag).StrFile;

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
            List<String> listLines = File.ReadLines(strFile)
                .Skip((int)nPrevDir)
                .Take((int)(nLineNo - nPrevDir - 1))
                .ToList();

            if (listLines.Count <= 0)
            {
                return null;
            }

            List<String[]> listFiles = new List<String[]>();
            ulong nLengthDebug = 0;

            foreach (String strFileLine in listLines)
            {
                String[] strArrayFiles = strFileLine.Split('\t').Skip(3).ToArray();
                ulong nLength = 0;

                if ((strArrayFiles.Length > nColLENGTH_LV) && StrValid(strArrayFiles[nColLENGTH_LV]))
                {
                    nLengthDebug += nLength = ulong.Parse(strArrayFiles[nColLENGTH_LV]);
                    strArrayFiles[nColLENGTH_LV] = FormatSize(strArrayFiles[nColLENGTH_LV]);
                }

                listFiles.Add(strArrayFiles);

                if (listLength != null)
                {
                    listLength.Add(nLength);
                }
            }

            Utilities.Assert(1301.2307, nLengthDebug == nodeDatum.nLength);
            return listFiles;
        }

        void Go_B()
        {
            // Volume detail

            if (m_hashCache.ContainsKey("driveInfo" + m_strFile))
            {
                String strDriveInfo = (String)m_hashCache["driveInfo" + m_strFile];
                String[] arrDriveInfo = strDriveInfo.Split(new String[] { "\r\n", "\n" }, StringSplitOptions.None);

                Utilities.Assert(1301.2308, new int[] { 7, 8 }.Contains(arrDriveInfo.Length));
                m_statusCallback(lvVol: new ListViewItem(new String[] { "Available Free Space", FormatSize(arrDriveInfo[0], bBytes: true) }));
                m_statusCallback(lvVol: new ListViewItem(new String[] { "Drive Format", arrDriveInfo[1] }));
                m_statusCallback(lvVol: new ListViewItem(new String[] { "Drive Type", arrDriveInfo[2] }));
                m_statusCallback(lvVol: new ListViewItem(new String[] { "Name", arrDriveInfo[3] }));
                m_statusCallback(lvVol: new ListViewItem(new String[] { "Root Directory", arrDriveInfo[4] }));
                m_statusCallback(lvVol: new ListViewItem(new String[] { "Total Free Space", FormatSize(arrDriveInfo[5], bBytes: true) }));
                m_statusCallback(lvVol: new ListViewItem(new String[] { "Total Size", FormatSize(arrDriveInfo[6], bBytes: true) }));

                if (arrDriveInfo.Length > 7)
                {
                    m_statusCallback(lvVol: new ListViewItem(new String[] { "Volume Label", arrDriveInfo[7] }));
                }
            }
        }

        void Go()
        {
            Go_A();

            if (m_bCompareMode == false)
            {
                // not in compare mode so do directory detail
                Go_B();
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
