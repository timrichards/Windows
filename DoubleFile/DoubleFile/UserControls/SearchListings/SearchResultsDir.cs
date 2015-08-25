using System;
using System.Collections.Generic;

namespace DoubleFile
{
    class SearchResultsDir : IComparable<SearchResultsDir>, IComparable
    {
        internal PathBuilder
            PathBuilder;
        internal readonly IDictionary<TabledString<TabledStringType_Files>, bool>
            ListFiles = new Dictionary<TabledString<TabledStringType_Files>, bool>();

        public int CompareTo(object other)
        {
            var nRet = CompareTo(((SearchResultsDir)other).PathBuilder);

            Util.Assert(99790, 0 != nRet);
            return nRet;
        }

        public int CompareTo(SearchResultsDir that) => PathBuilder.CompareTo(that.PathBuilder);
    }
}
