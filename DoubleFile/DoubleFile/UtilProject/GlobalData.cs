namespace DoubleFile
{
    partial class GlobalData
    {
        internal static LocalWindow static_TopWindow { get { return static_topWindow_ ?? static_MainWindow; } set { static_topWindow_ = value; } } static LocalWindow static_topWindow_ = null;
        internal static MainWindow static_MainWindow { get; private set; }

        internal readonly SDL_Timer m_tmrDoTree = new SDL_Timer();
        internal bool m_bRestartTreeTimer = false;
        internal bool m_bKillTree = true;

        internal static GlobalData Instance
        {
            get { return _Instance; }
            private set
            {
                MBox.Assert(0, _Instance == null);
                _Instance = value;
            }
        }
        static GlobalData _Instance = null;

        internal GlobalData(MainWindow wpfWin)
        {
            static_MainWindow = wpfWin;
            _Instance = this;
            m_tmrDoTree.Interval = new System.TimeSpan(0, 0, 3);
        }

        internal void RestartTreeTimer()
        {
            m_bRestartTreeTimer = m_bKillTree;
            m_tmrDoTree.Stop();
            m_tmrDoTree.Start();
        }
    }
}
