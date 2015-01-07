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

    class SaveListingsCallback
    {
        GlobalData gd = null;
        WinSaveInProgress m_winProgress = null;
        System.Windows.Window m_app = null;

        internal SaveListingsCallback(UList<LVitem_VolumeVM> list_lvVolStrings)
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
                (m_winProgress = new WinSaveInProgress()).Show(m_app);
                m_winProgress.InitProgress(listNicknames, listPaths);

                if ((gd.m_saveDirListings != null) && (gd.m_saveDirListings.IsAborted == false))
                {
                    MBox.Assert(0, false);
                    gd.m_saveDirListings.EndThread();
                }

                (gd.m_saveDirListings = new SaveDirListings(list_lvVolStrings,
                  SaveDirListingsStatusCallback, SaveDirListingsDoneCallback)).DoThreadFactory();
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
            MBox.ShowDialog("Completed. " + nFilesWritten + " files written.", "Save Directory Listings");
        }
    }

    class SaveDirListings : FileParse
    {
        readonly SaveDirListingsStatusDelegate m_statusCallback = null;
        readonly Action m_doneCallback = null;
        Thread m_thread = null;
        bool m_bThreadAbort = false;
        ConcurrentBag<SaveDirListing> m_cbagWorkers = new ConcurrentBag<SaveDirListing>();
        readonly UList<LVitem_VolumeVM> m_list_LVitem_VolumeVM = new UList<LVitem_VolumeVM>();

        internal int FilesWritten { get; set; }
        internal static bool DoHash = true;

        class SaveDirListing : FileUtil
        {
            readonly SaveDirListingsStatusDelegate m_statusCallback = null;
            Thread m_thread = null;
            bool m_bThreadAbort = false;
            readonly LVitem_VolumeVM m_volStrings = null;
            long m_nLengthTotal = 0;
            long m_nFilesTotal_1 = 0;
            long m_nFilesTotal_2 = 0;
            long m_nFilesDiff = 0;
            readonly List<string> m_list_Errors = new List<string>();

            internal SaveDirListing(LVitem_VolumeVM volStrings,
                SaveDirListingsStatusDelegate statusCallback)
            {
                m_volStrings = volStrings;
                m_statusCallback = statusCallback;
            }

            private void WriteHeader(TextWriter fs, string strVolumeName, string strPath)
            {
                fs.WriteLine(mSTRheader01);
                // assume SaveFields() by caller because SaveFields() has already prompted user
                fs.WriteLine(strVolumeName);
                fs.WriteLine(strPath);

                string strModel = null;
                string strSerialNo = null;
                int? nSize = null;

                DriveSerial.Get(strPath, out strModel, out strSerialNo, out nSize);

            //    if ((strModel.ToPrintString() == null) && 

                fs.WriteLine(mSTRdrive01);
                DriveInfo driveInfo = new DriveInfo(strPath.Substring(0, strPath.IndexOf('\\')));

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

            class ResultItem
            {
                public bool Ready { get; private set; }
                public string strHash { private get; set; }
                public string strOut { get { return FormatString(strFile: strFile, dtCreated: fi.CreationTime, strAttributes: ((int)fi.Attributes).ToString("X"), dtModified: fi.LastWriteTime, nLength: fi.Size, strError1: strError1, strError2: strError2_File, strHash: strHash); } }

                string strFile;
                Win32FindFile.FileData fi;
                string strError1;
                string strError2_File;

                public ResultItem(string strFile_in, Win32FindFile.FileData fi_in, string strError1_in, string strError2_in)
                {
                    strFile = strFile_in;
                    fi = fi_in;
                    strError1 = strError1_in;
                    strError2_File = strError2_in;
                }
            }

            struct DirData
            {
                internal Win32FindFile.DATUM winDir;
                internal List<Win32FindFile.DATUM> listSubDirs;
                internal List<Win32FindFile.DATUM> listFiles;
                internal string strError2_Dir;

                internal DirData(Win32FindFile.DATUM winDir_in)
                {
                    winDir = winDir_in;
                    listSubDirs = new List<Win32FindFile.DATUM>();
                    listFiles = new List<Win32FindFile.DATUM>();
                    strError2_Dir = null;
                }
            }

            List<DirData> TraverseTree(TextWriter fs, string root)
            {
                var stackDirs = new Stack<Win32FindFile.DATUM>(64);
                var listAll = new List<DirData>();
                Win32FindFile.DATUM winRoot;

                Win32FindFile.FileData.WinFile(root, out winRoot);
                stackDirs.Push(winRoot);

                while (stackDirs.Count > 0)
                {
                    if (m_bThreadAbort || GlobalData.AppExit)
                    {
                        return null;
                    }

                    var dirData = new DirData(stackDirs.Pop());

                    listAll.Add(dirData);

                    if (Win32FindFile.GetDirectory(dirData.winDir.strAltFileName, ref dirData.listSubDirs, ref dirData.listFiles) == false)
                    {
                        m_list_Errors.Add(FormatString(strDir: dirData.winDir.strAltFileName,
                            strError1: new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error()).Message, strError2: dirData.strError2_Dir));
                        continue;
                    }

                    foreach (Win32FindFile.DATUM winData in dirData.listFiles)
                    {
                        Win32FindFile.FileData fi = new Win32FindFile.FileData(winData);

                        if (fi.IsValid == false)
                        {
                            continue;
                        }

                        ++m_nFilesTotal_1;
                    }

                    foreach (Win32FindFile.DATUM winData in dirData.listSubDirs)
                    {
                        stackDirs.Push(winData);
                    }
                }

                return listAll;
            }

            void TraverseTree(TextWriter fs, List<DirData> listAll)
            {
                if (listAll == null)
                {
                    return;
                }

                Queue<Task<ResultItem>> queueTasks = new Queue<Task<ResultItem>>();                 // TODO/WIP

                foreach (var listItem in listAll)
                {
                    if (m_bThreadAbort || GlobalData.AppExit)
                    {
                        return;
                    }

                    long nDirLength = 0;
                    bool bHasLength = false;
                    Dictionary<string, string> dictHash = new Dictionary<string, string>();
                    Dictionary<string, string> dictInvalid_FileRead = new Dictionary<string, string>();

                    if (DoHash)
                        Parallel.ForEach(listItem.listFiles, winData =>
                    {
                        Win32FindFile.FileData fi = new Win32FindFile.FileData(winData);

                        if ((fi.IsValid == false) || (fi.Size <= 0))
                        {
                            return;     // return from lambda
                        }

                        Microsoft.Win32.SafeHandles.SafeFileHandle fileHandle =
                            DriveSerial.CreateFile(@"\\?\" + winData.strAltFileName, FileAccess.Read, FileShare.ReadWrite, IntPtr.Zero, 3, 0, IntPtr.Zero);

                        if (fileHandle.IsInvalid)
                        {
                            dictInvalid_FileRead[winData.strFileName] = new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error()).Message;
                            return;     // return from lambda
                        }

                        using (var fsA = new FileStream(fileHandle, FileAccess.Read))
                        {
                            const int kBufferSize = 4096;   // Hash in the first 4K of the file for speed
                            byte[] buffer = new byte[kBufferSize];

                            fsA.Read(buffer, 0, kBufferSize);

                            using (var md5 = System.Security.Cryptography.MD5.Create())
                            {
                                string strHash = DRDigit.Fast.ToHexString(md5.ComputeHash(buffer));

                                lock (dictHash)
                                {
                                    dictHash[winData.strFileName] = strHash;
                                }
                            }
                        }
                    });

                    foreach (Win32FindFile.DATUM winData in listItem.listFiles)
                    {
                        Win32FindFile.FileData fi = new Win32FindFile.FileData(winData);
                        string strFile = winData.strFileName;
                        string strError2_File = CheckNTFS_chars(ref strFile, bFile: true) ?? "";

                        if (fi.IsValid == false)
                        {
                            string strErrorFile = strFile;
                            string strErrorDir = winData.strAltFileName.Substring(0, winData.strAltFileName.LastIndexOf('\\'));

                            CheckNTFS_chars(ref strErrorDir);
                            m_list_Errors.Add(FormatString(strFile: strErrorFile, strDir: strErrorDir, strError2: strError2_File));
                            continue;
                        }

                        m_nLengthTotal += fi.Size;
                        nDirLength += fi.Size;
                        ++m_nFilesTotal_2;
                        ++m_nFilesDiff;

                        string strError1 = null;

                        if (winData.strAltFileName.Length > 260)
                        {
                            strError1 = "Path Length: " + winData.strAltFileName.Length.ToString();
                        }

                        MBox.Assert(1306.7307, fi.Size >= 0);

                        if (fi.Size > 0)
                        {
                            bHasLength = true;
                        }

                        string strHash = null;

                        if (dictHash.ContainsKey(strFile))
                        {
                            strHash = dictHash[strFile];
                        }

                        if (dictInvalid_FileRead.ContainsKey(strFile))
                        {
                            strError2_File += " " + dictInvalid_FileRead[strFile];
                            strError2_File = strError2_File.TrimStart();
                        }

                        string strOut = FormatString(strFile: strFile, dtCreated: fi.CreationTime, strAttributes: ((int)fi.Attributes).ToString("X"), dtModified: fi.LastWriteTime, nLength: fi.Size, strError1: strError1, strError2: strError2_File, strHash: strHash);

                        fs.WriteLine(strOut);
                    }

                    {
                        string strError1 = null;

                        if (listItem.winDir.strAltFileName.Length > 240)
                        {
                            strError1 = "Path Length: " + listItem.winDir.strAltFileName.Length.ToString();
                        }

                        MBox.Assert(1306.7308, bHasLength == (nDirLength > 0));
                        MBox.Assert(1306.7301, nDirLength >= 0);

                        Win32FindFile.FileData di = new Win32FindFile.FileData(listItem.winDir);

                        if (listItem.winDir.strAltFileName.EndsWith(@":\"))         // root directory
                        {
                            MBox.Assert(1306.7302, di.IsValid == false);            // yes, yes...
                            MBox.Assert(1306.7303, listItem.winDir.strAltFileName.Length == 3);
                            fs.WriteLine(FormatString(strDir: listItem.winDir.strAltFileName, nLength: nDirLength, strError1: strError1, strError2: listItem.strError2_Dir));
                        }
                        else
                        {
                            MBox.Assert(1306.7304, di.IsValid);
                            fs.WriteLine(FormatString(strDir: listItem.winDir.strAltFileName, dtCreated: di.CreationTime, strAttributes: ((int)di.Attributes).ToString("X"), dtModified: di.LastWriteTime, nLength: nDirLength, strError1: strError1, strError2: listItem.strError2_Dir));
                        }
                    }
                }
            }

            void Go()
            {
                string strVolumeName = m_volStrings.VolumeName;
                string strPath = m_volStrings.Path;
                string strSaveAs = m_volStrings.SaveAs;

                if (Directory.Exists(strPath) == false)
                {
                    m_statusCallback(m_volStrings.Path, mSTRnotSaved);
                    MBox.ShowDialog("Source Path does not exist.", "Save Directory Listing");
                    return;
                }

                if (string.IsNullOrWhiteSpace(strSaveAs))
                {
                    m_statusCallback(m_volStrings.Path, mSTRnotSaved);
                    MBox.ShowDialog("Must specify save filename.", "Save Directory Listing");
                    return;
                }

                string strPathOrig = Directory.GetCurrentDirectory();

                try
                {
                    using (TextWriter fs = File.CreateText(strSaveAs))
                    {
                        WriteHeader(fs, strVolumeName, strPath);
                        fs.WriteLine();
                        fs.WriteLine(FormatString(nHeader: 0));
                        fs.WriteLine(FormatString(nHeader: 1));
                        fs.WriteLine(mSTRstart01 + " " + DateTime.Now.ToString());

                        TimeSpan timeSpan = new TimeSpan(0, 0, 0, 1);

                        System.Threading.Timer timer = new System.Threading.Timer(new TimerCallback((Object state) =>
                        {
                            m_statusCallback(m_volStrings.Path, nProgress: m_nFilesTotal_2 / (double)m_nFilesTotal_1);
                        }), null, timeSpan, timeSpan);

                        TraverseTree(fs, TraverseTree(fs, strPath));
                        timer.Dispose();
                        fs.WriteLine(mSTRend01 + " " + DateTime.Now.ToString());
                        fs.WriteLine();
                        fs.WriteLine(mSTRerrorsLoc01);

                        foreach (string strError in m_list_Errors)
                        {
                            fs.WriteLine(strError);
                        }

                        fs.WriteLine();
                        fs.WriteLine(FormatString(strDir: mSTRtotalLengthLoc01, nLength: m_nLengthTotal));
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

        internal SaveDirListings(UList<LVitem_VolumeVM> list_LVitem_VolumeVM,
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
