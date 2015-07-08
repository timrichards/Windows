namespace DoubleFile
{
    partial class WinFolderListVM
    {
        internal WinFolderListVM Init(string strFragment)
        {
            Icmd_GoTo = new RelayCommand(GoTo, () => null != _selectedItem);

            switch (strFragment)
            {
                case MainWindow.FolderListLarge:
                {
                    Util.WriteLine("FolderListLarge");
                    break;
                }

                case MainWindow.FolderListSmall:
                {
                    Util.WriteLine("FolderListSmall");
                    break;
                }

                case MainWindow.FolderListRandom:
                {
                    Util.WriteLine("FolderListRandom");
                    break;
                }

                case MainWindow.FolderListUnique:
                {
                    Util.WriteLine("FolderListUnique");
                    break;
                }

                case MainWindow.FolderListSameVol:
                {
                    Util.WriteLine("FolderListSameVol");
                    break;
                }

                case MainWindow.FolderListClones:
                {
                    Util.WriteLine("FolderListClones");
                    break;
                }

                default:
                {
                    MBoxStatic.Assert(99887, false);
                    break;
                }
            }

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
