
namespace DoubleFile
{
    partial class WinFileHashVM : ObservableObject_OwnerWindow
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

        readonly GlobalData_Base _gd = null;
        readonly TreeView_FileHashVM _tvVM = null;
        readonly LV_ProjectVM _lvProjectVM = null;
        readonly WinProgress _winProgress = null;
        readonly double _nCorrelateProgressDenominator = 0;
    }
}
