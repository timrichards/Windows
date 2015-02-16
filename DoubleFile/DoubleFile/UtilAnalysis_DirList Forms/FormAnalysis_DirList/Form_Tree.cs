using System.Windows.Forms;
using System.Drawing;
using System;
using System.Threading;
using System.Linq;

namespace DoubleFile
{
    partial class FormAnalysis_DirList
    {
        internal void ClearMem_TreeForm()
        {
            gd_Tree.ClearMem_TreeForm();

            foreach (ListViewItem lvItem in form_lvCopyScratchpad.Items)
            {
                lvItem.Tag = null;
            }
        }

        void TreeStatusCallback(LVitem_ProjectVM volStrings, TreeNode rootNode = null, bool bError = false)
        {
            if ((gd_Tree != null) &&
                (IsDisposed || (form_treeViewBrowse.IsDisposed) ||
                (gd_Tree.m_tree == null) || gd_Tree.m_tree.IsAborted))
            {
                gd_Tree.TreeCleanup();
                return;
            }

            UtilAnalysis_DirList.UIthread(this, () =>
            {
                if (bError)
                {
                    //        volStrings.SetStatus_BadFile(form_lvVolumesMain);
                }
                else if (rootNode != null)
                {
                    lock (form_treeViewBrowse.Nodes)
                    {
                        form_treeViewBrowse.Nodes.Add(rootNode.Text);    // items added to show progress
                        //             volStrings.SetStatus_Done(form_lvVolumesMain, rootNode);
                    }

                    lock (gd_Tree.m_listRootNodes)
                    {
                        gd_Tree.m_listRootNodes.Add(rootNode);
                    }
                }
                else
                {
                    MBoxStatic.Assert(1304.5309, false, "No data. Could be the directory is Access Denied.");
                }
            });
        }

        void TreeDoneCallback()
        {
            DoCollation();

            UtilAnalysis_DirList.UIthread(this, () =>
            {
                ListView lvFake = new ListView();

                foreach (ListViewItem lvItem in form_lvCopyScratchpad.Items)
                {
                    lvFake.Items.Add((ListViewItem)lvItem.Clone());
                }

                form_lvCopyScratchpad.Items.Clear();
                LoadCopyScratchPad(lvFake);
            });
        }

        void DoCollation()
        {
            if (gd_Tree.m_listRootNodes.IsEmpty())
            {
                UtilAnalysis_DirList.UIthread(this, () =>
                {
                    form_treeViewBrowse.Nodes.Clear();
                });
                
                return;
            }

            MBoxStatic.Assert(1304.5304, gd_Tree.m_listTreeNodes.IsEmpty());
            MBoxStatic.Assert(1304.5305, InvokeRequired);

            UtilAnalysis_DirList.UIthread(this, () =>
            {
                MBoxStatic.Assert(1304.5306, gd_Tree.m_listLVignore.IsEmpty());

                foreach (ListViewItem lvItem in form_lvIgnoreList.Items)
                {
                    ListViewItem lvItem_A = (ListViewItem)lvItem.Clone();

                    lvItem_A.Tag = lvItem;
                    gd_Tree.m_listLVignore.Add(lvItem_A);
                }
            });

            if (gd_Tree == null)
            {
                return;
            }

            Collate collate = new Collate(new GlobalData_Form(this),
                gd_Tree.m_dictNodes,
                form_treeViewBrowse,
                form_lvClones, form_lvSameVol, form_lvUnique,
                gd_Tree.m_listRootNodes, gd_Tree.m_listTreeNodes, gd.m_bCheckboxes,
                gd_Tree.m_listLVignore, form_chkLoose.Checked);
            DateTime dtStart = DateTime.Now;

            collate.Step1_OnThread();
            UtilProject.WriteLine("Step1_OnThread " + (DateTime.Now - dtStart).TotalMilliseconds / 1000.0 + " seconds."); dtStart = DateTime.Now;

            if (gd_Tree == null)
            {
                return;
            }

            if (IsDisposed)
            {
                gd_Tree.TreeCleanup();
                return;
            }

            gd.m_bPutPathInFindEditBox = true;
            UtilAnalysis_DirList.UIthread(this, collate.Step2_OnForm);
            UtilProject.WriteLine("Step2_OnForm " + (DateTime.Now - dtStart).TotalMilliseconds / 1000.0 + " seconds."); dtStart = DateTime.Now;
            collate = null;

            if (gd_Tree == null)
            {
                return;
            }

            gd_Tree.TreeCleanup();

            if (IsDisposed)
            {
                return;
            }

            int nNodeCount = form_treeViewBrowse.GetNodeCount(includeSubTrees: true);
            int nNodeCount_A = CountNodes.Go((TreeNode)form_treeViewBrowse.Nodes[0]);

            MBoxStatic.Assert(1304.5307, CountNodes.Go(gd_Tree.m_listRootNodes) == nNodeCount);
            MBoxStatic.Assert(1304.5308, gd_Tree.m_listTreeNodes.Count == nNodeCount);

            if (Form.ActiveForm == null)
            {
                FlashWindowStatic.Go();
            }
        }

