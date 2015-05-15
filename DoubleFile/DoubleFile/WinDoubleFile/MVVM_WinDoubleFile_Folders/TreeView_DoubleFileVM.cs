using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Reactive.Linq;

namespace DoubleFile
{
    class TreeView_DoubleFileVM
    {
        internal TreeViewItem_DoubleFileVM
            SelectedItem { get; set; }
        internal Dictionary<TreeViewItem_DoubleFileVM, bool>
            _listExpanded = new Dictionary<TreeViewItem_DoubleFileVM, bool>();

        internal TreeView_DoubleFileVM(TreeView tvfe, IEnumerable<LocalTreeNode> rootNodes)
        {
            var nIndex = -1;

            foreach (var treeNode in rootNodes)
                _Items.Add(new TreeViewItem_DoubleFileVM(this, treeNode, ++nIndex));

            UtilProject.UIthread(() => tvfe.DataContext = _Items);

            Observable.Timer(TimeSpan.FromMilliseconds(33)).Timestamp()
                .Subscribe(x => { if (0 < _Items.Count) _Items[0].SelectedItem_Set(); });
        }

        readonly ObservableCollection<TreeViewItem_DoubleFileVM>
            _Items = new ObservableCollection<TreeViewItem_DoubleFileVM>();
    }
}
