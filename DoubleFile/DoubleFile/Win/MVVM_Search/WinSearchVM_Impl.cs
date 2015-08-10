using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace DoubleFile
{
    partial class WinSearchVM : IDisposable, ISearchStatus, IWinProgressClosing
    {
        internal Func<bool> IsEditBoxNonEmpty = null;
        bool IsSearchEnabled() => IsEditBoxNonEmpty() && (null == _searchType2);

        static internal IObservable<Tuple<Tuple<LVitem_ProjectVM, string, string>, int>>
            GoToFile => _goToFile;
        static readonly LocalSubject<Tuple<LVitem_ProjectVM, string, string>> _goToFile = new LocalSubject<Tuple<LVitem_ProjectVM, string, string>>();
        static void GoToFileOnNext(Tuple<LVitem_ProjectVM, string, string> value) => _goToFile.LocalOnNext(value, 99982);

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

        bool GenerationStarting_And_FullPathFound(string strPath)
        {
            if (string.IsNullOrWhiteSpace(SearchText))
                return true;        // found the UI doesn't block the user's attempt to search for nothing

            if (0 == Statics.WithLVprojectVM(p => p?.CanLoadCount ?? 0))
                return true;        // found there are no volumes loaded

            LVitem_SearchVM.RootText = Nicknames;   // bool: root text is nickname or volume group based
            ClearItems();
            TabledString<TabledStringType_Files>.GenerationStarting();

            var result = new SearchResultsDir();
            var treeNode = LocalTV.GetOneNodeByRootPathA(strPath, null);

            if (null != treeNode)
            {
                result.PathBuilder = PathBuilder.FactoryCreateOrFind(strPath);
            }
            else
            {
                var nLastBackSlashIx = strPath.LastIndexOf('\\');

                if (2 > nLastBackSlashIx)
                    return false;

                result.PathBuilder = PathBuilder.FactoryCreateOrFind(strPath.Substring(0, nLastBackSlashIx));
                treeNode = LocalTV.GetOneNodeByRootPathA("" + result.PathBuilder, null);

                if (null == treeNode)
                    return false;

                result.ListFiles.Add((TabledString<TabledStringType_Files>)strPath.Substring(nLastBackSlashIx + 1), false);
            }

            var lvItemProjectVM = treeNode.Root.NodeDatum.As<RootNodeDatum>().LVitemProjectVM;

            ((ISearchStatus)this).Status(new SearchResults(strPath, lvItemProjectVM, new[] { result }));
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
            lock (_lsSearchResults)     // Each status represents a listing file, so this insert-sort is infrequent; small
            {
                if (Nicknames)
                {
                    _lsSearchResults.Insert(_lsSearchResults.TakeWhile(r =>
                        searchResults.LVitemProjectVM.RootText.CompareTo(r.LVitemProjectVM.RootText) >= 0).Count(),
                        searchResults);
                }
                else
                {
                    _lsSearchResults.Insert(_lsSearchResults.TakeWhile(r =>
                        searchResults.LVitemProjectVM.SourcePath.CompareTo(r.LVitemProjectVM.SourcePath) >= 0).Count(),
                        searchResults);
                }
            }
        }

        void ISearchStatus.Done()
        {
            _searchType2 = null;

            if (false == _bDisposed)
                TabledString<TabledStringType_Files>.GenerationEnded();

            bool bClosed = false;   // A Nicety (CloseForced can take care of itself)

            try
            {
                for (;;)
                {
                    var results = _lsSearchResults.FirstOrDefault();

                    if (null == results)
                        break;

                    _lsSearchResults.RemoveAt(0);

                    if (false == _bDisposed)
                        Util.UIthread(99809, () => Add(MakeLVitems(results)));

                    if (bClosed)
                        continue;

                    // first time through
                    WinProgress.CloseForced();
                    bClosed = true;
                }
            }
            catch (ArgumentOutOfRangeException) { } // ConfirmClose() below calls _lsSearchResults.Clear()
            catch (InvalidOperationException) { }   // user restarted search during Block() in ListViewItemVM_Base.Add(): enumeration can't continue
            catch (Exception e) when ((e is ArgumentNullException) || (e is NullReferenceException))
            {
                Util.Assert(99878, _bDisposed);
            }
            catch (OutOfMemoryException)
            {
                Util.Assert(99660, false, "OutOfMemoryException in Search");
            }

            if (false == bClosed)
                WinProgress.CloseForced();
        }

        bool IWinProgressClosing.ConfirmClose()
        {
            _searchType2?.Abort();
            _searchType2 = null;
            _lsSearchResults.Clear();
            return true;
        }

        IEnumerable<LVitem_SearchVM>
            MakeLVitems(SearchResults searchResults)
        {
            PathBuilder LastFolder = null;
            var nPrevHasFolder = 1;

            foreach (var searchResult in searchResults.Results)
            {
                if (_bDisposed)
                    break;

                // SearchResults.PathBuilder has a \ at the end for folder & file search where folder matches,
                // because the key would dupe for file matches.
                var Directory = PathBuilder.FactoryCreateOrFind(("" + searchResult.PathBuilder).TrimEnd('\\'));
                var bHasFolder = (LastFolder == Directory);

                if (0 < (searchResult.ListFiles?.Count ?? 0))
                {
                    foreach (var tabledStringFilename in searchResult.ListFiles.Keys)
                    {
                        if (_bDisposed)
                            break;

                        yield return (new LVitem_SearchVM
                        {
                            Directory = Directory,
                            TabledStringFilename = tabledStringFilename,
                            Alternate = (bHasFolder) ? nPrevHasFolder : 0
                        });
                    }

                    nPrevHasFolder = ((false == bHasFolder) || (2 == nPrevHasFolder)) ? 1 : 2;
                }
                else
                {
                    yield return (new LVitem_SearchVM { Directory = Directory });
                    LastFolder = Directory;
                }
            }
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

            var ieLVitems =
                lsTreeNodes
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
                _lsSearchResults = new List<SearchResults>();

                _searchType2 =
                    new SearchListings(Statics.LVprojectVM_Copy, new SearchBase
                (
                    SearchText,
                    SearchText.ToLower() != SearchText,
                    SearchBase.FolderSpecialHandling.None,
                    bSearchFilesOnly,
                    null,
                    Regex,
                    Nicknames,
                    new WeakReference<ISearchStatus>(this)
                ))
                    .DoThreadFactory();
            })
            {
                WindowClosingCallback = new WeakReference<IWinProgressClosing>(this)
            })
                .ShowDialog();
        }

        List<SearchResults>
            _lsSearchResults = null;
        SearchListings
            _searchType2 = null;
        bool
            _bDisposed = false;
        const string
            _ksSearchKey = "Searching";
    }
}
