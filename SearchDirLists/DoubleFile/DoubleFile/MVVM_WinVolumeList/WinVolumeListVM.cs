using System.Windows.Input;

namespace DoubleFile
{
    partial class WinVolumeListVM : ObservableObject_OwnerWindow
    {
        // In order of appearance on the form
        public ICommand Icmd_LoadProject { get { return mIcmd_LoadProject; } }
        public ICommand Icmd_SaveProject { get { return mIcmd_SaveProject; } }

        public ICommand Icmd_NewVolume { get { return mIcmd_NewVolume; } }
        public ICommand Icmd_LoadVolume { get { return mIcmd_LoadVolume; } }
        public ICommand Icmd_SaveVolume { get { return mIcmd_SaveVolume; } }

        public ICommand Icmd_EditVolume { get { return mIcmd_EditVolume; } }
        public ICommand Icmd_RemoveVolume { get { return mIcmd_RemoveVolume; } }

        public ICommand Icmd_ToggleInclude { get { return mIcmd_ToggleInclude; } }

        readonly ICommand mIcmd_LoadProject = null;
        readonly ICommand mIcmd_SaveProject = null;

        readonly ICommand mIcmd_NewVolume = null;
        readonly ICommand mIcmd_LoadVolume = null;
        readonly ICommand mIcmd_SaveVolume = null;

        readonly ICommand mIcmd_EditVolume = null;
        readonly ICommand mIcmd_RemoveVolume = null;

        readonly ICommand mIcmd_ToggleInclude = null;

        internal WinVolumeListVM()
        {
            mIcmd_LoadProject = new RelayCommand(param => { LoadProject(); });
            mIcmd_SaveProject = new RelayCommand(param => { SaveProject(); });

            mIcmd_NewVolume = new RelayCommand(param => { NewVolume(); });
            mIcmd_LoadVolume = new RelayCommand(param => { LoadVolume(); });
            // probably not:
            mIcmd_SaveVolume = new RelayCommand(param => { m_lvVM.SaveVolume(); }, param => m_lvVM.SelectedOne() );

            mIcmd_EditVolume = new RelayCommand(param => { m_lvVM.EditVolume(); }, param => m_lvVM.SelectedOne());
            mIcmd_RemoveVolume = new RelayCommand(param => { RemoveVolume(); }, param => m_lvVM.SelectedAny());

            mIcmd_ToggleInclude = new RelayCommand(param => { m_lvVM.ToggleInclude(); }, param => m_lvVM.SelectedAny());
        
            // edit volume group selected any
        }

        internal void SetPartner(LV_VolumeVM lvVM)
        {
            m_lvVM = lvVM;
        }

        LV_VolumeVM m_lvVM = null;
    }
}
