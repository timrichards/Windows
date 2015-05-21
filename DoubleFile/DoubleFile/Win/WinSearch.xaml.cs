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

        protected override void CopyTag_NewWindow(System.WeakReference weakReference)
        {
            LocalNavigatedTo();
        }

        protected override void LocalDispose_WindowClosed()
        {
            if (null != _winSearchVM)
                _winSearchVM.Dispose();
        }

        WinSearchVM
            _winSearchVM = null;
    }
}
