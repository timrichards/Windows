namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for UC_SolitaryHere.xaml
    /// </summary>
    public partial class UC_ClonesHere
    {
        public UC_ClonesHere()
        {
            InitializeComponent();
        }

        protected override void LocalNavigatedTo()
        {
            DataContext = 
                _vm =
                new UC_ClonesHereVM { UseNicknames = _bNicknames }
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

        UC_ClonesHereVM
            _vm = null;
        bool
            _bNicknames = false;
    }
}
