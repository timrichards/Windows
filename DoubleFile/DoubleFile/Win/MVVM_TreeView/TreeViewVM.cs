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

        static internal ObservableCollection<TreeViewItemVM> FactoryCreate(TreeView tvfe)
        {
            return
                new TreeViewVM(LocalTV.RootNodes)
                ._Items;
        }

        TreeViewVM(IEnumerable<LocalTreeNode> rootNodes)
        {
            var nIndex = -1;

            foreach (var treeNode in rootNodes)
                _Items.Add(new TreeViewItemVM(this, treeNode, ++nIndex));
        }

        readonly ObservableCollection<TreeViewItemVM>
            _Items = new ObservableCollection<TreeViewItemVM>();
    }
}
