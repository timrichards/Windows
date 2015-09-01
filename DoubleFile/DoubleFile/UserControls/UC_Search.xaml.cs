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

            if (Statics.Namespace != GetType().Namespace)   // no project so no nicknames in the VolTreeMap assembly
                formChk_Nicknames.Visibility = Visibility.Collapsed;
        }

        protected override void LocalNavigatedTo()
        {
            var strText = formEdit_search.Text;

            DataContext =
                _vm =
                new UC_SearchVM
            {
                IsEditBoxNonEmpty = () => false == string.IsNullOrWhiteSpace(formEdit_search.Text),
                Regex = _bRegex,
                UseNicknames = _bNicknames
            }
                .Init();

            _vm.SearchText = strText;      // set this after DataContext

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
            _bRegex = formChk_Regex.IsChecked ?? false;
            _bNicknames = formChk_Nicknames.IsChecked ?? false;
            _vm.Dispose();

            DataContext =
                _vm =
                null;
        }

        UC_SearchVM
            _vm = null;
        bool
            _bRegex = false;
        bool
            _bNicknames = false;
    }
}
