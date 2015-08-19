namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinDuplicates.xaml
    /// </summary>
    partial class WinDuplicates
    {
        public WinDuplicates()
        {
            InitializeComponent();
        }

        protected override void LocalNavigatedTo()
        {
            DataContext =
                _winDuplicatesVM =
                new WinDuplicatesVM
            {
                UseNicknames = _bNicknames
            }
                .Init();
        }

        protected override void LocalNavigatedFrom()
        {
            _bNicknames = formChk_Nicknames.IsChecked ?? false;
            _winDuplicatesVM.Dispose();

            DataContext =
                _winDuplicatesVM =
                null;
        }

        WinDuplicatesVM
            _winDuplicatesVM = null;
        bool
            _bNicknames = false;
    }
}
