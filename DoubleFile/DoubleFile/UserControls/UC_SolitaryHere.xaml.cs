namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for UC_SolitaryHere.xaml
    /// </summary>
    public partial class UC_SolitaryHere
    {
        public UC_SolitaryHere()
        {
            InitializeComponent();
        }

        protected override void LocalNavigatedTo()
        {
            DataContext = 
                _vm =
                new UC_SolitaryHereVM { UseNicknames = _bNicknames }
                .Init();
        }

        protected override void LocalNavigatedFrom()
        {
            _bNicknames = form_ucFolderList.formChk_Nicknames.IsChecked ?? false;
            _vm?.Dispose();

            DataContext =
                _vm =
                null;
        }

        UC_SolitaryHereVM
            _vm = null;
        bool
            _bNicknames = false;
    }
}
