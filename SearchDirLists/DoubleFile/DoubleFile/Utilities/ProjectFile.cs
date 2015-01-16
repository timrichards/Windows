using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace DoubleFile
{
    class ProjectFile
    {
        string m_strProjectFilename = null;
        IEnumerable<LVitem_VolumeVM> m_list_lvVolStrings = null;
        
        internal ProjectFile LoadProject(string strProjectFilename = null)
        {
            return this;
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

            var listNicknames = new List<string>();
            var listListingFiles = new List<string>();

            foreach (LVitem_VolumeVM volStrings in list_lvVolStrings)
            {
                if (false == SaveDirListings.WontSave(volStrings))
                {
                    continue;
                }

                listNicknames.Add(volStrings.Nickname);
                listListingFiles.Add(Path.GetFileName(volStrings.ListingFile));
            }

            if (listListingFiles.Count <= 0)
            {
                return;
            }

            var winProgress = new WinSaveInProgress();

            winProgress.InitProgress(listNicknames, listListingFiles);

            System.Threading.Tasks.Task.Run(() =>
            {
                using (var outStream = new FileStream(strProjectFilename, FileMode.Create, FileAccess.ReadWrite))
                using (var zip = new ZipArchive(outStream, ZipArchiveMode.Create))
                {
                    foreach (var volStrings in m_list_lvVolStrings)
                    {
                        if (string.IsNullOrWhiteSpace(volStrings.ListingFile))
                        {
                            MBox.Assert(0, false);
                            continue;
                            // inFile = Path.GetRandomFileName();
                        }

                        var zipEntry = zip.CreateEntry(Path.GetFileName(volStrings.ListingFile), CompressionLevel.Optimal);

                        using (var inStream = new StreamReader(volStrings.ListingFile))
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
                                winProgress.SetProgress(Path.GetFileName(volStrings.ListingFile), numerator / denominator);
                            }
                        }

                        winProgress.SetCompleted(Path.GetFileName(volStrings.ListingFile));
                    }
                }
            });

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
