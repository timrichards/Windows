using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace DoubleFile
{
    class PathBuilder : IComparable<PathBuilder>, IComparable
    {
        static internal void
            AddRef()
        {
            _t = TabledStringTypesBase.Types[new TabledStringType_Folders().Type].As<TabledStringGenerated>();

            if (0 < _nRefCount++)
                return;

            _dictPathParts = new ConcurrentDictionary<string, PathBuilder>(Statics.LVprojectVM.CanLoadCount, 16384);
        }

        static internal void
            DropRef()
        {
            if (0 < --_nRefCount)
                return;

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

        static internal PathBuilder
            FactoryCreateOrFind(string strDir, Action Cancel = null)
        {
            try
            {
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

        public override string
            ToString()
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

        static protected int
            FindString(string str)
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

        static TabledStringGenerated
            _t = null;
        static ConcurrentDictionary<string, PathBuilder>
            _dictPathParts = null;
        static int
            _nRefCount = 0;

        internal int[]
            PathParts { get; private set; }
    }
}
