namespace DoubleFile
{
    class LVitem_SearchVM : ListViewItemVM_Base
    {
        internal PathBuilder Directory { get { return _datum.As<PathBuilder>(); } set { _datum = value; } }
        internal LocalTreeNode LocalTreeNode { get { return _datum.As<LocalTreeNode>(); } set { _datum = value; } }
        object _datum = null;

        public string FolderOrFile
        {
            get
            {
                if (null != Directory)
                {
                    if (null != Filename)
                    {
                        return (string)Filename;
                    }
                    else
                    {
                        var strDirectory = "" + Directory;
                        var nIx = strDirectory.LastIndexOf('\\');

                        return (nIx < 0)
                            ? strDirectory
                            : strDirectory.Substring(nIx + 1);
                    }
                }
                else
                {
                    return LocalTreeNode.Text;
                }
            }
        }

        public string Parent
        {
            get
            {
                if (null != Directory)
                {
                    var strDirectory = "" + Directory;
                    var nIx = strDirectory.LastIndexOf('\\');

                    if (null != Filename)
                    {
                        return strDirectory;
                    }
                    else
                    {
                        return (nIx < 0)
                            ? strDirectory
                            : strDirectory.Substring(0, nIx);
                    }
                }
                else
                {
                    return LocalTreeNode.Parent?.FullPath;
                }
            }
        }

        internal override int NumCols => NumCols_;
        internal const int NumCols_ = 0;

        protected override string[] _propNames { get { return _propNamesA; } set { _propNamesA = value; } }
        static string[] _propNamesA = null;

        internal TabledString<TabledStringType_Files> Filename;
    }
}
