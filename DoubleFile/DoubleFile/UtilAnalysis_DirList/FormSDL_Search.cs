using System.Windows.Forms;
using System.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Collections.Concurrent;
using DoubleFile;
using System.Windows;

namespace SearchDirLists
{
    delegate void SearchStatusDelegate(SearchResults searchResults, bool bFirst = false, bool bLast = false);

    partial class FormSearchDirLists
    {
        bool SearchResultsType2_Nav(SDL_TreeView treeView)
        {
            int nCounter = -1;

            while (true)
            {
                if (gd.m_SearchResultsType2_List.Count <= 0)       // torturing the UX
                {
                    return false;
                }

                foreach (SearchResults searchResults in gd.m_SearchResultsType2_List)
                {
                    foreach (SearchResultsDir resultDir in searchResults.Results)
                    {
                        if (resultDir.ListFiles.Count <= 0)
                        {
                            if (++nCounter <= gd.m_nSearchResultsIndexer)
                            {
                                continue;
                            }

                            TreeNode treeNode = gd.GetNodeByPath(resultDir.StrDir, treeView);

                            if (treeNode == null)
                            {
                                // compare mode, or tree is not loaded yet
                                MBox.Assert(1307.8308, false);
                            }

                            if (treeNode != null)
                            {
                                gd.m_bTreeViewIndirectSelChange = true;
                                gd.m_blinky.SelectTreeNode(treeNode);
                                ++gd.m_nSearchResultsIndexer;
                            }

                            return (treeNode != null);
                        }
                        else
                        {
                            foreach (string strFile in resultDir.ListFiles)
                            {
                                if (++nCounter <= gd.m_nSearchResultsIndexer)
                                {
                                    continue;
                                }

                                gd.m_strSelectFile = strFile;

                                TreeNode treeNode = gd.GetNodeByPath(resultDir.StrDir, treeView);

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

                                    ++gd.m_nSearchResultsIndexer;
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
            if (GlobalData.AppExit)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(gd.m_strSelectFile))
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
            ListViewItem lvItem = (ListViewItem)form_lvFiles.FindItemWithText(gd.m_strSelectFile, true, 0, false);

            if ((lvItem != null) && MBox.Assert(0, lvItem.Text == gd.m_strSelectFile))
            {
                form_tabControlFileList.SelectedTab = form_tabPageFileList;
                gd.m_blinky.SelectLVitem(lvItem: lvItem);
            }

            gd.m_strSelectFile = null;
        }

        void SearchDoneCallback()
        {
            if (GlobalData.AppExit || (gd.m_searchType2 == null) || gd.m_searchType2.IsAborted)
            {
                return;
            }

            if (InvokeRequired) { Invoke(new Action(SearchDoneCallback)); return; }

            gd.m_searchType2 = null;
            gd.m_SearchResultsType2_List.Sort((x, y) => (x.VolStrings.Nickname.CompareTo(y.VolStrings.Nickname)));

            if (gd.m_firstSearchResults != null)
            {
                gd.m_SearchResultsType2_List.Insert(0, gd.m_firstSearchResults);
                gd.m_firstSearchResults = null;
            }

            if (gd.m_lastSearchResults != null)
            {
                gd.m_SearchResultsType2_List.Add(gd.m_lastSearchResults);
                gd.m_lastSearchResults = null;
            }

            gd.m_blinky.Reset();

            if (gd.m_SearchResultsType2_List.Count <= 0)
            {
                gd.SearchFail();
                return;
            }

            gd.m_bSearchResultsType2_List = true;

            if (SearchResultsType2_Nav(form_treeViewBrowse) == false)
            {
                MBox.Assert(1307.8311, false);
                gd.SearchFail();
            }

            MBox.MessageBoxKill(GlobalDataSDL.ksSearchTitle);
        }

        private void DoSearchType2(string strSearch, bool bKill = false, bool bSearchFilesOnly = false)
        {
            if (gd.m_searchType2 != null)
            {
                MessageBoxResult mboxRet = MessageBoxResult.Yes;

                if (bKill == false)
                {
                    mboxRet = MBox.ShowDialog("Already in progress. Restart search?" + "\n(or Cancel search.)".PadRight(100), GlobalDataSDL.ksSearchTitle, MessageBoxButton.YesNoCancel);

                    if (mboxRet == MessageBoxResult.No)
                    {
                        return;
                    }
                }

                gd.m_blinky.Reset();

                if (gd.m_searchType2 != null)   // needs to be here
                {
                    // DialogResult.Cancel
                    gd.m_searchType2.EndThread();
                    gd.m_searchType2 = null;
                }

                if (mboxRet != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            gd.m_blinky.Go(bProgress: true);
            gd.m_SearchResultsType2_List.Clear();

            SearchBase.FolderSpecialHandling folderHandling = SearchBase.FolderSpecialHandling.None;    // not used
            string strCurrentNode = null;

            if (form_treeViewBrowse.SelectedNode != null)
            {
                strCurrentNode = GlobalDataSDL.FullPath((TreeNode)form_treeViewBrowse.SelectedNode);
            }

            MBox.Assert(1307.8312, gd.m_searchType2 == null);

            UList<LVitem_ProjectVM> list_lvVolStrings = new UList<LVitem_ProjectVM>();

            //foreach (ListViewItem lvItem in form_lvVolumesMain.Items)
            //{
            //    list_lvVolStrings.Add(new LVitem_ProjectVM(lvItem));
            //}

            gd.m_searchType2 = new SearchType2(list_lvVolStrings, strSearch, strSearch.ToLower() != strSearch,
                folderHandling, bSearchFilesOnly, strCurrentNode,
                new SearchStatusDelegate(gd.SearchStatusCallback), SearchDoneCallback);
            gd.m_searchType2.DoThreadFactory();
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
                if (gd.m_nSearchResultsIndexer < 0)
                {
                    gd.SearchType1_FindNode(form_cbFindbox.Text, (TreeNode)treeView.SelectedNode, treeView);
                }

                if (gd.m_bSearchResultsType2_List)
                {
                    SearchResultsType2_Nav(treeView);
                    break;
                }

                if ((gd.m_SearchResultsType1_Array != null) && (gd.m_SearchResultsType1_Array.Length > 0))
                {
                    TreeNode treeNode = gd.m_SearchResultsType1_Array[gd.m_nSearchResultsIndexer % gd.m_SearchResultsType1_Array.Length];

                    gd.m_bTreeViewIndirectSelChange = true;
                    gd.m_blinky.SelectTreeNode(treeNode);
                    ++gd.m_nSearchResultsIndexer;
                }
                else if (treeView == form_treeCompare1)
                {
                    treeView = form_treeCompare2;
                    continue;
                }
                else if (form_cbFindbox.Text.Contains('\\'))
                {
                    MBox.Assert(1307.8313, form_cbFindbox.Text.EndsWith(@"\") == false);

                    int nPos = form_cbFindbox.Text.LastIndexOf('\\');
                    string strMaybePath = form_cbFindbox.Text.Substring(0, nPos);
                    TreeNode treeNode = gd.GetNodeByPath(strMaybePath, form_treeViewBrowse);

                    gd.m_strSelectFile = form_cbFindbox.Text.Substring(nPos + 1);

                    if (treeNode != null)
                    {
                        gd.m_bTreeViewIndirectSelChange = true;
                        treeNode.TreeView.SelectedNode = treeNode;
                    }
                    else
                    {
                        MBox.Assert(1307.8314, gd.m_SearchResultsType2_List.Count <= 0);
                        gd.SearchFail();
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

    class SearchResultsDir
    {
        readonly List<string> m_listFiles = new List<string>();

        internal string StrDir { get; set; }
        internal List<string> ListFiles { get { return m_listFiles; } }

        void AddFile(string strFile)
        {
            m_listFiles.Add(strFile);
        }
    }

    class SearchResults
    {
        internal readonly string StrSearch = null;
        internal readonly LVitem_ProjectVM VolStrings = null;
        internal readonly List<SearchResultsDir> Results = null;

        internal SearchResults(string strSearch, LVitem_ProjectVM volStrings, List<SearchResultsDir> listResults)
        {
            StrSearch = strSearch;
            VolStrings = volStrings;
            Results = listResults;
        }
    }

    class SearchBase : UtilAnalysis_DirList
    {
        protected string m_strSearch = null;
        protected bool m_bCaseSensitive = true;
        protected bool m_bSearchFilesOnly = false;
        protected string m_strCurrentNode = null;
        protected static SearchStatusDelegate m_statusCallback = null;

        internal enum FolderSpecialHandling { None, Outermost, Innermost };                     // not used
        protected FolderSpecialHandling m_folderHandling = FolderSpecialHandling.Outermost;     // not used

        protected SearchBase(SearchStatusDelegate statusCallback)
        {
            m_statusCallback = statusCallback;
        }
        
        protected SearchBase(SearchBase searchBase)
        {
            m_strSearch = searchBase.m_strSearch;
            m_bCaseSensitive = searchBase.m_bCaseSensitive;
            m_bSearchFilesOnly = searchBase.m_bSearchFilesOnly;
            m_strCurrentNode = searchBase.m_strCurrentNode;
            m_folderHandling = searchBase.m_folderHandling;
            MBox.Assert(1307.8315, m_statusCallback != null);
        }
    }

    class SearchType2 : SearchBase
    {
        Thread m_thread = null;
        bool m_bThreadAbort = false;
        ConcurrentBag<SearchFile> m_cbagWorkers = new ConcurrentBag<SearchFile>();
        readonly Action m_doneCallback = null;
        readonly UList<LVitem_ProjectVM> m_list_lvVolStrings = null;

        class SearchFile : SearchBase
        {
            Thread m_thread = null;
            bool m_bThreadAbort = false;
            LVitem_ProjectVM m_volStrings = null;

            internal SearchFile(SearchBase searchBase, LVitem_ProjectVM volStrings)
                : base(searchBase)
            {
                m_volStrings = volStrings;
            }

            internal void Go()
            {
                if (m_volStrings.CanLoad == false)
                {
                    return;
                }

                using (StreamReader file = new StreamReader(m_volStrings.ListingFile))
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
                        if (m_bThreadAbort || GlobalData.AppExit)
                        {
                            return;
                        }

                        bool bDir = strLine.StartsWith(ksLineType_Directory);
                        bool bFile = strLine.StartsWith(ksLineType_File);

                        if ((bDir == false) && (bFile == false))
                        {
                            continue;
                        }

                        string[] arrLine = strLine.Split('\t');
                        string strMatchDir = null;
                        string strMatchFile = null;

                        if (bDir) { strMatchDir = arrLine[2].TrimEnd('\\'); }
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

                        if (bDir) { strDir = arrLine[2].TrimEnd('\\'); }

                        if (bDir && (searchResultDir != null))
                        {
                            searchResultDir.StrDir = strDir;
                            searchResultDir.ListFiles.Sort();
                            listResults.Add(searchResultDir);
                            searchResultDir = null;
                        }

                        // ...now just the last folder name for strMatchDir...      // "outermost"
                        if (bDir && strMatchDir.Contains('\\'))
                        {
                            if (strSearch.Contains('\\') == false)
                            {
                                strMatchDir = strMatchDir.Substring(strMatchDir.LastIndexOf('\\') + 1);
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
                        MBox.Assert(1307.8301, searchResultDir.StrDir == null);
                    }
                    else
                    {
                        MBox.Assert(1307.8302, searchResultDir == null);
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

        internal SearchType2(UList<LVitem_ProjectVM> list_lvVolStrings, string strSearch, bool bCaseSensitive,
            SearchBase.FolderSpecialHandling folderHandling, bool bSearchFilesOnly, string strCurrentNode,
            SearchStatusDelegate statusCallback, Action doneCallback) : base(statusCallback)
        {
            m_list_lvVolStrings = list_lvVolStrings;
            m_strSearch = strSearch;
            m_bCaseSensitive = bCaseSensitive;
            m_folderHandling = folderHandling;          // not used
            m_bSearchFilesOnly = bSearchFilesOnly;
            m_strCurrentNode = strCurrentNode;
            m_doneCallback = doneCallback;
        }

        void Go()
        {
            UtilAnalysis_DirList.WriteLine("Searching for '" + m_strSearch + "'");

            DateTime dtStart = DateTime.Now;

            foreach (LVitem_ProjectVM volStrings in m_list_lvVolStrings)
            {
                SearchFile searchFile = new SearchFile((SearchBase)this, volStrings);

                m_cbagWorkers.Add(searchFile.DoThreadFactory());
            }

            foreach (SearchFile worker in m_cbagWorkers)
            {
                worker.Join();
            }

            UtilAnalysis_DirList.WriteLine(string.Format("Completed Search for {0} in {1} seconds.", m_strSearch, ((int)(DateTime.Now - dtStart).TotalMilliseconds / 100) / 10.0));

            if (m_bThreadAbort || GlobalData.AppExit)
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

    partial class GlobalDataSDL    // Get one node by path
    {
        internal TreeNode GetNodeByPath(string path, SDL_TreeView treeView)
        {
            return GetNodeByPath_A(path, treeView) ?? GetNodeByPath_A(path, treeView, bIgnoreCase: true);
        }

        internal TreeNode GetNodeByPath_A(string strPath, SDL_TreeView treeView, bool bIgnoreCase = false)
        {
            if (string.IsNullOrWhiteSpace(strPath))
            {
                return null;
            }

            if (bIgnoreCase)
            {
                strPath = strPath.ToLower();
            }

            TreeNode nodeRet = null;

            foreach (Object obj in treeView.Nodes)
            {
                TreeNode topNode = (TreeNode)obj;
                string[] arrPath = null;
                int nPathLevelLength = 0;
                int nLevel = 0;
                string strNode = topNode.Name.TrimEnd('\\').Replace(@"\\", @"\");

                if (bIgnoreCase)
                {
                    strNode = strNode.ToLower();
                }

                arrPath = strPath.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                nPathLevelLength = arrPath.Length;

                if (strNode.Contains('\\'))
                {
                    int nCount = strNode.Count(c => c == '\\');

                    for (int n = 0; n < nPathLevelLength - 1; ++n)
                    {
                        if (n < nCount)
                        {
                            arrPath[0] += '\\' + arrPath[n + 1];
                        }
                    }

                    for (int n = 1; n < nPathLevelLength - 1; ++n)
                    {
                        if ((nCount + n) < arrPath.Length)
                        {
                            arrPath[n] = arrPath[nCount + n];
                        }
                    }

                    if (nPathLevelLength > 1)
                    {
                        MBox.Assert(1308.9329, (nPathLevelLength - nCount) > 0);
                        nPathLevelLength -= nCount;
                    }
                }

                if (strNode == arrPath[nLevel])
                {
                    nodeRet = (TreeNode)topNode;
                    nLevel++;

                    if ((nLevel < nPathLevelLength) && nodeRet != null)
                    {
                        nodeRet = GetSubNode(nodeRet, arrPath, nLevel, nPathLevelLength, bIgnoreCase);

                        if (nodeRet != null)
                        {
                            return nodeRet;
                        }
                    }
                }
            }

            return nodeRet;
        }

        TreeNode GetSubNode(TreeNode node, string[] pathLevel, int i, int nPathLevelLength, bool bIgnoreCase)
        {
            foreach (TreeNode subNode in node.Nodes)
            {
                string strText = bIgnoreCase ? subNode.Text.ToLower() : subNode.Text;

                if (strText != pathLevel[i])
                {
                    continue;
                }

                if (++i == nPathLevelLength)
                {
                    return subNode;
                }

                return GetSubNode(subNode, pathLevel, i, nPathLevelLength, bIgnoreCase);
            }

            return null;
        }
    }

    partial class GlobalDataSDL    // Search type 1: array of nodes (Search Folders button); and 2: list of nodes and files
    {
        internal bool m_bSearchResultsType2_List = false;
        internal int m_nSearchResultsIndexer = -1;
        internal TreeNode[] m_SearchResultsType1_Array = null;
        internal readonly List<SearchResults> m_SearchResultsType2_List = new List<SearchResults>();

        internal SearchType2 m_searchType2 = null;
        internal SearchResults m_firstSearchResults = null;
        internal SearchResults m_lastSearchResults = null;
        internal string m_strSelectFile = null;

        internal void ClearMem_Search()
        {
            m_bSearchResultsType2_List = false;
            m_nSearchResultsIndexer = -1;
            m_SearchResultsType1_Array = null;
            m_SearchResultsType2_List.Clear();

            MBox.Assert(1307.8303, m_searchType2 == null);
            MBox.Assert(1307.8304, m_firstSearchResults == null);
            MBox.Assert(1307.8305, m_lastSearchResults == null);
            m_searchType2 = null;
            m_firstSearchResults = null;
            m_lastSearchResults = null;
        }

        internal const string ksSearchTitle = "Search";

        internal void SearchFail()
        {
            m_nSearchResultsIndexer = -1;
            MBox.Assert(1307.8306, m_SearchResultsType1_Array == null, bTraceOnly: true);
            m_SearchResultsType1_Array = null;
            m_bSearchResultsType2_List = false;
            m_strSelectFile = null;
            m_blinky.Go(clr: Color.Red, Once: true);
            MBox.ShowDialog("Couldn't find the specified search parameter.", ksSearchTitle);
        }

        internal void SearchStatusCallback(SearchResults searchResults, bool bFirst = false, bool bLast = false)
        {
            if (searchResults.Results.Count <= 0)
            {
                MBox.Assert(1307.8307, false); // caller takes care of this
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

        internal TreeNode SearchType1_FindNode(string strSearch, TreeNode startNode = null, SDL_TreeView treeView = null)
        {
            ClearMem_Search();

            if (string.IsNullOrWhiteSpace(strSearch))
            {
                return null;
            }

            if (startNode != null)
            {
                treeView = (SDL_TreeView)startNode.TreeView;
            }
            else if (m_bCompareMode)
            {
                treeView = (m_treeCopyToClipboard is SDL_TreeView) ? (SDL_TreeView)m_treeCopyToClipboard : (SDL_TreeView)GlobalDataSDL.static_form.form_treeCompare1;
            }
            else if (treeView == null)
            {
                treeView = ((SDL_TreeView)GlobalDataSDL.static_form.form_treeViewBrowse);
            }

            TreeNode treeNode = GetNodeByPath(strSearch, treeView);

            if (treeNode == null)
            {
                // case sensitive only when user enters an uppercase character

                List<TreeNode> listTreeNodes = m_listTreeNodes.ToList();

                if (m_bCompareMode)
                {
                    listTreeNodes = ((treeView == (SDL_TreeView)GlobalDataSDL.static_form.form_treeCompare2) ? m_listTreeNodes_Compare2 : m_listTreeNodes_Compare1).ToList();
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
    }
}
