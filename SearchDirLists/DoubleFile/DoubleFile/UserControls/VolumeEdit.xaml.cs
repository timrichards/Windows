using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace DoubleFile.UserControls
{
    /// <summary>
    /// Interaction logic for VolumeEdit.xaml
    /// </summary>
    public partial class VolumeEdit : UserControl
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
                return new string[] { form_EditNickname.Text, form_EditSourcePath.Text, form_EditListingPath.Text, "Not saved", "Yes" };
            }
        }

        public VolumeEdit()
        {
            InitializeComponent();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = new VolumeEditVM();

            uc_VolumeEdit.DataContext = vm;
            vm.IsOKenabled = new VolumeEditVM.BoolQuery(() => { return ((form_EditSourcePath.Text.Length > 1) && (form_EditSourcePath.Text[1] == ':')); });
            vm.SourcePath_CurrentText = new VolumeEditVM.StringQuery(() => { return form_EditSourcePath.Text; });
            vm.ListingPath_CurrentText = new VolumeEditVM.StringQuery(() => { return form_EditListingPath.Text; });
            vm.FromSourcePathDlg = new System.Action<string>((s) => { form_EditSourcePath.Text = s; });
            vm.FromProbe = new System.Action<VolumeEditVM.ProbeStruct>((t) => { form_EditDriveModel.Text = t.DriveModel; form_EditSerialNo.Text = t.DriveSerial; });
            vm.FromListingPathDlg = new System.Action<string>((s) => { form_EditListingPath.Text = s; });
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            Window.GetWindow(uc_VolumeEdit).DialogResult = true;
        }
    }
}
