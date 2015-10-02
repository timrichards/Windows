using System.Windows.Media;
using System;
using System.Windows;
using System.IO;
using System.IO.IsolatedStorage;
using System.Reactive.Linq;
using System.Windows.Threading;
using System.Windows.Interop;

namespace DoubleFile
{
    public class Statics
    {
        static internal IObservable<Tuple<bool, decimal>>
            EscKey => _escKey;
        static readonly LocalSubject<bool> _escKey = new LocalSubject<bool>();      // bool is a no-op: generic placeholder
        static void EscKeyOnNext() => _escKey.LocalOnNext(false, 99624);

        static internal string
            Namespace => _wr.Get(s => s._namespace);
        readonly string _namespace = null;

        static internal LocalUserControlBase
            CurrentPage
        {
            get { return _wr.Get(s => s._currentPage); }
            set
            {
                _wr.Get(s =>
                {
                    if (value == s._currentPage)
                        return s;   // from lambda

                    s._currentPage = value;
                    MainWindow.UpdateTitleLinks();
                    return s;       // from lambda
                });
            }
        }
        LocalUserControlBase _currentPage;

        static internal T
            WithLVprojectVM<T>(Func<LV_ProjectVM, T> doSomethingWith) => doSomethingWith(_wr.Get(s => s._lvProjectVM));
        static internal LV_ProjectVM
            LVprojectVM_Copy => _wr.Get(s => new LV_ProjectVM(s._lvProjectVM));
        static internal LV_ProjectVM
            LVprojectVM { set { _wr.Get(s => s._lvProjectVM = value); } }
        LV_ProjectVM _lvProjectVM;

        static internal DupeFileDictionary
            DupeFileDictionary { get { return _wr.Get(s => s._fileDictionary); } set { _wr.Get(s => s._fileDictionary = value); } }
        DupeFileDictionary _fileDictionary = new DupeFileDictionary();

        static internal SaveDirListings
            SaveDirListings { get { return _wr.Get(s => s._saveDirListings); } set { _wr.Get(s => s._saveDirListings = value); } }
        SaveDirListings _saveDirListings;

        static internal ILocalWindow
            TopWindow
        {
            get { return _wr.Get(s => s._topWindow); }
            set { _wr.Get(s => s._topWindow = Util.AssertNotNull(99775, value)); }
        }
        ILocalWindow _topWindow;

        static public bool
            AppActivated { get { return _wr.Get(s => s._appActivated); } set { _wr.Get(s => s._appActivated = value); } }
        bool _appActivated;

        static public ImageSource
            Icon { get { return _wr.Get(s => s._icon); } set { _wr.Get(s => s._icon = value); } }
        ImageSource _icon;

        FileStream _lockTempIsoDir = null;

        static internal IObservable<Tuple<bool, decimal>>   // bool is a no-op: generic placeholder
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

