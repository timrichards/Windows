using System;
using System.Windows;
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
            ShowActivated = true;
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
            _simulatingModal = true;           // Change it here to switch to simulated dialog
            GlobalData.static_Dialog = this;
            Owner = me;

            bool? bResult = null;
            DispatcherFrame blockingFrame = null;

            Action<WindowCollection, bool> SetEnabled = null;
            Action<WindowCollection, bool> SetEnabled_ = (windowCollection, bEnabled) =>
            {
                foreach (var window_ in windowCollection)
                {
                    var window = window_ as Window;

                    if (null == window)
                    {
                        MBoxStatic.Assert(0, false);
                        continue;
                    }

                    window.IsEnabled = bEnabled;
                    window.IsManipulationEnabled = bEnabled;
                    window.Focusable = bEnabled;
                    SetEnabled(window.OwnedWindows, bEnabled);
                }
            };
            SetEnabled = SetEnabled_;

            Closed += (o, e) =>
            {
                GlobalData.static_Dialog = me;
                me.Activate();

                if (_simulatingModal)
                {
                    Owner.IsEnabled = true;
                    Owner.IsManipulationEnabled = true;
                    Owner.Focusable = true;
                    SetEnabled(Owner.OwnedWindows, true);
                    bResult = LocalDialogResult;
                    blockingFrame.Continue = false;
                }
            };

            if ( _simulatingModal)
            {
                Owner.IsEnabled = false;
                Owner.IsManipulationEnabled = false;
                Owner.Focusable = false;
                SetEnabled(Owner.OwnedWindows, false);

                IsEnabled = true;
                base.Show();
                Dispatcher.PushFrame(blockingFrame = new DispatcherFrame(true));
                return bResult;
            }

            return base.ShowDialog();
        }

        bool _simulatingModal = false;      // NO. Must be false. Look up. (this class also controls plain windows.)
    }
}
