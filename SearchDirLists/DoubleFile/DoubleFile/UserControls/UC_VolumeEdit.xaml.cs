using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace DoubleFile.UserControls
{
    /// <summary>
    /// Interaction logic for UC_VolumeEdit.xaml
    /// </summary>
    partial class UC_VolumeEdit : UserControl
    {
        [Description("User control supports choice"), Category("New or Edit Volume window")]
        public bool IsVolumeNew
        {
            set
            {
                form_EditSourcePath.IsEnabled = value;
                form_BtnSourcePath.IsEnabled = value;
                form_Border.VerticalAlignment = value ? VerticalAlignment.Top : VerticalAlignment.Center;
                form_lblDriveLetter.Visibility = value ? Visibility.Hidden : Visibility.Visible;
                form_EditDriveLetter.Visibility = value ? Visibility.Hidden : Visibility.Visible;
                form_BtnDriveModel.IsEnabled = value;
                form_BtnListingPath.IsEnabled = value;
                form_EditListingPath.IsEnabled = value;
            }
            get
            {
                return form_EditSourcePath.IsEnabled;
            }
        }

        public string[] StringValues
        {
            get
            {
                return new string[] { form_EditNickname.Text, form_EditSourcePath.Text, form_EditListingPath.Text, "Not saved", "Yes",
                    form_EditVolumeGroup.Text, form_EditDriveModel.Text, form_EditDriveSerial.Text };
            }
            set
            {
                int i = 0;

                if (value.Length > i) { form_EditNickname.Text = value[i++]; } else { System.Diagnostics.Debug.Assert(false); }
                if (value.Length > i) { form_EditSourcePath.Text = value[i++]; } else { System.Diagnostics.Debug.Assert(false); }
                if (value.Length > i) { form_EditListingPath.Text = value[i++]; }
                if (value.Length > i) { i++; }  // Status
                if (value.Length > i) { i++; }  // Include
                if (value.Length > i) { form_EditVolumeGroup.Text = value[i++]; }
                if (value.Length > i) { form_EditDriveModel.Text = value[i++]; }
                if (value.Length > i) { form_EditDriveSerial.Text = value[i++]; }
            }
        }

        public UC_VolumeEdit()
        {
            InitializeComponent();
        }

        bool IsOKenabled
        {
            get
            {
                return ((form_EditSourcePath.Text.Length > 1) && (form_EditSourcePath.Text[1] == ':'));
            }
        }

        Window window { get { return Window.GetWindow(uc_VolumeEdit); } }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = new UC_VolumeEditVM();

            uc_VolumeEdit.DataContext = vm;
            vm.IsOKenabled = () => { return IsOKenabled; };
            vm.SourcePath_CurrentText = () => { return form_EditSourcePath.Text; };
            vm.ListingPath_CurrentText = () => { return form_EditListingPath.Text; };
            vm.FromSourcePathDlg = s => { form_EditSourcePath.Text = s; };
            vm.FromProbe = t => { form_EditDriveModel.Text = t.DriveModel; form_EditDriveSerial.Text = t.DriveSerial; };
            vm.FromListingPathDlg = s => { form_EditListingPath.Text = s; };

            vm.GetWindow = () => { return window; };
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            if (IsOKenabled)
            {
                window.DialogResult = true;
            }
            else
            {
                System.Diagnostics.Debug.Assert(false);
            }
        }
    }
}
