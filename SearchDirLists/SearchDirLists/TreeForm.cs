using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using System.Drawing;

namespace SearchDirLists
{
    partial class Form1 : Form
    {
        SortedDictionary<HashKey, List<TreeNode>> m_dictNodes = new SortedDictionary<HashKey,List<TreeNode>>();
        Dictionary<String, String> m_dictDriveInfo = new Dictionary<string,string>();
        List<TreeNode> m_listTreeNodes = new List<TreeNode>();
        List<TreeNode> m_listRootNodes = new List<TreeNode>();
        List<ListViewItem> m_list_lvIgnore = new List<ListViewItem>();
        Tree m_tree = null;
        Thread m_threadCorrelate = null;
        Thread m_threadSelect = null;
        Thread m_threadSelectCompare = null;

        void TreeCleanup()
        {
            m_tree = null;
            m_threadCorrelate = null;
            m_list_lvIgnore.Clear();
        }

        void LoadIgnoreList()
        {
            Utilities.Assert(1304.5303, m_list_lvIgnore.Count == 0);

            foreach (ListViewItem lvItem in form_lvIgnoreList.Items)
            {
                ListViewItem lvItem_A = (ListViewItem)lvItem.Clone();

                lvItem_A.Tag = lvItem;
                m_list_lvIgnore.Add(lvItem_A);
            }
        }

        void DoCorrelation()
        {
            Utilities.Assert(1304.5304, m_listTreeNodes.Count == 0);
            Utilities.Assert(1304.5305, InvokeRequired);
            Utilities.CheckAndInvoke(this, new Action(LoadIgnoreList));

            Correlate correlate = new Correlate(form_treeView_Browse, m_dictNodes,
                form_lvClones, form_lvSameVol, form_lvUnique,
                m_listRootNodes, m_listTreeNodes, m_bCheckboxes,
                m_list_lvIgnore, form_chkLoose.Checked);
            DateTime dtStart = DateTime.Now;

            correlate.Step1_OnThread();
            Console.WriteLine("Step1_OnThread " + (DateTime.Now - dtStart).TotalMilliseconds / 1000.0 + " seconds."); dtStart = DateTime.Now;

            if (AppExit)
            {
                TreeCleanup();
                return;
            }

            m_bPutPathInFindEditBox = true;
            Utilities.CheckAndInvoke(this, new Action(correlate.Step2_OnForm));
            Console.WriteLine("Step2_OnForm " + (DateTime.Now - dtStart).TotalMilliseconds / 1000.0 + " seconds."); dtStart = DateTime.Now;
            correlate = null;
            TreeCleanup();
            GC.Collect();
        }

