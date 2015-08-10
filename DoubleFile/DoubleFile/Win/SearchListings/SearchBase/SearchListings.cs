using System;
using System.Collections.Concurrent;
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
        internal SearchListings(LV_ProjectVM lvProjectVM, SearchBase searchBase)
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
                _cbagWorkers.Add(new SearchListing(this, volStrings, Abort).DoThreadFactory());

            foreach (SearchListing worker in _cbagWorkers)
                worker.Join();

            Util.WriteLine(string.Format("Completed Search for {0} in {1} seconds.", _strSearch, ((int)(DateTime.Now - dtStart).TotalMilliseconds / 100) / 10d));

            if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true))
                return;

            var searchStatus = _callbackWR?.Get(w => w);

            if (null == searchStatus)
            {
                Util.Assert(99859, false);
                return;
            }

            Util.ThreadMake(() => searchStatus.Done());
        }

        internal void Abort()
        {
            foreach (SearchListing worker in _cbagWorkers)
                worker.Abort();

            _thread.Abort();
        }

        internal SearchListings DoThreadFactory()
        {
            _thread = Util.ThreadMake(Go);
            return this;
        }

        readonly LV_ProjectVM
            _lvProjectVM = null;

        readonly ConcurrentBag<SearchListing>
            _cbagWorkers = new ConcurrentBag<SearchListing>();
        Thread
            _thread = new Thread(() => { });
    }
}
