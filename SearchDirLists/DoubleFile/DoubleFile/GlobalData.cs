namespace DoubleFile
{
    partial class GlobalData
    {
        internal static bool AppExit = false;
        internal static LocalWindow static_TopWindow { get { return static_topWindow_ ?? static_MainWindow; } set { static_topWindow_ = value; } } static LocalWindow static_topWindow_ = null;
        internal static LocalWindow static_MainWindow { get; private set; }
        internal static GlobalData GetInstance(LocalWindow wpfWin) { if (static_instance == null) { static_instance = new GlobalData(wpfWin); } return static_instance; }
        internal readonly SDL_Timer timer_DoTree = new SDL_Timer();
        internal bool m_bKillTree = true;
        internal bool m_bRestartTreeTimer = false;

        GlobalData(LocalWindow wpfWin)   // private constructor: singleton pattern
        {
            static_MainWindow = wpfWin;
            timer_DoTree.Interval = new System.TimeSpan(0, 0, 3);
        }

        static GlobalData static_instance = null;

        internal static GlobalData GetInstance()
        {
            MBox.Assert(1308.9329, static_instance != null);
            return static_instance;
        }
        internal void RestartTreeTimer()
        {
            m_bRestartTreeTimer = m_bKillTree;
            timer_DoTree.Stop();
            timer_DoTree.Start();
        }
    }
}
