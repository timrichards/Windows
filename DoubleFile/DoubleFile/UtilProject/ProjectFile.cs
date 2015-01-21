using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DoubleFile
{

    class ProjectFile
    {
        internal static string TempPath { get { return System.IO.Path.GetTempPath() + @"DoubleFile\"; } }
        internal static string TempPath01 { get { return TempPath.TrimEnd(new char[] { '\\' }) + "01"; } }

        readonly string m_strErrorLogFile = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "ErrorLog";
        System.Text.StringBuilder m_sbError = new System.Text.StringBuilder();

        System.Diagnostics.Process m_process = new System.Diagnostics.Process();
        WinProgress m_winProgress = null;
    
        internal ProjectFile()
        {
            m_process.StartInfo.FileName = System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)
                .Replace(@"file:\", "") +
                @"\UtilProject\7z920x86\7z.exe";
            m_process.StartInfo.CreateNoWindow = true;
            m_process.StartInfo.UseShellExecute = false;
            m_process.StartInfo.RedirectStandardOutput = true;
            m_process.StartInfo.RedirectStandardError = true;
            m_process.OutputDataReceived += (sender, args) => { UtilProject.WriteLine(args.Data); m_sbError.AppendLine(args.Data); };
            m_process.ErrorDataReceived += (sender, args) => { UtilProject.WriteLine(args.Data); m_sbError.AppendLine(args.Data); };
            m_process.EnableRaisingEvents = true;
        }

        internal void OpenProject(string strProjectFilename, System.Action<IEnumerable<string>, bool> openListingFiles)
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

            m_process.Exited += (sender, args) =>
            {
                GlobalData.static_TopWindow.Dispatcher.Invoke(() =>
                {
                    if (m_winProgress.IsLoaded)                 // close box/cancel/undo
                    {
                        openListingFiles(Directory.GetFiles(TempPath).ToList(), true);
                        m_winProgress.Close();

                        if (Directory.Exists(TempPath01))
                        {
                            Directory.Delete(TempPath01, true);
                        }
                    }
                    else if (Directory.Exists(TempPath01))      // close box/cancel/undo
                    {
                        Directory.Delete(TempPath, true);
                        Directory.Move(TempPath01, TempPath);
                    }
                });
            };

            m_process.StartInfo.WorkingDirectory = TempPath;
            m_process.StartInfo.Arguments = "e " + strProjectFilename + " -y";

            if (false == StartProcess("Opening project.", Path.GetFileName(strProjectFilename)))
            {
                MBox.ShowDialog("Couldn't open the project. Reinstall Double File or open your project file " +
                    "and get to your listing files using a download from 7-zip.org.", "Open Project");
            }
        }

        internal void SaveProject(IEnumerable<LVitem_ProjectVM> listLVvolStrings, string strProjectFilename)
        {
            var listListingFiles = new List<string>();
            var listListingFiles_Check = new List<string>();

            foreach (LVitem_ProjectVM volStrings in listLVvolStrings)
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

                    for (int n = 0; n < knMaxAttempts; ++n)
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

            if (listListingFiles.Count <= 0)
            {
                MBox.ShowDialog("Any listing files in project have not yet been saved." +
                    " Click OK on the Project window to start saving directory listings of your drives.",
                    "Save Project");
                return;
            }

            var sbSource = new System.Text.StringBuilder();

            foreach (var listingFile in listListingFiles)
            {
                sbSource.Append("\"").Append(Path.GetFileName(listingFile)).Append("\" ");
            }

            var strProjectFileNoPath = Path.GetFileName(strProjectFilename);

            m_process.Exited += (sender, args) => 
            {
                if (File.Exists(strProjectFilename))
                {
                    File.Delete(strProjectFilename);
                }

                try
                {
                    File.Move(strProjectFilename + ".7z", strProjectFilename);
                    m_winProgress.SetCompleted(strProjectFileNoPath);
                    MBox.ShowDialog("Todo: save volume group.");
                }
                catch
                {
                    GlobalData.static_TopWindow.Dispatcher.Invoke(() => m_winProgress.Close());

                    var strError = string.Join("", m_sbError.ToString().Split('\n').SkipWhile(s => s.StartsWith("Error:") == false));

                    if (string.IsNullOrWhiteSpace(strError))
                    {
                        strError = "Error saving project.";
                    }

                    MBox.ShowDialog(strError, "Error Saving Project");
                    File.AppendAllText(m_strErrorLogFile, m_sbError.ToString());
                }
            };

            m_process.StartInfo.WorkingDirectory = Path.GetDirectoryName(listListingFiles[0]);
            m_process.StartInfo.Arguments = "a " + strProjectFilename + ".7z " + sbSource + " -mx=3 -md=128m";

            if (false == StartProcess("Saving project.", strProjectFileNoPath))
            {
                var strDir = strProjectFilename;
                var strMessage = "";
                var bSuccess = false;
                const int knMaxAttempts = 16;

                for (int n = 0; n < knMaxAttempts; ++n)
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

                MBox.ShowDialog("Couldn't save the project." + strMessage, "Save Project");
            }
        }

        bool StartProcess(string status, string strProjectFileNoPath)
        {
            MBox.Assert(0, m_winProgress == null);

            if (File.Exists(m_process.StartInfo.FileName))
            {
                try
                {
                    m_sbError.AppendLine(DateTime.Now.ToLongTimeString().PadRight(80, '-'));
                    m_process.Start();
                    m_process.BeginOutputReadLine();
                    m_winProgress = new WinProgress();
                    m_winProgress.InitProgress(new string[] { status }, new string[] { strProjectFileNoPath });
                    m_winProgress.ShowDialog();
                    return true;
                }
                catch
                {
                    // the WPF Application class won't exit the app until the window list is cleared,
                    // even though the window never opened.
                    m_winProgress.Close();
                }
            }

            return false;
        }
    }
}
