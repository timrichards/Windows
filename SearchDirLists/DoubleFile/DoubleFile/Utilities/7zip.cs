using System;
using System.IO;
using SevenZip;
using System.IO.Compression;

namespace DoubleFile
{
    class _7zip : ICodeProgress
    {
        WinSaveInProgress progress = new WinSaveInProgress();
        double denominator = double.Epsilon;
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
            using (var outStream = new FileStream(outFile, FileMode.Create, FileAccess.Write))
            using (var zip = new ZipArchive(outStream, ZipArchiveMode.Create))
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

        internal void bar_1()
        {
            using (var outStream = new FileStream(outFile, FileMode.Create, FileAccess.Write))
            using (var zip = new System.IO.Compression.GZipStream(outStream, System.IO.Compression.CompressionLevel.Optimal))
            using (var inStream = new FileStream(inFile, FileMode.Open, FileAccess.Read))
            {
                int kBufferSize = 4096;
                var buffer = new byte[kBufferSize];
                var numerator = 0;
                double denominator = inStream.Length;    // double preserves mantissa
                var nTransacted = 0;

                while ((nTransacted = inStream.Read(buffer, 0, kBufferSize)) > 0)
                {
                    zip.Write(buffer, 0, nTransacted);
                    numerator += nTransacted;
                    progress.SetProgress("test", numerator / denominator);
                }
            }
        }

        internal void bar_0()
        {
            using (var inStream = new FileStream(inFile, FileMode.Open, FileAccess.Read))
            using (var outStream = new FileStream(outFile, FileMode.Create, FileAccess.Write))
            {
                var encoder = new SevenZip.Compression.LZMA.Encoder();
                encoder.SetCoderProperties(new CoderPropID[] 
			    {
				    CoderPropID.DictionarySize,
				    CoderPropID.PosStateBits,
				    CoderPropID.LitContextBits,
				    CoderPropID.LitPosBits,
				    CoderPropID.NumFastBytes,
				    CoderPropID.MatchFinder,
				    CoderPropID.EndMarker,
			    }, new object[] 
			    {
				    1 << 23,
				    2,
				    3,
				    0,
				    128,
				    "bt4",
				    false,
			    });

                encoder.WriteCoderProperties(outStream);
                denominator = inStream.Length;

                Int64 fileSize = inStream.Length;

                for (int i = 0; i < 8; i++)
                    outStream.WriteByte((Byte)(fileSize >> (8 * i)));
                encoder.Code(inStream, outStream, -1, -1, this);
                progress.SetCompleted("test");
            }
        }

        public void SetProgress(Int64 inSize, Int64 outSize)
        {
            progress.SetProgress("test", inSize / denominator);
        }
    }
}
