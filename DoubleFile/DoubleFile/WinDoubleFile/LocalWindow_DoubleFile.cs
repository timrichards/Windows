using System.Windows;

namespace DoubleFile
{
    public class LocalWindow_DoubleFile : LocalWindow
    {
        protected override void PositionWindow()
        {
            //var chrome = new System.Windows.Shell.WindowChrome();
            //chrome.CornerRadius = new System.Windows.CornerRadius(0);
            //chrome.GlassFrameThickness = new System.Windows.Thickness(1);
            WindowStyle = WindowStyle.ToolWindow;

            var lastWin = GlobalData.static_lastPlacementWindow ?? GlobalData.static_MainWindow;
            var bUseLastWindow = (null != lastWin);

            if (_nWantsLeft > -1)
            {
                Left = _nWantsLeft;
                Top = _nWantsTop;
                _nWantsLeft = _nWantsTop = -1;

                bUseLastWindow &= (Left + Width > SystemParameters.PrimaryScreenWidth);
            }

            if (bUseLastWindow)
            {
                Left = lastWin.Left + lastWin.Width + 5;
                Top = lastWin.Top;

                if (Left + Width > SystemParameters.PrimaryScreenWidth)
                {
                    _nWantsLeft = Left;
                    _nWantsTop = Top;
                    Left = GlobalData.static_MainWindow.Left;
                    Top = lastWin.Top + lastWin.Height + 5;
                }
            }

            GlobalData.static_lastPlacementWindow = this;
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
            
            if (WantsLeft > -1)
            {
                Left = WantsLeft;
                Top = WantsTop;
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

            WantsLeft = Left;
            WantsTop = Top;

            GlobalData.static_lastPlacementWindow = null;
        }

        LocalWindow_DoubleFile _chainedWindow = null;

        virtual protected double WantsLeft { get; set; }
        virtual protected double WantsTop { get; set; }

        static double _nWantsLeft = -1;
        static double _nWantsTop = -1;
    }
}
