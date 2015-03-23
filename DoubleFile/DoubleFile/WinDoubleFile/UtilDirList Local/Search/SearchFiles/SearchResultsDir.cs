using System;
using System.Collections.Generic;

namespace DoubleFile
{
    class SearchResultsDir : IComparable
    {
        internal string
            StrDir { get; set; }

        internal KeyListSorted<string>
            ListFiles { get { return _listFiles; } }
        readonly KeyListSorted<string> _listFiles = new KeyListSorted<string>();

        public int CompareTo(object other)
        {
            var otherResults = other as SearchResultsDir;
            var nCmp = StrDir.CompareTo(otherResults.StrDir);

            if (0 != nCmp)
                return nCmp;

            if (object.ReferenceEquals(this, other))
                return 0;

            return 1;       // objects are unequal but have the same directory. Stick it at the end.
        }
    }
}
