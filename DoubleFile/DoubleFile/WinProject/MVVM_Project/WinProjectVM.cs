using System.Windows.Input;
using System.Linq;

namespace DoubleFile
{
    partial class WinProjectVM : Observable_OwnerWindowBase
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

        internal WinProjectVM(LV_ProjectVM lvVM)
        {
            _lvVM = lvVM;

            Icmd_OpenProject = new RelayCommand(OpenProject);
            Icmd_SaveProject = new RelayCommand(SaveProject, () => _lvVM.Items.LocalAny());

            Icmd_NewListingFile = new RelayCommand(NewListingFile);
            Icmd_OpenListingFile = new RelayCommand(OpenListingFile);

            Icmd_EditListingFile = new RelayCommand(_lvVM.EditListingFile, _lvVM.SelectedOne);
            Icmd_RemoveListingFile = new RelayCommand(_lvVM.RemoveListingFile, _lvVM.SelectedAny);

            Icmd_ToggleInclude = new RelayCommand(_lvVM.ToggleInclude, _lvVM.SelectedAny);
            Icmd_VolumeGroup = new RelayCommand(_lvVM.EditVolumeGroupLabel, _lvVM.SelectedAny);
        }

        readonly LV_ProjectVM _lvVM = null;
    }
}
