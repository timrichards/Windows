using System;
using System.Collections.Generic;

namespace DoubleFile
{
    class TabledStringGenerated : TabledStringBase
    {
        internal
            TabledStringGenerated(TabledStringBase t_)
        {
            var t = t_.As<TabledStringGenerating>();

            if (null == t)
            {
                Util.Assert(99915, false);
                return;
            }

            var nCount = t.DictSortedStrings.Count;

            Util.Assert(99922, t.IndexGenerator == nCount);
            _strings = new string[nCount];
            _sort = new int[nCount];

            var nIx = 0;

            foreach (var kvp in t.DictSortedStrings)
            {
                _strings[nIx] = kvp.Key;
                _sort[kvp.Value] = nIx++;
            }
        }

        internal override int
            IndexOf(string str)
        {
            var nMin = 0;
            var nMax = _strings.Length;

            for (;;)
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
                        throw new ArgumentException(IndexOf_Exception);
                    }
                }

                var nIx = nMin + nShift;

                switch (str.LocalCompare(_strings[nIx]))
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

        internal override int
            CompareTo(int nIx, int thatIx) => Math.Sign(_sort[nIx] - _sort[thatIx]);
        internal override string
            Get(int nIndex) => _strings[_sort[nIndex]];

        internal IReadOnlyList<string>
            Strings => _strings;
        string[] _strings;

        int[] _sort;

        internal const string
           IndexOf_Exception = "99788";
    }
}
