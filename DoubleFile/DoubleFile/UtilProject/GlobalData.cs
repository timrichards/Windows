using System;

namespace DoubleFile
{
    partial class GlobalData : IDisposable
    {
        internal static MainWindow static_MainWindow { get; private set; }
        internal static LocalWindow static_Dialog { get; set; }
        internal static LocalWindow static_lastPlacementWindow { get; set; }

        internal readonly SDL_Timer m_tmrDoTree = null;
        internal bool m_bRestartTreeTimer = false;
        internal bool m_bKillTree = true;

        internal static GlobalData Instance
        {
            get { return _Instance; }
            private set
            {
                MBoxStatic.Assert(99945, _Instance == null);
                _Instance = value;
            }
        }
        static GlobalData _Instance = null;

        internal GlobalData_Tree gd_Tree
        {
            private get { return _gd_Tree; }
            set
            {
                if (value != null)
                    MBoxStatic.Assert(99944, gd_Tree == null);
                _gd_Tree = value;
            }
        }
        GlobalData_Tree _gd_Tree = null;

        internal GlobalData(MainWindow wpfWin)
        {
            static_Dialog = 
                static_MainWindow = 
                wpfWin;
            Instance = this;
            m_tmrDoTree = new SDL_Timer { Interval = 3000 };
        }

        public void Dispose()
        {
            if (m_tmrDoTree != null)
            {
                m_tmrDoTree.Dispose();
            }

            m_blinky.Dispose();
            _Instance = null;
            static_MainWindow = null;
            static_Dialog = null;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "DoubleFile.GlobalData")]
        internal static GlobalData Reset()
        {
            var win = static_MainWindow;

            if (_Instance != null)
                _Instance.Dispose();

            new GlobalData(win);
            return _Instance;
        }

        internal void RestartTreeTimer()
        {
            m_bRestartTreeTimer = m_bKillTree;
            m_tmrDoTree.Stop();
            m_tmrDoTree.Start();
        }
    }
}
