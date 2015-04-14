using System;
using System.Collections.Generic;

namespace DoubleFile
{
    class SearchResultsDir : IComparable
    {
        internal PathBuilder
            StrDir { get; set; }
        internal readonly SortedDictionary<TabledString<Tabled_Files>, bool>
            ListFiles = new SortedDictionary<TabledString<Tabled_Files>, bool>();

        public int CompareTo(object other)
        {
            return StrDir.CompareTo(((SearchResultsDir)other).StrDir);
        }
    }
}
