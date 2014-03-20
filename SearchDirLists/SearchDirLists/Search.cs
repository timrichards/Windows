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
    delegate void SearchStatusDelegate();
    delegate void SearchDoneDelegate();

    class Search : Utilities
    {
        List<LVvolStrings> m_list_lvVolStrings = new List<LVvolStrings>();
        String m_strSearch = null;
        bool m_bCaseSensitive = true;
        SearchStatusDelegate m_callbackStatus = null;
        SearchDoneDelegate m_callbackDone = null;
        public List<ListViewItem> m_listLVitems = new List<ListViewItem>();
        System.Threading.Timer m_timerFillList = null;
        public bool m_bFillPaths = false;

        public enum FolderSpecialHandling { None, Outermost, Innermost };
        FolderSpecialHandling m_folderHandling;

        public void TimerFillList_Tick(Object stateInfo)
        {
            m_callbackStatus();
        }

        public Search(ListView.ListViewItemCollection lvItems,
            String strSearch, bool bCaseSensitive, FolderSpecialHandling folderHandling,
            SearchStatusDelegate callbackStatus, SearchDoneDelegate callbackDone)
        {
            foreach (ListViewItem lvItem in lvItems)
            {
                m_list_lvVolStrings.Add(new LVvolStrings(lvItem));
            }

            m_strSearch = strSearch;
            m_bCaseSensitive = bCaseSensitive;
            m_folderHandling = folderHandling;
            m_callbackStatus = callbackStatus;
            m_callbackDone = callbackDone;

            m_timerFillList = new System.Threading.Timer(this.TimerFillList_Tick, new AutoResetEvent(false), 500, 500);
        }

        public void Go()
        {
            Console.WriteLine("Searching for '" + m_strSearch + "'");

            DateTime dtStart = DateTime.Now;

            foreach (LVvolStrings lvStrings in m_list_lvVolStrings)
            {
                if (LV_VolumesItemInclude(lvStrings) == false)
                {
                    continue;
                }

                String strVolumeName = lvStrings.VolumeName;
                String strPath = lvStrings.Path;
                String strSaveAs = lvStrings.SaveAs;

                if (FormatPath(ref strPath, ref strSaveAs, false) == false)
                {
                    return;
                }

                using (StreamReader file = new StreamReader(strSaveAs))
                {
                    String line = "";
                    long counter = 0;   // lines number from one

                    if (m_bCaseSensitive == false)
                    {
                        m_strSearch = m_strSearch.ToLower();
                    }

                    while ((line = file.ReadLine()) != null)
                    {
                        ++counter;

                        if (m_bCaseSensitive)
                        {
                            if (line.Contains(m_strSearch) == false)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (line.ToLower().Contains(m_strSearch) == false)
                            {
                                continue;
                            }
                        }

                        String[] strArray = line.Split('\t');

                        if (strArray[0].Length > 0) // directory
                        {
                            if (m_folderHandling == FolderSpecialHandling.Outermost)
                            {
                                if (strArray[0].EndsWith(m_strSearch) == false)
                                {
                                    continue;
                                }
                            }
                            else if (m_folderHandling == FolderSpecialHandling.Innermost)
                            {
                                if (strArray.Length < 6)
                                {
                                    continue;
                                }

                                long nParse = 0;

                                if (long.TryParse(strArray[5], out nParse) == false)
                                {
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            m_bFillPaths = true;
                        }

                        if ((strArray.Length > 5) && (strArray[5].Length > 0))
                        {
                            strArray[5] = FormatSize(strArray[5]);
                        }

                        m_listLVitems.Add(new ListViewItem(strArray)); 
                        Console.WriteLine(counter.ToString() + ": " + line);
                    }
                }
            }

            m_timerFillList.Dispose();
            m_callbackDone();
            TimeSpan span = DateTime.Now - dtStart;
            String strTimeSpent = String.Format("Completed Search for {0} in {1} milliseconds.", m_strSearch, (int)span.TotalMilliseconds);

            Console.WriteLine(strTimeSpent);
            MessageBox.Show(strTimeSpent);
        }
    }

    public partial class Form1 : Form
    {
        private bool m_bThreadingSearch = false;
        Search m_search = null;

        void UpdateLV()
        {
            if (m_search == null)
            {
                Debug.Assert(false);
                return;
            }

            form_btn_Search_FillPaths.Enabled = m_search.m_bFillPaths;
            form_lv_SearchResults.Items.AddRange(m_search.m_listLVitems.ToArray());
            m_search.m_listLVitems.Clear();
        }

        void SearchStatusCallback()
        {
            if (InvokeRequired) { Invoke(new SearchStatusDelegate(SearchStatusCallback)); return; }

            UpdateLV();
        }

        void SearchDoneCallback()
        {
            if (InvokeRequired) { Invoke(new SearchDoneDelegate(SearchDoneCallback)); return; }

            UpdateLV();
            m_bThreadingSearch = false;
            m_search = null;
        }

        private void DoSearch()
        {
            if (m_bThreadingSearch)
            {
                MessageBox.Show("Already in progress.                       ", "Search");
                return;
            }

            form_lv_SearchResults.Items.Clear();
            Console.Clear();
            form_btn_Search_FillPaths.Enabled = false;

            Search.FolderSpecialHandling folderHandling = Search.FolderSpecialHandling.None;

            if (form_rad_Folder_Outermost.Checked)
            {
                folderHandling = Search.FolderSpecialHandling.Outermost;
            }
            else if (form_rad_Folder_Innermost.Checked)
            {
                folderHandling = Search.FolderSpecialHandling.Innermost;
            }

            m_strSearch = form_cb_Search.Text;
            m_search = new Search(form_LV_VolumesMain.Items, m_strSearch, form_chk_SearchCase.Checked, folderHandling,
                new SearchStatusDelegate(SearchStatusCallback), new SearchDoneDelegate(SearchDoneCallback));

            m_bThreadingSearch = true;

            new Thread(new ThreadStart(m_search.Go)).Start();
        }
    }
}
