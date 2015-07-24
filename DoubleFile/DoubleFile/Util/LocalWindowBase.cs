using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Interop;

namespace DoubleFile
{
    abstract public class LocalWindowBase : Window, ILocalWindow
    {
        internal bool LocalDidOpen { get; private set; }
        internal bool LocalIsClosing { get; private set; }
        public bool LocalIsClosed { get; private set; } = true;

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

        protected LocalWindowBase(Action<Action> /* future proof */ InitForMainWindowOnly = null)
        {
            if (null != InitForMainWindowOnly)  // future proof
            {
                InitForMainWindowOnly(Init);
                Statics.TopWindow = this;
                LocalIsClosed = false;          // bootstrap main window
                return;
            }

            // Keep this around so you see how it's done
            // Icon = BitmapFrame.Create(new Uri(@"pack://application:,,/Assets/ic_people_white_18dp.png"));

            Icon = Statics.Icon;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.CanResizeWithGrip;
            ShowInTaskbar = false;
            Init();
        }

        void Init()
        {
            // You can comment this stuff out all you want: the flashing close box on the
            // system file dialogs isn't going away...

            Observable.FromEventPattern(this, "SourceInitialized")
                .Subscribe(x =>
                HwndSource
                    .FromHwnd((NativeWindow)this)
                    .AddHook(WndProc));

            // use-case: assert before main window shown
            if (null == Statics.TopWindow)
                return;

            var prevTopWindow = Statics.TopWindow;

            Observable.FromEventPattern(this, "Activated")
                .Subscribe(x =>
            {
                var bCanFlashWindow = App.CanFlashWindow_ResetsIt;     // querying it resets it
                var topWindow = Statics.TopWindow;

                if (topWindow.SimulatingModal)
                {
                    if (((this != topWindow) && (false == I.SimulatingModal)) ||
                        (this is ICantBeTopWindow))
                    {
                        topWindow.Activate();

                        if (bCanFlashWindow)
                            Win32Screen.FlashWindow((Window)topWindow);

                        return;     // from lambda
                    }
                }

                if ((0 == OwnedWindows.Count) &&
                    (false == this is ICantBeTopWindow))
                {
                    prevTopWindow =
                        (Statics.TopWindow is ExtraWindow)
                        ? Statics.TopWindow
                        : MainWindow.WithMainWindow(w => w);

                    Statics.TopWindow = this;
                }
            });

            Observable.FromEventPattern(this, "Loaded")
                .Subscribe(x =>
            {
                LocalDidOpen = true;
                LocalIsClosed = false;
            });

            Observable.FromEventPattern(this, "Closing")
                .Subscribe(x => LocalIsClosing = true);

            Observable.FromEventPattern(this, "Closed")
                .Subscribe(x =>
            {
                LocalIsClosed = true;

                //if (this is MainWindow)       // future proof
                //    return;

                if (this != Statics.TopWindow)
                    return;

                Statics.TopWindow =
                    (false == prevTopWindow.LocalIsClosed)
                    ? prevTopWindow
                    : MainWindow.WithMainWindow(w => w);
            });

            ShowActivated = true;
        }

        IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            //if (this is MainWindow)       // future proof
            //    return IntPtr.Zero;

            var command = NativeMethods.Command(wParam);

            if (NativeMethods.SC_MINIMIZE == command)
            {
                handled = true;
                return IntPtr.Zero;
            }

            if (this == Statics.TopWindow)
                return IntPtr.Zero;

            if (NativeMethods.WM_SYSCOMMAND != msg)
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
        
        internal LocalWindowBase CloseIfSimulatingModal()
        {
            if (I.SimulatingModal)
                Close();

            return this;
        }

        internal new LocalWindowBase Show()
        {
            if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true))
                return this;

            if (this is IModalWindow)
            {
                MBoxStatic.Assert(99796, false);
                return this;
            }

            MBoxStatic.Restart();

            if (null == Owner)
                Owner = Application.Current.MainWindow;

            base.Show();
            return this;
        }

        internal new bool? ShowDialog() { return ModalThread.Go(ShowDialog); }

        protected bool? ShowDialog(ILocalWindow me)
        {
            if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true))
                return null;

            if ((false == this is IModalWindow) &&
                (false == this is IDarkWindow)) // IDarkWindow will start both modal and modeless; will never be modern window.
            {
                MBoxStatic.Assert(99795, false);
                return null;
            }

            if (me.LocalIsClosed)
            {
                MBoxStatic.Assert(99981, false);
                return null;
            }

            if ((false == this is LocalMbox) &&
                (false == this is IDarkWindow))     // IDarkWindow will never be modern window.
            {
                MBoxStatic.Restart();
            }

            I.SimulatingModal = Statics.SimulatingModal;
            Owner = (Window)me;

            Observable.FromEventPattern(this, "Closed")
                .Subscribe(x =>
            {
                me.Activate();
                GoModeless();
            });

            if (false == I.SimulatingModal)
                return base.ShowDialog();

            if (false == LocalIsClosing)
            {
                base.Show();
                _blockingFrame.PushFrameToTrue();
            }

            return LocalDialogResult;
        }

        protected LocalWindowBase GoModeless()
        {
            _blockingFrame.Continue = false;
            return this;
        }

        internal new LocalWindowBase Close()
        {
            if (false == LocalIsClosed)
                Util.UIthread(99842, base.Close);
            else
                MBoxStatic.Assert(99805, false, bTraceOnly: true);

            return this;
        }

        ILocalWindow
            I { get { return this; } }
        bool
            ILocalWindow.SimulatingModal { get; set; }
        LocalDispatcherFrame
            _blockingFrame = new LocalDispatcherFrame(99884);
    }
}
