using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static internal ILocalWindow
            LocalMainWindow { get; set; }

        static internal ILocalWindow
            TopWindow { get; set; }

        static internal LV_ProjectVM
            LVprojectVM { get; set; }

        static internal FileDictionary 
            FileDictionary = new FileDictionary();

        static internal SaveDirListings
            SaveDirListings { get; set; }

        static internal IObservable<Tuple<bool, int>>   // bool is a no-op: generic placeholder
            DeactivateDidOccur { get { return _deactivateDidOccur.AsObservable(); } }
        static readonly LocalSubject<bool> _deactivateDidOccur = new LocalSubject<bool>();
        static void DeactivateDidOccurOnNext() { _deactivateDidOccur.LocalOnNext(false, 99839); }

        static internal ImageSource Icon { get; set; }

        static internal bool LocalActivated { get; private set; }
        static internal bool LocalExit { get; private set; }

        // SimulatingModal flag

        // 3/9/15 This is false because e.g. bringing up a New Listing File dialog does not
        // properly focus: a second click is needed to move the window or do anything in it.

        // 3/26/15 This is true because e.g. 1) left open or cancel during Explorer initialize:
        // the Folders VM disappears and crashes on close. 2) Do you want to cancel, left open,
        // mysteriously leaves WinProject unpopulated after clicking OK: does not run any code
        // in MainWindow.xaml.cs after volumes.ShowDialog. Acts like a suppressed null pointer.

        // 6/4/15 This is also true because of GoModeless(), which I've never seen demostrated
        // natively, and believe is not possible.
        static internal readonly bool SimulatingModal = true;   // Change it here to switch to simulated dialog

        static internal bool CanFlashWindow_ResetsIt
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

            Observable.FromEventPattern(this, "Activated")
                .Subscribe(x => Application_Activated());

            Observable.FromEventPattern(this, "Deactivated")
                .Subscribe(x => { LocalActivated = false; DeactivateDidOccurOnNext(); });

            Observable.FromEventPattern(this, "Exit")
                .Subscribe(x => LocalExit = true);   // App.FileDictionary.Dispose();

#if (false == DEBUG)
            Observable.FromEventPattern<System.Windows.Threading.DispatcherUnhandledExceptionEventArgs>(this, "DispatcherUnhandledException")
                .Subscribe(args =>
            {
                args.EventArgs.Handled = true;
                MBoxStatic.Assert(-1, false, args.EventArgs.Exception.Message);
            });
#endif
            ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        static void Application_Activated()
        {
            if (false == LocalActivated)
                _canFlashWindow_ResetsIt = false;
            
            LocalActivated = true;
        }
    }
}
