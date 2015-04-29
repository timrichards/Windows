using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using System.IO;
using System.Reactive.Linq;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for UC_VolumeEdit.xaml
    /// </summary>
    partial class UC_VolumeEdit
    {
        internal char DriveLetter { get { return (form_EditDriveLetter.Text + "\0")[0]; } }

        [Description("User control supports choice"), Category("New or Edit Volume window")]
        internal bool IsVolumeNew
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

            Observable.FromEventPattern(form_grid, "Loaded")
                .Subscribe(args => Grid_Loaded());

            Observable.FromEventPattern(form_EditSourcePath, "LostFocus")
                .Subscribe(args => { if (IsValidSourcePathEdit) form_EditSourcePath.Text = CapDrive(form_EditSourcePath.Text); });

            Observable.FromEventPattern<KeyEventArgs>(form_EditDriveLetter, "PreviewKeyDown")
                .Subscribe(args => form_EditDriveLetter_PreviewKeyDown(args.EventArgs));

            Observable.FromEventPattern(form_EditListingFile, "LostFocus")
                .Subscribe(args => form_EditListingFile_LostFocus());

            Observable.FromEventPattern(form_btnOK, "Click")
                .Subscribe(args => BtnOK_Click());

            Observable.FromEventPattern(form_btnCancel, "Click")
                .Subscribe(args => BtnCancel_Click());
        }

        internal LVitem_ProjectVM LVitemVolumeTemp
        {
            get
            {
                return new LVitem_ProjectVM(new string[] {
                    form_EditNickname.Text, form_EditSourcePath.Text,
                    (IsVolumeNew ? form_EditListingFile.Text : _strListingFile),
                    _strStatus, _strIncludeYN,
                    form_ucVolumeGroup.Text, form_EditDriveModel.Text, form_EditDriveSerial.Text
                });
            }
            set
            {
                if (null == value)
                    return;

                var astr = value.StringValues;
                var i = 0;

                if (astr.Length > i) { form_EditNickname.Text = value[i++]; } else { MBoxStatic.Assert(99926, false); }
                if (astr.Length > i) { form_EditSourcePath.Text = value[i++]; } else { MBoxStatic.Assert(99925, false); }
                if (astr.Length > i) { _strListingFile = value[i++]; form_EditListingFile.Text = (IsVolumeNew ? _strListingFile : Path.GetFileName(_strListingFile)); }
                if (astr.Length > i) { var s = value[i++]; if (s != null) { _strStatus = s; } }
                if (astr.Length > i) { var s = value[i++]; if (s != null) { _strIncludeYN = s; } }
                if (astr.Length > i) { form_ucVolumeGroup.Text = value[i++]; }
                if (astr.Length > i) { form_EditDriveModel.Text = value[i++]; }
                if (astr.Length > i) { form_EditDriveSerial.Text = value[i++]; }
            }
        }

        bool IsValidListingEdit
        {
            get
            {
                try { return (Path.GetFileName(Path.GetFullPath(form_EditListingFile.Text)).Length > 0); }
                catch (ArgumentException) { return false; }
            }
        }

        bool IsValidSourcePathEdit
        {
            get
            {
                return (SaveDirListings.IsGoodDriveSyntax(form_EditSourcePath.Text) &&
                    ((false == IsVolumeNew) || Directory.Exists(form_EditSourcePath.Text)));
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

        static string CapDrive(string strPath)
        {
            var a = strPath.ToCharArray();

            if (a.Length > 0)
            {
                a[0] = ("" + a[0]).ToUpper()[0];
                return string.Join("", a);
            }

            return strPath;
        }

        #region form_handlers
        private void Grid_Loaded()
        {
            uc_VolumeEdit.DataContext = new UC_VolumeEditVM
            {
                IsOKenabled = () => IsOKenabled,
                SourcePath_CurrentText = () => form_EditSourcePath.Text,
                ListingFile_CurrentText = () => form_EditListingFile.Text,
                FromSourcePathDlg = s => form_EditSourcePath.Text = s,
                FromProbe = (strDriveModel, strDriveSerial) => { form_EditDriveModel.Text = strDriveModel; form_EditDriveSerial.Text = strDriveSerial; },
                FromListingFileDlg = s => form_EditListingFile.Text = s
            }.Init();

            form_EditDriveLetter.CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, (o, e1) => { e1.Handled = true; }));

            var textBox = (IsVolumeNew ? form_EditSourcePath : form_EditDriveLetter);

            textBox.Focus();
            textBox.CaretIndex = int.MaxValue;
        }

        private void form_EditDriveLetter_PreviewKeyDown(KeyEventArgs e)
        {
            if (new Key[] { Key.Tab, Key.Back, Key.Delete, Key.Left, Key.Right }.Contains(e.Key))
                return;

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

        private void form_EditListingFile_LostFocus()
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

        private void BtnOK_Click()
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

        private void BtnCancel_Click()
        {
            var window = Window.GetWindow(uc_VolumeEdit) as LocalWindow;

            if (window != null)
                window.CloseIfSimulatingModal();
        }
        #endregion form_handlers

        string _strListingFile = null;
        string _strStatus = FileParse.ksNotSaved;
        string _strIncludeYN = FileParse.ksInclude;
    }
}
