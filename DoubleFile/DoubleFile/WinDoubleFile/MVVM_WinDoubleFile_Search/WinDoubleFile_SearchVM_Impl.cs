using System;

namespace DoubleFile
{
    partial class WinDoubleFile_SearchVM
    {
        internal WinDoubleFile_SearchVM()
        {
            Icmd_Folders = new RelayCommand(param => SearchFolders());
            Icmd_Goto = new RelayCommand(param => Goto(), param => null != _selectedItem);
        }

        void SearchFolders()
        {
            MBoxStatic.ShowDialog(SearchText);
        }

        private object Goto()
        {
            throw new NotImplementedException();
        }
    }
}
