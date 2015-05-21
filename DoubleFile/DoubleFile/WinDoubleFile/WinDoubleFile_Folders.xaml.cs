﻿using System;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinFolders.xaml
    /// </summary>
    partial class WinFolders
    {
        public WinFolders()
        {
            InitializeComponent();
        }

        protected override void LocalNavigatedTo()
        {
            new TreeView_DoubleFileVM(form_tv, LocalTV.RootNodes);
        }

        protected override void CopyTag_NewWindow(WeakReference wr)
        {
            LocalNavigatedTo();
        }
    }
}
