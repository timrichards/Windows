using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using System.Windows.Interop;
using Drawing = System.Drawing;
using System.Windows.Media.Imaging;
using System.Threading;
using Microsoft.Win32;

namespace DoubleFile
{
    interface IModalWindow : ILocalWindow { }
    interface ICantBeTopWindow { }
    interface IDarkWindow : ILocalWindow, ICantBeTopWindow { }

    static class ModalThread
    {
        class
            DarkWindow : LocalWindowBase, IDarkWindow
        {
            internal Rect Rect;
            internal static bool ShowDarkWindows = true;

            internal DarkWindow(Window owner)
            {
                Rect = ShowDarkWindows ? Win32Screen.GetWindowRect(owner) : new Rect();
                this.SetRect(new Rect());
                Owner = owner;
                Background = Brushes.Black;
                AllowsTransparency = true;
                Opacity = 0.4;
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
                Focusable = false;
                IsEnabled = false;
            }

            internal new DarkWindow Show() { ((Window)this).Show(); return this; }                  // Darkens ExtraWindows and WinTooltip
            internal new void ShowDialog() => base.ShowDialog((ILocalWindow)Application.Current.MainWindow);      // then modally darkens MainWindow
            internal new void GoModeless() => base.GoModeless();
        }

        class Push : IDisposable
        {
            internal readonly string
                DialogTitle = null;

            internal
                Push(string strDlgTitle)
            {
                DialogTitle = strDlgTitle;
                _prevPush = _wr.Get(p => p);
                _wr.SetTarget(this);

                _dictDimmedWindows = Application.Current.Windows.Cast<Window>()
                    .ToDictionary(w => w, w => w.IsEnabled);

                _dictDimmedWindows.ForEach(kvp => kvp.Key.IsEnabled = false);

                if (1 < ++_nRefCount)
                    return;

                Util.Assert(99767, null == _lockupTimer);

                _lockupTimer = Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1)).Timestamp()
                    .LocalSubscribe(99738, x => Util.UIthread(99798, () =>
                    RepeatedOuterCheckForLockup()));
            }

            public void
                Dispose()
            {
                Application.Current.Windows.Cast<Window>()
                    .ForEach(w =>
                {
                    var bIsEnabled = true;

                    if (_dictDimmedWindows.TryGetValue(w, out bIsEnabled))
                        w.IsEnabled = bIsEnabled;
                });

                _wr.SetTarget(_prevPush);

                if (0 < --_nRefCount)
                    return;

                Util.AssertNotNull(99768, _lockupTimer)?
                    .Dispose();

                _lockupTimer = null;
            }

            static internal void
                AssertAllClear() => Util.Assert(99770, 0 == _nRefCount);

            IDictionary<Window, bool>
                _dictDimmedWindows = new Dictionary<Window, bool>();

            static internal T
                WithPush<T>(Func<Push, T> doSomethingWith) => _wr.Get(p => doSomethingWith(p));
            static WeakReference<Push> _wr = new WeakReference<Push>(null);
            Push _prevPush;

