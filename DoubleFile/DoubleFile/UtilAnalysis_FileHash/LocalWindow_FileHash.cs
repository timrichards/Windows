namespace DoubleFile
{
    public class LocalWindow_DoubleFile : LocalWindow
    {
        protected override void PositionWindow()
        {
            //var chrome = new System.Windows.Shell.WindowChrome();
            //chrome.CornerRadius = new System.Windows.CornerRadius(0);
            //chrome.GlassFrameThickness = new System.Windows.Thickness(1);
            WindowStyle = System.Windows.WindowStyle.ToolWindow;

            var lastWin = GlobalData.static_lastPlacementWindow ?? GlobalData.static_MainWindow;

            if (_nWantsLeft > -1)
            {
                Left = _nWantsLeft;
                Top = _nWantsTop;
                _nWantsLeft = _nWantsTop = -1;
            }
            else if (null != lastWin)
            {
                Left = lastWin.Left + lastWin.Width + 5;
                Top = lastWin.Top;

                if (Left + Width > System.Windows.SystemParameters.PrimaryScreenWidth)
                {
                    _nWantsLeft = Left;
                    _nWantsTop = Top;
                    Left = GlobalData.static_MainWindow.Left;
                    Top = lastWin.Top + lastWin.Height + 5;
                }
            }

            GlobalData.static_lastPlacementWindow = this;
            Closed += (o, e) => GlobalData.static_lastPlacementWindow = null;
        }

        static double _nWantsLeft = -1;
        static double _nWantsTop = -1;
    }
}
