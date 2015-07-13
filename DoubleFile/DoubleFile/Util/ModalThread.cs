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
    interface IModalWindow : ILocalWindow { }
    interface ICantBeTopWindow { }
    interface IDarkWindow : ILocalWindow, ICantBeTopWindow { }

    static class ModalThread
    {
        class
            DarkWindow : LocalWindowBase, IDarkWindow
        {
            internal Rect Rect;

            internal DarkWindow(ILocalWindow owner)
            {
                var winOwner = (Window)owner;

                Rect = Win32Screen.GetWindowRect(winOwner);
                this.SetRect(new Rect());
                Owner = winOwner;
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

        internal static T
            Go<T>(Func<IDarkWindow, T> showDialog)
        {
            if (_bNappingDontDebounce)
                return default(T);

            var retVal = default(T);

            if (_thread.IsAlive)
            {
                var prevTopWindow = App.TopWindow;
                var darkWindow = new DarkWindow(App.TopWindow);

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
                    retVal = GoA(showDialog);

                _blockingFrame.Continue = false;
            }));

            using (Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1)).Timestamp()
                .Subscribe(x => Util.UIthread(99798, () =>
                RepeatedOuterCheckForLockup())))
                _blockingFrame.PushFrameToTrue();
            
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

            var strStuckFrames = LocalDispatcherFrame.ClearFrames();

            MBoxStatic.Assert(nLocation, false, strStuckFrames);
        }

        static IEnumerable<NativeWindow>
            GetNativeWindowsTopDown(IEnumerable<ILocalWindow> ieWindows)
        {
            var lsNativeWindows = ieWindows.Select(w => (NativeWindow)(Window)w).ToList();

            if (0 == lsNativeWindows.Count)
                yield break;

            for (var nativeWindow = NativeMethods.GetTopWindow(IntPtr.Zero); nativeWindow != IntPtr.Zero;
                nativeWindow = NativeMethods.GetWindow(nativeWindow, NativeMethods.GW_HWNDNEXT))
            {
                // array enumerable Linq extension Contains came up empty -?
                var nIx = lsNativeWindows.IndexOf(nativeWindow);

                if (0 > nIx)
                    continue;

                yield return nativeWindow;
                lsNativeWindows.RemoveAt(nIx);

                if (0 == lsNativeWindows.Count)
                    yield break;
            }
        }

        static T
            GoA<T>(Func<IDarkWindow, T> showDialog)
        {
            var dictOwners = Util.Closure(() =>
            {
                var dictOwners_ = new Dictionary<ILocalWindow, Window>();

                Application.Current.Windows
                    .OfType<ILocalWindow>()
                    .Where(w => false == w.LocalIsClosed)    // not shown yet
                    .ForEach(window => dictOwners_.Add(window, ((Window)window).Owner));

                return new ReadOnlyDictionary<ILocalWindow, Window>(dictOwners_);   // from lasmbda
            });

            var mainWindow = MainWindow.WithMainWindow(w => w);
            var doubleBufferWindow = Step1_ShowDoubleBufferedWindow(mainWindow);
            var fakeBaseWindow = Step2_ShowFakeBaseWindow();
            var lsDarkWindows = new List<DarkWindow> { };

            var lsNativeWindowsDarkenedLowestFirst = Util.Closure(() =>
            {
                var lsDarkenedWindows = dictOwners.Keys.Where(w => mainWindow != w).ToList();

                lsDarkenedWindows.ForEach(window =>
                    ((Window)window).Owner = fakeBaseWindow);

                lsDarkWindows =
                    lsDarkenedWindows
                    .Select(w => new DarkWindow(w))
                    .ToList();

                return GetNativeWindowsTopDown(lsDarkenedWindows).Reverse().ToList();  // from lambda
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
                    .Subscribe(x => Util.UIthread(99871, () =>
                    Step3_RepeatedInnerCheckForLockup(darkDialog, ref nWindowCount))))
                    darkDialog.ShowDialog();
            }

            dictOwners.ForEach(kvp =>
            {
                if (false == kvp.Key.LocalIsClosed)
                    ((Window)kvp.Key).Owner = kvp.Value;
            });

            fakeBaseWindow.Close();

            lsNativeWindowsDarkenedLowestFirst.ForEach(NativeMethods
                .BringWindowToTop);

            lsDarkWindows.ForEach(darkWindow =>
            {
                if (false == darkWindow.LocalIsClosed)
                    darkWindow.Close();
            });

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
                .Where(w => false == w.LocalIsClosed))
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

            var owner = NativeMethods.GetWindow(NativeTopWindow(), NativeMethods.GW_OWNER);
 
            if (owner.Equals(MainWindow.WithMainWindow(w => w)))
                return;     // use-case: Open/Save Project; Open Listing File system dialogs

            var lsModalWindows = 
                Application.Current.Windows
                .OfType<IModalWindow>()
                .ToList();

            if (owner.Equals(GetNativeWindowsTopDown(lsModalWindows).FirstOrDefault()))
                return;     // use-case: Source Path and Save To system dialogs in in New/Edit Listing File IModalWindows

            var lsNativeModalWindows = 
                lsModalWindows
                .Select(w => (NativeWindow)(Window)w)
                .ToList();

            if (lsNativeModalWindows.Contains(NativeTopWindow()))
                return;     // use-case: all IModalWindows: New/Edit Listing File; WinProgress; LocalMbox

            if (1 != lsNativeModalWindows.Count)    // if there are two stuck then it needs to be looked into.
            {
                Abort_ClearOut(99797);
            }

            UnstickDialog(lsNativeModalWindows[0]);
        }

        static Window
            Step1_ShowDoubleBufferedWindow(MainWindow mainWindow)
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

                MBoxStatic.Assert(99885, false, strStuckFrames);
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
            MBoxStatic.Assert(99802, nativeModalWindow.Equals(NativeTopWindow()));
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
