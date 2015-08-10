using System;
using System.Collections.Generic;

namespace DoubleFile
{
    class FolderKeyTuple : Tuple<ulong, uint, uint, uint, uint, uint>
    {
        internal FolderKeyTuple(ulong nTotalLength,
            uint nFilesInSubdirs,
            uint nDirsWithFiles,
            IReadOnlyList<uint> folderScore)
        : base(nTotalLength, nFilesInSubdirs, nDirsWithFiles,
              folderScore[0], folderScore[1], folderScore[2])
        {
            Util.Assert(99902, 3 == folderScore.Count);
        }
    }
}
