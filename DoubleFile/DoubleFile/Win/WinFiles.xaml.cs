using System;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinFiles.xaml
    /// </summary>
    partial class WinFiles
    {
        public WinFiles()
        {
            InitializeComponent();
        }

        protected override void LocalNavigatedTo()
        {
            DataContext =
                _lvFilesVM =
                new LV_FilesVM();
        }

        protected override void LocalNavigatedFrom()
        {
            _lvFilesVM.Dispose();

            DataContext =
                _lvFilesVM =
                null;
        }

        LV_FilesVM
            _lvFilesVM = null;
    }
}
