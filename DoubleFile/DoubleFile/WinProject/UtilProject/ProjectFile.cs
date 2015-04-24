﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Windows;

namespace DoubleFile
{
    // The Process disposable field is managed by wrapper functions that dispose it once control returns.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    class ProjectFile
    {
        static internal event Func<string> OnSavingProject = null;
        static internal event Action OnOpenedProject = null;

        static internal string TempPath { get { return System.IO.Path.GetTempPath() + @"DoubleFile\"; } }
        static internal string TempPath01 { get { return TempPath.TrimEnd(new char[] { '\\' }) + "01"; } }

        Process Init()
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = Path.GetDirectoryName(
                        new Uri(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)
                        .LocalPath) +
                        @"\WinProject\UtilProject\7z920x86\7z.exe",

                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            _lsDisposable.Add(Observable.FromEventPattern<DataReceivedEventArgs>(process, "OutputDataReceived")
                .Subscribe(args => { UtilProject.WriteLine(args.EventArgs.Data); _sbError.AppendLine(args.EventArgs.Data); }));

            _lsDisposable.Add(Observable.FromEventPattern<DataReceivedEventArgs>(process, "ErrorDataReceived")
                .Subscribe(args => { UtilProject.WriteLine(args.EventArgs.Data); _sbError.AppendLine(args.EventArgs.Data); }));

            process.EnableRaisingEvents = true;
            _lsDisposable.Add(process);
            return process;
        }

        internal void OpenProject(string strProjectFilename,
            Action<IEnumerable<string>, bool, Func<bool>> openListingFiles)
        {
            var process = Init();

            if (Directory.Exists(TempPath))                     // close box/cancel/undo
            {
                if (Directory.Exists(TempPath01))
                    Directory.Delete(TempPath01, true);

                Directory.Move(TempPath, TempPath01);
            }

            Directory.CreateDirectory(TempPath);

            Observable.FromEventPattern(process, "Exited")
                .Subscribe(args => 
            {
                var bErr = ReportAnyErrors(process, "Opening",
                    new Win32Exception(Marshal.GetLastWin32Error()).Message);

                if (bErr || _bUserCancelled)
                {
                    if (Directory.Exists(TempPath01))
                    {
                        Directory.Delete(TempPath, true);
                        Directory.Move(TempPath01, TempPath);
                    }

                    _bProcessing = false;
                    return;
                }

                openListingFiles(
                    Directory
                    .GetFiles(TempPath)
                    .Where(s =>
                    {
                        var strExt = Path.GetExtension(Path.GetFileName(s) ?? "");

                        if (strExt.Length == 0)
                            return false;

                        return 
                            strExt
                            .Remove(0, 1)
                            .Equals(FileParse.ksFileExt_Listing,
                            StringComparison.InvariantCultureIgnoreCase);
                    }),
                    true, () => _bUserCancelled);

                if (null != OnOpenedProject)
                    OnOpenedProject();

                if (Directory.Exists(TempPath01))
                    Directory.Delete(TempPath01, true);

                if (false == _winProgress.LocalIsClosed)
                {
                    _winProgress.Aborted = true;
                    UtilProject.UIthread(_winProgress.Close);
                }
                
                _bProcessing = false;
            });

            process.StartInfo.WorkingDirectory = TempPath;
            process.StartInfo.Arguments = "e \"" + strProjectFilename + "\" -y";

            if (false == StartProcess("Opening project", Path.GetFileName(strProjectFilename), process))
            {
                MBoxStatic.ShowDialog("Couldn't open the project. Reinstall Double File or open your project file " +
                    "and get to your listing files using a download from 7-zip.org.", "Open Project");
            }

            foreach (var d in _lsDisposable)
                d.Dispose();
        }

        internal bool SaveProject(LV_ProjectVM lvProjectVM, string strProjectFilename)
        {
            var bRet = false;

            SaveProject_(lvProjectVM, strProjectFilename, out bRet);

            foreach (var d in _lsDisposable)
                d.Dispose();

            return bRet;
        }

        void SaveProject_(LV_ProjectVM lvProjectVM,
            string strProjectFilename,
            out bool bRet_out)
        {
            var process = Init();

            var listListingFiles = new List<string>();
            var listListingFiles_Check = new List<string>();

            foreach (var volStrings in lvProjectVM.ItemsCast)
            {
                if (volStrings.WouldSave)
                    continue;

                var strNewName = Path.GetFileName(volStrings.ListingFile);

                if (listListingFiles_Check.Contains(strNewName))
                {
                    var bSuccess = false;
                    const int knMaxAttempts = 16;

                    for (var n = 0; n < knMaxAttempts; ++n)
                    {
                        strNewName = Path.GetFileNameWithoutExtension(volStrings.ListingFile) +
                            "_" + n.ToString("00") + Path.GetExtension(volStrings.ListingFile);

                        if (false == listListingFiles_Check.Contains(strNewName))
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

                listListingFiles.Add(volStrings.ListingFile);
                listListingFiles_Check.Add(Path.GetFileName(volStrings.ListingFile));
            }

            if (listListingFiles.IsEmpty())
            {
                MBoxStatic.ShowDialog("Any listing files in project have not yet been saved." +
                    " Click OK on the Project window to start saving directory listings of your drives.",
                    "Save Project");

                bRet_out = false;
                return;
            }

            var sbSource = new System.Text.StringBuilder();
            var strPath = Path.GetDirectoryName(listListingFiles[0]) + '\\';

            foreach (var listingFile in listListingFiles)
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
                    .Select(onSavingProject => (string)onSavingProject.DynamicInvoke()))
                {
                    sbSource.Append("\"").Append(strFilename.Replace(strPath, "")).Append("\" ");
                }
            }

            var strProjectFileNoPath = Path.GetFileName(strProjectFilename);
            var bRet = true;

            Observable.FromEventPattern(process, "Exited")
                .Subscribe(args => 
            {
                var bErr = ReportAnyErrors(process, "Saving",
                    new Win32Exception(Marshal.GetLastWin32Error()).Message);

                if (bErr || _bUserCancelled)
                {
                    bRet = false;
                    return;
                }

                if (File.Exists(strProjectFilename))
                    File.Delete(strProjectFilename);

                File.Move(strProjectFilename + ".7z", strProjectFilename);
                _winProgress.SetCompleted(strProjectFileNoPath);
                MBoxStatic.ShowDialog("Todo: save volume group.\nTodo: save include y/n");
                _bProcessing = false;
            });

            process.StartInfo.WorkingDirectory = strPath;

            var str7z = strProjectFilename + ".7z";

            if (File.Exists(str7z))
                File.Delete(str7z);

            process.StartInfo.Arguments = "a \"" + str7z + "\" " + sbSource + " -mx=3 -md=128m";

            if (false == StartProcess("Saving project", strProjectFileNoPath, process))
            {
                var strDir = strProjectFilename;
                var strMessage = "";
                var bSuccess = false;
                const int knMaxAttempts = 16;

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
                    foreach (var listingFile in listListingFiles)
                        File.Copy(listingFile, strDir + '\\' + Path.GetFileName(listingFile));

                    strMessage = " Copied the listing files to\n" + strDir;
                }

                MBoxStatic.ShowDialog("Couldn't save the project." + strMessage, "Save Project");
                bRet = false;
            }

            bRet_out = bRet;
        }

        bool StartProcess(string status,
            string strProjectFileNoPath,
            Process process)
        {
            MBoxStatic.Assert(99942, null == _winProgress);

            if (File.Exists(process.StartInfo.FileName))
            {
                _sbError.AppendLine(DateTime.Now.ToLongTimeString().PadRight(80, '-'));
                _bProcessing = true;
                process.Start();
                process.BeginOutputReadLine();
                _winProgress = new WinProgress();
                _winProgress.InitProgress(new[] { status }, new[] { strProjectFileNoPath });
                _winProgress.WindowClosingCallback = () =>
                {
                    if (false == _bProcessing)
                        return true;

                    var bRet = false;

                    UtilProject.UIthread(() =>
                    {
                        if (MessageBoxResult.Yes ==
                            MBoxStatic.ShowDialog("Do you want to cancel?", status, MessageBoxButton.YesNo, _winProgress))
                        {
                            _bUserCancelled = true;

                            if (false == process.HasExited)
                                process.Kill();

                            bRet = true;
                        }
                    });

                    return bRet;
                };

                _winProgress.ShowDialog();
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

            if (_bUserCancelled)
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
                UtilProject.UIthread(_winProgress.Close);
            }

            Observable.Timer(TimeSpan.FromMilliseconds(33)).Timestamp()
                .Subscribe(x =>
            {
                MBoxStatic.ShowDialog(strError, "Error " + strMode + " Project");
            });

            return true;
        }

        readonly string
            _strErrorLogFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "ErrorLog";
        readonly System.Text.StringBuilder 
            _sbError = new System.Text.StringBuilder();

        WinProgress
            _winProgress = null;
        bool
            _bProcessing = false;
        bool
            _bUserCancelled = false;
        List<IDisposable>
            _lsDisposable = new List<IDisposable>();
    }
}
