﻿using System.Collections.Generic;
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
        internal readonly ulong
            Length;

        internal int
            AllFilesHash = 0;

        internal IReadOnlyList<int>
            FilesHereHashes { get { return _LSH_object.As<IReadOnlyList<int>>(); } set { _LSH_object = value; } }
        internal int
            LSH_Index { get { return (int)_LSH_object; } set { _LSH_object = value; } }
        object _LSH_object = null;

        internal ulong
            TotalLength;
        internal uint
            FileCountHere;                          // Found 15 bits
        internal uint
            FileCountTotal;                         // Found 21 bits
        internal uint
            SubDirs;                                // Found 17 bits
        internal uint
            DirsWithFiles;                          // Found 15 bits

        internal Rect
            TreeMapRect;

        internal DetailsDatum() { }
        internal DetailsDatum(uint nPrevLineNo, uint nLineNo, ulong nLength, int nAllFilesHash, IReadOnlyList<int> lsFilesHereHashes)
        {
            PrevLineNo = nPrevLineNo;
            LineNo = nLineNo;
            Length = nLength;
            AllFilesHash = nAllFilesHash;
            FilesHereHashes = lsFilesHereHashes;
        }

        protected DetailsDatum(DetailsDatum datum)
        {
            TotalLength = datum.TotalLength;
            FileCountTotal = datum.FileCountTotal;
            SubDirs = datum.SubDirs;
            FileCountHere = datum.FileCountHere;
            DirsWithFiles = datum.DirsWithFiles;
            PrevLineNo = datum.PrevLineNo;
            LineNo = datum.LineNo;
            Length = datum.Length;
            AllFilesHash = datum.AllFilesHash;
            FilesHereHashes = datum.FilesHereHashes;
        }

        static public DetailsDatum
            operator +(DetailsDatum datum1, DetailsDatum datum2) =>
            new DetailsDatum
        {
            TotalLength = datum1.TotalLength + datum2.TotalLength,
            FileCountTotal = datum1.FileCountTotal + datum2.FileCountTotal,
            SubDirs = datum1.SubDirs + datum2.SubDirs,
            FileCountHere = datum1.FileCountHere + datum2.FileCountHere,
            DirsWithFiles = datum1.DirsWithFiles + datum2.DirsWithFiles,
            AllFilesHash = datum1.AllFilesHash + datum2.AllFilesHash
        };

        internal FolderKeyTuple
            Key => new FolderKeyTuple(TotalLength, FileCountTotal, DirsWithFiles, AllFilesHash);
    }
}
