using System;
using System.Collections.Concurrent;
using System.Linq;

namespace DoubleFile
{
    // can't be struct because of null
    class UString : IComparable<string>
    {
        public static implicit operator UString(string value){ return (value == null) ? null : new UString { nIndex = Set(value) }; }
        public static implicit operator string(UString value) { return (value == null) ? null : Get(value.nIndex); }
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
                MBoxStatic.Assert(99936, _dictStrings.Count == 0);
                MBoxStatic.Assert(99935, _dictStringsRev.Count == 0);
            }

            ++_refCount;
        }

        static internal void DropRef()
        {
            --_refCount;
            MBoxStatic.Assert(99934, _refCount >= 0);

            if (_refCount == 0)
            {
                _dictStrings.Clear();
                _dictStringsRev.Clear();
                _acStrings = null;
            }
        }

        static internal void GenerationEnded()
        {
            MBoxStatic.Assert(99922, _indexGenerator == _dictStrings.Count);
            _acStrings = new string[_dictStrings.Count];
            
            foreach (var kvp in _dictStrings)
            {
                MBoxStatic.Assert(99917, kvp.Key != null);
                _acStrings[kvp.Value] = kvp.Key;
            }

            _dictStrings.Clear();
            _dictStringsRev.Clear();
            _bGenerating = false;
        }

        static internal int Set(string str)
        {
            int nValue = 0;

            if (null != _acStrings)
            {
                lock (_acStrings)
                lock (_dictStrings)
                lock (_dictStringsRev)
                {
                    if (null != _acStrings)     // another thread set it up
                    {
                        MBoxStatic.Assert(99921, _bGenerating == false);
                        MBoxStatic.Assert(99920, _dictStrings.Count == 0);
                        MBoxStatic.Assert(99919, _dictStringsRev.Count == 0);
                        MBoxStatic.Assert(99918, _indexGenerator == _acStrings.Length);

                        for (int nIx = 0; nIx < _acStrings.Length; ++nIx)
                        {
                            _dictStrings[_acStrings[nIx]] = nIx;
                            _dictStringsRev[nIx] = _acStrings[nIx];
                        }

                        _bGenerating = true;
                    }

                    _acStrings = null;
                }
            }

            if (false == _dictStrings.TryGetValue(str, out nValue))
            {
                _dictStrings[str] = _indexGenerator;
                _dictStringsRev[_indexGenerator] = str;
                return _indexGenerator++;                   // note that auto post-increment is compatible with return
            }

            return nValue;
        }

        static internal string Get(int nIndex)
        {
            return _bGenerating ? GetB(nIndex) : GetA(nIndex);
        }

        static string GetA(int nIndex)
        {
            return _acStrings[nIndex];
        }

        static string GetB(int nIndex)
        {
            string strValue = null;

            if (false == _dictStringsRev.TryGetValue(nIndex, out strValue))
            {
                MBoxStatic.Assert(99933, false);
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
        static string[] _acStrings = null;
    }
}
