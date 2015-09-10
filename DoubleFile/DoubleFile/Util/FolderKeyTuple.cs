using System;
using System.Collections.Generic;

namespace DoubleFile
{
    class FolderKeyTuple : Tuple<ulong, uint, uint, int>
    {
        internal FolderKeyTuple(ulong nTotalLength,
            uint nFilesInSubdirs,
            uint nDirsWithFiles,
            int nAllFilesHash)
        : base(nTotalLength, nFilesInSubdirs, nDirsWithFiles, nAllFilesHash)
        {
        }
    }
}
