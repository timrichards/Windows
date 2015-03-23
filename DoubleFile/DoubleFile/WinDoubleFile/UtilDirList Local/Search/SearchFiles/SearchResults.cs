using System.Collections.Generic;

namespace DoubleFile
{
    class SearchResults
    {
        internal readonly string
            StrSearch = null;
        internal readonly LVitem_ProjectVM
            VolStrings = null;
        internal readonly IReadOnlyList<SearchResultsDir>
            Results = null;

        internal SearchResults(string strSearch, LVitem_ProjectVM volStrings, IReadOnlyList<SearchResultsDir> listResults)
        {
            StrSearch = strSearch;
            VolStrings = volStrings;
            Results = listResults;
        }
    }
}
