using System;
using System.Windows.Input;

namespace DoubleFile
{
    partial class UC_VolumeEditVM : Observable_OwnerWindowBase
    {
        public Func<bool>
            IsOKenabled = () => DesignModeOK(false);
        public Func<string>
            SourcePath_CurrentText = () => DesignModeOK<string>(null);
        public Func<string>
            ListingFile_CurrentText = () => DesignModeOK<string>(null);
        public Func<string>
            DriveModel_CurrentText = () => DesignModeOK<string>(null);
        public Func<string>
            DriveSerial_CurrentText = () => DesignModeOK<string>(null);
        public Action<string>
            FromSourcePathDlg = s => DesignModeOK<string>(null);
        public Action<string, string>
            FromProbe = (strDriveModel, strDriveSerial) => DesignModeOK<string>(null);
        public Action<string>
            FromListingFileDlg = s => DesignModeOK<string>(null);

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
