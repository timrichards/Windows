﻿using System;
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

        internal WinSearchVM Init()
        {
            Icmd_Folders = new RelayCommand(SearchFolders, IsSearchEnabled);
            Icmd_FoldersAndFiles = new RelayCommand(() => SearchFoldersAndFiles(), IsSearchEnabled);
            Icmd_Files = new RelayCommand(() => SearchFoldersAndFiles(bSearchFilesOnly: true), IsSearchEnabled);
            Icmd_GoTo = new RelayCommand(GoTo, () => null != _selectedItem);
            TabledString<Tabled_Files>.AddRef();
            return this;
        }

        public void Dispose()
        {
            _bDisposed = true;
            TabledString<Tabled_Files>.DropRef();
        }

        bool IsSearchEnabled() => IsEditBoxNonEmpty() && (null == _searchType2);

        void SearchFolders()
        {
            if (Reinitialize_And_FullPathFound(SearchText))
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

            if (false == ieLVitems.Any())
                return;

            Util.UIthread(99816, () => Add(ieLVitems));
        }

        void SearchFoldersAndFiles(bool bSearchFilesOnly = false)
        {
            if (Reinitialize_And_FullPathFound(SearchText))
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

        bool Reinitialize_And_FullPathFound(string strPath)
        {
            if (string.IsNullOrWhiteSpace(SearchText))
                return true;        // found the UI doesn't block the user's attempt to search for nothing

            if (0 == (Statics.LVprojectVM?.CanLoadCount ?? 0))
                return true;        // found there are no volumes loaded

            ClearItems();
            TabledString<Tabled_Files>.Reinitialize();
            TabledString<Tabled_Files>.GenerationStarting();

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

                result.ListFiles.Add((TabledString<Tabled_Files>)strPath.Substring(nLastBackSlashIx + 1), false);
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
                GoToFileOnNext(Tuple.Create((LVitem_ProjectVM)null, "" + _selectedItem.Directory, "" + _selectedItem.Filename));
            else
                _selectedItem.LocalTreeNode.GoToFile(null);
        }

        void ISearchStatus.Status(SearchResults searchResults, bool bFirst, bool bLast)
        {
            try
            {
                foreach (var result in searchResults.Results)
                    _dictResults.Add(result, false);
            }
            catch (Exception ex)
            {
                Util.Assert(99942, false, "Exception in ISearchStatus.Status\n" +
                    ex.GetBaseException().Message);

                _searchType2?.EndThread();
                _searchType2 = null;
                _dictResults = null;
                Dispose();
                return;
            }
        }

        void ISearchStatus.Done()
        {
            Done();

            try
            {
                TabledString<Tabled_Files>.GenerationEnded();
            }
            catch (NullReferenceException)
            {
                Util.Assert(99875, _bDisposed);
            }
        }

        void Done()
        {
            _searchType2 = null;

            var lsLVitems = new List<LVitem_SearchVM>();

            foreach (var searchResult in _dictResults.Select(result => result.Key).OrderBy(d => d))
            {
                if (_bDisposed)
                    return;

                try
                {
                    // SearchResults.StrDir has a \ at the end for folder & file search where folder matches, because the key would dupe for file matches.
                    var Directory = PathBuilder.FactoryCreateOrFind(("" + searchResult.PathBuilder).TrimEnd('\\'));

                    if (0 < (searchResult.ListFiles?.Count ?? 0))
                    {
                        foreach (var strFile in searchResult.ListFiles.Keys)
                        {
                            if (_bDisposed)
                                return;

                            lsLVitems.Add(new LVitem_SearchVM { Directory = Directory, Filename = strFile });
                        }
                    }
                    else
                    {
                        lsLVitems.Add(new LVitem_SearchVM { Directory = Directory });
                    }
                }
                catch (Exception ex) when (ex is ArgumentNullException || ex is NullReferenceException)
                {
                    Util.Assert(99878, _bDisposed);
                    _dictResults = null;
                    return;
                }
                catch (OutOfMemoryException)
                {
                    Util.Assert(99660, false, "OutOfMemoryException in Search");
                    _dictResults = null;
                    return;
                }
            }

            WinProgress.WithWinProgress(w =>
                w.SetCompleted(_ksSearchKey).Close());

            _dictResults = null;

            if (0 == lsLVitems.Count)
                return;

            if (_bDisposed)
                return;

            Util.UIthread(99809, () => Add(lsLVitems, Cancel: () => _bDisposed));
        }

        SearchListings
            _searchType2 = null;
        IDictionary<SearchResultsDir, bool>
            _dictResults = null;
        bool
            _bDisposed = false;
        const string
            _ksSearchKey = "Searching";
    }
}
