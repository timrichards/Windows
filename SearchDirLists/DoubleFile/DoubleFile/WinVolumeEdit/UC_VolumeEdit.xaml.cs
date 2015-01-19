using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;

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
                    (IsVolumeNew ? form_EditListingFile.Text : m_strListingFile),
                    m_strStatus, m_strIncludeYN,
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

                if (astr.Length > i) { form_EditNickname.Text = value[i++]; } else { MBox.Assert(0, false); }
                if (astr.Length > i) { form_EditSourcePath.Text = value[i++]; } else { MBox.Assert(0, false); }
                if (astr.Length > i) { m_strListingFile = value[i++]; form_EditListingFile.Text = (IsVolumeNew ? m_strListingFile : System.IO.Path.GetFileName(m_strListingFile)); }
                if (astr.Length > i) { var s = value[i++]; if (s != null) { m_strStatus = s; } }
                if (astr.Length > i) { var s = value[i++]; if (s != null) { m_strIncludeYN = s; } }
                if (astr.Length > i) { form_UC_VolumeGroup.Text = value[i++]; }
                if (astr.Length > i) { form_EditDriveModel.Text = value[i++]; }
                if (astr.Length > i) { form_EditDriveSerial.Text = value[i++]; }
            }
        }

        string m_strListingFile = null;
        string m_strStatus = FileParse.ksNotSaved;
        string m_strIncludeYN = FileParse.ksInclude;

        bool IsOKenabled
        {
            get
            {
                return (SaveDirListings.IsGoodDriveSyntax(form_EditSourcePath.Text) &&
                    ((IsVolumeNew == false) || System.IO.Directory.Exists(form_EditSourcePath.Text)));
            }
        }

        LocalWindow window { get { return Window.GetWindow(uc_VolumeEdit) as LocalWindow; } }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = new UC_VolumeEditVM();

            uc_VolumeEdit.DataContext = vm;
            vm.IsOKenabled = () => { return IsOKenabled; };
            vm.SourcePath_CurrentText = () => { return form_EditSourcePath.Text; };
            vm.ListingFile_CurrentText = () => { return form_EditListingFile.Text; };
            vm.FromSourcePathDlg = s => { form_EditSourcePath.Text = s; };
            vm.FromProbe = t => { form_EditDriveModel.Text = t.DriveModel; form_EditDriveSerial.Text = t.DriveSerial; };
            vm.FromListingFileDlg = s => { form_EditListingFile.Text = s; };

            form_EditDriveLetter.CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, (o, e1) => { e1.Handled = true; }));
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            if (IsOKenabled)
            {
                window.DialogResult = true;
            }
            else
            {
                MBox.Assert(0, false);
            }
        }

        string CapDrive(string strPath)
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
            if (IsOKenabled)
            {
                form_EditSourcePath.Text = CapDrive(form_EditSourcePath.Text);
            }
        }

        private void form_EditListingFile_LostFocus(object sender, RoutedEventArgs e)
        {
            if (form_EditListingFile.Text.Length > 0)
            {
                try
                {
                    form_EditListingFile.Text = CapDrive(System.IO.Path.GetFullPath(form_EditListingFile.Text));
                }
                catch { }
            }
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
    }
}
