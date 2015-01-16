using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace DoubleFile
{
    class ProjectFile
    {
        string m_strProjectFilename = null;
        IEnumerable<LVitem_VolumeVM> m_list_lvVolStrings = null;
        
        internal void LoadProject(string strProjectFilename = null)
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

            var winProgress = new WinSaveInProgress();
            var strProgressTag = Path.GetFileName(strProjectFilename);
            var listFiles = new List<string>();

            winProgress.InitProgress(new string[] { "Loading project." }, new string[] { strProgressTag });

            System.Threading.Tasks.Task.Run(() =>
            {
                using (var inStream = new FileStream(strProjectFilename, FileMode.Open, FileAccess.Read))
                using (var zip = new ZipArchive(inStream, ZipArchiveMode.Read))
                using (var reader = new StreamReader(zip.Entries[0].Open()))
                {
                    int kBufferSize = 4096;
                    var buffer = new char[kBufferSize];
                    var numerator = 0;
                    double denominator = zip.Entries[0].Length;    // double preserves mantissa
                    var nTransacted = 0;

                    listFiles.Add(Path.GetTempFileName());
                    Utilities.WriteLine("listFiles[listFiles.Count - 1] " + listFiles[listFiles.Count - 1]);

                    var writer = new StreamWriter(listFiles[listFiles.Count - 1]);
                    while ((nTransacted = reader.Read(buffer, 0, kBufferSize)) > 0)
                    {
                        bool bNewFile = false;

                        for (int i = 0; i < buffer.Length; ++i)
                        {
                            if (buffer[i] == ConcatenatedStream.FileEndMarker)
                            {
                                writer.Write(buffer, 0, i - 1);
                                writer.Close();

                                if (numerator < denominator - 1)    // did I get that extra FileEndMarker?
                                {
                                    listFiles.Add(Path.GetTempFileName());
                                    Utilities.WriteLine("listFiles[listFiles.Count - 1] " + listFiles[listFiles.Count - 1]);
                                    writer = new StreamWriter(listFiles[listFiles.Count - 1]);
                                    writer.Write(buffer, i + 1, nTransacted - i - 1);
                                }

                                bNewFile = true;
                                break;
                            }
                        }

                        if (bNewFile)
                        {
                            bNewFile = false;
                        }
                        else
                        {
                            writer.Write(buffer, 0, nTransacted);
                        }

                        numerator += nTransacted;
                        winProgress.SetProgress(strProgressTag, numerator / denominator);
                    }

                    writer.Close();
                    Utilities.WriteLine("numerator " + numerator);
                    Utilities.WriteLine("zip.Entries[0].Length " + zip.Entries[0].Length);
                    winProgress.SetCompleted(strProgressTag);
                }
            });

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

            var winProgress = new WinSaveInProgress();
            var strProgressTag = Path.GetFileName(strProjectFilename);

            winProgress.InitProgress(new string[] { "Saving project." }, new string[] { strProgressTag });

            System.Threading.Tasks.Task.Run(() =>
            {
                using (var outStream = new FileStream(strProjectFilename, FileMode.Create, FileAccess.Write))
                using (var zip = new ZipArchive(outStream, ZipArchiveMode.Create))
                using (var concatenatedStream = new StreamReader(new ConcatenatedStream(listListingFiles)))
                {
                    var zipEntry = zip.CreateEntry(strProjectFilename, CompressionLevel.Optimal);

                    using (var writer = new StreamWriter(zipEntry.Open()))
                    {
                        int kBufferSize = 4096;
                        var buffer = new char[kBufferSize];
                        var numerator = 0;
                        double denominator = concatenatedStream.BaseStream.Length;    // double preserves mantissa
                        var nTransacted = 0;

                        while ((nTransacted = concatenatedStream.Read(buffer, 0, kBufferSize)) > 0)
                        {
                            writer.Write(buffer, 0, nTransacted);
                            numerator += nTransacted;
                            winProgress.SetProgress(strProgressTag, numerator / denominator);
                        }
                    }

                    Utilities.WriteLine("concatenatedStream.BaseStream.Length " + concatenatedStream.BaseStream.Length);
                    winProgress.SetCompleted(strProgressTag);
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
