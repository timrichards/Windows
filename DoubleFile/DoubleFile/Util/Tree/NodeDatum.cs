using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    interface ISetNodeDatum_Hash_AllFiles
    {
        void Set(int nHash);
    }

    // can't be struct because it serves covariance; has parameterless constructor
    class NodeDatum : ISetNodeDatum_Hash_AllFiles
    {
        internal int
            FileCountHere => (int)(FolderDetails.LineNo - FolderDetails.PrevLineNo - 1);      // Found 15 bits
        internal uint
            FileCountTotal { get; private set; }    // Found 21 bits

        internal uint
            SubDirs { get; private set; }           // Found 17 bits
        internal uint
            DirsWithFiles { get; private set; }     // Found 15 bits

        internal int
            Hash_AllFiles { get; private set; }

        internal IReadOnlyList<int>
            Hashes_FilesHere => FolderDetails.FilesHereHashes;
        internal bool
            Hashes_FilesHere_IsComplete => FolderDetails.IsHashComplete;

        internal IReadOnlyList<int>
            Hashes_SubnodeFiles_Scratch = null;
        internal bool
            Hashes_SubnodeFiles_IsComplete = false;

        internal ulong
            LengthHere => FolderDetails?.Length ?? 0;
        internal ulong
            LengthTotal { get; private set; }

        internal Tree.FolderDetails
            FolderDetails { get; private set; }

        internal NodeDatum() { }
        internal NodeDatum(Tree.FolderDetails datum)
        {
            FolderDetails = datum;
        }
        internal NodeDatum(ulong lengthTotal)
        {
            LengthTotal = lengthTotal;
        }

        protected NodeDatum(NodeDatum datum)
        {
            LengthTotal = datum.LengthTotal;
            FileCountTotal = datum.FileCountTotal;
            SubDirs = datum.SubDirs;
            DirsWithFiles = datum.DirsWithFiles;
            FolderDetails = datum.FolderDetails;
        }

        internal NodeDatum AddDatum(NodeDatum datum)
        {
            LengthTotal += datum.LengthTotal;
            FileCountTotal += datum.FileCountTotal;
            SubDirs += datum.SubDirs;
            DirsWithFiles += datum.DirsWithFiles;
            return this;
        }

        internal NodeDatum SetDatum(NodeDatum datum, uint nodeCount)
        {
            if (null == FolderDetails)
            {
                FolderDetails = datum.FolderDetails;
            }

            if (null == FolderDetails)
            {
                Util.Assert(0, false, "NodeDatum SetDatum()\nnull != FolderDetails", bIfDefDebug: true);
                FolderDetails = new Tree.FolderDetails { };
            }

            LengthTotal = (datum.LengthTotal += FolderDetails.Length);
            FileCountTotal = (datum.FileCountTotal += (uint)FileCountHere);
            SubDirs = (datum.SubDirs += nodeCount);

            if (0 < FileCountHere)
                ++datum.DirsWithFiles;

            DirsWithFiles = datum.DirsWithFiles;
            return this;
        }

        void ISetNodeDatum_Hash_AllFiles.Set(int nHash) => Hash_AllFiles = nHash;
    }
}
