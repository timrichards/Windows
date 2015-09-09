using System;
using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    internal class MinHash
    {
        internal MinHash(int universeSize, int numHashFunctions)
        {
            NumHashFunctions = numHashFunctions;
            GenerateHashFunctions(BitsForUniverse(universeSize));
        }

        internal int
            NumHashFunctions;

        internal delegate uint Hash(int toHash);

        internal Hash[]
            HashFunctions;

        // Generates the Universal Random Hash functions
        // http://en.wikipedia.org/wiki/Universal_hashing
        void GenerateHashFunctions(int u)
        {
            HashFunctions = new Hash[NumHashFunctions];

            // will get the same hash functions each time since the same random number seed is used
            var r = new Random(10);

            for (int i = 0; i < NumHashFunctions; ++i)
            {
                var a = (uint)((r.Next() + 3) >> 1) << 1;
                var b = ((uint)r.Next((1 << u) - 1) + 1);   // parameter b must be greater than zero and less than universe size

                HashFunctions[i] = x => QHash(x, a, b, u);
            }
        }

        // Returns the number of bits needed to store the universe
        internal int
            BitsForUniverse(int universeSize) => (int)Math.Truncate(Math.Log(universeSize, 2)) + 1;

        // Universal hash function with two parameters a and b, and universe size in bits
        static uint
            QHash(int x, uint a, uint b, int u) => (a * (uint)x + b) >> (32 - u);

        // Returns the list of min hashes for the given set of input word IDs
        internal List<int>
            GetMinHash(IReadOnlyList<int> inputWordIDs)
        {
            var minHashes = new int[NumHashFunctions];

            for (int h = 0; h < NumHashFunctions; ++h)
                minHashes[h] = int.MaxValue;

            foreach (int id in inputWordIDs)
            {
                for (int h = 0; h < NumHashFunctions; ++h)
                    minHashes[h] = (int)Math.Min(minHashes[h], HashFunctions[h](id));
            }

            return minHashes.ToList();
        }

        // Calculates the similarity of two lists of min hash values. Approximately Numerically equivilant to Jaccard Similarity
        internal double
            Similarity(List<int> l1, List<int> l2) => Jaccard.Calc(l1, l2);
    }
}
