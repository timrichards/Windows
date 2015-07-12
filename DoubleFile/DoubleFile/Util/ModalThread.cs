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

namespace DoubleFile
{
    interface IModalWindow : ILocalWindow { }
    interface ICantBeTopWindow { }
    interface IDarkWindow : ILocalWindow, ICantBeTopWindow { }

    static class ModalThread
    {
        internal static T Go<T>(Func<IDarkWindow, T> showDialog)
        {
            return new _ModalThread<T>(showDialog).Go();
        }

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

        class _ModalThread<T>
        {
            internal _ModalThread(Func<DarkWindow, T> showDialog) { _showDialog = showDialog; }

            internal T Go()
            {
                if (_bBlocking)     // debounce
                    return default(T);

                if (_bDarkening)
                {
                    var prevTopWindow = App.TopWindow;
                    var darkWindow = new DarkWindow((Window)App.TopWindow);

                    darkWindow
                        .SetRect(darkWindow.Rect)
                        .Show();

                    var retValA = _showDialog(darkWindow);

                    if (false == darkWindow.LocalIsClosed)      // happens with system dialogs
                        darkWindow.Close();

                    App.TopWindow = prevTopWindow;
                    return retValA;
                }

                _bDarkening = true;

                {
                    var napTime = _dtLastDarken - DateTime.Now + TimeSpan.FromMilliseconds(250);

                    if (0 < napTime.TotalMilliseconds)
                    {
                        _bBlocking = true;
                        Util.Block(napTime);
                        _bBlocking = false;
                    }
                }

                if (null == Application.Current)
                {
                    _bDarkening = false;
                    return default(T);
                }
            
                return Go_();
            }

            T Go_()
            {
                var dictOwners = new Dictionary<Window, Window>();

                Application.Current.Windows.Cast<Window>()
                    .Where(w => (w is ILocalWindow) && (false == ((ILocalWindow)w).LocalIsClosed))
                    .ForEach(window => dictOwners.Add(window, window.Owner));

                var mainWindow = MainWindow.WithMainWindow(w => w);
                var doubleBufferWindow = ShowDoubleBufferedWindow(mainWindow);
                var fakeBaseWindow = ShowFakeBaseWindow();
                var lsDarkWindows = new List<DarkWindow> { };

                List<NativeWindow> lsNativeWindowsDarkenedFrontToBack = Util.Closure(() =>
                {
                    var lsDarkenedWindows = dictOwners.Keys.Where(w => mainWindow != w).ToList();

                    lsDarkenedWindows.ForEach(window =>
                        window.Owner = fakeBaseWindow);

                    lsDarkenedWindows.Select(w => new DarkWindow(w)).ForEach(
                        lsDarkWindows.Add);     // Add to lsDarkWindows

                    return GetNativeWindowsDarkenedFrontToBack(lsDarkenedWindows);  // from lambda
                });

                NativeMethods.SetWindowPos(mainWindow, SWP.HWND_TOP, 0, 0, 0, 0, SWP.NOSIZE | SWP.NOMOVE);

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
                        retVal = _showDialog(darkDialog);
                        CloseDarkDialog(darkDialog);
                    });

                    var nWindowCount = 0;

                    using (Observable.Timer(TimeSpan.FromMilliseconds(250), TimeSpan.FromSeconds(1)).Timestamp()
                        .Subscribe(x => Util.UIthread(99871, () =>
                        RepeatedCheckForLockup(darkDialog, ref nWindowCount))))
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
                _bDarkening = false;
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

            void RepeatedCheckForLockup(DarkWindow darkDialog, ref int nWindowCount)
            {
                if (0 == nWindowCount)
                    nWindowCount = darkDialog.OwnedWindows.Count;

                var bAssert = (0 < nWindowCount) && (0 == darkDialog.OwnedWindows.Count);

                darkDialog.OwnedWindows.Cast<ILocalWindow>()
                    .FirstOnlyAssert(w => bAssert = w.LocalIsClosed);

                if (false == bAssert)
                    return;             // from lambda

                darkDialog.Close();

                var strStuckFrames = LocalDispatcherFrame.ClearFrames();

                MBoxStatic.Assert(99885, false, strStuckFrames);
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
                    if (false == darkDialog.LocalIsClosed)  // Should only occur at assert loc 9 9 8 8 5 below
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
                    // Win32 owner; parent; and child windows ignored by WPF and without hierarchy.
                    Func<NativeWindow> nativeTopWindow = () =>
                    {
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
                    };

                    if (false == nativeModalWindow.Equals(nativeTopWindow()))
                    {
                        NativeMethods
                            .SetWindowPos(nativeModalWindow, SWP.HWND_TOP, 0, 0, 0, 0, SWP.NOSIZE | SWP.NOMOVE | SWP.NOACTIVATE);

                        Util.WriteLine("Modal window got stuck behind dark window.");
                        MBoxStatic.Assert(99802, nativeModalWindow.Equals(nativeTopWindow()));
                    }
                }
                else if (Application.Current.Windows.Cast<Window>()
                    .Where(w => (w is IDarkWindow))
                    .Any())
                {
                    // This block has never been hit.
                    // There are no modal windows up. Assert; clear stuck frames; and close all dark windows. 
                    // This will prevent the entire code block around assert loc 9 9 8 8 5 above.
                    // -actually no it doesn't. But just in case.
                    var strStuckFrames = LocalDispatcherFrame.ClearFrames();

                    MBoxStatic.Assert(99803, false, strStuckFrames);

                    Application.Current.Windows.Cast<Window>()
                        .Where(w => (w is IDarkWindow))
                        .ForEach(w => w.Close());
                }
            }

            Func<DarkWindow, T>
                _showDialog;
            //Thread
            //    _thread;

            static bool
                _bDarkening = false;
            static bool
                _bBlocking = false;
            static DateTime
                _dtLastDarken = DateTime.MinValue;
        }
    }
}
