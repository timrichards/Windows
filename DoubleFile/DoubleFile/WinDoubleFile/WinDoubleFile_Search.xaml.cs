using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows;
using System;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinDoubleFile_Duplicates.xaml
    /// </summary>
    partial class WinDoubleFile_Search
    {
        internal WinDoubleFile_Search()
        {
            InitializeComponent();
            
            var vm = new WinDoubleFile_SearchVM
            {
                IsEditBoxNonEmpty = () => false == string.IsNullOrWhiteSpace(form_searchText.Text)
            }.Init();

            DataContext = vm;

            Observable.FromEventPattern(this, "Closed")
                .Subscribe(args => vm.Dispose());
        }

        override protected Rect
            PosAtClose { get { return _rcPosAtClose; } set { _rcPosAtClose = value; } }
        static Rect
            _rcPosAtClose = Rect.Empty;
    }
}
