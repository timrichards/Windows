using System;
using System.IO;
using System.Reactive.Linq;
using System.Windows.Input;

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

            Observable.FromEventPattern<KeyEventArgs>(formEdit_DriveLetter, "PreviewKeyDown")
                .LocalSubscribe(99580, args =>
            {
                args.EventArgs.Handled = true;

                var strChar = (new KeyConverter().ConvertToString(args.EventArgs.Key) + "\0")[0] + @":\";

                if (false == Directory.Exists(strChar))
                {
                    formEdit_DriveLetter.Text = "";
                    return;
                }

                if (false == _vm.CheckDriveLetter(strChar[0]))
                    return;

                UC_VolumeEdit.DriveLetterPreviewKeyDown(args.EventArgs);
            });
        }

        protected override void LocalNavigatedTo()
        {
            var vm = _vmSave;

            _vmSave = null;

            DataContext =
                _vm =
                (vm?.IsDisposed ?? true)
                ? new UC_BackupVM()
                : vm;

            _vm.UseNicknames = _bNicknames;
            _vm.LocalOwner = LocalOwner;
            _vm.Init();
        }

        protected override void LocalNavigatedFrom()
        {
            _bNicknames = formChk_Nicknames.IsChecked ?? false;
            DataContext = null;
            _vmSave = _vm;

            // One-shot: no need to dispose
            Observable.Timer(TimeSpan.FromMinutes(1)).Timestamp()
                .LocalSubscribe(99615, x => Clear());

            LV_ProjectVM.Modified
                .LocalSubscribe(99612, x => Clear());
        }

        void Clear()
        {
            _vmSave?.Dispose();
            _vmSave = null;
        }

        UC_BackupVM
            _vm = null;
        static UC_BackupVM
            _vmSave = null;
        bool
            _bNicknames = false;
    }
}
