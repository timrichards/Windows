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
        }

        protected override void LocalNavigatedTo()
        {
            var vm = _vmSave;

            _vmSave = null;

            DataContext =
                _vm =
                (vm?.IsDisposed ?? true)
                ? new UC_CompareVM()
                : vm;

            _vm.Connect();
        }

        protected override void LocalNavigatedFrom()
        {
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

        UC_CompareVM
            _vm = null;
        static UC_CompareVM
            _vmSave = null;
    }
}
