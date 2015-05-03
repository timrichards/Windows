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

namespace DoubleFile
{
    partial class SaveDirListings
    {
        class SaveDirListing : TraverseTreeBase
        {
            internal SaveDirListing(LVitem_ProjectVM volStrings, WeakReference<ISaveDirListingsStatus> callbackWR)
                : base(volStrings)
            {
                _callbackWR = callbackWR;
            }

            internal SaveDirListing DoThreadFactory()
            {
                _thread = new Thread(Go) {IsBackground = true};
                _thread.Start();
                return this;
            }

            internal void Join()
            {
                _thread.Join();
            }

            internal void Abort()
            {
                m_bThreadAbort = true;
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

            void Hash(IEnumerable<string> listFilePaths,
                out ConcurrentDictionary<string, HashTuple> dictHash_out,
                out Dictionary<string, string> dictException_FileRead_out)
            {
                if (null == listFilePaths)
                {
                    dictHash_out = null;
                    dictException_FileRead_out = null;
                    return;
                }

                var nProgressNumerator = 0;
                double nProgressDenominator = listFilePaths.Count();        // double preserves mantissa

                using (Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(500)).Timestamp()
                    .Subscribe(x => StatusCallback(LVitemProjectVM, nProgress: nProgressNumerator/nProgressDenominator)))
                {
                    var dictHash = new ConcurrentDictionary<string, HashTuple>();
                    var dictException_FileRead = new Dictionary<string, string>();

                    Parallel.ForEach(listFilePaths, strFile =>
                    {
                        if (m_bThreadAbort)
                            return;     // from lambda

                        Interlocked.Increment(ref nProgressNumerator);

                        var fileHandle =
                            DriveSerialStatic
                            .CreateFile(@"\\?\" + strFile, FileAccess.Read, FileShare.ReadWrite, IntPtr.Zero, 3, 0, IntPtr.Zero);

                        if (fileHandle.IsInvalid)
                        {
                            dictException_FileRead[strFile] = new Win32Exception(Marshal.GetLastWin32Error()).Message;
                            return;     // from lambda
                        }

                        using (var fsA = new FileStream(fileHandle, FileAccess.Read))
                        {
                            const int kBufferSize = 4096; // Hash the first 4K of the file for speed
                            var buffer = new byte[kBufferSize];

                            fsA.Read(buffer, 0, kBufferSize);

                            using (var md5 = System.Security.Cryptography.MD5.Create())
                                dictHash[strFile] = HashTuple.FactoryCreate(md5.ComputeHash(buffer));
                        }
                    });

                    dictHash_out = dictHash;
                    dictException_FileRead_out = dictException_FileRead;
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
                    LVitemProjectVM.ListingFile = ProjectFile.TempPath +
                        LVitemProjectVM.SourcePath[0] + "_Listing_" +
                        Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + "." + ksFileExt_Listing;
                }

                var strPathOrig = Directory.GetCurrentDirectory();

                Directory.CreateDirectory(Path.GetDirectoryName(LVitemProjectVM.ListingFile));

                try
                {
                    using (TextWriter fs = File.CreateText(LVitemProjectVM.ListingFile))
                    {
                        ConcurrentDictionary<string, HashTuple> dictHash = null;
                        Dictionary<string, string> dictException_FileRead = null;

                        WriteHeader(fs);
                        fs.WriteLine();
                        fs.WriteLine(FormatString(nHeader: 0));
                        fs.WriteLine(FormatString(nHeader: 1));
                        fs.WriteLine(ksStart01 + " " + DateTime.Now);
                        Hash(GetFileList(), out dictHash, out dictException_FileRead);
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
                        m_bThreadAbort)
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
                    m_statusCallback(LVitemProjectVM, strError: e.GetBaseException().Message, bDone: true);
                }
#endif
                finally { }
            }

            void StatusCallback(LVitem_ProjectVM lvItemProjectVM, string strError = null, bool bDone = false, double nProgress = double.NaN)
            {
                if (null == _callbackWR)
                {
                    MBoxStatic.Assert(99871, false);
                    return;
                }

                ISaveDirListingsStatus saveDirListingsStatus = null;

                _callbackWR.TryGetTarget(out saveDirListingsStatus);

                if (null == saveDirListingsStatus)
                {
                    MBoxStatic.Assert(99870, false);
                    return;
                }

                saveDirListingsStatus.Status(lvItemProjectVM, strError, bDone, nProgress);
            }

            readonly WeakReference<ISaveDirListingsStatus>
                _callbackWR = null;
            Thread
                _thread = null;
        }
    }
}
