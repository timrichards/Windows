using System;
using System.Windows.Threading;
using System.Windows;
using System.Reactive.Linq;
using System.Windows.Controls;
using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Windows.Navigation;
using FirstFloor.ModernUI.Windows;

namespace DoubleFile
{
    public abstract class LocalUserControlBase : UserControl, IContent
    {
        public void OnFragmentNavigation(FragmentNavigationEventArgs e) { }
        public void OnNavigatedFrom(NavigationEventArgs e) { }
        public void OnNavigatedTo(NavigationEventArgs e) { ModernWindow1.CurrentPage = this; LocalNavigatedTo(); }
        protected virtual void LocalNavigatedTo() { }
        public void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if ("/ExtraWindowUC.xaml" == "" + e.Source)
            {
                new ExtraWindow
                {
                    Content = Activator.CreateInstance(ModernWindow1.CurrentPage.GetType())
                }
                    .Show();

                e.Cancel = true;
            }
        }

        public string Title { get; set; }

        internal bool LocalIsClosed { get; private set; }
        internal bool LocalIsClosing { get; private set; }

        internal bool? LocalDialogResult { get; set; }
        internal void CloseIfSimulatingModal() { }

        protected virtual LocalWindow_DoubleFile CreateChainedWindow() { return null; }
        internal ResizeMode ResizeMode { get; set; }
        virtual protected Rect PosAtClose { get; set; }
    }

    interface ILocalWindow
    {
        bool LocalIsClosed { get; }
        bool SimulatingModal { get; set; }
        bool Activate();
    }

    abstract public class LocalWindowBase : Window, ILocalWindow
    {
        public bool LocalIsClosed { get; private set; }
        internal bool LocalIsClosing { get; private set; }

        internal bool? LocalDialogResult
        {
            get
            {
                return
                    I.SimulatingModal
                    ? _LocalDialogResult
                    : DialogResult;
            }

            set
            {
                if (I.SimulatingModal)
                    _LocalDialogResult = value;
                else
                    DialogResult = value;
            }
        }
        bool? _LocalDialogResult = null;

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
                .Subscribe(args =>
            {
                var bCanFlashWindow = App.CanFlashWindow_ResetsIt;     // querying it resets it
                var topWindow = App.TopWindow;

                if (topWindow.SimulatingModal &&
                    (this != topWindow))
                {
                    topWindow.Activate();

                    if (bCanFlashWindow)
                        FlashWindowStatic.Go((Window)topWindow);
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
            if (I.SimulatingModal)
                Close();
        }

        internal new void Show()
        {
            if ((null != MBoxStatic.MessageBox) &&
                (this != MBoxStatic.MessageBox))
            {
                return;
            }

            Owner = (Window)App.TopWindow;
            base.Show();
            PositionWindow();
        }

        internal new bool? ShowDialog() { return ShowDialog(App.TopWindow); }

        protected virtual void PositionWindow()
        {
        }

        bool? ShowDialog(ILocalWindow me)
        {
            // 3/9/15 This is false because e.g. bringing up a New Listing File dialog does not
            // properly focus: a second click is needed to move the window or do anything in it.

            // 3/26/15 This is true because e.g. 1) left open or cancel during Explorer initialize:
            // the Folders VM disappears and crashes on close. 2) Do you want to cancel, left open,
            // mysteriously leaves WinProject unpopulated after clicking OK: does not run any code
            // in MainWindow.xaml.cs after volumes.ShowDialog. Acts like a suppressed null pointer.
            I.SimulatingModal = true;           // Change it here to switch to simulated dialog
            App.TopWindow = this;
            Owner = (Window)me;

            bool? bResult = null;
            DispatcherFrame blockingFrame = null;

            Observable.FromEventPattern(this, "Closed")
                .Subscribe(args =>
            {
                App.TopWindow = me;
                me.Activate();

                if (I.SimulatingModal)
                {
                    bResult = LocalDialogResult;
                    blockingFrame.Continue = false;
                }
            });

            if (I.SimulatingModal)
            {
                base.Show();
                Dispatcher.PushFrame(blockingFrame = new DispatcherFrame(true));
                I.SimulatingModal = false;
                return bResult;
            }

            return base.ShowDialog();
        }

        ILocalWindow I { get { return this as ILocalWindow; } }
        bool ILocalWindow.SimulatingModal { get; set; }
    }

    abstract public class LocalModernWindowBase : ModernWindow, ILocalWindow
    {
        public bool LocalIsClosed { get; private set; }
        internal bool LocalIsClosing { get; private set; }

        internal bool? LocalDialogResult
        {
            get
            {
                return
                    I.SimulatingModal
                    ? _LocalDialogResult
                    : DialogResult;
            }

            set
            {
                if (I.SimulatingModal)
                    _LocalDialogResult = value;
                else
                    DialogResult = value;
            }
        }
        bool? _LocalDialogResult = null;

        protected LocalModernWindowBase(Action<Action> InitForMainWindowOnly = null)
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
                .Subscribe(args =>
            {
                var bCanFlashWindow = App.CanFlashWindow_ResetsIt;     // querying it resets it
                var topWindow = App.TopWindow;

                if (topWindow.SimulatingModal &&
                    (this != topWindow))
                {
                    topWindow.Activate();

                    if (bCanFlashWindow)
                        FlashWindowStatic.Go((Window)topWindow);
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
            if (I.SimulatingModal)
                Close();
        }

        internal new void Show()
        {
            if ((null != MBoxStatic.MessageBox)) // &&
    //            (this != MBoxStatic.MessageBox))
            {
                return;
            }

            Owner = (Window)App.TopWindow;
            base.Show();
            PositionWindow();
        }

        internal new bool? ShowDialog() { return ShowDialog(App.TopWindow); }

        protected virtual void PositionWindow()
        {
        }

        bool? ShowDialog(ILocalWindow me)
        {
            // 3/9/15 This is false because e.g. bringing up a New Listing File dialog does not
            // properly focus: a second click is needed to move the window or do anything in it.

            // 3/26/15 This is true because e.g. 1) left open or cancel during Explorer initialize:
            // the Folders VM disappears and crashes on close. 2) Do you want to cancel, left open,
            // mysteriously leaves WinProject unpopulated after clicking OK: does not run any code
            // in MainWindow.xaml.cs after volumes.ShowDialog. Acts like a suppressed null pointer.
            I.SimulatingModal = true;           // Change it here to switch to simulated dialog
            App.TopWindow = this;
            Owner = (Window)me;

            bool? bResult = null;
            DispatcherFrame blockingFrame = null;

            Observable.FromEventPattern(this, "Closed")
                .Subscribe(args =>
            {
                App.TopWindow = me;
                me.Activate();

                if (I.SimulatingModal)
                {
                    bResult = LocalDialogResult;
                    blockingFrame.Continue = false;
                }
            });

            if (I.SimulatingModal)
            {
                base.Show();
                Dispatcher.PushFrame(blockingFrame = new DispatcherFrame(true));
                I.SimulatingModal = false;
                return bResult;
            }

            return base.ShowDialog();
        }

        ILocalWindow I { get { return this; } }
        bool ILocalWindow.SimulatingModal { get; set; }
    }
}
