﻿using System;
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
            _bDisposed = true;
            TabledString<TypedArray1>.DropRef();
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

        bool Reinitialize_And_FullPathFound(string strPath)
        {
            UtilProject.UIthread(Items.Clear);
            TabledString<TypedArray1>.Reinitialize();

            var result = new SearchResultsDir();

            if (null != LocalTV.GetOneNodeByRootPathA(strPath, null))
            {
                result.StrDir = PathBuilder<TypedArray1>.FactoryCreateOrFind(strPath);
            }
            else
            {
                var nLastBackSlashIx = strPath.LastIndexOf('\\');

                if (2 > nLastBackSlashIx)
                    return false;

                result.StrDir = PathBuilder<TypedArray1>.FactoryCreateOrFind(strPath.Substring(0, nLastBackSlashIx));

                if (null == LocalTV.GetOneNodeByRootPathA(result.StrDir.ToString(), null))
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
            foreach (var result in searchResults.Results)
                _dictResults.Add(result, false);
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
                    var Directory = PathBuilder<TypedArray1>.FactoryCreateOrFind(searchResult.StrDir.ToString().TrimEnd('\\'));

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
                TabledString<TypedArray1>.GenerationEnded();
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
