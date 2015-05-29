using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;
using System.Linq;

namespace DoubleFile
{
    abstract public class LocalModernWindowBase : ModernWindow, ILocalWindow
    {
        public bool LocalIsClosed { get; private set; }
        internal bool LocalIsClosing { get; private set; }

        internal bool? LocalDialogResult
        {
            get
            {
                return
                    I.SimulatingModal
                    ? _localDialogResult
                    : DialogResult;
            }

            set
            {
                if (I.SimulatingModal)
                    _localDialogResult = value;
                else
                    DialogResult = value;
            }
        }
        bool? _localDialogResult = null;

        protected LocalModernWindowBase(Action<Action> InitForMainWindowOnly = null)
        {
            if (null != InitForMainWindowOnly)
            {
                InitForMainWindowOnly(Init);
                return;
            }

            // Keep this around so you see how it's done
            // Icon = BitmapFrame.Create(new Uri(@"pack://application:,,/Assets/ic_people_white_18dp.png"));

            Icon = App.Icon;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.CanResizeWithGrip;
            ShowInTaskbar = false;
            Init();
        }

        void Init()
        {
            // You can comment this stuff out all you want: the flashing close box on the
            // system file dialogs isn't going away...

            if (null == App.TopWindow)
                App.TopWindow = this;

            Observable.FromEventPattern(this, "Activated")
                .Subscribe(x =>
            {
                var bCanFlashWindow = App.CanFlashWindow_ResetsIt;     // querying it resets it
                var topWindow = App.TopWindow;

                if (topWindow.SimulatingModal)
                {
                    if ((this != topWindow) &&
                        (false == I.SimulatingModal))
                    {
                        topWindow.Activate();

                        if (bCanFlashWindow)
                            FlashWindowStatic.Go((Window)topWindow);
                    }
                }

                if ((0 == OwnedWindows.Count)
                    && Focusable)
                {
                    App.TopWindow = this;
                }
            });

            ShowActivated = true;

            Observable.FromEventPattern(this, "Loaded")
                .Subscribe(x =>
            {
                LocalIsClosed = false;

                var hwnd = new WindowInteropHelper(this).Handle;

         //       NativeMethods.SetWindowLong(hwnd, NativeMethods.GWL_STYLE, NativeMethods.GetWindowLong(hwnd, NativeMethods.GWL_STYLE) & ~NativeMethods.WS_MINIMIZEBOX);

                HwndSource
                    .FromHwnd(hwnd)
                    .AddHook(WndProc);
            });

            Observable.FromEventPattern(this, "Closed")
                .Subscribe(x => LocalIsClosed = true);

            Observable.FromEventPattern(this, "Closing")
                .Subscribe(x => LocalIsClosing = true);

            LocalIsClosed = true;
        }

        IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (this is MainWindow)
                return IntPtr.Zero;

            var command = NativeMethods.Command(wParam);

            if (NativeMethods.SC_MINIMIZE == command)
            {
                handled = true;
                return IntPtr.Zero;
            }

            if (this == App.TopWindow)
                return IntPtr.Zero;

            if (msg != NativeMethods.WM_SYSCOMMAND)
                return IntPtr.Zero;

            if (false ==
                new[] { NativeMethods.SC_MAXIMIZE, NativeMethods.SC_RESTORE, NativeMethods.SC_MOVE }
                .Contains(command))
            {
                return IntPtr.Zero;
            }

            handled = true;
            return IntPtr.Zero;
        }
        
        internal void CloseIfSimulatingModal()
        {
            if (I.SimulatingModal)
                Close();
        }

        internal new Window Show()
        {
            if ((null != MBoxStatic.MessageBox)) // &&
    //            (this != MBoxStatic.MessageBox))
            {
                return this;
            }

            Owner = (Window)App.LocalMainWindow;
            base.Show();
            return this;
        }

        internal new bool? ShowDialog() { return MainWindow.Darken(ShowDialog); }

        bool? ShowDialog(ILocalWindow me)
        {
            if (me.LocalIsClosed)
            {
                MBoxStatic.Assert(99980, false);
                return null;
            }

            I.SimulatingModal = App.SimulatingModal;
            Owner = (Window)me;

            bool? bResult = null;
            DispatcherFrame blockingFrame = null;

            Observable.FromEventPattern(this, "Closed")
                .Subscribe(x =>
            {
                App.TopWindow = me;
                me.Activate();

                if (I.SimulatingModal)
                {
                    bResult = LocalDialogResult;
                    blockingFrame.Continue = false;
                }
            });

            if (I.SimulatingModal)
            {
                base.Show();
                Dispatcher.PushFrame(blockingFrame = new DispatcherFrame(true));
                I.SimulatingModal = false;
                return bResult;
            }

            return base.ShowDialog();
        }

        ILocalWindow I { get { return this; } }
        bool ILocalWindow.SimulatingModal { get; set; }
    }
}
