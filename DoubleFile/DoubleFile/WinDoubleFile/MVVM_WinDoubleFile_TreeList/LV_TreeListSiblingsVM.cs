using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace DoubleFile
{
    class LV_TreeListSiblingsVM : ListViewVM_GenericBase<LVitem_TreeListVM>, IDisposable
    {
        static internal IObservable<LocalTreeNode>
            TreeListSiblingSelected { get { return _treeListSiblingSelected.AsObservable(); } }
        static readonly Subject<LocalTreeNode> _treeListSiblingSelected = new Subject<LocalTreeNode>();

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

                _treeListSiblingSelected.OnNext(value.LocalTreeNode);
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
            _lsDisposable.Add(UC_TreeMap.TreeMapRendered.Subscribe(Populate));
            _lsDisposable.Add(UC_TreeMap.TreeMapChildSelected.Subscribe(UC_TreeMap_TreeMapChildSelected));
        }

        public void Dispose()
        {
            foreach (var d in _lsDisposable)
                d.Dispose();
        }

        void UC_TreeMap_TreeMapChildSelected(LocalTreeNode treeNodeChild)
        {
            UtilDirList.Write("L");
            if (_treeNode != treeNodeChild.Parent)
                return;

            ItemsCast
                .Where(lvItem => lvItem.LocalTreeNode == _treeNode)
                .FirstOnlyAssert(SelectedItem_Set);

            _lvChildrenVM.ItemsCast
                .Where(lvItem => lvItem.LocalTreeNode == treeNodeChild)
                .FirstOnlyAssert(lvItem => _lvChildrenVM.SelectedItem_Set(lvItem));
        }

        void Populate(LocalTreeNode treeNodeSel)
        {
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
        List<IDisposable>
            _lsDisposable = new List<IDisposable>();
    }
}
