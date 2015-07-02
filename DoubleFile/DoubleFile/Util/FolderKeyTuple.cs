using System;

namespace DoubleFile
{
    class FolderKeyTuple : Tuple<ulong, uint, uint, int, int>
    {
        internal FolderKeyTuple(ulong nTotalLength,
            uint nFilesInSubdirs,
            uint nDirsWithFiles,
            Tuple<int, int> folderScoreTuple)
        : base(nTotalLength, nFilesInSubdirs, nDirsWithFiles, folderScoreTuple.Item1, folderScoreTuple.Item2)
        {
        }
    }
}
