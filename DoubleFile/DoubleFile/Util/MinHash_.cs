using System;
using System.Collections.Generic;

namespace DoubleFile
{
    class MinHash_
    {
        internal
			MinHash_(int universeSize)
        {
            Util.Assert(99895, 0 < universeSize);
            _hashFunctions = new Hash[_numHashFunctions];

            var r = new Random(11);

            for (var i = 0; i < _numHashFunctions; ++i)
            {
                var a = (uint)r.Next(universeSize);
                var b = (uint)r.Next(universeSize);
                var c = (uint)r.Next(universeSize);

                _hashFunctions[i] = x => QHash((uint)x, a, b, c, (uint)universeSize);
            } 
        }

        static int
			QHash(uint x, uint a, uint b, uint c, uint bound) =>	//Modify the hash family as per the size of possible elements in a Set
            Math.Abs((int)((a * (x >> 4) + b * x + c) & 131071));

        internal double
			Similarity<T>(HashSet<T> set1, HashSet<T> set2)
        {
            Util.Assert(99636, (0 < set1.Count) && (0 < set2.Count));

            var numSets = 2;
            var bitmap = BuildBitmap(set1, set2);
            var minHashValues = GetMinHashSlots(numSets, _numHashFunctions);

            ComputeMinHashForSet(set1, 0, minHashValues, bitmap);
            ComputeMinHashForSet(set2, 1, minHashValues, bitmap);

            return ComputeSimilarityFromSignatures(minHashValues, _numHashFunctions);
        }

        void
			ComputeMinHashForSet<T>(HashSet<T> set, short setIndex, int[][] minHashValues, IDictionary<T, bool[]> bitArray)
        {
            var index = 0;

            foreach (var element in bitArray.Keys)
            {
                for (var i = 0; i < _numHashFunctions; ++i)
                {
                    if (false == set.Contains(element))
                        continue;

                    int hindex = _hashFunctions[i](index);

                    if (hindex < minHashValues[setIndex][i])
                        minHashValues[setIndex][i] = hindex;
                }

                ++index;
            }
        }

        static int[][]
			GetMinHashSlots(int numSets, int numHashFunctions)
        {
            var minHashValues = Util.CreateJaggedArray<int[][]>(numSets, numHashFunctions);

            for (int i = 0; i < numSets; ++i)
            {
                for (int j = 0; j < numHashFunctions; j++)
                    minHashValues[i][j] = int.MaxValue;
            }

            return minHashValues;
        }

        static Dictionary<T, bool[]>
			BuildBitmap<T>(HashSet<T> set1, HashSet<T> set2)
        {
            var bitArray = new Dictionary<T, bool[]>();

            foreach (var item in set1)
                bitArray.Add(item, new bool[2] { true, false });

            foreach (var item in set2)
            {
                bool[] value = null;

                if (bitArray.TryGetValue(item, out value))
                    bitArray[item] = new[] { true, true };		// item is present in set1
                else
                    bitArray.Add(item, new[] { false, true });	// item is not present in set1
            }

            return bitArray;
        }

        static double
			ComputeSimilarityFromSignatures(int[][] minHashValues, int numHashFunctions)
        {
            var identicalMinHashes = 0;

            for (int i = 0; i < numHashFunctions; ++i)
            {
                if (minHashValues[0][i] == minHashValues[1][i])
                    ++identicalMinHashes;
            }

            return (1.0 * identicalMinHashes) / numHashFunctions;
        }

        const int
			_numHashFunctions = 100; //Modify this parameter
        delegate int
			Hash(int index);
        Hash[]
			_hashFunctions;
    }
}