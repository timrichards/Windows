using System;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinDoubleFile_Folders.xaml
    /// </summary>
    partial class WinDoubleFile_Folders
    {
        public WinDoubleFile_Folders()
        {
            InitializeComponent();
        }

        protected override void LocalNavigatedTo()
        {
            new TreeView_DoubleFileVM(form_tv, LocalTV.RootNodes);
        }

        protected override void CopyTag_NewWindow(WeakReference wr)
        {
            LocalNavigatedTo();
        }
    }
}
