using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows;
using System;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinDoubleFile_Duplicates.xaml
    /// </summary>
    partial class WinDoubleFile_Duplicates
    {
        internal WinDoubleFile_Duplicates()
        {
            InitializeComponent();

            Observable.FromEventPattern(form_grid, "Loaded")
                .Subscribe(args => DataContext = _winDoubleFile_DuplicatesVM = new WinDoubleFile_DuplicatesVM());

            Observable.FromEventPattern(this, "ContentRendered")
                .Subscribe(args => { if (Rect.Empty == _rcPosAtClose) Top += 50; });

            Observable.FromEventPattern(this, "Closed")
                .Subscribe(args => _winDoubleFile_DuplicatesVM.Dispose());
        }

        protected override LocalWindow_DoubleFile CreateChainedWindow()
        {
            return new WinDoubleFile_Detail();
        }

        WinDoubleFile_DuplicatesVM
            _winDoubleFile_DuplicatesVM = null;

        override protected Rect
            PosAtClose { get { return _rcPosAtClose; } set { _rcPosAtClose = value; } }
        static Rect
            _rcPosAtClose = Rect.Empty;
    }
}
