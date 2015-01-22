using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    partial class MainWindow : LocalWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            GlobalData.GetInstance(this);
        }

        internal IEnumerable<LVitem_ProjectVM> ListLVvolStrings { get; private set; }
        internal FormAnalysis_DirList Analysis_DirListForm { get; private set; }

        void FormAnalysis_DirListAction(System.Action<FormAnalysis_DirList, IEnumerable<LVitem_ProjectVM>> action)
        {
            if ((Analysis_DirListForm == null) || (Analysis_DirListForm.IsDisposed))
            {
                return;
            }

            action(Analysis_DirListForm, ListLVvolStrings);
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
#if (DEBUG)
            //#warning DEBUG is defined.
            MBox.Assert(0, System.Diagnostics.Debugger.IsAttached, "Debugger is not attached!");
#else
            if (MBox.Assert(0, (System.Diagnostics.Debugger.IsAttached == false), "Debugger is attached but DEBUG is not defined.") == false)
            {
                return;
            }

            ActivationArguments args = AppDomain.CurrentDomain.SetupInformation.ActivationArguments;

            if (MBox.Assert(1308.93165, args != null) == false)
            {
                return;
            }

            string[] arrArgs = args.ActivationData;

            if (arrArgs == null)
            {
                // scenario: launched from Start menu
                return;
            }

            if (MBox.Assert(1308.93165, arrArgs.Length > 0) == false)
            {
                return;
            }

            string strFile = arrArgs[0];

            switch (Path.GetExtension(strFile).Substring(1))
            {
                case Utilities.ksFileExt_Listing:
                {
                    form_cbSaveAs.Text = strFile;
                    AddVolume();
                    form_tabControlMain.SelectedTab = form_tabPageBrowse;
                    RestartTreeTimer();
                    break;
                }

                case Utilities.ksFileExt_Volume:
                {
                    if (LoadVolumeList(strFile))
                    {
                        RestartTreeTimer();
                    }

                    break;
                }

                case Utilities.ksFileExt_Copy:
                {
                    form_tabControlMain.SelectedTab = form_tabPageBrowse;
                    form_tabControlCopyIgnore.SelectedTab = form_tabPageCopy;
                    m_blinky.Go(form_lvCopyScratchpad, clr: Color.Yellow, Once: true);
                    FormAnalysis_DirListMessageBox("The Copy scratchpad cannot be loaded with no directory listings.", "Load Copy scratchpad externally");
                    Application.Exit();
                    break;
                }

                case Utilities.ksFileExt_Ignore:
                {
                    LoadIgnoreList(strFile);
                    form_tabControlMain.SelectedTab = form_tabPageBrowse;
                    form_tabControlCopyIgnore.SelectedTab = form_tabPageIgnore;
                    break;
                }
            }
#endif
        }

        void ShowProjectWindow(bool bOpenProject = false)
        {
            var volumes = new WinProject(ListLVvolStrings, bOpenProject);

            if (false == (volumes.ShowDialog() ?? false))
            {
                return;
            }

            ListLVvolStrings = volumes.ListLVvolStrings;
            FormAnalysis_DirListAction(FormAnalysis_DirList.RestartTreeTimer);

            if (ListLVvolStrings != null)
            {
                new SaveListingsProcess(ListLVvolStrings);
            }
        }

        private void LocalWindow_Closed(object sender, System.EventArgs e)
        {
            if (Directory.Exists(ProjectFile.TempPath))
            {
                Directory.Delete(ProjectFile.TempPath, true);
            }

            if (Directory.Exists(ProjectFile.TempPath01))
            {
                Directory.Delete(ProjectFile.TempPath01, true);
            }
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
            if (ListLVvolStrings != null)
            {
                WinProjectVM.SaveProject(ListLVvolStrings);
            }
            else
            {
                MBox.ShowDialog("No project to save.", "Save Project");
            }
        }

        private void Button_SearchDirLists_Click(object sender, RoutedEventArgs e)
        {
            if ((Analysis_DirListForm == null) || (Analysis_DirListForm.IsDisposed))
            {
                (Analysis_DirListForm = new FormAnalysis_DirList(this, ListLVvolStrings)).Show();
            }
            else
            {
                Analysis_DirListForm.Activate();
            }
        }
    }
}
