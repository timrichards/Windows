using System;

namespace DoubleFile
{
    class FolderKeyTuple : Tuple<ulong, uint, uint, int>
    {
        internal FolderKeyTuple(ulong nTotalLength,
            uint nFilesInSubdirs,
            uint nDirsWithFiles,
            int nFolderScore)
        : base(nTotalLength, nFilesInSubdirs, nDirsWithFiles, nFolderScore)
        {
        }
    }
}
