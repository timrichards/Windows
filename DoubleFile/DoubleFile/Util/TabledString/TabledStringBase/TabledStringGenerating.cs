using System.Collections.Concurrent;
using System.Threading;

namespace DoubleFile
{
    class TabledStringGenerating : TabledStringBase
    {
        internal
            TabledStringGenerating(TabledStringBase t_ = null)
        {
            var nThreads = Statics.LVprojectVM.CanLoadCount;
            var t = t_.As<TabledStringGenerated>();

            if (null != t)
            {
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

        ConcurrentDictionary<int, string>
            DictStringsRev;
    }
}
