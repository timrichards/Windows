using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

namespace DoubleFile
{
    abstract class TypedArrayBase
    {
        internal abstract int Type { get; }
        static internal TabledStringStatics[] tA = new TabledStringStatics[2];
    }
    class Tabled_Folders : TypedArrayBase { internal override int Type => 0; }
    class Tabled_Files : TypedArrayBase { internal override int Type => 1; }

    class TabledStringStatics
    {
        internal int
            RefCount;
        internal bool
            Generating;

        internal int
            IndexGenerator;
        internal ConcurrentDictionary<string, int>
            DictStrings;
        internal ConcurrentDictionary<int, string>
            DictStringsRev;

        internal string[]
            Strings;
        internal int[]
            Sort;

        internal ConcurrentDictionary<string, PathBuilder>
            DictPathParts;
    }

    // can't be struct because of null
    class TabledString<T> : IComparable<TabledString<T>>, IComparable
        where T : TypedArrayBase, new()
    {
        static TabledStringStatics
            _t = null;

        static public explicit operator
            TabledString<T>(string value) =>
            string.IsNullOrWhiteSpace(value) ? null : new TabledString<T> { nIndex = Set(value) };

        static public explicit operator
            string(TabledString<T> value) => (null == value) ? null : Get(value.nIndex);

        public int CompareTo(object that) => CompareTo((TabledString<T>)that);
        public int CompareTo(TabledString<T> that) => ("" + Get(nIndex)).CompareTo(Get(that.nIndex));
        //public int CompareTo(TabledString<T> that)
        //{
        //    if (null == _t)
        //        return -1;

        //    return
        //        (_t.Generating)
        //     //   ? ((string)this).LocalCompare((string)that)
        //        ? ("" + Get(nIndex)).CompareTo(Get(that.nIndex))
        //        : _t.Sort[nIndex] - _t.Sort[that.nIndex];
        //}

        static internal void Reinitialize()
        {
            var nRefCount = _t?.RefCount ?? 0;

            _t = TypedArrayBase.tA[new T().Type] = 
                new TabledStringStatics()
            {
                RefCount = nRefCount,
                Generating = true,
            };
        }

        static internal void AddRef()
        {
            if (null == _t)
                Reinitialize();

            ++_t.RefCount;
        }

        static internal void DropRef()
        {
            Util.Assert(99934, 0 < _t.RefCount);
            --_t.RefCount;

            if (0 >= _t.RefCount)
                _t = TypedArrayBase.tA[new T().Type] = null;
        }

        static internal void GenerationStarting()
        {
            var nThreads = Statics.LVprojectVM.CanLoadCount;

            _t.DictPathParts = new ConcurrentDictionary<string, PathBuilder>(nThreads, 16384);

            if (null == _t.Strings)
            {
                Util.Assert(99915, _t.Generating);
                _t.DictStrings = new ConcurrentDictionary<string, int>(nThreads, 16384);
                _t.DictStringsRev = new ConcurrentDictionary<int, string>(nThreads, 16384);
                return;
            }

            Util.Assert(99921, false == _t.Generating);
            Util.Assert(99920, null == _t.DictStrings);
            Util.Assert(99919, null == _t.DictStringsRev);
            Util.Assert(99918, _t.IndexGenerator == _t.Strings.Length);
            Util.Assert(99916, 1 < _t.RefCount);

            _t.DictStrings = new ConcurrentDictionary<string, int>(nThreads, _t.Strings.Length);
            _t.DictStringsRev = new ConcurrentDictionary<int, string>(nThreads, _t.Strings.Length);

            for (var nIx = 0; nIx < _t.Strings.Length; ++nIx)
            {
                var strA = _t.Strings[nIx];
                         
                _t.DictStrings[strA] = nIx;
                _t.DictStringsRev[nIx] = strA;
            }

            _t.Generating = true;
            _t.Strings = null;
        }

        static internal void GenerationEnded()
        {
            if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true))
                return;

            var nCount = _t.DictStrings.Count;

            _t.DictPathParts = null;
            Util.Assert(99922, _t.IndexGenerator == nCount);

            SortedDictionary<string, int> sortedStrings = new SortedDictionary<string, int>();

