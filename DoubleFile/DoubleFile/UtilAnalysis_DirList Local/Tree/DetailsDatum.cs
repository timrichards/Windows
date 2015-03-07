namespace DoubleFile
{
    // can't be struct because it serves covariance; has parameterless constructor
    class DetailsDatum
    {
        internal ulong
            TotalLength { get; set; }
        internal uint
            FilesInSubdirs { get; set; }
        internal uint
            SubDirs { get; set; }
        internal uint
            ImmediateFiles { get; set; }
        internal uint
            DirsWithFiles { get; set; }
        internal int
            HashParity { get; set; }

        internal DetailsDatum() { }
        internal DetailsDatum(DetailsDatum datum)
        {
            TotalLength = datum.TotalLength;
            FilesInSubdirs = datum.FilesInSubdirs;
            SubDirs = datum.SubDirs;
            ImmediateFiles = datum.ImmediateFiles;
            DirsWithFiles = datum.DirsWithFiles;
            HashParity = datum.HashParity;
        }

        static public DetailsDatum operator +(DetailsDatum datum1, DetailsDatum datum2)
        {
            return new DetailsDatum
            {
                TotalLength = datum1.TotalLength + datum2.TotalLength,
                FilesInSubdirs = datum1.FilesInSubdirs + datum2.FilesInSubdirs,
                SubDirs = datum1.SubDirs + datum2.SubDirs,
                ImmediateFiles = datum1.ImmediateFiles + datum2.ImmediateFiles,
                DirsWithFiles = datum1.DirsWithFiles + datum2.DirsWithFiles,
                HashParity = datum1.HashParity + datum2.HashParity
            };
        }
    }
}
