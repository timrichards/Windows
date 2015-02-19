using System.Windows.Threading;
namespace DoubleFile
{
    public class LocalWindow : System.Windows.Window
    {
        internal bool IsClosed = true;
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

        protected void Init()
        {
            Loaded += (o, e) => IsClosed = false;
            Closed += (o, e) => IsClosed = true;
        }

        protected void CloseIfSimulatingModal()
        {
            if (_simulatingModal)
                Close();
        }

        void Show(LocalWindow me)
        {
            Owner = me;
            base.Show();
        }

        bool? ShowDialog(LocalWindow me)
        {
            _simulatingModal = true;              // This is the switch to control simulated dialog
            GlobalData.static_Dialog = this;
            Owner = me;

            bool? bResult = null;

            Closed += (o, e) =>
            {
                GlobalData.static_Dialog = me;
                me.Activate();

                if (_simulatingModal)
                {
                    bResult = LocalDialogResult;
                    _blockingFrame.Continue = false;
                }
            };

            if ( _simulatingModal)
            {
                base.Show();
                Dispatcher.PushFrame(_blockingFrame = new DispatcherFrame(true));
                return bResult;
            }

            return base.ShowDialog();
        }

        DispatcherFrame _blockingFrame;
        bool _simulatingModal = false;      // NO. Must be false. Look up. (this class also controls plain windows.)
    }
}
