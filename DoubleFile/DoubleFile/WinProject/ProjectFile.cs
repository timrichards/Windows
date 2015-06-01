﻿using System;
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
        bool Callback(IEnumerable<string> lsFiles, bool bClearItems, Func<bool> userCanceled);
    }

    // The Process disposable field is managed by wrapper functions that dispose it once control returns.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    class ProjectFile : IWinProgressClosing
    {
        static internal event Func<string> OnSavingProject = null;
        static internal event Action OnOpenedProject = null;

        static internal string TempPath { get { return Path.GetTempPath() + @"DoubleFile\"; } }
        static internal string TempPath01 { get { return TempPath.TrimEnd('\\') + "01"; } }

        static internal bool OpenProject(string strProjectFilename, WeakReference<IOpenListingFiles> openListingFilesWR)
        {
            var projectFile = new ProjectFile();

            _weakReference.Target = projectFile;
            var bRet = projectFile.OpenProject_(strProjectFilename, openListingFilesWR);

            foreach (var d in projectFile._lsDisposable)
                d.Dispose();

            return bRet;
        }

        static internal bool SaveProject(LV_ProjectVM lvProjectVM, string strProjectFilename)
        {
            var projectFile = new ProjectFile();

            _weakReference.Target = projectFile;

            var bRet = projectFile.SaveProject_(lvProjectVM, strProjectFilename);

            foreach (var d in projectFile._lsDisposable)
                d.Dispose();

            return bRet;
        }

        ProjectFile()
        {
            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Path.GetDirectoryName(
                        new Uri(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)
                        .LocalPath) +
                        @"\WinProject\7z920x86\7z.exe",

                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            _lsDisposable.Add(Observable.FromEventPattern<DataReceivedEventArgs>(_process, "OutputDataReceived")
                .Subscribe(args => { Util.WriteLine(args.EventArgs.Data); _sbError.AppendLine(args.EventArgs.Data); }));

            _lsDisposable.Add(Observable.FromEventPattern<DataReceivedEventArgs>(_process, "ErrorDataReceived")
                .Subscribe(args => { Util.WriteLine(args.EventArgs.Data); _sbError.AppendLine(args.EventArgs.Data); }));

            _process.EnableRaisingEvents = true;
            _lsDisposable.Add(_process);
        }

        bool OpenProject_(string strProjectFilename, WeakReference<IOpenListingFiles> openListingFilesWR)
        {
            if (Directory.Exists(TempPath))                     // close box/cancel/undo
            {
                if (Directory.Exists(TempPath01))
                    Directory.Delete(TempPath01, true);

                Directory.Move(TempPath, TempPath01);
            }

            Directory.CreateDirectory(TempPath);

            var bRet = false;

            if (false == StartProcess("Opening project", Path.GetFileName(strProjectFilename),
                TempPath,
                "e \"" + strProjectFilename + "\" -y",
                () => bRet = OpenProjectExited(openListingFilesWR)))
            {
                MBoxStatic.ShowDialog("Couldn't open the project. Reinstall Double File or open your project file " +
                    "and get to your listing files using a download from 7-zip.org.", "Open Project");
            }

            return bRet;
        }

        bool OpenProjectExited(WeakReference<IOpenListingFiles> openListingFilesWR)
        {
            var bErr = ReportAnyErrors(_process, "Opening",
                new Win32Exception(Marshal.GetLastWin32Error()).Message);

            if (bErr || _bUserCanceled)
            {
                if (Directory.Exists(TempPath01))
                {
                    Directory.Delete(TempPath, true);
                    Directory.Move(TempPath01, TempPath);
                }

                _bProcessing = false;
                return false;
            }

            if (null == openListingFilesWR)
            {
                MBoxStatic.Assert(99863, false);
                return false;
            }

            IOpenListingFiles openListingFiles = null;

            openListingFilesWR.TryGetTarget(out openListingFiles);

            if (null == openListingFiles)
            {
                MBoxStatic.Assert(99874, false);
                return false;
            }

            var bRet = openListingFiles.Callback(
                Directory
                .GetFiles(TempPath)
                .Where(s =>
                {
                    var strExt = "" + Path.GetExtension(Path.GetFileName(s));

                    if (0 == strExt.Length)
                        return false;

                    return 
                        strExt
                        .Remove(0, 1)
                        .Equals(FileParse.ksFileExt_Listing,
                        StringComparison.InvariantCultureIgnoreCase);
                }),
                true, () => _bUserCanceled);

            if (null != OnOpenedProject)
                OnOpenedProject();

            if (Directory.Exists(TempPath01))
                Directory.Delete(TempPath01, true);

            if (false == _winProgress.LocalIsClosed)
            {
                _winProgress.Aborted = true;
                Util.UIthread(_winProgress.Close);
            }

            _bProcessing = false;
            return bRet && (false == _bUserCanceled);
        }

        bool SaveProject_(LV_ProjectVM lvProjectVM, string strProjectFilename)
        {
            var lsListingFiles = new List<string>();
            var lsListingFiles_Check = new List<string>();

            foreach (var volStrings in lvProjectVM.ItemsCast)
            {
                if (volStrings.WouldSave)
                    continue;

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
                        strNewName = TempPath + strNewName;
                        File.Copy(volStrings.ListingFile, strNewName);
                        volStrings.ListingFile = strNewName;
                    }
                }

                lsListingFiles.Add(volStrings.ListingFile);
                lsListingFiles_Check.Add(Path.GetFileName(volStrings.ListingFile));
            }

            if (lsListingFiles.IsEmpty())
            {
                MBoxStatic.ShowDialog("Any listing files in project have not yet been saved." +
                    " Click OK on the Project window to start saving directory listings of your drives.",
                    "Save Project");

                return false;
            }

            var sbSource = new StringBuilder();
            var strPath = Path.GetDirectoryName(lsListingFiles[0]) + '\\';

            foreach (var listingFile in lsListingFiles)
            {
                var strFilename = listingFile;

                if (strFilename.StartsWith(strPath))
                    strFilename = strFilename.Replace(strPath, "");

                sbSource.Append("\"").Append(strFilename).Append("\" ");
            }

            if (null != OnSavingProject)
            {
                foreach (var strFilename
                    in OnSavingProject
                    .GetInvocationList()
                    .Select(onSavingProject => "" + onSavingProject.DynamicInvoke()))
                {
                    sbSource.Append("\"").Append(strFilename.Replace(strPath, "")).Append("\" ");
                }
            }

            var strProjectFileNoPath = Path.GetFileName(strProjectFilename);
            var bRet = true;

            var str7z = strProjectFilename + ".7z";

            if (File.Exists(str7z))
                File.Delete(str7z);

            if (false == StartProcess("Saving project", strProjectFileNoPath,
                strPath,
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
            _winProgress.SetCompleted(Path.GetFileName(strProjectFilename));
            MBoxStatic.ShowDialog("Todo: save volume group.\nTodo: save include y/n");
            _bProcessing = false;
            bRet = true;
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
                    strDir = TempPath.TrimEnd('\\') + "_" + strProjectFileNoPath;

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
                    File.Copy(listingFile, strDir + '\\' + Path.GetFileName(listingFile));

                strMessage = " Copied the listing files to\n" + strDir;
            }

            MBoxStatic.ShowDialog("Couldn't save the project." + strMessage, "Save Project");
        }

        bool StartProcess(string status, string strProjectFileNoPath,
            string strWorkingDirectory,
            string strArguments,
            Action onExit)
        {
            MBoxStatic.Assert(99942, null == _winProgress);
            _status = status;
            _process.StartInfo.WorkingDirectory = strWorkingDirectory;
            _process.StartInfo.Arguments = strArguments;
            
            Observable.FromEventPattern(_process, "Exited")
                .Subscribe(x => onExit());

            if (File.Exists(_process.StartInfo.FileName))
            {
                _sbError.AppendLine(DateTime.Now.ToLongTimeString().PadRight(80, '-'));
                _bProcessing = true;
                _process.Start();
                _process.BeginOutputReadLine();

                (_winProgress = new WinProgress(new[] { _status }, new[] { strProjectFileNoPath })
                {
                    WindowClosingCallback = new WeakReference<IWinProgressClosing>(this)
                }).ShowDialog();

                return true;
            }

            return false;
        }

        bool IWinProgressClosing.ConfirmClose()
        {
            if (false == _bProcessing)
                return true;

            if (MessageBoxResult.Yes ==
                MBoxStatic.ShowDialog("Do you want to cancel?", _status, MessageBoxButton.YesNo, _winProgress))
            {
                _bUserCanceled = true;

                if (false == _process.HasExited)
                    _process.Kill();

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
                    .SkipWhile(s => false == s.StartsWith("Error:")))
                .Trim();

            if (string.IsNullOrWhiteSpace(strError))
                strError = strWin32Error;

            if (string.IsNullOrWhiteSpace(strError))
                strError = "Error " + strMode.ToLower() + " project.";

            File.AppendAllText(_strErrorLogFile, "" + _sbError);

            // bootstrap the window close with a delay then messagebox
            // otherwise it freezes
            if (false == _winProgress.LocalIsClosed)
            {
                _winProgress.Aborted = true;
                Util.UIthread(_winProgress.Close);
            }

            Observable.Timer(TimeSpan.FromMilliseconds(33)).Timestamp()
                .Subscribe(x => MBoxStatic.ShowDialog(strError, "Error " + strMode + " Project"));

            return true;
        }

        readonly string
            _strErrorLogFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "ErrorLog";
        readonly StringBuilder 
            _sbError = new StringBuilder();

        WinProgress
            _winProgress = null;
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
        static readonly WeakReference
            _weakReference = new WeakReference(null);

    }
}
