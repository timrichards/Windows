using System.Windows;
using System.Windows.Input;

namespace DoubleFile
{
    partial class UC_VolumeEditVM : ObservableObject
    {
        public delegate bool BoolQuery();
        public BoolQuery IsOKenabled = null;
        public delegate string StringQuery();
        public StringQuery SourcePath_CurrentText = null;
        public StringQuery ListingPath_CurrentText = null;
        public System.Action<string> FromSourcePathDlg = null;
        public struct ProbeStruct { public string DriveModel; public string DriveSerial; };
        public System.Action<ProbeStruct> FromProbe = null;
        public System.Action<string> FromListingPathDlg = null;

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
