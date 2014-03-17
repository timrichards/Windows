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
    delegate void SearchStatusDelegate(String[] strArray, bool bFillPaths);

    class Search : Utilities
    {
        List<LVvolStrings> m_list_lvVolStrings = new List<LVvolStrings>();
        SearchStatusDelegate m_callbackStatus = null;
        String m_strSearch = null;
        bool m_bCaseSensitive = true;

        public enum FolderSpecialHandling { None, Outermost, Innermost };
        FolderSpecialHandling m_folderHandling;

        public Search(ListView.ListViewItemCollection lvItems, SearchStatusDelegate callbackStatus,
            String strSearch, bool bCaseSensitive, FolderSpecialHandling folderHandling)
        {
            foreach (ListViewItem lvItem in lvItems)
            {
                m_list_lvVolStrings.Add(new LVvolStrings(lvItem));
            }

            m_callbackStatus = callbackStatus;
            m_strSearch = strSearch;
            m_bCaseSensitive = bCaseSensitive;
            m_folderHandling = folderHandling;
        }

        public void Go()
        {
            Console.WriteLine("Searching for '" + m_strSearch + "'");

            DateTime dtStart = DateTime.Now;
            bool bFillPaths = false;

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
                    return; // false;
                }

                using (StreamReader file = new StreamReader(strSaveAs))
                {
                    String line = "";
                    long counter = -1;

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
                            bFillPaths = true;
                        }

                        if ((strArray.Length > 5) && (strArray[5].Length > 0))
                        {
                            strArray[5] = FormatSize(strArray[5]);
                        }

                        m_callbackStatus(strArray, bFillPaths);
                        Console.WriteLine(counter.ToString() + ": " + line);
                    }
                }
            }

            TimeSpan span = DateTime.Now - dtStart;
            String strTimeSpent = String.Format("Completed Search for {0} in {1} milliseconds.", m_strSearch, (int)span.TotalMilliseconds);

            Console.WriteLine(strTimeSpent);
            MessageBox.Show(strTimeSpent);
        }
    }

    public partial class Form1 : Form
    {
        void SearchStatusCallback(String[] strArray, bool bFillPaths)
        {
            if (InvokeRequired)
            {
                // called on a worker thread. marshal the call to the user interface thread
                Invoke(new SearchStatusDelegate(SearchStatusCallback), new object[] { strArray, bFillPaths });
                return;
            }

            form_lv_SearchResults.Items.Add(new ListViewItem(strArray));
            form_btn_Search_FillPaths.Enabled = bFillPaths;
        }

        private void DoSearch()
        {
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
            Search search = new Search(form_lv_Volumes.Items, new SearchStatusDelegate(SearchStatusCallback),
                m_strSearch, form_chk_SearchCase.Checked, folderHandling);

            new Thread(new ThreadStart(search.Go)).Start();
        }
    }
}