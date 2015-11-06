﻿using System;
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
using System.Windows;

namespace DoubleFile
{
    partial class SaveDirListings
    {
        static internal bool Hash = true;   // use-case: VolTreemap project

        class SaveDirListing : TraverseTreeBase
        {
            internal
                SaveDirListing(
                LVitem_ProjectVM volStrings,
                ISaveDirListingsStatus saveDirListingsStatus)
                : base(volStrings)
            {
                _saveDirListingsStatus = saveDirListingsStatus;
            }

            internal SaveDirListing DoThreadFactory()
            {
                _thread = Util.ThreadMake(() => Go());
                return this;
            }

            internal void
                Join() => _thread.Join();

            internal void Abort()
            {
                _bThreadAbort = true;
            }

            void StatusCallback(LVitem_ProjectVM lvItemProjectVM, string strError = null, bool bDone = false, double nProgress = double.NaN)
            {
                if (null == _saveDirListingsStatus)
                {
                    Util.Assert(99870, false);
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
                        LocalIsoStore.TempDir +
                        LVitemProjectVM.SourcePath[0] + "_Listing_" +
                        Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + "." + ksFileExt_Listing;
                }

                if (LocalIsoStore.FileExists(LVitemProjectVM.ListingFile))
                    LocalIsoStore.DeleteFile(LVitemProjectVM.ListingFile);

                try
                {
                    _bDowngrade4K_Slow = false;
                    var hash = Hash ? HashAllFiles(GetFileList()) : null;

                    Util.WriteLine("hashed " + LVitemProjectVM.SourcePath);

                    using (var sw = new StreamWriter(LocalIsoStore.CreateFile(LVitemProjectVM.ListingFile)))
                    {
                        WriteHeader(sw);
                        sw.WriteLine();
                        sw.WriteLine(FormatString(nHeader: 0));
                        sw.WriteLine(FormatString(nHeader: 1));
                        sw.WriteLine(ksStart01 + " " + DateTime.Now);
                        WriteDirectoryListing(sw, hash);
                        sw.WriteLine(ksEnd01 + " " + DateTime.Now);
                        sw.WriteLine();
                        sw.WriteLine(ksErrorsLoc01);

                        // Unit test metrix on non-system volume
                        //MBox.Assert(99893, nProgressDenominator >= nProgressNumerator);       file creation/deletion between times
                        //MBox.Assert(99892, nProgressDenominator == m_nFilesDiff);             ditto
                        //MBox.Assert(99891, nProgressDenominator == dictHash.Count);           ditto

                        foreach (var strError in ErrorList)
                            sw.WriteLine(strError);

                        sw.WriteLine();
                        sw.WriteLine(FormatString(strDir: ksTotalLengthLoc01, nLength: LengthRead));
                    }

                    if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true) ||
                        _bThreadAbort)
                    {
                        LocalIsoStore.DeleteFile(LVitemProjectVM.ListingFile);
                        return;
                    }

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
                var strModel = LVitemProjectVM.DriveModel;
                var strSerial = LVitemProjectVM.DriveSerial;

                // at minimum get the drive size
                var nSize = DriveSerialStatic.Get(LVitemProjectVM.SourcePath, ref strModel, ref strSerial);

                fs.WriteLine(ksDrive01);

