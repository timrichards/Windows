using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using System.Management;
using System.Security.AccessControl;
using System.Text;

namespace SearchDirLists
{
    delegate void SaveDirListingsStatusDelegate(string strPath, string strText = null, bool bDone = false, long nFilesTotal = 0, long nLengthTotal = 0, double nFilesDiff = 0);

#if (WPF == false)
    partial class Form1
    {
        void SaveDirListingsStatusCallback(string strPath, string strText = null, bool bDone = false, long nFilesTotal = 0, long nLengthTotal = 0, double nFilesDiff = 0)
        {
            if (GlobalData.AppExit || (gd.m_saveDirListings == null) || gd.m_saveDirListings.IsAborted)
            {
                return;
            }

            if (InvokeRequired) { Invoke(new SaveDirListingsStatusDelegate(SaveDirListingsStatusCallback), new object[] { strPath, strText, bDone, nFilesTotal, nLengthTotal, nFilesDiff }); return; }

            if (nLengthTotal > 0)
            {
                Utilities.Assert(1306.7305, strText == null);
                Utilities.Assert(1306.7306, bDone == false);

                strText = nFilesTotal.ToString("###,###,###,###") + " (" + Utilities.FormatSize(nLengthTotal) + ") SD " + nFilesDiff.ToString("#0.0");
            }

            if (bDone)
            {
                lock (gd.m_saveDirListings)
                {
                    ++gd.m_saveDirListings.FilesWritten;
                }
            }

            lock (form_lvVolumesMain)
            {
                ListViewItem lvItem = form_lvVolumesMain.Items[strPath];

                if (Utilities.Assert(0, lvItem != null))
                {
                    lvItem.SubItems[3].Text = strText;

                    if (bDone && (strText == Utilities.mSTRsaved))
                    {
                        lvItem.Name = null;    // indexing by path, only for unsaved volumes
                    }

                    form_lvVolumesMain.Invalidate();
                }
            }
        }

        void SaveDirListingsDoneCallback()
        {
            if (GlobalData.AppExit || (gd.m_saveDirListings == null) || gd.m_saveDirListings.IsAborted)
            {
                return;
            }

            if (InvokeRequired) { Invoke(new Action(SaveDirListingsDoneCallback)); return; }

            if (gd.m_saveDirListings.FilesWritten > 0)
            {
                gd.RestartTreeTimer();
                form_tabControlMain.SelectedTab = form_tabPageBrowse;
            }

            int nFilesWritten = gd.m_saveDirListings.FilesWritten;

            gd.m_saveDirListings = null;   // has to precede messagebox
            Utilities.MBox("Completed. " + nFilesWritten + " files written.", "Save Directory Listings");
        }
    }
#endif
    partial class VolumesTabVM
    {
        void SaveDirListingsStatusCallback(string strPath, string strText = null, bool bDone = false, long nFilesTotal = 0, long nLengthTotal = 0, double nFilesDiff = 0)
        {
#if (WPF == false)
        }
        void SaveDirListingsDoneCallback()
        { }
    }
#else
            if (GlobalData.AppExit || (gd.m_saveDirListings == null) || gd.m_saveDirListings.IsAborted)
            {
                return;
            }

            if (m_app.Dispatcher.CheckAccess() == false) { m_app.Dispatcher.Invoke(new SaveDirListingsStatusDelegate(SaveDirListingsStatusCallback), new object[] { strPath, strText, bDone, nFilesTotal, nLengthTotal, nFilesDiff }); return; }

            if (nLengthTotal > 0)
            {
                Utilities.Assert(1306.7305, strText == null);
                Utilities.Assert(1306.7306, bDone == false);

                strText = nFilesTotal.ToString("###,###,###,###") + " (" + Utilities.FormatSize(nLengthTotal) + ") SD " + nFilesDiff.ToString("#0.0");
            }

            if (bDone)
            {
                lock (gd.m_saveDirListings)
                {
                    ++gd.m_saveDirListings.FilesWritten;
                }
            }

