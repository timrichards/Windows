using System;
using System.Reactive.Linq;

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
        }

        protected override void LocalNavigatedTo()
        {
            DataContext =
                _winSearchVM =
                new WinSearchVM
            {
                IsEditBoxNonEmpty = () => false == string.IsNullOrWhiteSpace(formEdit_search.Text)
            }
                .Init();

            var strText = formEdit_search.Text;

            formEdit_search.Text = "";

            // Notify new vm of existing text in search box
            // One-shot: no need to dispose
            Observable.Timer(TimeSpan.FromMilliseconds(100)).Timestamp()
                .LocalSubscribe(99685, x => Util.UIthread(99791, () =>
            {
                formEdit_search.Text = strText;
                formEdit_search.CaretIndex = int.MaxValue;
                formEdit_search.Focus();
                formChk_Regex.IsChecked = _bRegex;
            }));
        }

        protected override void LocalNavigatedFrom()
        {
            _bRegex = formChk_Regex.IsChecked ?? false;
            _winSearchVM.Dispose();

            DataContext =
                _winSearchVM =
                null;
        }

        WinSearchVM
            _winSearchVM = null;
        bool
            _bRegex = false;
    }
}
