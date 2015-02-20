using System;
using System.Windows;
using System.Windows.Threading;
namespace DoubleFile
{
    public class LocalWindow : System.Windows.Window
    {
        internal bool IsClosed = true;

        static protected bool AppActivated = false;

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

        internal new void Show() { Init(); Show(GlobalData.static_Dialog); }
        internal new bool? ShowDialog() { Init(); return ShowDialog(GlobalData.static_Dialog); }

        internal void CloseIfSimulatingModal()
        {
            if (_simulatingModal)
                Close();
        }

        protected void Init()
        {
            // You can comment this stuff out all you want: the flashing close box on the
            // system file dialogs isn't going away...

            Action<Action> notTopDialog = (action) =>
            {
                if (GlobalData.static_Dialog._simulatingModal &&
                    (this != GlobalData.static_Dialog))
                {
                    action();
                }
            };

            Action activateTopDialog = () =>
            {
                GlobalData.static_Dialog.Activate();

                if (false == AppActivated)
                    FlashWindowStatic.Go(GlobalData.static_Dialog);

                AppActivated = false;
            };

            Activated += (o, e) => notTopDialog(activateTopDialog);
            Closing += (o, e) => notTopDialog(() => e.Cancel = true);    // just in case

            if (_simulatingModal)    // just in case
            {
                Deactivated += (o, e) => { if (this == GlobalData.static_Dialog) activateTopDialog(); };
            }

            ShowActivated = true;
            ShowInTaskbar = (this == GlobalData.static_MainWindow);
            Loaded += (o, e) => IsClosed = false;
            Closed += (o, e) => IsClosed = true;
        }

        void Show(LocalWindow me)
        {
            Owner = me;
            base.Show();
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
    }
}