            VolumeLVitemVM lvItem = LV.ItemsCast.Where(i => (i.datum != null) && (i.datum.ListView != null)).FirstOrDefault();

            if (lvItem != null)     // e.g. user may have clicked "Remove Volume" while saving
                lock (LV)
            {
                lvItem.Status = strText;

                if (bDone && (strText == Utilities.mSTRsaved))
                {
                    lvItem.SaveAsExists = true;    // copied code. non-WPF comment: "indexing by path, only for unsaved volumes"
                }
            }
        }

        void SaveDirListingsDoneCallback()
        {
            if (GlobalData.AppExit || (gd.m_saveDirListings == null) || gd.m_saveDirListings.IsAborted)
            {
                return;
            }

            if (m_app.Dispatcher.CheckAccess() == false) { m_app.Dispatcher.Invoke(new Action(SaveDirListingsDoneCallback)); return; }

            if (gd.m_saveDirListings.FilesWritten > 0)
            {
                gd.RestartTreeTimer();
                m_app.xaml_tabControlMain.SelectedItem = m_app.xaml_tabItemBrowse;
            }

            int nFilesWritten = gd.m_saveDirListings.FilesWritten;

            gd.m_saveDirListings = null;   // has to precede messagebox
            Utilities.MBox("Completed. " + nFilesWritten + " files written.", "Save Directory Listings");
        }
    }
