using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Reactive.Linq;

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

            Util.UIthread(() => tvfe.DataContext = _Items);

            Observable.Timer(TimeSpan.FromMilliseconds(33)).Timestamp()
                .Subscribe(x => { if (0 < _Items.Count) _Items[0].SelectedItem_Set(true, nInitiator: 0); });
        }

        readonly ObservableCollection<TreeViewItemVM>
            _Items = new ObservableCollection<TreeViewItemVM>();
    }
}
