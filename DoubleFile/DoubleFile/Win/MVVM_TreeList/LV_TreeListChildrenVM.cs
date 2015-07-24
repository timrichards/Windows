using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace DoubleFile
{
    class LV_TreeListChildrenVM : SliderVM_Base<LVitem_TreeListVM>
    {
        public ICommand Icmd_GoTo { get; }
        internal override void GoTo(LocalTreeNode treeNode) { treeNode.GoToFile(null); }

        static internal IObservable<Tuple<LocalTreeNode, int>>
            TreeListChildSelected { get { return _treeListChildSelected; } }
        static readonly LocalSubject<LocalTreeNode> _treeListChildSelected = new LocalSubject<LocalTreeNode>();
        static void TreeListChildSelectedOnNext(LocalTreeNode value) { _treeListChildSelected.LocalOnNext(value, kChildSelectedOnNext); }
        internal const int kChildSelectedOnNext = 99854;

        internal LV_TreeListChildrenVM()
        {
            Icmd_GoTo = new RelayCommand(() => _selectedItem.LocalTreeNode.GoToFile(null), () => null != _selectedItem);
        }

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

                TreeListChildSelectedOnNext(value.LocalTreeNode);
                TreeSelect.DoThreadFactory(value.LocalTreeNode, nInitiator: kChildSelectedOnNext);
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
            DeepNode = _selectedItem.LocalTreeNode;
        }
        LVitem_TreeListVM _selectedItem = null;

        public string WidthFolder { get { return SCW; } }                   // franken all NaN

        internal override int NumCols { get { return LVitem_TreeListVM.NumCols_; } }

        internal void Populate(LocalTreeNode treeNodeParent)
        {
            ClearItems();

            if (null == treeNodeParent.Nodes)
                return;

            var lsLVitems = new List<LVitem_TreeListVM>();

            foreach (var treeNode in treeNodeParent.Nodes)
                lsLVitems.Add(new LVitem_TreeListVM(new[] { treeNode.Name }) { LocalTreeNode = treeNode });

            SelectedItem_Set(null);
            Util.UIthread(99815, () => Add(lsLVitems));
        }
    }
}
