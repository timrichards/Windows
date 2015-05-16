using System;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinDoubleFile_Duplicates.xaml
    /// </summary>
    partial class WinDoubleFile_Duplicates
    {
        protected override void LocalNavigatedTo()
        {
            LocalDispose_WindowClosed();

            DataContext =
                _winDoubleFile_DuplicatesVM =
                new WinDoubleFile_DuplicatesVM();
        }

        public WinDoubleFile_Duplicates()
        {
            InitializeComponent();
        }

        protected override void CopyTag_NewWindow(WeakReference wr)
        {
            LocalNavigatedTo();
        }

        protected override void LocalDispose_WindowClosed()
        {
            if (null != _winDoubleFile_DuplicatesVM)
                _winDoubleFile_DuplicatesVM.Dispose();
        }

        WinDoubleFile_DuplicatesVM
            _winDoubleFile_DuplicatesVM = null;
    }
}
