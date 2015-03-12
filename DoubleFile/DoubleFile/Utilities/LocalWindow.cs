﻿using System;
using System.Windows.Threading;
using System.Windows;

namespace DoubleFile
{
    public class LocalWindow : Window
    {
        internal bool LocalIsClosed { get; private set; }
        internal bool LocalIsClosing { get; private set; }

        internal bool? LocalDialogResult
        {
            get
            {
                return _simulatingModal
                    ? _LocalDialogResult
                    : DialogResult;
            }

            set
            {
                if (_simulatingModal)
                    _LocalDialogResult = value;
                else
                    DialogResult = value;
            }
        }
        bool? _LocalDialogResult = null;

        protected LocalWindow(bool bIsMainWindow = false)
        {
            // You can comment this stuff out all you want: the flashing close box on the
            // system file dialogs isn't going away...

            Activated += (o, e) =>
            {
                var bCanFlashWindow = App.CanFlashWindow_ResetsIt;     // querying it resets it

                if (GlobalData.static_Dialog._simulatingModal &&
                    (this != GlobalData.static_Dialog))
                {
                    GlobalData.static_Dialog.Activate();

                    if (bCanFlashWindow)
                        FlashWindowStatic.Go(GlobalData.static_Dialog);
                }
            };

            // Keep this around so you see how it's done
            // Icon = BitmapFrame.Create(new Uri(@"pack://application:,,/Resources/ic_people_black_18dp.png"));

            if (false == bIsMainWindow)
            {
                Icon = GlobalData.static_MainWindow.Icon;
                WindowStartupLocation = WindowStartupLocation.CenterOwner;
                ShowInTaskbar = false;
            }

            ResizeMode = ResizeMode.CanResizeWithGrip;
            ShowActivated = true;
            Loaded += (o, e) => LocalIsClosed = false;
            Closed += (o, e) => LocalIsClosed = true;
            Closing += (o, e) => LocalIsClosing = true;
#if (DEBUG)
            Activated += (o, e) => UtilProject.WriteLine(this + " Activated");
            Deactivated += (o, e) => UtilProject.WriteLine(this + " Deactivated");
            GotFocus += (o, e) => UtilProject.WriteLine(this + " GotFocus");
            LostFocus += (o, e) => UtilProject.WriteLine(this + " LostFocus");
#endif
            LocalIsClosed = true;
        }

        internal void CloseIfSimulatingModal()
        {
            if (_simulatingModal)
                Close();
        }

        internal new LocalWindow Show()
        {
            Owner = GlobalData.static_Dialog;
            base.Show();
            PositionWindow();
            return this;
        }

        internal new bool? ShowDialog() { return ShowDialog(GlobalData.static_Dialog); }

        protected virtual void PositionWindow()
        {
        }

        bool? ShowDialog(LocalWindow me)
        {
            // 3/9/15 This is false because e.g. bringing up a New Listing File dialog does not
            // properly focus: a second click is needed to move the window or do anything in it.
            _simulatingModal = false;           // Change it here to switch to simulated dialog
            GlobalData.static_Dialog = this;
            Owner = me;

            bool? bResult = null;
            DispatcherFrame blockingFrame = null;

            Closed += (o, e) =>
            {
                GlobalData.static_Dialog = me;
                me.Activate();

                if (_simulatingModal)
                {
                    bResult = LocalDialogResult;
                    blockingFrame.Continue = false;
                }
            };

            if ( _simulatingModal)
            {
                base.Show();
                Dispatcher.PushFrame(blockingFrame = new DispatcherFrame(true));
                _simulatingModal = false;
                return bResult;
            }

            return base.ShowDialog();
        }

        bool _simulatingModal = false;      // NO. Must be false. Look up. (this class also controls plain windows.)
    }
}