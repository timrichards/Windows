﻿using System;
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
            internal SaveDirListing(GlobalData_Base gd_in,
                LVitem_ProjectVM volStrings, 
                SaveDirListingsStatusDelegate statusCallback)
                : base(gd_in, volStrings)
            {
                m_statusCallback = statusCallback;
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
                DriveSerial.Get(LVitemProjectVM.SourcePath, out strModel, out strSerial, out nSize);

                if ((((false == string.IsNullOrWhiteSpace(LVitemProjectVM.DriveModel)) && (strModel != LVitemProjectVM.DriveModel)) ||
                    ((false == string.IsNullOrWhiteSpace(LVitemProjectVM.DriveSerial)) && (strSerial != LVitemProjectVM.DriveSerial))) &&
                    ((MBox.ShowDialog("Overwrite user-entered drive model and serial # for " + LVitemProjectVM.SourcePath[0] + @":\ ?", "Save Directory Listings",
                        System.Windows.MessageBoxButton.YesNo) ==
                        System.Windows.MessageBoxResult.No)))
                {
                    strModel = LVitemProjectVM.DriveModel;
                    strSerial = LVitemProjectVM.DriveSerial;
                }

                fs.WriteLine(ksDrive01);

                var driveInfo = new DriveInfo(LVitemProjectVM.SourcePath[0] + @":\");
                var sb = new StringBuilder();
                int nCount = 0;

                var WriteLine = new Action<Object>((o) =>
                {
                    var s = (o != null) ? o.ToString() : null;

                    // Hack. Prevent blank line continue in FileParse.ConvertFile()
                    sb.AppendLine(((s == null) || (s.Length <= 0)) ? " " : s.Trim());
                    ++nCount;
                });

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
                MBox.Assert(0, nCount == FileParse.knDriveInfoItems);
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
                    m_statusCallback(LVitemProjectVM, nProgress: nProgressNumerator / nProgressDenominator);
                }), null, timeSpan, timeSpan);

                var dictHash = new Dictionary<string, string>();
                var dictException_FileRead = new Dictionary<string, string>();

                Parallel.ForEach(listFilePaths, strFile =>
                {
                    if (m_bThreadAbort)
                    {
                        return;                         // return from lambda
                    }

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
                m_statusCallback(LVitemProjectVM, nProgress: 1);
            }

            void Go()
            {
                if (IsGoodDriveSyntax(LVitemProjectVM.SourcePath) == false)
                {
                    m_statusCallback(LVitemProjectVM, strError: "Bad drive syntax.");
                    return;
                }

                if (Directory.Exists(LVitemProjectVM.SourcePath) == false)
                {
                    m_statusCallback(LVitemProjectVM, strError: "Source Path does not exist.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(LVitemProjectVM.ListingFile))
                {
                    LVitemProjectVM.ListingFile = ProjectFile.TempPath + LVitemProjectVM.SourcePath[0] + "_Listing_" +
                        Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + "." + ksFileExt_Listing;
                }

                string strPathOrig = Directory.GetCurrentDirectory();

                Directory.CreateDirectory(Path.GetDirectoryName(LVitemProjectVM.ListingFile));

                try
                {
                    using (TextWriter fs = File.CreateText(LVitemProjectVM.ListingFile))
                    {
                        Dictionary<string, string> dictHash = null;
                        Dictionary<string, string> dictException_FileRead = null;

                        WriteHeader(fs);
                        fs.WriteLine();
                        fs.WriteLine(FormatString(nHeader: 0));
                        fs.WriteLine(FormatString(nHeader: 1));
                        fs.WriteLine(ksStart01 + " " + DateTime.Now.ToString());
                        Hash(GetFileList(), out dictHash, out dictException_FileRead);
                        WriteDirectoryListing(fs, dictHash, dictException_FileRead);
                        fs.WriteLine(ksEnd01 + " " + DateTime.Now.ToString());
                        fs.WriteLine();
                        fs.WriteLine(ksErrorsLoc01);

                        // Unit test metrix on non-system volume
                        //MBox.Assert(0, nProgressDenominator >= nProgressNumerator);       file creation/deletion between times
                        //MBox.Assert(0, nProgressDenominator == m_nFilesDiff);             ditto
                        //MBox.Assert(0, nProgressDenominator == dictHash.Count);           ditto

                        foreach (string strError in ErrorList)
                        {
                            fs.WriteLine(strError);
                        }

                        fs.WriteLine();
                        fs.WriteLine(FormatString(strDir: ksTotalLengthLoc01, nLength: LengthRead));
                    }

                    if (m_bThreadAbort || gd.WindowClosed)
                    {
                        File.Delete(LVitemProjectVM.ListingFile);
                        return;
                    }

                    Directory.SetCurrentDirectory(strPathOrig);
                    m_statusCallback(LVitemProjectVM, bDone: true);
                }
#if DEBUG == false
                catch (Exception e)
                {
                    m_statusCallback(m_volStrings.SourcePath, strText: ksNotSaved, bDone: true);
                    MBox.ShowDialog(m_volStrings.ListingFile.PadRight(100) + "\nException: " + e.Message, "Save Directory Listing");
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

            readonly SaveDirListingsStatusDelegate m_statusCallback = null;
            Thread m_thread = null;
        }
    }
}
