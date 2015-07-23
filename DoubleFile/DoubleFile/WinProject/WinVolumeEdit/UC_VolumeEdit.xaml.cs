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

            Observable.FromEventPattern(this, "Loaded")
                .Subscribe(x => LoadHandler());

            Observable.FromEventPattern(formEdit_SourcePath, "LostFocus")
                .Subscribe(x => { if (IsValidSourcePathEdit) formEdit_SourcePath.Text = CapDrive(formEdit_SourcePath.Text); });

            Observable.FromEventPattern<KeyEventArgs>(formEdit_DriveLetter, "PreviewKeyDown")
                .Subscribe(args => formEdit_DriveLetter_PreviewKeyDown(args.EventArgs));

            Observable.FromEventPattern(formEdit_SaveListingFile, "LostFocus")
                .Subscribe(x => formEdit_ListingFile_LostFocus());

            Observable.FromEventPattern(formBtn_OK, "Click")
                .Subscribe(x => BtnOK_Click());

            Observable.FromEventPattern(formBtn_Cancel, "Click")
                .Subscribe(x => BtnCancel_Click());
        }

        internal LVitem_ProjectVM LVitemVolumeTemp
        {
            get
            {
                _lvItemTemp.Nickname = formEdit_Nickname.Text;
                _lvItemTemp.SourcePath = formEdit_SourcePath.Text;

                if (IsVolumeNew)
                    _lvItemTemp.ListingFile = formEdit_SaveListingFile.Text;

                _lvItemTemp.VolumeGroup = formUC_VolumeGroup.Text;
                _lvItemTemp.DriveModel = formEdit_DriveModel.Text;
                _lvItemTemp.DriveSerial = formEdit_DriveSerial.Text;
                // Status; IncludeYN; ScannedLength; (etc.) are preserved
                return _lvItemTemp;
            }
            set
            {
                _lvItemTemp = new LVitem_ProjectVM(value);

                if (null == value)
                    return;
                
                formEdit_Nickname.Text = value.Nickname;
                formEdit_SourcePath.Text = value.SourcePath;
                formEdit_SaveListingFile.Text = IsVolumeNew ? value.ListingFile : Path.GetFileName(value.ListingFile);
                formUC_VolumeGroup.Text = value.VolumeGroup;
                formEdit_DriveModel.Text = value.DriveModel;
                formEdit_DriveSerial.Text = value.DriveSerial;
            }
        }
        LVitem_ProjectVM _lvItemTemp = null;

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

        void LoadHandler()
        {
            uc_VolumeEdit.DataContext =
                new UC_VolumeEditVM
            {
                IsOKenabled = () => IsOKenabled,
                SourcePath_CurrentText = () => formEdit_SourcePath.Text,
                ListingFile_CurrentText = () => formEdit_SaveListingFile.Text,
                DriveModel_CurrentText = () => formEdit_DriveModel.Text,
                DriveSerial_CurrentText = () => formEdit_DriveSerial.Text,
                FromSourcePathDlg = s => formEdit_SourcePath.Text = s,
                FromProbe = (strDriveModel, strDriveSerial) => { formEdit_DriveModel.Text = strDriveModel; formEdit_DriveSerial.Text = strDriveSerial; },
                FromListingFileDlg = s => formEdit_SaveListingFile.Text = s
            }
                .Init();

            formEdit_DriveLetter.CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, (o, e) => e.Handled = true));

            var textBox = (IsVolumeNew ? formEdit_SourcePath : formEdit_DriveLetter);

            textBox.Focus();
            textBox.CaretIndex = int.MaxValue;
        }

        void formEdit_DriveLetter_PreviewKeyDown(KeyEventArgs e)
        {
            if (new[] { Key.Tab, Key.Back, Key.Delete, Key.Left, Key.Right }.Contains(e.Key))
                return;

            if (0 < formEdit_DriveLetter.Text.Length)
            {
                e.Handled = true;
                return;
            }
            
            if ((Key.A > e.Key) ||
                (Key.Z < e.Key))
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

        void formEdit_ListingFile_LostFocus()
        {
            if (string.IsNullOrWhiteSpace(formEdit_SaveListingFile.Text) ||
                (false == IsValidListingEdit))
            {
                return;
            }

            var strListingFile = CapDrive(Path.GetFullPath(formEdit_SaveListingFile.Text));
            var strExt = "" + Path.GetExtension(strListingFile);

            if ((0 == strExt.Length) ||
                (false ==
                strExt
                    .Remove(0, 1)
                    .Equals(FileParse.ksFileExt_Listing, StringComparison.InvariantCultureIgnoreCase)))
            {
                strListingFile += "." + FileParse.ksFileExt_Listing;
            }

            formEdit_SaveListingFile.Text = strListingFile;
        }

        void BtnOK_Click()
        {
            if (IsOKenabled)
            {
                var window = Window.GetWindow(uc_VolumeEdit).As<LocalModernWindowBase>();

                if (null != window)
                    window.LocalDialogResult = true;

                window?.CloseIfSimulatingModal();
            }
            else
            {
                MBoxStatic.Assert(99924, false);
            }
        }

        void BtnCancel_Click()
        {
            Window
                .GetWindow(uc_VolumeEdit)
                .As<LocalModernWindowBase>()?
                .CloseIfSimulatingModal();
        }
    }
}
