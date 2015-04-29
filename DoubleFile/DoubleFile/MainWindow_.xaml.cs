using System;               // release mode
using System.IO;
using System.Reactive.Linq;
using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>

    // MainWindow_Closing has gd_old.Dispose();
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    partial class MainWindow
    {
        internal LV_ProjectVM
            LVprojectVM { get; private set; }

        static internal FileDictionary
            FileDictionary { get { var o = _weakReference.Target as MainWindow; return (null != o) ? o._fileDictionary : null; } }
        FileDictionary _fileDictionary = new FileDictionary();

        static internal SaveDirListings
            SaveDirListings
        {
            get { var o = _weakReference.Target as MainWindow; return (null != o) ? o._saveDirListings : null; }
            set { var o = _weakReference.Target as MainWindow; if (null != o) o._saveDirListings = value; }
        }
        SaveDirListings _saveDirListings = null;

        static internal MainWindow
            Instance { get { var o = _weakReference.Target as MainWindow; return (null != o) ? o : null; } }

        static internal LocalWindow
            TopWindow
        {
            get { var o = _weakReference.Target as MainWindow; return (null != o) ? o._topWindow : null; }
            set { var o = _weakReference.Target as MainWindow; if (null != o) o._topWindow = value; }
        }
        LocalWindow _topWindow = null;

        static internal LocalWindow
            LastPlacementWindow
        {
            get { var o = _weakReference.Target as MainWindow; return (null != o) ? o._lastPlacementWindow : null; }
            set { var o = _weakReference.Target as MainWindow; if (null != o) o._lastPlacementWindow = value; }
        }
        LocalWindow _lastPlacementWindow = null;

        static internal LocalWindow
            LeftWindow
        {
            get { var o = _weakReference.Target as MainWindow; return (null != o) ? o._leftWindow : null; }
            set { var o = _weakReference.Target as MainWindow; if (null != o) o._leftWindow = value; }
        }
        LocalWindow _leftWindow = null;

        static Action Init = null;
        static void InitForMainWindowOnly(Action init) { Init = init; }
        public
            MainWindow()
            : base(InitForMainWindowOnly)
        {
            _weakReference = new WeakReference(this);
            Init();
            Init = null;
            SizeToContent = SizeToContent.WidthAndHeight;
            ResizeMode = ResizeMode.CanMinimize;
            InitializeComponent();

            Observable.FromEventPattern(form_grid, "Loaded")
                .Subscribe(args => Grid_Loaded());

            Observable.FromEventPattern<System.ComponentModel.CancelEventArgs>(this, "Closing")
                .Subscribe(args => MainWindow_Closing(args.EventArgs));

            Observable.FromEventPattern(form_btnViewProject, "Click")
                .Subscribe(args => ShowProjectWindow());

            Observable.FromEventPattern(form_btnOpenProject, "Click")
                .Subscribe(args => ShowProjectWindow(bOpenProject: true));

            Observable.FromEventPattern(form_btnSaveProject, "Click")
                .Subscribe(args => Button_SaveProject_Click());

            Observable.FromEventPattern(form_btnDuplicateFileExplorer, "Click")
                .Subscribe(args => Button_DuplicateFileExplorer_Click());

            Observable.FromEventPattern(form_btnNewline, "Click")
                .Subscribe(args => UtilProject.WriteLine());

            Observable.FromEventPattern(this, "MouseDown")
                .Subscribe(args => DragMove());

            Observable.FromEventPattern(this, "StateChanged")     // app minimize
                .Subscribe(args => WinTooltip.CloseTooltip());
        }

        void ShowProjectWindow(bool bOpenProject = false)
        {
            WinProject volumes = null;

            if (bOpenProject)
            {
                if ((null != LVprojectVM) &&
                    LVprojectVM.Unsaved &&
                    (MessageBoxResult.Cancel ==
                    MBoxStatic.ShowDialog(WinProjectVM.UnsavedWarning, "Open Project", MessageBoxButton.OKCancel)))
                {
                    return;
                }

                volumes = new WinProject(bOpenProject: true);
            }
            else
            {
                volumes = new WinProject(LVprojectVM);
            }

            if (false == (volumes.ShowDialog() ?? false))
                return;

            if (null == volumes.LVprojectVM)
                return;

            if (volumes.LVprojectVM.LocalEquals(LVprojectVM))
                return;

            LVprojectVM = volumes.LVprojectVM;
            new SaveListingsProcess(LVprojectVM);
            _fileDictionary.Clear();

            if ((null != _winDoubleFile_Folders) &&
                (false == _winDoubleFile_Folders.LocalIsClosed))
            {
                _winDoubleFile_Folders.Close();

                (_winDoubleFile_Folders = new WinDoubleFile_Folders(LVprojectVM))
                    .Show();

                Observable.FromEventPattern(_winDoubleFile_Folders, "Closed")
                    .Subscribe(args => _winDoubleFile_Folders = null);
            }
        }

        #region form_handlers
        private void Grid_Loaded()
        {
            MinWidth = Width;
            MinHeight = Height;
#if (DEBUG)
            //#warning DEBUG is defined.
            MBoxStatic.Assert(99998, System.Diagnostics.Debugger.IsAttached, "Debugger is not attached!");
#else
            if (MBoxStatic.Assert(99997, (System.Diagnostics.Debugger.IsAttached == false), "Debugger is attached but DEBUG is not defined.") == false)
            {
                return;
            }

            var args = AppDomain.CurrentDomain.SetupInformation.ActivationArguments;

            if (null == args)
            {
                return;
            }

            var arrArgs = args.ActivationData;

            if (arrArgs == null)
            {
                // scenario: launched from Start menu
                return;
            }

            if (MBoxStatic.Assert(1308.93165, arrArgs.Length > 0) == false)
            {
                return;
            }

            var strFile = arrArgs[0];

            if (strFile.Length < 2)
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
                    //m_blinky.Go(form_lvCopyScratchpad, clr: Color.Yellow, Once: true);
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

        private void MainWindow_Closing(System.ComponentModel.CancelEventArgs e)
        {
            if ((null != LVprojectVM) &&
                LVprojectVM.Unsaved &&
                (MessageBoxResult.Cancel == 
                MBoxStatic.ShowDialog(WinProjectVM.UnsavedWarning, "Quit Double File", MessageBoxButton.OKCancel)))
            {
                e.Cancel = true;
                return;
            }

            _weakReference.Target = null;
            _fileDictionary.Dispose();

            if (Directory.Exists(ProjectFile.TempPath))
            {
                try { Directory.Delete(ProjectFile.TempPath, true); }
                catch { }
            }

            if (Directory.Exists(ProjectFile.TempPath01))
                Directory.Delete(ProjectFile.TempPath01, true);
        }

        private void Button_SaveProject_Click()
        {
            if (null != LVprojectVM)
                WinProjectVM.SaveProject(LVprojectVM);
            else
                MBoxStatic.ShowDialog("No project to save.", "Save Project");
        }

        private void Button_DuplicateFileExplorer_Click()
        {
            if ((null == _winDoubleFile_Folders) || (_winDoubleFile_Folders.LocalIsClosed))
            {
                (_winDoubleFile_Folders = new WinDoubleFile_Folders(LVprojectVM))
                    .Show();

                Observable.FromEventPattern(_winDoubleFile_Folders, "Closed")
                    .Subscribe(args => _winDoubleFile_Folders = null);
            }
            else if (false == _winDoubleFile_Folders.ShowWindows())       // returns true if it created a window
            {
                _winDoubleFile_Folders.Activate();                        // UX feedback
            }
        }
        #endregion form_handlers

        WinDoubleFile_Folders
            _winDoubleFile_Folders = null;
        static WeakReference
            _weakReference = null;
    }
}
