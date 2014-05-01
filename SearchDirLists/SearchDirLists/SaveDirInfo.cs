﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;

namespace SearchDirLists
{
    delegate void SaveDirListingsStatusDelegate(int nIndex, String strText = null, bool bSuccess = false, long nFilesTotal = 0, long nLengthTotal = 0, double nFilesDiff = 0);
    delegate void SaveDirListingsDoneDelegate();

    class SaveDirListing : Utilities
    {
        SaveDirListingsStatusDelegate m_statusCallback = null;
        Thread m_thread = null;
        LVvolStrings m_volStrings = null;
        long m_nLengthTotal = 0;
        long m_nFilesTotal = 0;
        long m_nFilesDiff = 0;
        List<double> m_listFileDiffs = new List<double>();
        List<String> m_list_Errors = new List<String>();
        System.Threading.Timer m_timerStatus = null;

        private double StdDevSign(List<double> values)
        {
            double nAvg = values.Average();
            double nStdDev = Math.Sqrt(
                values.Sum(d => Math.Pow(d - nAvg, 2)) /
                (values.Count() - 1));

            return nStdDev *= Math.Sign(values[values.Count - 1] - nAvg);
        }
        
        void SaveDirListing_TimerCallback(object state)
        {
            m_listFileDiffs.Add(m_nFilesDiff);

            if (m_listFileDiffs.Count < 2)
            {
                return;
            }

            double nFilesDiff = m_nFilesDiff/StdDevSign(m_listFileDiffs);

            m_nFilesDiff = 0;
            m_statusCallback(m_volStrings.Index, nFilesTotal: m_nFilesTotal, nLengthTotal: m_nLengthTotal, nFilesDiff: nFilesDiff);
        }

        internal SaveDirListing(LVvolStrings volStrings,
            SaveDirListingsStatusDelegate statusCallback)
        {
            m_volStrings = volStrings;
            m_statusCallback = statusCallback;
        }

        private void WriteHeader(TextWriter fs, String strVolumeName, String strPath)
        {
            fs.WriteLine(m_str_HEADER_01);
            // assume SaveFields() by caller because SaveFields() has already prompted user
            fs.WriteLine(strVolumeName);
            fs.WriteLine(strPath);

            DriveInfo driveInfo = new DriveInfo(strPath.Substring(0, strPath.IndexOf(Path.DirectorySeparatorChar)));

            fs.WriteLine(m_str_DRIVE_01);
            fs.WriteLine(driveInfo.AvailableFreeSpace);
            fs.WriteLine(driveInfo.DriveFormat);
            fs.WriteLine(driveInfo.DriveType);
            fs.WriteLine(driveInfo.Name);
            fs.WriteLine(driveInfo.RootDirectory);
            fs.WriteLine(driveInfo.TotalFreeSpace);
            fs.WriteLine(driveInfo.TotalSize);
            fs.WriteLine(driveInfo.VolumeLabel);
        }

        String CheckNTFS_chars(String strFile, bool bFile = false)
        {
            char[] arrChar = bFile ? Path.GetInvalidFileNameChars() : Path.GetInvalidPathChars();
            int nIx = -1;

            if ((nIx = strFile.IndexOfAny(arrChar)) > -1)
            {
                return "NTFS ASCII " + ((int)strFile[nIx]).ToString();
            }
            else
            {
                return null;
            }
        }

