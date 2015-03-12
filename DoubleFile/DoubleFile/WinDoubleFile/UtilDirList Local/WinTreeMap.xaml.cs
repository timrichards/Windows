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
            Activated += (o, e) => form_ucTreeMap.ClearSelection();
            Deactivated += (o, e) => form_ucTreeMap.ClearSelection();
            SizeChanged += (o, e) => form_ucTreeMap.ClearSelection();
            LocationChanged += (o, e) => form_ucTreeMap.ClearSelection();
            form_ucTreeMap.TooltipAnchor = this;
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
