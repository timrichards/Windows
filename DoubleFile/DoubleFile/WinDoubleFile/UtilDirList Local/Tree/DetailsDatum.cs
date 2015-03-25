using System.Drawing;

namespace DoubleFile
{
    // can't be struct because it serves covariance; has parameterless constructor
    class DetailsDatum
    {
        internal uint
            PrevLineNo { get; private set; }        // Found 21 bits
        internal uint
            LineNo { get; private set; }            // Found 21 bits
        internal ulong
            Length { get; private set; }
        internal int
            HashParity { get; set; }

        internal ulong
            TotalLength { get; set; }
        internal uint
            FilesInSubdirs { get; set; }            // Found 21 bits
        internal uint
            SubDirs { get; set; }                   // Found 17 bits
        internal uint
            ImmediateFiles { get; set; }            // Found 15 bits
        internal uint
            DirsWithFiles { get; set; }             // Found 15 bits

        internal Rectangle
            TreeMapRect { get; set; }

        internal DetailsDatum() { }
        internal DetailsDatum(uint nPrevLineNo, uint nLineNo, ulong nLength, int nHashParity)
        {
            PrevLineNo = nPrevLineNo;
            LineNo = nLineNo;
            Length = nLength;
            HashParity = nHashParity;
        }

        protected DetailsDatum(DetailsDatum datum)
        {
            TotalLength = datum.TotalLength;
            FilesInSubdirs = datum.FilesInSubdirs;
            SubDirs = datum.SubDirs;
            ImmediateFiles = datum.ImmediateFiles;
            DirsWithFiles = datum.DirsWithFiles;
            PrevLineNo = datum.PrevLineNo;
            LineNo = datum.LineNo;
            Length = datum.Length;
            HashParity = datum.HashParity;
        }

        static public DetailsDatum operator +(DetailsDatum datum1, DetailsDatum datum2)
        {
            return new DetailsDatum
            {
                TotalLength = datum1.TotalLength + datum2.TotalLength,
                FilesInSubdirs = datum1.FilesInSubdirs + datum2.FilesInSubdirs,
                SubDirs = datum1.SubDirs + datum2.SubDirs,
                ImmediateFiles = (ushort)(datum1.ImmediateFiles + datum2.ImmediateFiles),
                DirsWithFiles = (ushort)(datum1.DirsWithFiles + datum2.DirsWithFiles),
                HashParity = datum1.HashParity + datum2.HashParity
            };
        }

        internal FolderKeyTuple Key
        {
            get
            {
                return new FolderKeyTuple(TotalLength, FilesInSubdirs, DirsWithFiles, HashParity);
            }
        }
    }
}
