using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace DoubleFile
{
    class LVitem_DoubleFile_SearchVM : ListViewItemVM_Base
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

        protected override string[] PropertyNames { get { return new[] { "Results" }; } }

        internal override int NumCols { get { return NumCols_; } }
        internal const int NumCols_ = 1;

        internal LVitem_DoubleFile_SearchVM(IEnumerable<string> ieString = null)
            : base(null, ieString)
        {
            marr = null;
        }

        internal LVitem_DoubleFile_SearchVM(LVitem_DoubleFile_SearchVM lvItemTemp)
            : this(lvItemTemp.StringValues)
        {
        }
    }
}
