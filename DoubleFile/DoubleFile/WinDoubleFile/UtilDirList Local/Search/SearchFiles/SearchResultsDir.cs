using System;
using System.Collections.Generic;

namespace DoubleFile
{
    class SearchResultsDir : IComparable
    {
        internal PathBuilder<TypedArray1>
            StrDir { get; set; }
        internal readonly SortedDictionary<TabledString<TypedArray1>, bool>
            ListFiles = new SortedDictionary<TabledString<TypedArray1>, bool>();

        public int CompareTo(object other)
        {
            return StrDir.CompareTo(((SearchResultsDir)other).StrDir);
        }
    }
}
