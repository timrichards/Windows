﻿using System;

namespace DoubleFile
{
    class Correlate : IComparable
    {
        internal Correlate(ulong nTotalLength_in, uint nFilesInSubdirs_in, uint nDirsWithFiles_in)
        {
            m_nTotalLength = nTotalLength_in;
            m_nFilesInSubdirs = nFilesInSubdirs_in;
            m_nDirsWithFiles = nDirsWithFiles_in;

            // overflow mixes the bits
            m_nHashCode = 37;       // prime
            m_nHashCode *= 397;     // prime
            m_nHashCode += (int)m_nTotalLength;
            m_nHashCode *= 397;
            m_nHashCode += (int)(m_nTotalLength >> 32);
            m_nHashCode *= 397;
            m_nHashCode += (int)m_nFilesInSubdirs;
            m_nHashCode *= 397;
            m_nHashCode += (int)m_nDirsWithFiles;
            m_nHashCode *= 397;
        }

        public int CompareTo(object obj)
        {
            var that = (Correlate)obj;

            if (this > that) return -1;             // reverse sort
            if (this == that) return 0;
            return 1;
        }

        public override bool Equals(object obj)
        {
            if ((obj is Correlate) == false)
                return false;

            return (((Correlate)obj) == this);
        }

        public override int GetHashCode()
        {
            return m_nHashCode;
        }

        public override string ToString()
        {
            return "nTotalLength: " + m_nTotalLength + "\n" +
                "nFilesInSubdirs: " + m_nFilesInSubdirs + "\n" +
                "nDirsWithFiles: " + m_nDirsWithFiles + "\n";
        }

        public static bool operator ==(Correlate x, Correlate y)
        {
            return (x.m_nTotalLength == y.m_nTotalLength) &&
                (x.m_nFilesInSubdirs == y.m_nFilesInSubdirs) &&
                (x.m_nDirsWithFiles == y.m_nDirsWithFiles);
        }

        public static bool operator >(Correlate x, Correlate y)
        {
            if (x.m_nTotalLength < y.m_nTotalLength) return false;
            if (x.m_nTotalLength > y.m_nTotalLength) return true;
            if (x.m_nFilesInSubdirs < y.m_nFilesInSubdirs) return false;
            if (x.m_nFilesInSubdirs > y.m_nFilesInSubdirs) return true;
            if (x.m_nDirsWithFiles < y.m_nDirsWithFiles) return false;
            if (x.m_nDirsWithFiles > y.m_nDirsWithFiles) return true;
            return false;
        }

        public static bool operator !=(Correlate x, Correlate y) { return ((x == y) == false); }
        public static bool operator <(Correlate x, Correlate y) { return ((x >= y) == false); }
        public static bool operator >=(Correlate x, Correlate y) { return ((x > y) || (x == y)); }
        public static bool operator <=(Correlate x, Correlate y) { return ((x > y) == false); }

        readonly ulong m_nTotalLength = 0;       //  found   41 bits
        readonly uint m_nFilesInSubdirs = 0;     //          23 bits
        readonly uint m_nDirsWithFiles = 0;      //          16 bits
        readonly int m_nHashCode = 0;
    }
}
