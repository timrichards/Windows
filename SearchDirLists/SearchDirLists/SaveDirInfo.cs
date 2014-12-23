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

namespace SearchDirLists
{
    delegate void SaveDirListingsStatusDelegate(String strPath, String strText = null, bool bDone = false, long nFilesTotal = 0, long nLengthTotal = 0, double nFilesDiff = 0);

#if (WPF == false)
    partial class Form1
    {
        void SaveDirListingsStatusCallback(String strPath, String strText = null, bool bDone = false, long nFilesTotal = 0, long nLengthTotal = 0, double nFilesDiff = 0)
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
        void SaveDirListingsStatusCallback(String strPath, String strText = null, bool bDone = false, long nFilesTotal = 0, long nLengthTotal = 0, double nFilesDiff = 0)
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

            if (m_app.Dispatcher.CheckAccess() == false) { m_app.Dispatcher.Invoke(new SaveDirListingsStatusDelegate(SaveDirListingsStatusCallback), new object[] { nIndex, strText, bDone, nFilesTotal, nLengthTotal, nFilesDiff }); return; }

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
        internal static bool ComputeChecksum = false;

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
            readonly List<String> m_list_Errors = new List<String>();

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

            private void WriteHeader(TextWriter fs, String strVolumeName, String strPath)
            {
                fs.WriteLine(mSTRheader01);
                // assume SaveFields() by caller because SaveFields() has already prompted user
                fs.WriteLine(strVolumeName);
                fs.WriteLine(strPath);

                // device info

                String strModel = null;
                String strSerialNo = null;
                object nSize = null;

                var letter = strPath.Substring(0, 2);

                new ManagementObjectSearcher(
                        new ManagementScope("\\\\.\\ROOT\\cimv2"),
                        new ObjectQuery("SELECT * FROM Win32_LogicalDisk WHERE DeviceID='" + letter + "'")
                ).Get().Cast<ManagementObject>().FirstOnlyAssert(new Action<ManagementObject>((logicalDisk) =>
                {
                    logicalDisk.GetRelated("Win32_DiskPartition").Cast<ManagementObject>().FirstOnlyAssert(new Action<ManagementObject>((partition) =>
                    {
                        partition.GetRelated("Win32_DiskDrive").Cast<ManagementObject>().FirstOnlyAssert(new Action<ManagementObject>((diskDrive) =>
                        {
                            nSize = diskDrive["Size"];
                            strModel = diskDrive["Model"].ToString();

                            if (strModel == null)
                            {
                                strModel = diskDrive["Caption"].ToString();
                            }

                            strSerialNo = diskDrive["SerialNumber"].ToString();

                            if (strSerialNo == null)
                            {
                                diskDrive.GetRelated("Win32_PhysicalMedia").Cast<ManagementObject>().FirstOnlyAssert(new Action<ManagementObject>((diskMedia) =>
                                {
                                    strSerialNo = diskMedia["SerialNumber"].ToString();
                                }));
                            }
                        }));
                    }));
                }));

                fs.WriteLine(mSTRdrive01);
                DriveInfo driveInfo = new DriveInfo(strPath.Substring(0, strPath.IndexOf(Path.DirectorySeparatorChar)));

                var WriteLine = new Action<Object>((o) =>
                {
                    String s = (o != null) ? o.ToString() : null;

                    // Hack. Prevent blank line continue in Utilities.Convert()
                    fs.WriteLine(((s == null) || (s.Length <= 0)) ? " " : s.Trim());
                });

                WriteLine(driveInfo.AvailableFreeSpace); // These could all be named better, so mAstrDIlabels is different.
                WriteLine(driveInfo.DriveFormat);        // Misnomer. Should be VolumeFormat.
                WriteLine(driveInfo.DriveType);
                WriteLine(driveInfo.Name);
                WriteLine(driveInfo.RootDirectory);
                WriteLine(driveInfo.TotalFreeSpace);
                WriteLine(driveInfo.TotalSize);
                WriteLine(driveInfo.VolumeLabel);
                WriteLine(strModel);
                WriteLine(strSerialNo);
                WriteLine(nSize);
            }

            class ResultItem
            {
                public bool Ready { get; private set; }
                public String strChecksum { private get; set; }
                public String strOut { get { return FormatString(strFile: strFile, dtCreated: fi.CreationTime, strAttributes: ((int)fi.Attributes).ToString("X"), dtModified: fi.LastWriteTime, nLength: fi.Size, strError1: strError1, strError2: strError2_File, strChecksum: strChecksum); } }

                String strFile;
                Win32FindFile.FileData fi;
                String strError1;
                String strError2_File;

                public ResultItem(String strFile_in, Win32FindFile.FileData fi_in, String strError1_in, String strError2_in)
                {
                    strFile = strFile_in;
                    fi = fi_in;
                    strError1 = strError1_in;
                    strError2_File = strError2_in;
                }
            }

