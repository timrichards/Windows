using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Reactive.Linq;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Microsoft.Win32.SafeHandles;

namespace DoubleFile
{
    partial class SaveDirListings
    {
        class SaveDirListing : TraverseTreeBase
        {
            internal SaveDirListing(LVitem_ProjectVM volStrings, ISaveDirListingsStatus saveDirListingsStatus)
                : base(volStrings)
            {
                _saveDirListingsStatus = saveDirListingsStatus;
            }

            internal SaveDirListing DoThreadFactory()
            {
                _thread = Util.ThreadMake(() => Go());
                return this;
            }

            internal void Join()
            {
                _thread.Join();
            }

            internal void Abort()
            {
                _bThreadAbort = true;
                _thread.Abort();
            }

            void WriteHeader(TextWriter fs)
            {
                fs.WriteLine(ksHeader01);
                fs.WriteLine(LVitemProjectVM.Nickname);
                fs.WriteLine(LVitemProjectVM.SourcePath);

                string strModel = null;
                string strSerial = null;
                ulong? nSize = null;

                // at minimum get the drive size
                DriveSerialStatic.Get(LVitemProjectVM.SourcePath, out strModel, out strSerial, out nSize);

                var bAsk_DriveModel = ((false == string.IsNullOrWhiteSpace(strModel)) &&
                    ((false == string.IsNullOrWhiteSpace(LVitemProjectVM.DriveModel)) &&
                    (strModel != LVitemProjectVM.DriveModel)));

                var bAsk_DriveSerial = ((false == string.IsNullOrWhiteSpace(strSerial)) &&
                    ((false == string.IsNullOrWhiteSpace(LVitemProjectVM.DriveSerial)) &&
                    (strSerial != LVitemProjectVM.DriveSerial)));

                if ((bAsk_DriveModel || bAsk_DriveSerial) &&
                    ((MBoxStatic.ShowDialog("Overwrite user-entered drive model and/or serial # for " +
                        LVitemProjectVM.SourcePath[0] + @":\ ?", "Save Directory Listings",
                        System.Windows.MessageBoxButton.YesNo) ==
                        System.Windows.MessageBoxResult.No)))
                {
                    // separating these allows one user value to substitute blank robo-get, while keeping the other one
                    if (bAsk_DriveModel)
                        strModel = LVitemProjectVM.DriveModel;

                    if (bAsk_DriveSerial)
                        strSerial = LVitemProjectVM.DriveSerial;
                }

                fs.WriteLine(ksDrive01);

                var driveInfo = new DriveInfo(LVitemProjectVM.SourcePath[0] + @":\");
                var sb = new StringBuilder();
                var nCount = 0;

                Action<Object> WriteLine = o =>
                {
                    var s = (o != null) ? "" + o : null;

                    // Hack. Prevent blank line continue in FileParse.ConvertFile()
                    sb.AppendLine(string.IsNullOrWhiteSpace(s) ? " " : s.Trim());
                    ++nCount;
                };

                WriteLine(driveInfo.AvailableFreeSpace); // These could all be named better, so kasDIlabels is different.
                WriteLine(driveInfo.DriveFormat);        // Misnomer. Should be VolumeFormat.
                WriteLine(driveInfo.DriveType);
                WriteLine(driveInfo.Name);
                WriteLine(driveInfo.RootDirectory);
                WriteLine(driveInfo.TotalFreeSpace);
                WriteLine(driveInfo.TotalSize);
                WriteLine(driveInfo.VolumeLabel);
                WriteLine(strModel);
                WriteLine(strSerial);
                WriteLine(nSize);
                MBoxStatic.Assert(99941, nCount == knDriveInfoItems);
                fs.WriteLine(("" + sb).Trim());
            }

            IEnumerable<Tuple<string, long, SafeFileHandle, string>>
                OpenFiles(IEnumerable<Tuple<string, long>> listFilePaths)
            {
                foreach (var tuple in listFilePaths)
                {
                    var strFile = tuple.Item1;
                    var fileHandle = NativeMethods
                        .CreateFile(@"\\?\" + strFile, FileAccess.Read, FileShare.ReadWrite, IntPtr.Zero,
                        NativeMethods.OPEN_EXISTING,
                        (FileAttributes)NativeMethods.FILE_FLAG_RANDOM_ACCESS, IntPtr.Zero);

                    string strError = fileHandle.IsInvalid ? new Win32Exception(Marshal.GetLastWin32Error()).Message : null;

                    yield return Tuple.Create(strFile, tuple.Item2, fileHandle, strError);
                }
            }

            bool FillBuffer(FileStream fs, int nBufferSize, IList<byte[]> lsBuffer, bool bTruncate = true)
            {
                var readBuffer = new byte[nBufferSize];
                var nRead = fs.Read(readBuffer, 0, nBufferSize);

                if (bTruncate &&
                    (0 < nRead) &&
                    (nRead < nBufferSize))
                {
                    var truncBuffer = new byte[nRead];

                    Array.Copy(readBuffer, truncBuffer, nRead);
                    readBuffer = truncBuffer;
                }

                lsBuffer.Add(readBuffer);

                if (0 == nRead)
                {
                    MBoxStatic.Assert(99930, false);
                    return false;
                }

                bool bMoreToRead = fs.Position < fs.Length;
                
                if (bMoreToRead)
                    MBoxStatic.Assert(99926, nRead == nBufferSize);

                return bMoreToRead;
            }

            IEnumerable<Tuple<string, long, string, IReadOnlyList<byte[]>>>
                ReadBuffers(IEnumerable<Tuple<string, long, SafeFileHandle, string>> listFileHandles)
            {
                foreach (var tuple in listFileHandles)
                {
                    var lsRet = new List<byte[]>();
                    var retval = Tuple.Create(tuple.Item1, tuple.Item2, tuple.Item4, (IReadOnlyList<byte[]>)lsRet);
                    var fileHandle = tuple.Item3;

                    if (fileHandle.IsInvalid)
                    {
                        fileHandle.Dispose();
                        yield return retval;
                        continue;
                    }

                    using (fileHandle)
                    using (var fs = new FileStream(fileHandle, FileAccess.Read))
                    Util.Closure(() =>
                    {
                        const int knLilBuffLength = 4096;
                        const int knBigBuffLength = 65536;

                        if (false == FillBuffer(fs, knLilBuffLength, lsRet, bTruncate: false))
                            return;     // from lambda

                        if (false == FillBuffer(fs, knBigBuffLength, lsRet))
                            return;     // from lambda

                        var desiredPos = fs.Length - knBigBuffLength;

                        if (desiredPos > fs.Position)
                        {
                            MBoxStatic.Assert(99931, knBigBuffLength + knLilBuffLength == fs.Position);
                            fs.Position = desiredPos;
                        }

                        if (false == FillBuffer(fs, knBigBuffLength, lsRet))
                            return;     // from lambda
                    });

                    yield return retval;
                }
            }

            void Hash(IReadOnlyList<Tuple<string, long>> listFilePaths,
                out IReadOnlyDictionary<string, Tuple<HashTuple, HashTuple>> dictHash_out,
                out IReadOnlyDictionary<string, string> dictException_FileRead_out,
                bool bSSD)
            {
                if (null == listFilePaths)
                {
                    dictHash_out = null;
                    dictException_FileRead_out = null;
                    return;
                }

                // Default cluster size on any NTFS volume up to 16TB is 4K
                // Limiting hashes is not really the limiting factor: CreateFile is the bottleneck by far.
                long nProgressNumerator = 0;

                double nProgressDenominator =               // double preserves mantissa
                    listFilePaths.Count;

                using (Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(500)).Timestamp()
                    .Subscribe(x => StatusCallback(LVitemProjectVM, nProgress: nProgressNumerator/nProgressDenominator)))
                {
                    var dictHash = new ConcurrentDictionary<string, Tuple<HashTuple, HashTuple>>();
                    var dictException_FileRead = new ConcurrentDictionary<string, string>();

                    // Parallel works great for SSDs; not so great for HDDs.
                    Parallel.ForEach(ReadBuffers(OpenFiles(listFilePaths)),
                        new ParallelOptions { MaxDegreeOfParallelism = bSSD ? 4 : 1 }, tuple =>
                    {
                        if (_bThreadAbort)
                            return;                     // from lambda Parallel.ForEach

                        var strFile = tuple.Item1;
                        Interlocked.Increment(ref nProgressNumerator);

                        dictHash[strFile] =
                            Util.Closure(() =>
                        {
                            var retval = Tuple.Create(default(HashTuple), default(HashTuple));

                            if (null != tuple.Item3)
                            {
                                dictException_FileRead[strFile] = tuple.Item3;
                                return retval;          // from lambda Util.Closure
                            }

                            var lsBuffer = tuple.Item4;

                            if (0 == lsBuffer.Count)
                            {
                                MBoxStatic.Assert(99932, false);
                                return retval;          // from lambda Util.Closure
                            }

                            using (var md5 = MD5.Create())
                            {
                                var hash1pt0 = HashTuple.FactoryCreate(md5.ComputeHash(lsBuffer[0]));

                                retval = Tuple.Create(hash1pt0, hash1pt0);

                                if (1 == lsBuffer.Count)
                                    return retval;      // from lambda Util.Closure

                                var lsHash = new List<byte[]>();

                                foreach (var buffer in lsBuffer.Skip(1))
                                {
                                    const int knBuffLength = 4096;
                                    var nLastPos = buffer.Length - knBuffLength;
                                    var nOffset = 0;

                                    for (; nOffset <= nLastPos; nOffset += knBuffLength)
                                        lsHash.Add(md5.ComputeHash(buffer, nOffset, knBuffLength));

                                    if (buffer.Length > nOffset)
                                        lsHash.Add(md5.ComputeHash(buffer, nOffset, buffer.Length - nOffset));
                                }

                                var hashArray = new byte[lsHash.Count * 16];
                                var nIx = 0;

                                foreach (var hash in lsHash)
                                    hash.CopyTo(hashArray, nIx++ * 16);

                                return Tuple.Create(hash1pt0, HashTuple.FactoryCreate(md5.ComputeHash(hashArray)));          // from lambda Util.Closure
                            }
                        });
                    });

                    dictHash_out = dictHash.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                    dictException_FileRead_out = dictException_FileRead.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }

                StatusCallback(LVitemProjectVM, nProgress: 1);
            }

