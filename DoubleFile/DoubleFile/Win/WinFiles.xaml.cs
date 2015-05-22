using System;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinFiles.xaml
    /// </summary>
    partial class WinFiles
    {
        protected override void LocalNavigatedTo()
        {
            DataContext =
                _lvFilesVM =
                new LV_FilesVM();
        }

        public WinFiles()
        {
            InitializeComponent();
        }

        protected override void LocalNavigatedFrom()
        {
            if (null != _lvFilesVM)
                _lvFilesVM.Dispose();
        }

        LV_FilesVM
            _lvFilesVM = null;
    }
}
