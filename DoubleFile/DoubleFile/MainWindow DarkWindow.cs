﻿using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using System.Windows.Interop;
using Drawing = System.Drawing;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace DoubleFile
{
    public partial class MainWindow
    {
        class DarkWindow : LocalWindowBase
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

            internal new void Show() { ((Window)this).Show(); }                         // Darkens ExtraWindows and WinTooltip
            internal new void ShowDialog() { base.ShowDialog(App.LocalMainWindow); }    // then modally darkens MainWindow
            internal new void GoModeless() { base.GoModeless(); }
        }

        static internal T
            Darken<T>(Func<ILocalWindow, T> showDialog) { return MainWindow.WithMainWindow(mainWindow => mainWindow.DarkenA(showDialog)); }
        T DarkenA<T>(Func<ILocalWindow, T> showDialog)
        {
            if (_bBlocking)     // debounce
                return default(T);

            if (_bDarkening)
                return showDialog(App.TopWindow);

            _bDarkening = true;

            {
                var napTime = _dtLastDarken - DateTime.Now + TimeSpan.FromMilliseconds(250);

                if (0 < napTime.Milliseconds)
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
            
            var dictOwners = new Dictionary<Window, Window>();

            foreach (var window in Application.Current.Windows.Cast<Window>()
                .Where(w => (w is ILocalWindow) && (false == ((ILocalWindow)w).LocalIsClosed)))
            {
                dictOwners.Add(window, window.Owner);
            }

            var bounds = MainWindow.WithMainWindow(Win32Screen.GetWindowMonitorInfo).rcMonitor;

            var doubleBufferWindow = new Window
            {
                Topmost = true,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
                AllowsTransparency = true,
                Opacity = 0
            };

            doubleBufferWindow.SetRect(bounds);

            using (var bitmap = new Drawing.Bitmap(bounds.Width, bounds.Height))
            {
                using (var g = Drawing.Graphics.FromImage(bitmap))
                    g.CopyFromScreen(Drawing.Point.Empty, Drawing.Point.Empty, new Drawing.Size(bounds.Width, bounds.Height));

                doubleBufferWindow.Background = new ImageBrush(Imaging.CreateBitmapSourceFromHBitmap(
                    bitmap.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions()));
            }

            doubleBufferWindow.Show();
            doubleBufferWindow.Opacity = 1;

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

            var darkenedWindows = dictOwners.Keys.Where(w => w != this);
            var lsWindowOrder = new List<NativeWindow>();

            {
                var nativeWindows = darkenedWindows.Select(w => (NativeWindow)w).ToArray();

                for (var nativeWindow = NativeMethods.GetTopWindow(IntPtr.Zero); nativeWindow != IntPtr.Zero;
                    nativeWindow = NativeMethods.GetWindow(nativeWindow, NativeMethods.GW_HWNDNEXT))
                {
                    // array enumerable Contains came up empty -?
                    if (Array.Exists(nativeWindows, w => w.Equals(nativeWindow)))
                        lsWindowOrder.Insert(0, nativeWindow);
                }
            }

            foreach (var window in darkenedWindows)
                window.Owner = fakeBaseWindow;

            var lsDarkWindows = new List<DarkWindow>();

            darkenedWindows
                .Select(w => new DarkWindow(w))
                .ForEach(lsDarkWindows.Add);

            NativeMethods.SetWindowPos(this, SWP.HWND_TOP, 0, 0, 0, 0, SWP.NOSIZE | SWP.NOMOVE);

            foreach (var darkWindow in lsDarkWindows)
                darkWindow.Show();

            foreach (var darkWindow in lsDarkWindows)
                darkWindow.SetRect(darkWindow.Rect);

            var retVal = default(T);

            {
                var darkDialog = new DarkWindow(this) { Content = new Grid() };

                Observable.FromEventPattern(darkDialog, "SourceInitialized")
                    .Subscribe(x => darkDialog.ShowActivated = false);

                Observable.FromEventPattern(darkDialog, "ContentRendered")
                    .Subscribe(x =>
                {
                    NativeMethods.SetWindowPos(fakeBaseWindow, SWP.HWND_TOP, 0, 0, 0, 0, SWP.NOSIZE | SWP.NOMOVE | SWP.NOACTIVATE);

                    foreach (var nativeWindow in lsWindowOrder)
                        NativeMethods.BringWindowToTop(nativeWindow);

                    darkDialog.SetRect(darkDialog.Rect);
                    doubleBufferWindow.Opacity = 0;
                    doubleBufferWindow.Close();
                    retVal = showDialog(darkDialog);

                    if (darkDialog.OwnedWindows.Cast<ILocalWindow>()
                        .Any(dialog =>
                    {
                        if (dialog.LocalIsClosed)
                            return false;   // from lambda: close dark window: modal loop ended

                        Observable.FromEventPattern(dialog, "Closed")
                            .Subscribe(y =>
                        {
                            if ((null == darkDialog) || darkDialog.LocalIsClosing || darkDialog.LocalIsClosed)
                                return;     // from lambda

                            darkDialog.Close();
                        });

                        return true;        // from lambda: do not close dark window: went modeless
                    }))
                    {
                        // Went modeless. Use-case 6/4/15: another progress window is being shown after
                        // the first using AllowNewProcess. Now program flow will proceed just like Show()
                        // except that the app effectively remains modal to the user (prevent UI via dark
                        // dialog). 
                        darkDialog.GoModeless();
                    }
                    else
                    {
                        darkDialog.Close();
                    }
                });

                darkDialog.ShowDialog();
            }

            foreach (var kvp in dictOwners)
                kvp.Key.Owner = kvp.Value;

            fakeBaseWindow.Close();

            foreach (var nativeWindow in lsWindowOrder)
                NativeMethods.BringWindowToTop(nativeWindow);

            foreach (var darkWindow in lsDarkWindows)
                darkWindow.Close();

            _dtLastDarken = DateTime.Now;
            _bDarkening = false;
            return retVal;
        }

        bool
            _bDarkening = false;
        bool
            _bBlocking = false;
        DateTime
            _dtLastDarken = DateTime.MinValue;
    }
}
