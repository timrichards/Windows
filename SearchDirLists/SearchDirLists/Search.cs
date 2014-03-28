using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using System.Diagnostics;
using System.Threading;

namespace SearchDirLists
{
    delegate void SearchStatusDelegate(String strFile, List<String> listResultLines);
    delegate void SearchDoneDelegate();
    delegate void SearchResultsDelegate();

    class SearchResults
    {
        String m_strFile;
        List<String> m_listResultLines;

        public String StrFile { get { return m_strFile; } }
        public List<String> ResultLines { get { return m_listResultLines; } }

        public SearchResults(String strFile, List<String> listResultLines)
        {
            m_strFile = strFile;
            m_listResultLines = listResultLines;
        }
    }

    class SearchFile : Utilities
    {
        Thread m_thread = null;
        SearchStatusDelegate m_statusCallback = null;
        LVvolStrings m_volStrings = null;
        String m_strSearch = null;
        bool m_bCaseSensitive = true;
        List<String> m_listResultLines = new List<string>();

        public enum FolderSpecialHandling { None, Outermost, Innermost };
        FolderSpecialHandling m_folderHandling;

        public SearchFile(LVvolStrings volStrings, String strSearch, bool bCaseSensitive, FolderSpecialHandling folderHandling,
            SearchStatusDelegate statusCallback)
        {
            m_volStrings = volStrings;
            m_strSearch = strSearch;
            m_bCaseSensitive = bCaseSensitive;
            m_folderHandling = folderHandling;
            m_statusCallback = statusCallback;
        }

        public void Go()
        {
            if (LV_VolumesItemInclude(m_volStrings) == false)
            {
                return;
            }

            String strVolumeName = m_volStrings.VolumeName;
            String strPath = m_volStrings.Path;
            String strSaveAs = m_volStrings.SaveAs;

            if (FormatPath(ref strPath, ref strSaveAs, false) == false)
            {
                return;
            }

            using (StreamReader file = new StreamReader(strSaveAs))
            {
                String strLine = "";
                long nLineNo = 0;   // lines number from one

                if (m_bCaseSensitive == false)
                {
                    m_strSearch = m_strSearch.ToLower();
                }

                while ((strLine = file.ReadLine()) != null)
                {
                    ++nLineNo;

                    if (m_bCaseSensitive)
                    {
                        if (strLine.Contains(m_strSearch) == false)
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (strLine.ToLower().Contains(m_strSearch) == false)
                        {
                            continue;
                        }
                    }

                    String[] strArray = strLine.Split('\t');

                    if (strArray[2].Length > 0) // directory
                    {
                        if (m_folderHandling == FolderSpecialHandling.Outermost)
                        {
                            if (strArray[2].EndsWith(m_strSearch) == false)
                            {
                                continue;
                            }
                        }
                        else if (m_folderHandling == FolderSpecialHandling.Innermost)
                        {
                            if (strArray.Length < nColLENGTH + 1)
                            {
                                continue;
                            }

                            long nParse = 0;

                            if (long.TryParse(strArray[nColLENGTH], out nParse) == false)
                            {
                                continue;
                            }
                        }
                    }

                    if ((strArray.Length > nColLENGTH) && (strArray[nColLENGTH].Length > 0))
                    {
                        strArray[nColLENGTH] = FormatSize(strArray[nColLENGTH]);
                    }

                    m_listResultLines.Add(strLine); 
                    Console.WriteLine(strSaveAs + ": " + strLine);
                }
            }

            if (m_listResultLines.Count > 0)
            {
                m_statusCallback(strSaveAs, m_listResultLines);
            }
        }

        public Thread DoThreadFactory()
        {
            m_thread = new Thread(new ThreadStart(Go));
            m_thread.IsBackground = true;
            m_thread.Start();
            return m_thread;
        }
    }

