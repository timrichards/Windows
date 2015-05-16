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
            
            DataContext = new WinDoubleFile_SearchVM
            {
                IsEditBoxNonEmpty = () => false == string.IsNullOrWhiteSpace(formEdit_search.Text)
            }.Init();
        }
    }
}
