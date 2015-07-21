using System.Windows;
using System.Windows.Media;

namespace DoubleFile
{
    class LVitem_FormsLVVM : ListViewItemVM_Base
    {
        public string
            Folder { get { return _localLVitemVM.Folder; } }
        public string
            Clones { get { return _localLVitemVM.Clones; } }

        public Brush Foreground { get { return _localLVitemVM.Foreground; } }
        public Brush Background { get { return _localLVitemVM.Background; } }

        public FontWeight
            FontWeight { get { return _localLVitemVM.FontWeight; } }

        internal LocalTreeNode
            LocalTreeNode { get { return _localLVitemVM.LocalTreeNode; } }

        protected override string[] _propNames { get { return _propNamesA; } set { _propNamesA = value; } }
        static string[] _propNamesA = null;

        internal override int NumCols { get { return NumCols_; } }
        internal const int NumCols_ = 0;

        internal LVitem_FormsLVVM(WinFormsLVVM lvvm, LocalLVitemVM localLVitemVM)
            : base(lvvm, null)
        {
            _localLVitemVM = localLVitemVM;
        }

        LocalLVitemVM
            _localLVitemVM = null;
    }
}
