using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace DoubleFile
{
    class LV_TreeListChildrenVM : ListViewVM_Base<LVitem_TreeListVM>
    {
        static internal IObservable<Tuple<LocalTreeNode, int>>
            TreeListChildSelected { get { return _treeListChildSelected.AsObservable(); } }
        static readonly LocalSubject<LocalTreeNode> _treeListChildSelected = new LocalSubject<LocalTreeNode>();
        static void TreeListChildSelectedOnNext(LocalTreeNode value) { _treeListChildSelected.LocalOnNext(value, 99854, -1); }
        internal bool
            SkipOne_TreeMapChildSelected_ResetsIt { get { var b = _skipOne_TreeMapChildSelected_ResetsIt; _skipOne_TreeMapChildSelected_ResetsIt = false; return b; } set { _skipOne_TreeMapChildSelected_ResetsIt = value; } }
        bool _skipOne_TreeMapChildSelected_ResetsIt = false;

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

                MBoxStatic.Assert(99837, false == _skipOne_TreeMapChildSelected_ResetsIt);
                SkipOne_TreeMapChildSelected_ResetsIt = true;
                TreeListChildSelectedOnNext(value.LocalTreeNode);
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
        }
        LVitem_TreeListVM _selectedItem = null;

        public string WidthFolder { get { return SCW; } }                   // franken all NaN

        internal override int NumCols { get { return LVitem_TreeListVM.NumCols_; } }

        internal void Populate(LocalTreeNode treeNodeParent)
        {
            UtilProject.UIthread(ClearItems);

            if (null == treeNodeParent.Nodes)
                return;

            var lsLVitems = new List<LVitem_TreeListVM>();

            foreach (var treeNode in treeNodeParent.Nodes)
                lsLVitems.Add(new LVitem_TreeListVM(new[] { treeNode.Name }) { LocalTreeNode = treeNode });

            SelectedItem_Set(null);
            UtilProject.UIthread(() => Add(lsLVitems));
        }
    }
}