#endif
    class SaveDirListings : Utilities
    {
        readonly SaveDirListingsStatusDelegate m_statusCallback = null;
        readonly Action m_doneCallback = null;
        Thread m_thread = null;
        bool m_bThreadAbort = false;
        ConcurrentBag<SaveDirListing> m_cbagWorkers = new ConcurrentBag<SaveDirListing>();
        readonly UList<LVvolStrings> m_list_lvVolStrings = new UList<LVvolStrings>();

        internal int FilesWritten { get; set; }
        internal static bool DoFakeChecksum = true;

        class SaveDirListing
        {
            readonly SaveDirListingsStatusDelegate m_statusCallback = null;
            Thread m_thread = null;
            bool m_bThreadAbort = false;
            readonly LVvolStrings m_volStrings = null;
            long m_nLengthTotal = 0;
            long m_nFilesTotal = 0;
            long m_nFilesDiff = 0;
            readonly List<double> m_listFileDiffs = new List<double>();
            readonly List<string> m_list_Errors = new List<string>();

            private double StdDevSign(List<double> values)
            {
                double nAvg = values.Average();
                double nStdDev = Math.Sqrt(
                    values.Sum(d => Math.Pow(d - nAvg, 2)) /
                    (values.Count() - 1));

                return nStdDev *= Math.Sign(values[values.Count - 1] - nAvg);
            }

            internal SaveDirListing(LVvolStrings volStrings,
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
                public string strChecksum { private get; set; }
                public string strOut { get { return FormatString(strFile: strFile, dtCreated: fi.CreationTime, strAttributes: ((int)fi.Attributes).ToString("X"), dtModified: fi.LastWriteTime, nLength: fi.Size, strError1: strError1, strError2: strError2_File, strChecksum: strChecksum); } }

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
 
            void TraverseTree(TextWriter fs, string root)
            {
                Stack<Win32FindFile.DATUM> stackDirs = new Stack<Win32FindFile.DATUM>(64);
                List<Win32FindFile.DATUM> listSubDirs = new List<Win32FindFile.DATUM>();
                List<Win32FindFile.DATUM> listFiles = new List<Win32FindFile.DATUM>();
                Win32FindFile.DATUM winRoot;

                Win32FindFile.FileData.WinFile(root, out winRoot);
                stackDirs.Push(winRoot);

                Queue<Task<ResultItem>> queueTasks = new Queue<Task<ResultItem>>();                 // TODO/WIP

                while (stackDirs.Count > 0)
                {
                    if (m_bThreadAbort || GlobalData.AppExit)
                    {
                        return;
                    }

                    Win32FindFile.DATUM winDir = stackDirs.Pop();
                    string strFullPath = winDir.strAltFileName;
                    string strError2_Dir = CheckNTFS_chars(ref strFullPath);

                    if (Win32FindFile.GetDirectory(strFullPath, ref listSubDirs, ref listFiles) == false)
                    {
                        m_list_Errors.Add(FormatString(strDir: strFullPath,
                            strError1: new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error()).Message, strError2: strError2_Dir));
                        continue;
                    }

                    long nDirLength = 0;
                    bool bHasLength = false;
                    Dictionary<string, string> dictChecksum = new Dictionary<string, string>();
                    Dictionary<string, string> dictException_FileRead = new Dictionary<string, string>();

                    if (DoFakeChecksum)
                        Parallel.ForEach(listFiles, winData =>
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
                            dictException_FileRead[winData.strFileName] = new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error()).Message;
                            return;     // return from lambda
                        }

                        using (var fsA = new FileStream(fileHandle, FileAccess.Read))
                        {
                            const int kBufferSize = 4096;   // Checksum is fake because just reading in the first 4K of the file
                            byte[] buffer = new byte[kBufferSize];

                            fsA.Read(buffer, 0, kBufferSize);

                            using (var md5 = System.Security.Cryptography.MD5.Create())
                            {
                                string strChecksum = DRDigit.Fast.ToHexString(md5.ComputeHash(buffer));

                                lock (dictChecksum)
                                {
                                    dictChecksum[winData.strFileName] = strChecksum;
                                }
                            }
                        }
                    });

                    foreach (Win32FindFile.DATUM winData in listFiles)
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
                        ++m_nFilesTotal;
                        ++m_nFilesDiff;

                        string strError1 = null;

                        if (winData.strAltFileName.Length > 260)
                        {
                            strError1 = "Path Length: " + winData.strAltFileName.Length.ToString();
                        }

                        Utilities.Assert(1306.7307, fi.Size >= 0);

                        if (fi.Size > 0)
                        {
                            bHasLength = true;
                        }

                        string strChecksum = null;

                        if (dictChecksum.ContainsKey(strFile))
                        {
                            strChecksum = dictChecksum[strFile];
                        }

                        if (dictException_FileRead.ContainsKey(strFile))
                        {
                            strError2_File += " " + dictException_FileRead[strFile];
                            strError2_File = strError2_File.TrimStart();
                        }

                        string strOut = FormatString(strFile: strFile, dtCreated: fi.CreationTime, strAttributes: ((int)fi.Attributes).ToString("X"), dtModified: fi.LastWriteTime, nLength: fi.Size, strError1: strError1, strError2: strError2_File, strChecksum: strChecksum);

                        fs.WriteLine(strOut);
                    }

                    {
                        string strError1 = null;

                        if (strFullPath.Length > 240)
                        {
                            strError1 = "Path Length: " + strFullPath.Length.ToString();
                        }

                        Utilities.Assert(1306.7308, bHasLength == (nDirLength > 0));
                        Utilities.Assert(1306.7301, nDirLength >= 0);

                        Win32FindFile.FileData di = new Win32FindFile.FileData(winDir);

                        if (strFullPath.EndsWith(@":\"))                            // root directory
                        {
                            Utilities.Assert(1306.7302, di.IsValid == false);       // yes, yes...
                            Utilities.Assert(1306.7303, strFullPath.Length == 3);
                            fs.WriteLine(FormatString(strDir: strFullPath, nLength: nDirLength, strError1: strError1, strError2: strError2_Dir));
                        }
                        else
                        {
                            Utilities.Assert(1306.7304, di.IsValid);
                            fs.WriteLine(FormatString(strDir: strFullPath, dtCreated: di.CreationTime, strAttributes: ((int)di.Attributes).ToString("X"), dtModified: di.LastWriteTime, nLength: nDirLength, strError1: strError1, strError2: strError2_Dir));
                        }
                    }

                    foreach (Win32FindFile.DATUM winData in listSubDirs)
                    {
                        stackDirs.Push(winData);
                    }
                }
            }

            void Go()
            {
                string strVolumeName = m_volStrings.VolumeName;
                string strPath = m_volStrings.StrPath;
                string strSaveAs = m_volStrings.SaveAs;

                if (FormatPath(ref strPath, ref strSaveAs) == false)
                {
                    // FormatPath() has its own message box
                    m_statusCallback(m_volStrings.StrPath, mSTRnotSaved);
                    return;
                }

                if (Directory.Exists(strPath) == false)
                {
                    m_statusCallback(m_volStrings.StrPath, mSTRnotSaved);
                    MBox("Source Path does not exist.", "Save Directory Listing");
                    return;
                }

                if (StrValid(strSaveAs) == false)
                {
                    m_statusCallback(m_volStrings.StrPath, mSTRnotSaved);
                    MBox("Must specify save filename.", "Save Directory Listing");
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
                            m_listFileDiffs.Add(m_nFilesDiff);

                            if (m_listFileDiffs.Count >= 2)
                            {
                                double nFilesDiff = m_nFilesDiff / StdDevSign(m_listFileDiffs);

                                m_nFilesDiff = 0;
                                m_statusCallback(m_volStrings.StrPath, nFilesTotal: m_nFilesTotal, nLengthTotal: m_nLengthTotal, nFilesDiff: nFilesDiff);
                            }
                        }), null, timeSpan, timeSpan);

                        TraverseTree(fs, strPath);
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
                    m_statusCallback(m_volStrings.StrPath, strText: mSTRsaved, bDone: true);
                }
