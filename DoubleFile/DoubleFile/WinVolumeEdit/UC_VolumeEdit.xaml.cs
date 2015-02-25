using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using System.IO;

namespace DoubleFile
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
                form_BtnListingFile.IsEnabled = value;
                form_EditListingFile.IsEnabled = value;
            }
            get
            {
                return form_EditSourcePath.IsEnabled;
            }
        }

        public UC_VolumeEdit()
        {
            InitializeComponent();
        }

        internal char DriveLetter { get { return (form_EditDriveLetter.Text + "\0")[0]; } }

        internal LVitem_ProjectVM LVitemVolumeTemp
        {
            get
            {
                return new LVitem_ProjectVM(new string[] {
                    form_EditNickname.Text, form_EditSourcePath.Text,
                    (IsVolumeNew ? form_EditListingFile.Text : _strListingFile),
                    _strStatus, _strIncludeYN,
                    form_UC_VolumeGroup.Text, form_EditDriveModel.Text, form_EditDriveSerial.Text
                });
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                var astr = value.StringValues;
                var i = 0;

                if (astr.Length > i) { form_EditNickname.Text = value[i++]; } else { MBoxStatic.Assert(99926, false); }
                if (astr.Length > i) { form_EditSourcePath.Text = value[i++]; } else { MBoxStatic.Assert(99925, false); }
                if (astr.Length > i) { _strListingFile = value[i++]; form_EditListingFile.Text = (IsVolumeNew ? _strListingFile : Path.GetFileName(_strListingFile)); }
                if (astr.Length > i) { var s = value[i++]; if (s != null) { _strStatus = s; } }
                if (astr.Length > i) { var s = value[i++]; if (s != null) { _strIncludeYN = s; } }
                if (astr.Length > i) { form_UC_VolumeGroup.Text = value[i++]; }
                if (astr.Length > i) { form_EditDriveModel.Text = value[i++]; }
                if (astr.Length > i) { form_EditDriveSerial.Text = value[i++]; }
            }
        }

        bool IsValidListingEdit
        {
            get
            {
                var strListingFile = form_EditListingFile.Text;

                try { strListingFile = Path.GetFullPath(strListingFile); }
                catch (ArgumentException) { return false; }

                return (Path.GetFileName(strListingFile).Length > 0);
            }
        }

        bool IsValidSourcePathEdit
        {
            get
            {
                return (SaveDirListings.IsGoodDriveSyntax(form_EditSourcePath.Text) &&
                    ((IsVolumeNew == false) || Directory.Exists(form_EditSourcePath.Text)));
            }
        }

        bool IsOKenabled
        {
            get
            {
                return ((string.IsNullOrWhiteSpace(form_EditListingFile.Text) || IsValidListingEdit) &&
                    IsValidSourcePathEdit);
            }
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = new UC_VolumeEditVM();

            uc_VolumeEdit.DataContext = vm;
            vm.IsOKenabled = () => IsOKenabled;
            vm.SourcePath_CurrentText = () => form_EditSourcePath.Text;
            vm.ListingFile_CurrentText = () => form_EditListingFile.Text;
            vm.FromSourcePathDlg = s => form_EditSourcePath.Text = s;
            vm.FromProbe = (strDriveModel, strDriveSerial) => { form_EditDriveModel.Text = strDriveModel; form_EditDriveSerial.Text = strDriveSerial; };
            vm.FromListingFileDlg = s => form_EditListingFile.Text = s;

            form_EditDriveLetter.CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, (o, e1) => { e1.Handled = true; }));
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            if (IsOKenabled)
            {
                var window = Window.GetWindow(uc_VolumeEdit) as LocalWindow;

                if (window != null)
                {
                    window.LocalDialogResult = true;
                    window.CloseIfSimulatingModal();
                }
            }
            else
            {
                MBoxStatic.Assert(99924, false);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(uc_VolumeEdit) as LocalWindow;

            if (window != null)
            {
                window.CloseIfSimulatingModal();
            }
        }

        static string CapDrive(string strPath)
        {
            var a = strPath.ToCharArray();

            if (a.Length > 0)
            {
                a[0] = a[0].ToString().ToUpper()[0];
                return string.Join("", a);
            }

            return strPath;
        }

        private void form_EditSourcePath_LostFocus(object sender, RoutedEventArgs e)
        {
            if (IsValidSourcePathEdit)
            {
                form_EditSourcePath.Text = CapDrive(form_EditSourcePath.Text);
            }
        }

        private void form_EditListingFile_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(form_EditListingFile.Text) ||
                (false == IsValidListingEdit))
            {
                return;
            }

            var strListingFile = CapDrive(Path.GetFullPath(form_EditListingFile.Text));
            var strExt = Path.GetExtension(strListingFile) ?? "";

            if ((strExt.Length == 0) || (false ==
                strExt.Remove(0, 1)
                .Equals(FileParse.ksFileExt_Listing,
                StringComparison.InvariantCultureIgnoreCase)))
            {
                strListingFile += "." + FileParse.ksFileExt_Listing;
            }

            form_EditListingFile.Text = strListingFile;
        }

        private void form_EditDriveLetter_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (new Key[] {Key.Tab, Key.Back, Key.Delete, Key.Left, Key.Right}.Contains(e.Key))
            {
                return;
            }

            if (form_EditDriveLetter.Text.Length > 0)
            {
                e.Handled = true;
                return;
            }
            
            if ((e.Key < Key.A) || (e.Key > Key.Z))
            {
                e.Handled = true;
                return;
            }

            if ((new KeyConverter().ConvertToString(e.Key) + "\0")[0] ==
                form_EditSourcePath.Text[0])
            {
                e.Handled = true;
                return;
            }
        }

        private void uc_VolumeEdit_Loaded(object sender, RoutedEventArgs e)
        {
            var textBox = (IsVolumeNew ? form_EditSourcePath : form_EditDriveLetter);

            textBox.Focus();
            textBox.CaretIndex = int.MaxValue;
        }

        string _strListingFile = null;
        string _strStatus = FileParse.ksNotSaved;
        string _strIncludeYN = FileParse.ksInclude;
    }
}
