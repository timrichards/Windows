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
    partial class WinProjectVM : IOpenListingFiles, IWinProgressClosing, IDisposable
    {
        internal const string
            ListingFilter = "Double File Listing|*." + FileParse.ksFileExt_Listing + _ksAllFilesFilter;
        const string _ksProjectFilter = "Double File Project|*." + FileParse.ksFileExt_Project + _ksAllFilesFilter;
        const string _ksAllFilesFilter = "|All files|*.*";

        internal const string
            UnsavedWarning = "You are about to lose changes to an unsaved project.";

        internal WinProjectVM()
        {
            ProjectFile.OnSavingProject += Serialize;
            ProjectFile.OnOpenedProject += Deserialize;
        }

        public void Dispose()
        {
            ProjectFile.OnSavingProject -= Serialize;
            ProjectFile.OnOpenedProject -= Deserialize;
        }

        string Serialize()
        {
            using (var sw = new StreamWriter(LocalIsoStore.OpenFile(Metadata, FileMode.Create)))
                sw.Write(string.Join("\n", _lvVM.ItemsCast.Select(lvItem => lvItem.Serialize())));

            return Metadata;
        }

        void Deserialize()
        {
            if (false == Metadata.FileExists())
                return;

            var asLVitems = _lvVM.ItemsCast.ToList();

            foreach (var strLine in LocalIsoStore.ReadLines(Metadata))
            {
                for (int i = 0; i < asLVitems.Count; ++i)
                {
                    if (asLVitems[i].Deserialize(strLine))
                    {
                        asLVitems.RemoveAt(i);
                        break;
                    }
                }
            }

            Util.Assert(99857, 0 == asLVitems.Count);
            LocalIsoStore.DeleteFile(Metadata);
        }

        internal void
            OpenProject()
        {
            var bClearItems =
                (0 == _lvVM.Items.Count) ||
                (false == (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)));

            var strTitle = (bClearItems ? "Open" : "Append") + " Project";

            if (bClearItems && _lvVM.Unsaved &&
                (MessageBoxResult.Cancel ==
                MBoxStatic.ShowDialog(UnsavedWarning, strTitle, MessageBoxButton.OKCancel)))
            {
                return;
            }

            var dlg = new Microsoft.Win32.OpenFileDialog { Title = strTitle, Filter = _ksProjectFilter };

            if ((ModalThread.Go(darkWindow => dlg.ShowDialog((Window)darkWindow)) ?? false) &&
                ProjectFile.OpenProject(dlg.FileName, new WeakReference<IOpenListingFiles>(this), bClearItems))
            {
                _lvVM.Unsaved =
                    false == bClearItems;

                if (bClearItems)
                    _lvVM.SetModified();    // Unsaved has trickery to do a UX reset only when set to true: want always

                WinProject.OKtoNavigate_UpdateSaveListingsLink();
            }
        }

        internal void
            SaveProject() => SaveProject(_lvVM);
        static internal void
            SaveProject(LV_ProjectVM lvProjectVM)
        {
            string strFilename = null;

            for (;;)
            {
                var dlg = new Microsoft.Win32.SaveFileDialog
                {
                    Title = "Save Project",
                    Filter = _ksProjectFilter,
                    FileName = strFilename,
                    OverwritePrompt = false
                };

                if (ModalThread.Go(darkWindow => dlg.ShowDialog((Window)darkWindow)) ?? false)
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

        internal void
            NewListingFile()
        {
            var lvItemVolumeTemp = new LVitem_ProjectVM();

            for (;;)
            {
                var newVolume = new WinVolumeNew { LVitemVolumeTemp = new LVitem_ProjectVM(lvItemVolumeTemp) };

                if (false == (newVolume.ShowDialog() ?? false))
                    break;      // user canceled

                lvItemVolumeTemp = new LVitem_ProjectVM(newVolume.LVitemVolumeTemp);

                if (_lvVM.AlreadyInProject(lvItemVolumeTemp.ListingFile) ||
                    _lvVM.FileExists(lvItemVolumeTemp.ListingFile) ||
                    _lvVM.ContainsUnsavedPath(lvItemVolumeTemp.SourcePath))
                {
                    continue;
                }

                _lvVM.Add(lvItemVolumeTemp);
                _lvVM.Unsaved = true;
                break;
            }
        }

        internal void
            OpenListingFile()
        {
            _bUserCanceled = false;

            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Open Listing File",
                Filter = ListingFilter,
                Multiselect = true
            };

            if (false == (ModalThread.Go(darkWindow => dlg.ShowDialog((Window)darkWindow)) ?? false))
                return;

            var strPlural = (1 < dlg.FileNames.Length) ? "s" : "";

            new WinProgress(new[] { "Opening listing file" + strPlural }, new[] { "" }, winProgress =>
                Util.ThreadMake(() =>
                {
                    if (OpenListingFiles(dlg.FileNames, userCanceled: () => _bUserCanceled))
                        _lvVM.Unsaved = true;

                    WinProgress.CloseForced();
                }))
            {
                WindowClosingCallback = new WeakReference<IWinProgressClosing>(this)
            }
                .ShowDialog();
        }

        internal bool
            OpenListingFiles(IEnumerable<string> ieFiles, bool bClearItems = false, Func<bool> userCanceled = null)
        {
            var sbBadFiles = new StringBuilder();
            var bMultiBad = true;
            var sbAlreadyInProject = new StringBuilder();
            var listItems = new ConcurrentBag<LVitem_ProjectVM>();
            var cts = new CancellationTokenSource();

            if (false == bClearItems)
            {
                foreach (var lvItem in _lvVM.ItemsCast)
                    listItems.Add(lvItem);
            }

            Util.ParallelForEach(ieFiles, new ParallelOptions { CancellationToken = cts.Token }, strFilename =>
            {
                if (userCanceled?.Invoke() ?? false)
                {
                    cts.Cancel();
                    return;   // from lambda
                }

                if ((false == bClearItems) &&
                    _lvVM.AlreadyInProject(strFilename, bQuiet: true))
                {
                    lock (sbAlreadyInProject)
                        sbAlreadyInProject.Append("• ").Append(Path.GetFileName(strFilename)).Append("\n");

                    return;   // from lambda
                }

                LVitem_ProjectVM lvItem = null;

                if (FileParse.ReadHeader(strFilename, ref lvItem))
                {
                    listItems.Add(lvItem);
                }
                else
                {
                    bMultiBad = 0 < sbBadFiles.Length;

                    lock (sbBadFiles)
                        sbBadFiles.Append("• ").Append(Path.GetFileName(strFilename)).Append("\n");
                }
            });

            if (userCanceled?.Invoke() ?? false)
                return false;

            var bOpenedFiles = false;

            if (bClearItems ||
                (listItems.Count > _lvVM.Items.Count))
            {
                _lvVM.ClearItems();

                bOpenedFiles = listItems
                    .OrderBy(lvItem => lvItem.SourcePath)
                    .Aggregate(false, (current, lvItem) =>
                {
                    if (userCanceled?.Invoke() ?? false)
                        return false;   // from lambda

                    bool bAdded = false;

                    Util.UIthread(99849, () => bAdded = _lvVM.Add(lvItem));
                    return bAdded || current;  // from lambda
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

        bool IOpenListingFiles.Callback(IEnumerable<string> ieFiles, bool bClearItems, Func<bool> userCanceled) =>
            OpenListingFiles(ieFiles, bClearItems, userCanceled);

        bool IWinProgressClosing.ConfirmClose()
        {
            _bUserCanceled |= (MessageBoxResult.Yes ==
                MBoxStatic.ShowDialog("Do you want to cancel?", "Opening Listing Files", MessageBoxButton.YesNo));

            return false;   // it will close when the loop queries _bUserCanceled
        }

        bool
            _bUserCanceled = false;

        static readonly string
            Metadata = LocalIsoStore.TempDir + "metadata";
    }
}
