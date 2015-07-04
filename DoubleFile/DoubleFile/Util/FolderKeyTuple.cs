﻿using System;

namespace DoubleFile
{
    class FolderKeyTuple : Tuple<ulong, uint, uint, uint, uint, uint>
    {
        internal FolderKeyTuple(ulong nTotalLength,
            uint nFilesInSubdirs,
            uint nDirsWithFiles,
            uint[] folderScore)
        : base(nTotalLength, nFilesInSubdirs, nDirsWithFiles,
              folderScore[0], folderScore[1], folderScore[2])
        {
            MBoxStatic.Assert(99902, 3 == folderScore.Length);
        }
    }
}
