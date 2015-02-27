using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinVolumeEdit.xaml
    /// </summary>
    partial class WinVolumeGroup
    {
        public string Text { get { return form_ucVolumeGroup.Text; } set { form_ucVolumeGroup.Text = value; } }

        public WinVolumeGroup()
        {
            InitializeComponent();
            form_grid.Loaded += (o, e) => form_ucVolumeGroup.IsWinVolumeGroup = true;
            form_btnOK.Click += (o, e) => { LocalDialogResult = true; CloseIfSimulatingModal(); };
            form_btnCancel.Click += (o, e) => CloseIfSimulatingModal();
        }
    }
}
