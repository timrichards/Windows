using System;
using System.Reactive.Linq;
using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinSearch.xaml
    /// </summary>
    partial class UC_Search
    {
        public UC_Search()
        {
            InitializeComponent();
            CantDupeThisUsercontrol = true;     // only so _vmSave can be set, so there could be a workaround

            if (Statics.Namespace != GetType().Namespace)   // no project so no nicknames in the VolTreeMap assembly
                formChk_Nicknames.Visibility = Visibility.Collapsed;
        }

        protected override void LocalNavigatedTo()
        {
            var vm = _vmSave;

            _vmSave = null;

            if (null == vm)
            {
                var strText = formEdit_search.Text;

                DataContext =
                    _vm =
                    new UC_SearchVM
                {
                    IsEditBoxNonEmpty = () => false == string.IsNullOrWhiteSpace(formEdit_search.Text),
                    Regex = _bRegex,
                    UseNicknames = _bNicknames,
                    LocalOwner = LocalOwner
                }
                    .Init();

                _vm.SearchText = strText;      // set this after DataContext
            }
            else
            {
                 DataContext =
                    _vm =
                    vm
                    .Reconnect();
            }

            // Set search text box focus
            // One-shot: no need to dispose
            Observable.Timer(TimeSpan.FromMilliseconds(100)).Timestamp()
                .LocalSubscribe(99685, x => Util.UIthread(99791, () =>
            {
                formEdit_search.CaretIndex = int.MaxValue;
                formEdit_search.Focus();
            }));
        }

        protected override void LocalNavigatedFrom()
        {
            _bRegex = _vm.Regex;
            _bNicknames = formChk_Nicknames.IsChecked ?? false;
            DataContext = null;
            _vmSave = _vm;

            // One-shot: no need to dispose
            Observable.Timer(TimeSpan.FromSeconds(10)).Timestamp()
                .LocalSubscribe(99615, x => { _vmSave?.Dispose(); _vmSave = null; });
        }

        UC_SearchVM
            _vm = null;
        static UC_SearchVM
            _vmSave = null;
        bool
            _bRegex = false;
        bool
            _bNicknames = false;
    }
}
