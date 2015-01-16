using System.Windows.Input;

namespace DoubleFile
{
    partial class WinVolumeListVM : ObservableObject_OwnerWindow
    {
        // In order of appearance on the form
        public ICommand Icmd_OpenProject { get { return mIcmd_OpenProject; } }
        public ICommand Icmd_SaveProject { get { return mIcmd_SaveProject; } }

        public ICommand Icmd_NewListingFile { get { return mIcmd_NewListingFile; } }
        public ICommand Icmd_OpenListingFile { get { return mIcmd_OpenListingFile; } }

        public ICommand Icmd_EditListingFile { get { return mIcmd_EditListingFile; } }
        public ICommand Icmd_RemoveListingFile { get { return mIcmd_RemoveListingFile; } }

        public ICommand Icmd_ToggleInclude { get { return mIcmd_ToggleInclude; } }
        public ICommand Icmd_VolumeGroup { get { return mIcmd_VolumeGroup; } }

        readonly ICommand mIcmd_OpenProject = null;
        readonly ICommand mIcmd_SaveProject = null;

        readonly ICommand mIcmd_NewListingFile = null;
        readonly ICommand mIcmd_OpenListingFile = null;

        readonly ICommand mIcmd_EditListingFile = null;
        readonly ICommand mIcmd_RemoveListingFile = null;

        readonly ICommand mIcmd_ToggleInclude = null;
        readonly ICommand mIcmd_VolumeGroup = null;

        internal WinVolumeListVM()
        {
            mIcmd_OpenProject = new RelayCommand(param => { OpenProject(); });
            mIcmd_SaveProject = new RelayCommand(param => { SaveProject(); }, param => { return (m_lvVM.Count > 0); });

            mIcmd_NewListingFile = new RelayCommand(param => { NewListingFile(); });
            mIcmd_OpenListingFile = new RelayCommand(param => { OpenListingFile(); });

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
