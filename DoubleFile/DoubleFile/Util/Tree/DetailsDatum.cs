﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace DoubleFile
{
    // can't be struct because it serves covariance; has parameterless constructor
    class DetailsDatum
    {
        internal readonly uint
            PrevLineNo;                             // Found 21 bits
        internal readonly uint
            LineNo;                                 // Found 21 bits

        internal int
            Hash_AllFiles = 0;
        internal readonly IReadOnlyList<int>
            Hashes_FilesHere = null;
        internal IReadOnlyList<int>
            Hashes_SubnodeFiles_Scratch = null;

        internal readonly ulong
            LengthHere;
        internal ulong
            LengthTotal;

        internal uint
            FileCountHere;                          // Found 15 bits
        internal uint
            FileCountTotal;                         // Found 21 bits

        internal uint
            SubDirs;                                // Found 17 bits
        internal uint
            DirsWithFiles;                          // Found 15 bits

        internal Rect
            TreemapRect;

        internal DetailsDatum() { }
        internal DetailsDatum(uint nPrevLineNo, uint nLineNo, ulong nLength, int nAllFilesHash, IReadOnlyList<int> lsFilesHereHashes)
        {
            PrevLineNo = nPrevLineNo;
            LineNo = nLineNo;
            LengthHere = nLength;
            Hash_AllFiles = nAllFilesHash;
            Hashes_FilesHere = lsFilesHereHashes.OrderBy(n => n).Distinct().ToArray();
        }

        protected DetailsDatum(DetailsDatum datum)
        {
            LengthTotal = datum.LengthTotal;
            FileCountTotal = datum.FileCountTotal;
            SubDirs = datum.SubDirs;
            FileCountHere = datum.FileCountHere;
            DirsWithFiles = datum.DirsWithFiles;
            PrevLineNo = datum.PrevLineNo;
            LineNo = datum.LineNo;
            LengthHere = datum.LengthHere;
            Hash_AllFiles = datum.Hash_AllFiles;
            Hashes_FilesHere = datum.Hashes_FilesHere;
        }

        static public DetailsDatum
            operator +(DetailsDatum datum1, DetailsDatum datum2) =>
            new DetailsDatum
        {
            LengthTotal = datum1.LengthTotal + datum2.LengthTotal,
            FileCountTotal = datum1.FileCountTotal + datum2.FileCountTotal,
            SubDirs = datum1.SubDirs + datum2.SubDirs,
            FileCountHere = datum1.FileCountHere + datum2.FileCountHere,
            DirsWithFiles = datum1.DirsWithFiles + datum2.DirsWithFiles,
            Hash_AllFiles = datum1.Hash_AllFiles + datum2.Hash_AllFiles
        };

        internal FolderKeyTuple
            Key => new FolderKeyTuple(LengthTotal, FileCountTotal, DirsWithFiles, Hash_AllFiles);
    }
}
