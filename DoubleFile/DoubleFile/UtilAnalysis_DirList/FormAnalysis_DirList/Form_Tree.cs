﻿using System.Windows.Forms;
using System.Drawing;
using System;
using System.Threading;

namespace DoubleFile
{
    partial class FormAnalysis_DirList
    {
        internal void ClearMem_TreeForm()
        {
            gd.ClearMem_TreeForm();

            foreach (ListViewItem lvItem in form_lvCopyScratchpad.Items)
            {
                lvItem.Tag = null;
            }
        }

        void TreeDoneCallback()
        {
            DoCollation();

            UtilAnalysis_DirList.CheckAndInvoke(this, new Action(() =>
            {
                ListView lvFake = new ListView();

                foreach (ListViewItem lvItem in form_lvCopyScratchpad.Items)
                {
                    lvFake.Items.Add((ListViewItem)lvItem.Clone());
                }

                form_lvCopyScratchpad.Items.Clear();
                LoadCopyScratchPad(lvFake);
            }));
        }

        void DoCollation()
        {
            if (gd.m_listRootNodes.Count <= 0)
            {
                UtilAnalysis_DirList.CheckAndInvoke(this, new Action(() =>
                {
                    form_treeViewBrowse.Nodes.Clear();
                }));
                
                return;
            }

            MBox.Assert(1304.5304, gd.m_listTreeNodes.Count == 0);
            MBox.Assert(1304.5305, InvokeRequired);

            UtilAnalysis_DirList.CheckAndInvoke(this, new Action(() =>
            {
                MBox.Assert(1304.5306, gd.m_listLVignore.Count == 0);

                foreach (ListViewItem lvItem in form_lvIgnoreList.Items)
                {
                    ListViewItem lvItem_A = (ListViewItem)lvItem.Clone();

                    lvItem_A.Tag = lvItem;
                    gd.m_listLVignore.Add(lvItem_A);
                }
            }));

            Collate collate = new Collate(gd.m_dictNodes,
                form_treeViewBrowse,
                form_lvClones, form_lvSameVol, form_lvUnique,
                gd.m_listRootNodes, gd.m_listTreeNodes, gd.m_bCheckboxes,
                gd.m_listLVignore, form_chkLoose.Checked);
            DateTime dtStart = DateTime.Now;

            collate.Step1_OnThread();
            UtilAnalysis_DirList.WriteLine("Step1_OnThread " + (DateTime.Now - dtStart).TotalMilliseconds / 1000.0 + " seconds."); dtStart = DateTime.Now;

            if (GlobalData.Instance.FormAnalysis_DirList_Closing)
            {
                gd.TreeCleanup();
                return;
            }

            gd.m_bPutPathInFindEditBox = true;
            UtilAnalysis_DirList.CheckAndInvoke(this, new Action(collate.Step2_OnForm));
            UtilAnalysis_DirList.WriteLine("Step2_OnForm " + (DateTime.Now - dtStart).TotalMilliseconds / 1000.0 + " seconds."); dtStart = DateTime.Now;
            collate = null;
            gd.TreeCleanup();
            GC.Collect();

            int nNodeCount = form_treeViewBrowse.GetNodeCount(includeSubTrees: true);
            int nNodeCount_A = UtilAnalysis_DirList.CountNodes((TreeNode)form_treeViewBrowse.Nodes[0]);

            MBox.Assert(1304.5307, UtilAnalysis_DirList.CountNodes(gd.m_listRootNodes) == nNodeCount);
            MBox.Assert(1304.5308, gd.m_listTreeNodes.Count == nNodeCount);

            if (Form.ActiveForm == null)
            {
                FlashWindow.Go();
            }
        }

        internal void TreeStatusCallback(LVitem_ProjectVM volStrings, TreeNode rootNode = null, bool bError = false)
        {
            if (GlobalData.Instance.FormAnalysis_DirList_Closing || (gd.m_tree == null) || (gd.m_tree.IsAborted))
            {
                gd.TreeCleanup();
                return;
            }

            UtilAnalysis_DirList.CheckAndInvoke(this, new Action(() =>
            {
                if (bError)
                {
                    //        volStrings.SetStatus_BadFile(form_lvVolumesMain);
                }
                else if (rootNode != null)
                {
                    lock (form_treeViewBrowse)
                    {
                        form_treeViewBrowse.Nodes.Add(rootNode.Text);    // items added to show progress
                        //             volStrings.SetStatus_Done(form_lvVolumesMain, rootNode);
                    }

                    lock (gd.m_listRootNodes)
                    {
                        gd.m_listRootNodes.Add(rootNode);
                    }
                }
                else
                {
                    MBox.Assert(1304.5309, false, "No data. Could be the directory is Access Denied.");
                }
            }));
        }

