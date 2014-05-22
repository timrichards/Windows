﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Drawing;
using System.Collections.Concurrent;

namespace SearchDirLists
{
    delegate void SearchStatusDelegate(String strSearch, LVvolStrings volStrings, List<SearchResultsDir> listResults, bool bFirst = false, bool bLast = false);

    class SearchResultsDir
    {
        String m_strDir = null;
        List<String> m_listFiles = new List<String>();

        internal String StrDir { get { return m_strDir; } set { m_strDir = value; } }
        internal List<String> ListFiles { get { return m_listFiles; } }

        void AddFile(String strFile)
        {
            m_listFiles.Add(strFile);
        }
    }

    class SearchResults
    {
        internal readonly String StrSearch = null;
        internal readonly LVvolStrings VolStrings = null;
        internal readonly List<SearchResultsDir> Results = null;

        internal SearchResults(String strSearch, LVvolStrings volStrings, List<SearchResultsDir> listResults)
        {
            StrSearch = strSearch;
            VolStrings = volStrings;
            Results = listResults;
        }
    }

    class SearchBase : Utilities
    {
        protected String m_strSearch = null;
        protected bool m_bCaseSensitive = true;
        protected bool m_bSearchFilesOnly = false;
        protected String m_strCurrentNode = null;

        internal enum FolderSpecialHandling { None, Outermost, Innermost };       // not used
        protected SearchFile.FolderSpecialHandling m_folderHandling = SearchFile.FolderSpecialHandling.Outermost;     // not used

        protected SearchBase() { }
        protected SearchBase(SearchBase searchBase)
        {
            m_strSearch = searchBase.m_strSearch;
            m_bCaseSensitive = searchBase.m_bCaseSensitive;
            m_bSearchFilesOnly = searchBase.m_bSearchFilesOnly;
            m_strCurrentNode = searchBase.m_strCurrentNode;
            m_folderHandling = searchBase.m_folderHandling;
        }
    }

    class SearchFile : SearchBase
    {
        Thread m_thread = null;
        bool m_bThreadAbort = false;
        SearchStatusDelegate m_statusCallback = null;
        LVvolStrings m_volStrings = null;

        internal SearchFile(SearchBase searchBase, LVvolStrings volStrings, SearchStatusDelegate statusCallback)
            : base(searchBase)
        {
            m_volStrings = volStrings;
            m_statusCallback = statusCallback;
        }

