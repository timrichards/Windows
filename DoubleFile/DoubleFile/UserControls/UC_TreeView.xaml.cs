namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinTreeView.xaml
    /// </summary>
    partial class UC_TreeView
    {
        public UC_TreeView()
        {
            InitializeComponent();
        }

        protected override void
            LocalNavigatedTo() => form_tv.DataContext = UC_TreeViewVM.FactoryCreate();
        protected override void
            LocalNavigatedFrom() => form_tv.DataContext = null;
    }
}
