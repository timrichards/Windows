using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace DoubleFile
{
    partial class WinDoubleFile_FoldersVM : ObservableObject_OwnerWindow, IDisposable
    {
        internal static Func<IEnumerable<LocalTreeNode>> GetTreeNodes = null;

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
            TabledString<Tabled_Folders>.AddRef();
            DoTree();
            GetTreeNodes = () => _arrTreeNodes;
        }

        public void Dispose()
        {
            GetTreeNodes = null;
            LocalTV.StaticTreeView.Dispose();
            LocalTV.StaticTreeView = null;

            if ((null == _lvProjectVM) ||
                (0 == _lvProjectVM.Count))
            {
                return;
            }

            TabledString<Tabled_Folders>.DropRef();
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
