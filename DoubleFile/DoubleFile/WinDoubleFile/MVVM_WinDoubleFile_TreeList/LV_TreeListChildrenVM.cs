using System;
using System.Collections.Generic;

namespace DoubleFile
{
    class LV_TreeListChildrenVM : ListViewVM_GenericBase<LVitem_TreeListVM>
    {
        static internal event Action<LocalTreeNode> TreeListChildSelected = null;

        public LVitem_TreeListVM SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value == _selectedItem)
                    return;

                _selectedItem = value;
                RaisePropertyChanged("SelectedItem");

                if (null == value)
                    return;

                if (null != TreeListChildSelected)
                    TreeListChildSelected(value.LocalTreeNode);
            }
        }
        LVitem_TreeListVM _selectedItem = null;

        public string WidthFolder { get { return SCW; } }                   // franken all NaN

        internal override int NumCols { get { return LVitem_TreeListVM.NumCols_; } }

        internal void Populate(LocalTreeNode treeNodeParent)
        {
            var lsLVitems = new List<LVitem_TreeListVM>();

            foreach (var treeNode in treeNodeParent.Nodes)
            {
                var lvItem = new LVitem_TreeListVM(new[] { treeNode.Name });

                lvItem.LocalTreeNode = treeNode;
                lsLVitems.Add(lvItem);
            }

            UtilProject.UIthread(() =>
            {
                Items.Clear();
                Add(lsLVitems);
            });
        }
    }
}
