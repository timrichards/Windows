using System;
using System.Windows.Threading;
using System.Windows;
using System.Reactive.Linq;

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

        protected LocalWindow(params Action<Action>[] GetInit)
        {
            if (0 < GetInit.Length)
            {
                GetInit[0](Init);
                return;
            }

            // Keep this around so you see how it's done
            // Icon = BitmapFrame.Create(new Uri(@"pack://application:,,/Resources/ic_people_black_18dp.png"));

            Icon = MainWindow.GetMainWindow().Icon;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.CanResizeWithGrip;
            ShowInTaskbar = false;
            Init();
        }

        void Init()
        {
            // You can comment this stuff out all you want: the flashing close box on the
            // system file dialogs isn't going away...

            if (null == MainWindow.GetTopWindow())
                MainWindow.SetTopWindow(this);

            Observable.FromEventPattern(this, "Activated")
                .Subscribe(args =>
            {
                var bCanFlashWindow = App.CanFlashWindow_ResetsIt;     // querying it resets it

                if (MainWindow.GetTopWindow()._simulatingModal &&
                    (this != MainWindow.GetTopWindow()))
                {
                    MainWindow.GetTopWindow().Activate();

                    if (bCanFlashWindow)
                        FlashWindowStatic.Go(MainWindow.GetTopWindow());
                }
            });

            ShowActivated = true;

            Observable.FromEventPattern(this, "Loaded")
                .Subscribe(args => LocalIsClosed = false);

            Observable.FromEventPattern(this, "Closed")
                .Subscribe(args => LocalIsClosed = true);

            Observable.FromEventPattern(this, "Closing")
                .Subscribe(args => LocalIsClosing = true);

            LocalIsClosed = true;
        }

        internal void CloseIfSimulatingModal()
        {
            if (_simulatingModal)
                Close();
        }

        internal new void Show()
        {
            if ((null != MBoxStatic.MessageBox) &&
                (this != MBoxStatic.MessageBox))
            {
                return;
            }

            Owner = MainWindow.GetTopWindow();
            base.Show();
            PositionWindow();
        }

        internal new bool? ShowDialog() { return ShowDialog(MainWindow.GetTopWindow()); }

        protected virtual void PositionWindow()
        {
        }

        bool? ShowDialog(LocalWindow me)
        {
            // 3/9/15 This is false because e.g. bringing up a New Listing File dialog does not
            // properly focus: a second click is needed to move the window or do anything in it.

            // 3/26/15 This is true because e.g. 1) left open or cancel during Explorer initialize:
            // the Folders VM disappears and crashes on close. 2) Do you want to cancel, left open,
            // mysteriously leaves WinProject unpopulated after clicking OK: does not run any code
            // in MainWindow.xaml.cs after volumes.ShowDialog. Acts like a suppressed null pointer.
            _simulatingModal = true;           // Change it here to switch to simulated dialog
            MainWindow.SetTopWindow(this);
            Owner = me;

            bool? bResult = null;
            DispatcherFrame blockingFrame = null;

            Observable.FromEventPattern(this, "Closed")
                .Subscribe(args =>
            {
                MainWindow.SetTopWindow(me);
                me.Activate();

                if (_simulatingModal)
                {
                    bResult = LocalDialogResult;
                    blockingFrame.Continue = false;
                }
            });

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
