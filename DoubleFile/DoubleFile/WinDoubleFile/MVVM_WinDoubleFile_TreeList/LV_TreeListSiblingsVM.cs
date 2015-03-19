using System;
using System.Collections.Generic;

namespace DoubleFile
{
    class LV_TreeListSiblingsVM : ListViewVM_GenericBase<LVitem_TreeListVM>, IDisposable
    {
        public string Title
        {
            get { return (_Title ?? "").Replace("_", "__"); }
            internal set
            {
                _Title = value;
                RaisePropertyChanged("Title");
            }
        }
        string _Title = null;

        public string WidthFolder { get { return SCW; } }                   // franken all NaN

        internal override int NumCols { get { return LVitem_TreeListVM.NumCols_; } }

        internal LV_TreeListSiblingsVM(LV_TreeListChildrenVM lvChildrenVM)
        {
            _lvChildrenVM = lvChildrenVM;
            Local.TreeSelect.FolderDetailUpdated += TreeSelect_FolderDetailUpdated;
        }

        public void Dispose()
        {
            Local.TreeSelect.FolderDetailUpdated -= TreeSelect_FolderDetailUpdated;
        }

        internal void Select(LocalTreeNode treeNodeSel)
        {
            _lvChildrenVM.Populate(treeNodeSel);
        }

        void TreeSelect_FolderDetailUpdated(IEnumerable<string[]> lasDetail, LocalTreeNode treeNodeSel)
        {
            Items.Clear();

            var treeNodes =
                (null != treeNodeSel.Parent)
                ? treeNodeSel.Parent.Nodes
                : treeNodeSel.TreeView.Nodes;

            foreach (var treeNode in treeNodes)
                Add(new LVitem_TreeListVM(new[] { treeNode.Name }));

            Select(treeNodeSel);
        }

        LV_TreeListChildrenVM _lvChildrenVM = null;
    }
}
