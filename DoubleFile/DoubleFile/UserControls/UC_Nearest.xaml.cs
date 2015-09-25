namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinFolderList.xaml
    /// </summary>
    partial class UC_Nearest
    {
        public UC_Nearest()
        {
            InitializeComponent();
            CantDupeThisUsercontrol = true;
        }

        protected override void LocalNavigatedTo()
        {
            DataContext = 
                _vm =
                new UC_NearestVM { UseNicknames = _bNicknames }
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

        UC_NearestVM
            _vm = null;
        bool
            _bNicknames = false;
    }
}
