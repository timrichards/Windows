using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Linq;

namespace DoubleFile
{
    class TreeView_DoubleFileVM
    {
        internal TreeViewItem_DoubleFileVM
            SelectedItem { get; set; }
        internal KeyList<TreeViewItem_DoubleFileVM>
            _listExpanded = new KeyList<TreeViewItem_DoubleFileVM>();
        internal readonly TreeView
            _TVFE = null;

        internal TreeView_DoubleFileVM(TreeView tvfe)
        {
            _TVFE = tvfe;
        }

        internal void SetData(IEnumerable<LocalTreeNode> rootNodes)
        {
            SelectedItem = null;

            var nIndex = -1;

            foreach (var treeNode in rootNodes)
            {
                _Items.Add(new TreeViewItem_DoubleFileVM(this, treeNode, ++nIndex));
            }

            UtilProject.UIthread(() => _TVFE.DataContext = _Items);

            if (0 < _Items.Count)
                _Items[0].SelectedItem_Set();
        }

        readonly ObservableCollection<TreeViewItem_DoubleFileVM>
            _Items = new ObservableCollection<TreeViewItem_DoubleFileVM>();
    }
}
