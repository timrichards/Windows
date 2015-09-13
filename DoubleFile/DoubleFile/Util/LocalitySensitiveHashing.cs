using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    class LocalitySensitiveHashing
    {
        internal
            LocalitySensitiveHashing(int[,] minHashes, int rowsPerBand)
        {
            var thisHash = 0;
            var numSets = minHashes.GetUpperBound(0) + 1;
            var numBands = (minHashes.GetUpperBound(1) + 1) / rowsPerBand;

            for (var b = 0; b < numBands; ++b)
            {
                var thisSL = new Dictionary<int, List<int>>();

                for (int s = 0; s < numSets; s++)
                {
                    var hashValue = 0;

                    for (var th = thisHash; th < thisHash + rowsPerBand; ++th)
                        hashValue = unchecked(hashValue * 1174247 + minHashes[s, th]);

                    if (false == thisSL.ContainsKey(hashValue))
                        thisSL.Add(hashValue, new List<int>());

                    thisSL[hashValue].Add(s);
                }

                _lshBuckets.Add(thisSL.Values.Where(value => 1 < value.Count));
            }
        }

        internal List<int>
            GetNearest(int n)
        {
            var nearest = new List<int>();
            var bFound = false;

            foreach (var b in _lshBuckets)
            {
                foreach (var li in b)
                {
                    if (li.Contains(n))
                    {
                        nearest.AddRange(li);

                        if (bFound)
                            Util.Assert(0, false);

                        bFound = true;
                    }

                    if (bFound)
                        break;
                }

                //if (bFound)
                //    break;
            }

            nearest = nearest.Distinct().ToList();
            nearest.Remove(n);  // remove the document itself
            return nearest;
        }

        List<IEnumerable<List<int>>>
            _lshBuckets = new List<IEnumerable<List<int>>>();
    }
}
