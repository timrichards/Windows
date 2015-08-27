using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Interop;

namespace DoubleFile
{
    abstract public class LocalWindowBase : Window, ILocalWindow
    {
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
            Width -= 10;
            Height -= 10;
            ResizeMode = ResizeMode.CanResizeWithGrip;
            ShowInTaskbar = false;
            Init();
        }

        void Init()
        {
            // You can comment this stuff out all you want: the flashing close box on the
            // system file dialogs isn't going away...

            Observable.FromEventPattern(this, "SourceInitialized")
                .LocalSubscribe(99744, x =>
                HwndSource
                    .FromHwnd((NativeWindow)this)
                    .AddHook(WndProc));

            // use-case: assert before main window shown
            if (null == Statics.TopWindow)
                return;

            var prevTopWindow = Statics.TopWindow;

            Observable.FromEventPattern(this, "Activated")
                .LocalSubscribe(99743, x =>
            {
                var bCanFlashWindow = Statics.CanFlashWindow_ResetsIt;     // querying it resets it
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
                        : (ILocalWindow)Application.Current.MainWindow;

                    Statics.TopWindow = this;
                }
            });

            Observable.FromEventPattern(this, "Loaded")
                .LocalSubscribe(99742, x => LocalIsClosed = false);

            Observable.FromEventPattern(this, "Closing")
                .LocalSubscribe(99741, x => LocalIsClosing = true);

            Observable.FromEventPattern(this, "Closed")
                .LocalSubscribe(99740, x =>
            {
                LocalIsClosed = true;

                if (null == Application.Current?.MainWindow)
                    return;

                if (this == Application.Current?.MainWindow)
                    return;

                if (this != Statics.TopWindow)
                    return;

                Statics.TopWindow =
                    (false == prevTopWindow.LocalIsClosed)
                    ? prevTopWindow
                    : (ILocalWindow)Application.Current.MainWindow;
            });

            ShowActivated = true;
        }

        IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (this == Application.Current?.MainWindow)        // future proof
                return IntPtr.Zero;

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
                Util.Assert(99796, false);
                return this;
            }

            if (null == Owner)
                Owner = Application.Current.MainWindow;

            IsEnabled = true;
            base.Show();
            return this;
        }

        internal new bool? ShowDialog() => ModalThread.Go(ShowDialog);

        protected bool? ShowDialog(ILocalWindow me)
        {
            if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true))
                return null;

            if ((false == this is IModalWindow) &&
                (false == this is IDarkWindow)) // IDarkWindow will start both modal and modeless; will never be modern window.
            {
                Util.Assert(99795, false);
                return null;
            }

            IsEnabled = this is IModalWindow;

            if (me.LocalIsClosed)
            {
                Util.Assert(99981, false);
                return null;
            }

            I.SimulatingModal = Statics.SimulatingModal;
            Owner = (Window)me;

            Observable.FromEventPattern(this, "Closed")
                .LocalSubscribe(99739, x =>
            {
                me.Activate();
                GoModeless();
            });

            if (false == I.SimulatingModal)
                return base.ShowDialog();

            if (false == LocalIsClosing)
            {
                base.Show();
                _blockingFrame.PushFrameTrue();
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
                Util.Assert(99805, false, bTraceOnly: true);

            return this;
        }

        ILocalWindow
            I => this;
        bool
            ILocalWindow.SimulatingModal { get; set; }
        LocalDispatcherFrame
            _blockingFrame = new LocalDispatcherFrame(99884);
    }
}