        void TreeStatusCallback(TreeNode rootNode, LVvolStrings volStrings)
        {
            if (AppExit || (m_tree == null) || (m_tree.IsAborted))
            {
                TreeCleanup();
                return;
            }

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

        void TreeSelectStatusCallback(ListViewItem[] lvItemDetails = null, ListViewItem[] itemArray = null, ListViewItem lvVol = null, bool bSecondComparePane = false, LVitemFileTag lvFileItem = null)
        {
            if (AppExit)
            {
                return;
            }

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
                Utilities.Assert(1304.5301, false);
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
            m_blink.Go(lvItem: lvItem, Once: true);
        }

        void TreeSelectDoneCallback(bool bSecondComparePane)
        {
            if (AppExit)
            {
                return;
            }

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

        void KillTreeBuilder(bool bJoin = false)
        {
            if (m_tree != null)
            {
                m_tree.EndThread(bJoin);
                form_treeView_Browse.Nodes.Clear();
            }
        }

        void DoTree(bool bKill = false)
        {
            if (m_tree != null)
            {
                if (bKill)
                {
                    m_tree.EndThread(bJoin: true);
                    form_treeView_Browse.Nodes.Clear();
                    TreeCleanup();
                }
                else
                {
                    return;
                }
            }

            if (m_threadCorrelate != null)
            {
                if (bKill)
                {
                    if (m_threadCorrelate.IsAlive)
                    {
                        m_threadCorrelate.Abort();
                    }

                    TreeCleanup();
                }
                else
                {
                    return;
                }
            }

            foreach (ListView lv in new ListView[] { form_lvClones, form_lvSameVol, form_lvUnique })
            {
                lv.Tag = SortOrder.None;
                lv.SetSortIcon(0, SortOrder.None);
            }

            form_lvClones.Items.Clear();
            form_lvSameVol.Items.Clear();
            form_lvUnique.Items.Clear();

            if (bKill)
            {
                m_dictNodes.Clear();
            }

            if (m_dictNodes.Count == 0)     // .Clear() to signal recreate. Ignore list only requires recorrelation
            {                               // this works because m_tree is not null during recreate.
                m_dictDriveInfo.Clear();
                m_listRootNodes.Clear();
                m_listTreeNodes.Clear();
                m_nodeCompare1 = null;
                m_nodeCompare2 = null;
                m_arrayTreeFound = null;
                dictCompareDiffs.Clear();
                m_listTreeNodes_Compare1.Clear();
                m_listTreeNodes_Compare2.Clear();
                m_listHistory.Clear();
                m_nIxHistory = -1;
                form_lvFiles.Items.Clear();
                form_lvFileCompare.Items.Clear();
                form_lvDetail.Items.Clear();
                form_lvDetailVol.Items.Clear();
                form_tmapUserCtl.Clear();
                form_colFilename.Text = m_strColFilesOrig;
                form_colDirDetail.Text = m_strColDirDetailOrig;
                form_colVolDetail.Text = m_strColVolDetailOrig;

                if (form_lvVolumesMain.Items.Count <= 0)
                {
                    return;
                }

                TreeNode treeNode = new TreeNode("Creating treeview...        ");

                treeNode.NodeFont = new Font(form_treeView_Browse.Font, FontStyle.Bold | FontStyle.Underline);
                form_treeView_Browse.Nodes.Clear();
                form_treeView_Browse.Nodes.Add(treeNode);
                form_treeView_Browse.CheckBoxes = false;    // treeview items are faked to show progress
                form_treeView_Browse.Enabled = false;

                Utilities.Assert(1304.5302, m_listRootNodes.Count == 0);

                m_tree = new Tree(form_lvVolumesMain.Items, m_dictNodes, m_dictDriveInfo,
                    new TreeStatusDelegate(TreeStatusCallback), new TreeDoneDelegate(DoCorrelation));
                m_tree.DoThreadFactory();
            }
            else
            {
                foreach (TreeNode treeNode in m_listTreeNodes)
                {
                    treeNode.ForeColor = Color.Empty;
                    treeNode.BackColor = Color.Empty;

                    NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;

                    if (nodeDatum == null)
                    {
                        Utilities.Assert(1304.53025, false);
                        continue;
                    }

                    nodeDatum.m_lvItem = null;
                    nodeDatum.m_listClones = null;
                    nodeDatum.m_bDifferentVols = false;
                }

                m_listTreeNodes.Clear();
                m_threadCorrelate = new Thread(new ThreadStart(DoCorrelation));
                m_threadCorrelate.IsBackground = true;
                m_threadCorrelate.Start();
            }
        }

        void DoTreeSelect(TreeNode treeNode)
        {
            TreeNode rootNode = treeNode.Root();
            String strFile = (String)((RootNodeDatum)rootNode.Tag).StrFile;
            bool bSecondComparePane = (m_bCompareMode && rootNode.Checked);
            Thread threadKill = bSecondComparePane ? m_threadSelectCompare : m_threadSelect;

            if ((threadKill != null) && threadKill.IsAlive)
            {
                threadKill.Abort();
            }

            m_threadSelectCompare = null;
            m_threadSelect = null;

            TreeSelect treeSelect = new TreeSelect(treeNode, m_dictNodes, m_dictDriveInfo,
                strFile, m_bCompareMode, bSecondComparePane,
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
