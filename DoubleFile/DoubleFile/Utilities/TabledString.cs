﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DoubleFile
{
    abstract class TypedArrayBase
    {
        internal abstract int Type { get; }
        internal static TabledStringStatics[] tA = new TabledStringStatics[2];
    }
    class Tabled_Folders : TypedArrayBase { internal override int Type { get { return 0; } } }
    class Tabled_Files : TypedArrayBase { internal override int Type { get { return 1; } } }

    class TabledStringStatics
    {
        internal Func<int, string> Get;
        internal Func<string, int> Set;

        internal int
            RefCount { get; set; }
        internal bool
            Generating { get; set; }

        internal int
            IndexGenerator;
        internal ConcurrentDictionary<string, int>
            DictStrings { get; set; }
        internal ConcurrentDictionary<int, string>
            DictStringsRev { get; set; }

        internal string[]
            Strings { get; set; }
        internal int[]
            Sort { get; set; }

        internal int
            PathIxGenerator;
        internal ConcurrentDictionary<string, PathBuilderBase>
            DictPathParts { get; set; }
        internal ConcurrentDictionary<PathBuilderBase, int>
            DictPathInts { get; set; }
        internal ConcurrentDictionary<int, PathBuilderBase>
            DictPathRev { get; set; }

        internal PathBuilderBase[]
            Paths { get; set; }
        internal int[]
            PathSort { get; set; }
    }

    // can't be struct because of null
    class TabledString<T> : IComparable
        where T : TypedArrayBase, new()
    {
        public static implicit operator TabledString<T>(string value) { return (null == value) ? null : new TabledString<T> { nIndex = Set(value) }; }
        public static implicit operator string(TabledString<T> value) { return (null == value) ? null : Get(value.nIndex); }
        public int CompareTo(object obj) { return (Get(nIndex) + "").CompareTo(Get(((TabledString<T>)obj).nIndex)); }

        internal bool Contains(TabledString<T> ustr) { return Get(nIndex).Contains(Get(ustr.nIndex)); }
        internal bool Contains(char ch) { return Get(nIndex).Contains(ch); }
        internal bool EndsWith(string str) { return Get(nIndex).EndsWith(str); }
        internal string[] Split(char ch) { return Get(nIndex).Split(ch); }
        internal string[] Split(string[] arrStr, StringSplitOptions opts) { return Get(nIndex).Split(arrStr, opts); }
        internal bool StartsWith(string str) { return Get(nIndex).StartsWith(str); }
        internal TabledString<T> ToLower() { return Get(nIndex).ToLower(); }
        static internal TabledString<T> Empty { get { return string.Empty; } }

        static internal void Reinitialize()
        {
            var t = TypedArrayBase.tA[new T().Type];
            var nRefCount = 0;

            if (null != t)
                nRefCount = t.RefCount;

            TypedArrayBase.tA[new T().Type] = 
                new TabledStringStatics()
            {
                RefCount = nRefCount,
                Get = Get,
                Set = SetA,
                Generating = true,
                PathIxGenerator = -1,
                DictStrings = new ConcurrentDictionary<string, int>(MainWindow.static_MainWindow.LVprojectVM.Count, 16384),
                DictStringsRev = new ConcurrentDictionary<int, string>(MainWindow.static_MainWindow.LVprojectVM.Count, 16384),
                DictPathParts = new ConcurrentDictionary<string, PathBuilderBase>(MainWindow.static_MainWindow.LVprojectVM.Count, 16384),
                DictPathInts = new ConcurrentDictionary<PathBuilderBase, int>(MainWindow.static_MainWindow.LVprojectVM.Count, 16384),
                DictPathRev = new ConcurrentDictionary<int, PathBuilderBase>(MainWindow.static_MainWindow.LVprojectVM.Count, 16384)
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

            MBoxStatic.Assert(99934, 0 < t.RefCount);
            --t.RefCount;

            if (0 <= t.RefCount)
                TypedArrayBase.tA[new T().Type] = null;
        }

        static internal void GenerationStarting()
        {
            var t = TypedArrayBase.tA[new T().Type];

            if (null == t.Strings)
            {
                MBoxStatic.Assert(99915, t.Generating);
                return;
            }

            lock (t.Strings)               // this thread stuff shouldn't be necessary
            {
                if (null != t.Strings)     // another thread set it up
                {
                    MBoxStatic.Assert(99921, false == t.Generating);
                    MBoxStatic.Assert(99920, null == t.DictStrings);
                    MBoxStatic.Assert(99919, null == t.DictStringsRev);
                    MBoxStatic.Assert(99918, t.IndexGenerator == t.Strings.Length);
                    MBoxStatic.Assert(99916, 1 < t.RefCount);

                    t.DictStrings = new ConcurrentDictionary<string, int>(MainWindow.static_MainWindow.LVprojectVM.Count, t.Strings.Length);
                    t.DictStringsRev = new ConcurrentDictionary<int, string>(MainWindow.static_MainWindow.LVprojectVM.Count, t.Strings.Length);

                    for (var nIx = 0; nIx < t.Strings.Length; ++nIx)
                    {
                        var strA = t.Strings[nIx];
                         
                        t.DictStrings[strA] = nIx;
                        t.DictStringsRev[nIx] = strA;
                    }

                    t.DictPathParts = new ConcurrentDictionary<string, PathBuilderBase>(MainWindow.static_MainWindow.LVprojectVM.Count, t.Paths.Length);
                    t.DictPathInts = new ConcurrentDictionary<PathBuilderBase, int>(MainWindow.static_MainWindow.LVprojectVM.Count, t.Paths.Length);
                    t.DictPathRev = new ConcurrentDictionary<int, PathBuilderBase>(MainWindow.static_MainWindow.LVprojectVM.Count, t.Paths.Length);

                    for (var nIx = 0; nIx < t.Paths.Length; ++nIx)
                    {
                        var path = t.Paths[nIx];
                        var nIxPath = -nIx - 1;

                        t.DictPathParts[path.ToString()] = path;
                        t.DictPathInts[path] = nIxPath;
                        t.DictPathRev[nIxPath] = path;
                    }

                    t.Generating = true;
                }
            }

            t.Strings = null;
        }

        static internal void GenerationEnded()
        {
            var t = TypedArrayBase.tA[new T().Type];

            MBoxStatic.Assert(99922, t.IndexGenerator == t.DictStrings.Count);

            {
                var sortedStrings = new SortedDictionary<string, int>(t.DictStrings);
                var nIx = 0;

                t.DictStrings = null;
                t.DictStringsRev = null;
                t.Strings = new string[sortedStrings.Count];
                t.Sort = new int[sortedStrings.Count];

                foreach (var kvp in sortedStrings)
                {
                    t.Strings[nIx] = kvp.Key;
                    t.Sort[kvp.Value] = nIx++;
                }
            }

            t.Generating = false;

            {
                var sortedPathBuilders = new SortedDictionary<PathBuilderBase, int>(t.DictPathInts);
                var nIx = 0;

                t.DictPathParts = null;
                t.DictPathInts = null;
                t.DictPathRev = null;
                t.Paths = new PathBuilderBase[sortedPathBuilders.Count];
                t.PathSort = new int[sortedPathBuilders.Count];

                foreach (var kvp in sortedPathBuilders)
                {
                    t.Paths[nIx] = kvp.Key;
                    t.PathSort[-kvp.Value - 1] = nIx++;
                }
            }
        }

        static int Set(string str)
        {
            return
                (str.Contains('\\'))
                ? PathBuilder<T>.FactoryCreateOrFind(str).nIndex
                : SetA(str);
        }

        static int SetA(string str)
        {
            var t = TypedArrayBase.tA[new T().Type];

            if (null == t)
                return 0;

            if (false == t.Generating)
            {
                var nRet = FindString(str);

                MBoxStatic.Assert(99917, -1 != nRet);
                return nRet;
            }

            if (null == t)
                return 0;

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

            string strRet = null;

            if (t.Generating)
            {
                if (-1 < nIndex)
                    strRet = t.DictStringsRev[nIndex];
                else
                    strRet = t.DictPathRev[nIndex].ToString();
            }
            else
            {
                if (-1 < nIndex)
                    strRet = t.Strings[t.Sort[nIndex]];
                else
                    strRet = t.Paths[t.PathSort[-nIndex - 1]].ToString();
            }

            return strRet;
        }

        static protected int FindString(string str)
        {
            var t = TypedArrayBase.tA[new T().Type];
            var nMin = 0;
            var nMax = t.Strings.Length;

            for (; ; )
            {
                var nIx = nMin + ((nMax - nMin) >> 1);

                switch (str.CompareTo(t.Strings[nIx]))
                {
                    case -1:
                    {
                        if (nMax == nIx)
                            return -1;

                        nMax = nIx;
                        continue;
                    }

                    case 1:
                    {
                        if (nMin == nIx)
                            return -1;

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

        int nIndex = -1;
    }

    abstract class PathBuilderBase : IComparable
    {
        public int CompareTo(object obj)
        {
            var that = obj as PathBuilderBase;
            
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
        internal int[] PathParts { get; set; }
    }

    class PathBuilder<T> : PathBuilderBase
        where T : TypedArrayBase, new()
    {
        static internal PathBuilder<T> FactoryCreateOrFind(string str, Action Cancel = null)
        {
            var t = TypedArrayBase.tA[new T().Type];

            try
            {
                if (null == t)
                    throw new NullReferenceException();

                MBoxStatic.Assert(99917, t.Generating);

                lock (t.DictPathParts)
                {
                    return
                        (PathBuilder<T>)
                        t.DictPathParts.GetOrAdd(str, x =>
                    {
                        var path = new PathBuilder<T>(str)
                        {
                            nIndex = Interlocked.Decrement(ref t.PathIxGenerator) + 1
                        };

                        t.DictPathInts[path] = path.nIndex;
                        t.DictPathRev[path.nIndex] = path;
                        return path;
                    });
                }
            }
            catch (NullReferenceException)
            {
                if (null != Cancel)
                    Cancel();

                return null;
            }
        }

        PathBuilder(string str)
        {
            var t = TypedArrayBase.tA[new T().Type];
            var lsInts = new List<int>();

            foreach (var s in str.Split('\\'))
            {
                // MBoxStatic.Assert(99880, false == string.IsNullOrWhiteSpace(s));
                // Acceptable: search results dir
                // if (false == string.IsNullOrWhiteSpace(s))
                    lsInts.Add(t.Set(s));
            }

            PathParts = lsInts.ToArray();
        }

        public override string ToString()
        {
            var t = TypedArrayBase.tA[new T().Type];
            var sbRet = new StringBuilder();

            foreach (var nIx in PathParts)
                sbRet.Append(t.Get(nIx)).Append('\\');

            return sbRet.ToString().TrimEnd('\\');
        }
    }
}