        internal void Go()
        {
            if (m_volStrings.CanLoad == false)
            {
                return;
            }

            String strVolumeName = m_volStrings.VolumeName;
            String strPath = m_volStrings.StrPath;
            String strSaveAs = m_volStrings.SaveAs;

            if (FormatPath(ref strPath, ref strSaveAs, bFailOnDirectory: false) == false)
            {
                return;
            }

            using (StreamReader file = new StreamReader(strSaveAs))
            {
                String strLine = null;
                SearchResultsDir searchResultDir = null;
                String strSearch = m_strSearch;
                List<SearchResultsDir> listResults = new List<SearchResultsDir>();
                bool bFirst = false;
                String strCurrentNode = m_strCurrentNode ?? String.Empty;

                if (m_bCaseSensitive == false)
                {
                    strSearch = strSearch.ToLower();
                    strCurrentNode = strCurrentNode.ToLower();
                }

                while ((strLine = file.ReadLine()) != null)
                {
                    if (m_bThreadAbort || Form1.AppExit)
                    {
                        return;
                    }

                    bool bDir = strLine.StartsWith(m_strLINETYPE_Directory);
                    bool bFile = strLine.StartsWith(m_strLINETYPE_File);

                    if ((bDir == false) && (bFile == false))
                    {
                        continue;
                    }

                    String[] arrLine = strLine.Split('\t');
                    String strMatchDir = null;
                    String strMatchFile = null;

                    if (bDir) { strMatchDir = arrLine[2].TrimEnd(Path.DirectorySeparatorChar); }
                    if (bFile) { strMatchFile = arrLine[3]; }

                    if (m_bCaseSensitive == false)
                    {
                        if (bDir) { strMatchDir = strMatchDir.ToLower(); }
                        if (bFile) { strMatchFile = strMatchFile.ToLower(); }
                    }

                    // strMatchDir gets set to just the folder name after this, but first check the full path
                    if (bDir && (strMatchDir == strCurrentNode))
                    {
                        if (listResults.Count > 0)
                        {
                            listResults.Sort((x, y) => x.StrDir.CompareTo(y.StrDir));
                            m_statusCallback(m_strSearch, m_volStrings, listResults, bLast: true);
                            listResults = new List<SearchResultsDir>();
                        }

                        bFirst = true;
                    }

                    // "redoing" this logic prevents bugs during code maintenance from leaking into the result strings

                    String strDir = null;

                    if (bDir) { strDir = arrLine[2].TrimEnd(Path.DirectorySeparatorChar); }

                    if (bDir && (searchResultDir != null))
                    {
                        searchResultDir.StrDir = strDir;
                        searchResultDir.ListFiles.Sort();
                        listResults.Add(searchResultDir);
                        searchResultDir = null;
                    }

                    // ...now just the last folder name for strMatchDir...      // "outermost"
                    if (bDir && strMatchDir.Contains(Path.DirectorySeparatorChar))
                    {
                        if (strSearch.Contains(Path.DirectorySeparatorChar) == false)
                        {
                            strMatchDir = strMatchDir.Substring(strMatchDir.LastIndexOf(Path.DirectorySeparatorChar) + 1);
                        }
                    }

                    if ((m_bSearchFilesOnly == false) && bDir && strMatchDir.Contains(strSearch))
                    {
                        if (searchResultDir == null)
                        {
                            searchResultDir = new SearchResultsDir();
                        }

                        searchResultDir.StrDir = strDir;
                        listResults.Add(searchResultDir);
                        searchResultDir = null;
                    }
                    else if (bFile && strMatchFile.Contains(strSearch))
                    {
                        String strFile = arrLine[3];

                        if (searchResultDir == null)
                        {
                            searchResultDir = new SearchResultsDir();
                        }

                        searchResultDir.ListFiles.Add(strFile);
                    }
                }

                if (searchResultDir != null)
                {
                    Utilities.Assert(1307.8301, searchResultDir.StrDir == null);
                }
                else
                {
                    Utilities.Assert(1307.8302, searchResultDir == null);
                }

                if (listResults.Count > 0)
                {
                    listResults.Sort((x, y) => x.StrDir.CompareTo(y.StrDir));
                    m_statusCallback(m_strSearch, m_volStrings, listResults, bFirst: bFirst);
                }
            }
        }

        internal SearchFile DoThreadFactory()
        {
            m_thread = new Thread(new ThreadStart(Go));
            m_thread.IsBackground = true;
            m_thread.Start();
            return this;
        }

        internal void Join()
        {
            m_thread.Join();
        }

        internal void Abort()
        {
            m_bThreadAbort = true;
            m_thread.Abort();
        }
    }

    class Search : SearchBase
    {
        Thread m_thread = null;
        bool m_bThreadAbort = false;
        ConcurrentBag<SearchFile> m_cbagWorkers = new ConcurrentBag<SearchFile>();
        SearchStatusDelegate m_statusCallback = null;
        Action m_doneCallback = null;
        UList<LVvolStrings> m_list_lvVolStrings = new UList<LVvolStrings>();

