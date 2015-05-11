using System.Reactive.Linq;
using System.Windows;
using System;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinVolumeEdit.xaml
    /// </summary>
    partial class WinVolumeGroup
    {
        public string Text { get { return formUC_VolumeGroup.Text; } set { formUC_VolumeGroup.Text = value; } }

        public WinVolumeGroup()
        {
            InitializeComponent();

            Observable.FromEventPattern(form_grid, "Loaded")
                .Subscribe(args => formUC_VolumeGroup.IsWinVolumeGroup = true);

            Observable.FromEventPattern(formBtn_OK, "Click")
                .Subscribe(args => { LocalDialogResult = true; CloseIfSimulatingModal(); });

            Observable.FromEventPattern(formBtn_Cancel, "Click")
                .Subscribe(args => { CloseIfSimulatingModal(); });
        }
    }
}
