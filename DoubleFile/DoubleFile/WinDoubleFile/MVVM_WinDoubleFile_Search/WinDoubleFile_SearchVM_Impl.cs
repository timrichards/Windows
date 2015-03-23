using System;

namespace DoubleFile
{
    partial class WinDoubleFile_SearchVM
    {
        internal WinDoubleFile_SearchVM()
        {
            Icmd_Folders = new RelayCommand(param => SearchFolders());
            Icmd_Goto = new RelayCommand(param => Goto(), param => null != _selectedItem);
            Items.Add(new LVitem_DoubleFile_SearchVM(new[] { "Not implemented yet." }));
        }

        void SearchFolders()
        {
            MBoxStatic.ShowDialog(SearchText);
        }

        private void Goto()
        {
        }
    }
}
