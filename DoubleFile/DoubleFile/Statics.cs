﻿using System.Windows.Media;
using System;
using System.Windows;
using System.IO;
using System.IO.IsolatedStorage;
using System.Reactive.Linq;

namespace DoubleFile
{
    public class Statics
    {
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
                    MainWindow.WithMainWindowA(mainWindow => mainWindow.UpdateTitleLinks());
                    return s;       // from lambda
                });
            }
        }
        LocalUserControlBase _currentPage;

        static internal LV_ProjectVM
            LVprojectVM { get { return _wr.Get(s => s._lvProjectVM); } set { _wr.Get(s => s._lvProjectVM = value); } }
        LV_ProjectVM _lvProjectVM;

        static internal FileDictionary
            FileDictionary { get { return _wr.Get(s => s._fileDictionary); } set { _wr.Get(s => s._fileDictionary = value); } }
        FileDictionary _fileDictionary = new FileDictionary();

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

        static internal IsolatedStorageFile
            IsoStore = null;
        internal const string
            TempPathIso = @"DoubleFile\";
        FileStream _lockTempIsoDir = null;

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

        public Statics(Application app)
        {
            app.ShutdownMode = ShutdownMode.OnMainWindowClose;

            // ensure that Statics is created only once

            if (_wr.Get(s => true))
            {
                Util.Assert(99776, false);
                return;
            }

            _wr.SetTarget(this);

            // set up the IsoStore temp directory: clean it up if it's not locked by another app instance,
            // lock it so it can't be removed by this block of code in a new app instance

            Action CleanupTemp = () =>
            {
                _lockTempIsoDir?.Dispose();

                var bLocked = false;
                var isoStore = IsolatedStorageFile.GetUserStoreForAssembly();

                // files at root are lock files. Temp files are in TempPathIso
                foreach (var strFile in isoStore.GetFileNames())
                { try { isoStore.DeleteFile(strFile); } catch (IsolatedStorageException) { bLocked = true; break; } }

                if (false == bLocked)
                    try { isoStore.Remove(); } catch (IsolatedStorageException) { }
            };

            CleanupTemp();
            IsoStore = IsolatedStorageFile.GetUserStoreForAssembly();
            _lockTempIsoDir = IsoStore.OpenFile(Path.GetRandomFileName(), FileMode.OpenOrCreate, FileAccess.Read, FileShare.None);

            if (false == IsoStore.DirectoryExists(TempPathIso))
                IsoStore.CreateDirectory(TempPathIso);

            Observable.FromEventPattern(app, "Exit")
                .LocalSubscribe(99670, x => CleanupTemp());

            // set up App parameters, starting with the App events for CanFlashWindow_ResetsIt etc.

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

            // set up the main window menu

            foreach (var strSource in new[]
            {
                "/FirstFloor.ModernUI;component/Assets/ModernUI.xaml",
                "/DoubleFile;component/Assets/LocalBlankWindow.xaml",
                "/DoubleFile;component/Assets/LocalMUIdark.xaml",
                "/DoubleFile;component/Assets/Strings.xaml",
                "/DoubleFile;component/Assets/LocalStyles.xaml"
            })
                Application.Current?.Resources.MergedDictionaries.Add(new ResourceDictionary
            {
                Source = new Uri(strSource, UriKind.Relative)
            });
        }

        static WeakReference<Statics> _wr = new WeakReference<Statics>(null);

        // SimulatingModal flag

        // 3/9/15 This is false because e.g. bringing up a New Listing File dialog does not
        // properly focus: a second click is needed to move the window or do anything in it.

        // 3/26/15 This is true because e.g. 1) left open or cancel during Explorer initialize:
        // the Folders VM disappears and crashes on close. 2) Do you want to cancel, left open,
        // mysteriously leaves WinProject unpopulated after clicking OK: does not run any code
        // in MainWindow.xaml.cs after volumes.ShowDialog. Acts like a suppressed null pointer.

        // 6/4/15 This is also true because of GoModeless(), which might also be possible via subclass:
        // https://msdn.microsoft.com/en-us/library/windows/desktop/ms644996%28v=vs.85%29.aspx#modeless_box
        internal const bool SimulatingModal = true;   // Change it here to switch to simulated dialog
    }
}
