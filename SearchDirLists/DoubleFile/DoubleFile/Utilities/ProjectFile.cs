using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace DoubleFile
{

    class ProjectFile
    {
        string m_strProjectFilename = null;
        IEnumerable<LVitem_VolumeVM> m_list_lvVolStrings = null;
        System.Diagnostics.ProcessStartInfo processStartInfo = new System.Diagnostics.ProcessStartInfo();
        
        internal ProjectFile()
        {
            var strBasePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Replace(@"file:\", "");

            processStartInfo.FileName = strBasePath + @"\7z\7z.exe";
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.CreateNoWindow = true;
        }

        internal void OpenProject(string strProjectFilename, System.Action<IEnumerable<string>, bool> OpenListingFiles)
        {
            List<LVitem_VolumeVM> list_lvVolStrings = new List<LVitem_VolumeVM>();

            if (strProjectFilename != null)
            {
                MBox.Assert(0, m_strProjectFilename == null);
                m_strProjectFilename = strProjectFilename;
            }

            if (m_strProjectFilename == null)
            {
                MBox.Assert(0, false);
                return;
            }

            if (string.IsNullOrWhiteSpace(m_strProjectFilename))
            {
                MBox.Assert(0, false);
                return;
            }

            var strTempPath = Path.GetTempPath() + Path.GetRandomFileName();
            var strProjectFileNoPath = Path.GetFileName(strProjectFilename);

            Directory.CreateDirectory(strTempPath);
            File.Copy(m_strProjectFilename, m_strProjectFilename = strTempPath + strProjectFileNoPath);

            var process = new System.Diagnostics.Process();

            process.StartInfo = processStartInfo;
            process.StartInfo.WorkingDirectory = strTempPath;
            process.StartInfo.Arguments = "e " + m_strProjectFilename + " -y";

            var winProgress = new WinSaveInProgress();

            winProgress.InitProgress(new string[] { "Opening project." }, new string[] { strProjectFileNoPath });

            bool bCompleted = false;

            process.OutputDataReceived += (sender, args) =>
            {
                // process.Exited event wasn't working
                if (bCompleted == false)
                {
                    bCompleted = true;

                    var listFiles = Directory.GetFiles(strTempPath).ToList();

                    listFiles.Remove(strTempPath + strProjectFileNoPath);
                    GlobalData.static_TopWindow.Dispatcher.Invoke(() => OpenListingFiles(listFiles, true));
                    winProgress.SetCompleted(strProjectFileNoPath);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            winProgress.ShowDialog(GlobalData.static_TopWindow);
        }

        internal void SaveProject(IEnumerable<LVitem_VolumeVM> list_lvVolStrings = null, string strProjectFilename = null)
        {
            if (strProjectFilename != null)
            {
                MBox.Assert(0, m_strProjectFilename == null);
                m_strProjectFilename = strProjectFilename;
            }

            if (m_strProjectFilename == null)
            {
                MBox.Assert(0, false);
                return;
            }

            if (string.IsNullOrWhiteSpace(m_strProjectFilename))
            {
                MBox.Assert(0, false);
                return;
            }

            if (list_lvVolStrings != null)
            {
                MBox.Assert(0, m_list_lvVolStrings == null);
                m_list_lvVolStrings = list_lvVolStrings;
            }

            if (m_list_lvVolStrings == null)
            {
                MBox.Assert(0, false);
                return;
            }

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
            process.StartInfo.Arguments = "a " + m_strProjectFilename + " " + sbSource + " -mx=3 -md=128m";

            var strProjectFileNoPath = Path.GetFileName(strProjectFilename);
            var winProgress = new WinSaveInProgress();

            winProgress.InitProgress(new string[] { "Saving project." }, new string[] { strProjectFileNoPath });

            bool bCompleted = false;

            process.OutputDataReceived += (sender, args) => 
            {
                // process.Exited event wasn't working
                if (bCompleted == false)
                {
                    bCompleted = true;
                    try { File.Delete(m_strProjectFilename); } catch { }
                    try
                    {
                        File.Move(m_strProjectFilename + ".7z", m_strProjectFilename);
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
