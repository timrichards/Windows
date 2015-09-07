using System;

namespace DoubleFile
{
    class FolderKeyTuple : Tuple<ulong, uint, uint, double, double>
    {
        internal FolderKeyTuple(ulong nTotalLength,
            uint nFilesInSubdirs,
            uint nDirsWithFiles,
            double mean, double variance)
        : base(nTotalLength, nFilesInSubdirs, nDirsWithFiles,
              mean, variance)
        {
        }
    }
}
