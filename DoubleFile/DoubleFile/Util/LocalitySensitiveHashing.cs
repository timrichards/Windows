using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    internal class LocalitySensitiveHashing
    {
        internal LocalitySensitiveHashing(IReadOnlyList<IReadOnlyList<int>> minHashes, int rowsPerBand)
        {
            _minHashes = minHashes;
        }

        internal void Calc()
        {
            var lsSets = new Dictionary<int, List<int>>();
            var nRows = _minHashes.Count;

            for (var s = 0; s < nRows; ++s)
            {
                var hashValue = 0;
                var nCols = _minHashes[s].Count;

                for (var h = 0; h < nCols; ++h)
                    hashValue = unchecked(hashValue * 1174247 + _minHashes[s][h]);

                if (false == lsSets.ContainsKey(hashValue))
                    lsSets.Add(hashValue, new List<int>());

                lsSets[hashValue].Add(s);
            }

            var copy = new SortedList<int, List<int>>();

            foreach (var kvp in lsSets.Where(kvp => 1 < kvp.Value.Count))
                copy.Add(kvp.Key, kvp.Value);

            _lshBuckets.Add(copy);
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
        IReadOnlyList<IReadOnlyList<int>> _minHashes;
    }
}
