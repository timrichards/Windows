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
    delegate void TreeStatusDelegate(TreeNode rootNode);
    delegate void TreeDoneDelegate();
    delegate void TreeSelectStatusDelegate(ListViewItem lvItemDetails = null, ListViewItem[] itemArray = null);
    delegate void TreeSelectDoneDelegate();

    class NodeDatum
    {
        long m_nPrevLineNo = 0;
        long m_nLineNo = 0;

        public long PrevlineNo { get { return m_nPrevLineNo; } }
        public long LineNo { get { return m_nLineNo; } }
        public NodeDatum(long nPrevLineNo, long nLineNo) { m_nPrevLineNo = nPrevLineNo; m_nLineNo = nLineNo; }
    }

    class Node
    {
        static SortedDictionary<String, Node> nodes = null;
        SortedDictionary<String, Node> subNodes = new SortedDictionary<string, Node>();
        String m_strPath = "";
        static long m_nStaticLineNo = 0;
        long m_nPrevLineNo = 0;
        long m_nLineNo = 0;
        bool bUseShortPath = true;

        public Node(String in_str, long nLineNo)
        {
            if (in_str.EndsWith(":" + Path.DirectorySeparatorChar) == false)
            {
                Debug.Assert(in_str.Trim().EndsWith(Path.DirectorySeparatorChar.ToString()) == false);
            }

            m_strPath = in_str;
            m_nPrevLineNo = m_nStaticLineNo;
            m_nStaticLineNo = m_nLineNo = nLineNo;

            // Path.GetDirectoryName() does not preserve filesystem root

            String strParent = m_strPath;
            int nIndex = strParent.LastIndexOf(Path.DirectorySeparatorChar);

            if (nIndex < 0)
            {
                return;
            }

            strParent = strParent.Remove(nIndex);

            if (nodes.ContainsKey(strParent) == false)
            {
                nodes.Add(strParent, new Node(strParent, 0));
            }

            if (nodes[strParent].subNodes.ContainsKey(m_strPath) == false)
            {
                nodes[strParent].subNodes.Add(m_strPath, this);
            }
        }

        public static void SetRootNode(SortedDictionary<String, Node> node)
        {
            nodes = node;
        }

        public TreeNode AddToTree(String strVolumeName = null)
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

                if (this == nodes.Values.First())
                {
                    // cull all root node single-chains.
                    SetRootNode(subNodes);
                    subNode.m_strPath.Insert(0, m_strPath + Path.DirectorySeparatorChar);
                    subNode.bUseShortPath = false;
                    treeNode = subNode.AddToTree(strVolumeName);
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

            treeNode.Tag = new NodeDatum(m_nPrevLineNo, m_nLineNo);
            return treeNode;
        }
    }

    class DirData
    {
        SortedDictionary<String, Node> nodes = new SortedDictionary<string, Node>();
        static TreeStatusDelegate m_callbackStatus = null;

        public DirData(TreeStatusDelegate callbackStatus)
        {
            Node.SetRootNode(nodes);
            m_callbackStatus = callbackStatus;
        }

        public void AddToTree(String in_str, long nLineNo)
        {
            if (nodes.ContainsKey(in_str))
            {
                Node node = nodes[in_str];
                Debug.Assert(false);
            }

            while (in_str.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                in_str = in_str.Remove(in_str.LastIndexOf(Path.DirectorySeparatorChar));
            }

            nodes.Add(in_str, new Node(in_str, nLineNo));
        }

        public TreeNode AddToTree(String strVolumeName)
        {
            TreeNode rootNode = nodes.Values.First().AddToTree(strVolumeName);

            m_callbackStatus(rootNode);
            return rootNode;
        }
    }

    class Tree : Utilities
    {
        List<LVvolStrings> m_list_lvVolStrings = new List<LVvolStrings>();
        Hashtable m_hashCache;
        TreeStatusDelegate m_callbackStatus;
        TreeDoneDelegate m_callbackDone;

        public Tree(ListView.ListViewItemCollection lvItems, Hashtable hashCache, 
            TreeStatusDelegate callbackStatus, TreeDoneDelegate callbackDone)
        {
            foreach (ListViewItem lvItem in lvItems)
            {
                m_list_lvVolStrings.Add(new LVvolStrings(lvItem));
            }

            m_hashCache = hashCache;
            m_callbackStatus = callbackStatus;
            m_callbackDone = callbackDone;
        }

        public void Go()
        {
            Console.WriteLine();
            Console.WriteLine("Creating browsing tree.");

            DateTime dtStart = DateTime.Now;

            foreach (LVvolStrings volStrings in m_list_lvVolStrings)
            {
                if (LV_VolumesItemInclude(volStrings) == false)
                {
                    continue;
                }

                String strVolumeName = volStrings.VolumeName;
                String strPath = volStrings.Path;
                String strSaveAs = volStrings.SaveAs;

                if (FormatPath(ref strPath, ref strSaveAs, false) == false)
                {
                    return;
                }

                using (StreamReader file = new StreamReader(strSaveAs))
                {
                    String line = "";
                    DirData dirData = new DirData(m_callbackStatus);
                    long nLineNo = -1;

                    while ((line = file.ReadLine()) != null)
                    {
                        ++nLineNo;

                        StringBuilder strDriveInfo = new StringBuilder();

                        if (line == Utilities.m_str_DRIVE)
                        {
                            for (int i = 0; i < 8; ++i)
                            {
                                strDriveInfo.AppendLine(file.ReadLine());
                                ++nLineNo;
                            }

                            m_hashCache.Add("driveInfo" + strSaveAs, strDriveInfo.ToString().Trim());
                            continue;
                        }

                        if (line.StartsWith(Utilities.m_str_END))
                        {
                            break;  // all done
                        }

                        if (line.Contains('\t') == false)
                        {
                            continue;
                        }

                        if (line.Contains(":" + Path.DirectorySeparatorChar) == false)
                        {
                            continue;
                        }

                        String[] strArray = line.Split('\t');
                        String strNew = strArray[0];

                        if (strNew.Length <= 0)
                        {
                            continue;
                        }

                        // directory
                        dirData.AddToTree(strNew, nLineNo);
                    }

                    dirData.AddToTree(strVolumeName).Tag = strSaveAs;
                }
            }

            Console.WriteLine(String.Format("Completed browsing tree in {0} seconds.", ((int)(DateTime.Now - dtStart).TotalMilliseconds / 10) / 100.0));
            m_callbackDone();
        }
    }

    class TreeSelect
    {
        TreeNode m_treeNode = null;
        static Hashtable m_hashCache = null;
        static TreeSelectStatusDelegate m_statusCallback = null;
        static TreeSelectDoneDelegate m_doneCallback = null;

        public TreeSelect(TreeNode node, Hashtable hashCache,
            TreeSelectStatusDelegate statusCallback, TreeSelectDoneDelegate doneCallback)
        {
            m_treeNode = node;
            m_hashCache = hashCache;
            m_statusCallback = statusCallback;
            m_doneCallback = doneCallback;
        }

        public void Go()
        {
            Go_A();
            m_doneCallback();
        }

        private void Go_A()
        {
            TreeNode nodeParent = m_treeNode;

            while (nodeParent.Parent != null)
            {
                nodeParent = nodeParent.Parent;
            }

            Debug.Assert(nodeParent.Tag is String);

            String strFile = (String)nodeParent.Tag;

            if (File.Exists(strFile) == false)
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
            String strLine = File.ReadLines(strFile).Skip((int)nLineNo).Take(1).ToArray()[0];
            String[] strArray = strLine.Split('\t');
            long nIx = 0;
            DateTime dt;


            // Directory detail

            nIx = 2; if ((strArray.Length > nIx) && (strArray[nIx].Length > 0)) { m_statusCallback(new ListViewItem(new String[] { "Created\t", (dt = DateTime.Parse(strArray[nIx])).ToLongDateString() + ", " + dt.ToLongTimeString() })); }
            nIx = 3; if ((strArray.Length > nIx) && (strArray[nIx].Length > 0)) m_statusCallback(new ListViewItem(new String[] { "Modified\t", (dt = DateTime.Parse(strArray[nIx])).ToLongDateString() + ", " + dt.ToLongTimeString() }));
            nIx = 4; if ((strArray.Length > nIx) && (strArray[nIx].Length > 0)) m_statusCallback(new ListViewItem(new String[] { "Attributes\t", strArray[nIx] }));
            nIx = 5;

            long nLengthDebug_A = 0;

            if ((strArray.Length > nIx) && (strArray[nIx].Length > 0))
            {
                Debug.Assert(long.TryParse(strArray[nIx], out nLengthDebug_A));
                m_statusCallback(new ListViewItem(new String[] { "Size\t", Utilities.FormatSize(strArray[nIx], true) }));
            }

            nIx = 6; if ((strArray.Length > nIx) && (strArray[nIx].Length > 0)) m_statusCallback(new ListViewItem(new String[] { "Error 1\t", strArray[nIx] }));
            nIx = 7; if ((strArray.Length > nIx) && (strArray[nIx].Length > 0)) m_statusCallback(new ListViewItem(new String[] { "Error 2\t", strArray[nIx] }));

            Console.WriteLine(strLine);


            // Volume detail

            if (m_hashCache.ContainsKey("driveInfo" + strFile))
            {
                String strDriveInfo = (String)m_hashCache["driveInfo" + strFile];
                String[] arrDriveInfo = strDriveInfo.Split(new String[] { "\r\n", "\n" }, StringSplitOptions.None);

                Debug.Assert(new int[] { 7, 8 }.Contains(arrDriveInfo.Length));
                m_statusCallback(new ListViewItem());

                ListViewItem lvItem = new ListViewItem("Volume detail");

                lvItem.BackColor = Color.DarkGray;
                lvItem.ForeColor = Color.White;
                m_statusCallback(lvItem);
                m_statusCallback(new ListViewItem(new String[] { "Available Free Space", Utilities.FormatSize(arrDriveInfo[0], true) }));
                m_statusCallback(new ListViewItem(new String[] { "Drive Format", arrDriveInfo[1] }));
                m_statusCallback(new ListViewItem(new String[] { "Drive Type", arrDriveInfo[2] }));
                m_statusCallback(new ListViewItem(new String[] { "Name", arrDriveInfo[3] }));
                m_statusCallback(new ListViewItem(new String[] { "Root Directory", arrDriveInfo[4] }));
                m_statusCallback(new ListViewItem(new String[] { "Total Free Space", Utilities.FormatSize(arrDriveInfo[5], true) }));
                m_statusCallback(new ListViewItem(new String[] { "Total Size", Utilities.FormatSize(arrDriveInfo[6], true) }));

                if (arrDriveInfo.Length == 8)
                {
                    m_statusCallback(new ListViewItem(new String[] { "Volume Label", arrDriveInfo[7] }));
                }
            }


            // file list

            if (nPrevDir <= 0)
            {
                return;
            }

            if ((nLineNo - nPrevDir) <= 1)  // dir has no files
            {
                return;
            }

            long nLengthDebug = 0;

            DateTime dtStart = DateTime.Now;
            List<String> listLines = File.ReadLines(strFile)
                .Skip((int)nPrevDir + 1)
                .Take((int)(nLineNo - nPrevDir - 1))
                .ToList();

            listLines.Sort();

            ListViewItem[] itemArray = new ListViewItem[listLines.Count];

            for (int i = 0; i < listLines.Count; ++i)
            {
                strArray = listLines[i].Split('\t');

                if ((strArray.Length > 5) && (strArray[5].Length > 0))
                {
                    nLengthDebug += long.Parse(strArray[5]);
                    strArray[5] = Utilities.FormatSize(strArray[5]);
                }

                itemArray[i] = new ListViewItem(strArray);
            }

            m_statusCallback(itemArray: itemArray);
            m_statusCallback(new ListViewItem(new String[] { "# Files", (nLineNo - nPrevDir - 1).ToString() }));

            Debug.Assert(nLengthDebug == nLengthDebug_A);
        }
    }

    public partial class Form1 : Form
    {
        private bool m_bThreadingTree = false;
        private bool m_bThreadingTreeSelect = false;
        private bool m_bBrowseLoaded = false;
        Thread m_threadTree;
        Thread m_threadSelect;
        Hashtable m_hashCache = new Hashtable();

        void TreeStatusCallback(TreeNode rootNode)
        {
            if (InvokeRequired) { Invoke(new TreeStatusDelegate(TreeStatusCallback), new object[] { rootNode }); return; }

            form_treeView_Browse.Nodes.Add(rootNode);
        }

        void TreeDoneCallback()
        {
            if (InvokeRequired) { Invoke(new TreeDoneDelegate(TreeDoneCallback)); return; }

            m_bThreadingTree = false;
            m_bBrowseLoaded = true;
        }

        void TreeSelectStatusCallback(ListViewItem lvItemDetails = null, ListViewItem[] itemArray = null)
        {
            if (InvokeRequired) { Invoke(new TreeSelectStatusDelegate(TreeSelectStatusCallback), new object[] { lvItemDetails, itemArray }); return;  }

            if (lvItemDetails != null)
            {
                form_LV_Detail.Items.Add(lvItemDetails);
            }

            if (itemArray != null)
            {
                form_LV_Files.Items.AddRange(itemArray);
            }
        }

        void TreeSelectDoneCallback()
        {
            if (InvokeRequired) { Invoke(new TreeSelectDoneDelegate(TreeSelectDoneCallback)); return; }

            m_bThreadingTreeSelect = false;
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
            form_LV_Detail.Items.Clear();
            m_hashCache.Clear();

            Tree tree = new Tree(form_lv_Volumes.Items, m_hashCache,
                new TreeStatusDelegate(TreeStatusCallback), new TreeDoneDelegate(TreeDoneCallback));

            m_bThreadingTree = true;
            (m_threadTree = new Thread(new ThreadStart(tree.Go))).Start();
        }

        private void DoTreeSelect(TreeNode node)
        {
            if (m_bThreadingTreeSelect && m_threadSelect.IsAlive)
            {
                m_threadSelect.Abort();
            }

            TreeSelect treeSelect = new TreeSelect(node, m_hashCache,
                new TreeSelectStatusDelegate(TreeSelectStatusCallback), new TreeSelectDoneDelegate(TreeSelectDoneCallback));

            m_bThreadingTreeSelect = true;
            (m_threadSelect = new Thread(new ThreadStart(treeSelect.Go))).Start();
        }
    }
}
