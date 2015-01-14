using System.Windows.Input;

namespace DoubleFile
{
    partial class WinVolumeListVM : ObservableObject_OwnerWindow
    {
        // In order of appearance on the form
        public ICommand Icmd_LoadProject { get { return mIcmd_LoadProject; } }
        public ICommand Icmd_SaveProject { get { return mIcmd_SaveProject; } }

        public ICommand Icmd_NewListingFile { get { return mIcmd_NewListingFile; } }
        public ICommand Icmd_LoadListingFile { get { return mIcmd_LoadListingFile; } }

        public ICommand Icmd_EditListingFile { get { return mIcmd_EditListingFile; } }
        public ICommand Icmd_RemoveListingFile { get { return mIcmd_RemoveListingFile; } }

        public ICommand Icmd_ToggleInclude { get { return mIcmd_ToggleInclude; } }
        public ICommand Icmd_VolumeGroup { get { return mIcmd_VolumeGroup; } }

        readonly ICommand mIcmd_LoadProject = null;
        readonly ICommand mIcmd_SaveProject = null;

        readonly ICommand mIcmd_NewListingFile = null;
        readonly ICommand mIcmd_LoadListingFile = null;

        readonly ICommand mIcmd_EditListingFile = null;
        readonly ICommand mIcmd_RemoveListingFile = null;

        readonly ICommand mIcmd_ToggleInclude = null;
        readonly ICommand mIcmd_VolumeGroup = null;

        internal WinVolumeListVM()
        {
            mIcmd_LoadProject = new RelayCommand(param => { LoadProject(); });
            mIcmd_SaveProject = new RelayCommand(param => { SaveProject(); });

            mIcmd_NewListingFile = new RelayCommand(param => { NewListingFile(); });
            mIcmd_LoadListingFile = new RelayCommand(param => { LoadListingFile(); });

            mIcmd_EditListingFile = new RelayCommand(param => { m_lvVM.EditListingFile(); }, param => m_lvVM.SelectedOne());
            mIcmd_RemoveListingFile = new RelayCommand(param => { m_lvVM.RemoveListingFile(); }, param => m_lvVM.SelectedAny());

            mIcmd_ToggleInclude = new RelayCommand(param => { m_lvVM.ToggleInclude(); }, param => m_lvVM.SelectedAny());
            mIcmd_VolumeGroup = new RelayCommand(param => { m_lvVM.EditVolumeGroupLabel(); }, param => m_lvVM.SelectedAny());
        }

        internal void SetPartner(LV_VolumeVM lvVM)
        {
            m_lvVM = lvVM;
        }

        LV_VolumeVM m_lvVM = null;
    }
}
