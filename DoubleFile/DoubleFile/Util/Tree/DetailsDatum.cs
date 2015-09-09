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

        internal IReadOnlyList<int>
            Hashcodes { get { return _hashCodes.As<IReadOnlyList<int>>(); } set { _hashCodes = value; } }
        object _hashCodes = null;

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
        internal DetailsDatum(uint nPrevLineNo, uint nLineNo, ulong nLength, IReadOnlyList<int> hashcodes)
        {
            PrevLineNo = nPrevLineNo;
            LineNo = nLineNo;
            Length = nLength;
            Hashcodes = hashcodes;
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
            Hashcodes = datum.Hashcodes;
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
            Hashcodes = datum1.Hashcodes?.Concat(datum2.Hashcodes).ToList() ?? datum2.Hashcodes
        };

        internal FolderKeyTuple
            Key => new FolderKeyTuple(TotalLength, FileCountTotal, DirsWithFiles, Hashcodes.GetHashCode());
    }
}
