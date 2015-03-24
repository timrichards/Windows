using System.Windows;

namespace DoubleFile
{
    public class LocalWindow_DoubleFile : LocalWindow
    {
        protected override void PositionWindow()
        {
            base.PositionWindow();

            //var chrome = new System.Windows.Shell.WindowChrome();
            //chrome.CornerRadius = new System.Windows.CornerRadius(0);
            //chrome.GlassFrameThickness = new System.Windows.Thickness(1);
            WindowStyle = WindowStyle.ToolWindow;

            var lastWin = MainWindow.static_lastPlacementWindow ?? MainWindow.static_MainWindow;
            var bUseLastWindow = true;

            if (null == _leftWindow)
                _leftWindow = MainWindow.static_MainWindow;

            if (_nWantsLeft > -1)
            {
                Left = _nWantsLeft;
                Top = _nWantsTop;
                _nWantsLeft = _nWantsTop = -1;

                bUseLastWindow = (Left + Width > SystemParameters.PrimaryScreenWidth);
            }

            if ((null != lastWin) &&
                bUseLastWindow)
            {
                Left = lastWin.Left + lastWin.Width + 5;
                Top = lastWin.Top;

                if (Left + Width > SystemParameters.PrimaryScreenWidth)
                {
                    _nWantsLeft = Left;
                    _nWantsTop = Top;
                    Left = _leftWindow.Left;
                    Top = _leftWindow.Top + _leftWindow.Height + 5;
                    _leftWindow = this;
                }
            }

            MainWindow.static_lastPlacementWindow = this;
            Closed += Window_Closed;
        }

        internal new void Show()
        {
            if (false == LocalIsClosed)
            {
                MBoxStatic.Assert(99897, false, bTraceOnly: true);
                return;
            }

            base.Show();
            
            if (Rect.Empty != PosAtClose)
            {
                Left = PosAtClose.Left;
                Top = PosAtClose.Top;
                Width = PosAtClose.Width;
                Height = PosAtClose.Height;
            }

            ShowWindows();
        }

        internal bool ShowWindows()
        {
            if ((null != _chainedWindow) &&
                (false == _chainedWindow.LocalIsClosed))
            {
                return _chainedWindow.ShowWindows();
            }

            _chainedWindow = CreateChainedWindow();

            if (null == _chainedWindow)
                return false;

            _chainedWindow.Show();
            return true;
        }

        virtual protected LocalWindow_DoubleFile CreateChainedWindow()
        {
            return null;
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            if ((null != _chainedWindow) &&
                (false == _chainedWindow.LocalIsClosed))
            {
                _chainedWindow.Close();
            }

            PosAtClose = new Rect(Left, Top, Width, Height);
            MainWindow.static_lastPlacementWindow = null;
        }

        LocalWindow_DoubleFile _chainedWindow = null;

        virtual protected Rect PosAtClose { get; set; }

        static LocalWindow _leftWindow = null;
        static double _nWantsLeft = -1;
        static double _nWantsTop = -1;
    }
}
