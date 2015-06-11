﻿using System;
using System.Windows.Threading;
using System.Windows;
using System.Reactive.Linq;
using System.Windows.Interop;
using System.Linq;

namespace DoubleFile
{
    abstract public class LocalWindowBase : Window, ILocalWindow
    {
        internal bool LocalDidOpen { get; private set; }
        internal bool LocalIsClosing { get; private set; }
        public bool LocalIsClosed { get; private set; }

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

        protected LocalWindowBase(Action<Action> InitForMainWindowOnly = null)
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
                    if (((this != topWindow) && (false == I.SimulatingModal))
                        || (false == Focusable))
                    {
                        topWindow.Activate();

                        if (bCanFlashWindow)
                            Win32Screen.FlashWindow((Window)topWindow);

                        return;
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
                LocalDidOpen = true;
                LocalIsClosed = false;

                HwndSource
                    .FromHwnd((NativeWindow)this)
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
            //if (this is MainWindow)
            //    return IntPtr.Zero;

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
            MBoxStatic.Restart();

            if (null == Owner)
                Owner = (Window)App.LocalMainWindow;

            base.Show();
            return this;
        }

        internal new bool? ShowDialog() { return MainWindow.Darken(ShowDialog); }

        protected bool? ShowDialog(ILocalWindow me)
        {
            if (me.LocalIsClosed)
            {
                MBoxStatic.Assert(99981, false);
                return null;
            }

            I.SimulatingModal = App.SimulatingModal;
            Owner = (Window)me;

            Observable.FromEventPattern(this, "Closed")
                .Subscribe(x =>
            {
                App.TopWindow = me;
                me.Activate();
                GoModeless();
            });

            if (I.SimulatingModal)
            {
                base.Show();
                _blockingFrame.Continue = true;
                Dispatcher.PushFrame(_blockingFrame);
                return LocalDialogResult;
            }

            return base.ShowDialog();
        }

        protected void GoModeless()
        {
            _blockingFrame.Continue = false;
        }

        internal new void Close()
        {
            Util.UIthread(base.Close);
        }

        ILocalWindow
            I { get { return this; } }
        bool
            ILocalWindow.SimulatingModal { get; set; }
        DispatcherFrame
            _blockingFrame = new DispatcherFrame(true) { Continue = false };
    }
}
