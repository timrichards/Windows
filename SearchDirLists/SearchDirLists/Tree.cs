using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace SearchDirLists
{
    delegate void TreeStatusDelegate(TreeNode rootNode);
    delegate void TreeDoneDelegate();

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

    public partial class Form1 : Form
    {
        private bool m_bThreadingTree = false;
        private bool m_bBrowseLoaded = false;
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

        private void DoTree()
        {
            if (m_bBrowseLoaded)
            {
                return;
            }

            if (m_bThreadingTree)
            {
                //MessageBox.Show("Already in progress.                      ", "Create Browsing Tree");
                return;
            }

            form_treeView_Browse.Nodes.Clear();
            form_LV_Files.Items.Clear();
            form_LV_Detail.Items.Clear();
            m_hashCache.Clear();

            Tree tree = new Tree(form_lv_Volumes.Items, m_hashCache,
                new TreeStatusDelegate(TreeStatusCallback), new TreeDoneDelegate(TreeDoneCallback));

            m_bThreadingTree = true;
            new Thread(new ThreadStart(tree.Go)).Start();
        }
    }
}
