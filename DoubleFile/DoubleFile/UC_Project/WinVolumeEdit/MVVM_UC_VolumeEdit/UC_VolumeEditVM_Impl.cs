using System.Windows;

namespace DoubleFile
{
    partial class UC_VolumeEditVM
    {
        internal void EditSourcePath()
        {
            var dlg = new FolderBrowserDialog
            {
                SelectedPath = SourcePath_CurrentText(),
                Description = "Source path of the directory for which a listing file is to be created.",
                ShowNewFolderButton = false
            };

            if (ModalThread.Go(darkWindow => dlg.ShowDialog((Window)darkWindow)))
                FromSourcePathDlg(dlg.SelectedPath);
        }

        internal void Probe()
        { 
            if (IsOKenabled())
            {
                string strDriveModel = DriveModel_CurrentText();
                string strDriveSerial = DriveSerial_CurrentText();

                DriveSerialStatic.Get(SourcePath_CurrentText(), ref strDriveModel, ref strDriveSerial);
                FromProbe(strDriveModel, strDriveSerial);
            }
            else
            {
                Util.Assert(99927, false);
            }
        }

        internal void SaveListingFile()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = ListingFile_CurrentText(),
                Filter = UC_ProjectVM.ListingFilter,
                OverwritePrompt = false,
                Title = "Save Listing File"
            };

            if (ModalThread.Go(darkWindow => dlg.ShowDialog((Window)darkWindow) ?? false))
                FromListingFileDlg(dlg.FileName);
        }
    }
}
