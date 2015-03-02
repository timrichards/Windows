using System;

namespace DoubleFile
{
    partial class WinFileHashVM : ObservableObject_OwnerWindow, IDisposable
    {
        internal WinFileHashVM(GlobalData_Base gd, TreeView_FileHashVM tvVM, LV_ProjectVM lvProjectVM)
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
            DoTree();
        }

        public void Dispose()
        {
            if (null != _winFileHash_Files)
                _winFileHash_Files.Close();
        }

        readonly GlobalData_Base
            _gd = null;
        readonly TreeView_FileHashVM
            _tvVM = null;
        readonly LV_ProjectVM
            _lvProjectVM = null;
        readonly WinProgress
            _winProgress = null;
        readonly double
            _nCorrelateProgressDenominator = 0;
        WinFileHash_Files
            _winFileHash_Files = null;
    }
}
