using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DoubleFile
{
    partial class SaveDirListings
    {
        class SaveDirListing : TraverseTree
        {
            readonly SaveDirListingsStatusDelegate m_statusCallback = null;
            Thread m_thread = null;

            internal SaveDirListing(LVitem_VolumeVM volStrings, SaveDirListingsStatusDelegate statusCallback)
                : base(volStrings)
            {
                m_statusCallback = statusCallback;
            }

            void WriteHeader(TextWriter fs)
            {
                fs.WriteLine(mSTRheader01);
                fs.WriteLine(m_volStrings.Nickname);
                fs.WriteLine(m_volStrings.SourcePath);

                var strModel = m_volStrings.DriveModel;
                var strSerialNo = m_volStrings.DriveSerial;
                int? nSize = null;

                if (string.IsNullOrWhiteSpace(strModel))
                {
                    DriveSerial.Get(m_volStrings.SourcePath, out strModel, out strSerialNo, out nSize);
                }

                if ((string.IsNullOrWhiteSpace(m_volStrings.DriveSerial) == false) &&
                    (string.IsNullOrWhiteSpace(strSerialNo) == false) &&
                    (strSerialNo != m_volStrings.DriveSerial) &&
                    ((MBox.ShowDialog("Overwrite user-entered serial number for " + m_volStrings.SourcePath + " ?", "Save Directory Listings",
                        System.Windows.MessageBoxButton.YesNo) ==
                        System.Windows.MessageBoxResult.No)))
                {
                    strSerialNo = m_volStrings.DriveSerial;
                }

                fs.WriteLine(mSTRdrive01);

                var driveInfo = new DriveInfo(m_volStrings.SourcePath[0] + @":\");
                var sb = new StringBuilder();
                var WriteLine = new Action<Object>((o) =>
                {
                    var s = (o != null) ? o.ToString() : null;

                    // Hack. Prevent blank line continue in Utilities.Convert()
                    sb.AppendLine(((s == null) || (s.Length <= 0)) ? " " : s.Trim());
                });

                WriteLine(driveInfo.AvailableFreeSpace); // These could all be named better, so mAstrDIlabels is different.
                WriteLine(driveInfo.DriveFormat);        // Misnomer. Should be VolumeFormat.
                WriteLine(driveInfo.DriveType);
                WriteLine(driveInfo.Name);
                WriteLine(driveInfo.RootDirectory);
                WriteLine(driveInfo.TotalFreeSpace);
                WriteLine(driveInfo.TotalSize);
                WriteLine(driveInfo.VolumeLabel);
                WriteLine(strModel ?? "");
                WriteLine(strSerialNo ?? "");
                WriteLine(nSize ?? 0);

                fs.WriteLine(sb.ToString().Trim());
            }

            void Hash(IEnumerable<string> listFilePaths,
                out Dictionary<string, string> dictHash_out,
                out Dictionary<string, string> dictException_FileRead_out)
            {
                if (listFilePaths == null)
                {
                    dictHash_out = null;
                    dictException_FileRead_out = null;
                    return;
                }

                var nProgressNumerator = 0;
                double nProgressDenominator = listFilePaths.Count();        // double preserves mantissa
                var timeSpan = new TimeSpan(0, 0, 0, 1);

                System.Threading.Timer timer = new System.Threading.Timer(new TimerCallback((Object state) =>
                {
                    m_statusCallback(m_volStrings.SourcePath, nProgress: nProgressNumerator / nProgressDenominator);
                }), null, timeSpan, timeSpan);

                var dictHash = new Dictionary<string, string>();
                var dictException_FileRead = new Dictionary<string, string>();

                Parallel.ForEach(listFilePaths, strFile =>
                {
                    Interlocked.Increment(ref nProgressNumerator);

                    var fileHandle = DriveSerial.CreateFile(@"\\?\" + strFile, FileAccess.Read, FileShare.ReadWrite,
                        IntPtr.Zero, 3, 0, IntPtr.Zero);

                    if (fileHandle.IsInvalid)
                    {
                        dictException_FileRead[strFile] = new System.ComponentModel.Win32Exception(
                            System.Runtime.InteropServices.Marshal.GetLastWin32Error()).Message;
                        return;                         // return from lambda
                    }

                    using (var fsA = new FileStream(fileHandle, FileAccess.Read))
                    {
                        const int kBufferSize = 4096;   // Hash the first 4K of the file for speed
                        var buffer = new byte[kBufferSize];

                        fsA.Read(buffer, 0, kBufferSize);

                        using (var md5 = System.Security.Cryptography.MD5.Create())
                        {
                            var strHash = DRDigit.Fast.ToHexString(md5.ComputeHash(buffer));

                            lock (dictHash)
                            {
                                dictHash[strFile] = strHash;
                            }
                        }
                    }
                });

                timer.Dispose();
                dictHash_out = dictHash;
                dictException_FileRead_out = dictException_FileRead;
                m_statusCallback(m_volStrings.SourcePath, nProgress: 1);
            }

            void Go()
            {
                if (IsGoodDriveSyntax(m_volStrings.SourcePath) == false)
                {
                    MBox.ShowDialog("Bad drive syntax.", "Save Directory Listing");
                }

                if (Directory.Exists(m_volStrings.SourcePath) == false)
                {
                    m_statusCallback(m_volStrings.SourcePath, mSTRnotSaved);
                    MBox.ShowDialog("Source Path does not exist.", "Save Directory Listing");
                    return;
                }

                if (string.IsNullOrWhiteSpace(m_volStrings.ListingFile))
                {
                    m_statusCallback(m_volStrings.SourcePath, mSTRnotSaved);
                    MBox.ShowDialog("Must specify save filename.", "Save Directory Listing");
                    return;
                }

                string strPathOrig = Directory.GetCurrentDirectory();

                try
                {
                    using (TextWriter fs = File.CreateText(m_volStrings.ListingFile))
                    {
                        Dictionary<string, string> dictHash = null;
                        Dictionary<string, string> dictException_FileRead = null;

                        WriteHeader(fs);
                        fs.WriteLine();
                        fs.WriteLine(FormatString(nHeader: 0));
                        fs.WriteLine(FormatString(nHeader: 1));
                        fs.WriteLine(mSTRstart01 + " " + DateTime.Now.ToString());
                        Hash(GetFileList(), out dictHash, out dictException_FileRead);
                        WriteDirectoryListing(fs, dictHash, dictException_FileRead);
                        fs.WriteLine(mSTRend01 + " " + DateTime.Now.ToString());
                        fs.WriteLine();
                        fs.WriteLine(mSTRerrorsLoc01);

                        // Unit test metrix on non-system volume
                        //MBox.Assert(0, nProgressDenominator >= nProgressNumerator);       file creation/deletion between times
                        //MBox.Assert(0, nProgressDenominator == m_nFilesDiff);             ditto
                        //MBox.Assert(0, nProgressDenominator == dictHash.Count);           ditto

                        foreach (string strError in ErrorList)
                        {
                            fs.WriteLine(strError);
                        }

                        fs.WriteLine();
                        fs.WriteLine(FormatString(strDir: mSTRtotalLengthLoc01, nLength: LengthRead));
                    }

                    Directory.SetCurrentDirectory(strPathOrig);
                    m_statusCallback(m_volStrings.SourcePath, strText: mSTRsaved, bDone: true);
                }
#if DEBUG == false
                catch (Exception e)
                {
                    m_statusCallback(m_volStrings.Path, strText: mSTRnotSaved, bDone: true);
                    MBox.ShowDialog(strSaveAs.PadRight(100) + "\nException: " + e.Message, "Save Directory Listing");
                }
#endif
                finally { }
            }

            internal SaveDirListing DoThreadFactory()
            {
                m_thread = new Thread(new ThreadStart(Go));
                m_thread.IsBackground = true;
                m_thread.Start();
                return this;
            }

            internal void Join()
            {
                m_thread.Join();
            }

            internal void Abort()
            {
                m_bThreadAbort = true;
                m_thread.Abort();
            }
        }
    }
}
