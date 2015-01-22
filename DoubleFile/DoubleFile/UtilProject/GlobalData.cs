namespace DoubleFile
{
    partial class GlobalData
    {
        internal static LocalWindow static_TopWindow { get { return static_topWindow_ ?? static_MainWindow; } set { static_topWindow_ = value; } } static LocalWindow static_topWindow_ = null;
        internal static MainWindow static_MainWindow { get; private set; }
        internal static GlobalData GetInstance(MainWindow wpfWin) { if (static_instance == null) { static_instance = new GlobalData(wpfWin); } return static_instance; }

        static GlobalData static_instance = null;
        internal readonly SDL_Timer m_tmrDoTree = new SDL_Timer();
        internal bool m_bRestartTreeTimer = false;
        internal bool m_bKillTree = true;

        GlobalData(MainWindow wpfWin)   // private constructor: singleton pattern
        {
            static_MainWindow = wpfWin;
            m_tmrDoTree.Interval = new System.TimeSpan(0, 0, 3);
        }

        internal static GlobalData Instance
        {
            get
            {
                MBox.Assert(1308.9329, static_instance != null);
                return static_instance;
            }
        }

        internal void RestartTreeTimer()
        {
            m_bRestartTreeTimer = m_bKillTree;
            m_tmrDoTree.Stop();
            m_tmrDoTree.Start();
        }
    }
}
