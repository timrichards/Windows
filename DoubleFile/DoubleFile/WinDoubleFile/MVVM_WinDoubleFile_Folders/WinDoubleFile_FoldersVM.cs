using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace DoubleFile
{
    partial class LocalTV : IDisposable
    {
        static internal IEnumerable<LocalTreeNode>
            TreeNodes { get { var o = _weakReference.Target as LocalTV; return (null != o) ? o._arrTreeNodes : null; } }

        static internal LocalTV Instance { get; private set; }

        static internal void FactoryCreate(TreeView_DoubleFileVM tvVM, LV_ProjectVM lvProjectVM)
        {
            if (null != Instance)
            {
                MBoxStatic.Assert(99858, false);
                return;
            }

            _weakReference.Target =
                Instance =
                new LocalTV(tvVM, lvProjectVM);

            Instance.DoTree();
        }

        LocalTV(TreeView_DoubleFileVM tvVM, LV_ProjectVM lvProjectVM)
        {
            _lvProjectVM = lvProjectVM;

            if ((null != _lvProjectVM) &&
                (0 < _lvProjectVM.Count))
            {
                _nCorrelateProgressDenominator = _lvProjectVM.Count;
                _tvVM = tvVM;
                TabledString<Tabled_Folders>.AddRef();
            }

            _weakReference.Target = this;            
            _lsDisposable.Add(WinDoubleFile_DuplicatesVM.GoToFile.Subscribe(GoToFileA));
            _lsDisposable.Add(WinDoubleFile_SearchVM.GoToFile.Subscribe(GoToFileB));
        }

        static internal void LocalDispose()
        {
            if (null == Instance)
            {
                MBoxStatic.Assert(99857, false);
                return;
            }

            Instance.Dispose();
            Instance = null;
        }

        public void Dispose()
        {
            _weakReference.Target = null;

            if ((null != Instance._lvProjectVM) &&
                (0 < Instance._lvProjectVM.Count))
            {
                TabledString<Tabled_Folders>.DropRef();
            }

            foreach (var d in _lsDisposable)
                d.Dispose();
        }

        readonly TreeView_DoubleFileVM
            _tvVM = null;
        readonly LV_ProjectVM
            _lvProjectVM = null;
        readonly WinProgress
            _winProgress = new WinProgress();
        readonly double
            _nCorrelateProgressDenominator = 0;
    }
}
