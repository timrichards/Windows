using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
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

        static internal IObservable<bool>   // bool is a no-op: generic placeholder
            DeactivateDidOccur { get { return _deactivateDidOccur.AsObservable(); } }
        static readonly Subject<bool> _deactivateDidOccur = new Subject<bool>();
        static void DeactivateDidOccurOnNext() { _deactivateDidOccur.LocalOnNext(false, 99839); }

        static internal ImageSource Icon { get; set; }

        static internal bool LocalActivated { get; private set; }
        static internal bool LocalExit { get; private set; }

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
                .Subscribe(args => Application_Activated());

            Observable.FromEventPattern(this, "Deactivated")
                .Subscribe(args => { LocalActivated = false; DeactivateDidOccurOnNext(); });

            Observable.FromEventPattern(this, "Exit")
                .Subscribe(args => LocalExit = true);   // App.FileDictionary.Dispose();

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
            {
                _canFlashWindow_ResetsIt = false;
            }
            
            LocalActivated = true;
        }
    }
}