        internal Search(ListView.ListViewItemCollection lvVolItems, String strSearch, bool bCaseSensitive,
            SearchFile.FolderSpecialHandling folderHandling, bool bSearchFilesOnly, String strCurrentNode,
            SearchStatusDelegate statusCallback, Action doneCallback)
        {
            foreach (ListViewItem lvItem in lvVolItems)
            {
                m_list_lvVolStrings.Add(new LVvolStrings(lvItem));
            }

            m_strSearch = strSearch;
            m_bCaseSensitive = bCaseSensitive;
            m_folderHandling = folderHandling;          // not used
            m_bSearchFilesOnly = bSearchFilesOnly;
            m_strCurrentNode = strCurrentNode;
            m_statusCallback = statusCallback;
            m_doneCallback = doneCallback;
        }

        void Go()
        {
            Utilities.WriteLine("Searching for '" + m_strSearch + "'");

            DateTime dtStart = DateTime.Now;

            foreach (LVvolStrings volStrings in m_list_lvVolStrings)
            {
                SearchFile searchFile = new SearchFile((SearchBase)this, volStrings, m_statusCallback);

                m_cbagWorkers.Add(searchFile.DoThreadFactory());
            }

            foreach (SearchFile worker in m_cbagWorkers)
            {
                worker.Join();
            }

            Utilities.WriteLine(String.Format("Completed Search for {0} in {1} seconds.", m_strSearch, ((int)(DateTime.Now - dtStart).TotalMilliseconds / 100) / 10.0));

            if (m_bThreadAbort || Form1.AppExit)
            {
                return;
            }

            m_doneCallback();
        }

        internal void EndThread()
        {
            foreach (SearchFile worker in m_cbagWorkers)
            {
            	worker.Abort();
            }

            m_bThreadAbort = true;
            m_thread = null;
        }

        internal void DoThreadFactory()
        {
            m_thread = new Thread(new ThreadStart(Go));
            m_thread.IsBackground = true;
            m_thread.Start();
        }

        public bool IsAborted { get { return m_bThreadAbort; } }
    }

    partial class Form1 : Form
    {
        Search m_search = null;
        List<SearchResults> m_listSearchResults = new List<SearchResults>();
        SearchResults m_firstSearchResults = null;
        SearchResults m_lastSearchResults = null;

        void ClearMem_Search()
        {
            Utilities.Assert(1307.83025, m_search == null);

            m_search = null;

            m_listSearchResults.Clear();
            m_firstSearchResults = null;
            m_lastSearchResults = null;
        }

        bool NavToFile(TreeView treeView)
        {
            int nCounter = -1;
            int nSearchLoop = -1;

            while (true)
            {
                int nSearchIx = -1;

                if (m_listSearchResults.Count == 0)       // torturing the UX
                {
                    return false;
                }

                foreach (SearchResults searchResults in m_listSearchResults)
                {
                    foreach (SearchResultsDir resultDir in searchResults.Results)
                    {
                        if (resultDir.ListFiles.Count == 0)
                        {
                            if (++nCounter < m_nSearchResultsIndexer)
                            {
                                continue;
                            }

                            bool bContinue = false;
                            TreeNode treeNode = NavToFile(resultDir.StrDir, treeView,
                                ref nSearchLoop, ref nSearchIx, ref bContinue);

                            if (bContinue)
                            {
                                continue;
                            }

                            if (treeNode != null)
                            {
                                m_bTreeViewIndirectSelChange = true;
                                m_blinky.SelectTreeNode(treeNode);
                                ++m_nSearchResultsIndexer;
                            }

                            return (treeNode != null);
                        }
                        else
                        {
                            foreach (String strFile in resultDir.ListFiles)
                            {
                                if (++nCounter < m_nSearchResultsIndexer)
                                {
                                    continue;
                                }

                                m_strSelectFile = strFile;

                                bool bContinue = false;
                                TreeNode treeNode = NavToFile(resultDir.StrDir, treeView,
                                    ref nSearchLoop, ref nSearchIx, ref bContinue);

                                if (bContinue)
                                {
                                    continue;
                                }

                                if (treeNode != null)
                                {
                                    if (treeNode.TreeView.SelectedNode == treeNode)
                                    {
                                        SelectFoundFile();
                                    }
                                    else
                                    {
                                        m_bTreeViewIndirectSelChange = true;
                                        treeNode.TreeView.SelectedNode = treeNode;
                                    }

                                    ++m_nSearchResultsIndexer;
                                }

                                return (treeNode != null);
                            }
                        }
                    }
                }

                // Don't bother imposing a modulus. Just let m_nTreeFindTextChanged grow.
            }
        }

