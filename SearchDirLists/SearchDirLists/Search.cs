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
    delegate void SearchStatusDelegate(String strSearch, LVvolStrings volStrings, List<SearchResultDir> listResults);
    delegate void SearchDoneDelegate();
    delegate void SearchResultsDelegate();

    class SearchResultDir
    {
        String m_strDir;
        List<String> m_listFiles = new List<string>();

        public String StrDir { get { return m_strDir; } set { m_strDir = value; } }
        public List<String> ListFiles { get { return m_listFiles; } }

        void AddFile(String strFile)
        {
            m_listFiles.Add(strFile);
        }
    }

    class SearchResults
    {
        String m_strSearch = null;
        LVvolStrings m_volStrings = null;
        List<SearchResultDir> m_listResults = null;

        public String StrSearch { get { return m_strSearch; } }
        public LVvolStrings VolStrings { get { return m_volStrings; } }
        public List<SearchResultDir> Results { get { return m_listResults; } }

        public SearchResults(String strSearch, LVvolStrings volStrings, List<SearchResultDir> listResults)
        {
            m_strSearch = strSearch;
            m_volStrings = volStrings;
            m_listResults = listResults;
        }
    }

    class SearchFile : Utilities
    {
        Thread m_thread = null;
        SearchStatusDelegate m_statusCallback = null;
        LVvolStrings m_volStrings = null;
        String m_strSearch = null;
        bool m_bCaseSensitive = true;
        List<SearchResultDir> m_listResults = new List<SearchResultDir>();

        public enum FolderSpecialHandling { None, Outermost, Innermost };       // not used
        FolderSpecialHandling m_folderHandling;                                 // not used

        public SearchFile(LVvolStrings volStrings, String strSearch, bool bCaseSensitive, FolderSpecialHandling folderHandling,
            SearchStatusDelegate statusCallback)
        {
            m_volStrings = volStrings;
            m_strSearch = strSearch;
            m_bCaseSensitive = bCaseSensitive;
            m_folderHandling = folderHandling;                                 // not used
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

                if (m_bCaseSensitive == false)
                {
                    m_strSearch = m_strSearch.ToLower();
                }

                SearchResultDir searchResultDir = null;

                while ((strLine = file.ReadLine()) != null)
                {
                    String[] arrLine = strLine.Split('\t');

                    if (strLine.StartsWith(m_strLINETYPE_Directory) && (searchResultDir != null))
                    {
                        searchResultDir.StrDir = arrLine[2];
                        searchResultDir.ListFiles.Sort();
                        m_listResults.Add(searchResultDir);
                        searchResultDir = null;
                    }

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

                    if (searchResultDir == null)
                    {
                        searchResultDir = new SearchResultDir();
                    }

                    if (strLine.StartsWith(m_strLINETYPE_Directory))
                    {
                        searchResultDir.StrDir = arrLine[2];
                        m_listResults.Add(searchResultDir);
                        searchResultDir = null;
                    }
                    else if (strLine.StartsWith(m_strLINETYPE_File))
                    {
                        searchResultDir.ListFiles.Add(arrLine[3]);
                    }
                }

                Debug.Assert(searchResultDir == null);
            }

            m_listResults.Sort((x, y) => x.StrDir.CompareTo(y.StrDir));
            m_statusCallback(m_strSearch, m_volStrings, m_listResults);
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
        SearchFile.FolderSpecialHandling m_folderHandling = SearchFile.FolderSpecialHandling.Outermost;     // not used

        public Search(ListView.ListViewItemCollection lvVolItems, String strSearch, bool bCaseSensitive, SearchFile.FolderSpecialHandling folderHandling,
            SearchStatusDelegate statusCallback, SearchDoneDelegate doneCallback)
        {
            foreach (ListViewItem lvItem in lvVolItems)
            {
                m_list_lvVolStrings.Add(new LVvolStrings(lvItem));
            }

            m_strSearch = strSearch;
            m_bCaseSensitive = bCaseSensitive;
            m_folderHandling = folderHandling;                                                       // not used
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

        void SearchStatusCallback(String strSearch, LVvolStrings volStrings, List<SearchResultDir> listResults)
        {
            if (listResults.Count <= 0)
            {
                Debug.Assert(false); // caller takes care of this
                return;
            }

            lock (m_listSearchResults)
            {
                m_listSearchResults.Add(new SearchResults(strSearch, volStrings, listResults));
            }
        }

        void SearchDoneCallback()
        {
            if (InvokeRequired) { Invoke(new SearchDoneDelegate(SearchDoneCallback)); return; }

            lock (m_search)
            {
                m_search = null;
            }

            m_listSearchResults.Sort((x, y) => (x.VolStrings.VolumeName.CompareTo(y.VolStrings.VolumeName)));
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