            try
            {
                sortedStrings = new SortedDictionary<string, int>(_t.DictStrings);
            }
            catch (ArgumentException ex)
            {
                var lsDupes = new List<string> { };

                foreach (var kvp in _t.DictStrings)
                {
                    try
                    {
                        sortedStrings.Add(kvp.Key, kvp.Value);
                    }
                    catch (ArgumentException e)
                    {
                        lsDupes.Add(kvp.Key + " " + e.GetBaseException().Message);
                    }
                }

                Util.Assert(99661, false, "ArgumentException in TabledString creating SortedDictionary\n" +
                    ex.GetBaseException().Message + "\n" +
                    string.Join("\n", lsDupes.Select(s => s)));
            }

            _t.DictStrings = null;
            _t.DictStringsRev = null;
            _t.Strings = new string[nCount];
            _t.Sort = new int[nCount];

            var nIx = 0;

            foreach (var kvp in sortedStrings)
            {
                _t.Strings[nIx] = kvp.Key;
                _t.Sort[kvp.Value] = nIx++;
            }

            _t.Generating = false;
        }

        static int Set(string str_in)
        {
            var split = str_in.Split('\\');

            if (1 < split.Length)
            {
                foreach (var str in str_in.Split('\\'))
                {
                    if (0 < str.Length)
                        SetA(str);
                }
            }

            return SetA(str_in);
        }

        static int SetA(string str)
        {
            if (null == _t)
                return 0;

            if (false == _t.Generating)
                Util.Assert(99917, false);

            lock (_t.DictStrings)
            lock (_t.DictStringsRev)
            return _t.DictStrings.GetOrAdd(str, x =>
            {
                var nIx = Interlocked.Increment(ref _t.IndexGenerator) - 1;

                _t.DictStringsRev[nIx] = str;
                return nIx;
            });
        }

        static string Get(int nIndex)
        {
            if (null == _t)
                return null;

            return
                (_t.Generating)
                ? _t.DictStringsRev[nIndex]
                : _t.Strings[_t.Sort[nIndex]];
        }

        int nIndex = -1;
    }

    class PathBuilder : IComparable<PathBuilder>, IComparable
    {
        static TabledStringStatics
            _t = null;

        public int CompareTo(object that) => CompareTo((PathBuilder)that);
        public int CompareTo(PathBuilder that)
        {            
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

        static internal PathBuilder FactoryCreateOrFind(string strDir, Action Cancel = null)
        {
            try
            {
                if (null == _t)
                    throw new NullReferenceException();

                Util.Assert(99985, _t.Generating);

                lock (_t.DictPathParts)
                {
                    return
                        _t.DictPathParts
                        .GetOrAdd(strDir, x => new PathBuilder(strDir));
                }
            }
            catch (NullReferenceException)
            {
                Cancel?.Invoke();
                return null;
            }
        }

        PathBuilder(string strDir)
        {
            var lsInts = new List<int>();

            foreach (var s in strDir.Split('\\'))
            {
                // Util.Assert(99880, false == string.IsNullOrWhiteSpace(s));
                // Acceptable: search results dir
                // if (false == string.IsNullOrWhiteSpace(s))
                if (string.IsNullOrWhiteSpace(s))
                    lsInts.Add(-1);
                else
                    lsInts.Add(FindString(s));
            }

            PathParts = lsInts.ToArray();
        }

        public override string ToString()
        {
            var sbRet = new StringBuilder();

            foreach (var nIx in PathParts)
            {
                if (-1 != nIx)
                    sbRet.Append(_t.Strings[nIx]);

                sbRet.Append('\\');
            }

            return ("" + sbRet).TrimEnd('\\');
        }

        static protected int FindString(string str)
        {
            var nMin = 0;
            var nMax = _t.Strings.Length;

            for (; ; )
            {
                var nShift = (nMax - nMin) >> 1;

                if (0 == nShift)
                {
                    if ((1 == nMax) && (0 == nMin))     // zeroth
                    {
                        nShift = 0;
                    }
                    else
                    {
                        Util.Assert(99788, false);
                        return -2;
                    }
                }

                var nIx = nMin + nShift;

                switch (str.LocalCompare(_t.Strings[nIx]))
                {
                    case -1:
                    {
                        nMax = nIx;
                        continue;
                    }

                    case 1:
                    {
                        nMin = nIx;
                        continue;
                    }

                    case 0:
                    {
                        return nIx;
                    }
                }
            }
        }

        internal int[] PathParts;
    }
}