        TreeNode NavToFile(String strDir, TreeView treeView, ref int nSearchLoop, ref int nSearchIx, ref bool bContinue)
        {
            TreeNode treeNode = GetNodeByPath(strDir, treeView);

            bContinue = false;

            if (treeNode == null)
            {
                // compare mode
                Utilities.Assert(1307.8303, treeView != form_treeViewBrowse);

                if (treeView == form_treeViewBrowse)
                {
                    return null;
                }

                if (nSearchIx >= nSearchLoop)
                {
                    return null;
                }

                nSearchLoop = nSearchIx;
                bContinue = true;
            }

            return treeNode;
        }

        void SearchFail()
        {
            m_nSearchResultsIndexer = 0;
            m_bNavToFile = false;
            m_strSelectFile = null;
            m_blinky.Go(clr: Color.Red, Once: true);
            Form1MessageBox("Couldn't find the specified search parameter.", "Search");
        }

        void SelectFoundFile()
        {
            if (AppExit)
            {
                return;
            }

            if (Utilities.StrValid(m_strSelectFile) == false)
            {
                return;
            }

            if (form_lvFiles.Items.Count <= 0)
            {
                FlashWindow.Go(Once: true);
                return;
            }

            ListViewItem lvItem = form_lvFiles.FindItemWithText(m_strSelectFile, includeSubItemsInSearch: true, startIndex: 0);

            if (lvItem != null)
            {
                form_tabControlFileList.SelectedTab = form_tabPageFileList;
                m_blinky.SelectLVitem(lvItem: lvItem);
            }

            m_strSelectFile = null;
        }

        void SearchStatusCallback(String strSearch, LVvolStrings volStrings, List<SearchResultsDir> listResults, bool bFirst = false, bool bLast = false)
        {
            if (listResults.Count <= 0)
            {
                Utilities.Assert(1307.8304, false); // caller takes care of this
                return;
            }

            if (bFirst)
            {
                m_firstSearchResults = new SearchResults(strSearch, volStrings, listResults);
            }
            else if (bLast)
            {
                m_lastSearchResults = new SearchResults(strSearch, volStrings, listResults);
            }
            else
            {
                m_listSearchResults = new List<SearchResults>();

                lock (m_listSearchResults)
                {
                    m_listSearchResults.Add(new SearchResults(strSearch, volStrings, listResults));
                }
            }
        }

        void SearchDoneCallback()
        {
            if (AppExit || (m_search == null) || m_search.IsAborted)
            {
                return;
            }

            if (InvokeRequired) { Invoke(new Action(SearchDoneCallback)); return; }

            m_search = null;
            m_listSearchResults.Sort((x, y) => (x.VolStrings.VolumeName.CompareTo(y.VolStrings.VolumeName)));

            if (m_firstSearchResults != null)
            {
                m_listSearchResults.Insert(0, m_firstSearchResults);
                m_firstSearchResults = null;
            }

            if (m_lastSearchResults != null)
            {
                m_listSearchResults.Add(m_lastSearchResults);
                m_lastSearchResults = null;
            }

            if (m_listSearchResults.Count > 0)
            {
                m_bNavToFile = true;

                TreeView treeView = form_treeViewBrowse;

                if (m_bCompareMode)
                {
                    treeView = (m_ctlLastFocusForCopyButton is TreeView) ? (TreeView)m_ctlLastFocusForCopyButton : form_treeCompare1;

                    if (NavToFile(treeView) == false)
                    {
                        if (NavToFile((treeView == form_treeCompare1) ? form_treeCompare2 : form_treeCompare1) == false)
                        {
                            SearchFail();
                        }
                    }
                }
                else
                {
                    NavToFile(treeView);
                }
            }
            else
            {
                SearchFail();
            }
        }

