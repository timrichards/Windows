using System;
using System.Collections.Generic;

namespace DoubleFile
{
    class LV_TreeListSiblingsVM : ListViewVM_GenericBase<LVitem_TreeListVM>, IDisposable
    {
        public LVitem_TreeListVM SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;

                if (null == value)
                {
                    return;
                }

                _lvChildrenVM.Populate(_selectedItem.LocalTreeNode);
                RaisePropertyChanged("SelectedItem");
            }
        }
        LVitem_TreeListVM _selectedItem = null;

        public string WidthFolder { get { return SCW; } }                   // franken all NaN

        internal override int NumCols { get { return LVitem_TreeListVM.NumCols_; } }

        internal LV_TreeListSiblingsVM(LV_TreeListChildrenVM lvChildrenVM)
        {
            _lvChildrenVM = lvChildrenVM;
            Local.UC_TreeMap.TreeMapRendered += Populate;
        }

        public void Dispose()
        {
            Local.UC_TreeMap.TreeMapRendered -= Populate;
        }

        void Populate(LocalTreeNode treeNodeSel)
        {
            var treeNodes =
                (null != treeNodeSel.Parent)
                ? treeNodeSel.Parent.Nodes
                : treeNodeSel.TreeView.Nodes;

            var lsLVitems = new List<LVitem_TreeListVM>();
            LVitem_TreeListVM selectedItem = null;

            foreach (var treeNode in treeNodes)
            {
                var lvItem = new LVitem_TreeListVM(new[] { treeNode.Name });

                lvItem.LocalTreeNode = treeNode;
                lsLVitems.Add(lvItem);

                if ((null == selectedItem) &&
                    object.ReferenceEquals(treeNode, treeNodeSel))
                {
                    selectedItem = lvItem;
                }
            }

            UtilProject.UIthread(() =>
            {
                Items.Clear();
                Add(lsLVitems);
            });

            SelectedItem = selectedItem;
        }

        LV_TreeListChildrenVM _lvChildrenVM = null;
    }
}