            static int _nRefCount = 0;
            static IDisposable _lockupTimer = null;
        }

        internal static T
            Go<T>(Func<IDarkWindow, T> showDialog, object dlg)
        {
            if (null == dlg)
            {
                Util.Assert(99679, false);
                return default(T);
            }

            var strDlgTitle =
                (dlg is IModalWindow) ? null
                : (dlg is System.Windows.Forms.FolderBrowserDialog) ? "Browse For Folder"
                : dlg.As<OpenFileDialog>()?.Title
                ?? dlg.As<SaveFileDialog>()?.Title
                ?? "";

            if (0 == strDlgTitle?.Length)
            {
                Util.Assert(99677, false);
                DarkWindow.ShowDarkWindows = false;
            }

            if (_bNappingDontDebounce)
                return default(T);

            {
                var napTime = _dtLastDarken - DateTime.Now + TimeSpan.FromMilliseconds(250);

                if (0 < napTime.TotalMilliseconds)
                {
                    _bNappingDontDebounce = true;
                    Util.Block(napTime);
                    _bNappingDontDebounce = false;
                }
            }

            var retVal = default(T);

            if (_thread.IsAlive)
            {
                var prevTopWindow = Statics.TopWindow;
                var darkWindow = new DarkWindow((Window)Statics.TopWindow);

                darkWindow
                    .SetRect(darkWindow.Rect)
                    .Show();

                using (new Push(strDlgTitle))
                    retVal = showDialog(darkWindow);

                if (false == darkWindow.LocalIsClosed)      // happens with system dialogs
                    darkWindow.Close();

                Statics.TopWindow = prevTopWindow;
                Statics.TopWindow?.Activate();
                _dtLastDarken = DateTime.Now;
                return retVal;
            }

            _thread = Util.ThreadMake(() => Util.UIthread(99799, () =>
            {
                if (null != Application.Current)
                    retVal = GoA(showDialog);

                _blockingFrame.Continue = false;
            }));

            using (new Push(strDlgTitle))
                _blockingFrame.PushFrameToTrue();

            Push.AssertAllClear();
            DarkWindow.ShowDarkWindows = true;
            return retVal;
        }

        static void
            Abort_ClearOut(decimal nLocation)
        {
            _thread.Abort();
            _blockingFrame.Continue = false;

            Application.Current.Windows
                .OfType<DarkWindow>()
                .ForEach(w => w.Close());

            Application.Current.Windows.Cast<Window>()
                .ForEach(w => w.IsEnabled = true);

            var strStuckFrames = LocalDispatcherFrame.ClearFrames();

            Util.Assert(nLocation, false, strStuckFrames);
        }

        static IEnumerable<NativeWindow>
            GetNativeWindowsTopDown(IEnumerable<Window> ieWindows = null)
        {
            List<NativeWindow> lsNativeWindows = null;

            if (null != ieWindows)
            {
                lsNativeWindows = ieWindows.Select(w => (NativeWindow)w).ToList();

                if (0 == lsNativeWindows.Count)
                    yield break;
            }

            for (var nativeWindow = NativeMethods.GetTopWindow(IntPtr.Zero); nativeWindow != IntPtr.Zero;
                nativeWindow = NativeMethods.GetWindow(nativeWindow, NativeMethods.GW_HWNDNEXT))
            {
                if (null == lsNativeWindows)
                {
                    yield return nativeWindow;
                    continue;
                }

                // array enumerable Linq extension Contains came up empty -?
                var nIx = lsNativeWindows.IndexOf(nativeWindow);

                if (0 > nIx)
                    continue;

                yield return lsNativeWindows[nIx];
                lsNativeWindows.RemoveAt(nIx);

                if (0 == lsNativeWindows.Count)
                    yield break;
            }
        }

        static T
            GoA<T>(Func<IDarkWindow, T> showDialog)
        {
            IReadOnlyDictionary<ILocalWindow, Window> dictOwners = Util.Closure(() =>
            {
                var dictOwners_ = new Dictionary<ILocalWindow, Window>();

                Application.Current.Windows
                    .OfType<ILocalWindow>()
                    .Where(w => false == w.LocalIsClosed)    // not shown yet
                    .ForEach(window => dictOwners_.Add(window, ((Window)window).Owner));

                return dictOwners_;   // from lambda
            });

            var mainWindow = Application.Current.MainWindow;
            var doubleBufferWindow = Step1_ShowDoubleBufferedWindow();
            var fakeBaseWindow = Step2_ShowFakeBaseWindow();
            var lsDarkWindows = new List<DarkWindow> { };

            var lsNativeWindowsDarkenedLowestFirst = Util.Closure(() =>
            {
                var lsDarkenedWindows =
                    dictOwners
                    .Keys
                    .Where(w => mainWindow != w)
                    .Select(w => (Window)w)
                    .ToList();

                lsDarkenedWindows.ForEach(window =>
                    window.Owner = fakeBaseWindow);

                lsDarkWindows =
                    lsDarkenedWindows
                    .Select(w => new DarkWindow(w))
                    .ToList();

                return
                    GetNativeWindowsTopDown(lsDarkenedWindows)
                    .Reverse()
                    .ToList();  // from lambda
            });

            NativeMethods.SetWindowPos(
                mainWindow, SWP.HWND_TOP, 0, 0, 0, 0, SWP.NOSIZE | SWP.NOMOVE);

            lsDarkWindows.ForEach(darkWindow => darkWindow
                .Show());

            lsDarkWindows.ForEach(darkWindow => darkWindow
                .SetRect(darkWindow.Rect));

            var retVal = default(T);

            {
                var darkDialog = new DarkWindow(mainWindow) { Content = new Grid() };

                Observable.FromEventPattern(darkDialog, "SourceInitialized")
                    .LocalSubscribe(99737, x => darkDialog.ShowActivated = false);

                Observable.FromEventPattern(darkDialog, "ContentRendered")
                    .LocalSubscribe(99736, x =>
                {
                    NativeMethods.SetWindowPos(fakeBaseWindow, SWP.HWND_TOP, 0, 0, 0, 0, SWP.NOSIZE | SWP.NOMOVE | SWP.NOACTIVATE);

                    lsNativeWindowsDarkenedLowestFirst.ForEach(NativeMethods
                        .BringWindowToTop);

                    darkDialog.SetRect(darkDialog.Rect);
                    doubleBufferWindow.Opacity = 0;
                    doubleBufferWindow.Close();
                    retVal = showDialog(darkDialog);
                    Step4_CloseDarkDialog(darkDialog);
                });

                var nWindowCount = 0;

                using (Observable.Timer(TimeSpan.FromMilliseconds(250), TimeSpan.FromSeconds(1)).Timestamp()
                    .LocalSubscribe(99735, x => Util.UIthread(99871, () =>
                    Step3_RepeatedInnerCheckForLockup(darkDialog, ref nWindowCount))))
                    darkDialog.ShowDialog();
            }

            dictOwners.ForEach(kvp =>
            {
                if (false == kvp.Key.LocalIsClosed)
                    ((Window)kvp.Key).Owner = kvp.Value;
            });

            fakeBaseWindow.Close();

            lsDarkWindows.ForEach(darkWindow =>
            {
                if (false == darkWindow.LocalIsClosed)
                    darkWindow.Close();
            });

            lsNativeWindowsDarkenedLowestFirst.ForEach(NativeMethods
                .BringWindowToTop);

            // Look for a modal window stuck behind a dark window and bring it to top. This happens.
            Step5_LastCheckForLockup();
            _dtLastDarken = DateTime.Now;
            return retVal;
        }

        static NativeWindow
            NativeTopWindow()
        {
            // Win32 owner; parent; and child windows ignored by WPF and without hierarchy. Owner set to root owner.
            return GetNativeWindowsTopDown(
                Application.Current.Windows
                .OfType<ILocalWindow>()
                .Where(w => false == w.LocalIsClosed)
                .Select(w => (Window)w))
            .FirstOrDefault();
        }

        static void
            RepeatedOuterCheckForLockup()
        {
            if (false ==
                Application.Current.Windows
                .OfType<DarkWindow>()
                .Any())
            {
                Util.WriteLine("RepeatedOuterCheckForLockup wonky.");
                return;
            }

            var nativeTopWindow = NativeTopWindow();

            if (false == nativeTopWindow.Window is DarkWindow)
            {
                if (false == nativeTopWindow.Window.IsEnabled)
                    Abort_ClearOut(99671);

                return;
            }

            var lsNativeModalWindows = 
                Application.Current.Windows
                .OfType<IModalWindow>()
                .Where(w => false == w.LocalIsClosed)
                .Select(w => (NativeWindow)(Window)w)
                .ToList();

            var strDlgTitle = Util.AssertNotNull(99676, Push.WithPush(w => w))?
                .DialogTitle;

            if (null != strDlgTitle)
            {
                foreach (var systemDialog in
                    NativeMethods
                    .GetAllWindowsWithTitleOf(strDlgTitle))
                {
                    var owner =
                        NativeMethods.GetWindow(
                        NativeMethods.GetWindow(systemDialog, NativeMethods.GW_OWNER), NativeMethods.GW_OWNER);

                    if (owner.Equals(Application.Current.MainWindow))
                        return;     // use-case: system dialogs off the main window

                    if (lsNativeModalWindows.Contains(owner))
                        return;     // use-case: system dialogs off edit/new listing file dlg
                }
            }

            if (1 == lsNativeModalWindows.Count)
                UnstickDialog(lsNativeModalWindows[0]);
            else
                Abort_ClearOut(99797);    // if there are two stuck then it needs to be looked into.
        }

        static Window
            Step1_ShowDoubleBufferedWindow()
        {
            var bounds =
                Win32Screen
                .GetWindowMonitorInfo(Application.Current.MainWindow)
                .rcMonitor;

            var doubleBufferWindow =
                new Window
            {
                Topmost = true,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
                AllowsTransparency = true,
                Opacity = 0,
                ShowActivated = false,
                Focusable = false,
                IsEnabled = false
            }
                .SetRect(new Rect());
                //.SetRect(bounds);        // mahApps seems to have slowed window creation to a crawl

            using (var bitmap = new Drawing::Bitmap(bounds.Width, bounds.Height))
            {
                using (var g = Drawing::Graphics.FromImage(bitmap))
                    g.CopyFromScreen(Drawing::Point.Empty, Drawing::Point.Empty, new Drawing::Size(bounds.Width, bounds.Height));

                doubleBufferWindow.Background = new ImageBrush(Imaging.CreateBitmapSourceFromHBitmap(
                    bitmap.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions()));
            }

            doubleBufferWindow.Show();
            doubleBufferWindow.Opacity = 1;
            return doubleBufferWindow;
        }

        static Window
            Step2_ShowFakeBaseWindow()
        {
            var fakeBaseWindow = new Window
            {
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Opacity = 0
            };

            fakeBaseWindow.Show();

            NativeMethods.SetWindowLong(
                fakeBaseWindow, NativeMethods.GWL_EXSTYLE, NativeMethods.GetWindowLong(fakeBaseWindow, NativeMethods.GWL_EXSTYLE)
                | NativeMethods.WS_EX_TOOLWINDOW);

            return fakeBaseWindow;
        }

        static void
            Step3_RepeatedInnerCheckForLockup(DarkWindow darkDialog, ref int nWindowCount)
        {
            if (0 == nWindowCount)
                nWindowCount = darkDialog.OwnedWindows.Count;

            var bAssert = (0 < nWindowCount) && (0 == darkDialog.OwnedWindows.Count);

            darkDialog.OwnedWindows.OfType<ILocalWindow>()
                .FirstOnlyAssert(w => bAssert = w.LocalIsClosed);

            if (false == bAssert)
                return;             // from lambda

            if (false == darkDialog.LocalIsClosed)
            {
                darkDialog.Close();

                var strStuckFrames = LocalDispatcherFrame.ClearFrames();

                Util.Assert(99885, false, strStuckFrames);
            }
            else
            {
                Util.WriteLine("RepeatedInnerCheckForLockup wonky.");
            }
        }

        static void
            Step4_CloseDarkDialog(DarkWindow darkDialog)
        {
            if (false ==
                darkDialog.OwnedWindows
                .OfType<ILocalWindow>()
                .Where(dialog => false == dialog.LocalIsClosed)
                .FirstOnlyAssert(dialog =>
            {
                Observable.FromEventPattern(dialog, "Closed")
                    .LocalSubscribe(99734, y =>
                {
                    if ((null == darkDialog) || darkDialog.LocalIsClosing || darkDialog.LocalIsClosed)
                        return;     // from lambda

                    darkDialog.Close();
                });

                // Went modeless. Use-case 6/4/15: another progress window is being shown after
                // the first using AllowNewProcess. Now program flow will proceed just like Show()
                // except that the app effectively remains modal to the user (prevent UI via dark
                // dialog). 
                darkDialog.GoModeless();
            }))
            {
                // The above code block did not execute. Still modal: child dialog closed.
                if (false == darkDialog.LocalIsClosed)  // Should only occur at assert loc 9 9 8 8 5 above
                    darkDialog.Close();
            }
        }

        static void
            Step5_LastCheckForLockup()
        {
            // Look for a modal window stuck behind a dark window and bring it to top. This happens.
            var nativeModalWindow = Application.Current.Windows
                .OfType<IModalWindow>()
                .Select(w => (NativeWindow)(Window)w)
                .FirstOnlyAssert(); // Current use-case is one (LocalMbox) on another (WinProgress).  LocalMbox is now closed.

            if (null != nativeModalWindow)
            {
                if (false == nativeModalWindow.Equals(NativeTopWindow()))
                    UnstickDialog(nativeModalWindow);
            }
            else if (Application.Current.Windows
                .OfType<DarkWindow>()
                .Any())
            {
                // This block got hit once, because a standard file dialog made it through to GoB()
                // which happened because statics were per class<T>, so _blocking, at the time a bool, was false.
                // There are no modal windows up. Assert; clear stuck frames; and close all dark windows. 
                // This will prevent the entire code block around assert loc 9 9 8 8 5 above.
                // -actually no it doesn't. But just in case.
                Abort_ClearOut(99803);
            }
        }

        static void
            UnstickDialog(NativeWindow nativeModalWindow)
        {
            NativeMethods
                .SetWindowPos(nativeModalWindow, SWP.HWND_TOP, 0, 0, 0, 0, SWP.NOSIZE | SWP.NOMOVE | SWP.NOACTIVATE);

            Util.WriteLine("Modal window got stuck behind dark window.");
            Util.Assert(99802, nativeModalWindow.Equals(NativeTopWindow()));
        }

        static Thread
            _thread = new Thread(() => { });
        static LocalDispatcherFrame
            _blockingFrame = new LocalDispatcherFrame(99800);
        static bool
            _bNappingDontDebounce = false;
        static DateTime
            _dtLastDarken = DateTime.MinValue;
    }
}
