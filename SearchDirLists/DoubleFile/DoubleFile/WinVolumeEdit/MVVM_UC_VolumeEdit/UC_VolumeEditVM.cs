using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace DoubleFile
{
    partial class UC_VolumeEditVM : ObservableObject_OwnerWindow
    {
        public delegate bool BoolQuery();
        public BoolQuery IsOKenabled = () => { DesignModeOK(); return false; };
        public delegate string StringQuery();
        public StringQuery SourcePath_CurrentText = () => { DesignModeOK(); return null; };
        public StringQuery ListingPath_CurrentText = () => { DesignModeOK(); return null; };
        public System.Action<string> FromSourcePathDlg = s => { DesignModeOK(); };
        public struct ProbeStruct { public string DriveModel; public string DriveSerial; };
        public System.Action<ProbeStruct> FromProbe = s => { DesignModeOK(); };
        public System.Action<string> FromListingPathDlg = s => { DesignModeOK(); };

        // In order of appearance on the form
        public ICommand Icmd_EditSourcePath { get { return mIcmd_EditSourcePath; } }
        public ICommand Icmd_Probe { get { return mIcmd_Probe; } }
        public ICommand Icmd_EditListingPath { get { return mIcmd_EditListingPath; } }
        public ICommand Icmd_IsOKenabled { get { return mIcmd_IsOKenabled; } }

        readonly ICommand mIcmd_EditSourcePath = null;
        readonly ICommand mIcmd_Probe = null;
        readonly ICommand mIcmd_EditListingPath = null;
        readonly ICommand mIcmd_IsOKenabled = null;

        internal UC_VolumeEditVM()
        {
            mIcmd_EditSourcePath = new RelayCommand(param => { EditSourcePath(); });
            mIcmd_Probe = new RelayCommand(param => { Probe(); }, param => { return IsOKenabled(); });
            mIcmd_EditListingPath = new RelayCommand(param => { EditListingPath(); });
            mIcmd_IsOKenabled = new RelayCommand(param => { }, param => { return IsOKenabled(); });
        }
    }
}
