﻿using System;

namespace DoubleFile
{
    partial class WinFileHash_FoldersVM : ObservableObject_OwnerWindow, IDisposable
    {
        internal WinFileHash_FoldersVM(GlobalData_Base gd, TreeView_FileHashVM tvVM, LV_ProjectVM lvProjectVM)
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
            UString.AddRef();
            DoTree();
        }

        internal void ShowFilesBrowser()
        {
            if ((null == _winFileHash_Files) ||
                _winFileHash_Files.IsClosed)
            {
                (_winFileHash_Files = new WinFileHash_Files(_gd)).Show();
            }

            _winFileHash_Files.ShowFilesBrowser();
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
