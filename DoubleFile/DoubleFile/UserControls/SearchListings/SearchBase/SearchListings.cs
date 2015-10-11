using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;

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

            var stopwatch = Stopwatch.StartNew();

            foreach (var lvItemProjectVM in _lvProjectVM.Items.Cast<LVitemProject_Explorer>())
                _cbagWorkers.Add(new SearchListing(this, lvItemProjectVM, Abort).DoThreadFactory());

            foreach (SearchListing worker in _cbagWorkers)
                worker.Join();

            stopwatch.Stop();
            Util.WriteLine(string.Format("Completed Search for {0} in {1} seconds.", _strSearch, ((int)stopwatch.ElapsedMilliseconds / 100) / 10d));
            Util.ThreadMake(() => _callbackWR?.Get(w => w.Done()));
        }

        internal void Abort(ISearchListing caller = null)
        {
            foreach (SearchListing worker in _cbagWorkers)
            {
                if (caller != worker)
                    worker.Abort();
            }
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
