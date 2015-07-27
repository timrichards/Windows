using System.Reactive.Linq;
using System.Windows;
using System;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinVolumeGroup.xaml
    /// </summary>
    partial class WinVolumeGroup : IModalWindow
    {
        public string Text { get { return formUC_VolumeGroup.Text; } set { formUC_VolumeGroup.Text = value; } }

        public WinVolumeGroup()
        {
            InitializeComponent();

            Observable.FromEventPattern(this, "Loaded")
                .LocalSubscribe(99712, x => formUC_VolumeGroup.IsWinVolumeGroup = true);

            Observable.FromEventPattern(formBtn_OK, "Click")
                .LocalSubscribe(99711, x => { LocalDialogResult = true; CloseIfSimulatingModal(); });

            Observable.FromEventPattern(formBtn_Cancel, "Click")
                .LocalSubscribe(99710, x => CloseIfSimulatingModal());
        }
    }
}
