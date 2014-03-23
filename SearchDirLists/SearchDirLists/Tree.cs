﻿using System;
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
    delegate void TreeStatusDelegate(TreeNode rootNode);
    delegate void TreeDoneDelegate();
    delegate void TreeSelectStatusDelegate(ListViewItem[] lvItemDetails = null, ListViewItem[] itemArray = null, ListViewItem lvVol = null, bool bSecondComparePane = false);
    delegate void TreeSelectDoneDelegate(TreeSelect treeSelect);

    class DetailsDatum
    {
        protected long m_nLengthSubnodes = 0;
        protected long m_nNumSubnodeFiles = 0;
        protected long m_nNumSubnodes = 0;
        protected long m_nNumImmediateFiles = 0;

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
        public ListViewItem m_lvCloneItem = null;
    }

    class NodeDatum : DetailsDatum
    {
        long m_nPrevLineNo = 0;
        long m_nLineNo = 0;
        long m_nLength = 0;

        public long PrevlineNo { get { return m_nPrevLineNo; } }
        public long LineNo { get { return m_nLineNo; } }
        public long Length { get { return m_nLength; } }

        public List<TreeNode> m_listClones = null;

        public void SetLVitemHolder(NodeDatum holder) { m_lvCloneItem_ = (holder != null) ? holder.m_lvCloneItem_ : null; }
        NodeDatumLVitemHolder m_lvCloneItem_ = new NodeDatumLVitemHolder();
        public ListViewItem m_lvCloneItem
        {
            get { return (m_lvCloneItem_ != null) ? m_lvCloneItem_.m_lvCloneItem : null; }
            set { if (m_lvCloneItem_ != null) m_lvCloneItem_.m_lvCloneItem = value; }
        }

        public bool bDifferentDrives = false;

        public NodeDatum(long nPrevLineNo, long nLineNo, long nLength) { m_nPrevLineNo = nPrevLineNo; m_nLineNo = nLineNo; m_nLength = nLength; }

        protected NodeDatum(NodeDatum node)
        {
            m_nPrevLineNo = node.m_nPrevLineNo;
            m_nLineNo = node.m_nLineNo;
            m_nLength = node.m_nLength;
        }
    }

    class RootNodeDatum : NodeDatum
    {
        String m_strFile = null;
        String m_strDriveLetter = null;

        public String StrFile { get { return m_strFile; } }
        public String StrDriveLetter { get { return m_strDriveLetter; } }
        public RootNodeDatum(NodeDatum node, String strFile, String strDriveLetter) : base(node)
        {
            m_strFile = strFile;
            m_strDriveLetter = strDriveLetter;      // Drive letter is not currently used at all.
        }
    }

    class Node
    {
        SortedDictionary<String, Node>[] m_nodes = null;
        SortedDictionary<String, Node> subNodes = new SortedDictionary<string, Node>();
        String m_strPath = "";
        static long m_nStaticLineNo = 0;
        public static long FirstLineNo { set { m_nStaticLineNo = value; } }
        long m_nPrevLineNo = 0;
        long m_nLineNo = 0;
        long m_nLength = 0;
        bool bUseShortPath = true;

        public Node(String in_str, long nLineNo, long nLength, SortedDictionary<String, Node>[] nodes_in)
        {
            m_nodes = nodes_in;

            if (in_str.EndsWith(":" + Path.DirectorySeparatorChar) == false)
            {
                Debug.Assert(in_str.Trim().EndsWith(Path.DirectorySeparatorChar.ToString()) == false);
            }

            m_strPath = in_str;
            m_nPrevLineNo = m_nStaticLineNo;

            if (nLineNo == 0)   // new Node("...", 0, 0) when there's no root is a hack 
            {
                nLineNo = m_nStaticLineNo;
            }

            m_nStaticLineNo = m_nLineNo = nLineNo;
            m_nLength = nLength;

            // Path.GetDirectoryName() does not preserve filesystem root

            String strParent = m_strPath;
            int nIndex = strParent.LastIndexOf(Path.DirectorySeparatorChar);

            if (nIndex < 0)
            {
                return;
            }

            strParent = strParent.Remove(nIndex);

            if (m_nodes[0].ContainsKey(strParent) == false)
            {
                m_nodes[0].Add(strParent, new Node(strParent, 0, 0, m_nodes));
            }

            if (m_nodes[0][strParent].subNodes.ContainsKey(m_strPath) == false)
            {
                m_nodes[0][strParent].subNodes.Add(m_strPath, this);
            }
        }

        public TreeNode AddToTree(List<TreeNode> listTreeNodes, String strVolumeName = null)
        {
            int nIndex = m_strPath.LastIndexOf(Path.DirectorySeparatorChar);
            String strShortPath = bUseShortPath ? m_strPath.Substring(nIndex + 1) : m_strPath;

            if (strVolumeName != null)
            {
                bool bNotRedundant = (strVolumeName.EndsWith(strShortPath) == false);

                if (bNotRedundant)
                {
                    strShortPath = strVolumeName + " (" + strShortPath + ")";
                }
                else
                {
                    strShortPath = strVolumeName;
                }
            }

            TreeNode treeNode = null;

            if (subNodes.Count == 1)
            {
                Node subNode = subNodes.Values.First();

                if (this == m_nodes[0].Values.First())
                {
                    // cull all root node single-chains.
                    // m_listTreeNodes is not modified here, but searching for a drive letter works fine in practice so far
                    m_nodes[0] = subNodes;
                    subNode.m_strPath.Insert(0, m_strPath + Path.DirectorySeparatorChar);
                    subNode.bUseShortPath = false;
                    treeNode = subNode.AddToTree(listTreeNodes, strVolumeName);

                    // further down at new NodeDatum...
                    m_nPrevLineNo = subNode.m_nPrevLineNo;
                    m_nLength = subNode.m_nLength;
                    m_nLineNo = subNode.m_nLineNo;
                }
                else
                {
                    treeNode = new TreeNode(strShortPath, new TreeNode[] { subNode.AddToTree(listTreeNodes) });
                }
            }
            else if (subNodes.Count > 1)
            {
                List<TreeNode> treeList = new List<TreeNode>();

                foreach (Node node in subNodes.Values)
                {
                    treeList.Add(node.AddToTree(listTreeNodes));
                }

                treeNode = new TreeNode(strShortPath, treeList.ToArray());
            }
            else
            {
                treeNode = new TreeNode(strShortPath);
            }

            treeNode.Tag = new NodeDatum(m_nPrevLineNo, m_nLineNo, m_nLength);
            listTreeNodes.Add(treeNode);
            return treeNode;
        }
    }

    class DirData
    {
        SortedDictionary<String, Node>[] nodes = new SortedDictionary<string, Node>[] { new SortedDictionary<string, Node>() };
        static TreeStatusDelegate m_callbackStatus = null;

        public DirData(TreeStatusDelegate callbackStatus)
        {
            m_callbackStatus = callbackStatus;
        }

        public void AddToTree(String in_str, long nLineNo, long nLength)
        {
            if (nodes[0].ContainsKey(in_str))
            {
                Node node = nodes[0][in_str];
                Debug.Assert(false);
            }

            in_str = in_str.TrimEnd(Path.DirectorySeparatorChar);
            nodes[0].Add(in_str, new Node(in_str, nLineNo, nLength, nodes));
        }

        public TreeNode AddToTree(List<TreeNode> listTreeNodes, String strVolumeName)
        {
            TreeNode rootNode = nodes[0].Values.First().AddToTree(listTreeNodes, strVolumeName);

            m_callbackStatus(rootNode);
            return rootNode;
        }
    }

    class TreeRootNodeThread : Utilities
    {
        Thread m_thread = null;
        LVvolStrings m_volStrings = null;
        Hashtable m_hashCache = null;
        List<TreeNode> m_listTreeNodes = null;
        static TreeStatusDelegate m_statusCallback = null;

        public TreeRootNodeThread(LVvolStrings volStrings, Hashtable hashCache, List<TreeNode> listTreeNodes, TreeStatusDelegate statusCallback)
        {
            m_volStrings = volStrings;
            m_hashCache = hashCache;
            m_listTreeNodes = listTreeNodes;
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

            String strKey = nodeDatum.LengthSubnodes + " " + nodeDatum.NumSubnodeFiles + " " + nodeDatum.NumSubnodes;

            if (m_hashCache.ContainsKey(strKey))
            {
                ((List<TreeNode>)m_hashCache[strKey]).Add(treeNode);
            }
            else
            {
                List<TreeNode> listNodes = new List<TreeNode>();
                listNodes.Add(treeNode);
                m_hashCache.Add(strKey, listNodes);
            }

            return datum;
        }

        public void Go()
        {
            DateTime dtStart_B = DateTime.Now;

            if (LV_VolumesItemInclude(m_volStrings) == false)
            {
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
                String strLine = File.ReadLines(strSaveAs).Take(1).ToArray()[0];

                if (strLine == m_str_HEADER_01)
                {
                    Console.WriteLine("Converting " + strSaveAs);
                    Convert(strSaveAs);
                    Console.WriteLine("File converted to " + m_str_HEADER);
                }
            }

            {
                String strLine = File.ReadLines(strSaveAs).Take(1).ToArray()[0].Split('\t')[2];

                Debug.Assert(strLine == m_str_HEADER);

                if (strLine != m_str_HEADER)
                {
                    MessageBox.Show("Bad file.");
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

                m_hashCache.Add("driveInfo" + strSaveAs, strBuilder.ToString().Trim());
            }

            {
                String strStart = File.ReadLines(strSaveAs).Where(s => s.StartsWith(m_strLINETYPE_Start)).ToArray()[0];
                Node.FirstLineNo = long.Parse(strStart.Split('\t')[1]);
            }

            DateTime dtStart_A = DateTime.Now;
            List<String> listLines = File.ReadLines(strSaveAs).Where(s => s.StartsWith(m_strLINETYPE_Directory)).ToList();
            Console.WriteLine(strSaveAs + " File.ReadLines took " + (DateTime.Now - dtStart_A).TotalMilliseconds / 1000.0 + " seconds.");

            DirData dirData = new DirData(m_statusCallback);
            String strDriveLetter = null;

            dtStart_A = DateTime.Now;
            foreach (string strLine in listLines)
            {
                String[] strArray = strLine.Split('\t');

                long nLineNo = long.Parse(strArray[1]);

                // directory
                int nIx = nColLENGTH;
                long nLength = 0;

                if ((strArray.Length > nIx) && (strArray[nIx].Length > 0))
                {
                    nLength = long.Parse(strArray[nIx]);
                }

                String strDir = strArray[2];

                dirData.AddToTree(strDir, nLineNo, nLength);

                if (strDriveLetter == null)
                {
                    strDriveLetter = strDir.Substring(0, strDir.IndexOf(":" + Path.DirectorySeparatorChar));
                }
            }
            Console.WriteLine(strSaveAs + " foreach AddToTree() took " + (DateTime.Now - dtStart_A).TotalMilliseconds / 1000.0 + " seconds.");

            dtStart_A = DateTime.Now;
            TreeNode rootNode = dirData.AddToTree(m_listTreeNodes, strVolumeName);
            Console.WriteLine(strSaveAs + " rootNode AddToTree() took " + (DateTime.Now - dtStart_A).TotalMilliseconds / 1000.0 + " seconds.");

            rootNode.Tag = new RootNodeDatum((NodeDatum)rootNode.Tag, strSaveAs, strDriveLetter);
            TreeSubnodeDetails(rootNode);
            Console.WriteLine(strSaveAs + " browsing tree took " + (DateTime.Now - dtStart_B).TotalMilliseconds / 1000.0 + " seconds.");
            Console.WriteLine();
        }

        public void EndThread(bool bKill = false)
        {
            if (m_thread == null)
            {
                return;
            }

            if (bKill && m_thread.IsAlive)
            {
                m_thread.Abort();
            }

            m_thread = null;
        }

        public Thread DoThreadFactory()
        {
            (m_thread = new Thread(new ThreadStart(Go))).Start();

            return m_thread;
        }
    }

    class Tree : Utilities
    {
        List<LVvolStrings> m_list_lvVolStrings = new List<LVvolStrings>();
        Hashtable m_hashCache = null;
        TreeStatusDelegate m_statusCallback = null;
        TreeDoneDelegate m_doneCallback = null;
        List<TreeNode> m_listTreeNodes = null;

        public Tree(ListView.ListViewItemCollection lvVolItems, Hashtable hashCache, List<TreeNode> listTreeNodes,
            TreeStatusDelegate callbackStatus, TreeDoneDelegate callbackDone)
        {
            foreach (ListViewItem lvItem in lvVolItems)
            {
                m_list_lvVolStrings.Add(new LVvolStrings(lvItem));
            }

            m_hashCache = hashCache;
            m_listTreeNodes = listTreeNodes;
            m_statusCallback = callbackStatus;
            m_doneCallback = callbackDone;
        }

        public void Go()
        {
            Console.WriteLine();
            Console.WriteLine("Creating browsing tree.");

            DateTime dtStart = DateTime.Now;
            List<Thread> listThreads = new List<Thread>();

            foreach (LVvolStrings volStrings in m_list_lvVolStrings)
            {
                TreeRootNodeThread treeRoot = new TreeRootNodeThread(volStrings, m_hashCache, m_listTreeNodes, m_statusCallback);
                listThreads.Add(treeRoot.DoThreadFactory());
            }

            foreach (Thread thread in listThreads)
            {
                thread.Join();
            }

            Console.WriteLine(String.Format("Completed browsing tree in {0} seconds.", ((int)(DateTime.Now - dtStart).TotalMilliseconds / 10) / 100.0));
            m_doneCallback();
        }
    }

    // One tag at the first item, so the compare listviewer knows what the first listviewer's state is.
    class LVitemFileTag
    {
        String m_strCompareDir;
        long m_nNumFiles;   // equivalent to number of items in the listviewer. Not currently used

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
        static Hashtable m_hashCache = null;
        static TreeSelectStatusDelegate m_statusCallback = null;
        static TreeSelectDoneDelegate m_doneCallback = null;
        String m_strCompareDir = null;
        static Thread m_staticThread = null;
        Thread m_thread = null;

        public TreeSelect(TreeNode node, Hashtable hashCache,
            TreeSelectStatusDelegate statusCallback, TreeSelectDoneDelegate doneCallback)
        {
            m_treeNode = node;
            m_hashCache = hashCache;
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

        private String Go_A()
        {
            TreeNode nodeParent = GetParentRoot(m_treeNode);
            String strFile = null;
            bool bSecondComparePane = false;

            if (nodeParent.Tag is RootNodeDatum)
            {
                RootNodeDatum rootNode = (RootNodeDatum)nodeParent.Tag;
                strFile = (String)rootNode.StrFile;
            }
            else
            {
                // from compare LV
                strFile = nodeParent.Name;

                if (nodeParent.Checked)
                {
                    bSecondComparePane = true;
                }
            }

            if (File.Exists(strFile) == false)
            {
                Debug.Assert(false);
                return null;
            }

            if ((m_treeNode.Tag is NodeDatum) == false)
            {
                return strFile;
            }

            NodeDatum nodeDatum = (NodeDatum)m_treeNode.Tag;

            if (nodeDatum.LineNo <= 0)
            {
                return strFile;
            }

            long nPrevDir = nodeDatum.PrevlineNo;
            long nLineNo = nodeDatum.LineNo;
            String strLine = File.ReadLines(strFile).Skip((int)nLineNo).Take(1).ToArray()[0];
            String[] strArray = strLine.Split('\t');
            long nIx = 0;
            DateTime dt;


            // Directory detail

            List<ListViewItem> listItems = new List<ListViewItem>();

            nIx = 4; if ((strArray.Length > nIx) && (strArray[nIx].Length > 0)) { listItems.Add(new ListViewItem(new String[] { "Created\t", (dt = DateTime.Parse(strArray[nIx])).ToLongDateString() + ", " + dt.ToLongTimeString() })); }
            nIx = 5; if ((strArray.Length > nIx) && (strArray[nIx].Length > 0)) listItems.Add(new ListViewItem(new String[] { "Modified\t", (dt = DateTime.Parse(strArray[nIx])).ToLongDateString() + ", " + dt.ToLongTimeString() }));
            nIx = 6; if ((strArray.Length > nIx) && (strArray[nIx].Length > 0)) listItems.Add(new ListViewItem(new String[] { "Attributes\t", strArray[nIx] }));
            listItems.Add(new ListViewItem(new String[] { "Immediate Size\t", FormatSize(nodeDatum.Length, true) }));
            nIx = 8; if ((strArray.Length > nIx) && (strArray[nIx].Length > 0)) listItems.Add(new ListViewItem(new String[] { "Error 1\t", strArray[nIx] }));
            nIx = 9; if ((strArray.Length > nIx) && (strArray[nIx].Length > 0)) listItems.Add(new ListViewItem(new String[] { "Error 2\t", strArray[nIx] }));
            listItems.Add(new ListViewItem(new String[] { "# Immediate Files", (nLineNo - nPrevDir - 1).ToString() }));

            // Tree subnode detail

            String NUMFMT = "###,###,###,##0";

            listItems.Add(new ListViewItem(new String[] { "# Immediate Folders", m_treeNode.Nodes.Count.ToString(NUMFMT) }));
            listItems.Add(new ListViewItem(new String[] { "Total # Files", nodeDatum.NumSubnodeFiles.ToString(NUMFMT) }));

            if (nodeDatum.NumSubnodes > 0)
            {
                listItems.Add(new ListViewItem(new String[] { "Total # Folders", nodeDatum.NumSubnodes.ToString(NUMFMT) }));
            }

            listItems.Add(new ListViewItem(new String[] { "Total Size", FormatSize(nodeDatum.LengthSubnodes, true) }));

            m_statusCallback(lvItemDetails: listItems.ToArray(), bSecondComparePane: bSecondComparePane);
            Console.WriteLine(strLine);


            // file list

            if (nPrevDir <= 0)
            {
                return strFile;
            }

            if ((nLineNo - nPrevDir) <= 1)  // dir has no files
            {
                return strFile;
            }

            DateTime dtStart = DateTime.Now;
            List<String> listLines = File.ReadLines(strFile)
                .Skip((int)nPrevDir)
                .Take((int)(nLineNo - nPrevDir - 1))
                .ToList();

            listLines.Sort();

            ListViewItem[] itemArray = new ListViewItem[listLines.Count];
            long nLengthDebug = 0;

            for (int i = 0; i < listLines.Count; ++i)
            {
                String[] strArrayFiles = listLines[i].Split('\t');

                if ((strArrayFiles.Length > nColLENGTH) && (strArrayFiles[nColLENGTH].Length > 0))
                {
                    nLengthDebug += long.Parse(strArrayFiles[nColLENGTH]);
                    strArrayFiles[nColLENGTH] = FormatSize(strArrayFiles[nColLENGTH]);
                }

                itemArray[i] = new ListViewItem(strArrayFiles);
            }

            itemArray[0].Tag = new LVitemFileTag(m_strCompareDir, itemArray.Length);
            m_statusCallback(itemArray: itemArray, bSecondComparePane: bSecondComparePane);

            Debug.Assert(nLengthDebug == nodeDatum.Length);
            return strFile;
        }

        private void Go_B(String strFile)
        {
            // Volume detail

            if (m_hashCache.ContainsKey("driveInfo" + strFile))
            {
                String strDriveInfo = (String)m_hashCache["driveInfo" + strFile];
                String[] arrDriveInfo = strDriveInfo.Split(new String[] { "\r\n", "\n" }, StringSplitOptions.None);

                Debug.Assert(new int[] { 7, 8 }.Contains(arrDriveInfo.Length));
                m_statusCallback(lvVol: new ListViewItem(new String[] { "Available Free Space", FormatSize(arrDriveInfo[0], true) }));
                m_statusCallback(lvVol: new ListViewItem(new String[] { "Drive Format", arrDriveInfo[1] }));
                m_statusCallback(lvVol: new ListViewItem(new String[] { "Drive Type", arrDriveInfo[2] }));
                m_statusCallback(lvVol: new ListViewItem(new String[] { "Name", arrDriveInfo[3] }));
                m_statusCallback(lvVol: new ListViewItem(new String[] { "Root Directory", arrDriveInfo[4] }));
                m_statusCallback(lvVol: new ListViewItem(new String[] { "Total Free Space", FormatSize(arrDriveInfo[nColLENGTH], true) }));
                m_statusCallback(lvVol: new ListViewItem(new String[] { "Total Size", FormatSize(arrDriveInfo[6], true) }));

                if (arrDriveInfo.Length == 8)
                {
                    m_statusCallback(lvVol: new ListViewItem(new String[] { "Volume Label", arrDriveInfo[7] }));
                }
            }
        }

        public void Go()
        {
            String strFile = Go_A();

            if ((m_strCompareDir == null) && (strFile != null))     // not comparing, and there is a source file: do directory detail
            {
                Go_B(strFile);
            }

            m_doneCallback(this);
        }

        public void EndThread(bool bKill = false)
        {
            if (m_thread == null)
            {
                return;
            }

            if (bKill && m_thread.IsAlive)
            {
                m_thread.Abort();
            }

            if (m_thread == m_staticThread)
            {
                m_staticThread = null;
            }

            m_thread = null;
        }

        public void DoThreadFactory(String strCompareDir)
        {
            m_strCompareDir = strCompareDir;

            if ((m_strCompareDir == null) && (m_staticThread != null) && m_staticThread.IsAlive)
            {
                m_staticThread.Abort();
                m_staticThread = null;
            }

            (m_thread = new Thread(new ThreadStart(Go))).Start();

            if ((m_strCompareDir == null) && (m_staticThread == null)) 
            {
                m_staticThread = m_thread;
            }
        }
    }

    public partial class Form1 : Form
    {
        private bool m_bBrowseLoaded = false;
        Hashtable m_hashCache = new Hashtable();
        List<TreeNode> m_listTreeNodes = new List<TreeNode>();
        Thread m_threadTree = null;
        bool m_bThreadingTree = false;

        void TreeStatusCallback(TreeNode rootNode)
        {
            if (InvokeRequired) { Invoke(new TreeStatusDelegate(TreeStatusCallback), new object[] { rootNode }); return; }

            form_treeView_Browse.Nodes.Add(rootNode);
        }

        // If an outer directory is cloned then all the inner ones are part of the outer clone and their clone status is redundant.
        // Breadth-first.
        void FixClones(SortedDictionary<long, List<TreeNode>> dictClones, TreeNode treeNode, TreeNode rootClone = null)
        {
            // neither rootClone nor nMaxLength are used at all (rootClone is used as a bool).
            // provisional.

            NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;
            List<TreeNode> listClones = nodeDatum.m_listClones;
            long nLength = nodeDatum.LengthSubnodes;

            if (nLength < 100 * 1024)
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
                if (dictClones.ContainsKey(nLength))
                {
                    if (dictClones[nLength] != listClones)
                    {
                        while (dictClones.ContainsKey(nLength))
                        {
                            --nLength;
                        }

                        dictClones.Add(nLength, listClones);
                    }
                }
                else
                {
                    dictClones.Add(nLength, listClones);
                }


                // Test to see if clones are on separate volumes.

                TreeNode rootNode = TreeSelect.GetParentRoot(treeNode);

                Debug.Assert(treeNode.ForeColor == Color.Empty);
                treeNode.ForeColor = Color.DarkRed;
                bool bDifferentDrives = false;

                foreach (TreeNode subnode in listClones)
                {
                    if (rootNode != TreeSelect.GetParentRoot(subnode))
                    {
                        treeNode.ForeColor = Color.SteelBlue;
                        bDifferentDrives = true;
                        break;
                    }
                }

                if (bDifferentDrives)
                {
                    foreach (TreeNode subNode in listClones)
                    {
                        ((NodeDatum)subNode.Tag).bDifferentDrives = true;
                    }
                }

                rootClone = treeNode;
            }

            foreach (TreeNode subNode in treeNode.Nodes)
            {
                FixClones(dictClones, subNode, rootClone);
            }
        }

        void TreeDoneCallback()
        {
            if (InvokeRequired) { Invoke(new TreeDoneDelegate(TreeDoneCallback)); return; }

            List<String> strHashList = new List<string>();

            foreach (String str in m_hashCache.Keys)
            {
                strHashList.Add(str);
            }

            foreach (String str in strHashList)
            {
                object obj = m_hashCache[str];

                if ((obj is List<TreeNode>) == false)
                {
                    continue;
                }

                List<TreeNode> listNodes = (List<TreeNode>)obj;

                if (listNodes.Count > 1)
                {
                    foreach (TreeNode treeNode in listNodes)
                    {
                        NodeDatum nodeDatum = ((NodeDatum)treeNode.Tag);

                        if (nodeDatum.LengthSubnodes > 0)
                        {
                            ((NodeDatum)treeNode.Tag).m_listClones = listNodes;
                        }
                    }

                    m_hashCache.Remove(str);
                }
            }

            strHashList.Clear();

            SortedDictionary<long, List<TreeNode>> dictClones = new SortedDictionary<long, List<TreeNode>>();

            foreach (TreeNode treeNode in form_treeView_Browse.Nodes)
            {
                FixClones(dictClones, treeNode);
            }

            IEnumerable<KeyValuePair<long, List<TreeNode>>> dictReverse = dictClones.Reverse();
            dictClones = null;
            List<ListViewItem> listLVitems = new List<ListViewItem>();

            foreach (KeyValuePair<long, List<TreeNode>> listNodes in dictReverse)
            {
                String str_nClones = "";
                int nClones = listNodes.Value.Count;

                if (nClones <= 0)       // precisely from bRemoveClone above
                {
                    continue;
                }

                if (nClones > 2)
                {
                    str_nClones = nClones.ToString("###,###");
                }

                ListViewItem lvItem = new ListViewItem(new String[] { listNodes.Value[0].Text, str_nClones });

                lvItem.Tag = listNodes.Value;

                foreach (TreeNode treeNode in listNodes.Value)
                {
                    NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;

                    nodeDatum.m_lvCloneItem = lvItem;

                    if (nodeDatum.bDifferentDrives == false)
                    {
                        lvItem.ForeColor = Color.DarkRed;
                    }
                }

                listLVitems.Add(lvItem);
            }

            form_LV_Clones.Items.AddRange(listLVitems.ToArray());
            dictReverse = null;
            listLVitems = null;


            // Non-clones (unique)

            Debug.Assert(strHashList.Count == 0);

            foreach (String str in m_hashCache.Keys)
            {
                strHashList.Add(str);
            }

            SortedDictionary<long, TreeNode> dictUnique = new SortedDictionary<long, TreeNode>();

            foreach (String str in strHashList)
            {
                object obj = m_hashCache[str];

                if ((obj is List<TreeNode>) == false)
                {
                    continue;
                }

                List<TreeNode> listNodes = (List<TreeNode>)obj;

                Debug.Assert(listNodes.Count == 1);

                TreeNode treeNode = listNodes[0];
                NodeDatum nodeDatum = ((NodeDatum)treeNode.Tag);
                long nLength = nodeDatum.LengthSubnodes;

                if (dictUnique.ContainsKey(nLength))
                {
                    if (dictUnique[nLength] != treeNode)
                    {
                        while (dictUnique.ContainsKey(nLength))
                        {
                            --nLength;
                        }

                        dictUnique.Add(nLength, treeNode);
                    }
                }
                else
                {
                    dictUnique.Add(nLength, treeNode);
                }

                m_hashCache.Remove(str);
            }

            Debug.Assert(m_hashCache.Count == form_LV_VolumesMain.Items.Count);     // fun trivia for now

            IEnumerable<KeyValuePair<long, TreeNode>> dictUniqueReverse = dictUnique.Reverse();
            List<ListViewItem> listLVunique = new List<ListViewItem>();

            foreach (KeyValuePair<long, TreeNode> listNodes in dictUniqueReverse)
            {
                TreeNode treeNode = listNodes.Value;

                if (((NodeDatum)treeNode.Tag).LengthSubnodes < (100 * 1024))
                {
                    break;
                }

                ListViewItem lvItem = new ListViewItem(treeNode.Text);

                lvItem.Tag = treeNode;
                listLVunique.Add(lvItem);

                NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;

                Debug.Assert(nodeDatum.m_lvCloneItem == null);
                nodeDatum.m_lvCloneItem = lvItem;
            }

            form_lv_Unique.Items.AddRange(listLVunique.ToArray());

            m_bThreadingTree = false;
            m_bBrowseLoaded = true;
        }

        public class LVitemNameComparer : IEqualityComparer<ListViewItem>
        {

            public bool Equals(ListViewItem x, ListViewItem y)
            {
                //Check whether the objects are the same object.  
                if (Object.ReferenceEquals(x, y)) return true;

                //Check whether the objects' properties are equal.  
                return x != null && y != null && x.Name.Equals(y.Name);
            }

            public int GetHashCode(ListViewItem obj)
            {
                //Get hash code for the Name field if it is not null.  
                return obj.Name == null ? 0 : obj.Name.GetHashCode();
            }

            public static void NameItems(ListView.ListViewItemCollection list)
            {
                foreach (ListViewItem item in list)
                {
                    item.Name = item.SubItems[1].Text;

                    if (item.SubItems.Count > Utilities.nColLENGTH)
                    {
                        item.Name += item.SubItems[Utilities.nColLENGTH].Text;      // name + size
                    }
                }
            }

            public static void MarkItemsFrom1notIn2(ListView lv1, ListView lv2)
            {
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
                if (lv1.TopItem.Index > 0)
                {
                    return;
                }

                int nIx = lv2.TopItem.Index - (lv2.Items.Count - lv1.Items.Count);

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
        
        void TreeSelectStatusCallback(ListViewItem[] lvItemDetails = null, ListViewItem[] itemArray = null, ListViewItem lvVol = null, bool bSecondComparePane = false)
        {
            if (InvokeRequired) { Invoke(new TreeSelectStatusDelegate(TreeSelectStatusCallback), new object[] { lvItemDetails, itemArray, lvVol, bSecondComparePane }); return; }

            if (lvItemDetails != null)
            {
                if (bSecondComparePane)
                {
                    form_LV_DetailVol.Items.AddRange(lvItemDetails);
                }
                else
                {
                    form_LV_Detail.Items.AddRange(lvItemDetails);
                }
            }

            if (lvVol != null)
            {
                form_LV_DetailVol.Items.Add(lvVol);
            }


            // itemArray

            ListView lv1 = bSecondComparePane ? form_lv_FileCompare : form_LV_Files;
            ListView lv2 = bSecondComparePane ? form_LV_Files : form_lv_FileCompare;

            if (itemArray == null)
            {
                return;
            }

            lv1.Items.AddRange(itemArray);

            // Compare file listings in itemArray

            if ((lv1.Items.Count <= 0) || (lv2.Items.Count <= 0))
            {
                return;
            }

            LVitemFileTag tag1 = (LVitemFileTag)lv1.Items[0].Tag;
            LVitemFileTag tag2 = (LVitemFileTag)lv2.Items[0].Tag;

            if ((tag1.StrCompareDir != tag2.StrCompareDir))
            {
                return;
            }

            LVitemNameComparer.NameItems(lv1.Items);
            LVitemNameComparer.NameItems(lv2.Items);
            LVitemNameComparer.MarkItemsFrom1notIn2(lv1, lv2);
            LVitemNameComparer.MarkItemsFrom1notIn2(lv2, lv1);
            LVitemNameComparer.SetTopItem(lv1, lv2);
            LVitemNameComparer.SetTopItem(lv2, lv1);
        }

        void TreeSelectDoneCallback(TreeSelect treeSelect)
        {
            if (InvokeRequired) { Invoke(new TreeSelectDoneDelegate(TreeSelectDoneCallback), new object[] { treeSelect } ); return; }

            treeSelect.EndThread();
        }

        private void DoTree(bool bKill = false)
        {
            if (m_bBrowseLoaded)
            {
                return;
            }

            if (m_bThreadingTree)
            {
                if (bKill)
                {
                    m_threadTree.Abort();
                    m_bThreadingTree = false;
                }
                else
                {
                    return;
                }
            }

            form_treeView_Browse.Nodes.Clear();
            form_LV_Files.Items.Clear();
            form_lv_FileCompare.Items.Clear();
            form_LV_Detail.Items.Clear();
            form_LV_DetailVol.Items.Clear();
            form_LV_Clones.Items.Clear();
            m_hashCache.Clear();

            Tree tree = new Tree(form_LV_VolumesMain.Items, m_hashCache, m_listTreeNodes, 
                new TreeStatusDelegate(TreeStatusCallback), new TreeDoneDelegate(TreeDoneCallback));

            m_bThreadingTree = true;
            (m_threadTree = new Thread(new ThreadStart(tree.Go))).Start();
        }

        private void DoTreeSelect(TreeNode node, String strCompareDir = null)
        {
            TreeSelect treeSelect = new TreeSelect(node, m_hashCache,
                new TreeSelectStatusDelegate(TreeSelectStatusCallback), new TreeSelectDoneDelegate(TreeSelectDoneCallback));

            treeSelect.DoThreadFactory(strCompareDir);
        }
    }
}
