using System;

namespace DoubleFile
{
    class FolderKeyTuple : Tuple<ulong, uint, uint, int, uint>
    {
        internal FolderKeyTuple(ulong nTotalLength,
            uint nFilesInSubdirs,
            uint nDirsWithFiles,
            Tuple<int, uint> folderScoreTuple)
        : base(nTotalLength, nFilesInSubdirs, nDirsWithFiles, folderScoreTuple.Item1, folderScoreTuple.Item2)
        {
        }
    }
}
