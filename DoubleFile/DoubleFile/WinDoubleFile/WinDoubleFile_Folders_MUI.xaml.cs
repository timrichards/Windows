﻿using System;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinDoubleFile_Folders.xaml
    /// </summary>
    partial class WinDoubleFile_Folders_MUI
    {
        protected override void LocalNavigatedTo()
        {
            if (false == WinProject_MUI.InitExplorer())
            {
                if ((null == App.LVprojectVM) ||
                    (App.LVprojectVM == _lvProjectVM) ||
                    (App.LVprojectVM.LocalEquals(_lvProjectVM)))
                {
                    return;
                }
            }

            _lvProjectVM = new LV_ProjectVM(App.LVprojectVM);
            form_tv.DataContext = null;

            if (_lvProjectVM.Items.IsEmpty())
                return;

            new TreeView_DoubleFileVM(form_tv, LocalTV.RootNodes);
        }

        public WinDoubleFile_Folders_MUI()
        {
            InitializeComponent();
        }

        protected override void CopyTag(WeakReference wr)
        {
            LocalNavigatedTo();
        }

        LV_ProjectVM
            _lvProjectVM = null;
    }
}
