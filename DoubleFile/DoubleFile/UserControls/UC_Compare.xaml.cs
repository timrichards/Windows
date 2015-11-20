using System;
using System.Reactive.Linq;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for UC_SolitaryHere.xaml
    /// </summary>
    public partial class UC_Compare
    {
        public UC_Compare()
        {
            InitializeComponent();
            CantDupeThisUsercontrol = true;

            LV_ProjectVM.Modified
                .LocalSubscribe(99612, x => Clear());
        }

        protected override void LocalNavigatedTo()
        {
            var vm = _vm;

            DataContext =
                _vm =
                (vm?.IsDisposed ?? true)
                ? new UC_CompareVM()
                : vm;

            _vm.UseNicknames = _bNicknames;
        }

        protected override void LocalNavigatedFrom()
        {
            _bNicknames = formChk_Nicknames.IsChecked ?? false;
            DataContext = null;

            // One-shot: no need to dispose
            Observable.Timer(TimeSpan.FromMinutes(1)).Timestamp()
                .LocalSubscribe(99615, x => Clear());
        }

        void Clear()
        {
            _vm?.Dispose();
            _vm = null;
        }

        static UC_CompareVM
            _vm = null;
        bool
            _bNicknames = false;
    }
}
