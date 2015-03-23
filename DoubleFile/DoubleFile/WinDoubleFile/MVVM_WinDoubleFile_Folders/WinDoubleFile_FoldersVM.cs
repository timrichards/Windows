using System;
using System.Windows.Input;

namespace DoubleFile
{
    partial class WinDoubleFile_FoldersVM : ObservableObject_OwnerWindow, IDisposable
    {
        internal static Func<KeyList<LocalTreeNode>> GetTreeNodes = null;

        internal WinDoubleFile_FoldersVM(TreeView_DoubleFileVM tvVM, LV_ProjectVM lvProjectVM)
        {
            _lvProjectVM = lvProjectVM;

            if ((null == _lvProjectVM) ||
                (0 == _lvProjectVM.Count))
            {
                return;
            }

            _nCorrelateProgressDenominator = _lvProjectVM.Count;
            _tvVM = tvVM;
            _winProgress = new WinProgress(); 
            TabledString.AddRef();
            DoTree();
            GetTreeNodes = () => _listTreeNodes;
        }

        public void Dispose()
        {
            if ((null == _lvProjectVM) ||
                (0 == _lvProjectVM.Count))
            {
                return;
            }

            _localTV.Dispose();
            GetTreeNodes = null;
            TabledString.DropRef();
        }

        readonly TreeView_DoubleFileVM
            _tvVM = null;
        readonly LV_ProjectVM
            _lvProjectVM = null;
        readonly WinProgress
            _winProgress = null;
        readonly double
            _nCorrelateProgressDenominator = 0;
    }
}
