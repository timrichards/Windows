using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Windows.Navigation;
using System;
using System.IO;
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
                ? new UC_BackupVM { IsDeletedVolVM = true }
                : vm;

            _vm.UseNicknames = _bNicknames;
            _vm.LocalOwner = LocalOwner;
            _vm.Reset = () => Util.UIthread(99870, () => formEdit_DriveLetter.Text = null);
            _vm.Init();
            formEdit_DriveLetter.Text = null;

            _vm.Navigate = () =>
            {
                var top = NavigationHelper.FindFrame("_top", this);

                NavigationCommands.FirstPage.Execute(null, top);
      //          new BBCodeBlock().LinkNavigator.Navigate(new Uri("/DoubleFile;component/UC_Project/UC_Project.xaml", UriKind.Relative), this);
                Util.Block(250);
                NavigationCommands.GoToPage.Execute("/DoubleFile;component/UserControls/UC_Backup_DeletedVol.xaml", top);
      //          new BBCodeBlock().LinkNavigator.Navigate(new Uri("/DoubleFile;component/UserControls/UC_Backup_DeletedVol.xaml", UriKind.Relative), this);
            };
        }

        protected override void LocalNavigatedFrom()
        {
            _bNicknames = formChk_Nicknames.IsChecked ?? false;
            DataContext = null;

            // One-shot: no need to dispose
            Observable.Timer(TimeSpan.FromMinutes(1)).Timestamp()
                .LocalSubscribe(99568, x => Clear());
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
