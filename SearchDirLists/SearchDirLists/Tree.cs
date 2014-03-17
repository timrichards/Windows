using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace SearchDirLists
{
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
        TreeView form_treeView_Browse = null;
        SortedDictionary<String, Node> nodes = new SortedDictionary<string, Node>();

        public DirData(TreeView ctl)
        {
            form_treeView_Browse = ctl;
            Node.SetRootNode(nodes);
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

            form_treeView_Browse.Nodes.Add(rootNode);
            return rootNode;
        }
    }

    public partial class Form1 : Form
    {
        private void DoTree()
        {
            if (m_bBrowseLoaded)
            {
                return;
            }

            //    new Thread(new ThreadStart(CreateBrowsingTree)).Start();
            //}

            //private void CreateBrowsingTree()
            //{
            form_treeView_Browse.Nodes.Clear();
            form_LV_Files.Items.Clear();
            form_LV_Detail.Items.Clear();
            m_hashCache.Clear();

            Console.WriteLine();
            Console.WriteLine("Creating browsing tree.");

            DateTime dtStart = DateTime.Now;

            foreach (ListViewItem lvItem in form_lv_Volumes.Items)
            {
                if (LV_VolumesItemInclude(lvItem) == false)
                {
                    continue;
                }

                form_cb_VolumeName.Text = lvItem.SubItems[0].Text;
                form_cb_Path.Text = lvItem.SubItems[1].Text;
                form_cb_SaveAs.Text = lvItem.SubItems[2].Text;

                if (SaveFields(false) == false)
                {
                    return;
                }

                using (StreamReader file = new StreamReader(m_strSaveAs))
                {
                    String line = "";
                    DirData dirData = new DirData(form_treeView_Browse);
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

                            m_hashCache.Add("driveInfo" + m_strSaveAs, strDriveInfo.ToString().Trim());
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

                    dirData.AddToTree(m_strVolumeName).Tag = m_strSaveAs;
                }
            }

            Console.WriteLine(String.Format("Completed browsing tree in {0} seconds.", ((int)(DateTime.Now - dtStart).TotalMilliseconds / 10) / 100.0));
            m_bBrowseLoaded = true;
        }
    }
}
