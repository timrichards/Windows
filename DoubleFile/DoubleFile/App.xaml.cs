using System;
using System.Reactive.Linq;
using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static internal IObservable<Tuple<bool, int>>   // bool is a no-op: generic placeholder
            DeactivateDidOccur => _deactivateDidOccur;
        static readonly LocalSubject<bool> _deactivateDidOccur = new LocalSubject<bool>();
        static void DeactivateDidOccurOnNext() => _deactivateDidOccur.LocalOnNext(false, 99839);

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
            _statics = new Statics();
            Statics.AppActivated = true;      // Application_Activated() seemed to work but jic

            Observable.FromEventPattern(this, "Activated")
                .LocalSubscribe(99771, x => Application_Activated());

            Observable.FromEventPattern(this, "Deactivated")
                .LocalSubscribe(99769, x => { Statics.AppActivated = false; DeactivateDidOccurOnNext(); });

#if (false == DEBUG)
            Observable.FromEventPattern<System.Windows.Threading.DispatcherUnhandledExceptionEventArgs>(this, "DispatcherUnhandledException")
                .LocalSubscribe(args =>
            {
                args.EventArgs.Handled = true;
                Util.Assert(-1, false, args.EventArgs.Exception.Message);
            });
#endif
            ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        static void Application_Activated()
        {
            if (false == Statics.AppActivated)
                _canFlashWindow_ResetsIt = false;
            
            Statics.AppActivated = true;
        }

        Statics
            _statics = null;
    }
}
