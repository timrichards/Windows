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
        protected long m_nLengthSubnodes = 0;
        protected long m_nNumSubnodeFiles = 0;
        protected long m_nNumSubnodes = 0;
        protected long m_nNumImmediateFiles = 0;

        public DetailsDatum()
        {
        }

        public DetailsDatum(DetailsDatum in_datum)
        {
            m_nLengthSubnodes = in_datum.m_nLengthSubnodes;
            m_nNumSubnodeFiles = in_datum.m_nNumSubnodeFiles;
            m_nNumSubnodes = in_datum.m_nNumSubnodes;
            m_nNumImmediateFiles = in_datum.m_nNumImmediateFiles;
        }

        static public DetailsDatum operator +(DetailsDatum in_datum1, DetailsDatum in_datum2)
        {
            DetailsDatum datum = new DetailsDatum();

            datum.m_nLengthSubnodes = in_datum1.m_nLengthSubnodes + in_datum2.m_nLengthSubnodes;
            datum.m_nNumSubnodeFiles = in_datum1.m_nNumSubnodeFiles + in_datum2.m_nNumSubnodeFiles;
            datum.m_nNumSubnodes = in_datum1.m_nNumSubnodes + in_datum2.m_nNumSubnodes;
            datum.m_nNumImmediateFiles = in_datum1.m_nNumImmediateFiles + in_datum2.m_nNumImmediateFiles;
            return datum;
        }

        public long LengthSubnodes { get { return m_nLengthSubnodes; } set { m_nLengthSubnodes = value; } }
        public long NumSubnodeFiles { get { return m_nNumSubnodeFiles; } set { m_nNumSubnodeFiles = value; } }
        public long NumSubnodes { get { return m_nNumSubnodes; } set { m_nNumSubnodes = value; } }
        public long NumImmediateFiles { get { return m_nNumImmediateFiles; } set { m_nNumImmediateFiles = value; } }
    }

    class NodeDatumLVitemHolder     // this was a way of setting the listview item in a different node after processing the first. Not used.
    {
        public ListViewItem m_lvItem = null;
    }

    class NodeDatum : DetailsDatum
    {
        long m_nPrevLineNo = 0;
        long m_nLineNo = 0;
        long m_nLength = 0;

        public long PrevlineNo { get { return m_nPrevLineNo; } }
        public long LineNo { get { return m_nLineNo; } }
        public long Length { get { return m_nLength; } }

        public double Key
        {
            get
            {
                const double n1stDIV = 100000.0;
                double nSubnodes = NumSubnodes / n1stDIV;
                double nSubnodeFiles = NumSubnodeFiles / n1stDIV / 10000000.0;
                double nRet = LengthSubnodes + nSubnodeFiles + nSubnodes;

                Debug.Assert(nSubnodes < 1.0);
                Debug.Assert(nSubnodeFiles * n1stDIV < 1.0);
                return nRet;
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

        public NodeDatum(long nPrevLineNo, long nLineNo, long nLength) { m_nPrevLineNo = nPrevLineNo; m_nLineNo = nLineNo; m_nLength = nLength; }

        protected NodeDatum(NodeDatum node) : base(node)
        {
            m_nPrevLineNo = node.m_nPrevLineNo;
            m_nLineNo = node.m_nLineNo;
            m_nLength = node.m_nLength;
        }
    }

    class RootNodeDatum : NodeDatum
    {
        String m_strFile = null;
        String m_strVolumeGroup = null;

        public String StrFile { get { return m_strFile; } }
        public String StrVolumeGroup { get { return m_strVolumeGroup; } }
        public RootNodeDatum(NodeDatum node, String strFile, String strVolGroup) : base(node)
        {
            m_strFile = strFile;
            m_strVolumeGroup = strVolGroup;
        }
    }

    class RootNode
    {
        SortedDictionary<String, Node> m_nodes = new SortedDictionary<string, Node>();
        long m_firstLineNo = 0;

        public SortedDictionary<String, Node> Nodes { get { return m_nodes; } set { m_nodes = value; } }
        public long FirstLineNo { get { return m_firstLineNo; } set { m_firstLineNo = value; } }
    }

    class Node : Utilities
    {
        RootNode m_rootNode = null;
        SortedDictionary<String, Node> subNodes = new SortedDictionary<string, Node>();
        String m_strPath = null;
        long m_nPrevLineNo = 0;
        long m_nLineNo = 0;
        long m_nLength = 0;
        bool bUseShortPath = true;

        public Node(String in_str, long nLineNo, long nLength, RootNode rootNode)
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

            strParent = strParent.Remove(nIndex);

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

    class DirData
    {
        RootNode m_rootNode = null;
        static TreeStatusDelegate m_statusCallback = null;

        public DirData(TreeStatusDelegate statusCallback, RootNode rootNode)
        {
            m_statusCallback = statusCallback;
            m_rootNode = rootNode;
        }

        public void AddToTree(String in_str, long nLineNo, long nLength)
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

    class TreeRootNodeThread : Utilities
    {
        static TreeStatusDelegate m_statusCallback = null;
        Thread m_thread = null;
        LVvolStrings m_volStrings = null;
        Hashtable m_hashCache = null;

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

            if (nodeDatum.LineNo <= 0)
            {
                return datum;
            }

            nodeDatum.LengthSubnodes = (datum.LengthSubnodes += nodeDatum.Length);
            nodeDatum.NumImmediateFiles = (nodeDatum.LineNo - nodeDatum.PrevlineNo - 1);
            nodeDatum.NumSubnodeFiles = (datum.NumSubnodeFiles += nodeDatum.NumImmediateFiles);
            nodeDatum.NumSubnodes = (datum.NumSubnodes += treeNode.Nodes.Count);

            double nKey = nodeDatum.Key;

            lock (m_hashCache)
            {
                if (m_hashCache.ContainsKey(nKey))
                {
                    ((List<TreeNode>)m_hashCache[nKey]).Add(treeNode);
                }
                else if (nodeDatum.LengthSubnodes > 100 * 1024)
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
            String strPath = m_volStrings.Path;
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

                    do
                    {
                        String[] arrLine = File.ReadLines(strSaveAs).Take(1).ToArray();

                        if (arrLine.Length <= 0) break;

                        if (arrLine[0] == m_str_HEADER_01)
                        {
                            Console.WriteLine("Converting " + strSaveAs);
                            ConvertFile(strSaveAs);
                            Console.WriteLine("File converted to " + m_str_HEADER);
                        }

                        String strToken = File.ReadLines(strSaveAs).Take(1).ToArray()[0].Split('\t')[2];

                        if (strToken != m_str_HEADER) break;

                        String[] arrLine_A = File.ReadLines(strSaveAs).Where(s => s.StartsWith(m_strLINETYPE_Length)).ToArray();

                        if (arrLine_A.Length == 0) break;

                        String[] arrToken = arrLine_A[0].Split('\t');

                        if (arrToken.Length < 3) break;

                        String strToken_A = arrToken[2];

                        if (strToken_A != m_str_TOTAL_LENGTH_LOC) break;

                        bValid = true;
                    }
                    while (false);

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
                    strBuilder.AppendLine(strArray[2]);
                }

                lock (m_hashCache)
                {
                    m_hashCache.Add("driveInfo" + strSaveAs, strBuilder.ToString().Trim());
                }
            }

            RootNode rootNode = new RootNode();

            {
                String strStart = File.ReadLines(strSaveAs).Where(s => s.StartsWith(m_strLINETYPE_Start)).ToArray()[0];
                rootNode.FirstLineNo = long.Parse(strStart.Split('\t')[1]);
            }

            List<String> listLines = File.ReadLines(strSaveAs).Where(s => s.StartsWith(m_strLINETYPE_Directory)).ToList();
            DirData dirData = new DirData(m_statusCallback, rootNode);

            foreach (string strLine in listLines)
            {
                String[] strArray = strLine.Split('\t');
                long nLineNo = long.Parse(strArray[1]);
                int nIx = nColLENGTH;
                long nLength = 0;

                if ((strArray.Length > nIx) && StrValid(strArray[nIx]))
                {
                    nLength = long.Parse(strArray[nIx]);
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

    class Tree : Utilities
    {
        List<LVvolStrings> m_list_lvVolStrings = new List<LVvolStrings>();
        Hashtable m_hashCache = null;
        TreeStatusDelegate m_statusCallback = null;
        TreeDoneDelegate m_doneCallback = null;
        List<Thread> m_listThreads = new List<Thread>();
        Thread m_thread = null;

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

            if (m_listThreads.Count == 0)
            {
                return;
            }

            m_doneCallback();
        }

        public void EndThread()
        {
            foreach (Thread thread in m_listThreads)
            {
                if (thread.IsAlive)
                {
                    thread.Abort();
                }
            }

            m_listThreads = new List<Thread>();

            if ((m_thread != null) && m_thread.IsAlive)
            {
                m_thread.Abort();
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

    // One tag at the first item, so the compare listviewer knows what the first listviewer's state is.
    class LVitemFileTag
    {
        String m_strCompareDir = null;
        long m_nNumFiles = 0;   // equivalent to number of items in the listviewer. Not currently used

        public String StrCompareDir { get { return m_strCompareDir; } }
        public long NumFiles { get { return m_nNumFiles; } }

        public LVitemFileTag(string strCompareDir, long nNumFiles)
        {
            m_strCompareDir = strCompareDir;
            m_nNumFiles = nNumFiles;
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

            if (nodeDatum.LineNo <= 0)
            {
                return;
            }

            long nPrevDir = nodeDatum.PrevlineNo;
            long nLineNo = nodeDatum.LineNo;
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
            listItems.Add(new ListViewItem(new String[] { "Immediate Size\t", FormatSize(nodeDatum.Length, bBytes: true) }));
            nIx = 8; if ((strArray.Length > nIx) && StrValid(strArray[nIx])) listItems.Add(new ListViewItem(new String[] { "Error 1\t", strArray[nIx] }));
            nIx = 9; if ((strArray.Length > nIx) && StrValid(strArray[nIx])) listItems.Add(new ListViewItem(new String[] { "Error 2\t", strArray[nIx] }));
            listItems.Add(new ListViewItem(new String[] { "# Immediate Files", (nLineNo - nPrevDir - 1).ToString() }));

            // Tree subnode detail

            String NUMFMT = "###,###,###,##0";

            listItems.Add(new ListViewItem(new String[] { "# Immediate Folders", m_treeNode.Nodes.Count.ToString(NUMFMT) }));
            listItems.Add(new ListViewItem(new String[] { "Total # Files", nodeDatum.NumSubnodeFiles.ToString(NUMFMT) }));

            if (nodeDatum.NumSubnodes > 0)
            {
                listItems.Add(new ListViewItem(new String[] { "Total # Folders", nodeDatum.NumSubnodes.ToString(NUMFMT) }));
            }

            listItems.Add(new ListViewItem(new String[] { "Total Size", FormatSize(nodeDatum.LengthSubnodes, bBytes: true) }));

            m_statusCallback(lvItemDetails: listItems.ToArray(), bSecondComparePane:  m_bSecondComparePane);
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
            long nLengthDebug = 0;

            foreach (String strFileLine in listLines)
            {
                String[] strArrayFiles = strFileLine.Split('\t').Skip(3).ToArray();

                if ((strArrayFiles.Length > nColLENGTH_LV) && StrValid(strArrayFiles[nColLENGTH_LV]))
                {
                    nLengthDebug += long.Parse(strArrayFiles[nColLENGTH_LV]);
                    strArrayFiles[nColLENGTH_LV] = FormatSize(strArrayFiles[nColLENGTH_LV]);
                }

                listFiles.Add(new ListViewItem(strArrayFiles));
            }

            m_statusCallback(itemArray: listFiles.ToArray(), bSecondComparePane: m_bSecondComparePane, lvFileItem: new LVitemFileTag(m_treeNode.Text, listFiles.Count));
            Debug.Assert(nLengthDebug == nodeDatum.Length);
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

                if (arrDriveInfo.Length == 8)
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

    public partial class Form1 : Form
    {
        private bool m_bBrowseLoaded = false;
        Hashtable m_hashCache = new Hashtable();
        List<TreeNode> m_listTreeNodes = new List<TreeNode>();
        List<TreeNode> m_listRootNodes = null;
        Tree m_tree = null;
        Thread m_threadSelect = null;
        Thread m_threadSelectCompare = null;

        void TreeStatusCallback(TreeNode rootNode, LVvolStrings volStrings)
        {
            if (InvokeRequired) { Invoke(new TreeStatusDelegate(TreeStatusCallback), new object[] { rootNode, volStrings }); return; }

            if (volStrings != null)     // error state
            {
                volStrings.SetStatus_BadFile(form_lvVolumesMain);
            }

            if (rootNode != null)
            {
                lock (form_treeView_Browse)
                {
                    form_treeView_Browse.Nodes.Add(rootNode.Text);    // items added to show progress
                }

                lock (m_listRootNodes)
                {
                    m_listRootNodes.Add(rootNode);
                }
            }
        }

        // If an outer directory is cloned then all the inner ones are part of the outer clone and their clone status is redundant.
        // Breadth-first.
        void FixClones(SortedDictionary<double, List<TreeNode>> dictClones, TreeNode treeNode, TreeNode rootClone = null)
        {
            // neither rootClone nor nMaxLength are used at all (rootClone is used as a bool).
            // provisional.

            NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;
            List<TreeNode> listClones = nodeDatum.m_listClones;
            long nLength = nodeDatum.LengthSubnodes;

            if (nLength <= 100 * 1024)
            {
                treeNode.ForeColor = Color.LightGray;

                if (listClones != null)
                {
                    foreach (TreeNode otherNode in listClones)
                    {
                        ((NodeDatum)otherNode.Tag).m_listClones = null;
                    }
                }

                listClones = nodeDatum.m_listClones = null;
            }

            if ((listClones != null) && (rootClone == null))
            {
                rootClone = treeNode;

                if (dictClones.ContainsKey(nodeDatum.Key))
                {
                    Debug.Assert(dictClones[nodeDatum.Key] == listClones);
                    Debug.Assert(((NodeDatum)dictClones[nodeDatum.Key][0].Tag).m_bDifferentVols == nodeDatum.m_bDifferentVols);
                    Debug.Assert(dictClones[nodeDatum.Key][0].ForeColor == treeNode.ForeColor);
                }
                else
                {
                    dictClones.Add(nodeDatum.Key, listClones);

                    // Test to see if clones are on separate volumes.

                    TreeNode rootNode = TreeSelect.GetParentRoot(treeNode);
                    RootNodeDatum rootNodeDatum = (RootNodeDatum)rootNode.Tag;

                    Debug.Assert(treeNode.ForeColor == Color.Empty);
                    treeNode.ForeColor = Color.Firebrick;

                    bool bDifferentVols = false;

                    foreach (TreeNode subnode in listClones)
                    {
                        Debug.Assert(((NodeDatum)subnode.Tag).Key == nodeDatum.Key);

                        TreeNode rootNode_A = TreeSelect.GetParentRoot(subnode);

                        if (rootNode == rootNode_A)
                        {
                            continue;
                        }

                        RootNodeDatum rootNodeDatum_A = (RootNodeDatum)rootNode_A.Tag;

                        if (Utilities.StrValid(rootNodeDatum.StrVolumeGroup) &&
                            (rootNodeDatum.StrVolumeGroup == rootNodeDatum_A.StrVolumeGroup))
                        {
                            continue;
                        }

                        treeNode.ForeColor = Color.SteelBlue;
                        bDifferentVols = true;
                        break;
                    }

                    foreach (TreeNode subNode in listClones)
                    {
                        ((NodeDatum)subNode.Tag).m_bDifferentVols = bDifferentVols;
                        subNode.ForeColor = treeNode.ForeColor;
                    }
                }
            }

            foreach (TreeNode subNode in treeNode.Nodes)
            {
                FixClones(dictClones, subNode, rootClone);
            }
        }

        class InsertSizeMarker
        {
            static ListViewItem lvMarker = new ListViewItem();
            static bool bInit = false;

            static void Init()
            {
                if (bInit == false)
                {
                    lvMarker.BackColor = Color.DarkSlateGray;
                    lvMarker.ForeColor = Color.White;
                    lvMarker.Font = new Font(lvMarker.Font, FontStyle.Bold);
                    lvMarker.Tag = null;
                    bInit = true;
                }
            }

            public static void Go(List<ListViewItem> listLVitems, int nIx, bool bUnique, bool bAdd = false)
            {
                Init();

                ListViewItem lvItem = (ListViewItem)lvMarker.Clone();

                lvItem.Text = (Utilities.FormatSize(((NodeDatum)((TreeNode)(bUnique ? listLVitems[nIx].Tag : ((List<TreeNode>)listLVitems[nIx].Tag)[0])).Tag).LengthSubnodes, bNoDecimal: true));

                if (bAdd)
                {
                    listLVitems.Add(lvItem);
                }
                else
                {
                    listLVitems.Insert(nIx, lvItem);
                }
            }
        }

        void InsertSizeMarkers(List<ListViewItem> listLVitems)
        {
            if (listLVitems.Count <= 0)
            {
                return;
            }

            bool bUnique = (listLVitems[0].Tag is TreeNode);
            int nCount = listLVitems.Count;
            int nInterval = (nCount < 100) ? 10 : (nCount < 1000) ? 25 : 50;

            InsertSizeMarker.Go(listLVitems, nCount - 1, bUnique, bAdd: true);

            int nInitial = nCount % nInterval;

            if (nInitial == 0)
            {
                nInitial = nInterval;
            }

            for (int i = nCount - nInitial; i > nInterval / 2; i -= nInterval)
            {
                InsertSizeMarker.Go(listLVitems, i, bUnique);
            }

            InsertSizeMarker.Go(listLVitems, 0, bUnique);            // Enter the Zeroth
        }

        class AddTreeToList
        {
            List<TreeNode> m_listTreeNodes = null;
            List<TreeNode> m_listSameVol = null;

            public AddTreeToList(List<TreeNode> listTreeNodes, List<TreeNode> listSameVol)
            {
                m_listTreeNodes = listTreeNodes;
                m_listSameVol = listSameVol;
            }

            public void Go(TreeNode treeNode_in, bool bCloneOK = false)
            {
                TreeNode treeNode = treeNode_in;

                do
                {
                    NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;

                    if ((treeNode.ForeColor == Color.Firebrick) && (treeNode == nodeDatum.m_listClones[0]))
                    {
                        Debug.Assert((nodeDatum.m_listClones != null) && (nodeDatum.m_bDifferentVols == false));
                        m_listSameVol.Add(treeNode);
                    }

                    m_listTreeNodes.Add(treeNode);

                    if (bCloneOK)
                    {
                        treeNode.BackColor = Color.LightGoldenrodYellow;

                        if (nodeDatum.m_lvItem != null)
                        {
                            nodeDatum.m_lvItem.BackColor = treeNode.BackColor;
                        }
                    }

                    if (treeNode.FirstNode != null)
                    {
                        Go(treeNode.FirstNode, bCloneOK || (treeNode.ForeColor == Color.SteelBlue));
                    }

                    treeNode = treeNode.NextNode;
                }
                while (treeNode != null);
            }
        }

        void TreeDoneCallback()
        {
            if (InvokeRequired) { Invoke(new TreeDoneDelegate(TreeDoneCallback)); return; }

            Hashtable hashTable = new Hashtable();
            SortedDictionary<double, TreeNode> dictUnique = new SortedDictionary<double, TreeNode>();

            foreach (DictionaryEntry pair in m_hashCache)
            {
                if (pair.Value is List<TreeNode>)
                {
                    List<TreeNode> listNodes = (List<TreeNode>)pair.Value;

                    if (listNodes.Count > 1)
                    {
                        foreach (TreeNode treeNode in listNodes)
                        {
                            NodeDatum nodeDatum = ((NodeDatum)treeNode.Tag);

                            Debug.Assert(nodeDatum.LengthSubnodes > 100 * 1024);
                            nodeDatum.m_listClones = listNodes;
                        }
                    }
                    else
                    {
                        TreeNode treeNode = listNodes[0];
                        NodeDatum nodeDatum = ((NodeDatum)treeNode.Tag);

                        dictUnique.Add((double)pair.Key, treeNode);
                    }
                }
                else
                {
                    hashTable.Add(pair.Key, pair.Value);
                }
            }

            m_hashCache = hashTable;

            SortedDictionary<double, List<TreeNode>> dictClones = new SortedDictionary<double, List<TreeNode>>();

            foreach (TreeNode treeNode in m_listRootNodes)
            {
                FixClones(dictClones, treeNode);
            }

            IEnumerable<KeyValuePair<double, List<TreeNode>>> dictReverse = dictClones.Reverse();

            dictClones = null;

            List<ListViewItem> listLVitems = new List<ListViewItem>();

            foreach (KeyValuePair<double, List<TreeNode>> listNodes in dictReverse)
            {
                String str_nClones = null;
                int nClones = listNodes.Value.Count;

                if (nClones <= 0)
                {
                    Debug.Assert(false);
                    continue;
                }

                if (nClones > 2)        // includes the subject node: this line says don't put 2's all over the listviewer
                {
                    str_nClones = nClones.ToString("###,###");
                }

                String strNode = listNodes.Value[0].Text;

                Debug.Assert(Utilities.StrValid(strNode));

                ListViewItem lvItem = new ListViewItem(new String[] { strNode, str_nClones });

                lvItem.Tag = listNodes.Value;
                lvItem.ForeColor = listNodes.Value[0].ForeColor;

                foreach (TreeNode treeNode in listNodes.Value)
                {
                    ((NodeDatum)treeNode.Tag).m_lvItem = lvItem;
                }

                listLVitems.Add(lvItem);
            }

            dictReverse = null;
            InsertSizeMarkers(listLVitems);
            form_lvClones.Items.Clear();
            form_lvClones.Items.AddRange(listLVitems.ToArray());
            listLVitems = null;

            IEnumerable<KeyValuePair<double, TreeNode>> dictUniqueReverse = dictUnique.Reverse();
            List<ListViewItem> listLVunique = new List<ListViewItem>();

            int nCanceled = 0;

            foreach (KeyValuePair<double, TreeNode> listNodes in dictUniqueReverse)
            {
                TreeNode treeNode = listNodes.Value;
                TreeNode parentNode = treeNode.Parent;
                bool bCancel = false;

                while (parentNode != null)
                {
                    if (((NodeDatum)parentNode.Tag).m_bDifferentVols)
                    {
                        bCancel = true;
                        break;
                    }

                    parentNode = parentNode.Parent;
                }

                if (bCancel)
                {
                    ++nCanceled;
                    continue;
                }

                Debug.Assert(Utilities.StrValid(treeNode.Text));

                ListViewItem lvItem = new ListViewItem(treeNode.Text);

                lvItem.Tag = treeNode;

                NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;

                if (nodeDatum.NumImmediateFiles > 0)
                {
                    treeNode.ForeColor = lvItem.ForeColor = Color.Red;

                    TreeNode parentNode_A = treeNode.Parent;

                    while (parentNode_A != null)
                    {
                        if (parentNode_A.ForeColor == Color.Empty)
                        {
                            parentNode_A.ForeColor = Color.DarkRed;

                            NodeDatum nodeDatum_A = (NodeDatum)parentNode_A.Tag;

                            if (nodeDatum_A.m_lvItem != null)
                            {
                                nodeDatum_A.m_lvItem.ForeColor = parentNode_A.ForeColor;
                            }
                        }

                        parentNode_A = parentNode_A.Parent;
                    }
                }

                listLVunique.Add(lvItem);
                Debug.Assert(nodeDatum.m_lvItem == null);
                nodeDatum.m_lvItem = lvItem;
            }

            Console.WriteLine(nCanceled + " unique folders canceled because they're in cloned folders");
            dictUniqueReverse = null;
            InsertSizeMarkers(listLVunique);
            form_lvUnique.Items.Clear();
            form_lvUnique.Items.AddRange(listLVunique.ToArray());
            m_listRootNodes.Sort((x, y) => String.Compare(x.Text, y.Text));
            form_treeView_Browse.Nodes.Clear();
            form_treeView_Browse.Nodes.AddRange(m_listRootNodes.ToArray());

            List<TreeNode> listSameVolDescLength = new List<TreeNode>();

            if (form_treeView_Browse.Nodes.Count > 0)
            {
                new AddTreeToList(m_listTreeNodes, listSameVolDescLength).Go(form_treeView_Browse.Nodes[0]);
            }

            List<ListViewItem> listLVsameVol = new List<ListViewItem>();
            listSameVolDescLength.Sort((x, y) => ((NodeDatum)y.Tag).LengthSubnodes.CompareTo(((NodeDatum)x.Tag).LengthSubnodes));

            foreach (TreeNode treeNode in listSameVolDescLength)
            {
                Debug.Assert(Utilities.StrValid(treeNode.Text));

                ListViewItem lvItem = new ListViewItem(treeNode.Text);
                NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;

                lvItem.Tag = nodeDatum.m_listClones;
                lvItem.ForeColor = Color.Firebrick;
                lvItem.BackColor = treeNode.BackColor;
                listLVsameVol.Add(lvItem);

                if (nodeDatum.m_lvItem != null)
                {
                    nodeDatum.m_lvItem = lvItem;
                }
            }

            InsertSizeMarkers(listLVsameVol);
            form_lvSameVol.Items.Clear();
            form_lvSameVol.Items.AddRange(listLVsameVol.ToArray());
            listSameVolDescLength = null;
            listLVsameVol = null;
            m_listRootNodes = null;
            form_treeView_Browse.Enabled = true;
            form_treeView_Browse.CheckBoxes = m_bCheckboxes;
            m_bBrowseLoaded = true;
            GC.Collect();
        }

        public class LVitemNameComparer : IEqualityComparer<ListViewItem>
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

        void TreeSelectStatusCallback(ListViewItem[] lvItemDetails = null, ListViewItem[] itemArray = null, ListViewItem lvVol = null, bool bSecondComparePane = false, LVitemFileTag lvFileItem = null)
        {
            if (InvokeRequired) { Invoke(new TreeSelectStatusDelegate(TreeSelectStatusCallback), new object[] { lvItemDetails, itemArray, lvVol, bSecondComparePane, lvFileItem }); return; }

            if (lvItemDetails != null)
            {
                if (bSecondComparePane)
                {
                    lock (form_lvDetailVol)
                    {
                        form_lvDetailVol.Items.Clear();
                        form_lvDetailVol.Items.AddRange(lvItemDetails);
                    }
                }
                else
                {
                    lock (form_lvDetail)
                    {
                        form_lvDetail.Items.Clear();
                        form_lvDetail.Items.AddRange(lvItemDetails);
                    }
                }
            }

            if (lvVol != null)
            {
                lock (form_lvDetailVol) { form_lvDetailVol.Items.Add(lvVol); }
            }


            // itemArray

            if (itemArray == null)
            {
                return;
            }

            if (m_bCompareMode == false)
            {
                lock (form_lvFiles)
                {
                    form_lvFiles.Items.Clear();
                    form_lvFiles.Items.AddRange(itemArray);
                }

                return;
            }

            TreeView t1 = bSecondComparePane ? form_treeCompare2 : form_treeCompare1;
            TreeView t2 = bSecondComparePane ? form_treeCompare1 : form_treeCompare2;

            if (t1.SelectedNode == null)
            {
                return;
            }

            Console.Write("A");

            if (lvFileItem.StrCompareDir != t1.SelectedNode.Text)
            {
                // User is navigating faster than this thread.
                Console.WriteLine("Fast: " + lvFileItem.StrCompareDir + "\t\t" + t1.SelectedNode.Text);
                return;
            }

            ListView lv1 = bSecondComparePane ? form_lvFileCompare : form_lvFiles;
            ListView lv2 = bSecondComparePane ? form_lvFiles : form_lvFileCompare;

            lock (lv1)
            {
                lv1.Items.Clear();
                lv1.Items.AddRange(itemArray);
                lv1.Items[0].Tag = lvFileItem;
            }

            Console.Write("B");

            TreeNode treeNode1 = t1.SelectedNode;
            TreeNode treeNode2 = t2.SelectedNode;

            if (treeNode2 == null)
            {
                return;
            }

            Console.Write("C");

            if (treeNode1.Level != treeNode2.Level)
            {
                return;
            }

            if ((treeNode1.Level > 0) &&
                (treeNode1.Text != treeNode2.Text))
            {
                return;
            }

            Console.Write("D");

            if ((lv2.Items.Count > 0) &&
                (((LVitemFileTag)lv2.Items[0].Tag).StrCompareDir != treeNode2.Text))
            {
                Debug.Assert(false);
                return;
            }

            Console.Write("E");

            lock (lv1)
            {
                lock (lv2)
                {
                    LVitemNameComparer.NameItems(lv1.Items);
                    LVitemNameComparer.NameItems(lv2.Items);
                    LVitemNameComparer.MarkItemsFrom1notIn2(lv1, lv2);
                    LVitemNameComparer.MarkItemsFrom1notIn2(lv2, lv1);
                    LVitemNameComparer.SetTopItem(lv1, lv2);
                    LVitemNameComparer.SetTopItem(lv2, lv1);
                }
            }
        }

        void SelectFoundFile()
        {
            // find file results list from NavToFile()
            ListViewItem lvItem = form_lvFiles.FindItemWithText(m_strMaybeFile ?? form_cbNavigate.Text);

            if (lvItem == null)
            {
                return;
            }

            lvItem.Selected = true;
            lvItem.EnsureVisible();
            m_blink.Stop();
            m_blink.Go(Once: true);
        }

        void TreeSelectDoneCallback(bool bSecondComparePane)
        {
            if (InvokeRequired) { Invoke(new TreeSelectDoneDelegate(TreeSelectDoneCallback), new object[] { bSecondComparePane }); return; }

            if (bSecondComparePane)
            {
                m_threadSelectCompare = null;
            }
            else
            {
                m_threadSelect = null;
            }

            SelectFoundFile();
        }

        private void DoTree(bool bKill = false)
        {
            if (m_tree != null)
            {
                if (bKill)
                {
                    m_tree.EndThread();
                    m_bBrowseLoaded = false;
                }
                else
                {
                    return;
                }
            }

            if (m_bBrowseLoaded)
            {
                return;
            }

            form_treeView_Browse.Nodes.Clear();
            form_lvFiles.Items.Clear();
            form_lvFileCompare.Items.Clear();
            form_lvClones.Items.Clear();
            form_lvUnique.Items.Clear();
            m_hashCache.Clear();
            m_listRootNodes = new List<TreeNode>();
            form_treeView_Browse.CheckBoxes = false;    // treeview items will be added to show progress

            if (form_lvVolumesMain.Items.Count <= 0)
            {
                return;
            }

            TreeNode treeNode = new TreeNode("Creating treeview...        ");

            treeNode.NodeFont = new Font(form_treeView_Browse.Font, FontStyle.Bold | FontStyle.Underline);
            form_treeView_Browse.Nodes.Add(treeNode);
            form_treeView_Browse.Enabled = false;
            m_tree = new Tree(form_lvVolumesMain.Items, m_hashCache, 
                new TreeStatusDelegate(TreeStatusCallback), new TreeDoneDelegate(TreeDoneCallback));
            m_tree.DoThreadFactory();
        }

        private void DoTreeSelect(TreeNode treeNode)
        {
            TreeNode rootNode = TreeSelect.GetParentRoot(treeNode);
            String strFile = (String)((RootNodeDatum)rootNode.Tag).StrFile;
            bool bSecondComparePane = (m_bCompareMode && rootNode.Checked);
            Thread threadKill = bSecondComparePane ? m_threadSelectCompare : m_threadSelect;

            if ((threadKill != null) && threadKill.IsAlive)
            {
                threadKill.Abort();
                // no need to null it: gets reassigned below
            }

            TreeSelect treeSelect = new TreeSelect(treeNode, m_hashCache, strFile, m_bCompareMode, bSecondComparePane,
                new TreeSelectStatusDelegate(TreeSelectStatusCallback), new TreeSelectDoneDelegate(TreeSelectDoneCallback));

            Thread thread = treeSelect.DoThreadFactory();

            if (bSecondComparePane)
            {
                m_threadSelectCompare = thread;
            }
            else
            {
                m_threadSelect = thread;
            }
        }
    }
}