        void TreeSelectStatusCallback(ListViewItem[] lvItemDetails = null, ListViewItem[] itemArray = null, ListViewItem[] lvVolDetails = null, bool bSecondComparePane = false, LVitemFileTag lvFileItem = null)
        {
            UtilAnalysis_DirList.UIthread(this, () =>
            {
                if (lvItemDetails != null)
                {
                    ListView lv = bSecondComparePane ? form_lvDetailVol : form_lvDetail;

                    lock (lv.Items)
                    {
                        lv.Items.Clear();
                        lv.Items.AddRange(lvItemDetails);
                        lv.Invalidate();
                    }
                }

                if (lvVolDetails != null)
                {
                    ListView lv = form_lvDetailVol;

                    lock (lv.Items)
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
                    lock (form_lvFiles.Items)
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
                    UtilProject.WriteLine("Fast: " + lvFileItem.StrCompareDir + "\t\t" + t1.SelectedNode.Text);
                    return;
                }

                SDL_ListView lv1 = (SDL_ListView)(bSecondComparePane ? form_lvFileCompare : form_lvFiles);
                SDL_ListView lv2 = (SDL_ListView)(bSecondComparePane ? form_lvFiles : form_lvFileCompare);

                lock (lv1.Items)
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

                if ((false == lv2.Items.IsEmpty()) &&
                    (((LVitemFileTag)((ListViewItem)lv2.Items[0]).Tag).StrCompareDir != treeNode2.Text))
                {
                    MBoxStatic.Assert(1304.5311, false);
                    return;
                }

                UtilAnalysis_DirList.Write("E");

                lock (lv1.Items)
                {
                    lock (lv2.Items)
                    {
                        LVitemNameComparerStruct.NameItems(lv1.Items);
                        LVitemNameComparerStruct.NameItems(lv2.Items);
                        LVitemNameComparerStruct.MarkItemsFrom1notIn2(lv1, lv2);
                        LVitemNameComparerStruct.MarkItemsFrom1notIn2(lv2, lv1);
                        LVitemNameComparerStruct.SetTopItem(lv1, lv2);
                        LVitemNameComparerStruct.SetTopItem(lv2, lv1);
                    }
                }
            });
        }

        void TreeSelectDoneCallback(bool bSecondComparePane)
        {
            UtilAnalysis_DirList.UIthread(this, () =>
            {
                if (null == gd_Tree)
                {
                    return;
                }

                if (bSecondComparePane)
                {
                    gd_Tree.m_threadSelectCompare = null;
                }
                else
                {
                    gd_Tree.m_threadSelect = null;
                }

                SelectFoundFile();
            });
        }

        void DoTree(bool bKill = false)
        {
            if (null == gd_Tree)
            {
                return;
            }

            if (gd_Tree.m_tree != null)
            {
                if (bKill == false)
                {
                    return;
                }

                gd_Tree.KillTreeBuilder(bJoin: true);
            }

            if (gd_Tree.m_threadCollate != null)
            {
                if (bKill == false)
                {
                    return;
                }

                if (gd_Tree.m_threadCollate.IsAlive)
                {
                    gd_Tree.m_threadCollate.Abort();
                }

                gd_Tree.TreeCleanup();
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
                gd_Tree.m_dictNodes.Clear();
            }

            if (gd_Tree.m_dictNodes.IsEmpty())     // .Clear() to signal recreate. Ignore list only requires recollation
            {                                       // this works because gd.m_tree is not null during recreate.
                ClearMem();

                form_colFilename.Text = gd.m_strColFilesOrig;
                form_colDirDetail.Text = gd.m_strColDirDetailOrig;
                form_colVolDetail.Text = gd.m_strColVolDetailOrig;

                if ((LVprojectVM == null) || (LVprojectVM.Items.IsEmpty()))
                {
                    return;
                }

                MBoxStatic.Assert(1304.5312, gd_Tree.m_listRootNodes.IsEmpty());

                gd_Tree.m_tree = new Tree(new GlobalData_Form(this),
                    LVprojectVM, gd_Tree.m_dictNodes, gd_Tree.m_dictDriveInfo,
                    TreeStatusCallback, TreeDoneCallback);
                gd_Tree.m_tree.DoThreadFactory();

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

                MBoxStatic.Assert(1304.5313, gd_Tree.m_listTreeNodes.Count == nNodeCount);

                foreach (TreeNode treeNode in gd_Tree.m_listTreeNodes)
                {
                    treeNode.ForeColor = Color.Empty;
                    treeNode.BackColor = Color.Empty;

                    NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;

                    if (nodeDatum == null)
                    {
                        MBoxStatic.Assert(1304.5314, false);
                        continue;
                    }

                    nodeDatum.m_lvItem = null;
                    nodeDatum.m_listClones.Clear();
                    nodeDatum.m_bDifferentVols = false;
                }

                gd_Tree.m_listTreeNodes.Clear();
                gd_Tree.m_threadCollate = new Thread(DoCollation);
                gd_Tree.m_threadCollate.IsBackground = true;
                gd_Tree.m_threadCollate.Start();
            }
        }
    }
}
