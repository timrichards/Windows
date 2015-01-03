﻿using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace DoubleFile.UserControls
{
    /// <summary>
    /// Interaction logic for UC_VolumeEdit.xaml
    /// </summary>
    public partial class UC_VolumeEdit : UserControl
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

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = new UC_VolumeEditVM();

            uc_VolumeEdit.DataContext = vm;
            vm.IsOKenabled = new UC_VolumeEditVM.BoolQuery(() => { return IsOKenabled; });
            vm.SourcePath_CurrentText = new UC_VolumeEditVM.StringQuery(() => { return form_EditSourcePath.Text; });
            vm.ListingPath_CurrentText = new UC_VolumeEditVM.StringQuery(() => { return form_EditListingPath.Text; });
            vm.FromSourcePathDlg = new System.Action<string>((s) => { form_EditSourcePath.Text = s; });
            vm.FromProbe = new System.Action<UC_VolumeEditVM.ProbeStruct>((t) => { form_EditDriveModel.Text = t.DriveModel; form_EditDriveSerial.Text = t.DriveSerial; });
            vm.FromListingPathDlg = new System.Action<string>((s) => { form_EditListingPath.Text = s; });
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            if (IsOKenabled)
            {
                Window.GetWindow(uc_VolumeEdit).DialogResult = true;
            }
            else
            {
                System.Diagnostics.Debug.Assert(false);
            }
        }
    }
}
