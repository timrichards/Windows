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
                Owner = owner;
                Background = Brushes.Black;
                AllowsTransparency = true;
                Opacity = 0.4;
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
                Content = new Grid();
                Focusable = false;
                IsEnabled = false;
            }

            internal T ShowDialog<T>(Func<ILocalWindow, T> showDialog)
            {
                var retVal = default(T);

                Observable.FromEventPattern(this, "SourceInitialized")
                    .Subscribe(x => ShowActivated = false);
                
                Observable.FromEventPattern(this, "ContentRendered")
                    .Subscribe(x =>
                {
                    this.SetRect(Rect);
                    retVal = showDialog(this);
                    Close();
                });

                base.ShowDialog(App.LocalMainWindow);
                return retVal;
            }
        }

        static internal T
            Darken<T>(Func<ILocalWindow, T> showDialog) { return MainWindow.WithMainWindow(mainWindow => mainWindow.DarkenA(showDialog)); }
        T DarkenA<T>(Func<ILocalWindow, T> showDialog)
        {
            if (null != _d)
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

            _d = new DarkData();

            (_d.FakeBaseWindow = new Window
            {
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Opacity = 0
            })
                .Show();

            NativeMethods.SetWindowLong(
                _d.FakeBaseWindow, NativeMethods.GWL_EXSTYLE, NativeMethods.GetWindowLong(_d.FakeBaseWindow, NativeMethods.GWL_EXSTYLE)
                | NativeMethods.WS_EX_TOOLWINDOW);

            _d.ExtraWindows = OwnedWindows.Cast<Window>()
                .Where(w => w is ExtraWindow).ToArray();

            foreach (var window in _d.ExtraWindows)
                window.Owner = _d.FakeBaseWindow;

            for (_d.TopWindow = NativeMethods.GetTopWindow(IntPtr.Zero);
                _d.TopWindow != IntPtr.Zero;
                _d.TopWindow = NativeMethods.GetWindow(_d.TopWindow, NativeMethods.GW_HWNDNEXT))
            {
                if (_d.ExtraWindows.Select(w => (NativeWindow)w).Contains(_d.TopWindow))
                    break;
            }

            var darkDialog = new DarkWindow(this);

            _d.ExtraWindows
                .Select(w => new DarkWindow(w))
                .ForEach(_d.DarkWindows.Add);

            NativeMethods.SetWindowPos(this, SWP.HWND_TOP, 0, 0, 0, 0, SWP.NOSIZE | SWP.NOMOVE);

            foreach (var darkWindow in _d.DarkWindows)
                ((Window)darkWindow).Show();

            foreach (var darkWindow in _d.DarkWindows)
                darkWindow.SetRect(darkWindow.Rect);

            doubleBufferWindow.Opacity = 0;
            doubleBufferWindow.Close();

            Observable.FromEventPattern(darkDialog, "ContentRendered")
                .Subscribe(x => NativeMethods.SetWindowPos(_d.FakeBaseWindow, SWP.HWND_TOP, 0, 0, 0, 0, SWP.NOSIZE | SWP.NOMOVE | SWP.NOACTIVATE));

            var retVal =
                darkDialog.ShowDialog(showDialog);

            foreach (var window in _d.ExtraWindows)
                window.Owner = this;

            NativeMethods.BringWindowToTop(_d.TopWindow);
            _d.FakeBaseWindow.Close();

            foreach (var darkWindow in _d.DarkWindows)
                darkWindow.Close();

            _d.DarkWindows = new List<DarkWindow>();
            _d = null;
            return retVal;
        }

        class DarkData
        {
            internal Window
                FakeBaseWindow = null;
            internal NativeWindow
                TopWindow = null;
            internal List<DarkWindow>
                DarkWindows = new List<DarkWindow>();
            internal Window[]
                ExtraWindows = new Window[] { };
        }

        DarkData _d = null;
    }
}
