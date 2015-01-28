using System.Windows.Forms;
using System.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;
using System.Windows;

namespace DoubleFile
{
    delegate void SearchStatusDelegate(SearchResults searchResults, bool bFirst = false, bool bLast = false);

    partial class FormAnalysis_DirList
    {
        bool SearchResultsType2_Nav(SDL_TreeView treeView)
        {
            int nCounter = -1;

            while (true)
            {
                if (gd_Search_1_2.m_SearchResultsType2_List.Count <= 0)       // torturing the UX
                {
                    return false;
                }

                foreach (SearchResults searchResults in gd_Search_1_2.m_SearchResultsType2_List)
                {
                    foreach (SearchResultsDir resultDir in searchResults.Results)
                    {
                        if (resultDir.ListFiles.Count <= 0)
                        {
                            if (++nCounter <= gd_Search_1_2.m_nSearchResultsIndexer)
                            {
                                continue;
                            }

                            TreeNode treeNode = gd_Search_Path.GetNodeByPath(resultDir.StrDir, treeView);

                            if (treeNode == null)
                            {
                                // compare mode, or tree is not loaded yet
                                MBox.Assert(1307.8308, false);
                            }

                            if (treeNode != null)
                            {
                                gd.m_bTreeViewIndirectSelChange = true;
                                gd.m_blinky.SelectTreeNode(treeNode);
                                ++gd_Search_1_2.m_nSearchResultsIndexer;
                            }

                            return (treeNode != null);
                        }
                        else
                        {
                            foreach (string strFile in resultDir.ListFiles)
                            {
                                if (++nCounter <= gd_Search_1_2.m_nSearchResultsIndexer)
                                {
                                    continue;
                                }

                                gd_Search_1_2.m_strSelectFile = strFile;

                                TreeNode treeNode = gd_Search_Path.GetNodeByPath(resultDir.StrDir, treeView);

                                if (treeNode == null)
                                {
                                    // compare mode, or tree is not loaded yet
                                    MBox.Assert(1307.8309, false);
                                }

                                if (treeNode != null)
                                {
                                    if (treeNode.TreeView.SelectedNode == treeNode)
                                    {
                                        SelectFoundFile();
                                    }
                                    else
                                    {
                                        gd.m_bTreeViewIndirectSelChange = true;
                                        treeNode.TreeView.SelectedNode = treeNode;
                                    }

                                    ++gd_Search_1_2.m_nSearchResultsIndexer;
                                }

                                return (treeNode != null);
                            }
                        }
                    }
                }

                // Don't bother imposing a modulus. Just let m_nSearchResultsIndexer grow.
            }
        }

        void SelectFoundFile()
        {
            if (IsDisposed)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(gd_Search_1_2.m_strSelectFile))
            {
                return;
            }

            if (form_lvFiles.Items.Count <= 0)
            {
                // expected a file but got an empty listviewer
                FlashWindow.Go(Once: true);
                return;
            }

            // Includes subitems in search
            ListViewItem lvItem = (ListViewItem)form_lvFiles.FindItemWithText(gd_Search_1_2.m_strSelectFile, true, 0, false);

            if ((lvItem != null) && MBox.Assert(0, lvItem.Text == gd_Search_1_2.m_strSelectFile))
            {
                form_tabControlFileList.SelectedTab = form_tabPageFileList;
                gd.m_blinky.SelectLVitem(lvItem: lvItem);
            }

            gd_Search_1_2.m_strSelectFile = null;
        }

        void SearchDoneCallback()
        {
            UtilAnalysis_DirList.CheckAndInvoke(this, new Action(() =>
            {
                if ((gd_Search_1_2.m_searchType2 == null) || gd_Search_1_2.m_searchType2.IsAborted)
                {
                    return;
                }

                gd_Search_1_2.m_searchType2 = null;
                gd_Search_1_2.m_SearchResultsType2_List.Sort((x, y) => (x.VolStrings.Nickname.CompareTo(y.VolStrings.Nickname)));

                if (gd_Search_1_2.m_firstSearchResults != null)
                {
                    gd_Search_1_2.m_SearchResultsType2_List.Insert(0, gd_Search_1_2.m_firstSearchResults);
                    gd_Search_1_2.m_firstSearchResults = null;
                }

                if (gd_Search_1_2.m_lastSearchResults != null)
                {
                    gd_Search_1_2.m_SearchResultsType2_List.Add(gd_Search_1_2.m_lastSearchResults);
                    gd_Search_1_2.m_lastSearchResults = null;
                }

                gd.m_blinky.Reset();

                if (gd_Search_1_2.m_SearchResultsType2_List.Count <= 0)
                {
                    gd_Search_1_2.SearchFail();
                    return;
                }

                gd_Search_1_2.m_bSearchResultsType2_List = true;

                if (SearchResultsType2_Nav(form_treeViewBrowse) == false)
                {
                    MBox.Assert(1307.8311, false);
                    gd_Search_1_2.SearchFail();
                }

                MBox.MessageBoxKill(GlobalData_Search_1_2.ksSearchTitle);
            }));
        }

