using System.Reactive.Linq;
using System.Windows;
using System;

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

            var lastWin = MainWindow.GetLastPlacementWindow() ?? MainWindow.GetMainWindow();
            var bUseLastWindow = true;

            if (null == MainWindow.GetLeftWindow())
                MainWindow.SetLeftWindow(MainWindow.GetMainWindow());

            var leftWindow = MainWindow.GetLeftWindow();
            var rcMonitor = Win32Screen.GetOwnerMonitorRect(leftWindow);

            if (-1 < _nWantsLeft)
            {
                Left = _nWantsLeft;
                Top = _nWantsTop;
                _nWantsLeft = _nWantsTop = -1;
                bUseLastWindow = (Left + Width > rcMonitor.Width);
            }

            if ((null != lastWin) &&
                bUseLastWindow)
            {
                Left = lastWin.Left + lastWin.Width + 5;
                Top = lastWin.Top;

                if (Left + Width > rcMonitor.Width)
                {
                    _nWantsLeft = Left;
                    _nWantsTop = Top;
                    Left = leftWindow.Left;
                    Top = leftWindow.Top + leftWindow.Height + 5;
                    MainWindow.SetLeftWindow(this);
                }
            }

            MainWindow.SetLastPlacementWindow(this);

            Observable.FromEventPattern(this, "Closed")
                .Subscribe(args => Window_Closed());
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
            if (null != _chainedWindow)
            {
                MBoxStatic.Assert(99877, false == _chainedWindow.LocalIsClosed);
                return _chainedWindow.ShowWindows();
            }

            _chainedWindow = CreateChainedWindow();

            if (null == _chainedWindow)
                return false;

            Observable.FromEventPattern(_chainedWindow, "Closed")
                .Subscribe(args => _chainedWindow = null);

            _chainedWindow.Show();
            return true;
        }

        virtual protected LocalWindow_DoubleFile CreateChainedWindow()
        {
            return null;
        }

        private void Window_Closed()
        {
            if ((null != _chainedWindow) &&
                (false == _chainedWindow.LocalIsClosed))
            {
                _chainedWindow.Close();
            }

            PosAtClose = new Rect(Left, Top, Width, Height);
            MainWindow.SetLastPlacementWindow(null);
            MainWindow.SetLeftWindow(null);
        }

        LocalWindow_DoubleFile _chainedWindow = null;

        virtual protected Rect PosAtClose { get; set; }

        static double _nWantsLeft = -1;
        static double _nWantsTop = -1;
    }
}
