using System;
using System.Windows.Input;

namespace DoubleFile
{
    partial class WinDoubleFile_FoldersVM : ObservableObject_OwnerWindow, IDisposable
    {
        public ICommand Icmd_ShowFiles { get; private set; }

        internal WinDoubleFile_FoldersVM(GlobalData_Base gd, TreeView_DoubleFileVM tvVM, LV_ProjectVM lvProjectVM)
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
            TabledString.AddRef();
            DoTree();
        }

        internal bool ShowWindows()     // returns true if it created a window
        {
            bool bRet = false;

            if ((null == _winDoubleFile_Files) ||
                _winDoubleFile_Files.LocalIsClosed)
            {
                (_winDoubleFile_Files = new WinDoubleFile_Files(_gd)).Show();
                bRet = true;
            }

            return _winDoubleFile_Files.ShowWindows() || bRet;
        }

        public void Dispose()
        {
            if (null != _winDoubleFile_Files)
                _winDoubleFile_Files.Close();

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
        WinDoubleFile_Files
            _winDoubleFile_Files = null;
    }
}
