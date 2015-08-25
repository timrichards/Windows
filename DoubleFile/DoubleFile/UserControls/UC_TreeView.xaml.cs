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

        protected override void LocalNavigatedTo()
        {
            var items = UC_TreeViewVM.FactoryCreate();

            form_tv.DataContext = items;

            if (0 == items.Count)
                return;

            var folderDetail = LocalTV.TreeSelect_FolderDetail;

            if (null == folderDetail)   // do not kick off a new TreeSelect when navigating to
                items[0].SelectedItem_Set(true, nInitiator: 0);
        }

        protected override void LocalNavigatedFrom()
        {
            form_tv.DataContext = null;
        }
    }
}