                var driveInfo = new DriveInfo(LVitemProjectVM.SourcePath[0] + @":\");
                var sb = new StringBuilder();
                var nCount = 0;

                Action<object> WriteLine = o =>
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
                Util.Assert(99941, nCount == knDriveInfoItems);
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
                long nPrevProgress = 0;
                var lsProgress = new List<long> { };
                var bCompleted = false;

                using (Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(500)).Timestamp()
                    .LocalSubscribe(99721, x =>
                {
                    StatusCallback(LVitemProjectVM, nProgress: nProgressNumerator / nProgressDenominator);

                    var nDiff = nProgressNumerator - nPrevProgress;

                    lsProgress.Add(nDiff);
                    Util.Write(" " + nDiff);
                    nPrevProgress = nProgressNumerator;
                }))
                {
                    var lsFileHandles = new ConcurrentBag<Tuple<string, ulong, SafeFileHandle, string>>();
                    var cts = new CancellationTokenSource();

                    Util.ThreadMake(() =>
                    {
                        Util.ParallelForEach(99658, lsFilePaths,
                            new ParallelOptions
                        {
                            MaxDegreeOfParallelism = Environment.ProcessorCount,
                            CancellationToken = cts.Token
                        },
                            tuple =>
                        {
                            lsFileHandles.Add(OpenFile(tuple));

                            if (_bDowngrade4K_Slow)
                            {
                                while (0 < lsFileHandles.Count)
                                    Util.Block(1);
                            }

                            if (_bThreadAbort)
                                cts.Cancel();
                        });

                        bCompleted = true;
                    });

                    var dictHash = new ConcurrentDictionary<string, Tuple<HashTuple, HashTuple>> { };
                    var dictException_FileRead = new ConcurrentDictionary<string, string> { };
                    var dtDowngrade4K_Slow = DateTime.MinValue;
                    var bSetDowngrade4K_Slow = 0;
                    var blockWhileHashingPreviousBatch = new LocalDispatcherFrame(99872) { Continue = false };

                    // The above ThreadMake will be busy pumping out new file handles while the below processes will
                    // read those files' buffers and simultaneously hash them in batches until all files have been opened.
                    while (nProgressNumerator < nProgressDenominator)
                    {
                        if (_bThreadAbort)
                            break;

                        if (bCompleted)
                            Util.Block(100);

                        var lsOpenedFiles = new List<Tuple<string, ulong, SafeFileHandle, string>> { };

                        for (int i = 0; (1 << 9) > i; ++i)
                        {
                            Tuple<string, ulong, SafeFileHandle, string>
                                tupleA = null;

                            lsFileHandles.TryTake(out tupleA);

                            if (null == tupleA)
                                break;

                            lsOpenedFiles.Add(tupleA);
                        }

                        IReadOnlyList<Tuple<string, ulong, string, IReadOnlyList<IReadOnlyList<byte>>>>
                            lsFileBuffers_Enqueue = null;

                        try
                        {
                            // ToList() enumerates ReadBuffers() sequentially, reading disk I/O buffers one at a time.
                            // Up to proc count + 1 accesses to one disk are occurring simultaneously:
                            // CreateFile() x proc count and fs.Read() x1.
                            lsFileBuffers_Enqueue =
                                ReadBuffers(lsOpenedFiles)
                                .ToList();
                        }
                        catch (ThreadAbortException) { }
                        catch (Exception e)
                        {
#if (DEBUG)
                            Util.Assert(99677, false, e.GetBaseException().Message);
#endif
                            StatusCallback(LVitemProjectVM, strError: e.GetBaseException().Message);
                            break;
                        }

                        if (false == _bDowngrade4K_Slow)
                        {
                            if (10 < (DateTime.Now - dtDowngrade4K_Slow).Seconds)
                                ++bSetDowngrade4K_Slow;
                            else
                                bSetDowngrade4K_Slow = 0;
                        }

                        // Expect block to be false: reading buffers from disk is The limiting factor. Opening files is
                        // slow too, which makes it even less likely to block. Allow block. It does get hit a handful of times.
                        if (blockWhileHashingPreviousBatch.Continue)
                            blockWhileHashingPreviousBatch.PushFrameTrue();

                        blockWhileHashingPreviousBatch.Continue = true;

                        // in C# this copy occurs every iteration. A closure is created each time in ThreadMake.
                        // The closure along with the block being false should make the copy unnecessary but just in case.
                        var lsFileBuffers_Dequeue =
                            lsFileBuffers_Enqueue
                            .ToList();

                        if ((false == _bDowngrade4K_Slow) &&
                            (3 < bSetDowngrade4K_Slow))
                        {
                            Util.WriteLine("\nbSetDowngrade4K_Slow = true;");

                            // null 1M hash signals entire project to downgrade when building view
                            _bDowngrade4K_Slow = true;

                            foreach (var kvp in dictHash)
                                dictHash[kvp.Key] = Tuple.Create(kvp.Value.Item1, (HashTuple)null);
                        }
                        else
                        {
                            dtDowngrade4K_Slow = DateTime.Now;
                        }

                        // While reading in the next batch of buffers, hash this batch. Three processes are occurring
                        // simultaneously: opening all files on disk; reading in all buffers from files; hashing all buffers.
                        // So the time it takes to complete saving listings should just be the sum of reading buffers from disk.
                        // There is a miniscule start-up time, and a larger wind-down time, which is saving the listings to file.
                        Util.ThreadMake(() =>
                        {
                            Util.ParallelForEach(99657, lsFileBuffers_Dequeue, new ParallelOptions { CancellationToken = cts.Token }, tuple =>
                            {
                                var strFile = tuple.Item1;

                                if (null != tuple.Item3)
                                    dictException_FileRead[strFile] = tuple.Item3;
                                
                                dictHash[strFile] = HashFile(tuple);
#if (DEBUG && FOOBAR)
                                var check = HashFile(tuple);
                                var hash = dictHash[strFile];
                                Util.Assert(99578, "" + check.Item1 == "" + hash.Item1);
                                Util.Assert(99577, "" + check.Item2 == "" + hash.Item2);
#endif
                                if (_bThreadAbort)
                                    cts.Cancel();
                            });

                            nProgressNumerator += lsFileBuffers_Dequeue.Count;
                            blockWhileHashingPreviousBatch.Continue = false;
                        });

                        if (cts.IsCancellationRequested)
                            break;
                    }

                    double nAverage = 0;

                    foreach (var n in lsProgress) nAverage += n;
                    nAverage /= lsProgress.Count;
                    Util.WriteLine("\n--- " + nAverage);

                    if (_bThreadAbort)
                    {
                        StatusCallback(LVitemProjectVM, "Canceled");
                        return null;
                    }

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

            IEnumerable<Tuple<string, ulong, string, IReadOnlyList<IReadOnlyList<byte>>>>
                ReadBuffers(IEnumerable<Tuple<string, ulong, SafeFileHandle, string>> ieFileHandles)
            {
                foreach (var tuple in ieFileHandles)
                {
                    var lsRet = new List<byte[]> { };

                    var retVal = Tuple.Create(tuple.Item1, tuple.Item2, tuple.Item4,
                        (IReadOnlyList<IReadOnlyList<byte>>)lsRet);

                    var fileHandle = tuple.Item3;

                    if (fileHandle.IsInvalid)
                    {
                        fileHandle.Dispose();
                        yield return retVal;
                        continue;
                    }

                    using (fileHandle)
                    using (var fs = new FileStream(fileHandle, FileAccess.Read))
                    Util.Closure(() =>
                    {
                        lsRet.Add(new byte[1 << 12]);          // happens to be block size

                        var bFilled = FillBuffer(fs, lsRet);

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
                            Util.Assert(99945, false == bFilled);
                        }

                        if ((false == bFilled) ||
                            _bDowngrade4K_Slow)
                        {
                            return;     // from lambda
                        }

                        var desiredPos = fs.Length - (1 << 19);

                        if (desiredPos > fs.Position)
                        {
                            Util.Assert(99931, (1 << 19) == fs.Position);
                            desiredPos += ((1 << 12) - (desiredPos & ((1 << 12) - 1)));       // align to block boundary if possible
                            Util.Assert(99914, 0 == (desiredPos & ((1 << 12) - 1)));
                            fs.Position = desiredPos;
                        }

                        if (false == FillBuffer(fs, lsRet))
                            return;     // from lambda
                    });

                    yield return retVal;
                }
            }

