using System;
using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    class LV_TreeListSiblingsVM : ListViewVM_GenericBase<LVitem_TreeListVM>, IDisposable
    {
        static internal event Action<LocalTreeNode> TreeListSiblingSelected = null;

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

                if (null != TreeListSiblingSelected)
                    TreeListSiblingSelected(value.LocalTreeNode);

                SelectedItem_AllTriggers();
            }
        }
        internal void SelectedItem_Set(LVitem_TreeListVM value)
        {
            if (value == _selectedItem)
                return;

            _selectedItem = value;
            RaisePropertyChanged("SelectedItem");
            SelectedItem_AllTriggers();
        }
        void SelectedItem_AllTriggers()
        {
            if (null == _selectedItem)
                return;

            _lvChildrenVM.Populate(_selectedItem.LocalTreeNode);
        }
        LVitem_TreeListVM _selectedItem = null;

        public string WidthFolder { get { return SCW; } }                   // franken all NaN

        internal override int NumCols { get { return LVitem_TreeListVM.NumCols_; } }

        internal LV_TreeListSiblingsVM(LV_TreeListChildrenVM lvChildrenVM)
        {
            _lvChildrenVM = lvChildrenVM;
            UC_TreeMap.TreeMapRendered += Populate;
            UC_TreeMap.TreeMapChildSelected += UC_TreeMap_TreeMapChildSelected;
        }

        void UC_TreeMap_TreeMapChildSelected(LocalTreeNode treeNodeChild)
        {
            if (_treeNode != treeNodeChild.Parent)
                return;

            ItemsCast
                .Where(lvItem => lvItem.LocalTreeNode == _treeNode)
                .FirstOnlyAssert(SelectedItem_Set);

            _lvChildrenVM.ItemsCast
                .Where(lvItem => lvItem.LocalTreeNode == treeNodeChild)
                .FirstOnlyAssert(lvItem => _lvChildrenVM.SelectedItem_Set(lvItem));
        }

        public void Dispose()
        {
            UC_TreeMap.TreeMapRendered -= Populate;
        }

        void Populate(LocalTreeNode treeNodeSel)
        {
            var treeNodes =
                (null != treeNodeSel.Parent)
                ? treeNodeSel.Parent.Nodes
                : LocalTreeNode.TreeView.Nodes.AsEnumerable();

            var lsLVitems = new List<LVitem_TreeListVM>();
            LVitem_TreeListVM selectedItem = null;

            foreach (var treeNode in treeNodes)
            {
                var lvItem = new LVitem_TreeListVM(new[] { treeNode.Name }) { LocalTreeNode = treeNode };

                lsLVitems.Add(lvItem);

                if ((null == selectedItem) &&
                    ReferenceEquals(treeNode, treeNodeSel))
                {
                    selectedItem = lvItem;
                }

                _treeNode = treeNodeSel;
            }

            SelectedItem_Set(null);

            UtilProject.UIthread(() =>
            {
                Items.Clear();
                Add(lsLVitems);
            });

            SelectedItem_Set(selectedItem);
        }

        LocalTreeNode
            _treeNode = null;
        readonly LV_TreeListChildrenVM
            _lvChildrenVM = null;
    }
}
