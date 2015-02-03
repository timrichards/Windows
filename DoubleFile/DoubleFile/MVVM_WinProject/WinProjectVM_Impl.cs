using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
                    gd.FileDictionary.Clear();
                    m_lvVM.Unsaved = true;
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
                m_lvVM.Unsaved = true;
            }
        }

        internal void OpenListingFiles(IEnumerable<string> listFiles, bool bClearItems = false)
        {
            var sbBadFiles = new System.Text.StringBuilder();
            bool bMultiBad = true;

            if (bClearItems)
            {
                m_lvVM.Items.Clear();
            }

            var listItems = new ConcurrentBag<LVitem_ProjectVM>();

            //foreach (var test in listFiles.Select(async strFilename => await Task.Run(() =>
            //{
            //    LVitem_ProjectVM lvItem = null;

            //    if (FileParse.ReadHeader(strFilename, out lvItem))
            //        listItems.Add(lvItem);
            //}))) { UtilProject.WriteLine("a"); }

            {
                var thread = new Thread(new ThreadStart(() =>
                {
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
                }));

                thread.Start();
                thread.Join();
            }

            foreach (var lvItem in listItems)
            {
                m_lvVM.NewItem(lvItem);
            }

            if (sbBadFiles.Length > 0)
            {
                MBox.ShowDialog("Bad listing file" + (bMultiBad ? "s" : "") + ".\n" + sbBadFiles, "Open Listing File");
            }
        }
    }
}
