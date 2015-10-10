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

        internal readonly int
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
        internal DetailsDatum(uint nPrevLineNo, uint nLineNo, ulong nLength, IReadOnlyList<int> lsFilesHereHashes)
        {
            PrevLineNo = nPrevLineNo;
            FileCountHere = (int)(nLineNo - PrevLineNo - 1);
            LengthHere = nLength;
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
            LengthHere = datum.LengthHere;
            Hashes_FilesHere = datum.Hashes_FilesHere;
        }
    }
}
