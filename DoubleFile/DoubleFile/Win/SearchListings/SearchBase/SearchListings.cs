using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;

namespace DoubleFile
{
    interface ISearchStatus
    {
        void Status(SearchResults searchResults, bool bFirst = false, bool bLast = false);
        void Done();
    }

    partial class SearchListings : SearchBase
    {
        internal SearchListings(
            LV_ProjectVM lvProjectVM,
            SearchBase searchBase)
            : base(searchBase)
        {
            _lvProjectVM = lvProjectVM;
        }

        void Go()
        {
            if (null == _lvProjectVM)
                return;

            Util.WriteLine("Searching for '" + _strSearch + "'");

            var dtStart = DateTime.Now;

            foreach (var volStrings in _lvProjectVM.ItemsCast)
                _cbagWorkers.Add(new SearchListing(this, volStrings).DoThreadFactory());

            foreach (SearchListing worker in _cbagWorkers)
                worker.Join();

            Util.WriteLine(string.Format("Completed Search for {0} in {1} seconds.", _strSearch, ((int)(DateTime.Now - dtStart).TotalMilliseconds / 100) / 10d));

            if ((null == Application.Current) || Application.Current.Dispatcher.HasShutdownStarted)
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
            foreach (SearchListing worker in _cbagWorkers)
                worker.Abort();

            IsAborted = true;
            _thread.Abort();
        }

        internal SearchListings DoThreadFactory()
        {
            _thread = Util.ThreadMake(Go);
            return this;
        }

        internal bool
            IsAborted { get; private set; }

        readonly LV_ProjectVM
            _lvProjectVM = null;

        readonly ConcurrentBag<SearchListing>
            _cbagWorkers = new ConcurrentBag<SearchListing>();
        Thread
            _thread = new Thread(() => { });
    }
}
