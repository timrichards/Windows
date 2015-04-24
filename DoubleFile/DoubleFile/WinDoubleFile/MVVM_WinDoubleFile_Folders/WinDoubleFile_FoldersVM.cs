using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace DoubleFile
{
    partial class WinDoubleFile_FoldersVM : ObservableObject_OwnerWindow, IDisposable
    {
        static internal IEnumerable<LocalTreeNode>
            TreeNodes { get { return _weakReference.IsAlive ? ((WinDoubleFile_FoldersVM)_weakReference.Target)._arrTreeNodes : null; } }
        static internal LocalTV
            LocalTV { get { return _weakReference.IsAlive ? ((WinDoubleFile_FoldersVM)_weakReference.Target)._localTV : null; } }

        internal WinDoubleFile_FoldersVM(TreeView_DoubleFileVM tvVM, LV_ProjectVM lvProjectVM)
        {
            _weakReference = new WeakReference(this);            
            _lvProjectVM = lvProjectVM;

            if ((null == _lvProjectVM) ||
                (0 == _lvProjectVM.Count))
            {
                return;
            }

            _nCorrelateProgressDenominator = _lvProjectVM.Count;
            _tvVM = tvVM;
            TabledString<Tabled_Folders>.AddRef();
            DoTree();
        }

        public void Dispose()
        {
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
        static WeakReference
            _weakReference = null;
    }
}
