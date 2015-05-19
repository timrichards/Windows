using System;
using System.Windows.Threading;
using System.Windows;
using System.Reactive.Linq;
using System.Windows.Controls;
using FirstFloor.ModernUI.Windows.Controls;
using FirstFloor.ModernUI.Windows.Navigation;
using FirstFloor.ModernUI.Windows;
using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    public abstract class LocalUserControlBase : UserControl, IContent
    {
        static protected IEnumerable<WeakReference<LocalUserControlBase>>
            InstantiatedList { get { return _lsInstantiated; } }
        static List<WeakReference<LocalUserControlBase>> _lsInstantiated = new List<WeakReference<LocalUserControlBase>>();
        void LocalNavigatedToA() { _lsInstantiated.Add(new WeakReference<LocalUserControlBase>(this)); LocalNavigatedTo(); }
        void LocalDispose_WindowClosedA()
        {
            _lsInstantiated.First(w =>
            {
                LocalUserControlBase u = null;
                
                w.TryGetTarget(out u);

                var e = ReferenceEquals(this, u);

                if (e)
                    _lsInstantiated.Remove(w);

                return e;
            });
            
            LocalNavigatedTo();
        }

        public void OnFragmentNavigation(FragmentNavigationEventArgs e) { }
        public void OnNavigatedFrom(NavigationEventArgs e) { LocalDispose_WindowClosed(); }
        public void OnNavigatedTo(NavigationEventArgs e) { MainWindow.CurrentPage = this; LocalNavigatedToA(); }
        virtual protected void LocalNavigatedTo() { }
        public void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (false == WinProject.OKtoNavigate())
            {
                e.Cancel = true;
                return;
            }

            if (MainWindow.ExtraWindowFakeKey != "" + e.Source)
                return;

            var page = MainWindow.CurrentPage;
            var content = Activator.CreateInstance(page.GetType()) as LocalUserControlBase;

            content.CopyTag_NewWindow(new WeakReference(page.Tag));

            var window = new ExtraWindow
            {
                Content = content,
                Title = page.LocalTitle,
            }
                .Show();

            Observable.FromEventPattern(window, "Closed")
                .Subscribe(x => content.LocalDispose_WindowClosed());

            e.Cancel = true;
        }

        virtual protected void CopyTag_NewWindow(WeakReference weakReference)
        {
        }

        virtual protected void LocalDispose_WindowClosed()
        {
        }

        public string LocalTitle { get; set; }

        internal bool LocalIsClosed { get; private set; }
        internal bool LocalIsClosing { get; private set; }

        internal bool? LocalDialogResult { get; set; }
        internal void CloseIfSimulatingModal() { }
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
                .Subscribe(x => LocalIsClosed = false);

            Observable.FromEventPattern(this, "Closed")
                .Subscribe(x => LocalIsClosed = true);

            Observable.FromEventPattern(this, "Closing")
                .Subscribe(x => LocalIsClosing = true);

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
        }

        internal new bool? ShowDialog() { return ShowDialog(App.TopWindow); }

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
                .Subscribe(x =>
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
                .Subscribe(x => LocalIsClosed = false);

            Observable.FromEventPattern(this, "Closed")
                .Subscribe(x => LocalIsClosed = true);

            Observable.FromEventPattern(this, "Closing")
                .Subscribe(x => LocalIsClosing = true);

            LocalIsClosed = true;
        }

        internal void CloseIfSimulatingModal()
        {
            if (I.SimulatingModal)
                Close();
        }

        internal new Window Show()
        {
            if ((null != MBoxStatic.MessageBox)) // &&
    //            (this != MBoxStatic.MessageBox))
            {
                return this;
            }

            Owner = (Window)App.TopWindow;
            base.Show();
            return this;
        }

        internal new bool? ShowDialog() { return ShowDialog(App.TopWindow); }

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
                .Subscribe(x =>
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
