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
    /// Interaction logic for VolumeEdit.xaml
    /// </summary>
    public partial class VolumeEdit : Window
    {
        public VolumeEdit()
        {
            InitializeComponent();
        }

        private void form_btnPath_Click(object sender, RoutedEventArgs e)
        {
            new System.Windows.Forms.FolderBrowserDialog().ShowDialog();
        }

        private void form_btnDirlistFile_Click(object sender, RoutedEventArgs e)
        {
            new Microsoft.Win32.SaveFileDialog().ShowDialog();
        }
    }
}
