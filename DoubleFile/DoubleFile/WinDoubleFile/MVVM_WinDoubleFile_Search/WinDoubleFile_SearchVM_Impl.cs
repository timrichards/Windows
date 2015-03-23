using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    partial class WinDoubleFile_SearchVM
    {
        internal Func<bool> IsSearchEnabled = null;
        static internal event Action<LVitem_ProjectVM, string, string> GoToFile;

        internal WinDoubleFile_SearchVM()
        {
            Icmd_Folders = new RelayCommand(param => SearchFolders(), param => IsSearchEnabled());
            Icmd_FoldersAndFiles = new RelayCommand(param => SearchFoldersAndFiles(), param => IsSearchEnabled());
            Icmd_Files = new RelayCommand(param => SearchFoldersAndFiles(bSearchFilesOnly: true), param => IsSearchEnabled());
            Icmd_Goto = new RelayCommand(param => Goto(), param => null != _selectedItem);
        }

        void SearchFolders()
        {
            UtilProject.UIthread(Items.Clear);

            bool bCase = SearchText.ToLower() != SearchText;
            IEnumerable<LocalTreeNode> lsTreeNodes = null;

            if (bCase)
            {
                lsTreeNodes = WinDoubleFile_FoldersVM.GetTreeNodes().Keys
                    .Where(treeNode => treeNode.Text.Contains(SearchText));
            }
            else
            {
                lsTreeNodes = WinDoubleFile_FoldersVM.GetTreeNodes().Keys
                    .Where(treeNode => treeNode.Text.ToLower().Contains(SearchText));
            }

            var lsLVitems = new List<LVitem_DoubleFile_SearchVM>();

            foreach (var treeNode in lsTreeNodes)
            {
                var lvItem = new LVitem_DoubleFile_SearchVM();

                lvItem.LocalTreeNode = treeNode;
                lsLVitems.Add(lvItem);
            }

            UtilProject.UIthread(() => Add(lsLVitems));
        }

        void SearchFoldersAndFiles(bool bSearchFilesOnly = false)
        {
            UtilProject.UIthread(Items.Clear);
            _lsLVitems = new ConcurrentBag<LVitem_DoubleFile_SearchVM>();

            _searchType2 = new SearchType2
                (
                    GlobalData.static_MainWindow.LVprojectVM,
                    SearchText,
                    SearchText.ToLower() != SearchText,
                    SearchType2.FolderSpecialHandling.None,
                    bSearchFilesOnly,
                    null,
                    SearchStatusCallback,
                    SearchDoneCallback
                )
                .DoThreadFactory();
        }

        void Goto()
        {
            if (null != _selectedItem.SearchResultsDir)
            {
                var strFile =
                    (_selectedItem.FileIndex >= 0)
                    ? _selectedItem.SearchResultsDir.ListFiles[_selectedItem.FileIndex]
                    : null;

                if (null != GoToFile)
                    GoToFile(null, _selectedItem.SearchResultsDir.StrDir, strFile);
            }
            else
            {
                _selectedItem.LocalTreeNode.GoToFile(null);
            }
        }

        void SearchStatusCallback(SearchResults searchResults, bool bFirst = false, bool bLast = false)
        {
            foreach (var searchResult in searchResults.Results)
            {
                if ((null != searchResult.ListFiles) &&
                    (false == searchResult.ListFiles.IsEmpty()))
                {
                    for (var nIx = 0; nIx < searchResult.ListFiles.Count; ++nIx)
                    {
                        var lvItem = new LVitem_DoubleFile_SearchVM();

                        lvItem.SearchResultsDir = searchResult;
                        lvItem.FileIndex = nIx;
                        _lsLVitems.Add(lvItem);
                    }
                }
                else
                {
                    var lvItem = new LVitem_DoubleFile_SearchVM();

                    lvItem.SearchResultsDir = searchResult;
                    lvItem.FileIndex = -1;
                    _lsLVitems.Add(lvItem);
                }
            }
        }

        void SearchDoneCallback()
        {
            _searchType2 = null;
            UtilProject.UIthread(() => Add(_lsLVitems));
            _lsLVitems = null;
        }

        SearchType2
            _searchType2 = null;
        ConcurrentBag<LVitem_DoubleFile_SearchVM>
            _lsLVitems = null;
    }
}
