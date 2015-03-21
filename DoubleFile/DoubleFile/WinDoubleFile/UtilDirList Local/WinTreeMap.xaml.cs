using System.Windows;

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
            Closed += Window_Closed;
            SizeChanged += (o, e) => form_ucTreeMap.ClearSelection();
            LocationChanged += (o, e) => form_ucTreeMap.ClearSelection();
            ResizeMode = ResizeMode.CanResize;
            form_ucTreeMap.LocalOwner = this;
            base.DataContext = form_ucTreeMap.TreeMapVM = new WinTreeMapVM();
        }

        protected override LocalWindow_DoubleFile CreateChainedWindow()
        {
            return new WinDoubleFile_TreeList();
        }

        private void Window_Closed(object sender, System.EventArgs e)
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
