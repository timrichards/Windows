using System.Collections.Generic;

namespace DoubleFile
{
    class LVitem_SearchVM : ListViewItemVM_Base
    {
        internal PathBuilder
            Directory { get { return _datum.As<PathBuilder>(); } set { _datum = value; } }
        internal TabledString<TabledStringType_Files>
            TabledStringFilename;

        internal LocalTreeNode
            LocalTreeNode { get { return _datum.As<LocalTreeNode>(); } set { _datum = value; } }
        object _datum = null;

        public string
            In { get; private set; } = " in ";    // interned

        public int
            Alternate { get; internal set; } = 0;

        static internal bool
            UseNickname;

        public string
            FolderOrFile
        {
            get
            {
                string strRet = null;

                if (null != LocalTreeNode)
                {
                    var parent = LocalTreeNode.Parent;

                    if (null != parent)
                        return LocalTreeNode.Text;

                    strRet = LocalTreeNode.FullPathGet(UseNickname);
                }
                else
                {
                    if (null != TabledStringFilename)
                        return "" + TabledStringFilename;

                    strRet = "" + Directory;
                }

                In = null;
                RaisePropertyChanged("In");
                RaisePropertyChanged("Parent");
                return strRet;
            }
        }

        public string
            Parent
        {
            get
            {
                if (null == In)
                    return null;

                if (null != LocalTreeNode)
                    return LocalTreeNode.Parent?.FullPathGet(UseNickname);

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
