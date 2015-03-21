using System;
using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal static FileDictionary FileDictionary = new FileDictionary();

        internal static event Action DeactivateDidOccur;
        internal static bool LocalActivated { get; private set; }
        internal static bool LocalExit { get; private set; }

        internal static bool CanFlashWindow_ResetsIt
        {
            get
            {
                if (_canFlashWindow_ResetsIt)
                    return true;

                _canFlashWindow_ResetsIt = true;
                return false;
            }
        }
        static bool _canFlashWindow_ResetsIt = true;

        public App()
        {
            LocalActivated = true;      // seemed to work but jic
            LocalExit = false;

            Activated += Application_Activated;
            Deactivated += (o, e) => { LocalActivated = false; if (null != DeactivateDidOccur) DeactivateDidOccur(); };
            Exit += App_Exit;

            DispatcherUnhandledException += (o, e) =>
            {
                e.Handled = true;
                MBoxStatic.Assert(-1, false, e.Exception.Message);
            };

            ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        void App_Exit(object sender, ExitEventArgs e)
        {
            FileDictionary.Dispose();
            FileDictionary = null;
            LocalExit = true;
        }

        private void Application_Activated(object sender, System.EventArgs e)
        {
            if (false == LocalActivated)
            {
                _canFlashWindow_ResetsIt = false;
            }
            
            LocalActivated = true;
        }
    }
}
