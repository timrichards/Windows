﻿using System;
using System.IO;
using System.Reactive.Linq;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for UC_SolitaryHere.xaml
    /// </summary>
    public partial class UC_Backup
    {
        public UC_Backup()
        {
            InitializeComponent();
            CantDupeThisUsercontrol = true;

            LV_ProjectVM.Modified
                .LocalSubscribe(99569, x => Clear());

            Observable.FromEventPattern<KeyEventArgs>(formEdit_DriveLetter, "PreviewKeyDown")
                .LocalSubscribe(99580, args =>
            {
                if (Key.Tab == args.EventArgs.Key)
                    return;

                args.EventArgs.Handled = true;
                formEdit_DriveLetter.Text = null;

                var strChar = (new KeyConverter().ConvertToString(args.EventArgs.Key) + "\0")[0] + @":\";

                Util.Closure(() =>
                {
                    if (false == Directory.Exists(strChar))
                        return;     // from lambda

                    if (false == _vm.CheckDriveLetter(strChar[0]))
                        return;     // from lambda

                    UC_VolumeEdit.DriveLetterPreviewKeyDown(args.EventArgs);
                });

                if (string.IsNullOrEmpty(formEdit_DriveLetter.Text))
                    BeginStoryboard((Storyboard)form_RectDriveLetterError.FindResource("DriveLetterError"));

                // one way to source binding isn't disabling/enabling the Back up button in concert
                _vm.DriveLetter = formEdit_DriveLetter.Text;
                Util.UIthread(99579, () => CommandManager.InvalidateRequerySuggested());
            });
        }

        protected override void LocalNavigatedTo()
        {
            var vm = _vm;

            DataContext =
                _vm =
                (vm?.IsDisposed ?? true)
                ? new UC_BackupVM()
                : vm;

            _vm.UseNicknames = _bNicknames;
            _vm.LocalOwner = LocalOwner;
            _vm.Reset = () => Util.UIthread(99870, () => formEdit_DriveLetter.Text = null);
            _vm.Init();
            formEdit_DriveLetter.Text = null;
        }

        protected override void LocalNavigatedFrom()
        {
            _bNicknames = formChk_Nicknames.IsChecked ?? false;
            DataContext = null;

            // One-shot: no need to dispose
            Observable.Timer(TimeSpan.FromMinutes(1)).Timestamp()
                .LocalSubscribe(99570, x => Clear());
        }

        void Clear()
        {
            _vm?.Dispose();
            _vm = null;
        }

        static UC_BackupVM
            _vm = null;
        bool
            _bNicknames = false;
    }
}
