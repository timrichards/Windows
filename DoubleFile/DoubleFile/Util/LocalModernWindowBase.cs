using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace DoubleFile
{
    abstract public class LocalModernWindowBase : ModernWindow, ILocalWindow
    {
        public ICommand Icmd_OK { get; private set; }
        public ICommand Icmd_Cancel { get; private set; }

        public static Visibility GetDarkened(DependencyObject obj) => (Visibility)obj.GetValue(DarkenedProperty);
        public static void SetDarkened(DependencyObject obj, Visibility value) => obj.SetValue(DarkenedProperty, value);
        public static readonly DependencyProperty DarkenedProperty = DependencyProperty.RegisterAttached(
            "Darkened", typeof(Visibility), typeof(LocalModernWindowBase), new FrameworkPropertyMetadata(Visibility.Collapsed));

        public static Visibility GetShowMessagebox(DependencyObject obj) => (Visibility)obj.GetValue(ShowMessageboxProperty);
        public static void SetShowMessagebox(DependencyObject obj, Visibility value) => obj.SetValue(ShowMessageboxProperty, value);
        public static readonly DependencyProperty ShowMessageboxProperty = DependencyProperty.RegisterAttached(
            "ShowMessagebox", typeof(Visibility), typeof(LocalModernWindowBase), new FrameworkPropertyMetadata(Visibility.Collapsed));

        public static string GetMessageboxText(DependencyObject obj) => (string)obj.GetValue(MessageboxTextProperty);
        public static void SetMessageboxText(DependencyObject obj, string value) => obj.SetValue(MessageboxTextProperty, value);
        public static readonly DependencyProperty MessageboxTextProperty = DependencyProperty.RegisterAttached(
            "MessageboxText", typeof(string), typeof(LocalModernWindowBase), new FrameworkPropertyMetadata(null));

        public static Visibility GetShowProgress(DependencyObject obj) => (Visibility)obj.GetValue(ShowProgressProperty);
        public static void SetShowProgress(DependencyObject obj, Visibility value) => obj.SetValue(ShowProgressProperty, value);
        public static readonly DependencyProperty ShowProgressProperty = DependencyProperty.RegisterAttached(
            "ShowProgress", typeof(Visibility), typeof(LocalModernWindowBase), new FrameworkPropertyMetadata(Visibility.Collapsed));

        internal MessageBoxResult
            ShowMessagebox(string strMessage, string strTitle = null, MessageBoxButton? buttons = null)
        {
            Util.UIthread(99789, () =>
            {
                Application.Current.Windows.OfType<ModernWindow>()
                    .ForEach(w => SetDarkened(w, Visibility.Visible));

                SetShowMessagebox(this, Visibility.Visible);
                SetMessageboxText(this, strMessage);
            });

            _dispatcherFrame_MessageBox.PushFrameTrue();
            return _messageboxResult;
        }
        LocalDispatcherFrame _dispatcherFrame_MessageBox = new LocalDispatcherFrame(99786);
        MessageBoxResult _messageboxResult = MessageBoxResult.None;

        void Messagebox_Close()
        {
            Util.UIthread(99787, () =>
            {
                Application.Current.Windows.OfType<ModernWindow>()
                    .ForEach(w => SetDarkened(w, Visibility.Collapsed));

                SetShowMessagebox(this, Visibility.Collapsed);
            });

            _dispatcherFrame_MessageBox.Continue = false;
        }

        void Messagebox_OK()
        {
            _messageboxResult = MessageBoxResult.OK;
            Messagebox_Close();
        }

        void Messagebox_Cancel()
        {
            _messageboxResult = MessageBoxResult.Cancel;
            Messagebox_Close();
        }

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

        protected LocalModernWindowBase(Action<Action> InitForMainWindowOnly = null)
        {
            DataContext = this;
            Icmd_OK = new RelayCommand(Messagebox_OK);
            Icmd_Cancel = new RelayCommand(Messagebox_Cancel);

            if (null != InitForMainWindowOnly)
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
                .LocalSubscribe(99752, x =>
                HwndSource
                    .FromHwnd((NativeWindow)this)
                    .AddHook(WndProc));

            // use-case: assert before main window shown   future proof
            if (null == Statics.TopWindow)
                return;

            var prevTopWindow = Statics.TopWindow;

            Observable.FromEventPattern(this, "Activated")
                .LocalSubscribe(99751, x =>
            {
                var bCanFlashWindow = Statics.CanFlashWindow_ResetsIt;     // querying it resets it
                var topWindow = Statics.TopWindow;

                if (topWindow.SimulatingModal)
                {
                    if (((this != topWindow) && (false == I.SimulatingModal)) ||
                        (this is ICantBeTopWindow))    // no-op: all modern windows can be a top window
                    {
                        topWindow.Activate();

                        if (bCanFlashWindow)
                            Win32Screen.FlashWindow((Window)topWindow);

                        return;     // from lamnda
                    }
                }

                if ((0 == OwnedWindows.Count) &&
                    (false == this is ICantBeTopWindow))    // no-op: all modern windows can be a top window
                {
                    prevTopWindow =
                        (Statics.TopWindow is ExtraWindow)
                        ? Statics.TopWindow
                        : (ILocalWindow)Application.Current.MainWindow;

                    Statics.TopWindow = this;
                }
            });

            Observable.FromEventPattern(this, "Loaded")
                .LocalSubscribe(99750, x => LocalIsClosed = false);

            Observable.FromEventPattern(this, "Closing")
                .LocalSubscribe(99749, x => LocalIsClosing = true);

            Observable.FromEventPattern(this, "Closed")
                .LocalSubscribe(99748, x =>
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
            if (this == Application.Current?.MainWindow)
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
        
        internal LocalModernWindowBase CloseIfSimulatingModal()
        {
            if (I.SimulatingModal)
                Close();

            return this;
        }

        internal new LocalModernWindowBase Show()
        {
            if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true))
                return this;

            if (this is IModalWindow)
            {
                Util.Assert(99794, false);
                return this;
            }

            MBoxStatic.Restart();

            if (null == Owner)
                Owner = Application.Current.MainWindow;

            IsEnabled = true;
            base.Show();
            return this;
        }

        internal new bool? ShowDialog() => ModalThread.Go(ShowDialog);

        bool? ShowDialog(ILocalWindow me)
        {
            if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true))
                return null;

            if (false == this is IModalWindow)
            {
                Util.Assert(99793, false);
                return null;
            }

            IsEnabled = true;

            if (me.LocalIsClosed)
            {
                Util.Assert(99980, false);
                return null;
            }

            // if false == this is LocalMbox)    // future proof
            MBoxStatic.Restart();

            I.SimulatingModal = Statics.SimulatingModal;
            Owner = (Window)me;

            Observable.FromEventPattern(this, "Closed")
                .LocalSubscribe(99747, x =>
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

        protected LocalModernWindowBase GoModeless()
        {
            _blockingFrame.Continue = false;
            return this;
        }

        internal new LocalModernWindowBase Close()
        {
            if (false == LocalIsClosed)
                Util.UIthread(99829, base.Close);
            else
                Util.Assert(99801, false, bTraceOnly: true);

            return this;
        }

        ILocalWindow
            I => this;
        bool
            ILocalWindow.SimulatingModal { get; set; }
        LocalDispatcherFrame
            _blockingFrame = new LocalDispatcherFrame(99877);
    }
}
