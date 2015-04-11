using System;
using System.Collections.Generic;

namespace DoubleFile
{
    class SearchResultsDir : IComparable
    {
        internal string
            StrDir { get; set; }
        internal readonly SortedDictionary<string, bool>
            ListFiles = new SortedDictionary<string, bool>();

        public int CompareTo(object other)
        {
            return StrDir.CompareTo(((SearchResultsDir)other).StrDir);
        }
    }
}