        internal void TreeSelectStatusCallback(ListViewItem[] lvItemDetails = null, ListViewItem[] itemArray = null, ListViewItem[] lvVolDetails = null, bool bSecondComparePane = false, LVitemFileTag lvFileItem = null)
        {
            UtilAnalysis_DirList.CheckAndInvoke(this, new Action(() =>
            {
                if (lvItemDetails != null)
                {
                    ListView lv = bSecondComparePane ? form_lvDetailVol : form_lvDetail;

                    lock (lv)
                    {
                        lv.Items.Clear();
                        lv.Items.AddRange(lvItemDetails);
                        lv.Invalidate();
                    }
                }

                if (lvVolDetails != null)
                {
                    ListView lv = form_lvDetailVol;

                    lock (lv)
                    {
                        lv.Items.Clear();
                        lv.Items.AddRange(lvVolDetails);
                        lv.Invalidate();
                    }
                }

                if (itemArray == null)
                {
                    return;
                }

                if (gd.m_bCompareMode == false)
                {
                    lock (form_lvFiles)
                    {
                        form_lvFiles.Items.Clear();
                        form_lvFiles.Items.AddRange(itemArray);
                        form_lvFiles.Invalidate();
                    }

                    return;
                }

                SDL_TreeView t1 = bSecondComparePane ? form_treeCompare2 : form_treeCompare1;
                SDL_TreeView t2 = bSecondComparePane ? form_treeCompare1 : form_treeCompare2;

                if (t1.SelectedNode == null)
                {
                    return;
                }

                UtilAnalysis_DirList.Write("A");

                if (lvFileItem.StrCompareDir != t1.SelectedNode.Text)
                {
                    // User is navigating faster than this thread.
                    UtilAnalysis_DirList.WriteLine("Fast: " + lvFileItem.StrCompareDir + "\t\t" + t1.SelectedNode.Text);
                    return;
                }

                SDL_ListView lv1 = (SDL_ListView)(bSecondComparePane ? form_lvFileCompare : form_lvFiles);
                SDL_ListView lv2 = (SDL_ListView)(bSecondComparePane ? form_lvFiles : form_lvFileCompare);

                lock (lv1)
                {
                    lv1.Items.Clear();
                    lv1.Items.AddRange(itemArray);
                    lv1.Invalidate();
                    ((ListViewItem)lv1.Items[0]).Tag = lvFileItem;
                }

                UtilAnalysis_DirList.Write("B");

                TreeNode treeNode1 = (TreeNode)t1.SelectedNode;
                TreeNode treeNode2 = (TreeNode)t2.SelectedNode;

                if (treeNode2 == null)
                {
                    return;
                }

                UtilAnalysis_DirList.Write("C");

                if (treeNode1.Level != treeNode2.Level)
                {
                    return;
                }

                if ((treeNode1.Level > 0) &&
                    (treeNode1.Text != treeNode2.Text))
                {
                    return;
                }

                UtilAnalysis_DirList.Write("D");

                if ((lv2.Items.Count > 0) &&
                    (((LVitemFileTag)((ListViewItem)lv2.Items[0]).Tag).StrCompareDir != treeNode2.Text))
                {
                    MBox.Assert(1304.5311, false);
                    return;
                }

                UtilAnalysis_DirList.Write("E");

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
            }));
        }

        void TreeSelectDoneCallback(bool bSecondComparePane)
        {
            UtilAnalysis_DirList.CheckAndInvoke(this, new Action(() =>
            {
                if (bSecondComparePane)
                {
                    gd.m_threadSelectCompare = null;
                }
                else
                {
                    gd.m_threadSelect = null;
                }

                SelectFoundFile();
            }));
        }

        void DoTree(bool bKill = false)
        {
            if (gd.m_tree != null)
            {
                if (bKill == false)
                {
                    return;
                }

                gd.KillTreeBuilder(bJoin: true);
            }

            if (gd.m_threadCollate != null)
            {
                if (bKill == false)
                {
                    return;
                }

                if (gd.m_threadCollate.IsAlive)
                {
                    gd.m_threadCollate.Abort();
                }

                gd.TreeCleanup();
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
                gd.m_dictNodes.Clear();
            }

            if (gd.m_dictNodes.Count <= 0)      // .Clear() to signal recreate. Ignore list only requires recorrelation
            {                                   // this works because gd.m_tree is not null during recreate.
                ClearMem();

                form_colFilename.Text = gd.m_strColFilesOrig;
                form_colDirDetail.Text = gd.m_strColDirDetailOrig;
                form_colVolDetail.Text = gd.m_strColVolDetailOrig;

                if ((ListLVvolStrings == null) || (ListLVvolStrings.Count() <= 0))
                {
                    return;
                }

                MBox.Assert(1304.5312, gd.m_listRootNodes.Count == 0);

                gd.m_tree = new Tree(ListLVvolStrings, gd.m_dictNodes, gd.m_dictDriveInfo,
                    TreeStatusCallback, TreeDoneCallback);
                gd.m_tree.DoThreadFactory();

                // m_tree has to be not-null for this to work
                form_treeViewBrowse.Nodes.Clear();
                TreeNode treeNode = new TreeNode("Creating treeview...        ");

                treeNode.NodeFont = new Font(form_treeViewBrowse.Font, FontStyle.Bold | FontStyle.Underline);
                form_treeViewBrowse.Nodes.Add(treeNode);
                form_treeViewBrowse.CheckBoxes = false;    // treeview items are faked to show progress
                form_treeViewBrowse.Enabled = false;
            }
            else
            {
                int nNodeCount = form_treeViewBrowse.GetNodeCount(includeSubTrees: true);

                MBox.Assert(1304.5313, gd.m_listTreeNodes.Count == nNodeCount);

                foreach (TreeNode treeNode in gd.m_listTreeNodes)
                {
                    treeNode.ForeColor = Color.Empty;
                    treeNode.BackColor = Color.Empty;

                    NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;

                    if (nodeDatum == null)
                    {
                        MBox.Assert(1304.5314, false);
                        continue;
                    }

                    nodeDatum.m_lvItem = null;
                    nodeDatum.m_listClones.Clear();
                    nodeDatum.m_bDifferentVols = false;
                }

                gd.m_listTreeNodes.Clear();
                gd.m_threadCollate = new Thread(new ThreadStart(DoCollation));
                gd.m_threadCollate.IsBackground = true;
                gd.m_threadCollate.Start();
            }
        }
    }
}