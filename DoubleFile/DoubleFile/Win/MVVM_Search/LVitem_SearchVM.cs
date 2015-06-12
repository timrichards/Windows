﻿using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DoubleFile
{
    class LVitem_SearchVM : ListViewItemVM_Base
    {
        internal PathBuilder Directory { get { return _datum as PathBuilder; } set { _datum = value; } }
        internal LocalTreeNode LocalTreeNode { get { return _datum as LocalTreeNode; } set { _datum = value; } }
        object _datum = null;

        internal TabledString<Tabled_Files> Filename { get; set; }

        public string Results
        {
            get
            {
                if (null != Directory)
                {
                    var strFile = 
                        (false == string.IsNullOrEmpty(Filename))
                        ? Filename + " in "
                        : "";

                    return strFile + Directory;
                }
                else
                {
                    return LocalTreeNode.Text +
                        ((null != LocalTreeNode.Parent)
                        ? " in " + LocalTreeNode.Parent.FullPath
                        : "");
                }
            }
        }

        internal override int NumCols { get { return NumCols_; } }
        internal const int NumCols_ = 1;

        protected override IEnumerable<string> PropNames { get { return _propNames; } }
        static IEnumerable<string> _propNames = GetProps(typeof(LVitem_SearchVM));

        internal LVitem_SearchVM(IList<string> lsStr = null)
            : base(null, lsStr)
        {
            marr = null;
        }

        internal LVitem_SearchVM(LVitem_SearchVM lvItemTemp)
            : this(lvItemTemp.StringValues)
        {
        }
    }
}
