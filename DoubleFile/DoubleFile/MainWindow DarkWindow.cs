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
                Focusable = false;
                IsEnabled = false;
            }

            internal new void Show() { ((Window)this).Show(); }                         // Darkens ExtraWindows and WinTooltip
            internal new void ShowDialog() { base.ShowDialog(App.LocalMainWindow); }    // then modally darkens MainWindow
        }

        static internal T
            Darken<T>(Func<ILocalWindow, T> showDialog) { return MainWindow.WithMainWindow(mainWindow => mainWindow.DarkenA(showDialog)); }
        T DarkenA<T>(Func<ILocalWindow, T> showDialog)
        {
            if (_bDarkening)
                return showDialog(App.TopWindow);

            _bDarkening = true;

            var dictOwners = new Dictionary<Window, Window>();

            foreach (Window window in Application.Current.Windows.Cast<Window>()
                .Where(w => (w is ILocalWindow) && (false == ((ILocalWindow)w).LocalIsClosed)))
            {
                dictOwners.Add(window, (Window)window.Owner);
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

            foreach (var window in darkenedWindows)
                window.Owner = fakeBaseWindow;

            var topWindow = NativeMethods.GetTopWindow(IntPtr.Zero);

            for (; topWindow != IntPtr.Zero;
                topWindow = NativeMethods.GetWindow(topWindow, NativeMethods.GW_HWNDNEXT))
            {
                if (darkenedWindows.Where(w => w is ExtraWindow).Select(w => (NativeWindow)w).Contains(topWindow))
                    break;
            }

            var darkWindows = new List<DarkWindow>();

            darkenedWindows
                .Select(w => new DarkWindow(w))
                .ForEach(darkWindows.Add);

            NativeMethods.SetWindowPos(this, SWP.HWND_TOP, 0, 0, 0, 0, SWP.NOSIZE | SWP.NOMOVE);

            foreach (var darkWindow in darkWindows)
                darkWindow.Show();

            foreach (var darkWindow in darkWindows)
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
                    
                    foreach (var winTooltip in darkenedWindows.Where(w => w is WinTooltip))
                        NativeMethods.SetWindowPos(winTooltip, SWP.HWND_BOTTOM, 0, 0, 0, 0, SWP.NOSIZE | SWP.NOMOVE | SWP.NOACTIVATE);

                    NativeMethods.BringWindowToTop(topWindow);
                    darkDialog.SetRect(darkDialog.Rect);
                    doubleBufferWindow.Opacity = 0;
                    doubleBufferWindow.Close();
                    retVal = showDialog(darkDialog);
                    darkDialog.Close();
                });

                darkDialog.ShowDialog();
            }

            foreach (var kvp in dictOwners)
                kvp.Key.Owner = kvp.Value;

            fakeBaseWindow.Close();
            NativeMethods.BringWindowToTop(topWindow);

            foreach (var darkWindow in darkWindows)
                darkWindow.Close();

            _bDarkening = false;
            return retVal;
        }

        bool _bDarkening = false;
    }
}
