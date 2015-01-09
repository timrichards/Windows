using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Text;

namespace DoubleFile
{
    delegate void SaveDirListingsStatusDelegate(string strPath, string strText = null, bool bDone = false, double nProgress = double.NaN);

    class SaveListingsProcess
    {
        GlobalData gd = null;
        WinSaveInProgress m_winProgress = null;
        System.Windows.Window m_app = null;

        internal SaveListingsProcess(IEnumerable<LVitem_VolumeVM> list_lvVolStrings)
        {
            gd = GlobalData.GetInstance();
            m_app = GlobalData.static_wpfWin;

            var listNicknames = new List<string>();
            var listPaths = new List<string>();

            foreach (LVitem_VolumeVM volStrings in list_lvVolStrings)
            {
                if (SaveDirListings.WontSave(volStrings))
                {
                    continue;
                }

                listNicknames.Add(volStrings.VolumeName);
                listPaths.Add(volStrings.Path);
            }

            if (listPaths.Count > 0)
            {
                m_winProgress = new WinSaveInProgress();
                m_winProgress.InitProgress(listNicknames, listPaths);

                if ((gd.m_saveDirListings != null) && (gd.m_saveDirListings.IsAborted == false))
                {
                    MBox.Assert(0, false);
                    gd.m_saveDirListings.EndThread();
                }

                (gd.m_saveDirListings = new SaveDirListings(list_lvVolStrings,
                    SaveDirListingsStatusCallback, SaveDirListingsDoneCallback)).DoThreadFactory();
                m_winProgress.ShowDialog(m_app);
            }
        }

        internal void SaveDirListingsStatusCallback(string strPath, string strText = null, bool bDone = false, double nProgress = double.NaN)
        {
            if (GlobalData.AppExit || (gd.m_saveDirListings == null) || gd.m_saveDirListings.IsAborted)
            {
                return;
            }

            if (m_app.Dispatcher.CheckAccess() == false) { m_app.Dispatcher.Invoke(new SaveDirListingsStatusDelegate(SaveDirListingsStatusCallback), new object[] { strPath, strText, bDone, nProgress }); return; }

            if (nProgress >= 0)
            {
                MBox.Assert(1306.7305, strText == null);
                MBox.Assert(1306.7306, bDone == false);
                m_winProgress.SetProgress(strPath, nProgress);
            }

            if (bDone)
            {
                m_winProgress.SetCompleted(strPath);

                lock (gd.m_saveDirListings)
                {
                    ++gd.m_saveDirListings.FilesWritten;
                }
            }
        }

        internal void SaveDirListingsDoneCallback()
        {
            if (GlobalData.AppExit || (gd.m_saveDirListings == null) || gd.m_saveDirListings.IsAborted)
            {
                return;
            }

            if (m_app.Dispatcher.CheckAccess() == false) { m_app.Dispatcher.Invoke(new Action(SaveDirListingsDoneCallback)); return; }

            if (gd.m_saveDirListings.FilesWritten > 0)
            {
                gd.RestartTreeTimer();
            }

            int nFilesWritten = gd.m_saveDirListings.FilesWritten;

            gd.m_saveDirListings = null;   // has to precede messagebox
            MBox.ShowDialog("Completed. " + nFilesWritten + " file" + (nFilesWritten != 1 ? "s" : "") + " written.", "Save Directory Listings");
        }
    }

    class SaveDirListings : FileParse
    {
        readonly SaveDirListingsStatusDelegate m_statusCallback = null;
        readonly Action m_doneCallback = null;
        Thread m_thread = null;
        bool m_bThreadAbort = false;
        ConcurrentBag<SaveDirListing> m_cbagWorkers = new ConcurrentBag<SaveDirListing>();
        IEnumerable<LVitem_VolumeVM> m_list_LVitem_VolumeVM = null;

        internal int FilesWritten { get; set; }

