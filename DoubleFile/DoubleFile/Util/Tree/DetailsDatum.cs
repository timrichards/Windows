﻿using System;
using System.Drawing;

namespace DoubleFile
{
    // can't be struct because it serves covariance; has parameterless constructor
    class DetailsDatum
    {
        internal uint
            PrevLineNo { get; private set; }        // Found 21 bits
        internal uint
            LineNo { get; private set; }            // Found 21 bits
        internal ulong
            Length { get; private set; }
        internal Tuple<int, uint>
            FolderScoreTuple = Tuple.Create(0, 0U);

        internal ulong
            TotalLength { get; set; }
        internal uint
            FileCountHere { get; set; }             // Found 15 bits
        internal uint
            FileCountTotal { get; set; }            // Found 21 bits
        internal uint
            SubDirs { get; set; }                   // Found 17 bits
        internal uint
            DirsWithFiles { get; set; }             // Found 15 bits

        internal Rectangle
            TreeMapRect { get; set; }

        internal DetailsDatum() { }
        internal DetailsDatum(uint nPrevLineNo, uint nLineNo, ulong nLength, Tuple<int, uint> folderScoreTuple)
        {
            PrevLineNo = nPrevLineNo;
            LineNo = nLineNo;
            Length = nLength;
            FolderScoreTuple = folderScoreTuple;
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
            FolderScoreTuple = datum.FolderScoreTuple;
        }

        static public DetailsDatum operator +(DetailsDatum datum1, DetailsDatum datum2)
        {
            return new DetailsDatum
            {
                TotalLength = datum1.TotalLength + datum2.TotalLength,
                FileCountTotal = datum1.FileCountTotal + datum2.FileCountTotal,
                SubDirs = datum1.SubDirs + datum2.SubDirs,
                FileCountHere = datum1.FileCountHere + datum2.FileCountHere,
                DirsWithFiles = datum1.DirsWithFiles + datum2.DirsWithFiles,

                FolderScoreTuple =
                    Tuple.Create(
                    datum1.FolderScoreTuple.Item1 + datum2.FolderScoreTuple.Item1,
                    datum1.FolderScoreTuple.Item2 + datum2.FolderScoreTuple.Item2)
            };
        }

        internal FolderKeyTuple Key
        {
            get
            {
                return new FolderKeyTuple(TotalLength, FileCountTotal, DirsWithFiles, FolderScoreTuple);
            }
        }
    }
}
