using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace DoubleFile
{
    class LV_TreeListSiblingsVM : ListViewVM_Base<LVitem_TreeListVM>, IDisposable
    {
        static internal IObservable<Tuple<LocalTreeNode, int>>
            TreeListSiblingSelected { get { return _treeListSiblingSelected.AsObservable(); } }
        static readonly LocalSubject<LocalTreeNode> _treeListSiblingSelected = new LocalSubject<LocalTreeNode>();
        static void TreeListSiblingSelectedOnNext(LocalTreeNode value) { _treeListSiblingSelected.LocalOnNext(value, 99849, -1); }

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

                TreeListSiblingSelectedOnNext(value.LocalTreeNode);
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
            _lsDisposable.Add(UC_TreeMap.TreeMapRendered.Subscribe(UC_TreeMap_TreeMapRendered));
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

        void TreeSelect_FolderDetailUpdated(Tuple<Tuple<IEnumerable<IEnumerable<string>>, LocalTreeNode>, int> tupleA)
        {
            MBoxStatic.Assert(tupleA.Item2 + .5, false);

            var tuple = tupleA.Item1;

            if (_lvChildrenVM.SkipOne_TreeMapChildSelected_ResetsIt)
                return;

            UtilDirList.Write("L");
            if (_treeNode != tuple.Item2.Parent)
                return;

            ItemsCast
                .Where(lvItem => lvItem.LocalTreeNode == _treeNode)
                .FirstOnlyAssert(SelectedItem_Set);

            _lvChildrenVM.ItemsCast
                .Where(lvItem => lvItem.LocalTreeNode == tuple.Item2)
                .FirstOnlyAssert(lvItem => _lvChildrenVM.SelectedItem_Set(lvItem));
        }

        void UC_TreeMap_TreeMapRendered(Tuple<LocalTreeNode, int> tuple)
        {
            MBoxStatic.Assert(tuple.Item2 + .5, false);
            Populate(tuple.Item1);
        }

        void Populate(LocalTreeNode treeNodeSel)
        {
            if (treeNodeSel == _treeNode)
                return;

            ClearItems();

            if (null == treeNodeSel)
                return;

            _treeNode = treeNodeSel;
            UtilDirList.Write("K");

            var treeNodes =
                (null != treeNodeSel.Parent)
                ? treeNodeSel.Parent.Nodes
                : LocalTV.RootNodes;

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
            }

            UtilProject.UIthread(() => Add(lsLVitems));
            SelectedItem_Set(selectedItem);
        }

        LocalTreeNode
            _treeNode = null;
        readonly LV_TreeListChildrenVM
            _lvChildrenVM = null;
        List<IDisposable>
            _lsDisposable = new List<IDisposable>();
    }
}
