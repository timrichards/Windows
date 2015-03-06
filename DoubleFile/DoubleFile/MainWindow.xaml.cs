using System;
using System.IO;
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
        internal LV_ProjectVM LVprojectVM { get; private set; }
        internal FormAnalysis_DirList Analysis_DirListForm { get; private set; }

        public MainWindow()
            : base(bIsMainWindow: true)
        {
            _gd_old = new GlobalData(this);

            App.OnAppActivated += () =>
            {
                AppActivated = true;
            };

            InitializeComponent();
            form_grid.Loaded += Grid_Loaded;
            Closing += MainWindow_Closing;
            form_btnViewProject.Click += Button_ViewProject_Click;
            form_btnOpenProject.Click += Button_OpenProject_Click;
            form_btnSaveProject.Click += Button_SaveProject_Click;
            form_btnSearchDirLists.Click += Button_SearchDirLists_Click;
            form_btnDuplicateFileExplorer.Click += Button_DuplicateFileExplorer_Click;
        }

        void FormAnalysis_DirListAction(Action<FormAnalysis_DirList, LV_ProjectVM> action)
        {
            if ((null == Analysis_DirListForm) || Analysis_DirListForm.IsDisposed)
            {
                return;
            }

            action(Analysis_DirListForm, new LV_ProjectVM(_gd, LVprojectVM));
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

                volumes = new WinProject(_gd, bOpenProject: true);
            }
            else
            {
                volumes = new WinProject(_gd, LVprojectVM);
            }

            if (false == (volumes.ShowDialog() ?? false))
            {
                return;
            }

            if (null == volumes.LVprojectVM)
            {
                return;
            }

            if (volumes.LVprojectVM.Equals(LVprojectVM))
            {
                return;
            }

            LVprojectVM = volumes.LVprojectVM;

            new SaveListingsProcess(_gd, LVprojectVM);

            _gd.FileDictionary.Clear();
            FormAnalysis_DirListAction(FormAnalysis_DirList.RestartTreeTimer);

            if ((null != _winFileHash_Folders) && (false == _winFileHash_Folders.IsClosed))
            {
                _winFileHash_Folders.Close();
                (_winFileHash_Folders = new WinFileHash_Folders(_gd, LVprojectVM)).Show();
                _winFileHash_Folders.Closed += (o, a) => _winFileHash_Folders = null;
            }
        }

        #region form_handlers
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            _gd = new GlobalData_Window(this);

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
                    //FormAnalysis_DirListMessageBox("The Copy scratchpad cannot be loaded with no directory listings.", "Load Copy scratchpad externally");
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

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if ((null != LVprojectVM) &&
                LVprojectVM.Unsaved &&
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
            {
                Directory.Delete(ProjectFile.TempPath01, true);
            }

            _gd_old.Dispose();
        }

        private void Button_ViewProject_Click(object sender, RoutedEventArgs e)
        {
            ShowProjectWindow();
        }

        private void Button_OpenProject_Click(object sender, RoutedEventArgs e)
        {
            ShowProjectWindow(bOpenProject: true);
        }

        private void Button_SaveProject_Click(object sender, RoutedEventArgs e)
        {
            if (LVprojectVM != null)
            {
                WinProjectVM.SaveProject(LVprojectVM);
            }
            else
            {
                MBoxStatic.ShowDialog("No project to save.", "Save Project");
            }
        }

        private void Button_SearchDirLists_Click(object sender, RoutedEventArgs e)
        {
            if ((Analysis_DirListForm == null) || (Analysis_DirListForm.IsDisposed))
            {
                (Analysis_DirListForm = new FormAnalysis_DirList(this, LVprojectVM)).Show();
                Analysis_DirListForm.Closed += (o, a) => Analysis_DirListForm = null;
            }
            else
            {
                Analysis_DirListForm.Activate();
            }
        }

        private void Button_DuplicateFileExplorer_Click(object sender, RoutedEventArgs e)
        {
            if ((null == _winFileHash_Folders) || (_winFileHash_Folders.IsClosed))
            {
                (_winFileHash_Folders = new WinFileHash_Folders(_gd, LVprojectVM)).Show();
                _winFileHash_Folders.Closed += (o, a) => _winFileHash_Folders = null;
            }
            else if (false == _winFileHash_Folders.ShowWindows())       // returns true if it created a window
                _winFileHash_Folders.Activate();                        // UX feedback
        }
        #endregion form_handlers

        WinFileHash_Folders
            _winFileHash_Folders = null;
        GlobalData_Base
            _gd = null;
        readonly GlobalData
            _gd_old = null;
    }
}
