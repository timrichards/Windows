using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DoubleFile
{
    class LVitem_FolderListVM : ListViewItemVM_Base
    {
        internal LocalTreeNode LocalTreeNode { get; set; }

        public bool Alternate { get; internal set; }

        public string Folder { get { return LocalTreeNode.Text; } }
        public string Parent { get { var p = LocalTreeNode.Parent; return (null != p) ? p.FullPath : null; } }

        internal override int NumCols { get { return NumCols_; } }
        internal const int NumCols_ = 2;

        protected override string[] _propNames { get { return _propNamesA; } set { _propNamesA = value; } }
        static string[] _propNamesA = null;
    }
}
