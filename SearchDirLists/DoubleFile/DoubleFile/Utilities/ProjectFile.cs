using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace DoubleFile
{

    class ProjectFile
    {
        internal static string TempPath { get { return System.IO.Path.GetTempPath() + @"DoubleFile\"; } }
        System.Diagnostics.ProcessStartInfo processStartInfo = new System.Diagnostics.ProcessStartInfo();
        
        internal ProjectFile()
        {
            processStartInfo.FileName = System.IO.Path.GetDirectoryName(
                System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)
                .Replace(@"file:\", "") +
                @"\7z920x86\7z.exe";
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.CreateNoWindow = true;
        }

        internal void OpenProject(string strProjectFilename, System.Action<IEnumerable<string>, bool> OpenListingFiles)
        {
            List<LVitem_VolumeVM> list_lvVolStrings = new List<LVitem_VolumeVM>();

            var strProjectFileNoPath = Path.GetFileName(strProjectFilename);
            var strTempPath01 = TempPath.TrimEnd(new char[] {'\\'}) + "01";

            if (Directory.Exists(TempPath))
            {
                Directory.Move(TempPath, strTempPath01);
            }

            Directory.CreateDirectory(TempPath);

            var process = new System.Diagnostics.Process();

            process.StartInfo = processStartInfo;
            process.StartInfo.WorkingDirectory = TempPath;
            process.StartInfo.Arguments = "e " + strProjectFilename + " -y";

            var winProgress = new WinProgress();

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

                            if (Directory.Exists(strTempPath01))
                            {
                                Directory.Delete(strTempPath01, true);
                            }
                        }
                        else if (Directory.Exists(strTempPath01))   // close box/cancel/undo
                        {
                            Directory.Delete(TempPath, true);
                            Directory.Move(strTempPath01, TempPath);
                        }
                    });
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            winProgress.ShowDialog(GlobalData.static_TopWindow);
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

            var process = new System.Diagnostics.Process();

            process.StartInfo = processStartInfo;
            process.StartInfo.WorkingDirectory = Path.GetDirectoryName(listListingFiles[0]);
            process.StartInfo.Arguments = "a " + strProjectFilename + ".7z " + sbSource + " -mx=3 -md=128m";

            var strProjectFileNoPath = Path.GetFileName(strProjectFilename);
            var winProgress = new WinProgress();

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

            process.Start();
            process.BeginOutputReadLine();
            winProgress.ShowDialog(GlobalData.static_TopWindow);
        }
    }
}
