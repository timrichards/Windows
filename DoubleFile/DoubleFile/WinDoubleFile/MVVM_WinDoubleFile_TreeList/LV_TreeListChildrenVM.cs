using System.Collections.Generic;

namespace DoubleFile
{
    class LV_TreeListChildrenVM : ListViewVM_GenericBase<LVitem_TreeListVM>
    {
        public string WidthFolder { get { return SCW; } }                   // franken all NaN

        internal override int NumCols { get { return LVitem_TreeListVM.NumCols_; } }

        internal void Populate(LocalTreeNode treeNodeParent)
        {
            var lsLVitems = new List<LVitem_TreeListVM>();

            foreach (var treeNode in treeNodeParent.Nodes)
                lsLVitems.Add(new LVitem_TreeListVM(new[] { treeNode.Name }));

            UtilProject.UIthread(() =>
            {
                Items.Clear();
                Add(lsLVitems);
            });
        }
    }
}
