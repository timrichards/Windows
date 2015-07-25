using System;
using System.Collections.Generic;

namespace DoubleFile
{
    class SearchResultsDir : IComparable
    {
        internal PathBuilder
            StrDir;
        internal readonly SortedDictionary<TabledString<Tabled_Files>, bool>
            ListFiles = new SortedDictionary<TabledString<Tabled_Files>, bool>();

        public int CompareTo(object other)
        {
            var nRet = StrDir.CompareTo(((SearchResultsDir)other).StrDir);

            Util.Assert(99790, 0 != nRet);
            return nRet;
        }
    }
}
