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
using System.Collections.ObjectModel;

namespace DoubleFile
{
    interface IModalWindow { }
    interface ICantBeTopWindow { }
    interface IDarkWindow : ILocalWindow, ICantBeTopWindow { }

    class ModalThread
    {
        class DarkWindow : LocalWindowBase, IDarkWindow
        {
            internal Rect Rect;

            internal DarkWindow(Window owner)
            {
                Rect = Win32Screen.GetWindowRect(owner);
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

            internal new DarkWindow Show() { ((Window)this).Show(); return this; }      // Darkens ExtraWindows and WinTooltip
            internal new void ShowDialog() { base.ShowDialog(App.LocalMainWindow); }    // then modally darkens MainWindow
            internal new void GoModeless() { base.GoModeless(); }
        }

        internal static T Go<T>(Func<IDarkWindow, T> showDialog)
        {
            return new ModalThread().GoA<T>(showDialog);
        }

        T GoA<T>(Func<IDarkWindow, T> showDialog)
        {
            if (_bNappingDontDebounce)
                return default(T);

            var retVal = default(T);

            if (_thread.IsAlive)
            {
                var prevTopWindow = App.TopWindow;
                var darkWindow = new DarkWindow((Window)App.TopWindow);

                darkWindow
                    .SetRect(darkWindow.Rect)
                    .Show();

                retVal = showDialog(darkWindow);

                if (false == darkWindow.LocalIsClosed)      // happens with system dialogs
                    darkWindow.Close();

                App.TopWindow = prevTopWindow;
                return retVal;
            }

            _thread = Util.ThreadMake(() => Util.UIthread(99799, () =>
            {
                var napTime = _dtLastDarken - DateTime.Now + TimeSpan.FromMilliseconds(250);

                if (0 < napTime.TotalMilliseconds)
                {
                    _bNappingDontDebounce = true;
                    Util.Block(napTime);
                    _bNappingDontDebounce = false;
                }

                if (null != Application.Current)
                    retVal = GoB(showDialog);

                _blockingFrame.Continue = false;
            }));

            using (Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1)).Timestamp()
                .Subscribe(x => Util.UIthread(99797, () =>
                RepeatedOuterCheckForLockup())))
                _blockingFrame.PushFrameToTrue();
            
            return retVal;
        }

        void Abort()
        {
            _thread.Abort();
            _blockingFrame.Continue = false;
        }

        void RepeatedOuterCheckForLockup()
        {
            var lsNativeModalWindows = Application.Current.Windows.Cast<Window>()
                .Where(w => (w is IModalWindow))
                .Select(w => (NativeWindow)w)
                .ToList();

            if (lsNativeModalWindows.Contains(NativeTopWindow()))
                return;

            if (IntPtr.Zero != NativeMethods.GetWindow(NativeTopWindow(), NativeMethods.GW_OWNER))
                return;     // assume it's a system file/folder dialog:  holey

            if (1 != lsNativeModalWindows.Count)    // if there are two stuck then it needs to be looked into.
            {
                Abort();
                ClearOut(99797);
            }

            UnstickDialog(lsNativeModalWindows[0]);
        }

        void UnstickDialog(NativeWindow nativeModalWindow)
        {
            NativeMethods
                .SetWindowPos(nativeModalWindow, SWP.HWND_TOP, 0, 0, 0, 0, SWP.NOSIZE | SWP.NOMOVE | SWP.NOACTIVATE);

            Util.WriteLine("Modal window got stuck behind dark window.");
            MBoxStatic.Assert(99802, nativeModalWindow.Equals(NativeTopWindow()));
        }

        void ClearOut(decimal nLocation)
        {
            var strStuckFrames = LocalDispatcherFrame.ClearFrames();

            MBoxStatic.Assert(nLocation, false, strStuckFrames);

            Application.Current.Windows.Cast<Window>()
                .Where(w => (w is DarkWindow))
                .ForEach(w => w.Close());
        }

        NativeWindow NativeTopWindow()
        {
            // Win32 owner; parent; and child windows ignored by WPF and without hierarchy.
            var nativeWindows =
                Application.Current.Windows.Cast<Window>()
                .Where(w => (w is ILocalWindow) && (false == ((ILocalWindow)w).LocalIsClosed))
                .Select(w => (NativeWindow)w)
                .ToList();

            for (var nativeWindow = NativeMethods.GetTopWindow(IntPtr.Zero); nativeWindow != IntPtr.Zero;
                nativeWindow = NativeMethods.GetWindow(nativeWindow, NativeMethods.GW_HWNDNEXT))
            {
                if (nativeWindows.Contains(nativeWindow))
                    return nativeWindow;    // from lambda
            }

            return null;
        }

