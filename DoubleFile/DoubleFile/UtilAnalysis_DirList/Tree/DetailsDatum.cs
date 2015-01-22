
namespace SearchDirLists
{
    class DetailsDatum
    {
        internal ulong nTotalLength = 0;
        internal uint nFilesInSubdirs = 0;
        internal uint nSubDirs = 0;
        internal uint nImmediateFiles = 0;
        internal uint nDirsWithFiles = 0;

        internal DetailsDatum() { }
        internal DetailsDatum(DetailsDatum in_datum)
        {
            nTotalLength = in_datum.nTotalLength;
            nFilesInSubdirs = in_datum.nFilesInSubdirs;
            nSubDirs = in_datum.nSubDirs;
            nImmediateFiles = in_datum.nImmediateFiles;
            nDirsWithFiles = in_datum.nDirsWithFiles;
        }

        static public DetailsDatum operator +(DetailsDatum in_datum1, DetailsDatum in_datum2)
        {
            DetailsDatum datum = new DetailsDatum();

            datum.nTotalLength = in_datum1.nTotalLength + in_datum2.nTotalLength;
            datum.nFilesInSubdirs = in_datum1.nFilesInSubdirs + in_datum2.nFilesInSubdirs;
            datum.nSubDirs = in_datum1.nSubDirs + in_datum2.nSubDirs;
            datum.nImmediateFiles = in_datum1.nImmediateFiles + in_datum2.nImmediateFiles;
            datum.nDirsWithFiles = in_datum1.nDirsWithFiles + in_datum2.nDirsWithFiles;
            return datum;
        }
    }
}
