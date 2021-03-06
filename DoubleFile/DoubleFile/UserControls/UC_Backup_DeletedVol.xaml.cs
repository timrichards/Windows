﻿using System;
using System.Reactive.Linq;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for UC_SolitaryHere.xaml
    /// </summary>
    public partial class UC_Backup_DeleteVol
    {
        public UC_Backup_DeleteVol()
        {
            InitializeComponent();
            CantDupeThisUsercontrol = true;

            LV_ProjectVM.Modified
                .LocalSubscribe(99567, x => Clear());
        }

        protected override void LocalNavigatedTo()
        {
            _clearTimer?.Dispose();
            _clearTimer = null;

            var vm = _vm;

            DataContext =
                _vm =
                (vm?.IsDisposed ?? true)
                ? new UC_BackupVM { IsDeletedVolVM = true }
                : vm;

            _vm.UseNicknames = _bNicknames;
            _vm.LocalOwner = LocalOwner;
            _vm.Reset = () => Util.UIthread(99870, () => formEdit_DriveLetter.Text = null);
            _vm.Init();
            formEdit_DriveLetter.Text = null;

            _previewKeyDown = Observable.FromEventPattern<KeyEventArgs>(formEdit_DriveLetter, "PreviewKeyDown")
                .LocalSubscribe(99581, args => UC_Backup.DriveLetterKeyDown(args.EventArgs, formEdit_DriveLetter, _vm,
                () =>  BeginStoryboard((Storyboard)form_RectDriveLetterError.FindResource("DriveLetterError"))));
        }

        protected override void LocalNavigatedFrom()
        {
            _bNicknames = formChk_Nicknames.IsChecked ?? false;
            DataContext = null;
            _vm?.With(vm => vm.Reset = null);
            _previewKeyDown?.Dispose();

            _clearTimer = Observable.Timer(TimeSpan.FromMinutes(1)).Timestamp()
                .LocalSubscribe(99568, x => Clear());
        }

        void Clear()
        {
            _vm?.Dispose();
            _vm = null;
        }

        static IDisposable
            _clearTimer = null;
        static UC_BackupVM
            _vm = null;
        IDisposable
            _previewKeyDown = null;
        bool
            _bNicknames = false;
    }
}
