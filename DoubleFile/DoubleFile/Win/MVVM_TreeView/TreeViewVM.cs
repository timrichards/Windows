﻿using System.Collections.Generic;
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

        static internal void FactoryCreate(TreeView tvfe)
        {
            new TreeViewVM(tvfe, LocalTV.RootNodes);
        }

        TreeViewVM(TreeView tvfe, IEnumerable<LocalTreeNode> rootNodes)
        {
            var nIndex = -1;

            foreach (var treeNode in rootNodes)
                _Items.Add(new TreeViewItemVM(this, treeNode, ++nIndex));


            Util.UIthread(() => tvfe.DataContext = _Items);

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
