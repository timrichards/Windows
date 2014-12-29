using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for DiskVolumeList.xaml
    /// </summary>
    public partial class DiskVolumeList : Window
    {
        public DiskVolumeList()
        {
            InitializeComponent();
        }

        private void form_MenuAddVolume_Click(object sender, MouseButtonEventArgs e)
        {
            new VolumeEdit().ShowDialog();
        }
    }
}
