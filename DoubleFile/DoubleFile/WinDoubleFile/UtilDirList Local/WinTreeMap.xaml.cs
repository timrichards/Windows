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
                .Subscribe(args => form_ucTreeMap.ClearSelection());

            Observable.FromEventPattern(this, "LocationChanged")
                .Subscribe(args => form_ucTreeMap.ClearSelection());

            Observable.FromEventPattern(form_slider, "LostMouseCapture")
                .Subscribe(args => form_ucTreeMap.TreeMapVM.LostMouseCapture());

            Observable.FromEventPattern(this, "Closed")
                .Subscribe(args => Window_Closed());

            ResizeMode = ResizeMode.CanResize;
            form_ucTreeMap.LocalOwner = this;
            base.DataContext = form_ucTreeMap.TreeMapVM = new WinTreeMapVM();
        }

        protected override LocalWindow_DoubleFile CreateChainedWindow()
        {
            return new WinDoubleFile_TreeList();
        }

        private void Window_Closed()
        {
            _host.Dispose();
            form_ucTreeMap.Dispose();
        }

        override protected Rect
            PosAtClose { get { return _rcPosAtClose; } set { _rcPosAtClose = value; } }
        static Rect
            _rcPosAtClose = Rect.Empty;
    }
}
