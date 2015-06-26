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
using System.Windows.Threading;

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

            void StatusCallback(LVitem_ProjectVM lvItemProjectVM, string strError = null, bool bDone = false, double nProgress = double.NaN)
            {
                if (null == _saveDirListingsStatus)
                {
                    MBoxStatic.Assert(99870, false);
                    return;
                }

                _saveDirListingsStatus.Status(lvItemProjectVM, strError, bDone, nProgress);
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
                        HashAllFiles(GetFileList(), out dictHash, out dictException_FileRead);
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

            void HashAllFiles(IReadOnlyList<Tuple<string, ulong>> lsFilePaths,
                out IReadOnlyDictionary<string, Tuple<HashTuple, HashTuple>> dictHash_out,
                out IReadOnlyDictionary<string, string> dictException_FileRead_out)
            {
                if (null == lsFilePaths)
                {
                    dictHash_out = null;
                    dictException_FileRead_out = null;
                    return;
                }

                // Default cluster size on any NTFS volume up to 16TB is 4K
                // Maximize hash buffers while reducing CreateFile() and fs.Read() calls.
                long nProgressNumerator = 0;
                double nProgressDenominator = lsFilePaths.Count;  // double preserves mantissa

                using (Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(500)).Timestamp()
                    .Subscribe(x => StatusCallback(LVitemProjectVM, nProgress: nProgressNumerator/nProgressDenominator)))
                {
                    var lsFileHandles = new ConcurrentBag<Tuple<string, ulong, SafeFileHandle, string>> { };
                    var blockUntilAllFilesOpened = new DispatcherFrame(true) { Continue = true };
                    var bAllFilesOpened = false;
                    var cts = new CancellationTokenSource();

                    Util.ThreadMake(() =>
                    {
                        Parallel.ForEach(lsFilePaths,
                            new ParallelOptions
                            {
                                MaxDegreeOfParallelism = Environment.ProcessorCount,
                                CancellationToken = cts.Token
                            },
                            tuple =>
                        {
                            lsFileHandles.Add(OpenFile(tuple));
                        });

                        bAllFilesOpened = true;
                    });

                    var dictHash = new ConcurrentDictionary<string, Tuple<HashTuple, HashTuple>> { };
                    var dictException_FileRead = new ConcurrentDictionary<string, string> { };
                    var blockWhileHashingPreviousBatch = new DispatcherFrame(true) { Continue = false };
                    var bEnqueued = true;

                    // The above ThreadMake will be busy pumping out new file handles while the below processes will
                    // read those files' buffers and simultaneously hash them in batches until all files have been opened.
                    while ((false == bAllFilesOpened) ||
                        (0 < lsFileHandles.Count) ||
                        bEnqueued)
                    {
                        // Avoid spinning too quickly while waiting for new file handles.
                        Util.Block(100);

                        var lsOpenedFiles = new List<Tuple<string, ulong, SafeFileHandle, string>> { };

                        while (4096 > lsOpenedFiles.Count)
                        {
                            Tuple<string, ulong, SafeFileHandle, string> tupleA = null;

                            lsFileHandles.TryTake(out tupleA);

                            if (null == tupleA)
                                break;

                            lsOpenedFiles.Add(tupleA);
                        }

                        // ToList() enumerates ReadBuffers() sequentially, reading disk I/O buffers one at a time.
                        // Up to proc count + 1 accesses to one disk are occurring simultaneously:
                        // CreateFile() x proc count and fs.Read() x1.
                        var lsFileBuffers_Enqueue =
                            ReadBuffers(lsOpenedFiles)
                            .ToList();

                        // Expect block to be false: reading buffers from disk is The limiting factor. Opening files is
                        // slow too, which makes it even less likely to block. Allow block. It does get hit a handful of times.
                        Dispatcher.PushFrame(blockWhileHashingPreviousBatch);
                        blockWhileHashingPreviousBatch.Continue = true;

                        // in C# this copy occurs every iteration. A closure is created each time in ThreadMake.
                        // The closure along with the block being false should make the copy unnecessary but just in case.
                        var lsFileBuffers_Dequeue =
                            lsFileBuffers_Enqueue
                            .ToArray();

                        // While reading in the next batch of buffers, hash this batch. Three processes are occurring
                        // simultaneously: opening all files on disk; reading in all buffers from files; hashing all buffers.
                        // So the time it takes to complete saving listings should just be the sum of reading buffers from disk.
                        // There is a miniscule start-up time, and a larger wind-down time, which is saving the listings to file.
                        Util.ThreadMake(() =>
                        {
                            Parallel.ForEach(lsFileBuffers_Dequeue, new ParallelOptions { CancellationToken = cts.Token }, tuple =>
                            {
                                if (_bThreadAbort)
                                {
                                    cts.Cancel();
                                    bAllFilesOpened = true;
                                    return;     // from lambda Parallel.ForEach
                                }

                                var strFile = tuple.Item1;

                                if (null != tuple.Item3)
                                    dictException_FileRead[strFile] = tuple.Item3;
                                
                                dictHash[strFile] = HashFile(tuple);
                            });

                            nProgressNumerator += lsFileBuffers_Dequeue.Length;
                            blockWhileHashingPreviousBatch.Continue = false;

                            if (bAllFilesOpened)
                                blockUntilAllFilesOpened.Continue = false;
                        });

                        bEnqueued = (false == lsFileBuffers_Enqueue.IsEmpty());
                    }

                    Dispatcher.PushFrame(blockUntilAllFilesOpened);
                    dictHash_out = dictHash.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                    dictException_FileRead_out = dictException_FileRead.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }

                StatusCallback(LVitemProjectVM, nProgress: 1);
            }

            Tuple<string, ulong, SafeFileHandle, string>
                OpenFile(Tuple<string, ulong> tuple)
            {
                var strFile = tuple.Item1;
                var fileHandle = new SafeFileHandle(IntPtr.Zero, false);
                string strError = null;

                fileHandle.SetHandleAsInvalid();

                if (0 < tuple.Item2)       // non-empty file
                {
                    fileHandle =
                        NativeMethods
                        .CreateFile(@"\\?\" + strFile, FileAccess.Read, FileShare.ReadWrite, IntPtr.Zero,
                        NativeMethods.OPEN_EXISTING,
                        (FileAttributes)NativeMethods.FILE_FLAG_RANDOM_ACCESS, IntPtr.Zero);

                    if (fileHandle.IsInvalid)
                        strError = new Win32Exception(Marshal.GetLastWin32Error()).Message;
                }

                return Tuple.Create(strFile, tuple.Item2, fileHandle, strError);
            }

            IEnumerable<Tuple<string, ulong, string, IReadOnlyList<byte[]>>>
                ReadBuffers(IEnumerable<Tuple<string, ulong, SafeFileHandle, string>> listFileHandles)
            {
                foreach (var tuple in listFileHandles)
                {
                    var lsRet = new List<byte[]> { };
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
                        lsRet.Add(new byte[4096]);

                        const int knBigBuffLength = 65536;
                        var bFilled = FillBuffer(fs, knBigBuffLength, lsRet);

                        if (0 < lsRet[1].Length)
                        {
                            // virtually always: all non-empty files
                            Array.Copy(lsRet[1], lsRet[0], Math.Min(lsRet[1].Length, lsRet[0].Length));

                            if (lsRet[1].Length <= lsRet[0].Length)
                                lsRet.RemoveAt(1);
                        }
                        else
                        {
                            // virtually never: file emptied after being catalogued
                            lsRet.Clear();
                            MBoxStatic.Assert(99945, false == bFilled);
                        }

                        if (false == bFilled)
                            return;     // from lambda

                        var desiredPos = fs.Length - knBigBuffLength;

                        if (desiredPos > fs.Position)
                        {
                            MBoxStatic.Assert(99931, knBigBuffLength == fs.Position);
                            fs.Position = desiredPos;
                        }

                        if (false == FillBuffer(fs, knBigBuffLength, lsRet))
                            return;     // from lambda
                    });

                    yield return retval;
                }
            }

            bool FillBuffer(FileStream fs, int nBufferSize, IList<byte[]> lsBuffer)
            {
                var readBuffer = new byte[nBufferSize];
                var nRead = fs.Read(readBuffer, 0, nBufferSize);

                if (nRead < nBufferSize)
                {
                    // works fine with nRead == 0
                    var truncBuffer = new byte[nRead];

                    Array.Copy(readBuffer, truncBuffer, nRead);
                    readBuffer = truncBuffer;
                }

                lsBuffer.Add(readBuffer);

                if (0 == nRead)
                    return false;   // file was emptied since being catalogued

                bool bMoreToRead = fs.Position < fs.Length;
                
                if (bMoreToRead)
                    MBoxStatic.Assert(99926, nRead == nBufferSize);

                return bMoreToRead;
            }

            Tuple<HashTuple, HashTuple>
                HashFile(Tuple<string, ulong, string, IReadOnlyList<byte[]>> tuple)
            {
                var retval = Tuple.Create(default(HashTuple), default(HashTuple));
                var lsBuffer = tuple.Item4;

                if ((0 == tuple.Item2) ||       // empty file
                    (null != tuple.Item3))      // bad file handle, with error string
                {
                    MBoxStatic.Assert(99911, 0 == lsBuffer.Count);
                    return retval;
                }

                if (0 == lsBuffer.Count)
                {
                    MBoxStatic.Assert(99932, false);
                    return retval;
                }

                using (var md5 = MD5.Create())
                {
                    var hash1pt0 = HashTuple.FactoryCreate(md5.ComputeHash(lsBuffer[0]));

                    retval = Tuple.Create(hash1pt0, hash1pt0);

                    if (1 == lsBuffer.Count)
                        return retval;

                    var nSize = 0;

                    foreach (var buffer in lsBuffer.Skip(1))
                        nSize += buffer.Length;

                    var hashArray = new byte[nSize];
                    var nIx = 0;

                    foreach (var buffer in lsBuffer.Skip(1))
                    {
                        buffer.CopyTo(hashArray, nIx);
                        nIx += buffer.Length;
                    }

                    return Tuple.Create(hash1pt0,
                        HashTuple.FactoryCreate(md5.ComputeHash(hashArray)));
                }
            }

            readonly ISaveDirListingsStatus
                _saveDirListingsStatus = null;
            Thread
                _thread = null;
        }
    }
}
