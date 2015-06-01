using FirstFloor.ModernUI.Presentation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using System.Windows.Interop;
using Drawing = System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for ModernWindow1.xaml
    /// </summary>
    public partial class MainWindow
    {
        static Action Init = null;
        static void InitForMainWindowOnly(Action init) { Init = init; }
        public
            MainWindow()
            : base(InitForMainWindowOnly)
        {
            App.Icon = Icon;
            App.LocalMainWindow = this;
            Init();
            Init = null;
            InitializeComponent();
            _mainWindowWR.SetTarget(this);

            Observable.FromEventPattern(this, "Loaded")
                .Subscribe(Window_Loaded);

            Observable.FromEventPattern<System.ComponentModel.CancelEventArgs>(this, "Closing")
                .Subscribe(args => MainWindow_Closing(args.EventArgs));
        }

        static internal LocalUserControlBase
            CurrentPage
        {
            get { return WithMainWindow(mainWindow => mainWindow._currentPage); }
            set
            {
                WithMainWindow(mainWindow =>
                {
                    if (value == mainWindow._currentPage)
                        return false;

                    mainWindow._currentPage = value;
                    mainWindow.TitleLinks.Remove(_titleLink);

                    if (mainWindow._currentPage is WinProject)
                        return false;

                    //if (mainWindow._currentPage is info page)
                    //    return false;

                    mainWindow.TitleLinks.Add(_titleLink);
                    return true;
                });
            }
        }

        static internal T
            WithMainWindow<T>(Func<MainWindow, T> doSomethingWith)
        {
            MainWindow mainWindow = null;

            _mainWindowWR.TryGetTarget(out mainWindow);

            if (null == mainWindow)
            {
                MBoxStatic.Assert(99856, false);
                return default(T);
            }

            return doSomethingWith(mainWindow);
        }

        class DarkWindow : LocalWindowBase
        {
            internal Rect Rect;

            internal DarkWindow(Window owner)
            {
                Rect = Win32Screen.GetWindowRect(owner);
                this.SetRect(new Rect());
                Background = Brushes.Black;
                AllowsTransparency = true;
                Opacity = 0.4;
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
                Content = new Grid();
                Focusable = false;
                IsEnabled = false;
                _ownedWindows = owner.OwnedWindows.Cast<Window>().ToArray();

                foreach (var window in _ownedWindows)
                    window.Owner = null;

                Owner = owner;
            }

            internal T ShowDialog<T>(Func<ILocalWindow, T> showDialog)
            {
                var retVal = default(T);

                Observable.FromEventPattern(this, "SourceInitialized")
                    .Subscribe(x => ShowActivated = false);
                
                Observable.FromEventPattern(this, "ContentRendered")
                    .Subscribe(x =>
                {
                    this.SetRect(Rect);
                    retVal = showDialog(this);

                    foreach (var window in _ownedWindows)
                        window.Owner = Owner;

                    Close();
                });

                base.ShowDialog(App.LocalMainWindow);
                return retVal;
            }

            Window[] _ownedWindows = new Window[] { };
        }

        static List<DarkWindow> _lsDarkWindows = new List<DarkWindow>();

        static internal T
            Darken<T>(Func<ILocalWindow, T> showDialog)
        {
            if (0 < _lsDarkWindows.Count)
                return showDialog(App.TopWindow);

            var bounds = MainWindow.WithMainWindow(Win32Screen.GetWindowMonitorInfo).rcMonitor;

            var doubleBufferWindow = new Window
            {
                Topmost = true,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
                AllowsTransparency = true,
                Opacity = 0
            };

            doubleBufferWindow.SetRect(bounds);

            using (var bitmap = new Drawing.Bitmap(bounds.Width, bounds.Height))
            {
                using (Drawing.Graphics g = Drawing.Graphics.FromImage(bitmap))
                    g.CopyFromScreen(Drawing.Point.Empty, Drawing.Point.Empty, new Drawing.Size(bounds.Width, bounds.Height));

                doubleBufferWindow.Background = new ImageBrush(Imaging.CreateBitmapSourceFromHBitmap(
                    bitmap.GetHbitmap(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions()));
            }

            doubleBufferWindow.Show();
            doubleBufferWindow.Opacity = 1;

            var darkDialog = MainWindow.WithMainWindow(mainWindow =>
            {
                var ownedWindows = mainWindow.OwnedWindows.Cast<Window>().ToArray();

                _lsDarkWindows.Add(new DarkWindow(mainWindow));

                ownedWindows
                    .Where(w => w is ExtraWindow)
                    .Select(w => new DarkWindow(w))
                    .ForEach(_lsDarkWindows.Add);

                NativeMethods.SetWindowPos(new WindowInteropHelper(mainWindow).Handle, SWP.HWND_TOP, 0, 0, 0, 0, SWP.NOSIZE | SWP.NOMOVE);
                return _lsDarkWindows[0];
            });

            foreach (var darkWindow in _lsDarkWindows.Skip(1))
                ((Window)darkWindow).Show();

            foreach (var darkWindow in _lsDarkWindows.Skip(1))
                darkWindow.SetRect(darkWindow.Rect);

            doubleBufferWindow.Opacity = 0;
            doubleBufferWindow.Close();

            var retVal =
                darkDialog.ShowDialog(showDialog);

            foreach (var darkWindow in _lsDarkWindows.Skip(1))
                darkWindow.Close();

            _lsDarkWindows = new List<DarkWindow>();
            return retVal;
        }

        internal void
            ShowLinks(bool bHidden = false)
        {
            if (bHidden)
            {
                while (1 < MenuLinkGroups.Count)
                    MenuLinkGroups.RemoveAt(1);
            }
            else if (1 == MenuLinkGroups.Count)
            {
                foreach (var group in _links)
                    MenuLinkGroups.Add(group);
            }
        }

        void Window_Loaded(System.Reactive.EventPattern<object> obj)
        {
#if (DEBUG)
            //#warning DEBUG is defined.
            MBoxStatic.Assert(99998, System.Diagnostics.Debugger.IsAttached, "Debugger is not attached!");
#else
            if (MBoxStatic.Assert(99997, (System.Diagnostics.Debugger.IsAttached == false), "Debugger is attached but DEBUG is not defined.") == false)
                return;

            var args = AppDomain.CurrentDomain.SetupInformation.ActivationArguments;

            if (null == args)
                return;

            var arrArgs = args.ActivationData;

            // scenario: launched from Start menu
            if (null == arrArgs)                
                return;

            if (false == MBoxStatic.Assert(1308.93165, 0 < arrArgs.Length))
                return;

            var strFile = arrArgs[0];

            if (2 > strFile.Length)
                return;

            switch (Path.GetExtension(strFile).Substring(1))
            {
                case FileParse.ksFileExt_Listing:
                {
                    //form_cbSaveAs.Text = strFile;
                    //AddVolume();
                    //form_tabControlMain.SelectedTab = form_tabPageBrowse;
                    //RestartTreeTimer();
                    break;
                }

                case FileParse.ksFileExt_Project:
                {
                    //if (LoadVolumeList(strFile))
                    //{
                    //    RestartTreeTimer();
                    //}

                    break;
                }

                case FileParse.ksFileExt_Copy:
                {
                    //form_tabControlMain.SelectedTab = form_tabPageBrowse;
                    //form_tabControlCopyIgnore.SelectedTab = form_tabPageCopy;
                    //m_blinky.Go(formLV_CopyScratchpad, clr: Color.Yellow, Once: true);
                    //FormDirListMessageBox("The Copy scratchpad cannot be loaded with no directory listings.", "Load Copy scratchpad externally");
                    //Application.Exit();
                    break;
                }

                case FileParse.ksFileExt_Ignore:
                {
                    //LoadIgnoreList(strFile);
                    //form_tabControlMain.SelectedTab = form_tabPageBrowse;
                    //form_tabControlCopyIgnore.SelectedTab = form_tabPageIgnore;
                    break;
                }
            }
#endif
        }

        void MainWindow_Closing(System.ComponentModel.CancelEventArgs e)
        {
            if ((null != App.LVprojectVM) &&
                App.LVprojectVM.Unsaved &&
                (MessageBoxResult.Cancel == 
                MBoxStatic.ShowDialog(WinProjectVM.UnsavedWarning, "Quit Double File", MessageBoxButton.OKCancel)))
            {
                e.Cancel = true;
                return;
            }

            if (Directory.Exists(ProjectFile.TempPath))
            {
                try { Directory.Delete(ProjectFile.TempPath, true); }
                catch { }
            }

            if (Directory.Exists(ProjectFile.TempPath01))
                Directory.Delete(ProjectFile.TempPath01, true);
        }

        static readonly LinkGroup[]
            _links =
        {
            new LinkGroup { DisplayName="Explore", Links =
            {
                new Link { DisplayName = "Tree map", Source = new Uri("/Win/WinTreeMap.xaml", UriKind.Relative)},
                new Link { DisplayName = "Folders", Source = new Uri("/Win/WinTreeView.xaml", UriKind.Relative)},
                new Link { DisplayName = "Tree list", Source = new Uri("/Win/WinTreeList.xaml", UriKind.Relative)}
            }},
            new LinkGroup { DisplayName="Files", Links =
            {
                new Link { DisplayName = "Files in folder", Source = new Uri("/Win/WinFiles.xaml", UriKind.Relative)},
                new Link { DisplayName = "Duplicates", Source = new Uri("/Win/WinDuplicates.xaml", UriKind.Relative)}
            }},
            new LinkGroup { DisplayName="Search", Links =
            {
                new Link { DisplayName = "Search", Source = new Uri("/Win/WinSearch.xaml", UriKind.Relative)}
            }},
            new LinkGroup { DisplayName="Detailed info", Links =
            {
                new Link { DisplayName = "Detailed info", Source = new Uri("/Win/WinDetail.xaml", UriKind.Relative)}
            }}
        };

        LocalUserControlBase
            _currentPage = null;
        static internal string
            ExtraWindowFakeKey { get { return "/ExtraWindow.xaml"; } }
        static readonly Link
            _titleLink = new Link { DisplayName = "Extra Window", Source = new Uri(ExtraWindowFakeKey, UriKind.Relative) };
        static readonly WeakReference<MainWindow>
            _mainWindowWR = new WeakReference<MainWindow>(null);
    }
}
