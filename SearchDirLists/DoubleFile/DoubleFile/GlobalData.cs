using System.Windows;

namespace DoubleFile
{
    partial class GlobalData
    {
        internal static bool AppExit = false;
        internal static Window static_wpfOrForm { get { return static_wpfWin_; } }
        internal static Window static_wpfWin { get { return static_wpfWin_; } } static Window static_wpfWin_ = null;
        internal static GlobalData GetInstance(Window wpfWin) { if (static_instance == null) static_instance = new GlobalData(wpfWin); return static_instance; }
        internal readonly SDL_Timer timer_DoTree = new SDL_Timer();
        internal bool m_bKillTree = true;
        internal bool m_bRestartTreeTimer = false;

        GlobalData(Window wpfWin)   // private constructor: singleton pattern
        {
            static_wpfWin_ = wpfWin;
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
