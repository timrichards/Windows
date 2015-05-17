namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinDoubleFile_Duplicates.xaml
    /// </summary>
    partial class WinDoubleFile_Search
    {
        public WinDoubleFile_Search()
        {
            InitializeComponent();            
        }

        protected override void LocalNavigatedTo()
        {
            DataContext = 
                _winDoubleFile_SearchVM =
                new WinDoubleFile_SearchVM
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
            if (null != _winDoubleFile_SearchVM)
                _winDoubleFile_SearchVM.Dispose();
        }

        WinDoubleFile_SearchVM
            _winDoubleFile_SearchVM = null;
    }
}
