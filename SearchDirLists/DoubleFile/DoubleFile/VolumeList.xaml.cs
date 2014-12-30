using System.Windows;
using System.Windows.Input;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for VolumeList.xaml
    /// </summary>
    public partial class VolumeList : Window
    {
        public VolumeList()
        {
            InitializeComponent();
        }

        private void form_MenuAddVolume_Click(object sender, MouseButtonEventArgs e)
        {
            new VolumeEdit().ShowDialog();
        }
    }
}
