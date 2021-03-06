﻿using System;
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

                if (null == _selectedItem)
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

        public string WidthFolder => SCW;                  // franken all NaN

        internal override int NumCols => LVitem_TreeListVM.NumCols_;

        internal LV_TreeListSiblingsVM(LV_TreeListChildrenVM lvChildrenVM)
        {
            _lvChildrenVM = lvChildrenVM;
            _lsDisposable.Add(TreeSelect.FolderDetailUpdated.Observable.LocalSubscribe(99699, TreeSelect_FolderDetailUpdated));

            var folderDetail = LocalTV.TreeSelect_FolderDetail;

            if (null != folderDetail)
                TreeSelect_FolderDetailUpdated(Tuple.Create(folderDetail, /* UI initiator */ 0m));
        }

        public void Dispose() => Util.LocalDispose(_lsDisposable);

        void TreeSelect_FolderDetailUpdated(Tuple<TreeSelect.FolderDetailUpdated, decimal> initiatorTuple)
        {
            if (new[] { _kTreeSelect, LV_TreeListChildrenVM.kChildSelectedOnNext }
                .Contains(initiatorTuple.Item2))
            {
                return;
            }

            var folderDetail = initiatorTuple.Item1;
            var bSiblingFolder = (UC_TreemapVM.kSelRectAndTooltip != initiatorTuple.Item2);

            Util.Write("L");

            if (bSiblingFolder &&
                Populate(folderDetail.treeNode))
            {
                return;
            }

            {
                var siblingFolder =
                    bSiblingFolder
                    ? folderDetail.treeNode
                    : folderDetail.treeNode.Parent;

                ItemsCast
                    .Where(lvItem => ReferenceEquals(lvItem.LocalTreeNode, siblingFolder))
                    .FirstOnlyAssert(SelectedItem_Set);
            }

            if (bSiblingFolder)
                return;

            // no-op on descending treemap subfolders.
            if (false == ReferenceEquals(folderDetail.treeNode.Parent, _selectedItem?.LocalTreeNode))
                return;

            _lvChildrenVM.ItemsCast
                .Where(lvItem => lvItem.LocalTreeNode == folderDetail.treeNode)
                .FirstOnlyAssert(_lvChildrenVM.SelectedItem_Set);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="treeNodeSel"></param>
        /// <returns>true if it tried to select a folder</returns>
        bool Populate(LocalTreeNode treeNodeSel)
        {
            var parentNode = treeNodeSel?.Parent;

            if ((parentNode == _treeNode) &&
                (0 < Items.Count))
            {
                return false;
            }

            _treeNode = parentNode;
            ClearItems();
            Util.Write("K");

            var treeNodes = _treeNode?.Nodes ?? LocalTV.RootNodes;
            var lsLVitems = new List<LVitem_TreeListVM> { };
            LVitem_TreeListVM selectedItem = null;

            foreach (var treeNode in treeNodes)
            {
                var lvItem = new LVitem_TreeListVM(new[] { treeNode.PathShort }) { LocalTreeNode = treeNode };

                lsLVitems.Add(lvItem);

                if ((null == selectedItem) &&
                    ReferenceEquals(treeNode, treeNodeSel))
                {
                    selectedItem = lvItem;
                }
            }

            Util.UIthread(99811, () => Add(lsLVitems));
            SelectedItem_Set(selectedItem);
            return true;
        }

        const int
            _kTreeSelect = 99984;
        LocalTreeNode
            _treeNode = null;
        readonly LV_TreeListChildrenVM
            _lvChildrenVM = null;
        readonly IList<IDisposable>
            _lsDisposable = new List<IDisposable> { };
    }
}
