using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace DoubleFile
{
    class ProjectFile
    {
        string m_strProjectFilename = null;
        IEnumerable<LVitem_VolumeVM> m_list_lvVolStrings = null;
        
        internal void OpenProject(string strProjectFilename = null)
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

            var strBasePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Replace(@"file:\", "");
            var process = new System.Diagnostics.Process();

            process.StartInfo.FileName = strBasePath + @"\7z\7z.exe";
            process.StartInfo.WorkingDirectory = Path.GetDirectoryName(m_strProjectFilename);
            process.StartInfo.Arguments = "e " + m_strProjectFilename + " -y";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;

            var strProgressTag = Path.GetFileName(strProjectFilename);
            var winProgress = new WinSaveInProgress();

            winProgress.InitProgress(new string[] { "Opening project." }, new string[] { strProgressTag });

            bool bCompleted = false;

            process.OutputDataReceived += (sender, args) =>
            {
                // process.Exited event wasn't working
                if (bCompleted == false)
                {
                    bCompleted = true;
                    winProgress.SetCompleted(strProgressTag);
                }
            };

            process.StartInfo.CreateNoWindow = true;
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

            var strBasePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).Replace(@"file:\", "");
            var sbSource = new System.Text.StringBuilder();

            foreach(var listingFile in listListingFiles)
            {
                sbSource.Append("\"").Append(Path.GetFileName(listingFile)).Append("\" ");
            }

            var process = new System.Diagnostics.Process();

            process.StartInfo.FileName = strBasePath + @"\7z\7z.exe";
            process.StartInfo.WorkingDirectory = Path.GetDirectoryName(listListingFiles[0]);
            process.StartInfo.Arguments = "a " + m_strProjectFilename + " " + sbSource + " -mx=3 -md=128m";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;

            var strProgressTag = Path.GetFileName(strProjectFilename);
            var winProgress = new WinSaveInProgress();

            winProgress.InitProgress(new string[] { "Saving project." }, new string[] { strProgressTag });

            bool bCompleted = false;

            process.OutputDataReceived += (sender, args) => 
            {
                // process.Exited event wasn't working
                if (bCompleted == false)
                {
                    bCompleted = true;
                    File.Move(m_strProjectFilename + ".7z", m_strProjectFilename);
                    winProgress.SetCompleted(strProgressTag);
                }
            };

            process.StartInfo.CreateNoWindow = true;
            process.Start();
            process.BeginOutputReadLine();
            winProgress.ShowDialog(GlobalData.static_TopWindow);
        }
    }

    class Foo
    {
        WinSaveInProgress progress = new WinSaveInProgress();
        const string path = @"C:\_vs\SearchDirLists\DoubleFile\DoubleFile\bin\Debug\";
        string inFile = path + "7zipInputTest";
        string outFile = path + "test.gz";

        internal void bar()
        {
            progress.InitProgress(new string[] { "test" }, new string[] { "test" });
            System.Threading.Tasks.Task.Run(() => { foo(); progress.SetCompleted("test"); });
            progress.ShowDialog(GlobalData.static_TopWindow);
        }

        internal void foo()
        {
            using (var outStream = new FileStream(outFile, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            using (var zip = new ZipArchive(outStream, ZipArchiveMode.Update))
            {
                var zipEntry = zip.CreateEntry("7zipInputTest", CompressionLevel.Optimal);

                using (var inStream = new StreamReader(inFile))
                using (StreamWriter writer = new StreamWriter(zipEntry.Open()))
                {
                    int kBufferSize = 4096;
                    var buffer = new char[kBufferSize];
                    var numerator = 0;
                    double denominator = inStream.BaseStream.Length;    // double preserves mantissa
                    var nTransacted = 0;

                    while ((nTransacted = inStream.Read(buffer, 0, kBufferSize)) > 0)
                    {
                        writer.Write(buffer, 0, nTransacted);
                        numerator += nTransacted;
                        progress.SetProgress("test", numerator / denominator);
                    }
                }
            }
        }
    }
}
