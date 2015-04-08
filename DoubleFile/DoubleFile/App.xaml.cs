using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static internal event Action DeactivateDidOccur;
        static internal bool LocalActivated { get; private set; }
        static internal bool LocalExit { get; private set; }

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

            Observable
                .FromEventPattern<EventArgs>(this, "Activated")
                .Subscribe(args => Application_Activated());

            Observable
                .FromEventPattern<EventArgs>(this, "Deactivated")
                .Subscribe(args => { LocalActivated = false; if (null != DeactivateDidOccur) DeactivateDidOccur(); });

            Observable
                .FromEventPattern<EventArgs>(this, "Exit")
                .Subscribe(args => LocalExit = true);

#if (false == DEBUG)
            DispatcherUnhandledException += (o, e) =>
            {
                e.Handled = true;
                MBoxStatic.Assert(-1, false, e.Exception.Message);
            };
#endif
            ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        static void Application_Activated()
        {
            if (false == LocalActivated)
            {
                _canFlashWindow_ResetsIt = false;
            }
            
            LocalActivated = true;
        }
    }
}
