using System;
using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    class LV_TreeListSiblingsVM : ListViewVM_Base<LVitem_TreeListVM>, IDisposable
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

                TreeSelect.DoThreadFactory(value.LocalTreeNode, nInitiator: _kTreeSelect);
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
            _lsDisposable.Add(TreeSelect.FolderDetailUpdated.Subscribe(TreeSelect_FolderDetailUpdated));
        }

        internal void CopyFrom(LV_TreeListSiblingsVM vm)
        {
            Populate(vm._treeNode);
        }

        public void Dispose()
        {
            foreach (var d in _lsDisposable)
                d.Dispose();
        }

        void TreeSelect_FolderDetailUpdated(Tuple<Tuple<IEnumerable<IEnumerable<string>>, LocalTreeNode>, int> initiatorTuple)
        {
            if (new[] { _kTreeSelect, LV_TreeListChildrenVM.kChildSelectedOnNext }
                .Contains(initiatorTuple.Item2))
            {
                return;
            }

            var tuple = initiatorTuple.Item1;

            UtilDirList.Write("L");

            if (tuple.Item2 == _treeNode)
                return;

            if (UC_TreeMap.kSelRectAndTooltip != initiatorTuple.Item2)
            {
                Populate(tuple.Item2);
                return;
            }

            if (null == _treeNode)
                return;

            if (tuple.Item2.Parent != _treeNode)    // no-op on descending treemap subfolders.
                return;

            ItemsCast
                .Where(lvItem => lvItem.LocalTreeNode == _treeNode)
                .FirstOnlyAssert(SelectedItem_Set);

            _lvChildrenVM.ItemsCast
                .Where(lvItem => lvItem.LocalTreeNode == tuple.Item2)
                .FirstOnlyAssert(_lvChildrenVM.SelectedItem_Set);
        }

        void Populate(LocalTreeNode treeNodeSel)
        {
            if (treeNodeSel == _treeNode)
                return;

            _treeNode = treeNodeSel;
            ClearItems();

            if (null == _treeNode)
                return;

            UtilDirList.Write("K");

            var treeNodes =
                (null != _treeNode.Parent)
                ? _treeNode.Parent.Nodes
                : LocalTV.RootNodes;

            var lsLVitems = new List<LVitem_TreeListVM>();
            LVitem_TreeListVM selectedItem = null;

            foreach (var treeNode in treeNodes)
            {
                var lvItem = new LVitem_TreeListVM(new[] { treeNode.Name }) { LocalTreeNode = treeNode };

                lsLVitems.Add(lvItem);

                if ((null == selectedItem) &&
                    ReferenceEquals(treeNode, _treeNode))
                {
                    selectedItem = lvItem;
                }
            }

            UtilProject.UIthread(() => Add(lsLVitems));
            SelectedItem_Set(selectedItem);
        }

        const int
            _kTreeSelect = 99984;
        LocalTreeNode
            _treeNode = null;
        readonly LV_TreeListChildrenVM
            _lvChildrenVM = null;
        List<IDisposable>
            _lsDisposable = new List<IDisposable>();
    }
}
