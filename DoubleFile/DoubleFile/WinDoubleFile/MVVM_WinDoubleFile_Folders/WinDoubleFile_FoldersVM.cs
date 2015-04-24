using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace DoubleFile
{
    partial class WinDoubleFile_FoldersVM : ObservableObject_OwnerWindow, IDisposable
    {
        static internal Func<IEnumerable<LocalTreeNode>>
            GetTreeNodes = null;
        static internal Func<LocalTV>
            GetLocalTV = null;

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
            TabledString<Tabled_Folders>.AddRef();
            GetLocalTV = () => _localTV;
            DoTree();
            GetTreeNodes = () => _arrTreeNodes;
        }

        public void Dispose()
        {
            GetTreeNodes = null;
            GetLocalTV = null;

            if (null != _localTV)
                _localTV.Dispose();

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
            _winProgress = new WinProgress();
        readonly double
            _nCorrelateProgressDenominator = 0;
        readonly LocalTV
            _localTV = new LocalTV();
    }
}