        public Statics()
        {
            var app = Application.Current;

            _namespace = app.GetType().Namespace;
            app.ShutdownMode = ShutdownMode.OnMainWindowClose;

            Observable.FromEventPattern<DispatcherUnhandledExceptionEventArgs>(app, "DispatcherUnhandledException")
                .LocalSubscribe(99664, args =>
            {
                args.EventArgs.Handled = true;

                var o = args.EventArgs.Exception.GetBaseException();
                var t = o.GetType();
                var s = "" + o;

                Util.Assert(99665, false, "DispatcherUnhandledException\n" +
                    t + "\n" + o?.Message + "\n" + s + "\n" + o?.StackTrace);
            });

            Observable.FromEventPattern<UnhandledExceptionEventArgs>(AppDomain.CurrentDomain, "UnhandledException")
                .LocalSubscribe(99663, args =>
            {
                var o = args.EventArgs.ExceptionObject?.As<Exception>()?.GetBaseException();
                var t = o?.GetType();
                var s = "" + o;

                Util.Assert(99666, false, "UnhandledException\n" +
                    t + "\n" + o?.Message + "\n" + s + "\n" + o?.StackTrace);
            });

            // ensure that Statics is created only once

            if (_wr.Get(s => true))
            {
                Util.Assert(99776, false);
                return;
            }

            _wr.SetTarget(this);

            // set up the isolated storage temp directory: clean it up if it's not locked by another app instance,
            // lock it so it can't be removed by this block of code in a new app instance

            var isoStore = IsolatedStorageFile.GetUserStoreForAssembly();

            Action CleanupTemp = () =>
            {
                var bLocked = false;

                // files at root are lock files. Temp files are in TempPathIso
                foreach (var strFile in isoStore.GetFileNames())
                {
                    try { isoStore.DeleteFile(strFile); }
                    catch (IsolatedStorageException) { bLocked = true; break; }
                }

                if (false == bLocked)
                    try { isoStore.Remove(); } catch (IsolatedStorageException) { }
            };

            CleanupTemp();
            isoStore = IsolatedStorageFile.GetUserStoreForAssembly();
            LocalIsoStore.InitFromStatics(isoStore);

            var lockFilename = Path.GetRandomFileName();

            _lockTempIsoDir = LocalIsoStore.LockFile(lockFilename);
            LocalIsoStore.CreateDirectory(LocalIsoStore.TempDir);

            // handle Esc key globally: prevent repeat; escape lockup
            ComponentDispatcher.ThreadPreprocessMessage += ComponentDispatcher_ThreadPreprocessMessage;

            // set up App parameters, starting with the Exit event; then the App events for CanFlashWindow_ResetsIt etc.

            Observable.FromEventPattern(app, "Exit")
                .LocalSubscribe(99670, x =>
            {
                ComponentDispatcher.ThreadPreprocessMessage -= ComponentDispatcher_ThreadPreprocessMessage;

                _lockTempIsoDir.Dispose();

                foreach (var strFilename in LocalIsoStore.GetFileNames(LocalIsoStore.TempDir + @"\*.*"))
                {
                    try { LocalIsoStore.DeleteFile(LocalIsoStore.TempDir + @"\" + strFilename); }
                    catch (IsolatedStorageException) { }
                }

                try { LocalIsoStore.DeleteDirectory(LocalIsoStore.TempDir); }
                catch (IsolatedStorageException) { }

                try { LocalIsoStore.DeleteFile(lockFilename); }
                catch (IsolatedStorageException) { }

                CleanupTemp();
            });

            AppActivated = true;      // Application_Activated() seemed to work but jic

            Observable.FromEventPattern(app, "Activated")
                .LocalSubscribe(99771, x =>
            {
                if (false == AppActivated)
                    _canFlashWindow_ResetsIt = false;

                AppActivated = true;
            });

            Observable.FromEventPattern(app, "Deactivated")
                .LocalSubscribe(99769, x => { AppActivated = false; DeactivateDidOccurOnNext(); });

#if (false == DEBUG)
            Observable.FromEventPattern<System.Windows.Threading.DispatcherUnhandledExceptionEventArgs>(app, "DispatcherUnhandledException")
                .LocalSubscribe(99674, args =>
            {
                args.EventArgs.Handled = true;
                Util.Assert(-1, false, args.EventArgs.Exception.Message);
            });
#endif

            // set up the merge dictionaries across assemblies

            foreach (var strSource in new[]
            {
                "/FirstFloor.ModernUI;component/Assets/ModernUI.xaml",
                "/DoubleFile;component/Assets/LocalBlankWindow.xaml",
                "/DoubleFile;component/Assets/LocalModernWindow.xaml",
                "/DoubleFile;component/Assets/LocalMUIdark.xaml",
                "/DoubleFile;component/Assets/Strings.xaml",
                "/DoubleFile;component/Assets/LocalStyles.xaml"
            })
                Application.Current?.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri(strSource, UriKind.Relative)
            });
        }

        private void ComponentDispatcher_ThreadPreprocessMessage(ref MSG msg, ref bool handled)
        {
            // handle Esc key globally: prevent repeat; escape lockup
            const int WM_KEYDOWN = 0x100;
            const int VK_ESCAPE = 0x1B;

            if (WM_KEYDOWN != msg.message)
                return;

            if (VK_ESCAPE != (int)msg.wParam)
                return;

            if (0 == ((int)msg.lParam & (1 << 30)))
            {
                if (TimeSpan.FromMilliseconds(250) > (DateTime.Now - _dtLastEsc))
                {
                    LocalDispatcherFrame.ClearFrames();

                    var mainWindow = (LocalModernWindowBase)Application.Current?.MainWindow;

                    if (mainWindow?.LocalIsClosed ?? true)
                        return;

                    mainWindow.Undarken(-1, bForce: true);
                }
                else
                {
                    EscKeyOnNext();
                    _dtLastEsc = DateTime.Now;
                }

                return;           // previous state is not key down
            }

            if (0 == ((int)msg.lParam & ((1 << 16) - 1)))
                return;           // repeat count is zero, jic

            handled = true;
        }
        DateTime _dtLastEsc = DateTime.MinValue;

        static readonly WeakReference<Statics> _wr = new WeakReference<Statics>(null);

        // SimulatingModal flag

        // 3/9/15 This is false because e.g. bringing up a New Listing File dialog does not
        // properly focus: a second click is needed to move the window or do anything in it.

        // 3/26/15 This is true because e.g. 1) left open or cancel during Explorer initialize:
        // the Folders VM disappears and crashes on close. 2) Did you want to cancel, left open,
        // mysteriously leaves WinProject unpopulated after clicking OK: does not run any code
        // in MainWindow.xaml.cs after volumes.ShowDialog. Acts like a suppressed null pointer.

        // 6/4/15 This is also true because of GoModeless(), which might also be possible via subclass:
        // https://msdn.microsoft.com/en-us/library/windows/desktop/ms644996%28v=vs.85%29.aspx#modeless_box
        internal const bool SimulatingModal = true;   // Change it here to switch to simulated dialog
    }
}
