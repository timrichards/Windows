namespace DoubleFile
{
    partial class WinFolderListVM
    {
        internal WinFolderListVM Init()
        {
            Icmd_GoTo = new RelayCommand(GoTo, () => null != _selectedItem);
            return this;
        }

        void GoTo()
        {
            if (null == _selectedItem)
            {
                MBoxStatic.Assert(99897, false);    // binding should dim the button
                return;
            }

            _selectedItem.LocalTreeNode.GoToFile(null);
        }
    }
}