        internal static bool IsGoodDriveSyntax(string strDrive)
        {
            return ((strDrive.Length > 2) && char.IsLetter(strDrive[0]) && (strDrive.Substring(1, 2) == @":\"));
        }

        class TraverseTree : FileUtil
        {
            protected long LengthRead { get; private set; }
            protected long FilesRead { get; private set; }

            List<string> m_listErrors = new List<string>();
            protected IEnumerable<string> ErrorList { get { return m_listErrors; } }

            protected bool m_bThreadAbort = false;
            protected readonly LVitem_VolumeVM m_volStrings = null;

            protected TraverseTree(LVitem_VolumeVM volStrings)
            {
                m_volStrings = volStrings;
            }

            /// <summary>
            /// Two-pass private implementation. First to get file list; second to write to file.
            /// </summary>
            /// <param name="fs">If omitted then there can be no params and it returns the file list.</param>
            /// <param name="dictHash"></param>
            /// <param name="dictException_FileRead"></param>
            /// <returns>File list if first pass</returns>
            private IEnumerable<string> ImplementationDetails(
                TextWriter fs = null,
                Dictionary<string, string> dictHash = null,
                Dictionary<string, string> dictException_FileRead = null)
            {
                var stackDirs = new Stack<Win32FindFile.DATUM>(64);
                Win32FindFile.DATUM winRoot;

                Win32FindFile.FileData.WinFile(m_volStrings.Path, out winRoot);
                stackDirs.Push(winRoot);

                var listFilePaths = new List<string>();

                MBox.Assert(0, LengthRead == 0);
                MBox.Assert(0, FilesRead == 0);
                LengthRead = 0;
                FilesRead = 0;

                while (stackDirs.Count > 0)
                {
                    if (m_bThreadAbort || GlobalData.AppExit)
                    {
                        return null;
                    }

                    var winDir = stackDirs.Pop();
                    var strFullPath = winDir.strAltFileName;
                    var strError2_Dir = CheckNTFS_chars(ref strFullPath);
                    var listSubDirs = new List<Win32FindFile.DATUM>();
                    var listFiles = new List<Win32FindFile.DATUM>();

                    if (Win32FindFile.GetDirectory(strFullPath, ref listSubDirs, ref listFiles) == false)
                    {
                        if (fs != null)
                        {
                            m_listErrors.Add(FormatString(strDir: strFullPath,
                                strError1: new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error()).Message, strError2: strError2_Dir));
                        }

                        continue;
                    }

                    long nDirLength = 0;
                    var bHasLength = false;

                    foreach (var winFile in listFiles)
                    {
                        var fi = new Win32FindFile.FileData(winFile);
                        var strFile = winFile.strFileName;
                        var strError2_File = CheckNTFS_chars(ref strFile, bFile: true) ?? "";

                        if (fi.IsValid == false)
                        {
                            if (fs != null)
                            {
                                var strErrorDir = winFile.strAltFileName.Substring(0, winFile.strAltFileName.LastIndexOf('\\'));

                                CheckNTFS_chars(ref strErrorDir);
                                m_listErrors.Add(FormatString(strFile: strFile, strDir: strErrorDir, strError2: strError2_File));
                            }

                            continue;
                        }

                        MBox.Assert(1306.7307, fi.Size >= 0);

                        if (fs == null)
                        {
                            if (fi.Size > 0)
                            {
                                listFilePaths.Add(winFile.strAltFileName);
                            }
                        }
                        else
                        {
                            if (fi.Size > 0)
                            {
                                bHasLength = true;
                                LengthRead += fi.Size;
                                nDirLength += fi.Size;
                                ++FilesRead;
                            }

                            string strHash = null;

                            if (dictHash.ContainsKey(winFile.strAltFileName))
                            {
                                strHash = dictHash[winFile.strAltFileName];
                            }

                            string strError1 = null;

                            if (winFile.strAltFileName.Length > 260)
                            {
                                strError1 = "Path Length: " + winFile.strAltFileName.Length.ToString();
                            }

                            if (dictException_FileRead.ContainsKey(strFile))
                            {
                                strError2_File += " " + dictException_FileRead[strFile];
                                strError2_File = strError2_File.TrimStart();
                            }

                            fs.WriteLine(FormatString(strFile: strFile, dtCreated: fi.CreationTime,
                                strAttributes: ((int)fi.Attributes).ToString("X"), dtModified: fi.LastWriteTime,
                                nLength: fi.Size, strError1: strError1, strError2: strError2_File, strHash: strHash));
                        }
                    }

                    if (fs != null)
                    {
                        string strError1 = null;

                        if (strFullPath.Length > 240)
                        {
                            strError1 = "Path Length: " + strFullPath.Length.ToString();
                        }

                        MBox.Assert(1306.7308, bHasLength == (nDirLength > 0));
                        MBox.Assert(1306.7301, nDirLength >= 0);

                        var di = new Win32FindFile.FileData(winDir);

                        if (strFullPath.EndsWith(@":\"))                        // root directory
                        {
                            MBox.Assert(1306.7302, di.IsValid == false);        // yes, yes...
                            MBox.Assert(1306.7303, strFullPath.Length == 3);
                            fs.WriteLine(FormatString(strDir: strFullPath, nLength: nDirLength, strError1: strError1, strError2: strError2_Dir));
                        }
                        else
                        {
                            MBox.Assert(1306.7304, di.IsValid);
                            fs.WriteLine(FormatString(strDir: strFullPath, dtCreated: di.CreationTime, strAttributes: ((int)di.Attributes).ToString("X"), dtModified: di.LastWriteTime, nLength: nDirLength, strError1: strError1, strError2: strError2_Dir));
                        }
                    }

                    foreach (Win32FindFile.DATUM winData in listSubDirs)
                    {
                        stackDirs.Push(winData);
                    }
                }

                return listFilePaths;
            }

            protected IEnumerable<string> GetFileList()
            {
                return ImplementationDetails();
            }

            protected void WriteFileList(TextWriter fs, Dictionary<string, string> dictHash, Dictionary<string, string> dictException_FileRead)
            {
                ImplementationDetails(fs, dictHash, dictException_FileRead);
            }
        }

        class SaveDirListing : TraverseTree
        {
            readonly SaveDirListingsStatusDelegate m_statusCallback = null;
            Thread m_thread = null;

            internal SaveDirListing(LVitem_VolumeVM volStrings, SaveDirListingsStatusDelegate statusCallback)
                : base(volStrings)
            {
                m_statusCallback = statusCallback;
            }

            private void WriteHeader(TextWriter fs)
            {
                fs.WriteLine(mSTRheader01);
                fs.WriteLine(m_volStrings.VolumeName);
                fs.WriteLine(m_volStrings.Path);

                string strModel = m_volStrings.DriveModel;
                string strSerialNo = m_volStrings.DriveSerial;
                int? nSize = null;

                if (string.IsNullOrWhiteSpace(strModel))
                {
                    DriveSerial.Get(m_volStrings.Path, out strModel, out strSerialNo, out nSize);
                }

                if ((string.IsNullOrWhiteSpace(m_volStrings.DriveSerial) == false) &&
                    (string.IsNullOrWhiteSpace(strSerialNo) == false) &&
                    (strSerialNo != m_volStrings.DriveSerial) &&
                    ((MBox.ShowDialog("Overwrite user-entered serial number for " + m_volStrings.Path + " ?", "Save Directory Listings",
                        System.Windows.MessageBoxButton.YesNo) ==
                        System.Windows.MessageBoxResult.No)))
                {
                    strSerialNo = m_volStrings.DriveSerial;
                }
                
                fs.WriteLine(mSTRdrive01);

                DriveInfo driveInfo = new DriveInfo(m_volStrings.Path[0] + @":\");
                var sb = new StringBuilder();
                var WriteLine = new Action<Object>((o) =>
                {
                    string s = (o != null) ? o.ToString() : null;

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
                if (strModel != null) WriteLine(strModel);
                if (strSerialNo != null) WriteLine(strSerialNo);
                if (nSize != null) WriteLine(nSize);

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
                var timeSpan = new TimeSpan(0, 0, 0, 1);
                double nProgressDenominator = listFilePaths.Count();        // double preserves mantissa
                var dictHash = new Dictionary<string, string>();
                var dictException_FileRead = new Dictionary<string, string>();

                System.Threading.Timer timer = new System.Threading.Timer(new TimerCallback((Object state) =>
                {
                    m_statusCallback(m_volStrings.Path, nProgress: nProgressNumerator / nProgressDenominator);
                }), null, timeSpan, timeSpan);

                Parallel.ForEach(listFilePaths, strFile =>
                {
                    Interlocked.Increment(ref nProgressNumerator);

                    var fileHandle = DriveSerial.CreateFile(@"\\?\" + strFile, FileAccess.Read, FileShare.ReadWrite,
                        IntPtr.Zero, 3, 0, IntPtr.Zero);

                    if (fileHandle.IsInvalid)
                    {
                        dictException_FileRead[strFile] = new System.ComponentModel.Win32Exception(
                            Marshal.GetLastWin32Error()).Message;
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
            }

            void Go()
            {
                if (IsGoodDriveSyntax(m_volStrings.Path) == false)
                {
                    MBox.ShowDialog("Bad drive syntax.", "Save Directory Listing");
                }

                if (Directory.Exists(m_volStrings.Path) == false)
                {
                    m_statusCallback(m_volStrings.Path, mSTRnotSaved);
                    MBox.ShowDialog("Source Path does not exist.", "Save Directory Listing");
                    return;
                }

                if (string.IsNullOrWhiteSpace(m_volStrings.SaveAs))
                {
                    m_statusCallback(m_volStrings.Path, mSTRnotSaved);
                    MBox.ShowDialog("Must specify save filename.", "Save Directory Listing");
                    return;
                }

                string strPathOrig = Directory.GetCurrentDirectory();

                try
                {
                    using (TextWriter fs = File.CreateText(m_volStrings.SaveAs))
                    {
                        Dictionary<string, string> dictHash = null;
                        Dictionary<string, string> dictException_FileRead = null;

                        WriteHeader(fs);
                        fs.WriteLine();
                        fs.WriteLine(FormatString(nHeader: 0));
                        fs.WriteLine(FormatString(nHeader: 1));
                        fs.WriteLine(mSTRstart01 + " " + DateTime.Now.ToString());
                        Hash(GetFileList(), out dictHash, out dictException_FileRead);
                        WriteFileList(fs, dictHash, dictException_FileRead);
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
                    m_statusCallback(m_volStrings.Path, strText: mSTRsaved, bDone: true);
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

        internal SaveDirListings(IEnumerable<LVitem_VolumeVM> list_LVitem_VolumeVM,
            SaveDirListingsStatusDelegate statusCallback,
            Action doneCallback)
        {
            m_list_LVitem_VolumeVM = list_LVitem_VolumeVM;
            m_statusCallback = statusCallback;
            m_doneCallback = doneCallback;
        }

        internal static bool WontSave(LVitem_VolumeVM volStrings)
        {
            return ((mSTRusingFile + mSTRsaved + mSTRcantSave).Contains(volStrings.Status));
        }

        void Go()
        {
            Utilities.WriteLine();
            Utilities.WriteLine("Saving directory listings.");

            DateTime dtStart = DateTime.Now;

            foreach (LVitem_VolumeVM volStrings in m_list_LVitem_VolumeVM)
            {
                if (WontSave(volStrings))
                {
                    continue;
                }

                m_statusCallback(volStrings.Path, "Saving...");
                m_cbagWorkers.Add(new SaveDirListing(volStrings, m_statusCallback).DoThreadFactory());
            }

            foreach (SaveDirListing worker in m_cbagWorkers)
            {
                worker.Join();
            }

            Utilities.WriteLine(string.Format("Finished saving directory listings in {0} seconds.", ((int)(DateTime.Now - dtStart).TotalMilliseconds / 100) / 10.0));

            if (m_bThreadAbort || GlobalData.AppExit)
            {
                return;
            }

            m_doneCallback();
        }

        internal void EndThread()
        {
            foreach (SaveDirListing worker in m_cbagWorkers)
            {
                worker.Abort();
            }

            m_cbagWorkers = new ConcurrentBag<SaveDirListing>();
            m_bThreadAbort = true;
            m_thread = null;
        }

        internal void DoThreadFactory()
        {
            m_thread = new Thread(new ThreadStart(Go));
            m_thread.IsBackground = true;
            m_thread.Start();
        }

        internal bool IsAborted { get { return m_bThreadAbort; } }
    }

    partial class GlobalData
    {
        internal SaveDirListings m_saveDirListings = null;

        internal void ClearMem_SaveDirListings()
        {
            MBox.Assert(1306.73045, m_saveDirListings == null);

            m_saveDirListings = null;
        }

        internal bool DoSaveDirListings(IEnumerable<LVitem_VolumeVM> lvItems, SaveDirListingsStatusDelegate statusCallback, Action doneCallback)
        {
            if (m_saveDirListings != null)
            {
                MBox.ShowDialog("Already in progress.", "Save Directory Listings");
                return false;
            }

            UList<LVitem_VolumeVM> list_LVitem_VolumeVM = new UList<LVitem_VolumeVM>();

            foreach (LVitem_VolumeVM lvItem in lvItems)
            {
                list_LVitem_VolumeVM.Add(lvItem);
            }

            (m_saveDirListings = new SaveDirListings(list_LVitem_VolumeVM, statusCallback, doneCallback)).DoThreadFactory();
            return true;
        }
    }
}
