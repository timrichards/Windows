using System;
using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    class TabledStringGenerated : TabledStringBase
    {
        internal TabledStringGenerated(TabledStringBase t_)
        {
            var t = t_.As<TabledStringGenerating>();

            if (null == t)
            {
                Util.Assert(99915, false);
                return;
            }

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
}
