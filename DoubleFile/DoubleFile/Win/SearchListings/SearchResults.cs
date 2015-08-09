using System.Collections.Generic;

namespace DoubleFile
{
    class SearchResults
    {
        internal readonly TabledString<TabledStringType_Files>
            StrSearch = null;
        internal readonly LVitem_ProjectVM
            LVitemProjectVM = null;
        internal readonly IEnumerable<SearchResultsDir>
            Results = null;

        internal SearchResults(string strSearch, LVitem_ProjectVM lvItemProjectVM, IEnumerable<SearchResultsDir> listResults)
        {
            StrSearch = (TabledString<TabledStringType_Files>)strSearch;
            LVitemProjectVM = lvItemProjectVM;
            Results = listResults;
        }
    }
}
