using System.Windows;
using System.Windows.Input;

namespace DoubleFile
{
    partial class WinProjectVM : Observable_OwnerWindowBase
    {
        public Visibility SaveProjectProgressVisibility { get; private set; } = Visibility.Collapsed;
        public bool IsEnabled => Visibility.Visible != SaveProjectProgressVisibility;

        // In order of appearance on the form
        public ICommand Icmd_OpenProject { get; }
        public ICommand Icmd_SaveProject { get; }

        public ICommand Icmd_NewListingFile { get; }
        public ICommand Icmd_OpenListingFile { get; }

        public ICommand Icmd_EditListingFile { get; }
        public ICommand Icmd_RemoveListingFile { get; }

        public ICommand Icmd_ToggleInclude { get; }
        public ICommand Icmd_VolumeGroup { get; }

        internal WinProjectVM(LV_ProjectVM lvVM)
            : this()
        {
            _lvVM = lvVM;

            Icmd_OpenProject = new RelayCommand(OpenProject);
            Icmd_SaveProject = new RelayCommand(SaveProject, () => 0 < _lvVM.Items.Count);

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
