using System;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinDoubleFile_Folders.xaml
    /// </summary>
    partial class WinDoubleFile_Folders
    {
        protected override void LocalNavigatedTo()
        {
            new TreeView_DoubleFileVM(form_tv, LocalTV.RootNodes);
        }

        public WinDoubleFile_Folders()
        {
            InitializeComponent();
        }

        protected override void CopyTag_NewWindow(WeakReference wr)
        {
            LocalNavigatedTo();
        }
    }
}
