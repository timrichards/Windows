
namespace DoubleFile
{
    partial class VolumeEditVM : ObservableObject
    {
        internal void EditSourcePath()
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog();

            dlg.SelectedPath = SourcePath_CurrentText();

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FromSourcePathDlg(dlg.SelectedPath);
            }
        }

        internal void Probe()
        { 
            string strPath = SourcePath_CurrentText();

            if (IsOKenabled())
            {
                string strModel;
                string strSerialNo;
                int? nSize;

                DriveSerial.Get(strPath, out strModel, out strSerialNo, out nSize);
                
                ProbeStruct probeStruct = new ProbeStruct();

                probeStruct.DriveModel = strModel;
                probeStruct.DriveSerial = strSerialNo;
                FromProbe(probeStruct);
            }
        }

        internal void EditListingPath()
        {
            string strPath = ListingPath_CurrentText();
            var dlg = new Microsoft.Win32.SaveFileDialog();

            dlg.FileName = strPath;

            if (dlg.ShowDialog() ?? false)
            {
                FromListingPathDlg(dlg.FileName);
            }
        }
    }
}
