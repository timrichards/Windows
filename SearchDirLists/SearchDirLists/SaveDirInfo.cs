using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;

namespace SearchDirLists
{
    delegate void SaveDirListingsStatusDelegate(int nIndex, String strText = null, bool bDone = false, long nFilesTotal = 0, long nLengthTotal = 0, double nFilesDiff = 0);

    class SaveDirListings : Utilities
    {
        readonly SaveDirListingsStatusDelegate m_statusCallback = null;
        readonly Action m_doneCallback = null;
        Thread m_thread = null;
        bool m_bThreadAbort = false;
        ConcurrentBag<SaveDirListing> m_cbagWorkers = new ConcurrentBag<SaveDirListing>();
        readonly UList<LVvolStrings> m_list_lvVolStrings = new UList<LVvolStrings>();

        int m_nFilesWritten = 0;
        internal int FilesWritten { get { return m_nFilesWritten; } set { m_nFilesWritten = value; } }

        class SaveDirListing : Utilities
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

                DriveInfo driveInfo = new DriveInfo(strPath.Substring(0, strPath.IndexOf(Path.DirectorySeparatorChar)));

                fs.WriteLine(mSTRdrive01);
                fs.WriteLine(driveInfo.AvailableFreeSpace);
                fs.WriteLine(driveInfo.DriveFormat);
                fs.WriteLine(driveInfo.DriveType);
                fs.WriteLine(driveInfo.Name);
                fs.WriteLine(driveInfo.RootDirectory);
                fs.WriteLine(driveInfo.TotalFreeSpace);
                fs.WriteLine(driveInfo.TotalSize);
                fs.WriteLine(driveInfo.VolumeLabel);
            }

            void TraverseTree(TextWriter fs, String root)
            {
                Stack<Win32FindFile.DATUM> stackDirs = new Stack<Win32FindFile.DATUM>(64);
                List<Win32FindFile.DATUM> listSubDirs = new List<Win32FindFile.DATUM>();
                List<Win32FindFile.DATUM> listFiles = new List<Win32FindFile.DATUM>();
                Win32FindFile.DATUM winRoot;

                Win32FindFile.FileData.WinFile(root, out winRoot);
                stackDirs.Push(winRoot);

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

                        String strOut = FormatString(strFile: strFile, dtCreated: fi.CreationTime, strAttributes: fi.Attributes.ToString("X"), dtModified: fi.LastWriteTime, nLength: fi.Size, strError1: strError1, strError2: strError2_File);

                        Utilities.Assert(1306.7307, fi.Size >= 0);

                        if (fi.Size > 0)
                        {
                            bHasLength = true;
                        }

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
                            fs.WriteLine(FormatString(strDir: strFullPath, dtCreated: di.CreationTime, strAttributes: di.Attributes.ToString("X"), dtModified: di.LastWriteTime, nLength: nDirLength, strError1: strError1, strError2: strError2_Dir));
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
                    m_statusCallback(m_volStrings.Index, mSTRnotSaved);
                    return;
                }

                if (Directory.Exists(strPath) == false)
                {
                    m_statusCallback(m_volStrings.Index, mSTRnotSaved);
                    m_MessageboxCallback("Source Path does not exist.", "Save Directory Listing");
                    return;
                }

                if (StrValid(strSaveAs) == false)
                {
                    m_statusCallback(m_volStrings.Index, mSTRnotSaved);
                    m_MessageboxCallback("Must specify save filename.", "Save Directory Listing");
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
                                m_statusCallback(m_volStrings.Index, nFilesTotal: m_nFilesTotal, nLengthTotal: m_nLengthTotal, nFilesDiff: nFilesDiff);
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
                    m_statusCallback(m_volStrings.Index, strText: mSTRsaved, bDone: true);
                }
                catch (Exception e)
                {
                    m_statusCallback(m_volStrings.Index, strText: mSTRnotSaved, bDone: true);
                    m_MessageboxCallback(strSaveAs.PadRight(100) + "\nException: " + e.Message, "Save Directory Listing");
                }
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

        internal SaveDirListings(ListView.ListViewItemCollection lvVolItems,
            SaveDirListingsStatusDelegate statusCallback,
            Action doneCallback)
        {
            foreach (SDL_ListViewItem lvItem in lvVolItems)
            {
                m_list_lvVolStrings.Add(new LVvolStrings(lvItem));
            }

            m_statusCallback = statusCallback;
            m_doneCallback = doneCallback;
        }

        void Go()
        {
            Utilities.WriteLine();
            Utilities.WriteLine("Saving directory listings.");

            DateTime dtStart = DateTime.Now;
            int nVolIx = -1;

            foreach (LVvolStrings volStrings in m_list_lvVolStrings)
            {
                ++nVolIx;

                if ((mSTRusingFile + mSTRsaved).Contains(volStrings.Status))
                {
                    continue;
                }

                m_statusCallback(nVolIx, "Saving...");
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
    }

    partial class Form1
    {
        void SaveDirListingsStatusCallback(int nIndex, String strText = null, bool bDone = false, long nFilesTotal = 0, long nLengthTotal = 0, double nFilesDiff = 0)
        {
            if (GlobalData.AppExit || (gd.m_saveDirListings == null) || gd.m_saveDirListings.IsAborted)
            {
                return;
            }

            if (InvokeRequired) { Invoke(new SaveDirListingsStatusDelegate(SaveDirListingsStatusCallback), new object[] { nIndex, strText, bDone, nFilesTotal, nLengthTotal, nFilesDiff }); return; }

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
                form_lvVolumesMain.Items[nIndex].SubItems[3].Text = strText;

                if (bDone && (strText == Utilities.mSTRsaved))
                {
                    form_lvVolumesMain.Items[nIndex].Name = null;    // indexing by path, only for unsaved volumes
                }

                form_lvVolumesMain.Invalidate();
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
            Form1MessageBox("Completed. " + nFilesWritten + " files written.", "Save Directory Listings");
        }

        bool DoSaveDirListings()
        {
            if (gd.m_saveDirListings != null)
            {
                Form1MessageBox("Already in progress.", "Save Directory Listings");
                return false;
            }

            gd.m_saveDirListings = new SaveDirListings(form_lvVolumesMain.Items,
                new SaveDirListingsStatusDelegate(SaveDirListingsStatusCallback),
                new Action(SaveDirListingsDoneCallback));
            gd.m_saveDirListings.DoThreadFactory();
            return true;
        }
    }
}
