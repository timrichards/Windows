using System;
using System.Reactive.Linq;
using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinSearch.xaml
    /// </summary>
    partial class UC_Sinks
    {
        public UC_Sinks()
        {
            InitializeComponent();
            CantDupeThisUsercontrol = true;     // only so _vmSave can be set, so there could be a workaround

            if (Statics.Namespace != GetType().Namespace)   // no project so no nicknames in the VolTreemap assembly
                formChk_Nicknames.Visibility = Visibility.Collapsed;
        }

        protected override void LocalNavigatedTo()
        {
            var vm = _vmSave;

            _vmSave = null;

            if (vm?.IsDisposed ?? true)
            {
                DataContext =
                    _vm =
                    new UC_SinksVM
                {
                    UseNicknames = _bNicknames,
                }
                    .Init();
            }
            else
            {
                DataContext =
                    _vm =
                    vm
                    .Reconnect();
            }

            _vm.LocalOwner = LocalOwner;
        }

        protected override void LocalNavigatedFrom()
        {
            _bNicknames = formChk_Nicknames.IsChecked ?? false;
            DataContext = null;
            _vmSave = _vm;

            // One-shot: no need to dispose
            Observable.Timer(TimeSpan.FromSeconds(10)).Timestamp()
                .LocalSubscribe(99566, x => Clear());

            LV_ProjectVM.Modified
                .LocalSubscribe(99565, x => Clear());
        }

        void Clear()
        {
            _vmSave?.Dispose();
            _vmSave = null;
        }

        UC_SinksVM
            _vm = null;
        static UC_SinksVM
            _vmSave = null;
        bool
            _bNicknames = false;
    }
}
