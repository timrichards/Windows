using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;

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
                MBoxStatic.ShowDialog(ksUnsavedWarning, "Open Project", MessageBoxButton.OKCancel)))
            {
                return;
            }

            var dlg = new Microsoft.Win32.OpenFileDialog {Title = "Open Project", Filter = ksProjectFilter};

            if (dlg.ShowDialog() ?? false)
            {
                new ProjectFile().OpenProject(dlg.FileName,
                    (listFiles, bClearItems) => OpenListingFiles(listFiles, bClearItems));
                m_lvVM.Unsaved = false;
                // gd.FileDictionary.Clear();   Don't call this here. FileDictionary subscribes to the OnOpenedProject event.
            }
        }

        internal void SaveProject()
        {
            SaveProject(m_lvVM);
        }

        static internal void SaveProject(LV_ProjectVM lvProjectVM)
        {
            string strFilename = null;

            while (true)
            {
                var dlg = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Save Project",
                    Filter = ksProjectFilter,
                    FileName = strFilename,
                    OverwritePrompt = false
                };

                if (dlg.ShowDialog() ?? false)
                {
                    strFilename = dlg.FileName;

                    if (System.IO.File.Exists(strFilename))
                    {
                        MBoxStatic.ShowDialog("Project file exists. Please manually delete it using the Save Project dialog after this alert closes.", "Save Project");
                        continue;
                    }

                    // if it's saved, don't set it to unsaved if SaveProject() bails.
                    if (new ProjectFile().SaveProject(lvProjectVM, strFilename))
                        lvProjectVM.Unsaved = false;
                }

                break;
            }
        }

        internal void NewListingFile()
        {
            var lvItemVolumeTemp = new LVitem_ProjectVM(new string[] { });

            while (true)
            {
                var newVolume = new WinVolumeNew {LVitemVolumeTemp = new LVitem_ProjectVM(lvItemVolumeTemp)};

                if ((newVolume.ShowDialog() ?? false) == false)
                {
                    // user cancelled
                    break;
                }

                if ((false == m_lvVM.AlreadyInProject(null, newVolume.LVitemVolumeTemp.ListingFile)) &&
                    (false == m_lvVM.FileExists(newVolume.LVitemVolumeTemp.ListingFile)))
                {
                    m_lvVM.NewItem(newVolume.LVitemVolumeTemp);
                    gd.FileDictionary.Clear();
                    m_lvVM.Unsaved = true;
                    break;
                }

                lvItemVolumeTemp = new LVitem_ProjectVM(newVolume.LVitemVolumeTemp);
            }
        }

        internal void OpenListingFile()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Open Listing File",
                Filter = ksListingFilter,
                Multiselect = true
            };

            if ((dlg.ShowDialog() ?? false) &&
                OpenListingFiles(dlg.FileNames))
            {
                gd.FileDictionary.Clear();
                m_lvVM.Unsaved = true;
            }
        }

        internal bool OpenListingFiles(IEnumerable<string> listFiles, bool bClearItems = false)
        {
            var sbBadFiles = new System.Text.StringBuilder();
            var bMultiBad = true;
            var listItems = new ConcurrentBag<LVitem_ProjectVM>();

            if (false == bClearItems)
            {
                foreach (var lvItem in m_lvVM.ItemsCast)
                {
                    listItems.Add(lvItem);
                }
            }

            UtilProject.UIthread(() => m_lvVM.Items.Clear());

            Parallel.ForEach(listFiles, strFilename =>
            {
                LVitem_ProjectVM lvItem = null;

                if (FileParse.ReadHeader(strFilename, out lvItem))
                {
                    listItems.Add(lvItem);
                }
                else
                {
                    bMultiBad = (sbBadFiles.Length > 0);

                    lock (sbBadFiles)
                    {
                        sbBadFiles.Append("• ").Append(System.IO.Path.GetFileName(strFilename)).Append("\n");
                    }
                }
            });

            var bOpenedFiles = listItems
                .OrderBy(lvItem => lvItem.SourcePath)
                .Aggregate(false, (current, lvItem) => (bool)UtilProject.UIthread(() => m_lvVM.NewItem(lvItem)) || current);

            if (sbBadFiles.Length > 0)
            {
                MBoxStatic.ShowDialog("Bad listing file" + (bMultiBad ? "s" : "") + ".\n" + sbBadFiles, "Open Listing File");
            }

            return bOpenedFiles;
        }
    }
}
