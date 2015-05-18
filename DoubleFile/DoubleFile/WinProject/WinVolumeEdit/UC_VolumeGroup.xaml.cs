﻿using System.Reactive.Linq;
using System.Windows.Controls;
using System;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for UC_VolumeGroup.xaml
    /// </summary>
    public partial class UC_VolumeGroup : UserControl
    {
        public string Text { get { return formEdit_VolumeGroup.Text; } set { formEdit_VolumeGroup.Text = value; } }
        internal bool IsWinVolumeGroup { set; private get; }

        public UC_VolumeGroup()
        {
            InitializeComponent();

            Observable.FromEventPattern(form_grid, "Loaded")
                .Subscribe(x => Grid_Loaded());
        }

        private void Grid_Loaded()
        {
            if (IsWinVolumeGroup)
            {
                formEdit_VolumeGroup.Focus();
                formEdit_VolumeGroup.CaretIndex = int.MaxValue;
            }
        }
    }
}
