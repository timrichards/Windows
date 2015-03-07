using System;

namespace DoubleFile
{
    struct FolderKeyStruct : IComparable
    {
        internal FolderKeyStruct(ulong nTotalLength,
            uint nFilesInSubdirs,
            uint nDirsWithFiles,
            int nHashParity)
        {
            _nTotalLength = nTotalLength;
            _nFilesInSubdirs = nFilesInSubdirs;
            _nDirsWithFiles = nDirsWithFiles;
            _nHashParity = nHashParity;
        }

        public int CompareTo(object obj)
        {
            var that = (FolderKeyStruct)obj;

            if (this > that) return -1;             // reverse sort
            if (this == that) return 0;
            return 1;
        }

        public override bool Equals(object obj)
        {
            // The as operator must be used with a reference type or nullable type
            // ('DoubleFile.FolderKeyStruct' is a non-nullable value type)

            if ((obj is FolderKeyStruct) == false)
                return false;

            return (((FolderKeyStruct)obj) == this);
        }

        public override int GetHashCode()
        {
            // overflow mixes the bits
            int nHashCode = 37;     // prime
            nHashCode *= 397;       // prime
            nHashCode += (int)_nTotalLength;
            nHashCode *= 397;
            nHashCode += (int)(_nTotalLength >> 32);
            nHashCode *= 397;
            nHashCode += (int)_nFilesInSubdirs;
            nHashCode *= 397;
            nHashCode += (int)_nDirsWithFiles;
            nHashCode *= 397;
            nHashCode += _nHashParity;
            nHashCode *= 397;
            return nHashCode;
        }

        public override string ToString()
        {
            return "_nTotalLength: " + _nTotalLength + "\n" +
                "_nFilesInSubdirs: " + _nFilesInSubdirs + "\n" +
                "_nDirsWithFiles: " + _nDirsWithFiles + "\n" +
                "_nHashParity: " + _nHashParity + "\n";
        }

        public static bool operator ==(FolderKeyStruct x, FolderKeyStruct y)
        {
            return (x._nTotalLength == y._nTotalLength) &&
                (x._nFilesInSubdirs == y._nFilesInSubdirs) &&
                (x._nDirsWithFiles == y._nDirsWithFiles) &&
                (x._nHashParity == y._nHashParity);
        }

        public static bool operator >(FolderKeyStruct x, FolderKeyStruct y)
        {
            if (x._nTotalLength < y._nTotalLength) return false;
            if (x._nTotalLength > y._nTotalLength) return true;
            if (x._nFilesInSubdirs < y._nFilesInSubdirs) return false;
            if (x._nFilesInSubdirs > y._nFilesInSubdirs) return true;
            if (x._nDirsWithFiles < y._nDirsWithFiles) return false;
            if (x._nDirsWithFiles > y._nDirsWithFiles) return true;
            if (x._nHashParity < y._nHashParity) return false;
            if (x._nHashParity > y._nHashParity) return true;
            return false;
        }

        public static bool operator !=(FolderKeyStruct x, FolderKeyStruct y) { return ((x == y) == false); }
        public static bool operator <(FolderKeyStruct x, FolderKeyStruct y) { return ((x >= y) == false); }
        public static bool operator >=(FolderKeyStruct x, FolderKeyStruct y) { return ((x > y) || (x == y)); }
        public static bool operator <=(FolderKeyStruct x, FolderKeyStruct y) { return ((x > y) == false); }

        readonly ulong _nTotalLength;       //  found   41 bits
        readonly uint _nFilesInSubdirs;     //          23 bits
        readonly uint _nDirsWithFiles;      //          16 bits
        readonly int _nHashParity;
    }
}
