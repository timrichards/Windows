using System.Collections.Generic;
using System.Globalization;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinFolderList.xaml
    /// </summary>
    partial class UC_FolderList
    {
        public UC_FolderList()
        {
            InitializeComponent();
            CantDupeThisUsercontrol = true;
        }

        protected override void LocalNavigatedTo()
        {
            DataContext = 
                _vm =
                new UC_FolderListVM { UseNicknames = _bNicknames }
                .Init();
        }

        protected override void LocalNavigatedFrom()
        {
            _bNicknames = formChk_Nicknames.IsChecked ?? false;
            _vm?.Dispose();

            DataContext =
                _vm =
                null;
        }

        UC_FolderListVM
            _vm = null;
        bool
            _bNicknames = false;
    }
}
