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
                        WriteHeader(fs);
                        fs.WriteLine();
                        fs.WriteLine(FormatString(nHeader: 0));
                        fs.WriteLine(FormatString(nHeader: 1));
                        fs.WriteLine(ksStart01 + " " + DateTime.Now);
                        WriteDirectoryListing(fs, HashAllFiles(GetFileList()));
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

                // strModel and strSerial are not as throwaway as they look: this file will be read in when done
                string strModel = LVitemProjectVM.DriveModel;
                string strSerial = LVitemProjectVM.DriveSerial;

                // at minimum get the drive size
                ulong? nSize = DriveSerialStatic.Get(LVitemProjectVM.SourcePath, ref strModel, ref strSerial);

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

            Tuple<IReadOnlyDictionary<string, Tuple<HashTuple, HashTuple>>, IReadOnlyDictionary<string, string>>
                HashAllFiles(IReadOnlyList<Tuple<string, ulong>> lsFilePaths)
            {
                if (null == lsFilePaths)
                    return null;

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
                        Util.ParallelForEach(lsFilePaths,
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

                        for (int i = 0; i < 4096; ++i)
                        {
                            Tuple<string, ulong, SafeFileHandle, string> tupleA = null;

                            lsFileHandles.TryTake(out tupleA);

                            if (null == tupleA)
                                break;

                            lsOpenedFiles.Add(tupleA);
                        }

                        var lsFileBuffers_Enqueue =
                            default(IEnumerable<Tuple<string, ulong, string, IReadOnlyList<byte[]>>>);

                        try
                        {
                            // ToList() enumerates ReadBuffers() sequentially, reading disk I/O buffers one at a time.
                            // Up to proc count + 1 accesses to one disk are occurring simultaneously:
                            // CreateFile() x proc count and fs.Read() x1.
                            lsFileBuffers_Enqueue =
                                ReadBuffers(lsOpenedFiles)
                                .ToList();
                        }
                        catch (Exception e)
                        {
                            StatusCallback(LVitemProjectVM, strError: e.GetBaseException().Message);
                            return null;
                        }

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
                            Util.ParallelForEach(lsFileBuffers_Dequeue, new ParallelOptions { CancellationToken = cts.Token }, tuple =>
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
                    StatusCallback(LVitemProjectVM, nProgress: 1);
                    
                    return Tuple.Create(
                        (IReadOnlyDictionary<string, Tuple<HashTuple, HashTuple>>)
                            dictHash.ToDictionary(kvp => kvp.Key, kvp => kvp.Value),
                        (IReadOnlyDictionary<string, string>)
                            dictException_FileRead.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
                }
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
                        const int knBigBuffLength = 524288;

                        lsRet.Add(new byte[4096]);          // happens to be block size
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
                            desiredPos += (4096 - (desiredPos & 4095));       // align to block boundary if possible
                            MBoxStatic.Assert(99914, 0 == (desiredPos & 4095));
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

                var nCount = lsBuffer.Count;

                if (0 == nCount)
                {
                    MBoxStatic.Assert(99932, false);
                    return retval;
                }

                using (var md5 = MD5.Create())
                {
                    var hash1pt0 = HashTuple.FactoryCreate(md5.ComputeHash(lsBuffer[0]));

                    retval = Tuple.Create(hash1pt0, hash1pt0);

                    if (1 == nCount)
                        return retval;

                    var nSize = 0;

                    foreach (var buffer in lsBuffer.Skip(1))
                        nSize += buffer.Length;

                    MBoxStatic.Assert(99909, 1048576 >= nSize);

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
