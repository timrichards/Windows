﻿using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using System.IO;
using System.Text;
using System;

namespace DoubleFile
{
    partial class WinProjectVM : IOpenListingFiles
    {
        // Menu items       
        static internal string
            ListingFilter { get { return "Double File Listing|*." + FileParse.ksFileExt_Listing + _ksAllFilesFilter; } }
        const string _ksProjectFilter = "Double File project|*." + FileParse.ksFileExt_Project + _ksAllFilesFilter;
        const string _ksAllFilesFilter = "|All files|*.*";

        static internal string
            UnsavedWarning { get { return "You are about to lose changes to an unsaved project."; } }

        internal void OpenProject()
        {
            if (_lvVM.Unsaved &&
                (MessageBoxResult.Cancel ==
                MBoxStatic.ShowDialog(UnsavedWarning, "Open Project", MessageBoxButton.OKCancel)))
            {
                return;
            }

            var dlg = new Microsoft.Win32.OpenFileDialog { Title = "Open Project", Filter = _ksProjectFilter };

            if (dlg.ShowDialog() ?? false)
            {
                ProjectFile.OpenProject(dlg.FileName, new WeakReference<IOpenListingFiles>(this));

                _lvVM.Unsaved = false;
                _lvVM.SetModified();
            }
        }

        internal void SaveProject()
        {
            SaveProject(_lvVM);
        }

        static internal void SaveProject(LV_ProjectVM lvProjectVM)
        {
            string strFilename = null;

            for (; ; )
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

                    if (File.Exists(strFilename))
                    {
                        MBoxStatic.ShowDialog("Project file exists. Please manually delete it using the Save Project dialog after this alert closes.", "Save Project");
                        continue;
                    }

                    // if it's saved, don't set it to unsaved if SaveProject() bails.
                    if (ProjectFile.SaveProject(lvProjectVM, strFilename))
                        lvProjectVM.Unsaved = false;
                }

                break;
            }
        }

        internal void NewListingFile()
        {
            var lvItemVolumeTemp = new LVitem_ProjectVM();

            for (; ; )
            {
                var newVolume = new WinVolumeNew { LVitemVolumeTemp = new LVitem_ProjectVM(lvItemVolumeTemp) };

                if (false == (newVolume.ShowDialog() ?? false))
                {
                    // user canceled
                    break;
                }

                if ((false == _lvVM.AlreadyInProject(newVolume.LVitemVolumeTemp.ListingFile)) &&
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
            System.Func<bool> userCanceled = null)
        {
            var sbBadFiles = new StringBuilder();
            var bMultiBad = true;
            var sbAlreadyInProject = new StringBuilder();
            var listItems = new ConcurrentBag<LVitem_ProjectVM>();

            if (false == bClearItems)
            {
                foreach (var lvItem in _lvVM.ItemsCast)
                    listItems.Add(lvItem);
            }

            Util.UIthread(_lvVM.ClearItems);

            Parallel.ForEach(listFiles, strFilename =>
            {
                if ((null != userCanceled) &&
                    userCanceled())
                {
                    return;   // from lambda
                }

                if (_lvVM.AlreadyInProject(strFilename, bQuiet: true))
                {
                    lock (sbAlreadyInProject)
                        sbAlreadyInProject.Append("• ").Append(System.IO.Path.GetFileName(strFilename)).Append("\n");

                    return;   // from lambda
                }

                LVitem_ProjectVM lvItem = null;

                if (FileParse.ReadHeader(strFilename, out lvItem))
                {
                    listItems.Add(lvItem);
                }
                else
                {
                    bMultiBad = (0 < sbBadFiles.Length);

                    lock (sbBadFiles)
                        sbBadFiles.Append("• ").Append(System.IO.Path.GetFileName(strFilename)).Append("\n");
                }
            });

            if ((null != userCanceled) &&
                userCanceled())
            {
                return false;
            }

            var bOpenedFiles = listItems
                .OrderBy(lvItem => lvItem.SourcePath)
                .Aggregate(false, (current, lvItem) =>
            {
                if ((null != userCanceled) &&
                    userCanceled())
                {
                    return false;   // from lambda
                }

                return Util.UIthread(() => _lvVM.NewItem(lvItem)) || current;  // from lambda
            });

            var sbError = new StringBuilder();

            if (0 < sbBadFiles.Length)
                sbError.Append("Bad listing file" + (bMultiBad ? "s" : "") + ".\n\n" + sbBadFiles);

            if (0 < sbAlreadyInProject.Length)
                sbError.Append("Already in project.\n\n" + sbAlreadyInProject);

            if (0 < sbError.Length)
                MBoxStatic.ShowDialog("" + sbError, "Open Listing File");

            return bOpenedFiles;
        }

        void IOpenListingFiles.Callback(IEnumerable<string> lsFiles, bool bClearItems, Func<bool> userCanceled)
        {
            OpenListingFiles(lsFiles, bClearItems, userCanceled);
        }
    }
}
