using System.Collections.Generic;
using System.Text;

namespace DoubleFile
{
    class LVitem_DoubleFile_SearchVM : ListViewItemVM_Base
    {
        internal SearchResultsDir SearchResultsDir { get; set; }
        internal int FileIndex { get; set; }
        internal LocalTreeNode LocalTreeNode = null;

        public string Results
        {
            get
            {
                if (null != SearchResultsDir)
                {
                    var strRet =
                        (FileIndex >= 0)
                        ? SearchResultsDir.ListFiles[FileIndex] + " in "
                        : "";

                    return strRet + SearchResultsDir.StrDir;
                }
                else
                {
                    return LocalTreeNode.Text + " in " + GetParentPath();
                }
            }
        }

        string GetParentPath()
        {
            if (null == LocalTreeNode.Parent)
                return null;

            var sbPath = new StringBuilder();
            var treeNodeParent = LocalTreeNode.Parent;

            do
            {
                sbPath.Insert(0, treeNodeParent.Text + '\\');
            } while (null != (treeNodeParent = treeNodeParent.Parent));

            return sbPath.ToString().TrimEnd('\\');
        }

        protected override string[] PropertyNames { get { return new[] { "Results" }; } }

        internal override int NumCols { get { return NumCols_; } }
        internal const int NumCols_ = 1;

        internal LVitem_DoubleFile_SearchVM(IEnumerable<string> ieString = null)
            : base(null, ieString)
        {
        }

        internal LVitem_DoubleFile_SearchVM(LVitem_DoubleFile_SearchVM lvItemTemp)
            : this(lvItemTemp.StringValues)
        {
        }
    }
}
