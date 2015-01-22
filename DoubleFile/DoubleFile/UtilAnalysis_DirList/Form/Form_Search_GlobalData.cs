using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace DoubleFile
{
    partial class GlobalData    // Search type 1: array of nodes (Search Folders button); and 2: list of nodes and files
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
            m_blinky.Go(clr: System.Drawing.Color.Red, Once: true);
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
                treeView = (m_treeCopyToClipboard is SDL_TreeView) ? (SDL_TreeView)m_treeCopyToClipboard : (SDL_TreeView)GlobalData.static_MainWindow.SearchDirListsForm.form_treeCompare1;
            }
            else if (treeView == null)
            {
                treeView = ((SDL_TreeView)GlobalData.static_MainWindow.SearchDirListsForm.form_treeViewBrowse);
            }

            TreeNode treeNode = GetNodeByPath(strSearch, treeView);

            if (treeNode == null)
            {
                // case sensitive only when user enters an uppercase character

                List<TreeNode> listTreeNodes = m_listTreeNodes.ToList();

                if (m_bCompareMode)
                {
                    listTreeNodes = ((treeView == (SDL_TreeView)GlobalData.static_MainWindow.SearchDirListsForm.form_treeCompare2) ? m_listTreeNodes_Compare2 : m_listTreeNodes_Compare1).ToList();
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
