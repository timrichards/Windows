using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Drawing;
using System.Collections.Concurrent;

namespace SearchDirLists
{
    delegate void SearchStatusDelegate(SearchResults searchResults, bool bFirst = false, bool bLast = false);

    class SearchResultsDir
    {
        string m_strDir = null;
        List<string> m_listFiles = new List<string>();

        internal string StrDir { get { return m_strDir; } set { m_strDir = value; } }
        internal List<string> ListFiles { get { return m_listFiles; } }

        void AddFile(string strFile)
        {
            m_listFiles.Add(strFile);
        }
    }

    class SearchResults
    {
        internal readonly string StrSearch = null;
        internal readonly LVvolStrings VolStrings = null;
        internal readonly List<SearchResultsDir> Results = null;

        internal SearchResults(string strSearch, LVvolStrings volStrings, List<SearchResultsDir> listResults)
        {
            StrSearch = strSearch;
            VolStrings = volStrings;
            Results = listResults;
        }
    }

    class SearchBase : Utilities
    {
        protected string m_strSearch = null;
        protected bool m_bCaseSensitive = true;
        protected bool m_bSearchFilesOnly = false;
        protected string m_strCurrentNode = null;

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

            string strVolumeName = m_volStrings.VolumeName;
            string strPath = m_volStrings.StrPath;
            string strSaveAs = m_volStrings.SaveAs;

            if (FormatPath(ref strPath, ref strSaveAs, bFailOnDirectory: false) == false)
            {
                return;
            }

            using (StreamReader file = new StreamReader(strSaveAs))
            {
                string strLine = null;
                SearchResultsDir searchResultDir = null;
                string strSearch = m_strSearch;
                List<SearchResultsDir> listResults = new List<SearchResultsDir>();
                bool bFirst = false;
                string strCurrentNode = m_strCurrentNode ?? string.Empty;

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

                    bool bDir = strLine.StartsWith(mSTRlineType_Directory);
                    bool bFile = strLine.StartsWith(mSTRlineType_File);

                    if ((bDir == false) && (bFile == false))
                    {
                        continue;
                    }

                    string[] arrLine = strLine.Split('\t');
                    string strMatchDir = null;
                    string strMatchFile = null;

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
                            m_statusCallback(new SearchResults(m_strSearch, m_volStrings, listResults), bLast: true);
                            listResults = new List<SearchResultsDir>();
                        }

                        bFirst = true;
                    }

                    // "redoing" this logic prevents bugs during code maintenance from leaking into the result strings

                    string strDir = null;

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
                        string strFile = arrLine[3];

                        if (searchResultDir == null)
                        {
                            searchResultDir = new SearchResultsDir();
                        }

