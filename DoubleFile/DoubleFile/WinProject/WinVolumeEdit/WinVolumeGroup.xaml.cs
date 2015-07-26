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
                .LocalSubscribe(x => formUC_VolumeGroup.IsWinVolumeGroup = true);

            Observable.FromEventPattern(formBtn_OK, "Click")
                .LocalSubscribe(x => { LocalDialogResult = true; CloseIfSimulatingModal(); });

            Observable.FromEventPattern(formBtn_Cancel, "Click")
                .LocalSubscribe(x => CloseIfSimulatingModal());
        }
    }
}
