using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Reactive.Linq;

namespace DoubleFile
{
    partial class WinDoubleFile_SearchVM : IDisposable
    {
        internal Func<bool> IsEditBoxNonEmpty = null;

        static internal IObservable<Tuple<LVitem_ProjectVM, string, string>>
            GoToFile { get { return _goToFile.AsObservable(); } }
        static readonly Subject<Tuple<LVitem_ProjectVM, string, string>> _goToFile = new Subject<Tuple<LVitem_ProjectVM, string, string>>();

        internal WinDoubleFile_SearchVM()
        {
            Icmd_Folders = new RelayCommand(param => SearchFolders(), param => IsSearchEnabled());
            Icmd_FoldersAndFiles = new RelayCommand(param => SearchFoldersAndFiles(), param => IsSearchEnabled());
            Icmd_Files = new RelayCommand(param => SearchFoldersAndFiles(bSearchFilesOnly: true), param => IsSearchEnabled());
            Icmd_Goto = new RelayCommand(param => Goto(), param => null != _selectedItem);
            TabledString<Tabled_Files>.AddRef();
        }

        public void Dispose()
        {
            _bDisposed = true;
            TabledString<Tabled_Files>.DropRef();
        }

        bool IsSearchEnabled() { return IsEditBoxNonEmpty() && (null == _searchType2); }

        void SearchFolders()
        {
            if (Reinitialize_And_FullPathFound(SearchText))
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

        void SearchFoldersAndFiles(bool bSearchFilesOnly = false)
        {
            if (Reinitialize_And_FullPathFound(SearchText))
                return;

            _dictResults = new SortedDictionary<SearchResultsDir, bool>();

            _searchType2 =
                new SearchType2
            (
                MainWindow.GetMainWindow().LVprojectVM,
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

        bool Reinitialize_And_FullPathFound(string strPath)
        {
            UtilProject.UIthread(Items.Clear);
            TabledString<Tabled_Files>.Reinitialize();
            TabledString<Tabled_Files>.GenerationStarting();

            var result = new SearchResultsDir();

            if (null != LocalTV.GetOneNodeByRootPathA(strPath, null))
            {
                result.StrDir = PathBuilder.FactoryCreateOrFind(strPath);
            }
            else
            {
                var nLastBackSlashIx = strPath.LastIndexOf('\\');

                if (2 > nLastBackSlashIx)
                    return false;

                result.StrDir = PathBuilder.FactoryCreateOrFind(strPath.Substring(0, nLastBackSlashIx));

                if (null == LocalTV.GetOneNodeByRootPathA("" + result.StrDir, null))
                    return false;

                result.ListFiles.Add(strPath.Substring(nLastBackSlashIx + 1), false);
            }

            _dictResults = new SortedDictionary<SearchResultsDir, bool>();
            SearchStatusCallback(new SearchResults(strPath, null, new[] { result }));
            SearchDoneCallback();
            return true;
        }

        void Goto()
        {
            if (null != _selectedItem.Directory)
                _goToFile.OnNext(Tuple.Create((LVitem_ProjectVM)null, "" + _selectedItem.Directory, "" + _selectedItem.Filename));
            else
                _selectedItem.LocalTreeNode.GoToFile(null);
        }

        void SearchStatusCallback(SearchResults searchResults, bool bFirst = false, bool bLast = false)
        {
            try
            {
                foreach (var result in searchResults.Results)
                    _dictResults.Add(result, false);
            }
            catch (Exception ex)
            {
                if ((ex is ArgumentNullException) ||
                    (ex is NullReferenceException))
                {
                    if (null != _searchType2)
                        _searchType2.EndThread();

                    _searchType2 = null;
                    _dictResults = null;
                    Dispose();
                    return;
                }

                throw;
            }
        }

        void SearchDoneCallback()
        {
            _searchType2 = null;

            var lsLVitems = new List<LVitem_DoubleFile_SearchVM>();

            foreach (var searchResult in _dictResults.Select(result => result.Key))
            {
                if (_bDisposed)
                    return;

                try
                {
                    // SearchResults.StrDir has a \ at the end for folder & file search where folder matches, because the key would dupe for file matches.
                    var Directory = PathBuilder.FactoryCreateOrFind(("" + searchResult.StrDir).TrimEnd('\\'));

                    if ((null != searchResult.ListFiles) &&
                        (false == searchResult.ListFiles.IsEmpty()))
                    {
                        foreach (var strFile in searchResult.ListFiles.Keys)
                        {
                            if (_bDisposed)
                                return;

                            lsLVitems.Add(new LVitem_DoubleFile_SearchVM { Directory = Directory, Filename = strFile });
                        }
                    }
                    else
                    {
                        lsLVitems.Add(new LVitem_DoubleFile_SearchVM { Directory = Directory });
                    }
                }
                catch (Exception ex)
                {
                    if ((ex is ArgumentNullException) ||
                        (ex is NullReferenceException))
                    {
                        MBoxStatic.Assert(99878, _bDisposed);
                        _dictResults = null;
                        return;
                    }

                    throw;
                }
            }

            _dictResults = null;

            if (_bDisposed)
                return;

            UtilProject.UIthread(() => Add(lsLVitems, bQuiet: false, Cancel: () => _bDisposed));

            if (_bDisposed)
                return;

            try
            {
                TabledString<Tabled_Files>.GenerationEnded();
            }
            catch (NullReferenceException)
            {
                MBoxStatic.Assert(99875, _bDisposed);
            }
        }

        SearchType2
            _searchType2 = null;
        SortedDictionary<SearchResultsDir, bool>
            _dictResults = null;
        bool
            _bDisposed = false;
    }
}
