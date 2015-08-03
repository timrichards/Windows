using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;

namespace DoubleFile
{
    interface IOpenListingFiles
    {
        bool Callback(IEnumerable<string> ieFiles, bool bClearItems, Func<bool> userCanceled);
    }

    // The Process disposable field is managed by wrapper functions that dispose it once control returns.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    class ProjectFile : IWinProgressClosing, IDisposable
    {
        static internal event Func<string> OnSavingProject = null;
        static internal event Action OnOpenedProject = null;

        static string _tempPath = Path.GetTempPath() + Statics.TempPathIso.TrimEnd('\\') + "_" + Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + '\\';
        static string _tempPathIso01 = Statics.TempPathIso.TrimEnd('\\') + "01";

        static internal bool
            OpenProject(string strProjectFilename, WeakReference<IOpenListingFiles> openListingFilesWR, bool bClearItems)
        {
            using (var projectFile = new ProjectFile())
                return projectFile.OpenProject_(strProjectFilename, openListingFilesWR, bClearItems);
        }

        static internal bool
            SaveProject(LV_ProjectVM lvProjectVM, string strProjectFilename)
        {
            using (var projectFile = new ProjectFile())
                return projectFile.SaveProject_(lvProjectVM, strProjectFilename);
        }

        public void Dispose()
        {
            if (Directory.Exists(_tempPath))
                Directory.Delete(_tempPath, true);

            Util.LocalDispose(_lsDisposable);
        }

        ProjectFile()
        {
            Dispose();
            Directory.CreateDirectory(_tempPath);

            _lsDisposable.Add(
                _process = new Process
            {
                EnableRaisingEvents = true,

                StartInfo = new ProcessStartInfo
                {
                    FileName =
                        Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)
                        .LocalPath) + @"\WinProject\7z920x86\7z.exe",

                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            });

            _lsDisposable.Add(Observable.FromEventPattern<DataReceivedEventArgs>(_process, "OutputDataReceived")
                .LocalSubscribe(99725, args => { Util.WriteLine(args.EventArgs.Data); _sbError.AppendLine(args.EventArgs.Data); }));

            _lsDisposable.Add(Observable.FromEventPattern<DataReceivedEventArgs>(_process, "ErrorDataReceived")
                .LocalSubscribe(99724, args => { Util.WriteLine(args.EventArgs.Data); _sbError.AppendLine(args.EventArgs.Data); }));
        }

        bool OpenProject_(string strProjectFilename, WeakReference<IOpenListingFiles> openListingFilesWR, bool bClearItems)
        {
            var bRet = false;

            if (false == StartProcess((bClearItems ? "Opening" : "Appending") + " project",
                Path.GetFileName(strProjectFilename),
                _tempPath,
                "e \"" + strProjectFilename + "\" -y",
                () => bRet = OpenProjectExited(openListingFilesWR, bClearItems)))
            {
                MBoxStatic.ShowDialog("Couldn't open the project. Reinstall Double File or open your project file " +
                    "and get to your listing files using a download from 7-zip.org.", "Open Project");
            }

            return bRet;
        }

        bool OpenProjectExited(WeakReference<IOpenListingFiles> openListingFilesWR, bool bClearItems)
        {
            var bErr = ReportAnyErrors(_process, "Opening",
                new Win32Exception(Marshal.GetLastWin32Error()).Message);

            if (bErr || _bUserCanceled)
            {
                _bProcessing = false;
                return false;
            }

            if (null == openListingFilesWR)
            {
                Util.Assert(99863, false);
                return false;
            }

            IOpenListingFiles openListingFiles = null;

            openListingFilesWR.TryGetTarget(out openListingFiles);

            if (null == openListingFiles)
            {
                Util.Assert(99874, false);
                return false;
            }

            var lsFilenames = new List<string> { };

            foreach (var strFilename in Directory.GetFiles(_tempPath))
            {
                var strNewFilename = Statics.TempPathIso + Path.GetFileName(strFilename);

                lsFilenames.Add(strNewFilename);
                strFilename.FileMoveToIso(strNewFilename);
            }

            var bRet = openListingFiles.Callback
            (
                lsFilenames
                    .Where(s =>
                {
                    var strExt = "" + Path.GetExtension(Path.GetFileName(s));

                    if (0 == strExt.Length)
                        return false;   // from lambda

                    return              // from lambda
                        strExt
                        .Remove(0, 1)
                        .Equals(FileParse.ksFileExt_Listing,
                        StringComparison.InvariantCultureIgnoreCase);
                }),
                bClearItems,
                () => _bUserCanceled
            );

            OnOpenedProject?.Invoke();

            if (Statics.IsoStore.DirectoryExists(_tempPathIso01))
            {
                if (false == bClearItems)
                {
                    string strNew = null;

                    foreach (var strFile in
                        Statics.IsoStore.GetFileNames()
                        .Where(strFile =>
                    {
                        return
                            strFile.StartsWith(_tempPathIso01) &&
                            (false == Statics.IsoStore.FileExists(strNew = strFile.Replace(_tempPathIso01, Statics.TempPathIso)));
                    }))
                    {
                        // strNew is assigned each iteration in C# 4.
                        Statics.IsoStore.MoveFile(strFile, strNew);
                    }
                }

                Statics.IsoStore.DeleteDirectory(_tempPathIso01);
            }

            WinProgress.CloseForced();
            _bProcessing = false;
            return bRet && (false == _bUserCanceled);
        }