            void Go()
            {
                if (false == IsGoodDriveSyntax(LVitemProjectVM.SourcePath))
                {
                    StatusCallback(LVitemProjectVM, strError: "Bad drive syntax.");
                    return;
                }

                if (false == Directory.Exists(LVitemProjectVM.SourcePath))
                {
                    StatusCallback(LVitemProjectVM, strError: "Source Path does not exist.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(LVitemProjectVM.ListingFile))
                {
                    LVitemProjectVM.ListingFile =
                        ProjectFile.TempPath +
                        LVitemProjectVM.SourcePath[0] + "_Listing_" +
                        Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + "." + ksFileExt_Listing;
                }

                var strPathOrig = Directory.GetCurrentDirectory();

                Directory.CreateDirectory(Path.GetDirectoryName(LVitemProjectVM.ListingFile));

                try
                {
                    using (TextWriter fs = File.CreateText(LVitemProjectVM.ListingFile))
                    {
                        IReadOnlyDictionary<string, Tuple<HashTuple, HashTuple>> dictHash = null;
                        IReadOnlyDictionary<string, string> dictException_FileRead = null;

                        WriteHeader(fs);
                        fs.WriteLine();
                        fs.WriteLine(FormatString(nHeader: 0));
                        fs.WriteLine(FormatString(nHeader: 1));
                        fs.WriteLine(ksStart01 + " " + DateTime.Now);

                        bool bSSD = ("" + LVitemProjectVM.DriveModel).Contains("SSD");

                        Hash(GetFileList(), out dictHash, out dictException_FileRead, bSSD);
                        WriteDirectoryListing(fs, dictHash, dictException_FileRead);
                        fs.WriteLine(ksEnd01 + " " + DateTime.Now);
                        fs.WriteLine();
                        fs.WriteLine(ksErrorsLoc01);

                        // Unit test metrix on non-system volume
                        //MBox.Assert(99893, nProgressDenominator >= nProgressNumerator);       file creation/deletion between times
                        //MBox.Assert(99892, nProgressDenominator == m_nFilesDiff);             ditto
                        //MBox.Assert(99891, nProgressDenominator == dictHash.Count);           ditto

                        foreach (var strError in ErrorList)
                            fs.WriteLine(strError);

                        fs.WriteLine();
                        fs.WriteLine(FormatString(strDir: ksTotalLengthLoc01, nLength: LengthRead));
                    }

                    if (App.LocalExit ||
                        _bThreadAbort)
                    {
                        File.Delete(LVitemProjectVM.ListingFile);
                        return;
                    }

                    Directory.SetCurrentDirectory(strPathOrig);
                    StatusCallback(LVitemProjectVM, bDone: true);
                }
#if DEBUG == false
                catch (Exception e)
                {
                    StatusCallback(LVitemProjectVM, strError: e.GetBaseException().Message, bDone: true);
                }
#endif
                finally { }
            }

            void StatusCallback(LVitem_ProjectVM lvItemProjectVM, string strError = null, bool bDone = false, double nProgress = double.NaN)
            {
                if (null == _saveDirListingsStatus)
                {
                    MBoxStatic.Assert(99870, false);
                    return;
                }

                _saveDirListingsStatus.Status(lvItemProjectVM, strError, bDone, nProgress);
            }

            readonly ISaveDirListingsStatus
                _saveDirListingsStatus = null;
            Thread
                _thread = null;
        }
    }
}
