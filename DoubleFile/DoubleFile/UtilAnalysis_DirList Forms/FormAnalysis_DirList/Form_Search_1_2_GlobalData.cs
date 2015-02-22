using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace DoubleFile
{
    class GlobalData_Search_1_2    // Search type 1: array of nodes (Search Folders button); and 2: list of nodes and files
    {
        GlobalData gd
        {
            get { return _gd; }
            set
            {
                MBoxStatic.Assert(99985, _gd == null);
                _gd = value;
            }
        }
        GlobalData _gd = null;

        GlobalData_Search_Path gd_Search_Path
        {
            get { return _gd_Search_Path; }
            set
            {
                MBoxStatic.Assert(99984, _gd_Search_Path == null);
                _gd_Search_Path = value;
            }
        }
        GlobalData_Search_Path _gd_Search_Path = null;

        GlobalData_Tree gd_Tree
        {
            get { return _gd_Tree; }
            set
            {
                MBoxStatic.Assert(99983, gd_Tree == null);
                _gd_Tree = value;
            }
        }
        GlobalData_Tree _gd_Tree = null;

        internal bool m_bSearchResultsType2_List = false;
        internal int m_nSearchResultsIndexer = -1;
        internal TreeNode[] m_SearchResultsType1_Array = null;
        internal readonly List<SearchResults> m_SearchResultsType2_List = new List<SearchResults>();

        internal SearchType2 m_searchType2 = null;
        internal SearchResults m_firstSearchResults = null;
        internal SearchResults m_lastSearchResults = null;
        internal string m_strSelectFile = null;
        internal const string ksSearchTitle = "Search";

        internal GlobalData_Search_1_2(GlobalData gd_in, 
            GlobalData_Search_Path gd_Search_Path_in,
            GlobalData_Tree gd_Tree_in)
        {
            gd = gd_in;
            gd_Search_Path = gd_Search_Path_in;
            gd_Tree = gd_Tree_in;
        }

        internal void ClearMem_Search()
        {
            m_bSearchResultsType2_List = false;
            m_nSearchResultsIndexer = -1;
            m_SearchResultsType1_Array = null;
            m_SearchResultsType2_List.Clear();

            MBoxStatic.Assert(1307.8303, m_searchType2 == null);
            MBoxStatic.Assert(1307.8304, m_firstSearchResults == null);
            MBoxStatic.Assert(1307.8305, m_lastSearchResults == null);
            m_searchType2 = null;
            m_firstSearchResults = null;
            m_lastSearchResults = null;
        }

        internal void SearchFail()
        {
            m_nSearchResultsIndexer = -1;
        //    MBox.Assert(1307.8306, m_SearchResultsType1_Array == null, bTraceOnly: true);
            m_SearchResultsType1_Array = null;
            m_bSearchResultsType2_List = false;
            m_strSelectFile = null;
            gd.m_blinky.Go(clr: System.Drawing.Color.Red, Once: true);
            MBoxStatic.ShowDialog("Couldn't find the specified search parameter.", ksSearchTitle);
        }

        internal void SearchStatusCallback(SearchResults searchResults, bool bFirst = false, bool bLast = false)
        {
            if (searchResults.Results.IsEmptyA())
            {
                MBoxStatic.Assert(1307.8307, false); // caller takes care of this
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
            else if (gd.m_bCompareMode)
            {
                treeView = (gd.m_treeCopyToClipboard is SDL_TreeView) ? (SDL_TreeView)gd.m_treeCopyToClipboard : (SDL_TreeView)GlobalData.static_MainWindow.Analysis_DirListForm.form_treeCompare1;
            }
            else if (treeView == null)
            {
                treeView = ((SDL_TreeView)GlobalData.static_MainWindow.Analysis_DirListForm.form_treeViewBrowse);
            }

            TreeNode treeNode = gd_Search_Path.GetNodeByPath(strSearch, treeView);

            if (treeNode == null)
            {
                // case sensitive only when user enters an uppercase character

                List<TreeNode> listTreeNodes = gd_Tree.m_listTreeNodes.ToList();

                if (gd.m_bCompareMode)
                {
                    listTreeNodes = ((treeView == (SDL_TreeView)GlobalData.static_MainWindow.Analysis_DirListForm.form_treeCompare2) ? gd.m_listTreeNodes_Compare2 : gd.m_listTreeNodes_Compare1).ToList();
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
