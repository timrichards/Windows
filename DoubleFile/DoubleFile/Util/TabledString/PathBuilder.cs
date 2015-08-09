using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Linq;

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

            _dictPathParts =
                new ConcurrentDictionary<string, PathBuilder>(Statics.WithLVprojectVM(p => p?.CanLoadCount ?? 0), 16384);
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
                if ((_pathParts.Length == nIx) ||
                    (that._pathParts.Length == nIx))
                {
                    return Math.Sign(_pathParts.Length - that._pathParts.Length);
                }

                var nRet = _pathParts[nIx].CompareTo(that._pathParts[nIx]);

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
            var bHitBlank = false;

            foreach (var s in strDir.Split('\\'))
            {
                Util.Assert(99880, false == bHitBlank);

                if (string.IsNullOrWhiteSpace(s))
                {
                    // Acceptable: search results dir trailing backslash
                    bHitBlank = true;
                    lsInts.Add(-1);
                }
                else
                {
                    lsInts.Add(_t.IndexOf(s));
                }
            }

            _pathParts = lsInts.ToArray();
        }

        public override string
            ToString()
        {
            var sbRet = new StringBuilder();

            foreach (var nIx in _pathParts)
            {
                if (-1 != nIx)
                    sbRet.Append(_t.Strings[nIx]);

                sbRet.Append('\\');
            }

            return ("" + sbRet).TrimEnd('\\');
        }

        static TabledStringGenerated
            _t = null;
        static ConcurrentDictionary<string, PathBuilder>
            _dictPathParts = null;
        static int
            _nRefCount = 0;

        int[] _pathParts;
    }
}