        bool SaveProject_(LV_ProjectVM lvProjectVM, string strProjectFilename)
        {
            var lsListingFiles = new List<string>();
            var lsListingFiles_Check = new List<string>();
            var bUnsavedListings = false;

            foreach (var volStrings in lvProjectVM.ItemsCast)
            {
                if (volStrings.WouldSave)
                {
                    bUnsavedListings = true;
                    continue;
                }

                var strNewName = Path.GetFileName(volStrings.ListingFile);

                if (lsListingFiles_Check.Contains(strNewName))
                {
                    var bSuccess = false;
                    const int knMaxAttempts = 16;

                    for (var n = 0; n < knMaxAttempts; ++n)
                    {
                        strNewName = Path.GetFileNameWithoutExtension(volStrings.ListingFile) +
                            "_" + n.ToString("00") + Path.GetExtension(volStrings.ListingFile);

                        if (false == lsListingFiles_Check.Contains(strNewName))
                        {
                            bSuccess = true;
                            break;
                        }
                    }

                    if (bSuccess)
                    {
                        strNewName = Statics.TempPathIso + strNewName;
                        Statics.IsoStore.CopyFile(volStrings.ListingFile, strNewName);
                        volStrings.ListingFile = strNewName;
                    }
                }

                lsListingFiles.Add(volStrings.ListingFile);
                lsListingFiles_Check.Add(Path.GetFileName(volStrings.ListingFile));
            }

            bool bIsEmpty = 0 == lsListingFiles.Count;

            if (bUnsavedListings || bIsEmpty)
            {
                MBoxStatic.ShowDialog("One or more volumes in the project have not yet had directory listings saved for it." +
                    " Click an explorer link at the top of the window to start saving directory listings of your drives.",
                    "Save Project");

                if (bIsEmpty)
                    return false;
            }

            var sbSource = new StringBuilder();
            var strPath = Path.GetDirectoryName(lsListingFiles[0]) + '\\';

            foreach (var listingFile in lsListingFiles)
            {
                var strSourceFilename = listingFile;

                if (listingFile.StartsWith(Statics.TempPathIso))
                {
                    strSourceFilename = listingFile.Replace(strPath, "");

                    using (var sr = new StreamReader(Statics.IsoStore.OpenFile(listingFile, FileMode.Open)))
                    using (var sw = new StreamWriter(File.OpenWrite(_tempPath + '\\' + strSourceFilename)))
                        Util.CopyStream(sr, sw);
                }

                sbSource.Append("\"").Append(strSourceFilename).Append("\" ");
            }

            OnSavingProject?
                .GetInvocationList()
                .Select(onSavingProject => "" + onSavingProject.DynamicInvoke())
                .ForEach(strFilename =>
                sbSource.Append("\"").Append(strFilename.Replace(strPath, "")).Append("\" "));

            var strProjectFileNoPath = Path.GetFileName(strProjectFilename);
            var bRet = true;
            var str7z = strProjectFilename + ".7z";

            if (File.Exists(str7z))
                File.Delete(str7z);

            if (false == StartProcess("Saving project", strProjectFileNoPath,
                _tempPath,
                "a \"" + str7z + "\" " + sbSource + " -mx=3 -md=128m",
                () => SaveProcessExited(out bRet, strProjectFilename)))
            {
                SaveProjectFailedToStartProcess(strProjectFilename, lsListingFiles);
                bRet = false;
            }

            return bRet;
        }

        void SaveProcessExited(out bool bRet, string strProjectFilename)
        {
            var bErr = ReportAnyErrors(_process, "Saving",
                new Win32Exception(Marshal.GetLastWin32Error()).Message);

            if (bErr || _bUserCanceled)
            {
                bRet = false;
                return;
            }

            if (File.Exists(strProjectFilename))
                File.Delete(strProjectFilename);

            File.Move(strProjectFilename + ".7z", strProjectFilename);
            _bProcessing = false;
            bRet = true;
            
            WinProgress.WithWinProgress(w =>
                w.SetCompleted(Path.GetFileName(strProjectFilename)));
        }

