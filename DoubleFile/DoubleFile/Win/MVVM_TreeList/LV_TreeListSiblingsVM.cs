using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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

            var folderDetail = LocalTV.TreeSelect_FolderDetail;

            if (null != folderDetail)
                TreeSelect_FolderDetailUpdated(Tuple.Create(folderDetail, 0));
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
            var bSiblingFolder = (UC_TreeMap.kSelRectAndTooltip != initiatorTuple.Item2);

            Util.Write("L");

            if (bSiblingFolder &&
                Populate(tuple.Item2))
            {
                return;
            }

            {
                var siblingFolder =
                    bSiblingFolder
                    ? tuple.Item2
                    : tuple.Item2.Parent;

                ItemsCast
                    .Where(lvItem => lvItem.LocalTreeNode == siblingFolder)
                    .FirstOnlyAssert(SelectedItem_Set);
            }

            if (bSiblingFolder)
                return;

            if ((null != _selectedItem) &&
                (tuple.Item2.Parent != _selectedItem.LocalTreeNode))    // no-op on descending treemap subfolders.
            {
                return;
            }

            _lvChildrenVM.ItemsCast
                .Where(lvItem => lvItem.LocalTreeNode == tuple.Item2)
                .FirstOnlyAssert(_lvChildrenVM.SelectedItem_Set);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="treeNodeSel"></param>
        /// <returns>true if it tried to select a folder</returns>
        bool Populate(LocalTreeNode treeNodeSel)
        {
            var parentNode =
                (null != treeNodeSel)
                ? treeNodeSel.Parent
                : null;

            if ((parentNode == _treeNode) &&
                (0 < Items.Count))
            {
                return false;
            }

            _treeNode = parentNode;
            ClearItems();
            Util.Write("K");

            var treeNodes =
                (null != _treeNode)
                ? _treeNode.Nodes
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

            bool bCompleted = false;

            Util.UIthread(() =>
            {
                Add(lsLVitems);
                bCompleted = true;
            });

            while (false == bCompleted)
                Util.Block(20);

            SelectedItem_Set(selectedItem);
            return true;
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
