
namespace DoubleFile
{
    partial class VolumeEditVM : ObservableObject
    {
        internal void EditSourcePath()
        {
            new System.Windows.Forms.FolderBrowserDialog().ShowDialog();
            System.Windows.MessageBox.Show("EditSourcePath");
        }

        internal void Probe()
        { 
            System.Windows.MessageBox.Show("Probe");

            //string strPath = form_editPath.Text;

            //if ((strPath.Length > 1) && (strPath[1] == ':'))
            //{
            //    string strModel;
            //    string strSerialNo;
            //    int? nSize;

            //    DriveSerial.Get(strPath, out strModel, out strSerialNo, out nSize);

            //    form_editModel.Text = strModel;
            //    form_editSerialNo.Text = strSerialNo;
            //}
        }

        internal void EditListingPath()
        {
            new Microsoft.Win32.SaveFileDialog().ShowDialog();
            System.Windows.MessageBox.Show("EditListingPath"); 
        }

        internal void OK() { System.Windows.MessageBox.Show("OK"); }
    }
}
