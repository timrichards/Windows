using System;
using System.Windows.Input;

namespace DoubleFile
{
    partial class WinFileHash_FoldersVM : ObservableObject_OwnerWindow, IDisposable
    {
        public ICommand Icmd_ShowFiles { get; private set; }

        internal WinFileHash_FoldersVM(GlobalData_Base gd, TreeView_FileHashVM tvVM, LV_ProjectVM lvProjectVM)
        {
            _lvProjectVM = lvProjectVM;
            Icmd_ShowFiles = new RelayCommand(param => ShowWindows());

            if ((null == _lvProjectVM) ||
                (0 == _lvProjectVM.Count))
            {
                return;
            }

            _nCorrelateProgressDenominator = _lvProjectVM.Count;
            _gd = gd;
            _tvVM = tvVM;
            _winProgress = new WinProgress(); 
            UString.AddRef();
            DoTree();
        }

        internal bool ShowWindows()     // returns true if it created a window
        {
            bool bRet = false;

            if ((null == _winFileHash_Files) ||
                _winFileHash_Files.LocalIsClosed)
            {
                (_winFileHash_Files = new WinFileHash_Files(_gd)).Show();
                bRet = true;
            }

            return _winFileHash_Files.ShowWindows() || bRet;
        }

        public void Dispose()
        {
            if (null != _winFileHash_Files)
                _winFileHash_Files.Close();

            if ((null == _lvProjectVM) ||
                (0 == _lvProjectVM.Count))
            {
                return;
            }

            UString.DropRef();
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
