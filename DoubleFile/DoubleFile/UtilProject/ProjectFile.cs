using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;

namespace DoubleFile
{
    delegate string StringAction();

    // The Process disposable field is managed by wrapper functions that dispose it once control returns.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    class ProjectFile
    {
        internal static event StringAction OnSavingProject = null;
        internal static event Action OnOpenedProject = null;

        internal static string TempPath { get { return System.IO.Path.GetTempPath() + @"DoubleFile\"; } }
        internal static string TempPath01 { get { return TempPath.TrimEnd(new char[] { '\\' }) + "01"; } }

        Process Init(Process process)
        {
            process.StartInfo.FileName = Path.GetDirectoryName(
                new Uri(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)
                .LocalPath) +
                @"\UtilProject\7z920x86\7z.exe";
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.OutputDataReceived += (sender, args) => { UtilProject.WriteLine(args.Data); m_sbError.AppendLine(args.Data); };
            process.ErrorDataReceived += (sender, args) => { UtilProject.WriteLine(args.Data); m_sbError.AppendLine(args.Data); };
            process.EnableRaisingEvents = true;
            return process;
        }

        internal void OpenProject(string strProjectFilename, Action<IEnumerable<string>, bool> openListingFiles)
        {
            OpenProject_(strProjectFilename, openListingFiles,
                Init(new Process()))
                .Dispose();
        }

        Process OpenProject_(string strProjectFilename,
            Action<IEnumerable<string>, bool> openListingFiles,
            Process process)
        {
            if (Directory.Exists(TempPath))                     // close box/cancel/undo
            {
                if (Directory.Exists(TempPath01))
                {
                    Directory.Delete(TempPath01, true);
                }

                Directory.Move(TempPath, TempPath01);
            }

            Directory.CreateDirectory(TempPath);

            process.Exited += (sender, args) =>
            {
                var bErr = ReportAnyErrors(process, "Opening");

                if (bErr || m_bUserCancelled)
                {
                    if (Directory.Exists(TempPath01))
                    {
                        Directory.Delete(TempPath, true);
                        Directory.Move(TempPath01, TempPath);
                    }

                    m_bProcessing = false;
                    return;
                }

                openListingFiles(
                    Directory
                        .GetFiles(TempPath)
                        .Where(s =>
                        {
                            var strExt = Path.GetExtension(Path.GetFileName(s) ?? "");

                            if (strExt.Length == 0)
                            {
                                return false;
                            }

                            return 
                                strExt
                                .Remove(0, 1)
                                .Equals(FileParse.ksFileExt_Listing,
                                StringComparison.InvariantCultureIgnoreCase);
                        }),
                    true);

                if (null != OnOpenedProject)
                {
                    OnOpenedProject();
                }

                if (Directory.Exists(TempPath01))
                {
                    Directory.Delete(TempPath01, true);
                }

                UtilProject.UIthread(() => m_winProgress.Close());
                m_bProcessing = false;
            };

            process.StartInfo.WorkingDirectory = TempPath;
            process.StartInfo.Arguments = "e \"" + strProjectFilename + "\" -y";

            if (false == StartProcess("Opening project", Path.GetFileName(strProjectFilename), process))
            {
                MBoxStatic.ShowDialog("Couldn't open the project. Reinstall Double File or open your project file " +
                    "and get to your listing files using a download from 7-zip.org.", "Open Project");
            }

            return process;
        }

        internal bool SaveProject(LV_ProjectVM lvProjectVM, string strProjectFilename)
        {
            var bRet = false;

            SaveProject_(lvProjectVM, strProjectFilename, out bRet,
                Init(new Process()))
                .Dispose();
            return bRet;
        }

        Process SaveProject_(LV_ProjectVM lvProjectVM,
            string strProjectFilename,
            out bool bRet_out,
            Process process)
        {
            var listListingFiles = new List<string>();
            var listListingFiles_Check = new List<string>();

            foreach (var volStrings in lvProjectVM.ItemsCast)
            {
                if (volStrings.WouldSave)
                {
                    continue;
                }

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
                return process;
            }

            var sbSource = new System.Text.StringBuilder();
            var strPath = Path.GetDirectoryName(listListingFiles[0]) + '\\';

            foreach (var listingFile in listListingFiles)
            {
                var strFilename = listingFile;

                if (strFilename.StartsWith(strPath))
                {
                    strFilename = strFilename.Replace(strPath, "");
                }

                sbSource.Append("\"").Append(strFilename).Append("\" ");
            }

            if (null != OnSavingProject)
            {
                foreach (var strFilename
                    in OnSavingProject.GetInvocationList()
                    .Select(onSavingProject => (string)onSavingProject.DynamicInvoke()))
                {
                    sbSource.Append("\"").Append(strFilename.Replace(strPath, "")).Append("\" ");
                }
            }

            var strProjectFileNoPath = Path.GetFileName(strProjectFilename);
            var bRet = true;

            process.Exited += (sender, args) =>
            {
                var bErr = ReportAnyErrors(process, "Saving");

                if (bErr || m_bUserCancelled)
                {
                    bRet = false;
                    return;
                }

                if (File.Exists(strProjectFilename))
                {
                    File.Delete(strProjectFilename);
                }

                File.Move(strProjectFilename + ".7z", strProjectFilename);
                m_winProgress.SetCompleted(strProjectFileNoPath);
                MBoxStatic.ShowDialog("Todo: save volume group.\nTodo: save include y/n");
                m_bProcessing = false;
            };

            process.StartInfo.WorkingDirectory = strPath;

            var str7z = strProjectFilename + ".7z";

            if (File.Exists(str7z))
            {
                File.Delete(str7z);
            }

            process.StartInfo.Arguments = "a \"" + str7z + "\" " + sbSource + " -mx=3 -md=128m";

            if (false == StartProcess("Saving project", strProjectFileNoPath, process))
            {
                var strDir = strProjectFilename;
                var strMessage = "";
                var bSuccess = false;
                const int knMaxAttempts = 16;

                for (var n = 0; n < knMaxAttempts; ++n)
                {
                    if (n == knMaxAttempts - 1)
                    {
                        strDir = TempPath.TrimEnd('\\') + "_" + strProjectFileNoPath;
                    }

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
                    {
                        File.Copy(listingFile, strDir + '\\' + Path.GetFileName(listingFile));
                    }

                    strMessage = " Copied the listing files to\n" + strDir;
                }

                MBoxStatic.ShowDialog("Couldn't save the project." + strMessage, "Save Project");
                bRet = false;
            }

            bRet_out = bRet;
            return process;
        }

        bool StartProcess(string status,
            string strProjectFileNoPath,
            Process process)
        {
            MBoxStatic.Assert(0, m_winProgress == null);

            if (File.Exists(process.StartInfo.FileName))
            {
                m_sbError.AppendLine(DateTime.Now.ToLongTimeString().PadRight(80, '-'));
                m_bProcessing = true;
                process.Start();
                process.BeginOutputReadLine();
                m_winProgress = new WinProgress();
                m_winProgress.InitProgress(new[] { status }, new[] { strProjectFileNoPath });
                m_winProgress.WindowClosingCallback = () =>
                {
                    if (false == m_bProcessing)
                    {
                        return true;
                    }

                    var bRet = false;

                    UtilProject.UIthread(() =>
                    {
                        if (MBoxStatic.ShowDialog("Do you want to cancel?", status,
                            MessageBoxButton.YesNo, m_winProgress) ==
                            MessageBoxResult.Yes)
                        {
                            m_bUserCancelled = true;

                            if (false == process.HasExited)
                            {
                                process.Kill();
                            }

                            bRet = true;
                        }
                    });

                    return bRet;
                };

                m_winProgress.ShowDialog();
                return true;
            }

            return false;
        }

        bool ReportAnyErrors(Process process, string strMode)
        {
            m_winProgress.Aborted = true;

            var strWin32Error = new Win32Exception(Marshal.GetLastWin32Error()).Message;
            var bExitCode = -1;

            try { bExitCode = process.ExitCode; } catch (InvalidOperationException) {}

            if (bExitCode == 0)
            {
                return false;
            }

            m_bProcessing = false;

            if (m_bUserCancelled)
            {
                return false;
            }

            var strError =
                string
                .Join("", m_sbError
                    .ToString()
                    .Split('\n')
                    .SkipWhile(s => false == s.StartsWith("Error:")))
                .Trim();

            if (string.IsNullOrWhiteSpace(strError))
            {
                strError = strWin32Error;
            }

            if (string.IsNullOrWhiteSpace(strError))
            {
                strError = "Error " + strMode.ToLower() + " project.";
            }

            File.AppendAllText(m_strErrorLogFile, m_sbError.ToString());

            // bootstrap the window close with a delay then messagebox
            // otherwise it freezes
            UtilProject.UIthread(() => m_winProgress.Close());

            SDL_Timer tmr = null;
            var tmr_ = new SDL_Timer(33.0, () =>
            {
                tmr.Dispose();
                MBoxStatic.ShowDialog(strError, "Error " + strMode + " Project");
            }).Start();
            tmr = tmr_;
            return true;
        }

        readonly string m_strErrorLogFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "ErrorLog";
        readonly System.Text.StringBuilder m_sbError = new System.Text.StringBuilder();

        WinProgress m_winProgress = null;
        bool m_bProcessing = false;
        bool m_bUserCancelled = false;
    }
}
