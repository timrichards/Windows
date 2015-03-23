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
            return StrDir.CompareTo(((SearchResultsDir)other).StrDir);
        }
    }
}
