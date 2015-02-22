using System;
using System.Collections.Concurrent;
using System.Linq;

namespace DoubleFile
{
    // can't be struct because of null
    class UString : IComparable<string>
    {
        public static implicit operator UString(string value){ return (value == null) ? null : new UString { nIndex = Set(value) }; }
        public static implicit operator string(UString value) { if (value == null) return null; return Get(value.nIndex); }
        public int CompareTo(string other) { return Get(nIndex).CompareTo(other); }

        internal bool Contains(UString ustr) { return Get(nIndex).Contains(Get(ustr.nIndex)); }
        internal bool Contains(char ch) { return Get(nIndex).Contains(ch); }
        internal bool EndsWith(string str) { return Get(nIndex).EndsWith(str); }
        internal string[] Split(char ch) { return Get(nIndex).Split(ch); }
        internal string[] Split(string[] arrStr, StringSplitOptions opts) { return Get(nIndex).Split(arrStr, opts); }
        internal bool StartsWith(string str) { return Get(nIndex).StartsWith(str); }
        internal UString ToLower() { return Get(nIndex).ToLower(); }

        static internal UString Empty { get { return string.Empty; } }

        static internal void AddRef()
        {
            if (_refCount == 0)
            {
                MBoxStatic.Assert(0, _dictStrings.Count == 0);
            }

            ++_refCount;
        }

        static internal void DropRef()
        {
            --_refCount;
            MBoxStatic.Assert(0, _refCount >= 0);

            if (_refCount == 0)
            {
                _dictStrings.Clear();
                _dictStringsRev.Clear();
            }
        }

        static internal void GenerationEnded()
        {
            _dictStringsRev.Clear();
            _bGenerating = false;
        }

        static internal int Set(string str)
        {
            int nValue = 0;

            if (false == _dictStrings.TryGetValue(str, out nValue))
            {
                _dictStrings[str] = _indexGenerator;
                _dictStringsRev[_indexGenerator] = str;
                return _indexGenerator++;
            }

            return nValue;
        }

        static internal string Get(int nIndex)
        {
            return _bGenerating ? GetB(nIndex) : GetA(nIndex);
        }

        static string GetA(int nIndex)
        {
            string sRet = null;

            _dictStrings
                .Where(kvp => kvp.Value == nIndex)
                .FirstOnlyAssert(kvp => sRet = kvp.Key);

            return sRet;
        }

        static string GetB(int nIndex)
        {
            string strValue = null;

            if (false == _dictStringsRev.TryGetValue(nIndex, out strValue))
            {
                MBoxStatic.Assert(0, false);
                return null;
            }

            return strValue;
        }

        int nIndex;
        static ConcurrentDictionary<string, int> _dictStrings = new ConcurrentDictionary<string, int>();
        static ConcurrentDictionary<int, string> _dictStringsRev = new ConcurrentDictionary<int, string>();
        static int _refCount = 0;
        static int _indexGenerator = 0;
        static bool _bGenerating = true;
    }
}
