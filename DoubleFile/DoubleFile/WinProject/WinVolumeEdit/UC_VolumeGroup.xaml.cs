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
        public string Text { get { return formEdit_VolumeGroup.Text; } set { formEdit_VolumeGroup.Text = value; } }
        internal bool IsWinVolumeGroup { private get; set; }

        public UC_VolumeGroup()
        {
            InitializeComponent();

            Observable.FromEventPattern(this, "Loaded")
                .Subscribe(x =>
            {
                if (IsWinVolumeGroup)
                {
                    formEdit_VolumeGroup.Focus();
                    formEdit_VolumeGroup.CaretIndex = int.MaxValue;
                }
            });
        }
    }
}
