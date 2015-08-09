using System.Collections.Generic;
using System.Threading;
using System.Linq;

namespace DoubleFile
{
    class TabledStringGenerating : TabledStringBase
    {
        internal
            TabledStringGenerating(TabledStringBase t_ = null)
        {
            var t = t_.As<TabledStringGenerated>();

            if (null == t)
                return;

            _nIndexGenerator = t.Strings.Count;

            for (var nIx = 0; nIx < t.Strings.Count; ++nIx)
                _dictStrings[t.Strings.ElementAt(nIx)] = nIx;
        }

        internal override int
            Set(string str_in)
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
            lock (DictSortedStrings)
            {
                int retVal = 0;

                if (false == DictSortedStrings.TryGetValue(str, out retVal))
                {
                    retVal = Interlocked.Increment(ref _nIndexGenerator) - 1;
                    _dictStrings[str] = retVal;
                }

                return retVal;
            }
        }

        internal override int
            IndexOf(string str) =>
            DictSortedStrings[str];

        internal int
            IndexGenerator => _nIndexGenerator;
        int _nIndexGenerator;

        internal IReadOnlyDictionary<string, int>
            DictSortedStrings => (IReadOnlyDictionary<string, int>)_dictStrings;
        IDictionary<string, int> _dictStrings = new SortedDictionary<string, int>();
    }
}
