﻿using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DoubleFile
{
    class LVitem_SearchVM : ListViewItemVM_Base
    {
        internal PathBuilder Directory { get { return As<PathBuilder>(); } set { _datum = value; } }
        internal LocalTreeNode LocalTreeNode { get { return As<LocalTreeNode>(); } set { _datum = value; } }
        object _datum = null;
        T As<T>() where T: class { return  (_datum is T) ? (T)_datum : null; }  // 20x faster than as p. 123

        internal TabledString<Tabled_Files> Filename { get; set; }

        public string Folder
        {
            get
            {
                if (null != Directory)
                {
                    if (false == string.IsNullOrWhiteSpace(Filename))
                    {
                        return Filename;
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

                    if (false == string.IsNullOrWhiteSpace(Filename))
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
                    var p = LocalTreeNode.Parent; return (null != p) ? p.FullPath : null;
                }
            }
        }

        internal override int NumCols { get { return NumCols_; } }
        internal const int NumCols_ = 2;

        protected override string[] _propNames { get { return _propNamesA; } set { _propNamesA = value; } }
        static string[] _propNamesA = null;

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
