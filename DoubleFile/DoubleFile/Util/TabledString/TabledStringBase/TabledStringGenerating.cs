using System.Collections.Generic;

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
                _dictStrings[t.Strings[nIx]] = nIx;
        }

        internal override int
            Set(string str_in)
        {
            //var split = str_in.Split('\\');

            //if (1 < split.Length)
            //{
            //    foreach (var str in split)
            //    {
            //        if (0 < str.Length)
            //            SetA(str);
            //    }
            //}

            return SetA(str_in);
        }
        int SetA(string str)
        {
            lock (_dictStrings)
            {
                int retVal = 0;

                // false == TryGetValue beats expected value test
                if (false == _dictStrings.TryGetValue(str, out retVal))                   
                    _dictStrings[str] = retVal = _nIndexGenerator++;

                lock (_trie)
                {
                    int retVal_ = IndexGenerator_ + 1;

                    if (retVal_ == _trie.Put(str, retVal_))
                        IndexGenerator_ = retVal_;              
                }

                return retVal;
            }
        }

        internal override int               // unused
            IndexOf(string str) =>
            DictSortedStrings[str];

        internal int IndexGenerator_ { get; private set;  } = default(int);

        internal int
            IndexGenerator => _nIndexGenerator;
        int _nIndexGenerator = 0;   // left as non-prop in case of Interlocked.Increment (non-atomic? speed issue?)

        internal IReadOnlyDictionary<string, int>
            DictSortedStrings => (IReadOnlyDictionary<string, int>)_dictStrings;
        IDictionary<string, int> _dictStrings = new SortedDictionary<string, int>();
        TernarySearchTrie<int> _trie = new TernarySearchTrie<int>();
    }
}
