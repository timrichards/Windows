using System.Collections.Generic;

namespace DoubleFile
{
    class LVitem_FilesVM_Compare : LVitem_FilesVM_Base
    {
        internal LocalTreeNode TreeNode = null;

        protected override IReadOnlyList<string> _propNames { get { return _propNamesA; } set { _propNamesA = value; } }
        static IReadOnlyList<string> _propNamesA = null;
    }
}