        T GoB<T>(Func<IDarkWindow, T> showDialog)
        {
            IReadOnlyDictionary<Window, Window>
                dictOwners = Util.Closure(() =>
            {
                var dictOwners_ = new Dictionary<Window, Window>();

                Application.Current.Windows.Cast<Window>()
                    .Where(w => (w is ILocalWindow) && (false == ((ILocalWindow)w).LocalIsClosed))
                    .ForEach(window => dictOwners_.Add(window, window.Owner));

                return new ReadOnlyDictionary<Window, Window>(dictOwners_);
            });

            var mainWindow = MainWindow.WithMainWindow(w => w);
            var doubleBufferWindow = ShowDoubleBufferedWindow(mainWindow);
            var fakeBaseWindow = ShowFakeBaseWindow();
            var lsDarkWindows = new List<DarkWindow> { };

            List<NativeWindow> lsNativeWindowsDarkenedFrontToBack = Util.Closure(() =>
            {
                var lsDarkenedWindows = dictOwners.Keys.Where(w => mainWindow != w).ToList();

                lsDarkenedWindows.ForEach(window =>
                    window.Owner = fakeBaseWindow);

                lsDarkWindows =
                    lsDarkenedWindows
                    .Select(w => new DarkWindow(w))
                    .ToList();

                return GetNativeWindowsDarkenedFrontToBack(lsDarkenedWindows);  // from lambda
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
                    .Subscribe(x => darkDialog.ShowActivated = false);

                Observable.FromEventPattern(darkDialog, "ContentRendered")
                    .Subscribe(x =>
                {
                    NativeMethods.SetWindowPos(fakeBaseWindow, SWP.HWND_TOP, 0, 0, 0, 0, SWP.NOSIZE | SWP.NOMOVE | SWP.NOACTIVATE);

                    lsNativeWindowsDarkenedFrontToBack.ForEach(NativeMethods
                        .BringWindowToTop);

                    darkDialog.SetRect(darkDialog.Rect);
                    doubleBufferWindow.Opacity = 0;
                    doubleBufferWindow.Close();
                    retVal = showDialog(darkDialog);
                    CloseDarkDialog(darkDialog);
                });

                var nWindowCount = 0;

                using (Observable.Timer(TimeSpan.FromMilliseconds(250), TimeSpan.FromSeconds(1)).Timestamp()
                    .Subscribe(x => Util.UIthread(99871, () =>
                    RepeatedInnerCheckForLockup(darkDialog, ref nWindowCount))))
                    darkDialog.ShowDialog();
            }

            dictOwners.ForEach(kvp =>
                kvp.Key.Owner = kvp.Value);

            fakeBaseWindow.Close();

            lsNativeWindowsDarkenedFrontToBack.ForEach(NativeMethods
                .BringWindowToTop);

            lsDarkWindows.ForEach(darkWindow => darkWindow
                .Close());

            // Look for a modal window stuck behind a dark window and bring it to top. This happens.
            LastCheckForLockup();
            _dtLastDarken = DateTime.Now;
            return retVal;
        }

        List<NativeWindow> GetNativeWindowsDarkenedFrontToBack(IReadOnlyList<Window> lsDarkenedWindows)
        {
            var lsNativeWindowsDarkenedFrontToBack = new List<NativeWindow> { };
            var lsNativeDarkenedWindows = lsDarkenedWindows.Select(w => (NativeWindow)w).ToList();

            for (var nativeWindow = NativeMethods.GetTopWindow(IntPtr.Zero); nativeWindow != IntPtr.Zero;
                nativeWindow = NativeMethods.GetWindow(nativeWindow, NativeMethods.GW_HWNDNEXT))
            {
                // array enumerable Linq extension Contains came up empty -?
                if (lsNativeDarkenedWindows.Contains(nativeWindow))
                    lsNativeWindowsDarkenedFrontToBack.Insert(0, nativeWindow);
            }

            return lsNativeWindowsDarkenedFrontToBack;
        }

        Window ShowFakeBaseWindow()
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

        Window ShowDoubleBufferedWindow(MainWindow mainWindow)
        {
            var bounds = Win32Screen.GetWindowMonitorInfo(mainWindow).rcMonitor;

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

        void RepeatedInnerCheckForLockup(DarkWindow darkDialog, ref int nWindowCount)
        {
            if (0 == nWindowCount)
                nWindowCount = darkDialog.OwnedWindows.Count;

            var bAssert = (0 < nWindowCount) && (0 == darkDialog.OwnedWindows.Count);

            darkDialog.OwnedWindows.Cast<ILocalWindow>()
                .FirstOnlyAssert(w => bAssert = w.LocalIsClosed);

            if (false == bAssert)
                return;             // from lambda

            if (false == darkDialog.LocalIsClosed)
            {
                darkDialog.Close();

                var strStuckFrames = LocalDispatcherFrame.ClearFrames();

                MBoxStatic.Assert(99885, false, strStuckFrames);
            }
            else
            {
                Util.WriteLine("RepeatedCheckForLockup wonky.");
            }
        }

        void CloseDarkDialog(DarkWindow darkDialog)
        {
            if (false == darkDialog.OwnedWindows.Cast<ILocalWindow>()
                .Where(dialog => false == dialog.LocalIsClosed)
                .FirstOnlyAssert(dialog =>
            {
                Observable.FromEventPattern(dialog, "Closed")
                    .Subscribe(y =>
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

        void LastCheckForLockup()
        {
            // Look for a modal window stuck behind a dark window and bring it to top. This happens.
            var nativeModalWindow = Application.Current.Windows.Cast<Window>()
                .Where(w => (w is IModalWindow))
                .Select(w => (NativeWindow)w)
                .FirstOnlyAssert(); // Current use-case is one (LocalMbox) on another (WinProgress).  LocalMbox is now closed.

            if (null != nativeModalWindow)
            {
                if (false == nativeModalWindow.Equals(NativeTopWindow()))
                    UnstickDialog(nativeModalWindow);
            }
            else if (Application.Current.Windows.Cast<Window>()
                .Where(w => (w is DarkWindow))
                .Any())
            {
                // This block got hit once, because a standard file dialog made it through to GoB()
                // which happened because statics were per class<T>, so _blocking, at the time a bool, was false.
                // There are no modal windows up. Assert; clear stuck frames; and close all dark windows. 
                // This will prevent the entire code block around assert loc 9 9 8 8 5 above.
                // -actually no it doesn't. But just in case.
                ClearOut(99803);
            }
        }

        static Thread
            _thread = new Thread(() => { });
        LocalDispatcherFrame
            _blockingFrame = new LocalDispatcherFrame(99800);
        static bool
            _bNappingDontDebounce = false;
        static DateTime
            _dtLastDarken = DateTime.MinValue;
    }
}
