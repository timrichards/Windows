using System;
using System.Windows.Input;

namespace DoubleFile
{
    partial class UC_VolumeEditVM : ObservableObject_OwnerWindow
    {
        public BoolAction
            IsOKenabled = () => { DesignModeOK(); return false; };
        public StringAction
            SourcePath_CurrentText = () => { DesignModeOK(); return null; };
        public StringAction
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
        public ICommand Icmd_EditListingFile { get; private set; }
        public ICommand Icmd_IsOKenabled { get; private set; }

        internal UC_VolumeEditVM()
        {
            Icmd_EditSourcePath = new RelayCommand(param => { EditSourcePath(); });
            Icmd_Probe = new RelayCommand(param => { Probe(); }, param => IsOKenabled());
            Icmd_EditListingFile = new RelayCommand(param => { EditListingFile(); });
            Icmd_IsOKenabled = new RelayCommand(param => { }, param => IsOKenabled());
        }
    }
}
