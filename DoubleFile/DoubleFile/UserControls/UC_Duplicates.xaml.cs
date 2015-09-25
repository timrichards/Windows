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
                _vm =
                new UC_DuplicatesVM
            {
                UseNicknames = _bNicknames
            }
                .Init();
        }

        protected override void LocalNavigatedFrom()
        {
            _bNicknames = _vm.UseNicknames;
            _vm?.Dispose();

            DataContext =
                _vm =
                null;
        }

        UC_DuplicatesVM
            _vm = null;
        bool
            _bNicknames = false;
    }
}
