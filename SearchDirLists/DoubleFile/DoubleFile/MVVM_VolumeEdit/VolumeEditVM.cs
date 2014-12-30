using System.Windows.Input;

namespace DoubleFile
{
    partial class VolumeEditVM : ObservableObject
    {
        // In order of appearance on the form
        public ICommand Icmd_EditSourcePath { get { return mIcmd_EditSourcePath; } }
        public ICommand Icmd_Probe { get { return mIcmd_Probe; } }
        public ICommand Icmd_EditListingPath { get { return mIcmd_EditListingPath; } }
        public ICommand Icmd_OK { get { return mIcmd_OK; } }

        readonly ICommand mIcmd_EditSourcePath = null;
        readonly ICommand mIcmd_Probe = null;
        readonly ICommand mIcmd_EditListingPath = null;
        readonly ICommand mIcmd_OK = null;

        internal VolumeEditVM()
        {
            mIcmd_EditSourcePath = new RelayCommand(param => { EditSourcePath(); });
            mIcmd_Probe = new RelayCommand(param => { Probe(); });
            mIcmd_EditListingPath = new RelayCommand(param => { EditListingPath(); });
            mIcmd_OK = new RelayCommand(param => { OK(); });
        }
    }
}
