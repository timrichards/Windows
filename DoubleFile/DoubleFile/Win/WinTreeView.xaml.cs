using System.Reactive.Linq;
using System;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinFolders.xaml
    /// </summary>
    partial class WinTreeView
    {
        public WinTreeView()
        {
            InitializeComponent();
        }

        protected override void LocalNavigatedTo()
        {
            var items = TreeViewVM.FactoryCreate();

            form_tv.DataContext = items;

            if (0 >= items.Count)
                return;

            var folderDetail = LocalTV.TreeSelect_FolderDetail;

            if (null == folderDetail)   // do not reset selected item when navigating to
                items[0].SelectedItem_Set(true, nInitiator: 0);
        }

        protected override void LocalNavigatedFrom()
        {
            form_tv.DataContext = null;
        }
    }
}
