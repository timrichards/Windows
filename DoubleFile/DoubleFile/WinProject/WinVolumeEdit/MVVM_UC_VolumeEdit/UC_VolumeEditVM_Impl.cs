using System.Windows;
using System.Windows.Forms;

namespace DoubleFile
{
    partial class UC_VolumeEditVM
    {
        internal void EditSourcePath()
        {
            var dlg = new FolderBrowserDialog
            {
                SelectedPath = SourcePath_CurrentText(),
                Description = "Source path of the directory for which a listing file is to be created."
            };

            if (DialogResult.OK == MainWindow.Darken(x => dlg.ShowDialog()))
                FromSourcePathDlg(dlg.SelectedPath);
        }

        internal void Probe()
        { 
            if (IsOKenabled())
            {
                ulong? nSize;
                string strDriveModel = null;
                string strDriveSerial = null;

                DriveSerialStatic.Get(SourcePath_CurrentText(), out strDriveModel, out strDriveSerial, out nSize);
                FromProbe(strDriveModel, strDriveSerial);
            }
            else
            {
                MBoxStatic.Assert(99927, false);
            }
        }

        internal void SaveListingFile()
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                FileName = ListingFile_CurrentText(),
                Filter = WinProjectVM.ListingFilter,
                OverwritePrompt = false,
                Title = "Save Listing File"
            };

            if (MainWindow.Darken(darkWindow => dlg.ShowDialog((Window)darkWindow)) ?? false)
                FromListingFileDlg(dlg.FileName);
        }
    }
}
