using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DoubleFile
{
    partial class WinDoubleFile_SearchVM : IDisposable
    {
        internal Func<bool> IsEditBoxNonEmpty = null;
        static internal event Action<LVitem_ProjectVM, string, string> GoToFile;

        internal WinDoubleFile_SearchVM()
        {
            Icmd_Folders = new RelayCommand(param => SearchFolders(), param => IsSearchEnabled());
            Icmd_FoldersAndFiles = new RelayCommand(param => SearchFoldersAndFiles(), param => IsSearchEnabled());
            Icmd_Files = new RelayCommand(param => SearchFoldersAndFiles(bSearchFilesOnly: true), param => IsSearchEnabled());
            Icmd_Goto = new RelayCommand(param => Goto(), param => null != _selectedItem);
            TabledString<TypedArray1>.AddRef();
        }

        public void Dispose()
        {
            TabledString<TypedArray1>.DropRef();
        }

        bool IsSearchEnabled() { return IsEditBoxNonEmpty() && (null == _searchType2); }

        void SearchFolders()
        {
            UtilProject.UIthread(Items.Clear);

            if (FullPathFound(SearchText))
                return;

            if (null == WinDoubleFile_FoldersVM.GetTreeNodes)
                return;

            var bCase = SearchText.ToLower() != SearchText;
            var lsTreeNodes = WinDoubleFile_FoldersVM.GetTreeNodes();

            if (null == lsTreeNodes)
                return;

            if (bCase)
            {
                lsTreeNodes =
                    lsTreeNodes
                    .Where(treeNode => treeNode.Text.Contains(SearchText));
            }
            else
            {
                lsTreeNodes =
                    lsTreeNodes
                    .Where(treeNode => treeNode.Text.ToLower().Contains(SearchText));
            }

            var lsLVitems = lsTreeNodes.AsParallel().Select(treeNode => new LVitem_DoubleFile_SearchVM { LocalTreeNode = treeNode });

            UtilProject.UIthread(() => Add(lsLVitems));
        }

        bool FullPathFound(string strPath)
        {
            var result = new SearchResultsDir();

            if (null != LocalTV.GetOneNodeByRootPathA(strPath, null))
            {
                result.StrDir = strPath;
            }
            else
            {
                var nLastBackSlashIx = strPath.LastIndexOf('\\');

                if (2 > nLastBackSlashIx)
                    return false;

                result.StrDir = strPath.Substring(0, nLastBackSlashIx);

                if (null == LocalTV.GetOneNodeByRootPathA(result.StrDir, null))
                    return false;

                result.ListFiles.Add(strPath.Substring(nLastBackSlashIx + 1), false);
            }

            _lsLVitems = new ConcurrentBag<LVitem_DoubleFile_SearchVM>();
            SearchStatusCallback(new SearchResults(strPath, null, new[] { result }));
            SearchDoneCallback();
            return true;
        }

        void SearchFoldersAndFiles(bool bSearchFilesOnly = false)
        {
            UtilProject.UIthread(Items.Clear);

            if (FullPathFound(SearchText))
                return;

            _lsLVitems = new ConcurrentBag<LVitem_DoubleFile_SearchVM>();
            TabledString<TypedArray1>.GenerationStarting();

            _searchType2 =
                new SearchType2
            (
                MainWindow.static_MainWindow.LVprojectVM,
                SearchText,
                SearchText.ToLower() != SearchText,
                SearchBase.FolderSpecialHandling.None,
                bSearchFilesOnly,
                null,
                SearchStatusCallback,
                SearchDoneCallback
            )
                .DoThreadFactory();
        }

        void Goto()
        {
            if (null != _selectedItem.Directory)
            {
                if (null != GoToFile)
                    GoToFile(null, _selectedItem.Directory.ToString(), _selectedItem.Filename);
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
                var Directory = PathBuilder<TypedArray1>.FactoryCreate(searchResult.StrDir);

                if ((null != searchResult.ListFiles) &&
                    (false == searchResult.ListFiles.IsEmpty()))
                {
                    foreach (var strFile in searchResult.ListFiles.Keys)
                        _lsLVitems.Add(new LVitem_DoubleFile_SearchVM { Directory = Directory, Filename = strFile });
                }
                else
                {
                    _lsLVitems.Add(new LVitem_DoubleFile_SearchVM { Directory = Directory });
                }
            }
        }

        void SearchDoneCallback()
        {
            _searchType2 = null;
            UtilProject.UIthread(() => Add(_lsLVitems));
            _lsLVitems = null;
            TabledString<TypedArray1>.GenerationEnded();
        }

        SearchType2
            _searchType2 = null;
        ConcurrentBag<LVitem_DoubleFile_SearchVM>
            _lsLVitems = null;
    }
}
