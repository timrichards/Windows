using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Drawing;

namespace SearchDirLists
{
    delegate void TreeStatusDelegate(TreeNode rootNode = null, LVvolStrings volStrings = null);
    delegate void TreeDoneDelegate();
    delegate void TreeSelectStatusDelegate(ListViewItem[] lvItemDetails = null, ListViewItem[] itemArray = null, ListViewItem lvVol = null, bool bSecondComparePane = false, LVitemFileTag lvFileItem = null);
    delegate void TreeSelectDoneDelegate(bool bSecondComparePane);

    class DetailsDatum
    {
        protected ulong m_nTotalLength = 0;
        protected uint m_nFilesInSubdirs = 0;
        protected uint m_nSubDirs = 0;
        protected uint m_nImmediateFiles = 0;
        protected uint m_nDirsWithFiles = 0;

        public DetailsDatum()
        {
        }

        public DetailsDatum(DetailsDatum in_datum)
        {
            m_nTotalLength = in_datum.m_nTotalLength;
            m_nFilesInSubdirs = in_datum.m_nFilesInSubdirs;
            m_nSubDirs = in_datum.m_nSubDirs;
            m_nImmediateFiles = in_datum.m_nImmediateFiles;
            m_nDirsWithFiles = in_datum.m_nDirsWithFiles;
        }

        static public DetailsDatum operator +(DetailsDatum in_datum1, DetailsDatum in_datum2)
        {
            DetailsDatum datum = new DetailsDatum();

            datum.m_nTotalLength = in_datum1.m_nTotalLength + in_datum2.m_nTotalLength;
            datum.m_nFilesInSubdirs = in_datum1.m_nFilesInSubdirs + in_datum2.m_nFilesInSubdirs;
            datum.m_nSubDirs = in_datum1.m_nSubDirs + in_datum2.m_nSubDirs;
            datum.m_nImmediateFiles = in_datum1.m_nImmediateFiles + in_datum2.m_nImmediateFiles;
            datum.m_nDirsWithFiles = in_datum1.m_nDirsWithFiles + in_datum2.m_nDirsWithFiles;
            return datum;
        }

        public ulong TotalLength { get { return m_nTotalLength; } set { m_nTotalLength = value; } }
        public uint nFilesInSubdirs { get { return m_nFilesInSubdirs; } set { m_nFilesInSubdirs = value; } }
        public uint nSubDirs { get { return m_nSubDirs; } set { m_nSubDirs = value; } }
        public uint nImmediateFiles { get { return m_nImmediateFiles; } set { m_nImmediateFiles = value; } }
        public uint nDirsWithFiles { get { return m_nDirsWithFiles; } set { m_nDirsWithFiles = value; } }
    }

    // One tag at the first item, so the compare listviewer knows what the first listviewer's state is.
    class LVitemFileTag
    {
        public readonly String StrCompareDir = null;
        public readonly long nNumFiles = 0;   // equivalent to number of items in the listviewer. Not currently used

        public LVitemFileTag(String strCompareDir_in, long nNumFiles_in)
        {
            StrCompareDir = strCompareDir_in;
            nNumFiles = nNumFiles_in;
        }
    }

    class HashKey : IComparable
    {
        public readonly ulong nTotalLength;      //  found   41 bits
        public readonly uint nFilesInSubdirs;    //          23 bits
        public readonly uint nDirsWithFiles;     //          16 bits

        public HashKey(ulong nTotalLength_in, uint nFilesInSubdirs_in, uint nDirsWithFiles_in)
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
            unchecked               // any overflow mixes the bits a bit better
            {
                int result = 37;    // prime

                result *= 397;      // also prime
                result += nTotalLength.GetHashCode();
                result *= 397;
                result += nFilesInSubdirs.GetHashCode();
                result *= 397;
                result += nDirsWithFiles.GetHashCode();
                return result;
            }
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
        public readonly uint nPrevLineNo = 0;
        public readonly uint nLineNo = 0;
        public readonly ulong nLength = 0;

        class NodeDatumLVitemHolder     // this was a way of setting the listview item in a different node after processing the first. Not used.
        {
            public ListViewItem m_lvItem = null;
        }

        public HashKey Key
        {
            get
            {
                return new HashKey((ulong)TotalLength, nFilesInSubdirs, nDirsWithFiles);
            }
        }

        public List<TreeNode> m_listClones = null;

        public void SetLVitemHolder(NodeDatum holder) { m_lvItem_ = (holder != null) ? holder.m_lvItem_ : null; }
        NodeDatumLVitemHolder m_lvItem_ = new NodeDatumLVitemHolder();
        public ListViewItem m_lvItem
        {
            get { return (m_lvItem_ != null) ? m_lvItem_.m_lvItem : null; }
            set { if (m_lvItem_ != null) m_lvItem_.m_lvItem = value; }
        }

