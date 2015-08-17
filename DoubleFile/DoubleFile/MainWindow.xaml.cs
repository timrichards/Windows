using FirstFloor.ModernUI.Presentation;
using System;
using System.Reactive.Linq;
using System.Windows;
using System.Reactive;
using System.ComponentModel;
using System.Diagnostics;           // DEBUG
using System.Linq;
using System.Collections.Generic;
using System.IO;                    // false == DEBUG

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        class MyLink : Link
        {
            internal MyLink(string strDisplayName, string strSource)
            {
                DisplayName = strDisplayName;
                Source = new Uri(strSource, UriKind.Relative);
            }
        }

        class FolderListLink : Link
        {
            internal FolderListLink(KeyValuePair<string, string> kvp)
            {
                if (false == _bFolderListLinkCheck)
                {
                    Util.Assert(99886, 3 == WinFolderList.FolderListFragments.Count);
                    _bFolderListLinkCheck = true;
                }

                Source = new Uri("/Win/WinFolderList.xaml#" + kvp.Key, UriKind.Relative);
                DisplayName = kvp.Value;
            }
        }
        static bool _bFolderListLinkCheck = false;

        class ClonesLink : Link
        {
            internal ClonesLink(KeyValuePair<string, string> kvp)
            {
                if (false == _bFormsLV_LinkCheck)
                {
                    Util.Assert(99782, 3 == WinClones.FolderListFragments.Count);
                    _bFormsLV_LinkCheck = true;
                }

                Source = new Uri("/Win/WinClones.xaml#" + kvp.Key, UriKind.Relative);
                DisplayName = kvp.Value;
            }
        }
        static bool _bFormsLV_LinkCheck = false;

        internal const string SaveListingsFakeKey = "/SaveListings.xaml";
        static readonly Link _saveListingsLink = new MyLink("Save listings", SaveListingsFakeKey);

        // use-case for public: VolTreeMap project
        internal const string ExtraWindowFakeKey = "/ExtraWindow.xaml";
        public static readonly Link _extraWindowLink = new MyLink("Extra window", ExtraWindowFakeKey);

        internal const string AdvancedFakeKey = "/Advanced.xaml";
        static readonly Link _advancedLink = new MyLink("Advanced", AdvancedFakeKey);

        static Action Init = null;
        static void InitForMainWindowOnly(Action init) => Init = init;
        public
            MainWindow()
            : base(InitForMainWindowOnly)
        {
            Statics.Icon = Icon;
            _mainWindowWR.SetTarget(this);
            Init();
            Init = null;
            InitializeComponent();
            //Util.Assert(0, false);      // test ability to assert from the primordial soup

            Observable.FromEventPattern(this, "Loaded")
                .LocalSubscribe(99760, Window_Loaded);

            Observable.FromEventPattern<CancelEventArgs>(this, "Closing")
                .LocalSubscribe(99759, args => MainWindow_Closing(args.EventArgs));

            MenuLinkGroups.Add(new LinkGroup { DisplayName="Welcome", Links =
            {
                new MyLink("View project", "/WinProject/WinProject.xaml"),
                new MyLink("Introduction", "/Introduction.xaml")
            }});
        }

        internal void UpdateTitleLinks(bool? bListingsToSave = null)
        {
            TitleLinks = new LinkCollection();

            if (null != bListingsToSave)
                _bListingsToSave = bListingsToSave.Value;

            if (_bListingsToSave)
                TitleLinks.Add(_saveListingsLink);

            var currentPage = Statics.CurrentPage;

            if (currentPage is WinProject)
                return;

            if (currentPage is Introduction)
                return;

            TitleLinks.Add(_extraWindowLink);
#if DEBUG
            TitleLinks.Add(_advancedLink);
#endif
        }

        static internal void
            WithMainWindowA(Action<MainWindow> doSomethingWith)
        {
            WithMainWindow(mainWindow =>
            {
                doSomethingWith(mainWindow);
                return false;   // from lambda; no-op
            });
        }

        static internal T
            WithMainWindow<T>(Func<MainWindow, T> doSomethingWith)
        {
            if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true))
                return default(T);

            // use-case: assert before main window class allocated
            if (null == _mainWindowWR)
                return default(T);

            return _mainWindowWR.Get(mainWindow =>
            {
                if (mainWindow?.LocalIsClosed ?? true)
                {
                    Util.Assert(99856, false);
                    return default(T);      // from lambda
                }

                return doSomethingWith(mainWindow);      // from lambda
            });
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

        void Window_Loaded(EventPattern<object> obj)
        {
#if (DEBUG)
            //#warning DEBUG is defined.
            Util.Assert(99998, Debugger.IsAttached, "Debugger is not attached!");
#else
            if (Util.Assert(99997, (System.Diagnostics.Debugger.IsAttached == false), "Debugger is attached but DEBUG is not defined.") == false)
                return;

            var arrArgs = AppDomain.CurrentDomain.SetupInformation.ActivationArguments?.ActivationData;

            // scenario: launched from Start menu
            if (null == arrArgs)                
                return;

            if (false == Util.Assert(1308.93165m, 0 < arrArgs.Length))
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

        void MainWindow_Closing(CancelEventArgs e)
        {
            if ((Statics.WithLVprojectVM(p => p?.Unsaved ?? false)) &&
                (MessageBoxResult.Cancel == 
                MBoxStatic.ShowDialog(WinProjectVM.UnsavedWarning, "Quit Double File", MessageBoxButton.OKCancel)))
            {
                e.Cancel = true;
            }
        }

        static readonly IReadOnlyList<LinkGroup>
            _links = new[]
        {
            new LinkGroup { DisplayName="Explore", Links =
            {
                new MyLink("Tree map", "/Win/WinTreeMap.xaml"),
                new MyLink("Folders", "/Win/WinTreeView.xaml"),
                new MyLink("Tree list", "/Win/WinTreeList.xaml")
            }},
            new LinkGroup { DisplayName="Variance", Links =
            {
                new FolderListLink(WinFolderList.FolderListFragments.ElementAt(0)),
                new FolderListLink(WinFolderList.FolderListFragments.ElementAt(1)),
                new FolderListLink(WinFolderList.FolderListFragments.ElementAt(2)),
            }},
            new LinkGroup { DisplayName="Clones", Links =
            {
                new ClonesLink(WinClones.FolderListFragments.ElementAt(0)),
                new ClonesLink(WinClones.FolderListFragments.ElementAt(1)),
                new ClonesLink(WinClones.FolderListFragments.ElementAt(2))
            }},
            new LinkGroup { DisplayName="Files", Links =
            {
                new MyLink("Files in folder", "/Win/WinFiles.xaml"),
                new MyLink("Duplicates", "/Win/WinDuplicates.xaml")
            }},
            new LinkGroup { DisplayName="Search", Links =
            {
                new MyLink("Search", "/Win/WinSearch.xaml")
            }},
            new LinkGroup { DisplayName="Detailed info", Links =
            {
                new MyLink("Detailed info", "/Win/WinDetail.xaml")
            }}
        };

        bool
            _bListingsToSave = false;
        static readonly WeakReference<MainWindow>
            _mainWindowWR = new WeakReference<MainWindow>(null);
    }
}
