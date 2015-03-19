
namespace DoubleFile
{
    class LV_TreeListChildrenVM : ListViewVM_GenericBase<LVitem_TreeListVM>
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

        internal void Populate(LocalTreeNode treeNodeParent)
        {
            Items.Clear();

            foreach (var treeNode in treeNodeParent.Nodes)
                Add(new LVitem_TreeListVM(new[] { treeNode.Name }));
        }
    }
}
