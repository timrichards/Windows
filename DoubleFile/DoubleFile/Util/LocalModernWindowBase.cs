using FirstFloor.ModernUI.Windows.Controls;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Interop;

namespace DoubleFile
{
    abstract public class LocalModernWindowBase : ModernWindow, ILocalWindow
    {
        UC_Darken
            DarkenCtl => (UC_Darken)GetTemplateChild("DarkenCtl");
        internal UC_Progress
            ProgressCtl => DarkenCtl.ProgressCtl;

        internal MessageBoxResult
            ShowMessagebox(string strMessage, string strTitle = null, MessageBoxButton? buttons = null)
        {
            var ucMessagebox = DarkenCtl.MessageboxCtl;
            var retVal = MessageBoxResult.None;

            Util.UIthread(99789, () =>
            {
                var locMessagebox = 99692;

                Darken(locMessagebox);
                ucMessagebox.LocalShow();
                retVal = ucMessagebox.ShowMessagebox(strMessage, strTitle, buttons);
                Undarken(locMessagebox);
                ucMessagebox.LocalHide();
            });

            return retVal;
        }

        internal LocalModernWindowBase
            Progress_Darken()
        {
            Darken(_locProgress);
            ProgressCtl.LocalShow();
            return this;
        }
        internal LocalModernWindowBase
            Progress_Undarken()
        {
            Undarken(_locProgress);
            ProgressCtl.LocalHide();
            return this;
        }
        decimal _locProgress = 99637;

        internal LocalModernWindowBase
            Darken(decimal loc)
        {
            if (0 != _locDarkened)
                return this;

            _locDarkened = loc;

            Util.UIthread(99727, () =>
            {
                Application.Current.Windows.OfType<LocalModernWindowBase>()
                    .Where(w => w.IsLoaded)
                    .ForEach(w => w.DarkenCtl.LocalShow());
            });

            return this;
        }
        internal LocalModernWindowBase
            Undarken(decimal loc)
        {
            if (loc != _locDarkened)
                return this;

            _locDarkened = 0;

            Util.UIthread(99726, () =>
            {
                Application.Current.Windows.OfType<LocalModernWindowBase>()
                    .Where(w => w.IsLoaded)
                    .ForEach(w => w.DarkenCtl.LocalHide());
            });

            return this;
        }
        decimal _locDarkened = 0;

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
            if (Application.Current?.Dispatcher.HasShutdownStarted ?? true)
                return this;

            if (this is IModalWindow)
            {
                Util.Assert(99794, false);
                return this;
            }

            if (null == Owner)
                Owner = Application.Current.MainWindow;

            IsEnabled = true;
            base.Show();
            DarkenCtl.Visibility = Visibility.Collapsed;    //jic
            return this;
        }

        internal new bool? ShowDialog() => ModalThread.Go(ShowDialog);

        bool? ShowDialog(ILocalWindow me)
        {
            if (Application.Current?.Dispatcher.HasShutdownStarted ?? true)
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
                DarkenCtl.Visibility = Visibility.Collapsed;
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
