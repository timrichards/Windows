using System.Collections.Generic;
using System.Windows;

namespace DoubleFile
{
    partial class WinProjectVM
    {
        // Menu items
        
        const string ksAllFilesFilter = "|All files|*.*";
        const string ksProjectFilter = "Double File project|*." + FileParse.ksFileExt_Project + ksAllFilesFilter;
        internal const string ksListingFilter = "Double File Listing|*." + FileParse.ksFileExt_Listing + ksAllFilesFilter;
        internal const string ksUnsavedWarning = "You are about to lose changes to an unsaved project.";

        internal void OpenProject()
        {
            if (m_lvVM.Unsaved &&
                (MessageBoxResult.Cancel ==
                MBox.ShowDialog(ksUnsavedWarning, "Open Project", MessageBoxButton.OKCancel)))
            {
                return;
            }

            var dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.Title = "Open Project";
            dlg.Filter = ksProjectFilter;

            if (dlg.ShowDialog() ?? false)
            {
                new ProjectFile().OpenProject(dlg.FileName, OpenListingFiles);
            }
        }

        internal void SaveProject()
        {
            SaveProject(m_lvVM);
            m_lvVM.Unsaved = false;
        }

        static internal void SaveProject(LV_ProjectVM lvProjectVM)
        {
            string strFilename = null;

            while (true)
            {
                var dlg = new Microsoft.Win32.SaveFileDialog();

                dlg.Title = "Save Project";
                dlg.Filter = ksProjectFilter;
                dlg.FileName = strFilename;
                dlg.OverwritePrompt = false;

                if (dlg.ShowDialog() ?? false)
                {
                    strFilename = dlg.FileName;

                    if (System.IO.File.Exists(strFilename))
                    {
                        MBox.ShowDialog("Project file exists. Please manually delete it using the Save Project dialog after this alert closes.", "Save Project");
                        continue;
                    }
                    else
                    {
                        lvProjectVM.Unsaved = (false == new ProjectFile().SaveProject(lvProjectVM, strFilename));
                    }
                }

                break;
            }
        }

        internal void NewListingFile()
        {
            var lvItemVolumeTemp = new LVitem_ProjectVM(new string[] { });

            while (true)
            {
                var newVolume = new WinVolumeNew();

                newVolume.LVitemVolumeTemp = new LVitem_ProjectVM(lvItemVolumeTemp);

                if ((newVolume.ShowDialog() ?? false) == false)
                {
                    // user cancelled
                    break;
                }

                if ((false == m_lvVM.AlreadyInProject(null, newVolume.LVitemVolumeTemp.ListingFile)) &&
                    (false == m_lvVM.FileExists(newVolume.LVitemVolumeTemp.ListingFile)))
                {
                    m_lvVM.NewItem(newVolume.LVitemVolumeTemp);
                    break;
                }

                lvItemVolumeTemp = new LVitem_ProjectVM(newVolume.LVitemVolumeTemp);
            }
        }

        internal void OpenListingFile()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.Title = "Open Listing File";
            dlg.Filter = ksListingFilter;
            dlg.Multiselect = true;

            if (dlg.ShowDialog() ?? false)
            {
                OpenListingFiles(dlg.FileNames);
            }
        }

        internal void OpenListingFiles(IEnumerable<string> listFiles, bool bClearItems = false)
        {
            LVitem_ProjectVM lvItem = null;
            var sbBadFiles = new System.Text.StringBuilder();
            bool bMultiBad = true;

            if (bClearItems)
            {
                m_lvVM.Items.Clear();
            }

            foreach (var fileName in listFiles)
            {
                if (FileParse.ReadHeader(fileName, out lvItem))
                {
                    m_lvVM.NewItem(lvItem, bFromDisk: true);
                }
                else
                {
                    bMultiBad = (sbBadFiles.Length > 0);
                    sbBadFiles.Append("• ").Append(System.IO.Path.GetFileName(fileName)).Append("\n");
                }
            }

            if (sbBadFiles.Length > 0)
            {
                MBox.ShowDialog("Bad listing file" + (bMultiBad ? "s" : "") + ".\n" + sbBadFiles, "Open Listing File");
            }
        }
    }
}
