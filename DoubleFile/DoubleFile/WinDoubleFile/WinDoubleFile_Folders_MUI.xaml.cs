using System;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinDoubleFile_Folders.xaml
    /// </summary>
    partial class WinDoubleFile_Folders_MUI
    {
        protected override void LocalNavigatedTo()
        {
            form_tv.DataContext = null;
            new TreeView_DoubleFileVM(form_tv, LocalTV.RootNodes);
        }

        public WinDoubleFile_Folders_MUI()
        {
            InitializeComponent();
        }

        protected override void CopyTag_NewWindow(WeakReference wr)
        {
            LocalNavigatedTo();
        }
    }
}
