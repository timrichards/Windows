using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace DoubleFile
{
    partial class UC_SearchVM : IDisposable, ISearchStatus, IProgressOverlayClosing
    {
        internal Func<bool> IsEditBoxNonEmpty = null;
        bool IsSearchEnabled() => IsEditBoxNonEmpty() && (null == _searchType2);

        static internal IObservable<Tuple<Tuple<LVitem_ProjectVM, string, string>, int>>
            GoToFile => _goToFile;
        static readonly LocalSubject<Tuple<LVitem_ProjectVM, string, string>> _goToFile = new LocalSubject<Tuple<LVitem_ProjectVM, string, string>>();
        static void GoToFileOnNext(Tuple<LVitem_ProjectVM, string, string> value) => _goToFile.LocalOnNext(value, 99982);

        internal UC_SearchVM Init()
        {
            Icmd_Folders = new RelayCommand(SearchFolders, IsSearchEnabled);
            Icmd_FoldersAndFiles = new RelayCommand(() => SearchFoldersAndFiles(), IsSearchEnabled);
            Icmd_Files = new RelayCommand(() => SearchFoldersAndFiles(bSearchFilesOnly: true), IsSearchEnabled);
            Icmd_Nicknames = new RelayCommand(() => _nicknameUpdater.UpdateViewport(UseNicknames));
            _nicknameUpdater.UpdateViewport(UseNicknames);
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

            _nicknameUpdater.Clear();
            ClearItems();
            TabledString<TabledStringType_Files>.GenerationStarting();

            var result = new SearchResultsDir();
            var treeNode = LocalTV.GetOneNodeByRootPathA(strPath, null);

            if (null != treeNode)
            {
                Util.UIthread(99862, () =>
                    Add(new LVitem_SearchVM(new LVitemProject_Updater<bool>(treeNode.Root.NodeDatum.As<RootNodeDatum>().LVitemProjectVM, _nicknameUpdater), treeNode)));

                return true;
            }
            else
            {
                var nLastBackSlashIx = strPath.LastIndexOf('\\');

                if (0 > nLastBackSlashIx)
                    return false;

                treeNode = LocalTV.GetOneNodeByRootPathA(strPath.Substring(0, nLastBackSlashIx), null);

                if (null == treeNode)
                    return false;

                result.PathBuilder = PathBuilder.FactoryCreateOrFind(treeNode.FullPathGet(false));
                result.ListFiles.Add((TabledString<TabledStringType_Files>)strPath.Substring(nLastBackSlashIx + 1), false);
            }

            var lvItemProjectVM = treeNode.Root.NodeDatum.As<RootNodeDatum>().LVitemProjectVM;

            _lsSearchResults = new List<SearchResults> { new SearchResults(strPath, lvItemProjectVM, new[] { result }) };
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
                GoToFileOnNext(Tuple.Create((LVitem_ProjectVM)_selectedItem.LVitemProject_Updater, "" + _selectedItem.Directory, "" + _selectedItem.TabledStringFilename));
            else
                _selectedItem.LocalTreeNode.GoToFile(null);
        }

        void ISearchStatus.Status(SearchResults searchResults, bool bFirst, bool bLast)
        {
            lock (_lsSearchResults)     // Each status represents a listing file, so this lock is few and infrequent
                _lsSearchResults.Add(searchResults);
        }

        void ISearchStatus.Done()
        {
            _searchType2 = null;

            if (_bDisposed)
            {
                ProgressOverlay.CloseForced();
                return;
            }

            TabledString<TabledStringType_Files>.GenerationEnded();

            if (UseNicknames)
                _lsSearchResults.Sort((x, y) => x.LVitemProjectVM.RootText.CompareTo(y.LVitemProjectVM.RootText));
            else
                _lsSearchResults.Sort((x, y) => x.LVitemProjectVM.SourcePath.CompareTo(y.LVitemProjectVM.SourcePath));

            var bClosed = false;   // A Nicety (CloseForced can take care of itself)

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
                    ProgressOverlay.CloseForced();
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
                ProgressOverlay.CloseForced();
        }

        bool IProgressOverlayClosing.ConfirmClose()
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
            var lvItemProjectSearch = new LVitemProject_Updater<bool>(searchResults.LVitemProjectVM, _nicknameUpdater);

            foreach (var searchResult in searchResults.Results)
            {
                if (_bDisposed)
                    break;

                // SearchResults.PathBuilder has a \ at the end for folder & file search where folder matches,
                // because the key would dupe for file matches. In actuality it's for file: without \ folder sorts first.
                var Directory = PathBuilder.FactoryCreateOrFind(("" + searchResult.PathBuilder).TrimEnd('\\'));
                var bHasFolder = (LastFolder == Directory);

                if (0 < (searchResult.ListFiles?.Count ?? 0))
                {
                    foreach (var tabledFilename in searchResult.ListFiles.Keys)
                    {
                        if (_bDisposed)
                            break;

                        yield return
                            new LVitem_SearchVM(lvItemProjectSearch, Directory,
                            tabledFilename, (bHasFolder) ? nPrevHasFolder : 0);
                    }

                    nPrevHasFolder = ((false == bHasFolder) || (2 == nPrevHasFolder)) ? 1 : 2;
                }
                else
                {
                    yield return new LVitem_SearchVM(lvItemProjectSearch, Directory);
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

            (new ProgressOverlay(new[] { "" }, new[] { _ksSearchKey }, x =>
            {
                Util.ThreadMake(() =>
                {
                    var blockingFrame = new LocalDispatcherFrame(99860) { Continue = true };
                    IEnumerable<ListViewItemVM_Base> ieLVitems = null;

                    Util.ThreadMake(() =>
                    {
                        ieLVitems =
                            lsTreeNodes
                            .GroupBy(treeNode => treeNode.Root)
                            .Select(g => new { lvItemProjectVM = new LVitemProject_Updater<bool>(g.Key.NodeDatum.As<RootNodeDatum>().LVitemProjectVM, _nicknameUpdater), g = g })
                            .SelectMany(g => g.g, (g, treeNode) => new LVitem_SearchVM(g.lvItemProjectVM, treeNode))
                            .OrderBy(lvItem => lvItem.LocalTreeNode.FullPathGet(_nicknameUpdater.Value))
                            .ToList();

                        blockingFrame.Continue = false;     // 2
                    });

                    // fast operation may exit ThreadMake() before this line is even hit:
                    // 2 then 1 not the reverse.
                    if (blockingFrame.Continue)             // 1
                        blockingFrame.PushFrameTrue();

                    ProgressOverlay.CloseForced();

                    if (ieLVitems.Any())
                        Util.UIthread(99816, () => Add(ieLVitems));
                });
            }))
                .ShowDialog();
        }

        void SearchFoldersAndFiles(bool bSearchFilesOnly = false)
        {
            if (GenerationStarting_And_FullPathFound(SearchText))
                return;

            (new ProgressOverlay(new[] { "" }, new[] { _ksSearchKey }, x =>
            {
                _lsSearchResults = new List<SearchResults>();

                _searchType2 =
                    new SearchListings(LocalTV.LVprojectVM, new SearchBase
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
            })
            {
                WindowClosingCallback = new WeakReference<IProgressOverlayClosing>(this)
            })
                .ShowDialog();
        }

        readonly ListUpdater<bool>
            _nicknameUpdater = new ListUpdater<bool>(99662);
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
