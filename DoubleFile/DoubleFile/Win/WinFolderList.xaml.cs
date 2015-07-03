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

        protected override void LocalNavigatedTo()
        {
            DataContext = 
                _winFolderListVM =
                new WinFolderListVM()
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
