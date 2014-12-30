using System;
using System.Windows;

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

        private void form_btnProbe_Click(object sender, RoutedEventArgs e)
        {
            String strPath = form_editPath.Text;

            if ((strPath.Length>1) && (strPath[1]==':'))
            {
                String strModel;
                String strSerialNo;
                int? nSize;

                DriveSerial.Get(strPath, out strModel, out strSerialNo, out nSize);

                form_editModel.Text = strModel;
                form_editSerialNo.Text = strSerialNo;
            }
        }
    }
}
