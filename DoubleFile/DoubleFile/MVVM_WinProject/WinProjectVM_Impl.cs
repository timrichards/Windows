﻿using System.Collections.Concurrent;
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
        
        internal static string
            ListingFilter { get { return "Double File Listing|*." + FileParse.ksFileExt_Listing + _ksAllFilesFilter; } }
        const string _ksProjectFilter = "Double File project|*." + FileParse.ksFileExt_Project + _ksAllFilesFilter;
        const string _ksAllFilesFilter = "|All files|*.*";

        internal static string
            UnsavedWarning { get { return "You are about to lose changes to an unsaved project."; } }

        internal void OpenProject()
        {
            if (_lvVM.Unsaved &&
                (MessageBoxResult.Cancel ==
                MBoxStatic.ShowDialog(UnsavedWarning, "Open Project", MessageBoxButton.OKCancel)))
            {
                return;
            }

            var dlg = new Microsoft.Win32.OpenFileDialog {Title = "Open Project", Filter = _ksProjectFilter};

            if (dlg.ShowDialog() ?? false)
            {
                new ProjectFile().OpenProject(dlg.FileName,
                    (listFiles, bClearItems, userCancelled) => OpenListingFiles(listFiles, bClearItems, userCancelled));
                _lvVM.Unsaved = false;
            }
        }

        internal void SaveProject()
        {
            SaveProject(_lvVM);
        }

        static internal void SaveProject(LV_ProjectVM lvProjectVM)
        {
            string strFilename = null;

            while (true)
            {
                var dlg = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Save Project",
                    Filter = _ksProjectFilter,
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

                if ((false == _lvVM.AlreadyInProject(null, newVolume.LVitemVolumeTemp.ListingFile)) &&
                    (false == _lvVM.FileExists(newVolume.LVitemVolumeTemp.ListingFile)))
                {
                    _lvVM.NewItem(newVolume.LVitemVolumeTemp);
                    _lvVM.Unsaved = true;
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
                Filter = ListingFilter,
                Multiselect = true
            };

            if ((dlg.ShowDialog() ?? false) &&
                OpenListingFiles(dlg.FileNames))
            {
                _lvVM.Unsaved = true;
            }
        }

        internal bool OpenListingFiles(
            IEnumerable<string> listFiles,
            bool bClearItems = false,
            System.Func<bool> userCancelled = null)
        {
            var sbBadFiles = new System.Text.StringBuilder();
            var bMultiBad = true;
            var listItems = new ConcurrentBag<LVitem_ProjectVM>();

            if (false == bClearItems)
            {
                foreach (var lvItem in _lvVM.ItemsCast)
                {
                    listItems.Add(lvItem);
                }
            }

            Parallel.ForEach(listFiles, strFilename =>
            {
                if ((null != userCancelled) && userCancelled())
                {
                    return;
                }

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

            if ((null != userCancelled) && userCancelled())
            {
                return false;
            }

            UtilProject.UIthread(_lvVM.Items.Clear);

            var bOpenedFiles = listItems
                .OrderBy(lvItem => lvItem.SourcePath)
                .Aggregate(false, (current, lvItem) =>
            {
                if ((null != userCancelled) && userCancelled())
                {
                    return false;
                }

                return (bool)UtilProject.UIthread(() => _lvVM.NewItem(lvItem)) || current;
            });

            if (sbBadFiles.Length > 0)
            {
                MBoxStatic.ShowDialog("Bad listing file" + (bMultiBad ? "s" : "") + ".\n" + sbBadFiles, "Open Listing File");
            }

            return bOpenedFiles;
        }
    }
}
