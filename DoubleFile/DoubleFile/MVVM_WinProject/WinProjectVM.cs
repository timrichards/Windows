using System.Windows.Input;

namespace DoubleFile
{
    partial class WinProjectVM : ObservableObject_OwnerWindow
    {
        // In order of appearance on the form
        public ICommand Icmd_OpenProject { get; private set; }
        public ICommand Icmd_SaveProject { get; private set; }

        public ICommand Icmd_NewListingFile { get; private set; }
        public ICommand Icmd_OpenListingFile { get; private set; }

        public ICommand Icmd_EditListingFile { get; private set; }
        public ICommand Icmd_RemoveListingFile { get; private set; }

        public ICommand Icmd_ToggleInclude { get; private set; }
        public ICommand Icmd_VolumeGroup { get; private set; }

        internal WinProjectVM(GlobalData_Base gd_in, LV_ProjectVM lvVM)
        {
            _gd = gd_in;
            _lvVM = lvVM;

            Icmd_OpenProject = new RelayCommand(param => { OpenProject(); });
            Icmd_SaveProject = new RelayCommand(param => { SaveProject(); }, param => (false == _lvVM.Items.IsEmpty()));

            Icmd_NewListingFile = new RelayCommand(param => { NewListingFile(); });
            Icmd_OpenListingFile = new RelayCommand(param => { OpenListingFile(); });

            Icmd_EditListingFile = new RelayCommand(param => { _lvVM.EditListingFile(); }, param => _lvVM.SelectedOne());
            Icmd_RemoveListingFile = new RelayCommand(param => { _lvVM.RemoveListingFile(); }, param => _lvVM.SelectedAny());

            Icmd_ToggleInclude = new RelayCommand(param => { _lvVM.ToggleInclude(); }, param => _lvVM.SelectedAny());
            Icmd_VolumeGroup = new RelayCommand(param => { _lvVM.EditVolumeGroupLabel(); }, param => _lvVM.SelectedAny());
        }

        readonly LV_ProjectVM _lvVM = null;
        readonly GlobalData_Base _gd = null;
    }
}
