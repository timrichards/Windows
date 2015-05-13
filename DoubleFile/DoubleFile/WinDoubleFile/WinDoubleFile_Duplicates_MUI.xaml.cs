namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinDoubleFile_Duplicates.xaml
    /// </summary>
    partial class WinDoubleFile_Duplicates_MUI
    {
        protected override void LocalNavigatedTo()
        {
            if (null != _winDoubleFile_DuplicatesVM)
                _winDoubleFile_DuplicatesVM.Dispose();

            DataContext =
                _winDoubleFile_DuplicatesVM =
                new WinDoubleFile_DuplicatesVM();
        }

        public WinDoubleFile_Duplicates_MUI()
        {
            InitializeComponent();
        }

        WinDoubleFile_DuplicatesVM
            _winDoubleFile_DuplicatesVM = null;
    }
}
