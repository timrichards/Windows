using System;

namespace DoubleFile
{
    class FolderKeyTuple : Tuple<ulong, uint, uint, int>
    {
        internal FolderKeyTuple(ulong nTotalLength,
            uint nFilesInSubdirs,
            uint nDirsWithFiles,
            int nHashParity)
        : base(nTotalLength, nFilesInSubdirs, nDirsWithFiles, nHashParity)
        {
        }
    }
}
