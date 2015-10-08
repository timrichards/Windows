namespace DoubleFile
{
    class LVitem_CompareVM : LVitem_FilesVM
    {
        internal override bool ShowDuplicates => false;
        internal LocalTreeNode TreeNode = null;
    }
}
