
namespace DoubleFile
{
    partial class UC_VolumeEditVM
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
                int? nSize;
                var probeStruct = new ProbeStruct();

                DriveSerial.Get(strPath, out probeStruct.DriveModel, out probeStruct.DriveSerial, out nSize);
                
                FromProbe(probeStruct);
            }
            else
            {
                MBox.Assert(0, false);
            }
        }

        internal void EditListingPath()
        {
            string strPath = ListingPath_CurrentText();
            var dlg = new Microsoft.Win32.SaveFileDialog();

            dlg.FileName = strPath;

            if (dlg.ShowDialog(GetWindow()) ?? false)
            {
                FromListingPathDlg(dlg.FileName);
            }
        }
    }
}
