using System.Windows.Input;

namespace DoubleFile
{
    partial class VolumeListVM : ObservableObject
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

        internal VolumeListVM(VolumeList window)
        {
            window.form_VolumeList.DataContext = this;

            mIcmd_LoadProject = new RelayCommand(param => { LoadProject(); });
            mIcmd_SaveProject = new RelayCommand(param => { SaveProject(); });

            mIcmd_NewVolume = new RelayCommand(param => { NewVolume(); });
            mIcmd_LoadVolume = new RelayCommand(param => { LoadVolume(); });
            mIcmd_SaveVolume = new RelayCommand(param => { SaveVolume(); });

            mIcmd_EditVolume = new RelayCommand(param => { EditVolume(); });
            mIcmd_RemoveVolume = new RelayCommand(param => { RemoveVolume(); });

            mIcmd_ToggleInclude = new RelayCommand(param => { ToggleInclude(); });
        }
    }
}
