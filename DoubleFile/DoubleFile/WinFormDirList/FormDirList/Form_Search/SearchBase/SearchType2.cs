using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace DoubleFile
{
    partial class SearchType2 : SearchBase
    {
        internal SearchType2(
            LV_ProjectVM lvProjectVM,
            string strSearch, 
            bool bCaseSensitive,
            SearchBase.FolderSpecialHandling folderHandling,
            bool bSearchFilesOnly,
            string strCurrentNode,
            SearchStatusDelegate statusCallback,
            Action doneCallback)
            : base(statusCallback)
        {
            m_lvProjectVM = lvProjectVM;
            m_strSearch = strSearch;
            m_bCaseSensitive = bCaseSensitive;
            m_folderHandling = folderHandling;          // not used
            m_bSearchFilesOnly = bSearchFilesOnly;
            m_strCurrentNode = strCurrentNode;
            m_doneCallback = doneCallback;
        }

        void Go()
        {
            UtilProject.WriteLine("Searching for '" + m_strSearch + "'");

            DateTime dtStart = DateTime.Now;

            foreach (var volStrings in m_lvProjectVM.ItemsCast)
            {
                SearchFile searchFile = new SearchFile((SearchBase)this, volStrings);

                m_cbagWorkers.Add(searchFile.DoThreadFactory());
            }

            foreach (SearchFile worker in m_cbagWorkers)
            {
                worker.Join();
            }

            UtilProject.WriteLine(string.Format("Completed Search for {0} in {1} seconds.", m_strSearch, ((int)(DateTime.Now - dtStart).TotalMilliseconds / 100) / 10.0));

            if (App.LocalExit || m_bThreadAbort)
                return;

            m_doneCallback();
        }

        internal void EndThread()
        {
            foreach (SearchFile worker in m_cbagWorkers)
            {
                worker.Abort();
            }

            m_bThreadAbort = true;
            m_thread = null;
        }

        internal void DoThreadFactory()
        {
            m_thread = new Thread(Go);
            m_thread.IsBackground = true;
            m_thread.Start();
        }

        internal bool IsAborted { get { return m_bThreadAbort; } }

        readonly Action m_doneCallback = null;
        readonly LV_ProjectVM m_lvProjectVM = null;

        Thread m_thread = null;
        bool m_bThreadAbort = false;
        ConcurrentBag<SearchFile> m_cbagWorkers = new ConcurrentBag<SearchFile>();
    }
}
