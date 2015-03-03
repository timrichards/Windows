using System;
using System.Windows.Threading;
using System.Windows;

namespace DoubleFile
{
    public class LocalWindow : System.Windows.Window
    {
        internal bool IsClosed { get; private set; }
        static protected bool AppActivated { private get; set; }

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
                if (GlobalData.static_Dialog._simulatingModal &&
                    (this != GlobalData.static_Dialog))
                {
                    GlobalData.static_Dialog.Activate();

                    if (false == AppActivated)
                        FlashWindowStatic.Go(GlobalData.static_Dialog);

                    AppActivated = false;
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
            Loaded += (o, e) => IsClosed = false;
            Closed += (o, e) => IsClosed = true;
            IsClosed = true;
        }

        internal void CloseIfSimulatingModal()
        {
            if (_simulatingModal)
                Close();
        }

        internal new LocalWindow Show() { Show(GlobalData.static_Dialog); return this; }
        internal new bool? ShowDialog() { return ShowDialog(GlobalData.static_Dialog); }

        void Show(LocalWindow me)
        {
            Owner = me;
            base.Show();

            var lastWin = GlobalData.static_lastPlacementWindow ?? GlobalData.static_MainWindow;

            if (_nWantsLeft > -1)
            {
                Left = _nWantsLeft;
                Top = _nWantsTop;
                _nWantsLeft = _nWantsTop = -1;
            }
            else if (null != lastWin)
            {
                Left = lastWin.Left + lastWin.Width + 5;
                Top = lastWin.Top;

                if (Left + Width > System.Windows.SystemParameters.PrimaryScreenWidth)
                {
                    _nWantsLeft = Left;
                    _nWantsTop = Top;
                    Left = GlobalData.static_MainWindow.Left;
                    Top = lastWin.Top + lastWin.Height + 5;
                }
            }

            GlobalData.static_lastPlacementWindow = this;
            Closed += (o, e) => GlobalData.static_lastPlacementWindow = null;
        }

        bool? ShowDialog(LocalWindow me)
        {
            _simulatingModal = true;           // Change it here to switch to simulated dialog
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

        static double _nWantsLeft = -1;
        static double _nWantsTop = -1;
    }
}
