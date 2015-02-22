using System;
using System.Collections.Concurrent;
using System.Linq;

namespace DoubleFile
{
    // can't be struct because of null
    class UString : IComparable<string>
    {
        public static implicit operator UString(string value) { return new UString { nIndex = Set(value) }; }
        public static implicit operator string(UString value) { return Get(value.nIndex); }
        public int CompareTo(string other) { return Get(nIndex).CompareTo(other); }

        internal bool Contains(UString ustr) { return Get(nIndex).Contains(Get(ustr.nIndex)); }
        internal bool Contains(char ch) { return Get(nIndex).Contains(ch); }
        internal bool EndssWith(string str) { return Get(nIndex).EndsWith(str); }
        internal string[] Split(char ch) { return Get(nIndex).Split(ch); }
        internal string[] Split(string[] arrStr, StringSplitOptions opts) { return Get(nIndex).Split(arrStr, opts); }
        internal bool StartsWith(string str) { return Get(nIndex).StartsWith(str); }
        internal UString ToLower() { return Get(nIndex).ToLower(); }

        static internal UString Empty { get { return string.Empty; } }

        static internal int Set(string str)
        {
            if (false == _dictStrings.ContainsKeyA(str))
            {
                _dictStrings[str] = _dictStrings.Count - 1;
            }

            return _dictStrings[str];
        }

        static internal string Get(int nIndex)
        {
            return _dictStrings.Keys.ElementAt(nIndex);
        }

        int nIndex;
        static ConcurrentDictionary<string, int> _dictStrings = new ConcurrentDictionary<string, int>();
    }
}
