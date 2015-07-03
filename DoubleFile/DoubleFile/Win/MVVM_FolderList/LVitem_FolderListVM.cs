﻿using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DoubleFile
{
    class LVitem_FolderListVM : ListViewItemVM_Base
    {
        internal LocalTreeNode LocalTreeNode { get { return _datum as LocalTreeNode; } set { _datum = value; } }
        object _datum = null;

        public string Folder
        {
            get
            {
                return LocalTreeNode.Text +
                    ((null != LocalTreeNode.Parent)
                    ? " in " + LocalTreeNode.Parent.FullPath
                    : "");
            }
        }

        internal override int NumCols { get { return NumCols_; } }
        internal const int NumCols_ = 1;

        protected override string[] _propNames { get { return _propNamesA; } set { _propNamesA = value; } }
        static string[] _propNamesA = null;

        internal LVitem_FolderListVM(IList<string> lsStr = null)
            : base(null, lsStr)
        {
            marr = null;
        }

        internal LVitem_FolderListVM(LVitem_FolderListVM lvItemTemp)
            : this(lvItemTemp.StringValues)
        {
        }
    }
}
