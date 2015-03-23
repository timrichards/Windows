using System;
using System.Collections.Generic;

namespace DoubleFile
{
    partial class WinDoubleFile_SearchVM
    {
        internal Func<bool> IsSearchEnabled = null;

        internal WinDoubleFile_SearchVM()
        {
            Icmd_Folders = new RelayCommand(param => SearchFolders(), param => IsSearchEnabled());
            Icmd_FoldersAndFiles = new RelayCommand(param => SearchFoldersAndFiles(), param => IsSearchEnabled());
            Icmd_Files = new RelayCommand(param => SearchFiles(), param => IsSearchEnabled());
            Icmd_Goto = new RelayCommand(param => Goto(), param => null != _selectedItem);
        }

        void SearchFolders()
        {
        }

        void SearchFoldersAndFiles()
        {
        }

        void SearchFiles()
        {
            UtilProject.UIthread(Items.Clear);

            _searchType2 = new SearchType2
                (
                    GlobalData.static_MainWindow.LVprojectVM,
                    SearchText,
                    SearchText.ToLower() != SearchText,
                    SearchType2.FolderSpecialHandling.None,
                    true,
                    null,
                    SearchStatusCallback,
                    SearchDoneCallback
                )
                .DoThreadFactory();
        }

        void Goto()
        {
        }

        void SearchStatusCallback(SearchResults searchResults, bool bFirst = false, bool bLast = false)
        {
            var lsLVitems = new List<LVitem_DoubleFile_SearchVM>();

            foreach (var searchResult in searchResults.Results)
            {
                if ((null != searchResult.ListFiles) &&
                    (false == searchResult.ListFiles.IsEmpty()))
                {
                    foreach (var strFile in searchResult.ListFiles)
                    {
                        lsLVitems.Add(new LVitem_DoubleFile_SearchVM(new[] { strFile + " in " + searchResult.StrDir }));
                    }
                }
                else
                {
                    lsLVitems.Add(new LVitem_DoubleFile_SearchVM(new[] { searchResult.StrDir }));
                }
            }

            UtilProject.UIthread(() => Add(lsLVitems));
        }

        void SearchDoneCallback()
        {
            _searchType2 = null;
        }

        SearchType2
            _searchType2 = null;
    }
}
