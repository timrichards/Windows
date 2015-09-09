using System;
using System.Collections.Generic;

namespace DoubleFile
{
    class FolderKeyTuple : Tuple<ulong, uint, uint, int>
    {
        internal FolderKeyTuple(ulong nTotalLength,
            uint nFilesInSubdirs,
            uint nDirsWithFiles,
            int hashcode)
        : base(nTotalLength, nFilesInSubdirs, nDirsWithFiles, hashcode)
        {
        }
    }
}