        void TraverseTree(TextWriter fs, String root)
        {
            Stack<Win32.WIN32_FIND_DATA> stackDirs = new Stack<Win32.WIN32_FIND_DATA>(64);
            List<Win32.WIN32_FIND_DATA> listSubDirs = new List<Win32.WIN32_FIND_DATA>();
            List<Win32.WIN32_FIND_DATA> listFiles = new List<Win32.WIN32_FIND_DATA>();
            Win32.WIN32_FIND_DATA winRoot;

            Win32.FileData.WinFile(root, out winRoot);
            stackDirs.Push(winRoot);

            while (stackDirs.Count > 0)
            {
                Win32.WIN32_FIND_DATA winDir = stackDirs.Pop();
                long nDirLength = 0;
                bool bHasLength = false;
                String strError2_Dir = CheckNTFS_chars(winDir.strAltFileName);

                if (Win32.GetDirectory(winDir.strAltFileName, ref listSubDirs, ref listFiles) == false)
                {
                    m_list_Errors.Add(FormatString(strDir: winDir.strAltFileName,
                        strError1: new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error()).Message, strError2: strError2_Dir));
                    continue;
                }

                foreach (Win32.WIN32_FIND_DATA winData in listFiles)
                {
                    Win32.FileData fi = new Win32.FileData(winData);
                    String strError2_File = CheckNTFS_chars(winData.strFileName, bFile: true);

                    if (fi.IsValid == false)
                    {
                        String strErrorFile = winData.strFileName;
                        String strErrorDir = winData.strAltFileName.Substring(0, winData.strAltFileName.LastIndexOf(Path.DirectorySeparatorChar));

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

                    String strOut = FormatString(strFile: fi.Name, dtCreated: fi.CreationTime, strAttributes: fi.Attributes.ToString("X"), dtModified: fi.LastWriteTime, nLength: fi.Size, strError1: strError1, strError2: strError2_File);

                    Utilities.Assert(1306.7307, fi.Size >= 0);

                    if (fi.Size > 0)
                    {
                        bHasLength = true;
                    }

                    fs.WriteLine(strOut);
                }

                {
                    String strError1 = null;

                    if (winDir.strAltFileName.Length > 240)
                    {
                        strError1 = "Path Length: " + winDir.strAltFileName.Length.ToString();
                    }

                    Utilities.Assert(1306.7301, bHasLength == (nDirLength > 0));
                    Win32.FileData di = new Win32.FileData(winDir);

                    if (winDir.strAltFileName.EndsWith(@":\"))
                    {
                        Utilities.Assert(1306.7302, di.IsValid == false);          // yes, yes...
                        Utilities.Assert(1306.7303, winDir.strAltFileName.Length == 3);
                        fs.WriteLine(FormatString(strDir: winDir.strAltFileName, nLength: nDirLength, strError1: strError1, strError2: strError2_Dir));
                    }
                    else
                    {
                        Utilities.Assert(1306.7304, di.IsValid);
                        fs.WriteLine(FormatString(strDir: winDir.strAltFileName, dtCreated: di.CreationTime, strAttributes: di.Attributes.ToString("X"), dtModified: di.LastWriteTime, nLength: nDirLength, strError1: strError1, strError2: strError2_Dir));
                    }
                }

                foreach (Win32.WIN32_FIND_DATA winData in listSubDirs)
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
                m_statusCallback(m_volStrings.Index, "Not saved.");
                return;
            }

            if (Directory.Exists(strPath) == false)
            {
                m_statusCallback(m_volStrings.Index, "Not saved.");
                m_MessageboxCallback("Source Path does not exist.".PadRight(100), "Save Directory Listing");
                return;
            }

            if (StrValid(strSaveAs) == false)
            {
                m_statusCallback(m_volStrings.Index, "Not saved.");
                m_MessageboxCallback("Must specify save filename.".PadRight(100), "Save Directory Listing");
                return;
            }

            String strPathOrig = Directory.GetCurrentDirectory();

            using (TextWriter fs = File.CreateText(strSaveAs))
            {
                WriteHeader(fs, strVolumeName, strPath);
                fs.WriteLine();
                fs.WriteLine(FormatString(nHeader: 0));
                fs.WriteLine(FormatString(nHeader: 1));
                fs.WriteLine(m_str_START_01 + " " + DateTime.Now.ToString());
                m_timerStatus = new System.Threading.Timer(new TimerCallback(SaveDirListing_TimerCallback), null, 1000, 1000);
                TraverseTree(fs, strPath);
                m_timerStatus.Dispose();
                m_timerStatus = null;
                fs.WriteLine(m_str_END_01 + " " + DateTime.Now.ToString());
                fs.WriteLine();
                fs.WriteLine(m_str_ERRORS_LOC_01);

                foreach (String strError in m_list_Errors)
                {
                    fs.WriteLine(strError);
                }

                fs.WriteLine();
                fs.WriteLine(FormatString(strDir: m_str_TOTAL_LENGTH_LOC_01, nLength: m_nLengthTotal));
            }

            Directory.SetCurrentDirectory(strPathOrig);
            m_statusCallback(m_volStrings.Index, strText: m_str_SAVED, bSuccess: true);
        }

        internal Thread DoThreadFactory()
        {
            m_thread = new Thread(new ThreadStart(Go));
            m_thread.IsBackground = true;
            m_thread.Start();
            return m_thread;
        }
    }

    class SaveDirListings : Utilities
    {
        SaveDirListingsStatusDelegate m_statusCallback = null;
        SaveDirListingsDoneDelegate m_doneCallback = null;
        Thread m_thread = null;
        List<Thread> m_listThreads = new List<Thread>();
        List<LVvolStrings> m_list_lvVolStrings = new List<LVvolStrings>();

        int m_nFilesWritten = 0;
        internal int FilesWritten { get { return m_nFilesWritten; } set { m_nFilesWritten = value; } }

        internal SaveDirListings(ListView.ListViewItemCollection lvVolItems,
                SaveDirListingsStatusDelegate statusCallback,
                SaveDirListingsDoneDelegate doneCallback)
        {
            foreach (ListViewItem lvItem in lvVolItems)
            {
                m_list_lvVolStrings.Add(new LVvolStrings(lvItem));
            }

            m_statusCallback = statusCallback;
            m_doneCallback = doneCallback;
        }

        void Go()
        {
            Console.WriteLine();
            Console.WriteLine("Saving directory listings.");

            DateTime dtStart = DateTime.Now;
            int nVolIx = -1;

            foreach (LVvolStrings volStrings in m_list_lvVolStrings)
            {
                ++nVolIx;

                if ((m_str_USING_FILE + m_str_SAVED).Contains(volStrings.Status))
                {
                    continue;
                }

                m_statusCallback(nVolIx, "Saving...");
                m_listThreads.Add(new SaveDirListing(volStrings, m_statusCallback).DoThreadFactory());
            }

            foreach (Thread thread in m_listThreads)
            {
                thread.Join();
            }

            Console.WriteLine(String.Format("Finished saving directory listings in {0} seconds.", ((int)(DateTime.Now - dtStart).TotalMilliseconds / 100) / 10.0));
            m_doneCallback();
        }

        internal void EndThread()
        {
            lock (m_listThreads)
            {
                foreach (Thread thread in m_listThreads)
                {
                    if (thread.IsAlive)
                    {
                        thread.Abort();
                    }
                }
            }

            m_listThreads.Clear();

            if ((m_thread != null) && m_thread.IsAlive)
            {
                m_thread.Abort();
            }

            m_thread = null;
        }

        internal void DoThreadFactory()
        {
            m_thread = new Thread(new ThreadStart(Go));
            m_thread.IsBackground = true;
            m_thread.Start();
        }
    }

    partial class Form1 : Form
    {
        SaveDirListings m_saveDirListings = null;

        void SaveDirListingsStatusCallback(int nIndex, String strText = null, bool bSuccess = false, long nFilesTotal = 0, long nLengthTotal = 0, double nFilesDiff = 0)
        {
            if (InvokeRequired) { Invoke(new SaveDirListingsStatusDelegate(SaveDirListingsStatusCallback), new object[] { nIndex, strText, bSuccess, nFilesTotal, nLengthTotal, nFilesDiff }); return; }

            if (nLengthTotal > 0)
            {
                Utilities.Assert(1306.7305, strText == null);
                Utilities.Assert(1306.7306, bSuccess == false);

                strText = nFilesTotal.ToString("###,###,###,###") + " (" + Utilities.FormatSize(nLengthTotal) + ") SD " + nFilesDiff.ToString("#0.0");
            }

            if (bSuccess)
            {
                lock (m_saveDirListings)
                {
                    ++m_saveDirListings.FilesWritten;
                }
            }

            lock (form_lvVolumesMain)
            {
                form_lvVolumesMain.Items[nIndex].SubItems[3].Text = strText;
                form_lvVolumesMain.Invalidate();
            }
        }

        void SaveDirListingsDoneCallback()
        {
            if (InvokeRequired) { Invoke(new SaveDirListingsDoneDelegate(SaveDirListingsDoneCallback)); return; }

            if (m_saveDirListings.FilesWritten > 0)
            {
                RestartTreeTimer();
                form_tabControl.SelectedTab = form_tabPageBrowse;
            }

            MessageBox.Show("Completed. " + m_saveDirListings.FilesWritten + " files written.               ", "Save Directory Listings");
            m_saveDirListings = null;
        }

        private void DoSaveDirListings()
        {
            if (m_saveDirListings != null)
            {
                MessageBox.Show("Already in progress.                       ", "Save Directory Listings");
                return;
            }

            m_saveDirListings = new SaveDirListings(form_lvVolumesMain.Items,
                new SaveDirListingsStatusDelegate(SaveDirListingsStatusCallback),
                new SaveDirListingsDoneDelegate(SaveDirListingsDoneCallback));
            m_saveDirListings.DoThreadFactory();
        }
    }
}
