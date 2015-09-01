using System.Collections.Generic;
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
        internal readonly ulong
            Length;
        internal IReadOnlyList<uint>
            FolderScore = new[] { 0U, 0U, 0U };     // Weighted folder scores: HashParity (random); largest; smallest

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
        internal DetailsDatum(uint nPrevLineNo, uint nLineNo, ulong nLength, IReadOnlyList<uint> folderScore)
        {
            PrevLineNo = nPrevLineNo;
            LineNo = nLineNo;
            Length = nLength;
            FolderScore = folderScore;
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
            FolderScore = datum.FolderScore;
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

                FolderScore =
                    datum1.FolderScore.Zip(datum2.FolderScore, (n1, n2) => n1 + n2)
                    .ToArray()
            };
        }

        internal FolderKeyTuple Key =>
            new FolderKeyTuple(TotalLength, FileCountTotal, DirsWithFiles, FolderScore);
    }
}
