namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinFormAnalysis_DirList.xaml
    /// </summary>
    public partial class WinTreeMap
    {
        internal WinTreeMap()
        {
            InitializeComponent();
            Closed += Window_Closed;
            SizeChanged += _form.WinTreeMap_SizeChanged;

        //    _form.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
        //    _form.Controls.Add(_treeMap);
        //Local.UC_TreeMap _treeMap = new Local.UC_TreeMap
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
            _nWantsLeft = Left;
            _nWantsTop = Top;
        }

        static double _nWantsLeft = -1;
        static double _nWantsTop = -1;
    }
}
