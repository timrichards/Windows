using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace DoubleFile
{

    class ProjectFile
    {
        internal static string TempPath { get { return System.IO.Path.GetTempPath() + @"DoubleFile\"; } }
        internal static string TempPath01 { get { return TempPath.TrimEnd(new char[] { '\\' }) + "01"; } }

        System.Diagnostics.Process process = new System.Diagnostics.Process();
        WinProgress winProgress = new WinProgress();
    
        internal ProjectFile()
        {
            process.StartInfo.FileName = System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)
                .Replace(@"file:\", "") +
                @"\Utilities\7z920x86\7z.exe";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
        }

        internal void OpenProject(string strProjectFilename, System.Action<IEnumerable<string>, bool> OpenListingFiles)
        {
            List<LVitem_VolumeVM> list_lvVolStrings = new List<LVitem_VolumeVM>();

            var strProjectFileNoPath = Path.GetFileName(strProjectFilename);

            if (Directory.Exists(TempPath))
            {
                if (Directory.Exists(TempPath01))
                {
                    Directory.Delete(TempPath01, true);
                }

                Directory.Move(TempPath, TempPath01);
            }

            Directory.CreateDirectory(TempPath);

            process.StartInfo.WorkingDirectory = TempPath;
            process.StartInfo.Arguments = "e " + strProjectFilename + " -y";
            winProgress.InitProgress(new string[] { "Opening project." }, new string[] { strProjectFileNoPath });

            bool bCompleted = false;

            process.OutputDataReceived += (sender, args) =>
            {
                // process.Exited event wasn't working
                if (bCompleted == false)
                {
                    bCompleted = true;

                    var listFiles = Directory.GetFiles(TempPath).ToList();

                    GlobalData.static_TopWindow.Dispatcher.Invoke(() =>
                    {
                        if (winProgress.IsLoaded)                   // close box/cancel/undo
                        {
                            OpenListingFiles(listFiles, true);
                            winProgress.Close();

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
                }
            };

            if (false == StartProcess())
            {
                MBox.ShowDialog("Couldn't open the project. Reinstall Double File or download from 7-zip.org to open your project file and get to your listing files.", "Open Project");
            }
        }

        internal void SaveProject(IEnumerable<LVitem_VolumeVM> list_lvVolStrings, string strProjectFilename)
        {
            var listListingFiles = new List<string>();

            foreach (LVitem_VolumeVM volStrings in list_lvVolStrings)
            {
                if (false == SaveDirListings.WontSave(volStrings))
                {
                    continue;
                }

                listListingFiles.Add(volStrings.ListingFile);
            }

            if (listListingFiles.Count <= 0)
            {
                return;
            }

            var sbSource = new System.Text.StringBuilder();

            foreach(var listingFile in listListingFiles)
            {
                sbSource.Append("\"").Append(Path.GetFileName(listingFile)).Append("\" ");
            }

            process.StartInfo.WorkingDirectory = Path.GetDirectoryName(listListingFiles[0]);
            process.StartInfo.Arguments = "a " + strProjectFilename + ".7z " + sbSource + " -mx=3 -md=128m";

            var strProjectFileNoPath = Path.GetFileName(strProjectFilename);

            winProgress.InitProgress(new string[] { "Saving project." }, new string[] { strProjectFileNoPath });

            bool bCompleted = false;

            process.OutputDataReceived += (sender, args) => 
            {
                // process.Exited event wasn't working
                if (bCompleted == false)
                {
                    bCompleted = true;
                    try { File.Delete(strProjectFilename); } catch { }
                    try
                    {
                        File.Move(strProjectFilename + ".7z", strProjectFilename);
                        winProgress.SetCompleted(strProjectFileNoPath);
                    }
                    catch { MBox.ShowDialog("Couldn't save the project.", "Save Project"); }
                }
            };

            if (false == StartProcess())
            {
                string strDir = strProjectFilename + "_" + Path.GetFileNameWithoutExtension(Path.GetRandomFileName());

                Directory.CreateDirectory(strDir);

                foreach (var listingFile in listListingFiles)
                {
                    File.Copy(listingFile, strDir + '\\' + Path.GetFileName(listingFile));
                }

                MBox.ShowDialog("Couldn't save the project. Copied the listing files to\n" + strDir, "Save Project");
            }
        }

        bool StartProcess()
        {
            if (StartProcess_())
            {
                return true;
            }
            else
            {
                // the WPF Application class won't exit the app until the window list is cleared,
                // even though the window never opened.
                winProgress.Close();
                return false;
            }
        }

        bool StartProcess_()
        {
            if (false == File.Exists(process.StartInfo.FileName))
            {
                return false;
            }

            try
            {
                process.Start();
                process.BeginOutputReadLine();
                winProgress.ShowDialog(GlobalData.static_TopWindow);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