            bool FillBuffer(FileStream fs, IList<byte[]> lsBuffer)
            {
                var nBufferSize = 1 << (_bDowngrade4K_Slow ? 12 : 19);
                var readBuffer = new byte[nBufferSize];
                var nRead = fs.Read(readBuffer, 0, nBufferSize);

                if (nRead < nBufferSize)
                {
                    // works fine with 0 == nRead
                    var truncBuffer = new byte[nRead];

                    Array.Copy(readBuffer, truncBuffer, nRead);
                    readBuffer = truncBuffer;
                }

                lsBuffer.Add(readBuffer);

                if (0 == nRead)
                    return false;   // file was emptied since being catalogued

                var bMoreToRead = fs.Position < fs.Length;
                
                if (bMoreToRead)
                    Util.Assert(99926, nRead == nBufferSize);

                return bMoreToRead;
            }

            Tuple<HashTuple, HashTuple>
                HashFile(Tuple<string, ulong, string, IReadOnlyList<IReadOnlyList<byte>>> tuple)
            {
                var lsBuffer = tuple.Item4;
                var nCount = lsBuffer.Count;

                if ((0 == tuple.Item2) ||       // empty file
                    (null != tuple.Item3))      // bad file handle, with error string
                {
                    Util.Assert(99911, 0 == nCount);
                    return Tuple.Create((HashTuple)null, (HashTuple)null);
                }

                if (0 == nCount)
                {
                    Util.Assert(99932, false);
                    return Tuple.Create((HashTuple)null, (HashTuple)null);
                }

                using (var md5 = MD5.Create())
                {
                    var buffer0 = (byte[])lsBuffer[0];
                    var hash1pt0 = HashTuple.FactoryCreate(md5.ComputeHash(buffer0));

                    // null 1M hash signals entire project to downgrade when building view
                    // has to come before 1 == nCount since small files are valid for all 1M hashes
                    if (_bDowngrade4K_Slow)
                        return Tuple.Create(hash1pt0, (HashTuple)null);

                    if (1 == nCount)
                        return Tuple.Create(hash1pt0, hash1pt0);

                    var nSize = 0;

                    foreach (byte[] buffer in lsBuffer.Skip(1))
                        nSize += buffer.Length;

                    Util.Assert(99909, (1 << 20) >= nSize);

                    var hashArray = new byte[nSize];
                    var nIx = 0;

                    foreach (byte[] buffer in lsBuffer.Skip(1))
                    {
                        Array.Copy(buffer, 0, hashArray, nIx, buffer.Length);
                        nIx += buffer.Length;
                    }

                    return Tuple.Create(hash1pt0,
                        HashTuple.FactoryCreate(md5.ComputeHash(hashArray)));
                }
            }

            bool
                _bDowngrade4K_Slow = false;
            readonly ISaveDirListingsStatus
                _saveDirListingsStatus = null;
            Thread
                _thread = new Thread(() => { });
        }
    }
}
