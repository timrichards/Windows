using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace DoubleFile
{
    partial class WinSearchVM : IDisposable, ISearchStatus
    {
        internal Func<bool> IsEditBoxNonEmpty = null;

        static internal IObservable<Tuple<Tuple<LVitem_ProjectVM, string, string>, int>>
            GoToFile => _goToFile;
        static readonly LocalSubject<Tuple<LVitem_ProjectVM, string, string>> _goToFile = new LocalSubject<Tuple<LVitem_ProjectVM, string, string>>();
        static void GoToFileOnNext(Tuple<LVitem_ProjectVM, string, string> value) => _goToFile.LocalOnNext(value, 99982);

        bool IsSearchEnabled() => IsEditBoxNonEmpty() && (null == _searchType2);

        internal WinSearchVM Init()
        {
            Icmd_Folders = new RelayCommand(SearchFolders, IsSearchEnabled);
            Icmd_FoldersAndFiles = new RelayCommand(() => SearchFoldersAndFiles(), IsSearchEnabled);
            Icmd_Files = new RelayCommand(() => SearchFoldersAndFiles(bSearchFilesOnly: true), IsSearchEnabled);
            Icmd_GoTo = new RelayCommand(GoTo, () => null != _selectedItem);
            TabledString<TabledStringType_Files>.AddRef();
            PathBuilder.AddRef();
            return this;
        }

        public void Dispose()
        {
            _bDisposed = true;
            TabledString<TabledStringType_Files>.DropRef();
            PathBuilder.DropRef();
        }

        void SearchFolders()
        {
            if (GenerationStarting_And_FullPathFound(SearchText))
                return;

            var contains = SearchListings.GetContainsFunction(Regex);
            var bCase = (SearchText.ToLower() != SearchText) || Regex;

            var lsTreeNodes =
                bCase
                ? LocalTV.AllNodes
                    .Where(treeNode => contains(treeNode.Text, SearchText))
                : LocalTV.AllNodes
                    .Where(treeNode => treeNode.Text.ToLower().Contains(SearchText));

            var ieLVitems = lsTreeNodes
                .AsParallel()
                .Select(treeNode => new LVitem_SearchVM { LocalTreeNode = treeNode })
                .OrderBy(lvItem => lvItem.Parent + lvItem.FolderOrFile);

            if (ieLVitems.Any())
                Util.UIthread(99816, () => Add(ieLVitems));
        }

        void SearchFoldersAndFiles(bool bSearchFilesOnly = false)
        {
            if (GenerationStarting_And_FullPathFound(SearchText))
                return;

            (new WinProgress(new[] { "" }, new[] { _ksSearchKey }, x =>
            {
                _dictResults = new ConcurrentDictionary<SearchResultsDir, bool>();

                _searchType2 =
                    new SearchListings(Statics.LVprojectVM, new SearchBase
                (
                    SearchText,
                    SearchText.ToLower() != SearchText,
                    SearchBase.FolderSpecialHandling.None,
                    bSearchFilesOnly,
                    null,
                    Regex,
                    new WeakReference<ISearchStatus>(this)
                ))
                    .DoThreadFactory();
            }))
                .ShowDialog();
        }

        bool GenerationStarting_And_FullPathFound(string strPath)
        {
            if (string.IsNullOrWhiteSpace(SearchText))
                return true;        // found the UI doesn't block the user's attempt to search for nothing

            if (0 == (Statics.LVprojectVM?.CanLoadCount ?? 0))
                return true;        // found there are no volumes loaded

            _bRestarted = true;
            ClearItems();
            TabledString<TabledStringType_Files>.GenerationStarting();

            var result = new SearchResultsDir();

            if (null != LocalTV.GetOneNodeByRootPathA(strPath, null))
            {
                result.PathBuilder = PathBuilder.FactoryCreateOrFind(strPath);
            }
            else
            {
                var nLastBackSlashIx = strPath.LastIndexOf('\\');

                if (2 > nLastBackSlashIx)
                    return false;

                result.PathBuilder = PathBuilder.FactoryCreateOrFind(strPath.Substring(0, nLastBackSlashIx));

                if (null == LocalTV.GetOneNodeByRootPathA("" + result.PathBuilder, null))
                    return false;

                result.ListFiles.Add((TabledString<TabledStringType_Files>)strPath.Substring(nLastBackSlashIx + 1), false);
            }

            _dictResults = new ConcurrentDictionary<SearchResultsDir, bool>();
            ((ISearchStatus)this).Status(new SearchResults(strPath, null, new[] { result }));
            ((ISearchStatus)this).Done();
            return true;
        }

        void GoTo()
        {
            if (null == _selectedItem)
            {
                Util.Assert(99899, false);    // binding should dim the button
                return;
            }

            if (null != _selectedItem.Directory)
                GoToFileOnNext(Tuple.Create((LVitem_ProjectVM)null, "" + _selectedItem.Directory, "" + _selectedItem.TabledStringFilename));
            else
                _selectedItem.LocalTreeNode.GoToFile(null);
        }

        void ISearchStatus.Status(SearchResults searchResults, bool bFirst, bool bLast)
        {
            foreach (var result in searchResults.Results)
                _dictResults.Add(result, false);
        }

        void ISearchStatus.Done()
        {
            if (false == _bDisposed)
                TabledString<TabledStringType_Files>.GenerationEnded();

            _searchType2 = null;

            try
            {
                var lsLVitems = new List<LVitem_SearchVM> { };
                PathBuilder LastFolder = null;
                var nPrevHasFolder = 1;

                foreach (var searchResult in _dictResults.Keys.OrderBy(d => d))
                {
                    if (_bDisposed)
                        break;

                    // SearchResults.PathBuilder has a \ at the end for folder & file search where folder matches,
                    // because the key would dupe for file matches.
                    var Directory = PathBuilder.FactoryCreateOrFind(("" + searchResult.PathBuilder).TrimEnd('\\'));
                    var bHasFolder = (LastFolder == Directory);

                    if (0 < (searchResult.ListFiles?.Count ?? 0))
                    {
                        foreach (var tabledStringFile in searchResult.ListFiles.Keys)
                        {
                            if (_bDisposed)
                                break;

                            lsLVitems.Add(new LVitem_SearchVM
                            {
                                Directory = Directory,
                                TabledStringFilename = tabledStringFile,
                                Alternate = (bHasFolder) ? nPrevHasFolder : 0
                            });
                        }

                        nPrevHasFolder = ((false == bHasFolder) || (2 == nPrevHasFolder)) ? 1 : 2;
                    }
                    else
                    {
                        lsLVitems.Add(new LVitem_SearchVM { Directory = Directory });
                        LastFolder = Directory;
                    }
                }

                WinProgress.CloseForced();
                _dictResults = null;

                if ((0 < lsLVitems.Count) &&
                    (false == _bDisposed))
                {
                    _bRestarted = false;
                    Util.UIthread(99809, () => Add(lsLVitems, Cancel: () => _bDisposed || _bRestarted));
                }
            }
            catch (Exception e) when ((e is ArgumentNullException) || (e is NullReferenceException))
            {
                Util.Assert(99878, _bDisposed);
            }
            catch (OutOfMemoryException)
            {
                Util.Assert(99660, false, "OutOfMemoryException in Search");
            }

            WinProgress.CloseForced();
            _dictResults = null;
        }

        SearchListings
            _searchType2 = null;
        IDictionary<SearchResultsDir, bool>
            _dictResults = null;
        bool
            _bDisposed = false;
        bool
            _bRestarted = false;
        const string
            _ksSearchKey = "Searching";
    }
}
