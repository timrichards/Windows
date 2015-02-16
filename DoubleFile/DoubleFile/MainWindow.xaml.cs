using System;                       // release mode
using System.Collections.Generic;
using System.IO;
using System.Runtime.Hosting;       // release mode
using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    partial class MainWindow
    {
        public MainWindow()
        {
            gd_old = new GlobalData(this);
            InitializeComponent();
        }

        internal LV_ProjectVM LVprojectVM { get; private set; }
        internal FormAnalysis_DirList Analysis_DirListForm { get; private set; }

        void FormAnalysis_DirListAction(Action<FormAnalysis_DirList, LV_ProjectVM> action)
        {
            if ((Analysis_DirListForm == null) || Analysis_DirListForm.IsDisposed)
            {
                return;
            }

            action(Analysis_DirListForm, new LV_ProjectVM(gd, LVprojectVM));
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            gd = new GlobalData_Window(this);

#if (DEBUG)
            //#warning DEBUG is defined.
            MBoxStatic.Assert(0, System.Diagnostics.Debugger.IsAttached, "Debugger is not attached!");
#else
            if (MBoxStatic.Assert(0, (System.Diagnostics.Debugger.IsAttached == false), "Debugger is attached but DEBUG is not defined.") == false)
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

        void ShowProjectWindow(bool bOpenProject = false)
        {
            WinProject volumes = null;

            if (bOpenProject)
            {
                if ((LVprojectVM != null) && LVprojectVM.Unsaved &&
                    (MessageBoxResult.Cancel ==
                    MBoxStatic.ShowDialog(WinProjectVM.ksUnsavedWarning, "Open Project", MessageBoxButton.OKCancel)))
                {
                    return;
                }

                volumes = new WinProject(gd);
            }
            else
            {
                volumes = new WinProject(gd, LVprojectVM ?? new LV_ProjectVM());
            }

            if (false == (volumes.ShowDialog() ?? false))
            {
                return;
            }

            LVprojectVM = volumes.LVprojectVM;

            if (LVprojectVM != null)
            {
                FormAnalysis_DirListAction(FormAnalysis_DirList.RestartTreeTimer);
                new SaveListingsProcess(gd, LVprojectVM);
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if ((LVprojectVM != null) && LVprojectVM.Unsaved &&
                (MessageBoxResult.Cancel == 
                MBoxStatic.ShowDialog(WinProjectVM.ksUnsavedWarning, "Quit Double File", MessageBoxButton.OKCancel)))
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

            gd_old.Dispose();
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
            }
            else
            {
                Analysis_DirListForm.Activate();
            }
        }

        private void Button_FileHashExplorer_Click(object sender, RoutedEventArgs e)
        {
            new WinAnalysis_FileHash(gd, LVprojectVM).Show();
        }

        GlobalData_Base gd = null;
        readonly GlobalData gd_old = null;

        private void Button_GCcollect_Click(object sender, RoutedEventArgs e)
        {
            GC.Collect();
        }
    }
}
