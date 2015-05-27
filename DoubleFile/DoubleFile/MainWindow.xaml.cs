using FirstFloor.ModernUI.Presentation;
using System;
using System.IO;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;

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
            internal T ShowDialog<T>(Func<ILocalWindow, T> showDialog)
            {
                Background = Brushes.Black;
                Opacity = 0.4;
                AllowsTransparency = true;
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
                Content = new Grid();

                var retVal = default(T);

                Observable.FromEventPattern(this, "ContentRendered")
                    .Subscribe(x =>
                {
                    retVal = showDialog(this);
                    Close();
                });

                base.ShowDialog(App.TopWindow);
                return retVal;
            }
        }

        static DarkWindow _darkWindow = null;
        static internal T
            Darken<T>(Func<ILocalWindow, T> showDialog)
        {
            if (null != _darkWindow)
                return showDialog(App.LocalMainWindow);

            _darkWindow = WithMainWindow(mainWindow =>
            {
                var rc = Win32Screen.GetWindowRect(mainWindow);

                return new DarkWindow
                {
                    Left = rc.Left,
                    Top = rc.Top,
                    Width = rc.Width,
                    Height = rc.Height,
                };
            });

            var retVal = default(T);

            if (null != _darkWindow)
                retVal = _darkWindow.ShowDialog(showDialog);

            _darkWindow = null;
            MainWindow.WithMainWindow(mainWindow => mainWindow.Activate());
            return retVal;
        }

        internal void
            ShowLinks(bool bHidden = false)
        {
            if (bHidden)
            {
                while (MenuLinkGroups.Count > 1)
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
