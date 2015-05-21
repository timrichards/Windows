using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace DoubleFile
{
    interface ISearchStatus
    {
        void Status(SearchResults searchResults, bool bFirst = false, bool bLast = false);
        void Done();
    }

    partial class SearchType2 : SearchBase
    {
        internal SearchType2(
            LV_ProjectVM lvProjectVM,
            string strSearch, 
            bool bCaseSensitive,
            SearchBase.FolderSpecialHandling folderHandling,
            bool bSearchFilesOnly,
            string strCurrentNode,
            WeakReference<ISearchStatus> callbackWR)
            : base(callbackWR)
        {
            IsAborted = false;
            _lvProjectVM = lvProjectVM;
            _strSearch = strSearch;
            _bCaseSensitive = bCaseSensitive;
            _folderHandling = folderHandling;          // not used
            _bSearchFilesOnly = bSearchFilesOnly;
            _strCurrentNode = strCurrentNode;
        }

        void Go()
        {
            if (null == _lvProjectVM)
                return;

            Util.WriteLine("Searching for '" + _strSearch + "'");

            var dtStart = DateTime.Now;

            foreach (var volStrings in _lvProjectVM.ItemsCast)
                _cbagWorkers.Add(new SearchFile(this, volStrings).DoThreadFactory());

            foreach (SearchFile worker in _cbagWorkers)
                worker.Join();

            Util.WriteLine(string.Format("Completed Search for {0} in {1} seconds.", _strSearch, ((int)(DateTime.Now - dtStart).TotalMilliseconds / 100) / 10.0));

            if (App.LocalExit || IsAborted)
                return;

            if (null == _callbackWR)
            {
                MBoxStatic.Assert(99860, false);
                return;
            }

            ISearchStatus searchStatus = null;

            _callbackWR.TryGetTarget(out searchStatus);

            if (null == searchStatus)
            {
                MBoxStatic.Assert(99859, false);
                return;
            }

            searchStatus.Done();
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

        readonly LV_ProjectVM
            _lvProjectVM = null;

        readonly ConcurrentBag<SearchFile>
            _cbagWorkers = new ConcurrentBag<SearchFile>();
        Thread
            _thread = null;
    }
}
