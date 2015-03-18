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

        internal new void Show()
        {
            if (false == LocalIsClosed)
            {
                MBoxStatic.Assert(99897, false, bTraceOnly: true);
                return;
            }

            base.Show();
            
            if (_nWantsLeft > -1)
            {
                Left = _nWantsLeft;
                Top = _nWantsTop;
            }
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            _host.Dispose();
            form_ucTreeMap.Dispose();
            _nWantsLeft = Left;
            _nWantsTop = Top;
        }

        static double _nWantsLeft = -1;
        static double _nWantsTop = -1;
    }
}
