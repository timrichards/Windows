using System;

namespace DoubleFile
{
    partial class WinDoubleFile_SearchVM
    {
        internal WinDoubleFile_SearchVM()
        {
            Icmd_Folders = new RelayCommand(param => SearchFolders(), param => false == string.IsNullOrWhiteSpace(SearchText));
            Icmd_FoldersAndFiles = new RelayCommand(param => SearchFoldersAndFiles(), param => false == string.IsNullOrWhiteSpace(SearchText));
            Icmd_Files = new RelayCommand(param => SearchFiles(), param => false == string.IsNullOrWhiteSpace(SearchText));
            Icmd_Goto = new RelayCommand(param => Goto(), param => null != _selectedItem);
            Items.Add(new LVitem_DoubleFile_SearchVM(new[] { "Not implemented yet." }));
        }

        void SearchFolders()
        {
        }

        void SearchFoldersAndFiles()
        {
        }

        void SearchFiles()
        {
        }

        private void Goto()
        {
        }
    }
}
