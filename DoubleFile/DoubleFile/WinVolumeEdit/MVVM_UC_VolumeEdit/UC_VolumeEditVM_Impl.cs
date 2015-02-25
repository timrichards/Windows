
namespace DoubleFile
{
    partial class UC_VolumeEditVM
    {
        internal void EditSourcePath()
        {
            var dlg = new System.Windows.Forms.FolderBrowserDialog
            {
                SelectedPath = SourcePath_CurrentText(),
                Description = "Source path of the directory for which a listing file is to be created."
            };


            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                FromSourcePathDlg(dlg.SelectedPath);
            }
        }

        internal void Probe()
        { 
            var strPath = SourcePath_CurrentText();

            if (IsOKenabled())
            {
                ulong? nSize;
                string strDriveModel = null;
                string strDriveSerial = null;

                DriveSerialStatic.Get(strPath, out strDriveModel, out strDriveSerial, out nSize);
                FromProbe(strDriveModel, strDriveSerial);
            }
            else
            {
                MBoxStatic.Assert(99927, false);
            }
        }

        internal void EditListingFile()
        {
            var strPath = ListingFile_CurrentText();
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = strPath,
                Filter = WinProjectVM.ListingFilter,
                OverwritePrompt = false,
                Title = "Save Listing File"
            };

            if (dlg.ShowDialog() ?? false)
            {
                FromListingFileDlg(dlg.FileName);
            }
        }
    }
}
