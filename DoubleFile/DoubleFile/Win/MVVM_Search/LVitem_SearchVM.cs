using System.Collections.Generic;

namespace DoubleFile
{
    class LVitem_SearchVM : ListViewItemVM_Base
    {
        internal PathBuilder Directory { get { return _datum.As<PathBuilder>(); } set { _datum = value; } }
        internal TabledString<TabledStringType_Files> TabledStringFilename;

        internal LocalTreeNode LocalTreeNode { get { return _datum.As<LocalTreeNode>(); } set { _datum = value; } }
        object _datum = null;

        public string In { get; private set; } = " in ";    // interned

        public int Alternate { get; internal set; } = 0;

        public string FolderOrFile
        {
            get
            {
                if (null != LocalTreeNode)
                    return LocalTreeNode.Text;

                if (null != TabledStringFilename)
                    return "" + TabledStringFilename;

                In = null;
                RaisePropertyChanged("In");
                RaisePropertyChanged("Parent");
                return "" + Directory;
            }
        }

        public string Parent
        {
            get
            {
                if (null == In)
                    return null;

                if (null != LocalTreeNode)
                    return LocalTreeNode.Parent?.FullPath;

                var strDirectory = "" + Directory;
                var nIx = strDirectory.LastIndexOf('\\');

                return
                    ((null != TabledStringFilename) || (nIx < 0))
                    ? strDirectory
                    : strDirectory.Substring(0, nIx);
            }
        }

        internal override int NumCols => NumCols_;
        internal const int NumCols_ = 0;

        protected override IReadOnlyList<string> _propNames { get { return _propNamesA; } set { _propNamesA = value; } }
        static IReadOnlyList<string> _propNamesA = null;
    }
}
