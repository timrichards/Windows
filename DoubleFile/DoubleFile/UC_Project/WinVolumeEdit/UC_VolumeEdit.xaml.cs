﻿using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using System.IO;
using System.Reactive.Linq;
using System.Windows.Controls;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for UC_VolumeEdit.xaml
    /// </summary>
    partial class UC_VolumeEdit
    {
        internal char DriveLetter => (formEdit_DriveLetter.Text + "\0")[0];

        static internal void
            DriveLetterPreviewKeyDown(KeyEventArgs e)
        {
            if (new[] { Key.Tab, Key.Back, Key.Delete, Key.Left, Key.Right }.Contains(e.Key))
                return;

            e.Handled = true;

            if ((Key.A > e.Key) || (Key.Z < e.Key))
                return;

            var strChar = "" + (new KeyConverter().ConvertToString(e.Key) + "\0")[0];
            var textBox = (TextBox)e.Source;
            var selStart = textBox.SelectionStart;
            var selLength = textBox.SelectionLength;

            textBox.Text = strChar;
            textBox.SelectionStart = selStart;
            textBox.SelectionLength = selLength;
        }

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
                .LocalSubscribe(99719, x => LoadHandler());

            Observable.FromEventPattern(formEdit_SourcePath, "LostFocus")
                .LocalSubscribe(99718, x => { if (IsValidSourcePathEdit) formEdit_SourcePath.Text = CapDrive(formEdit_SourcePath.Text); });

            Observable.FromEventPattern<KeyEventArgs>(formEdit_DriveLetter, "PreviewKeyDown")
                .LocalSubscribe(99717, args => DriveLetterPreviewKeyDown(args.EventArgs));

            Observable.FromEventPattern(formEdit_SaveListingFile, "LostFocus")
                .LocalSubscribe(99716, x => formEdit_ListingFile_LostFocus());

            Observable.FromEventPattern(formBtn_OK, "Click")
                .LocalSubscribe(99715, x => BtnOK_Click());

            Observable.FromEventPattern(formBtn_Cancel, "Click")
                .LocalSubscribe(99714, x => BtnCancel_Click());
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
                _lvItemTemp = new LVitem_ProjectVM(value, accept_invalid_chars: true);

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

        bool IsValidSourcePathEdit =>
            (SaveDirListings.IsGoodDriveSyntax(formEdit_SourcePath.Text) &&
            ((false == IsVolumeNew) || Directory.Exists(formEdit_SourcePath.Text)));

        bool IsOKenabled =>
            ((string.IsNullOrWhiteSpace(formEdit_SaveListingFile.Text) || IsValidListingEdit) &&
            IsValidSourcePathEdit);

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
                    .Equals(FileParse.ksFileExt_Listing, StringComparison.OrdinalIgnoreCase)))
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
                Util.Assert(99924, false);
            }
        }

        void BtnCancel_Click() =>
            Window
            .GetWindow(uc_VolumeEdit)
            .As<LocalModernWindowBase>()?
            .CloseIfSimulatingModal();
    }
}
