using System;
using System.Collections.Generic;

namespace DoubleFile
{
    class SearchResults
    {
        internal readonly TabledString<Tabled_Files>
            StrSearch = null;
        internal readonly LVitem_ProjectVM
            VolStrings = null;
        internal readonly IEnumerable<SearchResultsDir>
            Results = null;

        internal SearchResults(string strSearch, LVitem_ProjectVM volStrings, IEnumerable<SearchResultsDir> listResults)
        {
            StrSearch = strSearch;
            VolStrings = volStrings;
            Results = listResults;
        }
    }
}
