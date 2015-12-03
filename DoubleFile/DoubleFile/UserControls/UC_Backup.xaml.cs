using System;
using System.IO;
using System.Reactive.Linq;
using System.Windows.Controls;
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
                .LocalSubscribe(99581, args => DriveLetterKeyDown(args.EventArgs, formEdit_DriveLetter,
                () =>  BeginStoryboard((Storyboard)form_RectDriveLetterError.FindResource("DriveLetterError"))));
        }

        protected override void LocalNavigatedTo()
        {
            _clearTimer?.Dispose();
            _clearTimer = null;

            var vm = _vm;

            DataContext =
                _vm =
                (vm?.IsDisposed ?? true)
                ? new UC_BackupVM()
                : vm;

            _vm.UseNicknames = _bNicknames;
            _vm.LocalOwner = LocalOwner;
            _vm.Reset = () => Util.UIthread(99886, () => formEdit_DriveLetter.Text = null);
            _vm.Init();
            formEdit_DriveLetter.Text = null;
        }

        protected override void LocalNavigatedFrom()
        {
            _bNicknames = formChk_Nicknames.IsChecked ?? false;
            DataContext = null;
            _vm?.With(vm => vm.Reset = null);

            _clearTimer = Observable.Timer(TimeSpan.FromMinutes(1)).Timestamp()
                .LocalSubscribe(99570, x => Clear());
        }

        void Clear()
        {
            _vm?.Dispose();
            _vm = null;
        }

        static internal void
            DriveLetterKeyDown(KeyEventArgs args, TextBox textBox, Action reportError)
        {
            if (Key.Tab == args.Key)
                return;

            args.Handled = true;
            textBox.Text = null;

            var strChar = (new KeyConverter().ConvertToString(args.Key) + "\0")[0] + @":\";

            Util.Closure(() =>
            {
                if (false == Directory.Exists(strChar))
                    return;     // from lambda

                if (false == _vm.CheckDriveLetter(strChar[0]))
                    return;     // from lambda

                UC_VolumeEdit.DriveLetterPreviewKeyDown(args);
            });

            if (string.IsNullOrEmpty(textBox.Text))
                reportError();

            // one way to source binding isn't disabling/enabling the Back up button in concert
            _vm.DriveLetter = textBox.Text;
            Util.UIthread(99579, CommandManager.InvalidateRequerySuggested);
        }

        static IDisposable
            _clearTimer = null;
        static UC_BackupVM
            _vm = null;
        bool
            _bNicknames = false;
    }
}