                        searchResultDir.ListFiles.Add(strFile);
                    }
                }

                if (searchResultDir != null)
                {
                    Utilities.Assert(1307.8307, searchResultDir.StrDir == null);
                }
                else
                {
                    Utilities.Assert(1307.8308, searchResultDir == null);
                }

                if (listResults.Count > 0)
                {
                    listResults.Sort((x, y) => x.StrDir.CompareTo(y.StrDir));
                    m_statusCallback(new SearchResults(m_strSearch, m_volStrings, listResults), bFirst: bFirst);
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

    class SearchType2 : SearchBase
    {
        Thread m_thread = null;
        bool m_bThreadAbort = false;
        ConcurrentBag<SearchFile> m_cbagWorkers = new ConcurrentBag<SearchFile>();
        SearchStatusDelegate m_statusCallback = null;
        Action m_doneCallback = null;
        UList<LVvolStrings> m_list_lvVolStrings = new UList<LVvolStrings>();

        internal SearchType2(ListView.ListViewItemCollection lvVolItems, string strSearch, bool bCaseSensitive,
            SearchFile.FolderSpecialHandling folderHandling, bool bSearchFilesOnly, string strCurrentNode,
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

            Utilities.WriteLine(string.Format("Completed Search for {0} in {1} seconds.", m_strSearch, ((int)(DateTime.Now - dtStart).TotalMilliseconds / 100) / 10.0));

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
        bool m_bSearchResultsType2_List = false;
        int m_nSearchResultsIndexer = -1;
        TreeNode[] m_SearchResultsType1_Array = null;
        List<SearchResults> m_SearchResultsType2_List = new List<SearchResults>();

        SearchType2 m_searchType2 = null;
        SearchResults m_firstSearchResults = null;
        SearchResults m_lastSearchResults = null;

        void ClearMem_Search()
        {
            m_bSearchResultsType2_List = false;
            m_nSearchResultsIndexer = -1;
            m_SearchResultsType1_Array = null;
            m_SearchResultsType2_List.Clear();

            Utilities.Assert(1307.8309, m_searchType2 == null);
            Utilities.Assert(1307.8301, m_firstSearchResults == null);
            Utilities.Assert(1307.8302, m_lastSearchResults == null);
            m_searchType2 = null;
            m_firstSearchResults = null;
            m_lastSearchResults = null;
        }

        const string mSTRsearchTitle = "Search";

        TreeNode SearchType1_FindNode(string strSearch, TreeNode startNode = null, TreeView treeView = null)
        {
            ClearMem_Search();

            if (Utilities.StrValid(strSearch) == false)
            {
                return null;
            }

            if (startNode != null)
            {
                treeView = startNode.TreeView;
            }
            else if (m_bCompareMode)
            {
                treeView = (m_treeCopyToClipboard is TreeView) ? (TreeView)m_treeCopyToClipboard : form_treeCompare1;
            }
            else if (treeView == null)
            {
                treeView = form_treeViewBrowse;
            }

            TreeNode treeNode = GetNodeByPath(strSearch, treeView);

            if (treeNode == null)
            {
                // case sensitive only when user enters an uppercase character

                List<TreeNode> listTreeNodes = m_listTreeNodes.ToList();

                if (m_bCompareMode)
                {
                    listTreeNodes = ((treeView == form_treeCompare2) ? m_listTreeNodes_Compare2 : m_listTreeNodes_Compare1).ToList();
                }

                if (strSearch.ToLower() == strSearch)
                {
                    m_SearchResultsType1_Array = listTreeNodes.FindAll(node => node.Text.ToLower().Contains(strSearch)).ToArray();
                }
                else
                {
                    m_SearchResultsType1_Array = listTreeNodes.FindAll(node => node.Text.Contains(strSearch)).ToArray();
                }
            }
            else
            {
                m_SearchResultsType1_Array = new TreeNode[] { treeNode };
            }

            if ((m_SearchResultsType1_Array != null) && (m_SearchResultsType1_Array.Length > 0))
            {
                if (m_SearchResultsType1_Array.Contains(startNode))
                {
                    m_nSearchResultsIndexer = (m_SearchResultsType1_Array.Count(node => node != startNode) - 1);
                    return startNode;
                }
                else
                {
                    m_nSearchResultsIndexer = 0;
                    return m_SearchResultsType1_Array[0];
                }
            }
            else
            {
                return null;
            }
        }

        bool SearchResultsType2_Nav(TreeView treeView)
        {
            int nCounter = -1;

            while (true)
            {
                if (m_SearchResultsType2_List.Count <= 0)       // torturing the UX
                {
                    return false;
                }

                foreach (SearchResults searchResults in m_SearchResultsType2_List)
                {
                    foreach (SearchResultsDir resultDir in searchResults.Results)
                    {
                        if (resultDir.ListFiles.Count <= 0)
                        {
                            if (++nCounter <= m_nSearchResultsIndexer)
                            {
                                continue;
                            }

                            TreeNode treeNode = GetNodeByPath(resultDir.StrDir, treeView);

                            if (treeNode == null)
                            {
                                // compare mode
                                Utilities.Assert(1307.8303, false);
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
                            foreach (string strFile in resultDir.ListFiles)
                            {
                                if (++nCounter <= m_nSearchResultsIndexer)
                                {
                                    continue;
                                }

                                m_strSelectFile = strFile;

                                TreeNode treeNode = GetNodeByPath(resultDir.StrDir, treeView);

                                if (treeNode == null)
                                {
                                    // compare mode
                                    Utilities.Assert(1307.8303, false);
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

                // Don't bother imposing a modulus. Just let m_nSearchResultsIndexer grow.
            }
        }

        void SearchFail()
        {
            m_nSearchResultsIndexer = -1;
            Utilities.Assert(1307.83037, m_SearchResultsType1_Array == null, bTraceOnly: true);
            m_SearchResultsType1_Array = null;
            m_bSearchResultsType2_List = false;
            m_strSelectFile = null;
            m_blinky.Go(clr: Color.Red, Once: true);
            Form1MessageBox("Couldn't find the specified search parameter.", mSTRsearchTitle);
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
                // expected a file but got an empty listviewer
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

        void SearchStatusCallback(SearchResults searchResults, bool bFirst = false, bool bLast = false)
        {
            if (searchResults.Results.Count <= 0)
            {
                Utilities.Assert(1307.8304, false); // caller takes care of this
                return;
            }

            if (bFirst)
            {
                m_firstSearchResults = searchResults;
            }
            else if (bLast)
            {
                m_lastSearchResults = searchResults;
            }
            else
            {
                lock (m_SearchResultsType2_List)
                {
                    m_SearchResultsType2_List.Add(searchResults);
                }
            }
        }

        void SearchDoneCallback()
        {
            if (AppExit || (m_searchType2 == null) || m_searchType2.IsAborted)
            {
                return;
            }

            if (InvokeRequired) { Invoke(new Action(SearchDoneCallback)); return; }

            m_searchType2 = null;
            m_SearchResultsType2_List.Sort((x, y) => (x.VolStrings.VolumeName.CompareTo(y.VolStrings.VolumeName)));

            if (m_firstSearchResults != null)
            {
                m_SearchResultsType2_List.Insert(0, m_firstSearchResults);
                m_firstSearchResults = null;
            }

            if (m_lastSearchResults != null)
            {
                m_SearchResultsType2_List.Add(m_lastSearchResults);
                m_lastSearchResults = null;
            }

            m_blinky.Reset();

            if (m_SearchResultsType2_List.Count <= 0)
            {
                SearchFail();
                return;
            }

            m_bSearchResultsType2_List = true;

            if (SearchResultsType2_Nav(form_treeViewBrowse) == false)
            {
                Utilities.Assert(1307.83043, false);
                SearchFail();
            }

            if ((m_form1MessageBoxOwner != null) && (m_form1MessageBoxOwner.Text == mSTRsearchTitle))
            {
                m_form1MessageBoxOwner.Dispose();
                m_form1MessageBoxOwner = null;
            }
        }

        private void DoSearchType2(string strSearch, bool bKill = false, bool bSearchFilesOnly = false)
        {
            if (m_searchType2 != null)
            {
                DialogResult dlgResult = DialogResult.Yes;

                if (bKill == false)
                {
                    dlgResult = Form1MessageBox("Already in progress. Restart search?" + "\n(or Cancel search.)".PadRight(100), mSTRsearchTitle, MessageBoxButtons.YesNoCancel);

                    if (dlgResult == DialogResult.No)
                    {
                        m_blinky.Go(bProgress: true);
                        return;
                    }
                }

                if (m_searchType2 != null)   // needs to be here
                {
                    // DialogResult.Cancel
                    m_searchType2.EndThread();
                    m_searchType2 = null;
                }

                if (dlgResult != DialogResult.Yes)
                {
                    return;
                }
            }

            m_blinky.Go(bProgress: true);
            m_SearchResultsType2_List = new List<SearchResults>();

      //      SearchFile.FolderSpecialHandling folderHandling = SearchFile.FolderSpecialHandling.None;    // not used
            string strCurrentNode = null;

            if (form_treeViewBrowse.SelectedNode != null)
            {
                strCurrentNode = FullPath(form_treeViewBrowse.SelectedNode);
            }

            Utilities.Assert(1307.83047, m_searchType2 == null);

            //m_searchType2 = new SearchType2(form_lvVolumesMain.Items, strSearch, strSearch.ToLower() != strSearch,
            //    folderHandling, bSearchFilesOnly, strCurrentNode,
            //    new SearchStatusDelegate(SearchStatusCallback), new Action(SearchDoneCallback));
            //m_searchType2.DoThreadFactory();
        }

        void DoSearch(object sender)
        {
            if (form_cbFindbox.Text.Length <= 0)
            {
                m_blinky.Go(clr: Color.Red, Once: true);
                return;
            }

            m_ctlLastSearchSender = (Control)sender;

            TreeView treeView = form_treeViewBrowse;

            if (m_bCompareMode)
            {
                treeView = m_treeCopyToClipboard;
            }

            while (true)
            {
                if (m_nSearchResultsIndexer < 0)
                {
                    SearchType1_FindNode(form_cbFindbox.Text, treeView.SelectedNode, treeView);
                }

                if (m_bSearchResultsType2_List)
                {
                    SearchResultsType2_Nav(treeView);
                    break;
                }

                if ((m_SearchResultsType1_Array != null) && (m_SearchResultsType1_Array.Length > 0))
                {
                    TreeNode treeNode = m_SearchResultsType1_Array[m_nSearchResultsIndexer % m_SearchResultsType1_Array.Length];

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
                    string strMaybePath = form_cbFindbox.Text.Substring(0, nPos);
                    TreeNode treeNode = GetNodeByPath(strMaybePath, form_treeViewBrowse);

                    m_strSelectFile = form_cbFindbox.Text.Substring(nPos + 1);

                    if (treeNode != null)
                    {
                        m_bTreeViewIndirectSelChange = true;
                        treeNode.TreeView.SelectedNode = treeNode;
                    }
                    else
                    {
                        Utilities.Assert(1307.8306, m_SearchResultsType2_List.Count <= 0);
                        SearchFail();
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