        private void DoSearchType2(string strSearch, bool bKill = false, bool bSearchFilesOnly = false)
        {
            if (gd_Search_1_2.m_searchType2 != null)
            {
                MessageBoxResult mboxRet = MessageBoxResult.Yes;

                if (bKill == false)
                {
                    mboxRet = MBox.ShowDialog("Already in progress. Restart search?" + "\n(or Cancel search.)".PadRight(100), GlobalData_Search_1_2.ksSearchTitle, MessageBoxButton.YesNoCancel);

                    if (mboxRet == MessageBoxResult.No)
                    {
                        return;
                    }
                }

                gd.m_blinky.Reset();

                if (gd_Search_1_2.m_searchType2 != null)   // needs to be here
                {
                    // DialogResult.Cancel
                    gd_Search_1_2.m_searchType2.EndThread();
                    gd_Search_1_2.m_searchType2 = null;
                }

                if (mboxRet != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            gd.m_blinky.Go(bProgress: true);
            gd_Search_1_2.m_SearchResultsType2_List.Clear();

            SearchBase.FolderSpecialHandling folderHandling = SearchBase.FolderSpecialHandling.None;    // not used
            string strCurrentNode = null;

            if (form_treeViewBrowse.SelectedNode != null)
            {
                strCurrentNode = GlobalData.FullPath((TreeNode)form_treeViewBrowse.SelectedNode);
            }

            MBox.Assert(1307.8312, gd_Search_1_2.m_searchType2 == null);

            gd_Search_1_2.m_searchType2 = new SearchType2(new GlobalData_Form(this),
                LVprojectVM,
                strSearch,
                strSearch.ToLower() != strSearch,
                folderHandling,
                bSearchFilesOnly,
                strCurrentNode,
                new SearchStatusDelegate(gd_Search_1_2.SearchStatusCallback),
                SearchDoneCallback);
            gd_Search_1_2.m_searchType2.DoThreadFactory();
        }

        void DoSearch(object sender)
        {
            if (form_cbFindbox.Text.Length <= 0)
            {
                gd.m_blinky.Go(clr: Color.Red, Once: true);
                return;
            }

            m_ctlLastSearchSender = (Control)sender;

            SDL_TreeView treeView = form_treeViewBrowse;

            if (gd.m_bCompareMode)
            {
                treeView = gd.m_treeCopyToClipboard;
            }

            while (true)
            {
                if (gd_Search_1_2.m_nSearchResultsIndexer < 0)
                {
                    gd_Search_1_2.SearchType1_FindNode(form_cbFindbox.Text, (TreeNode)treeView.SelectedNode, treeView);
                }

                if (gd_Search_1_2.m_bSearchResultsType2_List)
                {
                    SearchResultsType2_Nav(treeView);
                    break;
                }

                if ((gd_Search_1_2.m_SearchResultsType1_Array != null) && (gd_Search_1_2.m_SearchResultsType1_Array.Length > 0))
                {
                    TreeNode treeNode = gd_Search_1_2.m_SearchResultsType1_Array[gd_Search_1_2.m_nSearchResultsIndexer % gd_Search_1_2.m_SearchResultsType1_Array.Length];

                    gd.m_bTreeViewIndirectSelChange = true;
                    gd.m_blinky.SelectTreeNode(treeNode);
                    ++gd_Search_1_2.m_nSearchResultsIndexer;
                }
                else if (treeView == form_treeCompare1)
                {
                    treeView = form_treeCompare2;
                    continue;
                }
                else if (form_cbFindbox.Text.Contains('\\'))
                {
                    if (form_cbFindbox.Text.EndsWith(@"\"))
                    {
                        MBox.Assert(1307.8313, false);
                        form_cbFindbox.Text = form_cbFindbox.Text.TrimEnd('\\');
                    }

                    int nPos = form_cbFindbox.Text.LastIndexOf('\\');
                    string strMaybePath = form_cbFindbox.Text.Substring(0, nPos);
                    TreeNode treeNode = gd_Search_Path.GetNodeByPath(strMaybePath, form_treeViewBrowse);

                    gd_Search_1_2.m_strSelectFile = form_cbFindbox.Text.Substring(nPos + 1);

                    if (treeNode != null)
                    {
                        gd.m_bTreeViewIndirectSelChange = true;
                        treeNode.TreeView.SelectedNode = treeNode;
                    }
                    else
                    {
                        MBox.Assert(1307.8314, gd_Search_1_2.m_SearchResultsType2_List.Count <= 0);
                        gd_Search_1_2.SearchFail();
                    }
                }
                else
                {
                    DoSearchType2(form_cbFindbox.Text);
                }

                break;
            }
        }
    }
}
