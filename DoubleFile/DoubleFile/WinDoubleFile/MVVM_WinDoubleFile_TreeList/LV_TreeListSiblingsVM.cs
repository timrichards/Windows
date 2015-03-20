using System;
using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    class LV_TreeListSiblingsVM : ListViewVM_GenericBase<LVitem_TreeListVM>, IDisposable
    {
        public LVitem_TreeListVM SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (value == _selectedItem)
                    return;

                _selectedItem = value;

                if (null == value)
                    return;

                _lvChildrenVM.Populate(value.LocalTreeNode);
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
            Local.UC_TreeMap.TreeMapChildSelected += UC_TreeMap_TreeMapChildSelected;
        }

        void UC_TreeMap_TreeMapChildSelected(LocalTreeNode treeNodeChild)
        {
            if (_treeNode != treeNodeChild.Parent)
                return;

            ItemsCast
                .Where(lvItem => lvItem.LocalTreeNode == _treeNode)
                .FirstOnlyAssert(lvItem => SelectedItem = lvItem);

            _lvChildrenVM.ItemsCast
                .Where(lvItem => lvItem.LocalTreeNode == treeNodeChild)
                .FirstOnlyAssert(lvItem => _lvChildrenVM.SelectedItem = lvItem);
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

                _treeNode = treeNodeSel;
            }

            UtilProject.UIthread(() =>
            {
                Items.Clear();
                Add(lsLVitems);
            });

            SelectedItem = selectedItem;
        }

        LocalTreeNode _treeNode = null;
        LV_TreeListChildrenVM _lvChildrenVM = null;
    }
}
