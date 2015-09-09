using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    internal class LocalitySensitiveHashing 
    {
        internal LocalitySensitiveHashing(int[][] minHashes, int rowsPerBand)
        {
            _minHashes = minHashes;
            _numHashFunctions = minHashes.GetUpperBound(1) + 1;
            _numSets = minHashes.GetUpperBound(0) + 1;
            _rowsPerBand = rowsPerBand;
            _numBands = _numHashFunctions / rowsPerBand;
        }

        internal void Calc()
        {
            int hash = 0;

            for (int b = 0; b < _numBands; ++b)
            {
                var lsSets = new SortedList<int, List<int>>();

                for (var s = 0; s < _numSets; ++s)
                {
                    var hashValue = 0;

                    for (var h = hash; h < hash + _rowsPerBand; ++h)
                        hashValue = unchecked(hashValue * 1174247 + _minHashes[s][h]);

                    if (false == lsSets.ContainsKey(hashValue))
                        lsSets.Add(hashValue, new List<int>());

                    lsSets[hashValue].Add(s);
                }

                hash += _rowsPerBand;

                var copy = new SortedList<int, List<int>>();

                foreach (var ic in lsSets.Keys)
                {
                    if (1 < lsSets[ic].Count())
                        copy.Add(ic, lsSets[ic]);
                }

                _lshBuckets.Add(copy);
            }
        }

        internal List<int>
            GetNearest(int n)
        {
            var nearest = new List<int>();

            foreach (var b in _lshBuckets)
            {
                foreach (var li in b.Values)
                {
                    if (li.Contains(n))
                    {
                        nearest.AddRange(li);
                        break;
                    }
                }
            }

            nearest = nearest.Distinct().ToList();
            nearest.Remove(n);  // remove the document itself
            return nearest;
        }

        Dictionary<int, HashSet<int>> _dicthBuckets = new Dictionary<int, HashSet<int>>();
        List<SortedList<int, List<int>>> _lshBuckets = new List<SortedList<int, List<int>>>();
        int[][] _minHashes;
        int _numBands;
        int _numHashFunctions;
        int _rowsPerBand;
        int _numSets;
    }
}
