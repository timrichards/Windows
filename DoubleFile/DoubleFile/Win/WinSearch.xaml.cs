using System;
using System.Reactive.Linq;
using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinSearch.xaml
    /// </summary>
    partial class WinSearch
    {
        public WinSearch()
        {
            InitializeComponent();

            if (Statics.Namespace != GetType().Namespace)   // no project so no nicknames in the VolTreeMap assembly
                formChk_Nicknames.Visibility = Visibility.Collapsed;
        }

        protected override void LocalNavigatedTo()
        {
            var strText = formEdit_search.Text;

            DataContext =
                _winSearchVM =
                new WinSearchVM
            {
                IsEditBoxNonEmpty = () => false == string.IsNullOrWhiteSpace(formEdit_search.Text),
                Regex = _bRegex,
                UseNicknames = _bNicknames
            }
                .Init();

            _winSearchVM.SearchText = strText;      // set this after DataContext

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
            _winSearchVM.Dispose();

            DataContext =
                _winSearchVM =
                null;
        }

        WinSearchVM
            _winSearchVM = null;
        bool
            _bRegex = false;
        bool
            _bNicknames = false;
    }
}