        public bool m_bDifferentVols = false;

        public NodeDatum(uint nPrevLineNo_in, uint nLineNo_in, ulong nLength_in) { nPrevLineNo = nPrevLineNo_in; nLineNo = nLineNo_in; nLength = nLength_in; }

        protected NodeDatum(NodeDatum node)
            : base(node)
        {
            nPrevLineNo = node.nPrevLineNo;
            nLineNo = node.nLineNo;
            nLength = node.nLength;
        }
    }

    class RootNodeDatum : NodeDatum
    {
        public readonly String StrFile = null;
        public readonly String StrVolumeGroup = null;

        public RootNodeDatum(NodeDatum node, String strFile, String strVolGroup)
            : base(node)
        {
            StrFile = strFile;
            StrVolumeGroup = strVolGroup;
        }
    }

    class LVitemNameComparer : IEqualityComparer<ListViewItem>
    {
        public bool Equals(ListViewItem x, ListViewItem y)
        {
            if (Object.ReferenceEquals(x, y)) return true;

            return x != null && y != null && Utilities.StrValid(x.Name) && x.Name.Equals(y.Name);
        }

        public int GetHashCode(ListViewItem obj)
        {
            return obj.Name == null ? 0 : obj.Name.GetHashCode();
        }

        public static void NameItems(ListView.ListViewItemCollection list)
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

        public static void MarkItemsFrom1notIn2(ListView lv1, ListView lv2)
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

        public static void SetTopItem(ListView lv1, ListView lv2)
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
        List<Thread> m_listThreads = new List<Thread>();
        Thread m_thread = null;

        class TreeRootNodeThread : Utilities
        {
            static TreeStatusDelegate m_statusCallback = null;
            Thread m_thread = null;
            LVvolStrings m_volStrings = null;
            Hashtable m_hashCache = null;

            class DirData
            {
                RootNode m_rootNode = null;
                static TreeStatusDelegate m_statusCallback = null;

                public DirData(TreeStatusDelegate statusCallback, RootNode rootNode)
                {
                    m_statusCallback = statusCallback;
                    m_rootNode = rootNode;
                }

                public void AddToTree(String in_str, uint nLineNo, ulong nLength)
                {
                    if (m_rootNode.Nodes.ContainsKey(in_str))
                    {
                        Node node = m_rootNode.Nodes[in_str];
                        Debug.Assert(false);
                    }

                    in_str = in_str.TrimEnd(Path.DirectorySeparatorChar);
                    m_rootNode.Nodes.Add(in_str, new Node(in_str, nLineNo, nLength, m_rootNode));
                }

                public TreeNode AddToTree(String strVolumeName)
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

                public Node(String in_str, uint nLineNo, ulong nLength, RootNode rootNode)
                {
                    Debug.Assert(nLineNo != 0);
                    m_rootNode = rootNode;

                    if (in_str.EndsWith(":" + Path.DirectorySeparatorChar) == false)
                    {
                        Debug.Assert(in_str.Trim().EndsWith(Path.DirectorySeparatorChar.ToString()) == false);
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

                public TreeNode AddToTree(String strVolumeName = null)
                {
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

                public SortedDictionary<String, Node> Nodes { get { return m_nodes; } set { m_nodes = value; } }
                public uint FirstLineNo { get { return m_firstLineNo; } set { m_firstLineNo = value; } }
            }

            public TreeRootNodeThread(LVvolStrings volStrings, Hashtable hashCache, TreeStatusDelegate statusCallback)
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

                nodeDatum.TotalLength = (datum.TotalLength += nodeDatum.nLength);
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
                    else if (nodeDatum.TotalLength > 100 * 1024)
                    {
                        List<TreeNode> listNodes = new List<TreeNode>();

                        listNodes.Add(treeNode);
                        m_hashCache.Add(nKey, listNodes);
                    }
                }

                return datum;
            }

