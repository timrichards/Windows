namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinDoubleFile_Files.xaml
    /// </summary>
    partial class WinDoubleFile_Files_MUI
    {
        protected override void LocalNavigatedTo()
        {
            if (null != _lvDoubleFile_FilesVM)
                _lvDoubleFile_FilesVM.Dispose();

            DataContext =
                _lvDoubleFile_FilesVM =
                new LV_DoubleFile_FilesVM();
        }

        public WinDoubleFile_Files_MUI()
        {
            InitializeComponent();
        }

        LV_DoubleFile_FilesVM
            _lvDoubleFile_FilesVM = null;
    }
}
