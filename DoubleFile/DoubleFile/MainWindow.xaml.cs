﻿using FirstFloor.ModernUI.Presentation;
using System;
using System.IO;
using System.Reactive.Linq;
using System.Windows;
using System.Reactive;
using System.ComponentModel;
using System.Diagnostics;

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

        internal const string FolderListLarge = "large";
        internal const string FolderListSmall = "small";
        internal const string FolderListRandom = "random";
        internal const string FolderListUnique = "unique";
        internal const string FolderListSameVol = "sameVol";
        internal const string FolderListClones = "clones";
        static string FolderListUrlBuilder(string s = null) { return "/Win/WinFolderList.xaml#" + s; }

        internal const string SaveListingsFakeKey = "/SaveListings.xaml";
        static readonly Link _saveListingsLink = new MyLink("Save listings", SaveListingsFakeKey);

        internal const string ExtraWindowFakeKey = "/ExtraWindow.xaml";
        static readonly Link _extraWindowLink = new MyLink("Extra window", ExtraWindowFakeKey);

        internal const string AdvancedFakeKey = "/Advanced.xaml";
        static readonly Link _advancedLink = new MyLink("Advanced", AdvancedFakeKey);

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

            Observable.FromEventPattern<CancelEventArgs>(this, "Closing")
                .Subscribe(args => MainWindow_Closing(args.EventArgs));
        }

        static internal LocalUserControlBase
            CurrentPage
        {
            get { return WithMainWindow(mainWindow => mainWindow._currentPage); }
            set
            {
                WithMainWindowA(mainWindow =>
                {
                    if (value == mainWindow._currentPage)
                        return;     // from lambda

                    mainWindow._currentPage = value;
                    mainWindow.UpdateTitleLinks();
                });
            }
        }

        internal void UpdateTitleLinks(bool? bListingsToSave = null)
        {
            TitleLinks = new LinkCollection();

            if (null != bListingsToSave)
                _bListingsToSave = bListingsToSave.Value;

            if (_bListingsToSave)
                TitleLinks.Add(_saveListingsLink);

            if (_currentPage is WinProject)
                return;

            if (_currentPage is Introduction)
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
            MainWindow mainWindow = null;

            _mainWindowWR.TryGetTarget(out mainWindow);

            if (null == mainWindow)
            {
                MBoxStatic.Assert(99856, false);
                return default(T);
            }

            return doSomethingWith(mainWindow);
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
            MBoxStatic.Assert(99998, Debugger.IsAttached, "Debugger is not attached!");
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

            if (false == MBoxStatic.Assert(1308.93165m, 0 < arrArgs.Length))
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
                new MyLink("Tree map", "/Win/WinTreeMap.xaml"),
                new MyLink("Folders", "/Win/WinTreeView.xaml"),
                new MyLink("Tree list", "/Win/WinTreeList.xaml")
            }},
            new LinkGroup { DisplayName="Folder lists", Links =
            {
                new MyLink("ANOVA weighted large", FolderListUrlBuilder(FolderListLarge)),
                new MyLink("ANOVA weighted small", FolderListUrlBuilder(FolderListSmall)),
                new MyLink("ANOVA weighted random", FolderListUrlBuilder(FolderListRandom)),
                new MyLink("Unique", FolderListUrlBuilder(FolderListUnique)),
                new MyLink("Same volume", FolderListUrlBuilder(FolderListSameVol)),
                new MyLink("Clones", FolderListUrlBuilder(FolderListClones)),
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

        LocalUserControlBase
            _currentPage = null;
        bool
            _bListingsToSave = false;
        static readonly WeakReference<MainWindow>
            _mainWindowWR = new WeakReference<MainWindow>(null);
    }
}
