namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinDuplicates.xaml
    /// </summary>
    partial class WinSearch
    {
        public WinSearch()
        {
            InitializeComponent();            
        }

        protected override void LocalNavigatedTo()
        {
            DataContext = 
                _winSearchVM =
                new WinSearchVM
            {
                IsEditBoxNonEmpty = () => false == string.IsNullOrWhiteSpace(formEdit_search.Text)
            }
                .Init();
        }

        protected override void LocalNavigatedFrom()
        {
            _winSearchVM.Dispose();

            DataContext =
                _winSearchVM =
                null;
        }

        WinSearchVM
            _winSearchVM = null;
    }
}
