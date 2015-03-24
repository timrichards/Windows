using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace DoubleFile
{
    delegate void SearchStatusDelegate(SearchResults searchResults, bool bFirst = false, bool bLast = false);

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
            IsAborted = false;
            _lvProjectVM = lvProjectVM;
            _strSearch = strSearch;
            _bCaseSensitive = bCaseSensitive;
            _folderHandling = folderHandling;          // not used
            _bSearchFilesOnly = bSearchFilesOnly;
            _strCurrentNode = strCurrentNode;
            _doneCallback = doneCallback;
        }

        void Go()
        {
            if (null == _lvProjectVM)
                return;

            UtilProject.WriteLine("Searching for '" + _strSearch + "'");

            var dtStart = DateTime.Now;

            foreach (var volStrings in _lvProjectVM.ItemsCast)
                _cbagWorkers.Add(new SearchFile(this, volStrings).DoThreadFactory());

            foreach (SearchFile worker in _cbagWorkers)
                worker.Join();

            UtilProject.WriteLine(string.Format("Completed Search for {0} in {1} seconds.", _strSearch, ((int)(DateTime.Now - dtStart).TotalMilliseconds / 100) / 10.0));

            if (App.LocalExit || IsAborted)
                return;

            _doneCallback();
        }

        internal void EndThread()
        {
            foreach (SearchFile worker in _cbagWorkers)
                worker.Abort();

            IsAborted = true;
            _thread = null;
        }

        internal SearchType2 DoThreadFactory()
        {
            _thread = new Thread(Go);
            _thread.IsBackground = true;
            _thread.Start();
            return this;
        }

        internal bool
            IsAborted { get; private set; }

        readonly Action
            _doneCallback = null;
        readonly LV_ProjectVM
            _lvProjectVM = null;

        readonly ConcurrentBag<SearchFile>
            _cbagWorkers = new ConcurrentBag<SearchFile>();
        Thread
            _thread = null;
    }
}