        void SaveProjectFailedToStartProcess(string strProjectFilename, List<string> lsListingFiles)
        {
            var strDir = strProjectFilename;
            var strMessage = "";
            var bSuccess = false;
            const int knMaxAttempts = 16;
            var strProjectFileNoPath = Path.GetFileName(strProjectFilename);

            for (var n = 0; n < knMaxAttempts; ++n)
            {
                if (knMaxAttempts - 1 == n)
                    strDir = _tempPath.TrimEnd('\\') + "_" + strProjectFileNoPath;

                try
                {
                    Directory.CreateDirectory(strDir);
                    bSuccess = true;
                    break;
                }
                catch { }

                strDir = strProjectFilename + "_" + n.ToString("00");
            }

            if (bSuccess)
            {
                foreach (var listingFile in lsListingFiles)
                {
                    using (var sr = new StreamReader(Statics.IsoStore.OpenFile(listingFile, FileMode.Open)))
                    using (var sw = new StreamWriter(File.OpenWrite(strDir + '\\' + Path.GetFileName(listingFile))))
                        Util.CopyStream(sr, sw);
                }

                strMessage = " Copied the listing files to\n" + strDir;
            }

            MBoxStatic.ShowDialog("Couldn't save the project." + strMessage, "Save Project");
        }

        bool StartProcess(string status, string strProjectFileNoPath,
            string strWorkingDirectory,
            string strArguments,
            Action onExit)
        {
            if (false == File.Exists(_process.StartInfo.FileName))
                return false;

            _status = status;
            _process.StartInfo.WorkingDirectory = strWorkingDirectory;
            _process.StartInfo.Arguments = strArguments;
            
            Observable.FromEventPattern(_process, "Exited")
                .LocalSubscribe(99723, x => onExit());

            (new WinProgress(new[] { _status }, new[] { strProjectFileNoPath }, x =>
            {
                try
                {
                    _sbError.AppendLine(DateTime.Now.ToLongTimeString().PadRight(80, '-'));
                    _bProcessing = true;
                    _process.Start();
                    _process.BeginOutputReadLine();
                }
                catch (Exception e)
                {
                    Util.Assert(99668, false, "Exception in StartProcess\n" +
                        e.GetBaseException().Message);
                }
            })
            { WindowClosingCallback = new WeakReference<IWinProgressClosing>(this) })
                .ShowDialog();

            return true;
        }

        bool IWinProgressClosing.ConfirmClose()
        {
            if (false == _bProcessing)
                return true;

            if (MessageBoxResult.Yes == WinProgress.WithWinProgress(w =>
                MBoxStatic.ShowDialog("Do you want to cancel?", _status, MessageBoxButton.YesNo, w)))
            {
                _bUserCanceled = true;

                try
                {
                    if (false == _process.HasExited)
                        _process.Kill();
                }
                catch (InvalidOperationException) { }

                return true;
            }

            return false;
        }

        bool ReportAnyErrors(Process process, string strMode, string strWin32Error)
        {
            var bExitCode = -1;

            try { bExitCode = process.ExitCode; } catch (InvalidOperationException) {}

            if (0 == bExitCode)
                return false;

            _bProcessing = false;

            if (_bUserCanceled)
                return false;

            var strError =
                string
                .Join("",
                    ("" + _sbError)
                    .Split('\n')
                    .Where(s =>
                    (s.StartsWith("Error:") ||
                    s.Contains(" : The system cannot find the file specified."))))
                .Trim();

            if (string.IsNullOrWhiteSpace(strError))
                strError = strWin32Error;

            if (string.IsNullOrWhiteSpace(strError))
                strError = "Error " + strMode.ToLower() + " project.";

            using (var sw = new StreamWriter(File.Open(_strErrorLogFile, FileMode.Append)))
                sw.Write("" + _sbError);

            // bootstrap the window close with a delay then messagebox
            // otherwise it freezes
            WinProgress.CloseForced();

            // One-shot: no need to dispose
            Observable.Timer(TimeSpan.FromMilliseconds(33)).Timestamp()
                .LocalSubscribe(99722, x => MBoxStatic.ShowDialog(strError, "Error " + strMode + " Project"));

            return true;
        }

        readonly string
            _strErrorLogFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "ErrorLog";
        readonly StringBuilder 
            _sbError = new StringBuilder();

        bool
            _bProcessing = false;
        bool
            _bUserCanceled = false;
        List<IDisposable>
            _lsDisposable = new List<IDisposable>();

        string
            _status = null;
        readonly Process
            _process = null;
    }
}
