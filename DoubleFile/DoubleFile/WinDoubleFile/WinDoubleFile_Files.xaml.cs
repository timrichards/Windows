using System;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinDoubleFile_Files.xaml
    /// </summary>
    partial class WinDoubleFile_Files
    {
        protected override void LocalNavigatedTo()
        {
            LocalDispose_WindowClosed();

            DataContext =
                _lvDoubleFile_FilesVM =
                new LV_DoubleFile_FilesVM();
        }

        public WinDoubleFile_Files()
        {
            InitializeComponent();
        }

        protected override void CopyTag_NewWindow(WeakReference wr)
        {
            LocalNavigatedTo();
        }

        protected override void LocalDispose_WindowClosed()
        {
            if (null != _lvDoubleFile_FilesVM)
                _lvDoubleFile_FilesVM.Dispose();
        }

        LV_DoubleFile_FilesVM
            _lvDoubleFile_FilesVM = null;
    }
}
