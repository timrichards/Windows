using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinVolumeEdit.xaml
    /// </summary>
    partial class WinVolumeGroup : Window
    {
        public WinVolumeGroup()
        {
            InitializeComponent();
        }

        public string Text { get { return form_UC_VolumeGroup.Text; } set { form_UC_VolumeGroup.Text = value; } }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }
    }
}