        private void SearchFiles(String strSearch, bool bKill = false, bool bSearchFilesOnly = false)
        {
            if (m_search != null)
            {
                DialogResult dlgResult = DialogResult.Yes;

                if (bKill == false)
                {
                    dlgResult = Form1MessageBox("Already in progress. Restart search?", "Search", MessageBoxButtons.YesNoCancel);

                    if (dlgResult == DialogResult.No)
                    {
                        return;
                    }
                }

                if (m_search != null)   // needs to be here
                {
                    m_search.EndThread();
                }

                if (dlgResult != DialogResult.Yes)
                {
                    return;
                }
            }

            m_listSearchResults = new List<SearchResults>();

            SearchFile.FolderSpecialHandling folderHandling = SearchFile.FolderSpecialHandling.None;    // not used
            String strCurrentNode = null;

            if (form_treeViewBrowse.SelectedNode != null)
            {
                strCurrentNode = FullPath(form_treeViewBrowse.SelectedNode);
            }

            m_search = new Search(form_lvVolumesMain.Items, strSearch, strSearch.ToLower() != strSearch,
                folderHandling, bSearchFilesOnly, strCurrentNode,
                new SearchStatusDelegate(SearchStatusCallback), new Action(SearchDoneCallback));
            m_search.DoThreadFactory();
        }

        void DoSearch(object sender)
        {
            if (form_cbFindbox.Text.Length == 0)
            {
                m_blinky.Go(clr: Color.Red, Once: true);
                return;
            }

            m_ctlLastSearchSender = (Control)sender;

            TreeView treeView = form_treeViewBrowse;

            if (m_bCompareMode)
            {
                treeView = (m_ctlLastFocusForCopyButton is TreeView) ? (TreeView)m_ctlLastFocusForCopyButton : form_treeCompare1;
            }

            while (true)
            {
                if (m_nSearchResultsIndexer == 0)
                {
                    FindNode(form_cbFindbox.Text, treeView.SelectedNode, treeView);
                }

                if (m_bNavToFile)
                {
                    NavToFile(treeView);
                }
                else
                {
                    if ((m_arrSearchResults != null) && (m_arrSearchResults.Length > 0))
                    {
                        TreeNode treeNode = m_arrSearchResults[m_nSearchResultsIndexer % m_arrSearchResults.Length];

                        m_bTreeViewIndirectSelChange = true;
                        m_blinky.SelectTreeNode(treeNode);
                        ++m_nSearchResultsIndexer;
                    }
                    else if (treeView == form_treeCompare1)
                    {
                        treeView = form_treeCompare2;
                        continue;
                    }
                    else if (form_cbFindbox.Text.Contains(Path.DirectorySeparatorChar))
                    {
                        Utilities.Assert(1307.8305, form_cbFindbox.Text.EndsWith(Path.DirectorySeparatorChar.ToString()) == false);

                        int nPos = form_cbFindbox.Text.LastIndexOf(Path.DirectorySeparatorChar);
                        String strMaybePath = form_cbFindbox.Text.Substring(0, nPos);
                        TreeNode treeNode = GetNodeByPath(strMaybePath, form_treeViewBrowse);

                        m_strSelectFile = form_cbFindbox.Text.Substring(nPos + 1);

                        if (treeNode != null)
                        {
                            m_bTreeViewIndirectSelChange = true;
                            treeNode.TreeView.SelectedNode = treeNode;
                        }
                        else
                        {
                            Utilities.Assert(1307.8306, m_listSearchResults.Count <= 0);
                            SearchFail();
                        }
                    }
                    else
                    {
                        SearchFiles(form_cbFindbox.Text);
                    }
                }

                break;
            }
        }
    }
}
