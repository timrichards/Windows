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
                Description = "Source path of the directory for which a listing file is to be created.",
                ShowNewFolderButton = false
            };

            if (DialogResult.OK ==
                ModalThread.Go(darkWindow => dlg.ShowDialog(((NativeWindow)(Window)darkWindow)), dlg.GetType().Name))
            {
                FromSourcePathDlg(dlg.SelectedPath);
            }
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
                Filter = WinProjectVM.ListingFilter,
                OverwritePrompt = false,
                Title = "Save Listing File"
            };

            if (ModalThread.Go(darkWindow => dlg.ShowDialog((Window)darkWindow), dlg.Title) ?? false)
                FromListingFileDlg(dlg.FileName);
        }
    }
}