            public void Go()
            {
                DateTime dtStart = DateTime.Now;

                if (LV_VolumesItemCanLoad(m_volStrings) == false)
                {
                    Debug.Assert(false);    // guaranteed by caller
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

                    while (true)
                    {
                        bool bAttemptConvert = false;

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
                        m_MessageboxCallback("Bad file: " + strSaveAs);
                        m_statusCallback(volStrings: m_volStrings);
                        return;
                    }
                }

                {
                    String[] arrDriveInfo = File.ReadLines(strSaveAs).Where(s => s.StartsWith(m_strLINETYPE_DriveInfo)).ToArray();
                    StringBuilder strBuilder = new StringBuilder();

                    foreach (String strLine in arrDriveInfo)
                    {
                        String[] strArray = strLine.Split('\t');

                        if (strArray.Length > 2)
                        {
                            strBuilder.AppendLine(strArray[2]);
                        }
                    }

                    lock (m_hashCache)
                    {
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

                rootTreeNode.Tag = new RootNodeDatum((NodeDatum)rootTreeNode.Tag, strSaveAs, m_volStrings.VolumeGroup);
                TreeSubnodeDetails(rootTreeNode);
                Console.WriteLine(strSaveAs + " tree took " + (DateTime.Now - dtStart).TotalMilliseconds / 1000.0 + " seconds.");
            }

            public Thread DoThreadFactory()
            {
                m_thread = new Thread(new ThreadStart(Go));
                m_thread.IsBackground = true;
                m_thread.Start();
                return m_thread;
            }
        }

        public Tree(ListView.ListViewItemCollection lvVolItems, Hashtable hashCache,
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

                TreeRootNodeThread treeRoot = new TreeRootNodeThread(volStrings, m_hashCache, m_statusCallback);
                m_listThreads.Add(treeRoot.DoThreadFactory());
            }

            foreach (Thread thread in m_listThreads)
            {
                thread.Join();
            }

            Console.WriteLine(String.Format("Completed tree in {0} seconds.", ((int)(DateTime.Now - dtStart).TotalMilliseconds / 10) / 100.0));

            if (m_listThreads.Count <= 0)
            {
                return;
            }

            m_doneCallback();
        }

        public void EndThread(bool bJoin = false)
        {
            lock (m_listThreads)
            {
                foreach (Thread thread in m_listThreads)
                {
                    if (thread.IsAlive)
                    {
                        thread.Abort();
                    }
                }
            }

            m_listThreads.Clear();

            if ((m_thread != null) && m_thread.IsAlive)
            {
                m_thread.Abort();

                if (m_thread.IsAlive && bJoin)
                {
                    m_thread.Join();
                }
            }

            m_thread = null;
        }

        public void DoThreadFactory()
        {
            m_thread = new Thread(new ThreadStart(Go));
            m_thread.IsBackground = true;
            m_thread.Start();
        }
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

        public TreeSelect(TreeNode node, Hashtable hashCache, String strFile, bool bCompareMode, bool bSecondComparePane,
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

        static public TreeNode GetParentRoot(TreeNode treeNode)
        {
            TreeNode nodeParent = treeNode;

            while (nodeParent.Parent != null)
            {
                nodeParent = nodeParent.Parent;
            }

            return nodeParent;
        }

        void Go_A()
        {
            if (File.Exists(m_strFile) == false)
            {
                Debug.Assert(false);
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

            Debug.Assert(StrValid(strArray[2]));

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

            listItems.Add(new ListViewItem(new String[] { "Total Size", FormatSize(nodeDatum.TotalLength, bBytes: true) }));

            m_statusCallback(lvItemDetails: listItems.ToArray(), bSecondComparePane: m_bSecondComparePane);
            Console.WriteLine(strLine);


            // file list

            if (nPrevDir <= 0)
            {
                return;
            }

            if ((nLineNo - nPrevDir) <= 1)  // dir has no files
            {
                return;
            }

            DateTime dtStart = DateTime.Now;
            List<String> listLines = File.ReadLines(m_strFile)
                .Skip((int)nPrevDir)
                .Take((int)(nLineNo - nPrevDir - 1))
                .ToList();

            if (listLines.Count <= 0)
            {
                return;
            }

            List<ListViewItem> listFiles = new List<ListViewItem>();
            ulong nLengthDebug = 0;

            foreach (String strFileLine in listLines)
            {
                String[] strArrayFiles = strFileLine.Split('\t').Skip(3).ToArray();

                if ((strArrayFiles.Length > nColLENGTH_LV) && StrValid(strArrayFiles[nColLENGTH_LV]))
                {
                    nLengthDebug += ulong.Parse(strArrayFiles[nColLENGTH_LV]);
                    strArrayFiles[nColLENGTH_LV] = FormatSize(strArrayFiles[nColLENGTH_LV]);
                }

                listFiles.Add(new ListViewItem(strArrayFiles));
            }

            m_statusCallback(itemArray: listFiles.ToArray(), bSecondComparePane: m_bSecondComparePane, lvFileItem: new LVitemFileTag(m_treeNode.Text, listFiles.Count));
            Debug.Assert(nLengthDebug == nodeDatum.nLength);
        }

        void Go_B()
        {
            // Volume detail

            if (m_hashCache.ContainsKey("driveInfo" + m_strFile))
            {
                String strDriveInfo = (String)m_hashCache["driveInfo" + m_strFile];
                String[] arrDriveInfo = strDriveInfo.Split(new String[] { "\r\n", "\n" }, StringSplitOptions.None);

                Debug.Assert(new int[] { 7, 8 }.Contains(arrDriveInfo.Length));
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

        public Thread DoThreadFactory()
        {
            m_thread = new Thread(new ThreadStart(Go));

            m_thread.IsBackground = true;
            m_thread.Start();
            return m_thread;
        }
    }
}
