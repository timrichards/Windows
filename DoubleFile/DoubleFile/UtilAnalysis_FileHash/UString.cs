using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

namespace DoubleFile
{
    // can't be struct because of null
    class UString : IComparable<string>
    {
        public static implicit operator UString(string value) { return (null == value) ? null : new UString { nIndex = Set(value) }; }
        public static implicit operator string(UString value) { return (null == value) ? null : Get(value.nIndex); }
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
                MBoxStatic.Assert(99936, null == _dictStrings);
                MBoxStatic.Assert(99935, null == _dictStringsRev);
                MBoxStatic.Assert(99912, null == _acStrings);
                MBoxStatic.Assert(99911, _bGenerating);
                MBoxStatic.Assert(99914, 0 == _indexGenerator);
                _dictStrings = new ConcurrentDictionary<string, int>(GlobalData.static_MainWindow.LVprojectVM.Count, 16384);
                _dictStringsRev = new ConcurrentDictionary<int, string>(GlobalData.static_MainWindow.LVprojectVM.Count, 16384);
                _acStrings = null;
                _bGenerating = true;
                _indexGenerator = 0;
            }

            ++_refCount;
        }

        static internal void DropRef()
        {
            MBoxStatic.Assert(99934, 0 < _refCount);
            --_refCount;

            if (0 == _refCount)
            {
                _dictStrings = null;
                _dictStringsRev = null;
                _acStrings = null;
                _bGenerating = true;
                _indexGenerator = 0;
            }
        }

        static internal void GenerationStarting()
        {
            if (null == _acStrings)
            {
                MBoxStatic.Assert(99915, _bGenerating);
                return;
            }

            lock (_acStrings)               // this thread stuff shouldn't be necessary
            {
                if (null != _acStrings)     // another thread set it up
                {
                    MBoxStatic.Assert(99921, false == _bGenerating);
                    MBoxStatic.Assert(99920, null == _dictStrings);
                    MBoxStatic.Assert(99919, null == _dictStringsRev);
                    MBoxStatic.Assert(99918, _indexGenerator == _acStrings.Length);
                    MBoxStatic.Assert(99916, 1 < _refCount);

                    _dictStrings = new ConcurrentDictionary<string, int>(GlobalData.static_MainWindow.LVprojectVM.Count, _acStrings.Length);
                    _dictStringsRev = new ConcurrentDictionary<int, string>(GlobalData.static_MainWindow.LVprojectVM.Count, _acStrings.Length);

                    for (int nIx = 0; nIx < _acStrings.Length; ++nIx)
                    {
                        var strA = _acStrings[nIx];

                        _dictStrings[strA] = nIx;
                        _dictStringsRev[nIx] = strA;
                    }

                    _bGenerating = true;
                }
            }

            _acStrings = null;
        }

        static internal void GenerationEnded()
        {
            MBoxStatic.Assert(99922, _indexGenerator == _dictStrings.Count);
            _acStrings = new string[_dictStrings.Count];

            foreach (var kvp in _dictStrings)
            {
                _acStrings[kvp.Value] = kvp.Key;
            }

            _dictStrings = null;
            _dictStringsRev = null;
            _bGenerating = false;
        }

        static internal int Set(string str)
        {
            if (false == _bGenerating)
            {
                MBoxStatic.Assert(99917, false);
                return -1;
            }

            return _dictStrings.GetOrAdd(str, (q) =>
            {
                var nIx = Interlocked.Increment(ref _indexGenerator) - 1;

                _dictStringsRev[nIx] = q;

                return nIx;
            });
        }

        static string Get(int nIndex)
        {
            return _bGenerating ? _dictStringsRev[nIndex] : _acStrings[nIndex];
        }

        static ConcurrentDictionary<string, int>
            _dictStrings = null;
        static ConcurrentDictionary<int, string>
            _dictStringsRev = null;
        static int
            _refCount = 0;
        static int
            _indexGenerator = 0;
        static bool
            _bGenerating = true;
        static string[]
            _acStrings = null;

        int nIndex = -1;
    }
}
