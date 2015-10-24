﻿using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    interface ISetNodeDatum_Hash_AllFiles
    {
        void Set(int nHash);
    }

    // can't be struct because it serves covariance; has parameterless constructor
    class NodeDatum : ISetNodeDatum_Hash_AllFiles
    {
        internal readonly uint
            PrevLineNo;                             // Found 21 bits

        internal readonly int
            FileCountHere;                          // Found 15 bits
        internal uint
            FileCountTotal { get; private set; }    // Found 21 bits

        internal uint
            SubDirs { get; private set; }           // Found 17 bits
        internal uint
            DirsWithFiles { get; private set; }     // Found 15 bits

        internal int
            Hash_AllFiles { get; private set; }

        internal readonly IReadOnlyList<int>
            Hashes_FilesHere = null;
        internal bool
            Hashes_FilesHere_IsComplete { get; private set; }

        internal IReadOnlyList<int>
            Hashes_SubnodeFiles_Scratch = null;
        internal bool
            Hashes_SubnodeFiles_IsComplete = false;

        internal readonly ulong
            LengthHere;
        internal ulong
            LengthTotal { get; private set; }

        internal NodeDatum() { }
        internal NodeDatum(uint nPrevLineNo, uint nLineNo, ulong nLength, IReadOnlyList<int> lsFilesHereHashes, bool isHashComplete)
        {
            PrevLineNo = nPrevLineNo;
            FileCountHere = (int)(nLineNo - PrevLineNo - 1);
            LengthHere = nLength;
            Hashes_FilesHere = lsFilesHereHashes.OrderBy(n => n).Distinct().ToArray();
            Hashes_FilesHere_IsComplete = isHashComplete;
        }
        internal NodeDatum(ulong lengthTotal)
        {
            LengthTotal = lengthTotal;
        }

        protected NodeDatum(NodeDatum datum)
        {
            LengthTotal = datum.LengthTotal;
            FileCountTotal = datum.FileCountTotal;
            SubDirs = datum.SubDirs;
            DirsWithFiles = datum.DirsWithFiles;

            FileCountHere = datum.FileCountHere;
            PrevLineNo = datum.PrevLineNo;
            LengthHere = datum.LengthHere;
            Hashes_FilesHere = datum.Hashes_FilesHere;
            Hashes_FilesHere_IsComplete = datum.Hashes_FilesHere_IsComplete;
        }

        internal NodeDatum AddDatum(NodeDatum datum)
        {
            LengthTotal += datum.LengthTotal;
            FileCountTotal += datum.FileCountTotal;
            SubDirs += datum.SubDirs;
            DirsWithFiles += datum.DirsWithFiles;
            return this;
        }

        internal NodeDatum SetDatum(NodeDatum datum, uint nodeCount)
        {
            LengthTotal = (datum.LengthTotal += LengthHere);
            FileCountTotal = (datum.FileCountTotal += (uint)FileCountHere);
            SubDirs = (datum.SubDirs += nodeCount);

            if (0 < FileCountHere)
                ++datum.DirsWithFiles;

            DirsWithFiles = datum.DirsWithFiles;
            return this;
        }

        void ISetNodeDatum_Hash_AllFiles.Set(int nHash) => Hash_AllFiles = nHash;
    }
}
