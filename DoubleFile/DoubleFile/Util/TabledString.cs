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
    class TabledString<T> : IComparable
        where T : TypedArrayBase, new()
    {
        static public implicit operator TabledString<T>(string value) { return (null == value) ? null : new TabledString<T> { nIndex = Set(value) }; }
        static public implicit operator string(TabledString<T> value) { return (null == value) ? null : Get(value.nIndex); }
        public int CompareTo(object obj) { return ("" + Get(nIndex)).CompareTo(Get(((TabledString<T>)obj).nIndex)); }

        internal bool Contains(TabledString<T> ustr) { return Get(nIndex).Contains(Get(ustr.nIndex)); }
        internal bool Contains(char ch) { return Get(nIndex).Contains(ch); }
        internal bool EndsWith(string str) { return Get(nIndex).EndsWith(str); }
        internal string[] Split(char ch) { return Get(nIndex).Split(ch); }
        internal string[] Split(string[] arrStr, StringSplitOptions opts) { return Get(nIndex).Split(arrStr, opts); }
        internal bool StartsWith(string str) { return Get(nIndex).StartsWith(str); }
        internal TabledString<T> ToLower() { return Get(nIndex).ToLower(); }
        static internal TabledString<T> Empty { get { return ""; } }

        static internal void Reinitialize()
        {
            var t = TypedArrayBase.tA[new T().Type];
            var nRefCount = t?.RefCount ?? 0;

            TypedArrayBase.tA[new T().Type] = 
                new TabledStringStatics()
            {
                RefCount = nRefCount,
                Generating = true,
            };
        }

        static internal void AddRef()
        {
            if (null == TypedArrayBase.tA[new T().Type])
                Reinitialize();

            var t = TypedArrayBase.tA[new T().Type];

            ++t.RefCount;
        }

        static internal void DropRef()
        {
            var t = TypedArrayBase.tA[new T().Type];

            Util.Assert(99934, 0 < t.RefCount);
            --t.RefCount;

            if (0 >= t.RefCount)
                TypedArrayBase.tA[new T().Type] = null;
        }

        static internal void GenerationStarting()
        {
            var t = TypedArrayBase.tA[new T().Type];
            var nThreads = Statics.LVprojectVM.CanLoadCount;

            t.DictPathParts = new ConcurrentDictionary<string, PathBuilder>(nThreads, 16384);

            if (null == t.Strings)
            {
                Util.Assert(99915, t.Generating);
                t.DictStrings = new ConcurrentDictionary<string, int>(nThreads, 16384);
                t.DictStringsRev = new ConcurrentDictionary<int, string>(nThreads, 16384);
                return;
            }

            Util.Assert(99921, false == t.Generating);
            Util.Assert(99920, null == t.DictStrings);
            Util.Assert(99919, null == t.DictStringsRev);
            Util.Assert(99918, t.IndexGenerator == t.Strings.Length);
            Util.Assert(99916, 1 < t.RefCount);

            t.DictStrings = new ConcurrentDictionary<string, int>(nThreads, t.Strings.Length);
            t.DictStringsRev = new ConcurrentDictionary<int, string>(nThreads, t.Strings.Length);

            for (var nIx = 0; nIx < t.Strings.Length; ++nIx)
            {
                var strA = t.Strings[nIx];
                         
                t.DictStrings[strA] = nIx;
                t.DictStringsRev[nIx] = strA;
            }

            t.Generating = true;
            t.Strings = null;
        }

        static internal void GenerationEnded()
        {
            if ((Application.Current?.Dispatcher.HasShutdownStarted ?? true))
                return;

            var t = TypedArrayBase.tA[new T().Type];
            var nCount = t.DictStrings.Count;

            t.DictPathParts = null;
            Util.Assert(99922, t.IndexGenerator == nCount);

            var sortedStrings = new SortedDictionary<string, int>(t.DictStrings);

            t.DictStrings = null;
            t.DictStringsRev = null;
            t.Strings = new string[nCount];
            t.Sort = new int[nCount];

            var nIx = 0;

            foreach (var kvp in sortedStrings)
            {
                t.Strings[nIx] = kvp.Key;
                t.Sort[kvp.Value] = nIx++;
            }

            t.Generating = false;
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
            var t = TypedArrayBase.tA[new T().Type];

            if (null == t)
                return 0;

            if (false == t.Generating)
                Util.Assert(99917, false);

            lock (t.DictStrings)
            lock (t.DictStringsRev)
            return t.DictStrings.GetOrAdd(str, x =>
            {
                var nIx = Interlocked.Increment(ref t.IndexGenerator) - 1;

                t.DictStringsRev[nIx] = str;
                return nIx;
            });
        }

        static string Get(int nIndex)
        {
            var t = TypedArrayBase.tA[new T().Type];

            if (null == t)
                return null;

            return
                (t.Generating)
                ? t.DictStringsRev[nIndex]
                : t.Strings[t.Sort[nIndex]];
        }

        int nIndex = -1;
    }

    class PathBuilder : IComparable
    {
        public int CompareTo(object obj)
        {
            var that = (PathBuilder)obj;
            
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
            var t = TypedArrayBase.tA[new Tabled_Files().Type];

            try
            {
                if (null == t)
                    throw new NullReferenceException();

                Util.Assert(99985, t.Generating);

                lock (t.DictPathParts)
                {
                    return
                        t.DictPathParts
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
            var t = TypedArrayBase.tA[new Tabled_Folders().Type];
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
            var t = TypedArrayBase.tA[new Tabled_Folders().Type];
            var sbRet = new StringBuilder();

            foreach (var nIx in PathParts)
            {
                if (-1 != nIx)
                    sbRet.Append(t.Strings[nIx]);

                sbRet.Append('\\');
            }

            return ("" + sbRet).TrimEnd('\\');
        }

        static protected int FindString(string str)
        {
            var t = TypedArrayBase.tA[new Tabled_Folders().Type];
            var nMin = 0;
            var nMax = t.Strings.Length;

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

                switch (str.CompareTo(t.Strings[nIx]))
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
