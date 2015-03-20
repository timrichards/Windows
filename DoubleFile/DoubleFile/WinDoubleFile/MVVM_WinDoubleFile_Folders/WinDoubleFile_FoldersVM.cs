using System;
using System.Windows.Input;

namespace DoubleFile
{
    partial class WinDoubleFile_FoldersVM : ObservableObject_OwnerWindow, IDisposable
    {
        internal WinDoubleFile_FoldersVM(GlobalData_Base gd, TreeView_DoubleFileVM tvVM, LV_ProjectVM lvProjectVM)
        {
            _lvProjectVM = lvProjectVM;

            if ((null == _lvProjectVM) ||
                (0 == _lvProjectVM.Count))
            {
                return;
            }

            _nCorrelateProgressDenominator = _lvProjectVM.Count;
            _gd = gd;
            _tvVM = tvVM;
            _winProgress = new WinProgress(); 
            TabledString.AddRef();
            DoTree();
        }

        public void Dispose()
        {
            if ((null == _lvProjectVM) ||
                (0 == _lvProjectVM.Count))
            {
                return;
            }

            TabledString.DropRef();
        }

        readonly GlobalData_Base
            _gd = null;
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
