using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DoubleFile
{
    // can't be struct because of null
    class TabledString : IComparable<string>
    {
        public static implicit operator TabledString(string value) { return (null == value) ? null : new TabledString { nIndex = Set(value) }; }
        public static implicit operator string(TabledString value) { return (null == value) ? null : Get(value.nIndex); }
        public int CompareTo(string other) { return Get(nIndex).CompareTo(other); }

        internal bool Contains(TabledString ustr) { return Get(nIndex).Contains(Get(ustr.nIndex)); }
        internal bool Contains(char ch) { return Get(nIndex).Contains(ch); }
        internal bool EndsWith(string str) { return Get(nIndex).EndsWith(str); }
        internal string[] Split(char ch) { return Get(nIndex).Split(ch); }
        internal string[] Split(string[] arrStr, StringSplitOptions opts) { return Get(nIndex).Split(arrStr, opts); }
        internal bool StartsWith(string str) { return Get(nIndex).StartsWith(str); }
        internal TabledString ToLower() { return Get(nIndex).ToLower(); }

        static internal TabledString Empty { get { return string.Empty; } }

        static internal void AddRef()
        {
            if (0 ==_refCount)
            {
                MBoxStatic.Assert(99936, null == _dictStrings);
                MBoxStatic.Assert(99935, null == _dictStringsRev);
                MBoxStatic.Assert(99912, null == _acStrings);
                MBoxStatic.Assert(99911, _bGenerating);
                MBoxStatic.Assert(99914, 0 == _indexGenerator);
                _dictStrings = new ConcurrentDictionary<string, int>(MainWindow.static_MainWindow.LVprojectVM.Count, 16384);
                _dictStringsRev = new ConcurrentDictionary<int, string>(MainWindow.static_MainWindow.LVprojectVM.Count, 16384);
                _dictPathParts = new ConcurrentDictionary<string, PathBuilder>(MainWindow.static_MainWindow.LVprojectVM.Count, 16384);
                _dictPathInts = new ConcurrentDictionary<PathBuilder, int>(MainWindow.static_MainWindow.LVprojectVM.Count, 16384);
                _dictPathRev = new ConcurrentDictionary<int, PathBuilder>(MainWindow.static_MainWindow.LVprojectVM.Count, 16384);
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

                    _dictStrings = new ConcurrentDictionary<string, int>(MainWindow.static_MainWindow.LVprojectVM.Count, _acStrings.Length);
                    _dictStringsRev = new ConcurrentDictionary<int, string>(MainWindow.static_MainWindow.LVprojectVM.Count, _acStrings.Length);

                    for (var nIx = 0; nIx < _acStrings.Length; ++nIx)
                    {
                        var strA = _acStrings[nIx];
                         
                        _dictStrings[strA] = nIx;
                        _dictStringsRev[nIx] = strA;
                    }

                    _dictPathParts = new ConcurrentDictionary<string, PathBuilder>(MainWindow.static_MainWindow.LVprojectVM.Count, _acPaths.Length);
                    _dictPathInts = new ConcurrentDictionary<PathBuilder, int>(MainWindow.static_MainWindow.LVprojectVM.Count, _acPaths.Length);
                    _dictPathRev = new ConcurrentDictionary<int, PathBuilder>(MainWindow.static_MainWindow.LVprojectVM.Count, _acPaths.Length);

                    for (var nIx = 0; nIx < _acPaths.Length; ++nIx)
                    {
                        var path = _acPaths[nIx];
                        var nIxPath = -nIx - 1;

                        _dictPathParts[path] = path;
                        _dictPathInts[path] = nIxPath;
                        _dictPathRev[nIxPath] = path;
                    }

                    _bGenerating = true;
                }
            }

            _acStrings = null;
        }

        static internal void GenerationEnded()
        {
            MBoxStatic.Assert(99922, _indexGenerator == _dictStrings.Count);

            {
                var sortedStrings = new SortedDictionary<string, int>(_dictStrings);
                var nIx = 0;

                _dictStrings = null;
                _dictStringsRev = null;
                _acStrings = new string[sortedStrings.Count];
                _acSort = new int[sortedStrings.Count];

                foreach (var kvp in sortedStrings)
                {
                    _acStrings[nIx] = kvp.Key;
                    _acSort[kvp.Value] = nIx++;
                }
            }

            {
                var sortedPathBuilders = new SortedDictionary<PathBuilder, int>(_dictPathInts);
                var nIx = 0;

                _dictPathParts = null;
                _dictPathInts = null;
                _dictPathRev = null;
                _acPaths = new PathBuilder[sortedPathBuilders.Count];
                _acPathSort = new int[sortedPathBuilders.Count];

                foreach (var kvp in sortedPathBuilders)
                {
                    _acPaths[nIx] = kvp.Key;
                    _acPathSort[-kvp.Value - 1] = nIx++;
                }
            }

            _bGenerating = false;
        }

        static internal int Set(string str)
        {
            if (str.Contains('\\'))
            {
                if (false == _bGenerating)
                {
                    MBoxStatic.Assert(99917, false);
                    return -1;
                }

                lock (_dictPathParts)
                {
                    return _dictPathParts.GetOrAdd(str, x =>
                    {
                        var path = PathBuilder.FactoryCreate(str);

                        path.nIndex = Interlocked.Decrement(ref _pathIxGenerator) + 1;
                        _dictPathInts[path] = path.nIndex;
                        _dictPathRev[path.nIndex] = path;
                        return path;
                    }).nIndex;
                }
            }
            else
            {
                return SetA(str);
            }
        }

        static protected int SetA(string str)
        {
            if (false == _bGenerating)
            {
                var nRet = FindString(str);

                if (-1 == nRet)
                    MBoxStatic.Assert(99917, false);

                return nRet;
            }

            lock (_dictStrings)
            lock (_dictStringsRev)
            return _dictStrings.GetOrAdd(str, x =>
            {
                var nIx = Interlocked.Increment(ref _indexGenerator) - 1;

                _dictStringsRev[nIx] = str;
                return nIx;
            });
        }

        static protected string Get(int nIndex)
        {
            if (0 == _refCount)
                return null;

            string sRet = null;

            if (_bGenerating)
            {
                if (-1 < nIndex)
                    sRet = _dictStringsRev[nIndex];
                else
                    sRet = _dictPathRev[nIndex];
            }
            else
            {
                if (-1 < nIndex)
                    sRet = _acStrings[_acSort[nIndex]];
                else
                    sRet = _acPaths[_acPathSort[-nIndex - 1]];
            }

            return sRet;
        }

        static protected int FindString(string str)
        {
            var nMin = 0;
            var nMax = _acStrings.Length;

            for (; ; )
            {
                var nIx = nMin + ((nMax - nMin) >> 1);

                switch (str.CompareTo(_acStrings[nIx]))
                {
                    case -1:
                    {
                        if (nMax == nIx)
                            return -1;

                        nMax = nIx;
                        break;
                    }

                    case 1:
                    {
                        if (nMin == nIx)
                            return -1;

                        nMin = nIx;
                        break;
                    }

                    case 0:
                    {
                        return nIx;
                    }
                }
            }
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
        static int[]
            _acSort = null;

        int nIndex = -1;

        static ConcurrentDictionary<string, PathBuilder>
            _dictPathParts = null;
        static ConcurrentDictionary<PathBuilder, int>
            _dictPathInts = null;
        static ConcurrentDictionary<int, PathBuilder>
            _dictPathRev = null;
        static int
            _pathIxGenerator = -1;
        static PathBuilder[]
            _acPaths = null;
        static int[]
            _acPathSort = null;
    }

    class PathBuilder : TabledString, IComparable
    {
        public static implicit operator string(PathBuilder value) { return (null == value) ? null : value.ToStringA(); }

        internal static PathBuilder FactoryCreate(string str)
        {
            var aStr = str.Split('\\').ToArray();

            if (1 == aStr.Length)
                return null;

            var lsInts = new List<int>();

            foreach (var s in aStr)
                lsInts.Add(SetA(s));

            return new PathBuilder { PathParts = lsInts.ToArray() };
        }

        public override string ToString()
        {
            return ToStringA();
        }

        string ToStringA()
        {
            var sbRet = new StringBuilder();

            foreach (var nIx in PathParts)
                sbRet.Append(Get(nIx)).Append('\\');

            return sbRet.ToString().TrimEnd('\\');
        }

        public int CompareTo(object obj)
        {
            var that = obj as PathBuilder;
            
            for (var nIx = 0; ; ++nIx)
            {
                if ((PathParts.Length == nIx) ||
                    (that.PathParts.Length == nIx))
                {
                    return Math.Sign(PathParts.Length - that.PathParts.Length);
                }

                var nRet = PathParts[nIx].CompareTo(that.PathParts[nIx]);

                if (0 != nRet)
                    return nRet;
            }
        }

        internal int nIndex = -1;
        internal int[] PathParts { get; private set; }
    }
}
