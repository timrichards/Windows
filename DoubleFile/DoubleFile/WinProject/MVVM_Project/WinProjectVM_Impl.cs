using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Linq;
using System.IO;
using System.Text;
using System;
using System.Threading;
using System.Windows.Input;

namespace DoubleFile
{
    partial class WinProjectVM : IOpenListingFiles, IWinProgressClosing
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
            var bClearItems =
                (_lvVM.Count == 0) ||
                false == (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift));

            var strTitle = (bClearItems ? "Open" : "Append") + " Project";

            if (bClearItems && _lvVM.Unsaved &&
                (MessageBoxResult.Cancel ==
                MBoxStatic.ShowDialog(UnsavedWarning, strTitle, MessageBoxButton.OKCancel)))
            {
                return;
            }

            var dlg = new Microsoft.Win32.OpenFileDialog { Title = strTitle, Filter = _ksProjectFilter };

            if ((MainWindow.Darken(darkWindow => dlg.ShowDialog((Window)darkWindow)) ?? false) &&
                ProjectFile.OpenProject(dlg.FileName, new WeakReference<IOpenListingFiles>(this), bClearItems))
            {
                _lvVM.Unsaved =
                    false == bClearItems;

                if (bClearItems)
                    _lvVM.SetModified();    // Unsaved has trickery to do a UX reset only when set to true: want always
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

                if (MainWindow.Darken(darkWindow => dlg.ShowDialog((Window)darkWindow)) ?? false)
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

                lvItemVolumeTemp = new LVitem_ProjectVM(newVolume.LVitemVolumeTemp);

                if (_lvVM.AlreadyInProject(lvItemVolumeTemp.ListingFile) ||
                    _lvVM.FileExists(lvItemVolumeTemp.ListingFile) ||
                    _lvVM.ContainsUnsavedPath(lvItemVolumeTemp.SourcePath))
                {
                    continue;
                }

                _lvVM.NewItem(lvItemVolumeTemp);
                _lvVM.Unsaved = true;
                break;
            }
        }

        internal void OpenListingFile()
        {
            _bUserCanceled = false;

            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Open Listing File",
                Filter = ListingFilter,
                Multiselect = true
            };

            if (false == (MainWindow.Darken(darkWindow => dlg.ShowDialog((Window)darkWindow)) ?? false))
                return;

            var strPlural = (1 < dlg.FileNames.Length) ? "s" : "";

            new WinProgress(new[] { "Opening listing file" + strPlural }, new[] { "" }, winProgress =>
                new Thread(() =>
                {
                    if (OpenListingFiles(dlg.FileNames, userCanceled: () => _bUserCanceled))
                        _lvVM.Unsaved = true;

                    winProgress.SetAborted();
                    winProgress.Close();
                })
                     { IsBackground = true }
                     .Start())
            {
                WindowClosingCallback = new WeakReference<IWinProgressClosing>(this)
            }
                .ShowDialog();
        }

        internal bool OpenListingFiles(
            IEnumerable<string> listFiles,
            bool bClearItems = false,
            Func<bool> userCanceled = null)
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

            Parallel.ForEach(listFiles, strFilename =>
            {
                if ((null != userCanceled) &&
                    userCanceled())
                {
                    return;   // from lambda
                }

                if ((false == bClearItems) &&
                    _lvVM.AlreadyInProject(strFilename, bQuiet: true))
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

            var bOpenedFiles = false;

            if (bClearItems ||
                (listItems.Count > _lvVM.Count))
            {
                _lvVM.ClearItems();

                bOpenedFiles = listItems
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
            }

            var sbError = new StringBuilder();

            if (0 < sbBadFiles.Length)
                sbError.Append("Bad listing file" + (bMultiBad ? "s" : "") + ".\n\n" + sbBadFiles);

            if (0 < sbAlreadyInProject.Length)
                sbError.Append("Already in project.\n\n" + sbAlreadyInProject);

            if (0 < sbError.Length)
                MBoxStatic.ShowDialog("" + sbError, "Open Listing File");

            return bOpenedFiles;
        }

        bool IOpenListingFiles.Callback(IEnumerable<string> lsFiles, bool bClearItems, Func<bool> userCanceled)
        {
            return OpenListingFiles(lsFiles, bClearItems, userCanceled);
        }

        bool IWinProgressClosing.ConfirmClose()
        {
            _bUserCanceled |= (MessageBoxResult.Yes ==
                MBoxStatic.ShowDialog("Do you want to cancel?", "Opening Listing Files", MessageBoxButton.YesNo));

            return false;   // it will close when the loop queries _bUserCanceled
        }

        bool
            _bUserCanceled = false;
    }
}