#if DEBUG == false
                catch (Exception e)
                {
                    m_statusCallback(m_volStrings.StrPath, strText: mSTRnotSaved, bDone: true);
                    MBox(strSaveAs.PadRight(100) + "\nException: " + e.Message, "Save Directory Listing");
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

        internal SaveDirListings(UList<LVvolStrings> list_lvVolStrings,
            SaveDirListingsStatusDelegate statusCallback,
            Action doneCallback)
        {
            m_list_lvVolStrings = list_lvVolStrings;
            m_statusCallback = statusCallback;
            m_doneCallback = doneCallback;
        }

        void Go()
        {
            Utilities.WriteLine();
            Utilities.WriteLine("Saving directory listings.");

            DateTime dtStart = DateTime.Now;

            foreach (LVvolStrings volStrings in m_list_lvVolStrings)
            {
                if ((mSTRusingFile + mSTRsaved + mSTRcantSave).Contains(volStrings.Status))
                {
                    continue;
                }

                m_statusCallback(volStrings.StrPath, "Saving...");
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
            Utilities.Assert(1306.73045, m_saveDirListings == null);

            m_saveDirListings = null;
        }

        internal bool DoSaveDirListings(IEnumerable<VolumeLVitemVM> lvItems, SaveDirListingsStatusDelegate statusCallback, Action doneCallback)
#if (WPF == false)
        { return false; }
        internal bool DoSaveDirListings(ListView.ListViewItemCollection lvItems, SaveDirListingsStatusDelegate statusCallback, Action doneCallback)
#endif
        {
            if (m_saveDirListings != null)
            {
                Utilities.MBox("Already in progress.", "Save Directory Listings");
                return false;
            }

            UList<LVvolStrings> list_lvVolStrings = new UList<LVvolStrings>();

#if (WPF)
            foreach (VolumeLVitemVM lvItem in lvItems)
#else
            foreach (SDL_ListViewItem lvItem in lvItems)
#endif
            {
                list_lvVolStrings.Add(new LVvolStrings(lvItem));
            }

            (m_saveDirListings = new SaveDirListings(list_lvVolStrings, statusCallback, doneCallback)).DoThreadFactory();
            return true;
        }
    }
}
