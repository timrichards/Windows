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
        internal char DriveLetter { get { return (formEdit_DriveLetter.Text + "\0")[0]; } }

        [Description("User control supports choice"), Category("New or Edit Volume window")]
        internal bool IsVolumeNew
        {
            set
            {
                formEdit_SourcePath.IsEnabled = value;
                formBtn_SourcePath.IsEnabled = value;
                form_Border.VerticalAlignment = value ? VerticalAlignment.Top : VerticalAlignment.Center;
                formLabel_DriveLetter.Visibility = value ? Visibility.Hidden : Visibility.Visible;
                formEdit_DriveLetter.Visibility = value ? Visibility.Hidden : Visibility.Visible;
                formBtn_DriveModel.IsEnabled = value;
                formBtn_SaveListingFile.IsEnabled = value;
                formEdit_SaveListingFile.IsEnabled = value;
            }
            get
            {
                return formEdit_SourcePath.IsEnabled;
            }
        }

        public UC_VolumeEdit()
        {
            InitializeComponent();

            Observable.FromEventPattern(form_grid, "Loaded")
                .Subscribe(args => Grid_Loaded());

            Observable.FromEventPattern(formEdit_SourcePath, "LostFocus")
                .Subscribe(args => { if (IsValidSourcePathEdit) formEdit_SourcePath.Text = CapDrive(formEdit_SourcePath.Text); });

            Observable.FromEventPattern<KeyEventArgs>(formEdit_DriveLetter, "PreviewKeyDown")
                .Subscribe(args => formEdit_DriveLetter_PreviewKeyDown(args.EventArgs));

            Observable.FromEventPattern(formEdit_SaveListingFile, "LostFocus")
                .Subscribe(args => formEdit_ListingFile_LostFocus());

            Observable.FromEventPattern(formBtn_OK, "Click")
                .Subscribe(args => BtnOK_Click());

            Observable.FromEventPattern(formBtn_Cancel, "Click")
                .Subscribe(args => BtnCancel_Click());
        }

        internal LVitem_ProjectVM LVitemVolumeTemp
        {
            get
            {
                return new LVitem_ProjectVM(new[] {
                    formEdit_Nickname.Text, formEdit_SourcePath.Text,
                    (IsVolumeNew ? formEdit_SaveListingFile.Text : _strListingFile),
                    _strStatus, _strIncludeYN,
                    form_ucVolumeGroup.Text, formEdit_DriveModel.Text, formEdit_DriveSerial.Text
                });
            }
            set
            {
                if (null == value)
                    return;

                var astr = value.StringValues;
                var i = 0;

                if (i < astr.Length) formEdit_Nickname.Text = value[i++]; else MBoxStatic.Assert(99926, false);
                if (i < astr.Length) formEdit_SourcePath.Text = value[i++]; else MBoxStatic.Assert(99925, false);
                if (i < astr.Length) { _strListingFile = value[i++]; formEdit_SaveListingFile.Text = (IsVolumeNew ? _strListingFile : Path.GetFileName(_strListingFile)); }
                if (i < astr.Length) { var s = value[i++]; if (null != s) _strStatus = s; }
                if (i < astr.Length) { var s = value[i++]; if (null != s) _strIncludeYN = s; }
                if (i < astr.Length) form_ucVolumeGroup.Text = value[i++];
                if (i < astr.Length) formEdit_DriveModel.Text = value[i++];
                if (i < astr.Length) formEdit_DriveSerial.Text = value[i++];
            }
        }

        bool IsValidListingEdit
        {
            get
            {
                try { return 0 < Path.GetFileName(Path.GetFullPath(formEdit_SaveListingFile.Text)).Length; }
                catch (ArgumentException) { return false; }
            }
        }

        bool IsValidSourcePathEdit
        {
            get
            {
                return (SaveDirListings.IsGoodDriveSyntax(formEdit_SourcePath.Text) &&
                    ((false == IsVolumeNew) || Directory.Exists(formEdit_SourcePath.Text)));
            }
        }

        bool IsOKenabled
        {
            get
            {
                return ((string.IsNullOrWhiteSpace(formEdit_SaveListingFile.Text) || IsValidListingEdit) &&
                    IsValidSourcePathEdit);
            }
        }

        static string CapDrive(string strPath)
        {
            var a = strPath.ToCharArray();

            if (0 < a.Length)
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
                SourcePath_CurrentText = () => formEdit_SourcePath.Text,
                ListingFile_CurrentText = () => formEdit_SaveListingFile.Text,
                FromSourcePathDlg = s => formEdit_SourcePath.Text = s,
                FromProbe = (strDriveModel, strDriveSerial) => { formEdit_DriveModel.Text = strDriveModel; formEdit_DriveSerial.Text = strDriveSerial; },
                FromListingFileDlg = s => formEdit_SaveListingFile.Text = s
            }.Init();

            formEdit_DriveLetter.CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, (o, e) => e.Handled = true));

            var textBox = (IsVolumeNew ? formEdit_SourcePath : formEdit_DriveLetter);

            textBox.Focus();
            textBox.CaretIndex = int.MaxValue;
        }

        private void formEdit_DriveLetter_PreviewKeyDown(KeyEventArgs e)
        {
            if (new[] { Key.Tab, Key.Back, Key.Delete, Key.Left, Key.Right }.Contains(e.Key))
                return;

            if (0 < formEdit_DriveLetter.Text.Length)
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
                formEdit_SourcePath.Text[0])
            {
                e.Handled = true;
                return;
            }
        }

        private void formEdit_ListingFile_LostFocus()
        {
            if (string.IsNullOrWhiteSpace(formEdit_SaveListingFile.Text) ||
                (false == IsValidListingEdit))
            {
                return;
            }

            var strListingFile = CapDrive(Path.GetFullPath(formEdit_SaveListingFile.Text));
            var strExt = "" + Path.GetExtension(strListingFile);

            if ((strExt.Length == 0) || (false ==
                strExt.Remove(0, 1)
                .Equals(FileParse.ksFileExt_Listing,
                StringComparison.InvariantCultureIgnoreCase)))
            {
                strListingFile += "." + FileParse.ksFileExt_Listing;
            }

            formEdit_SaveListingFile.Text = strListingFile;
        }

        private void BtnOK_Click()
        {
            if (IsOKenabled)
            {
                var window = Window.GetWindow(uc_VolumeEdit) as LocalWindowBase;

                if (null != window)
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
            var window = Window.GetWindow(uc_VolumeEdit) as LocalWindowBase;

            if (null != window)
                window.CloseIfSimulatingModal();
        }
        #endregion form_handlers

        string _strListingFile = null;
        string _strStatus = FileParse.ksNotSaved;
        string _strIncludeYN = FileParse.ksInclude;
    }
}
