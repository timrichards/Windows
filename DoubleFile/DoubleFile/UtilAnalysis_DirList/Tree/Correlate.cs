using System;

namespace DoubleFile
{
    class Correlate : IComparable
    {
        internal Correlate(ulong nTotalLength_in, uint nFilesInSubdirs_in, uint nDirsWithFiles_in)
        {
            nTotalLength = nTotalLength_in;
            nFilesInSubdirs = nFilesInSubdirs_in;
            nDirsWithFiles = nDirsWithFiles_in;
        }

        public int CompareTo(object obj)
        {
            Correlate that = (Correlate)obj;

            if (this > that) return -1;             // reverse sort
            if (this == that) return 0;
            return 1;
        }

        public override bool Equals(object obj)
        {
            if ((obj is Correlate) == false) return false;
            return (((Correlate)obj) == this);
        }

        public override int GetHashCode()
        {
            // any overflow mixes the bits a bit better.
            int result = 37;    // prime

            result *= 397;      // also prime
            result += nTotalLength.GetHashCode();
            result *= 397;
            result += nFilesInSubdirs.GetHashCode();
            result *= 397;
            result += nDirsWithFiles.GetHashCode();
            result *= 397;
            return result;
        }

        public override string ToString()
        {
            return "nTotalLength: " + nTotalLength + "\n" +
                "nFilesInSubdirs: " + nFilesInSubdirs + "\n" +
                "nDirsWithFiles: " + nDirsWithFiles + "\n";
        }

        public static bool operator ==(Correlate x, Correlate y)
        {
            return (x.nTotalLength == y.nTotalLength) && (x.nFilesInSubdirs == y.nFilesInSubdirs) && (x.nDirsWithFiles == y.nDirsWithFiles);
        }

        public static bool operator >(Correlate x, Correlate y)
        {
            if (x.nTotalLength < y.nTotalLength) return false;
            if (x.nTotalLength > y.nTotalLength) return true;
            if (x.nFilesInSubdirs < y.nFilesInSubdirs) return false;
            if (x.nFilesInSubdirs > y.nFilesInSubdirs) return true;
            if (x.nDirsWithFiles < y.nDirsWithFiles) return false;
            if (x.nDirsWithFiles > y.nDirsWithFiles) return true;
            return false;
        }

        public static bool operator !=(Correlate x, Correlate y) { return ((x == y) == false); }
        public static bool operator <(Correlate x, Correlate y) { return ((x >= y) == false); }
        public static bool operator >=(Correlate x, Correlate y) { return ((x > y) || (x == y)); }
        public static bool operator <=(Correlate x, Correlate y) { return ((x > y) == false); }

        readonly ulong nTotalLength;       //  found   41 bits
        readonly uint nFilesInSubdirs;     //          23 bits
        readonly uint nDirsWithFiles;      //          16 bits
    }
}
