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
                Background = Brushes.Black;
                AllowsTransparency = true;
                Opacity = 0.4;
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
                Content = new Grid();
                Focusable = false;
                IsEnabled = false;
                _ownedWindows = owner.OwnedWindows.Cast<Window>().ToArray();

                foreach (var window in _ownedWindows)
                    window.Owner = _fakeBaseWindow;

                Owner = owner;
            }

            internal T ShowDialog<T>(Func<ILocalWindow, T> showDialog)
            {
                var retVal = default(T);

                Observable.FromEventPattern(this, "SourceInitialized")
                    .Subscribe(x => ShowActivated = false);
                
                Observable.FromEventPattern(this, "ContentRendered")
                    .Subscribe(x =>
                {
                    NativeMethods.SetWindowPos(_fakeBaseWindow, SWP.HWND_TOP, 0, 0, 0, 0, SWP.NOSIZE | SWP.NOMOVE | SWP.NOACTIVATE);
                    this.SetRect(Rect);
                    retVal = showDialog(this);

                    foreach (var window in _ownedWindows)
                        window.Owner = Owner;

                    if (null != (IntPtr)_topWindow)
                        NativeMethods.BringWindowToTop(_topWindow);

                    _topWindow = null;
                    _fakeBaseWindow.Close();
                    _fakeBaseWindow = null;
                    Close();
                });

                base.ShowDialog(App.LocalMainWindow);
                return retVal;
            }

            Window[] _ownedWindows = new Window[] { };
        }

        static List<DarkWindow> _lsDarkWindows = new List<DarkWindow>();
        static NativeWindow _topWindow = null;
        static Window _fakeBaseWindow = null;

        static internal T
            Darken<T>(Func<ILocalWindow, T> showDialog)
        {
            if (0 < _lsDarkWindows.Count)
                return showDialog(App.TopWindow);

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

            (_fakeBaseWindow = new Window
            {
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Opacity = 0
            })
                .Show();

            NativeMethods.SetWindowLong(
                _fakeBaseWindow, NativeMethods.GWL_EXSTYLE, NativeMethods.GetWindowLong(_fakeBaseWindow, NativeMethods.GWL_EXSTYLE)
                | NativeMethods.WS_EX_TOOLWINDOW);

            var darkDialog = MainWindow.WithMainWindow(mainWindow =>
            {
                var ownedWindows = mainWindow.OwnedWindows.Cast<Window>().ToArray();

                for (_topWindow = NativeMethods.GetTopWindow(IntPtr.Zero);
                    _topWindow != IntPtr.Zero;
                    _topWindow = NativeMethods.GetWindow(_topWindow, NativeMethods.GW_HWNDNEXT))
                {
                    if (ownedWindows.Select(w => (NativeWindow)w).Contains(_topWindow))
                        break;
                }

                _lsDarkWindows.Add(new DarkWindow(mainWindow));

                ownedWindows
                    .Where(w => w is ExtraWindow)
                    .Select(w => new DarkWindow(w))
                    .ForEach(_lsDarkWindows.Add);

                NativeMethods.SetWindowPos(mainWindow, SWP.HWND_TOP, 0, 0, 0, 0, SWP.NOSIZE | SWP.NOMOVE);
                return _lsDarkWindows[0];
            });

            foreach (var darkWindow in _lsDarkWindows.Skip(1))
                ((Window)darkWindow).Show();

            foreach (var darkWindow in _lsDarkWindows.Skip(1))
                darkWindow.SetRect(darkWindow.Rect);

            doubleBufferWindow.Opacity = 0;
            doubleBufferWindow.Close();

            var retVal =
                darkDialog.ShowDialog(showDialog);

            foreach (var darkWindow in _lsDarkWindows.Skip(1))
                darkWindow.Close();

            _lsDarkWindows = new List<DarkWindow>();
            return retVal;
        }
    }
}
