using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinFormDirList.xaml
    /// </summary>
    public partial class WinTreeList
    {
        internal WinTreeList()
        {
            InitializeComponent();
            Closed += Window_Closed;
            ResizeMode = ResizeMode.CanResize;
            LV_Siblings.DataContext = new LV_TreeListVM();
            LV_Children.DataContext = new LV_TreeListVM();
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
