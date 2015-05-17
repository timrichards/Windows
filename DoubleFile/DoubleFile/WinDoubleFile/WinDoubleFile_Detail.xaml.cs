﻿using System;
using System.Reactive.Linq;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinDoubleFile_Detail.xaml
    /// </summary>
    partial class WinDoubleFile_Detail
    {
        protected override void LocalNavigatedTo()
        {
            LocalDispose_WindowClosed();

            form_localPath.DataContext =
                formLV_File.DataContext =
                _lvFileDetailVM =
                new LV_FileDetailVM();         
 
            formLV_Folder.DataContext =
                _lvFolderDetailVM =
                new LV_FolderDetailVM();

            formLV_Volume.DataContext =
                _lvVolumeDetailVM =
                new LV_VolumeDetailVM();

            foreach (var ctl in new[] { formLV_File, formLV_Folder, formLV_Volume })
            {
                Observable.FromEventPattern(ctl, "SelectionChanged")
                    .Subscribe(args => ctl.UnselectAllCells());
            }
        }

        public WinDoubleFile_Detail()
        {
            InitializeComponent();
        }

        protected override void CopyTag_NewWindow(WeakReference wr)
        {
            LocalNavigatedTo();
        }

        protected override void LocalDispose_WindowClosed()
        {
            if (null != _lvFileDetailVM)
                _lvFileDetailVM.Dispose();

            if (null != _lvFolderDetailVM)
                _lvFolderDetailVM.Dispose();

            if (null != _lvVolumeDetailVM)
                _lvVolumeDetailVM.Dispose();
        }

        LV_FileDetailVM
            _lvFileDetailVM = null;
        LV_FolderDetailVM
            _lvFolderDetailVM = null;
        LV_VolumeDetailVM
            _lvVolumeDetailVM = null;
    }
}
