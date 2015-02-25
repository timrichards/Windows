using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinVolumeEdit.xaml
    /// </summary>
    partial class WinVolumeGroup
    {
        public string Text { get { return form_UC_VolumeGroup.Text; } set { form_UC_VolumeGroup.Text = value; } }

        public WinVolumeGroup()
        {
            InitializeComponent();
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            LocalDialogResult = true;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            form_UC_VolumeGroup.IsWinVolumeGroup = true;
        }
    }
}
