namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinFolderList.xaml
    /// </summary>
    partial class WinFolderList
    {
        public WinFolderList()
        {
            InitializeComponent();            
        }

        protected override void LocalFragmentNavigation(string strFragment)
        {
            DataContext = 
                _winFolderListVM =
                new WinFolderListVM(strFragment)
                .Init();
        }

        protected override void LocalNavigatedFrom()
        {
            DataContext =
                _winFolderListVM =
                null;
        }

        WinFolderListVM
            _winFolderListVM = null;
    }
}
