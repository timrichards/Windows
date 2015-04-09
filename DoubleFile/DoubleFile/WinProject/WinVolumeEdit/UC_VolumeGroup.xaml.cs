using System.Reactive.Linq;
using System.Windows.Controls;
using System;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for UC_VolumeGroup.xaml
    /// </summary>
    public partial class UC_VolumeGroup : UserControl
    {
        public string Text { get { return form_EditVolumeGroup.Text; } set { form_EditVolumeGroup.Text = value; } }
        internal bool IsWinVolumeGroup { set; private get; }

        public UC_VolumeGroup()
        {
            InitializeComponent();

            Observable.FromEventPattern(form_grid, "Loaded")
                .Subscribe(args => Grid_Loaded());
        }

        private void Grid_Loaded()
        {
            if (IsWinVolumeGroup)
            {
                form_EditVolumeGroup.Focus();
                form_EditVolumeGroup.CaretIndex = int.MaxValue;
            }
        }
    }
}