    class Search
    {
        Thread m_thread = null;
        List<Thread> m_listThreads = new List<Thread>();
        SearchStatusDelegate m_statusCallback = null;
        SearchDoneDelegate m_doneCallback = null;
        String m_strSearch = null;
        List<LVvolStrings> m_list_lvVolStrings = new List<LVvolStrings>();
        bool m_bCaseSensitive = false;
        SearchFile.FolderSpecialHandling m_folderHandling = SearchFile.FolderSpecialHandling.Outermost;

        public Search(ListView.ListViewItemCollection lvVolItems, String strSearch, bool bCaseSensitive, SearchFile.FolderSpecialHandling folderHandling,
            SearchStatusDelegate statusCallback, SearchDoneDelegate doneCallback)
        {
            foreach (ListViewItem lvItem in lvVolItems)
            {
                m_list_lvVolStrings.Add(new LVvolStrings(lvItem));
            }

            m_strSearch = strSearch;
            m_bCaseSensitive = bCaseSensitive;
            m_folderHandling = folderHandling;
            m_statusCallback = statusCallback;
            m_doneCallback = doneCallback;
        }

        void Go()
        {
            Console.WriteLine("Searching for '" + m_strSearch + "'");

            DateTime dtStart = DateTime.Now;

            foreach (LVvolStrings volStrings in m_list_lvVolStrings)
            {
                SearchFile searchFile = new SearchFile(volStrings, m_strSearch, m_bCaseSensitive, m_folderHandling,
                    m_statusCallback);
                m_listThreads.Add(searchFile.DoThreadFactory());
            }

            foreach (Thread thread in m_listThreads)
            {
                thread.Join();
            }

            Console.WriteLine(String.Format("Completed Search for {0} in {1} seconds.", m_strSearch, ((int)(DateTime.Now - dtStart).TotalMilliseconds / 100) / 10.0));
            m_doneCallback();
        }

        public void EndThread()
        {
            foreach (Thread thread in m_listThreads)
            {
                if (thread.IsAlive)
                {
                    thread.Abort();
                }
            }

            m_listThreads = new List<Thread>();

            if ((m_thread != null) && m_thread.IsAlive)
            {
                m_thread.Abort();
            }

            m_thread = null;
        }

        public void DoThreadFactory()
        {
            m_thread = new Thread(new ThreadStart(Go));
            m_thread.IsBackground = true;
            m_thread.Start();
        }
    }

    public partial class Form1 : Form
    {
        Search m_search = null;
        List<SearchResults> m_listSearchResults = new List<SearchResults>();
        SearchResultsDelegate m_searchResultsCallback = null;

        void SearchStatusCallback(String strFile, List<String> listResultLines)
        {
            if (listResultLines.Count <= 0)
            {
                Debug.Assert(false); // caller takes care of this
                return;
            }

            lock (m_listSearchResults)
            {
                m_listSearchResults.Add(new SearchResults(strFile, listResultLines));
            }
        }

        void SearchDoneCallback()
        {
            if (InvokeRequired) { Invoke(new SearchDoneDelegate(SearchDoneCallback)); return; }

            m_search = null;
            m_searchResultsCallback();
            m_searchResultsCallback = null;
        }

        private void SearchFiles(String strSearch, SearchResultsDelegate searchResultsCallback, bool bKill = false)
        {
            if (m_search != null)
            {
                DialogResult dlgResult = DialogResult.Yes;

                if (bKill == false)
                {
                    dlgResult = MessageBox.Show("Already in progress. Restart search?       ", "Search", MessageBoxButtons.YesNoCancel);

                    if (dlgResult == DialogResult.No)
                    {
                        return;
                    }
                }

                m_search.EndThread();   // no need to null: gets redefined below

                if (dlgResult != DialogResult.Yes)
                {
                    return;
                }
            }

            m_listSearchResults = new List<SearchResults>();
            m_searchResultsCallback = searchResultsCallback;

            SearchFile.FolderSpecialHandling folderHandling = SearchFile.FolderSpecialHandling.None;

            m_search = new Search(form_lvVolumesMain.Items, strSearch, true, folderHandling,
                new SearchStatusDelegate(SearchStatusCallback), new SearchDoneDelegate(SearchDoneCallback));
            m_search.DoThreadFactory();
        }
    }
}
