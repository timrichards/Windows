using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;

namespace DoubleFile
{
    abstract class TabledStringStatics
    {
        internal int
            RefCount;
        internal virtual int
            Set(string str_in) { Util.Assert(99920, false); return -1; }
        internal abstract int
            CompareTo(int nIx, int thatIx);
        internal abstract string
            Get(int index);
    }

    class T_Generating : TabledStringStatics
    {
        internal T_Generating(TabledStringStatics t_ = null)
        {
            RefCount = t_?.RefCount ?? 0;

            var nThreads = Statics.LVprojectVM.CanLoadCount;
            var t = t_.As<T_Generated>();

            if (null != t)
            {
                Util.Assert(99916, 1 < RefCount);
                IndexGenerator = t.Strings.Length;
                DictStrings = new ConcurrentDictionary<string, int>(nThreads, t.Strings.Length);
                DictStringsRev = new ConcurrentDictionary<int, string>(nThreads, t.Strings.Length);

                for (var nIx = 0; nIx < t.Strings.Length; ++nIx)
                {
                    var strA = t.Strings[nIx];

                    DictStrings[strA] = nIx;
                    DictStringsRev[nIx] = strA;
                }
            }
            else
            {
                DictStrings = new ConcurrentDictionary<string, int>(nThreads, 16384);
                DictStringsRev = new ConcurrentDictionary<int, string>(nThreads, 16384);
            }
        }

        internal override int Set(string str_in)
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
        int SetA(string str)
        {
            lock (DictStrings)
            lock (DictStringsRev)
            return DictStrings.GetOrAdd(str, x =>
            {
                var nIx = Interlocked.Increment(ref IndexGenerator) - 1;

                DictStringsRev[nIx] = str;
                return nIx;
            });
        }

        internal override int
            CompareTo(int nIx, int thatIx) => Get(nIx).LocalCompare(Get(thatIx));
        internal override string
            Get(int nIndex) => DictStringsRev[nIndex];

        internal int
            IndexGenerator;
        internal ConcurrentDictionary<string, int>
            DictStrings { get; private set; }
        internal ConcurrentDictionary<int, string>
            DictStringsRev { get; private set; }
    }

    class T_Generated : TabledStringStatics
    {
        internal T_Generated(TabledStringStatics t_)
        {
            var t = t_.As<T_Generating>();

            Util.Assert(99915, null != t);
            RefCount = t.RefCount;

            var nCount = t.DictStrings.Count;

            Util.Assert(99922, t.IndexGenerator == nCount);

            var sortedStrings = new SortedDictionary<string, int>();

            try
            {
                sortedStrings = new SortedDictionary<string, int>(t.DictStrings);
            }
            catch (ArgumentException ex)
            {
                var lsDupes = new List<string> { };

                foreach (var kvp in t.DictStrings)
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

            Strings = new string[nCount];
            Sort = new int[nCount];

            var nIx = 0;

            foreach (var kvp in sortedStrings)
            {
                Strings[nIx] = kvp.Key;
                Sort[kvp.Value] = nIx++;
            }
        }

        internal override int
            Set(string str_in) { Util.Assert(99920, false); return -1; }

        internal override int
            CompareTo(int nIx, int thatIx) => Math.Sign(Sort[nIx] - Sort[thatIx]);
        internal override string
            Get(int nIndex) => Strings[Sort[nIndex]];

        internal string[]
            Strings { get; private set; }
        internal int[]
            Sort { get; private set; }
    }

    abstract class TypedArrayBase
    {
        internal abstract int Type { get; }
        static internal TabledStringStatics[] tA = new TabledStringStatics[2];
    }
    class Tabled_Folders : TypedArrayBase { internal override int Type => 0; }
    class Tabled_Files : TypedArrayBase { internal override int Type => 1; }

    // can't be struct because of null
    class TabledString<T> : IComparable<TabledString<T>>, IComparable
        where T : TypedArrayBase, new()
    {
        static TabledStringStatics
            _t = null;

        static public explicit operator
            TabledString<T>(string value) =>
            string.IsNullOrWhiteSpace(value) ? null : new TabledString<T> { nIndex = _t.Set(value) };

        static public explicit operator
            string(TabledString<T> value) => (null == value) ? null : _t?.Get(value.nIndex);

        public int CompareTo(object that) => CompareTo((TabledString<T>)that);
        public int CompareTo(TabledString<T> that) => _t?.CompareTo(nIndex, that.nIndex) ?? 0;

        static internal bool IsAlive => null != _t;

        static internal void Reinitialize()
        {
            var nRefCount = _t?.RefCount ?? 0;

            _t =
                TypedArrayBase.tA[new T().Type] = 
                new T_Generating { RefCount = nRefCount };
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

            if (0 < _t.RefCount)
                return;

            _t =
                TypedArrayBase.tA[new T().Type] =
                null;
        }

        static internal void GenerationStarting() =>
            _t = TypedArrayBase.tA[new T().Type] = new T_Generating(_t);

        static internal void GenerationEnded()
        {
            if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true))
                return;

            _t =
                TypedArrayBase.tA[new T().Type] =
                new T_Generated(_t);
        }

        int nIndex = -1;
    }

    class PathBuilder : IComparable<PathBuilder>, IComparable
    {
        static TabledStringStatics
            _tFiles = null;
        static T_Generated
            _t = null;
        static ConcurrentDictionary<string, PathBuilder>
            _dictPathParts = null;

        static internal void AddRef()
        {
            TabledString<Tabled_Files>.AddRef();
            _t = TypedArrayBase.tA[new Tabled_Folders().Type].As<T_Generated>();

            if (0 < (_tFiles?.RefCount ?? 0))
                return;

            _tFiles = TypedArrayBase.tA[new Tabled_Files().Type];
            _dictPathParts = new ConcurrentDictionary<string, PathBuilder>(Statics.LVprojectVM.CanLoadCount, 16384);
        }

        static internal void DropRef()
        {
            TabledString<Tabled_Files>.DropRef();

            if (0 < _tFiles.RefCount)
                return;

            _tFiles = null;
            _t = null;
            _dictPathParts = null;
        }

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
                if (null == _tFiles)
                    throw new NullReferenceException();

                Util.Assert(99985, _tFiles is T_Generating);

                return
                    _dictPathParts
                    .GetOrAdd(strDir, x => new PathBuilder(strDir));
            }
            catch (NullReferenceException)
            {
                Cancel?.Invoke();
                return null;
            }
        }

        PathBuilder(string strDir)
        {
            var lsInts = new List<int> { };

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
