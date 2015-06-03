using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace DoubleFile
{
    class TreeViewVM
    {
        internal TreeViewItemVM
            SelectedItem { get; set; }
        internal Dictionary<TreeViewItemVM, bool>
            _listExpanded = new Dictionary<TreeViewItemVM, bool>();

        internal TreeViewVM(TreeView tvfe, IEnumerable<LocalTreeNode> rootNodes)
        {
            var nIndex = -1;

            foreach (var treeNode in rootNodes)
                _Items.Add(new TreeViewItemVM(this, treeNode, ++nIndex));

            bool bCompleted = false;

            Util.UIthread(() =>
            {
                tvfe.DataContext = _Items;
                bCompleted = true;
            });

            while (false == bCompleted)
                Util.Block(20);

            if (0 > _Items.Count)
                return;     // from lambda

            var folderDetail = LocalTV.TreeSelect_FolderDetail;

            if (null == folderDetail)
                _Items[0].SelectedItem_Set(true, nInitiator: 0);
        }

        readonly ObservableCollection<TreeViewItemVM>
            _Items = new ObservableCollection<TreeViewItemVM>();
    }
}
