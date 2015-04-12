using System;
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
    class TypedArray0 : TypedArrayBase { internal override int Type { get { return 0; } } }
    class TypedArray1 : TypedArrayBase { internal override int Type { get { return 1; } } }

    class TabledStringStatics
    {
        internal Func<int, string> Get;
        internal Func<string, int> Set;

        internal ConcurrentDictionary<string, int>
            DictStrings { get; set; }
        internal ConcurrentDictionary<int, string>
            DictStringsRev { get; set; }
        internal int
            RefCount { get; set; }
        internal int
            IndexGenerator;
        internal bool
            Generating { get; set; }
        internal string[]
            Strings { get; set; }
        internal int[]
            Sort { get; set; }
        internal ConcurrentDictionary<string, PathBuilderBase>
            DictPathParts { get; set; }
        internal ConcurrentDictionary<PathBuilderBase, int>
            DictPathInts { get; set; }
        internal ConcurrentDictionary<int, PathBuilderBase>
            DictPathRev { get; set; }
        internal int
            PathIxGenerator;
        internal PathBuilderBase[]
            Paths { get; set; }
        internal int[]
            PathSort { get; set; }
    }

    // can't be struct because of null
    class TabledString<T> : IComparable<string>
        where T : TypedArrayBase, new()
    {
        static TabledStringStatics t = null;

        public static implicit operator TabledString<T>(string value) { return (null == value) ? null : new TabledString<T> { nIndex = Set(value) }; }
        public static implicit operator string(TabledString<T> value) { return (null == value) ? null : Get(value.nIndex); }
        int IComparable<string>.CompareTo(string other) { return Get(nIndex).CompareTo(other); }

        internal bool Contains(TabledString<T> ustr) { return Get(nIndex).Contains(Get(ustr.nIndex)); }
        internal bool Contains(char ch) { return Get(nIndex).Contains(ch); }
        internal bool EndsWith(string str) { return Get(nIndex).EndsWith(str); }
        internal string[] Split(char ch) { return Get(nIndex).Split(ch); }
        internal string[] Split(string[] arrStr, StringSplitOptions opts) { return Get(nIndex).Split(arrStr, opts); }
        internal bool StartsWith(string str) { return Get(nIndex).StartsWith(str); }
        internal TabledString<T> ToLower() { return Get(nIndex).ToLower(); }

        static internal TabledString<T> Empty { get { return string.Empty; } }

        static TabledString()
        {
            t = TypedArrayBase.tA[new T().Type] = new TabledStringStatics()
            {
                Get = Get,
                Set = Set,
                Generating = true,
                PathIxGenerator = -1
            };
            UtilProject.WriteLine("Type " + new T().Type);
        }

        static internal void AddRef()
        {
            if (0 == t.RefCount)
            {
                MBoxStatic.Assert(99936, null == t.DictStrings);
                MBoxStatic.Assert(99935, null == t.DictStringsRev);
                MBoxStatic.Assert(99912, null == t.Strings);
                MBoxStatic.Assert(99911, t.Generating);
                MBoxStatic.Assert(99914, 0 == t.IndexGenerator);
                t.DictStrings = new ConcurrentDictionary<string, int>(MainWindow.static_MainWindow.LVprojectVM.Count, 16384);
                t.DictStringsRev = new ConcurrentDictionary<int, string>(MainWindow.static_MainWindow.LVprojectVM.Count, 16384);
                t.DictPathParts = new ConcurrentDictionary<string, PathBuilderBase>(MainWindow.static_MainWindow.LVprojectVM.Count, 16384);
                t.DictPathInts = new ConcurrentDictionary<PathBuilderBase, int>(MainWindow.static_MainWindow.LVprojectVM.Count, 16384);
                t.DictPathRev = new ConcurrentDictionary<int, PathBuilderBase>(MainWindow.static_MainWindow.LVprojectVM.Count, 16384);
                t.Strings = null;
                t.Generating = true;
                t.IndexGenerator = 0;
            }

            ++t.RefCount;
        }

        static internal void DropRef()
        {
            MBoxStatic.Assert(99934, 0 < t.RefCount);
            --t.RefCount;

            if (0 == t.RefCount)
            {
                t.DictStrings = null;
                t.DictStringsRev = null;
                t.Strings = null;
                t.Generating = true;
                t.IndexGenerator = 0;
            }
        }

        static internal void GenerationStarting()
        {
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

            t.Generating = false;
        }

        static int Set(string str)
        {
            if (str.Contains('\\'))
            {
                if (false == t.Generating)
                {
                    MBoxStatic.Assert(99917, false);
                    return -1;
                }

                lock (t.DictPathParts)
                {
                    return t.DictPathParts.GetOrAdd(str, x =>
                    {
                        var path = PathBuilder<T>.FactoryCreate(str);

                        path.nIndex = Interlocked.Decrement(ref t.PathIxGenerator) + 1;
                        t.DictPathInts[path] = path.nIndex;
                        t.DictPathRev[path.nIndex] = path;
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
            if (false == t.Generating)
            {
                var nRet = FindString(str);

                if (-1 == nRet)
                    MBoxStatic.Assert(99917, false);

                return nRet;
            }

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
            if (0 == t.RefCount)
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
        static TabledStringStatics t = TypedArrayBase.tA[new T().Type];

        internal static PathBuilder<T> FactoryCreate(string str)
        {
            var aStr = str.Split('\\').ToArray();

            if (1 == aStr.Length)
                return null;

            var lsInts = new List<int>();

            foreach (var s in aStr)
                lsInts.Add(t.Set(s));

            return new PathBuilder<T> { PathParts = lsInts.ToArray() };
        }

        public override string ToString()
        {
            var sbRet = new StringBuilder();

            foreach (var nIx in PathParts)
                sbRet.Append(t.Get(nIx)).Append('\\');

            return sbRet.ToString().TrimEnd('\\');
        }
    }
}
