using System.Reactive.Linq;
using System.Windows;
using System;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinFormDirList.xaml
    /// </summary>
    public partial class WinTreeMap
    {
        internal WinTreeMap()
        {
            InitializeComponent();

            Observable.FromEventPattern(this, "SizeChanged")
                .Subscribe(args => formUC_TreeMap.ClearSelection());

            Observable.FromEventPattern(this, "LocationChanged")
                .Subscribe(args => formUC_TreeMap.ClearSelection());

            Observable.FromEventPattern(form_slider, "LostMouseCapture")
                .Subscribe(args => formUC_TreeMap.TreeMapVM.LostMouseCapture());

            Observable.FromEventPattern(this, "Closed")
                .Subscribe(args => Window_Closed());

            ResizeMode = ResizeMode.CanResize;
            formUC_TreeMap.LocalOwner = this;
            base.DataContext = formUC_TreeMap.TreeMapVM = new WinTreeMapVM();
        }

        protected override LocalWindow_DoubleFile CreateChainedWindow()
        {
            return new WinDoubleFile_TreeList();
        }

        private void Window_Closed()
        {
            _host.Dispose();
            formUC_TreeMap.Dispose();
        }

        override protected Rect
            PosAtClose { get { return _rcPosAtClose; } set { _rcPosAtClose = value; } }
        static Rect
            _rcPosAtClose = Rect.Empty;
    }
}