            void TraverseTree(TextWriter fs, String root)
            {
                Stack<Win32FindFile.DATUM> stackDirs = new Stack<Win32FindFile.DATUM>(64);
                List<Win32FindFile.DATUM> listSubDirs = new List<Win32FindFile.DATUM>();
                List<Win32FindFile.DATUM> listFiles = new List<Win32FindFile.DATUM>();
                Win32FindFile.DATUM winRoot;

                Win32FindFile.FileData.WinFile(root, out winRoot);
                stackDirs.Push(winRoot);

                Queue<Task<ResultItem>> queueTasks = new Queue<Task<ResultItem>>();

                while (stackDirs.Count > 0)
                {
                    if (m_bThreadAbort || GlobalData.AppExit)
                    {
                        return;
                    }

                    Win32FindFile.DATUM winDir = stackDirs.Pop();
                    long nDirLength = 0;
                    bool bHasLength = false;
                    String strFullPath = winDir.strAltFileName;
                    String strError2_Dir = CheckNTFS_chars(ref strFullPath);

                    if (Win32FindFile.GetDirectory(strFullPath, ref listSubDirs, ref listFiles) == false)
                    {
                        m_list_Errors.Add(FormatString(strDir: strFullPath,
                            strError1: new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error()).Message, strError2: strError2_Dir));
                        continue;
                    }

                    Dictionary<String, String> dictChecksum = new Dictionary<string,string>();

                    if (ComputeChecksum) Parallel.ForEach(listFiles, t =>
                    {
                        using (var md5 = System.Security.Cryptography.MD5.Create())
                        {
                            try
                            {
                                using (Stream stream = new BufferedStream(File.OpenRead(strFullPath + Path.DirectorySeparatorChar + t.strFileName), 1024 * 1024))
                                {
                                    const int nMAX = 1024 * 1024 * 16;
                                    byte[] checksum = null;

                                    if (stream.Length <= nMAX)
                                    {
                                        byte[] data = new byte[stream.Length];
                                        int offset = 0;
                                        int remaining = data.Length;

                                        while (remaining > 0)
                                        {
                                            int read = stream.Read(data, offset, remaining);

                                            if (read <= 0)
                                            {
                                                throw new EndOfStreamException(String.Format("End of stream reached with {0} bytes left to read", remaining));
                                            }

                                            remaining -= read;
                                            offset += read;
                                        }

                                        checksum = md5.ComputeHash(data);
                                    }
                                    else
                                    {
                                        checksum = md5.ComputeHash(stream);
                                    }

                                    String strChecksum = DRDigit.Fast.ToHexString(checksum);

                                    lock (dictChecksum)
                                    {
                                        dictChecksum[t.strFileName] = strChecksum;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Utilities.WriteLine(ex.Message);
                                FlashWindow.Go(Once: true);
                            }
                        }
                    });

                    foreach (Win32FindFile.DATUM winData in listFiles)
                    {
                        Win32FindFile.FileData fi = new Win32FindFile.FileData(winData);
                        String strFile = winData.strFileName;
                        String strError2_File = CheckNTFS_chars(ref strFile, bFile: true);

                        if (fi.IsValid == false)
                        {
                            String strErrorFile = strFile;
                            String strErrorDir = winData.strAltFileName.Substring(0, winData.strAltFileName.LastIndexOf(Path.DirectorySeparatorChar));

                            CheckNTFS_chars(ref strErrorDir);
                            m_list_Errors.Add(FormatString(strFile: strErrorFile, strDir: strErrorDir, strError2: strError2_File));
                            continue;
                        }

                        m_nLengthTotal += fi.Size;
                        nDirLength += fi.Size;
                        ++m_nFilesTotal;
                        ++m_nFilesDiff;

                        String strError1 = null;

                        if (winData.strAltFileName.Length > 260)
                        {
                            strError1 = "Path Length: " + winData.strAltFileName.Length.ToString();
                        }

                        Utilities.Assert(1306.7307, fi.Size >= 0);

                        if (fi.Size > 0)
                        {
                            bHasLength = true;
                        }

                        String strChecksum = null;

                        if (dictChecksum.ContainsKey(strFile))
                        {
                            strChecksum = dictChecksum[strFile];
                        }

                        String strOut = FormatString(strFile: strFile, dtCreated: fi.CreationTime, strAttributes: ((int)fi.Attributes).ToString("X"), dtModified: fi.LastWriteTime, nLength: fi.Size, strError1: strError1, strError2: strError2_File, strChecksum: strChecksum);

                        fs.WriteLine(strOut);
                    }

                    {
                        String strError1 = null;

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
                String strVolumeName = m_volStrings.VolumeName;
                String strPath = m_volStrings.StrPath;
                String strSaveAs = m_volStrings.SaveAs;

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

                String strPathOrig = Directory.GetCurrentDirectory();

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

                        foreach (String strError in m_list_Errors)
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

            Utilities.WriteLine(String.Format("Finished saving directory listings in {0} seconds.", ((int)(DateTime.Now - dtStart).TotalMilliseconds / 100) / 10.0));

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
