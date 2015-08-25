namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinDuplicates.xaml
    /// </summary>
    partial class UC_Duplicates
    {
        public UC_Duplicates()
        {
            InitializeComponent();
            CantDupeThisUsercontrol = true;
        }

        protected override void LocalNavigatedTo()
        {
            DataContext =
                _winDuplicatesVM =
                new UC_DuplicatesVM
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

        UC_DuplicatesVM
            _winDuplicatesVM = null;
        bool
            _bNicknames = false;
    }
}
