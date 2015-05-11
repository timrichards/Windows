using System;
using System.Windows.Input;

namespace DoubleFile
{
    partial class UC_VolumeEditVM : Observable_OwnerWindowBase
    {
        public Func<bool>
            IsOKenabled = () => { DesignModeOK(); return false; };
        public Func<string>
            SourcePath_CurrentText = () => { DesignModeOK(); return null; };
        public Func<string>
            ListingFile_CurrentText = () => { DesignModeOK(); return null; };
        public Action<string>
            FromSourcePathDlg = s => DesignModeOK();
        public Action<string, string>
            FromProbe = (strDriveModel, strDriveSerial) => DesignModeOK();
        public Action<string>
            FromListingFileDlg = s => DesignModeOK();

        // In order of appearance on the form
        public ICommand Icmd_EditSourcePath { get; private set; }
        public ICommand Icmd_Probe { get; private set; }
        public ICommand Icmd_SaveListingFile { get; private set; }
        public ICommand Icmd_IsOKenabled { get; private set; }

        internal UC_VolumeEditVM Init()
        {
            Icmd_EditSourcePath = new RelayCommand(EditSourcePath);
            Icmd_Probe = new RelayCommand(Probe, IsOKenabled);
            Icmd_SaveListingFile = new RelayCommand(SaveListingFile);
            Icmd_IsOKenabled = new RelayCommand(() => { }, IsOKenabled);
            return this;
        }
    }
}
