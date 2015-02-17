using System;
using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    struct HashStruct : IComparable
    {
        internal HashStruct(IEnumerable<byte> abHash_in)
        {
            var abHash = abHash_in
                .Reverse()
                .ToArray();

            MBoxStatic.Assert(0, abHash.Length == 16);
            m_nHash0 = BitConverter.ToUInt64(abHash, 8);
            m_nHash1 = BitConverter.ToUInt64(abHash, 0);
        }

        internal HashStruct(string strHash)
            : this(Enumerable.Range(0, strHash.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(strHash.Substring(x, 2), 16)))
        {
        }

        public int CompareTo(object obj)
        {
            var that = (HashStruct)obj;

            if (this > that) return -1;             // reverse sort
            if (this == that) return 0;
            return 1;
        }

        public override bool Equals(object obj)
        {
            // The as operator must be used with a reference type or nullable type
            // ('DoubleFile.HashStruct' is a non-nullable value type)

            if ((obj is HashStruct) == false)
                return false;

            return (((HashStruct)obj) == this);
        }

        public override int GetHashCode()
        {
            var hashCode = (int)m_nHash0;
            hashCode *= 397;        // prime
            hashCode += (int)(m_nHash0 >> 32);
            hashCode *= 397;
            hashCode += (int)m_nHash1;
            hashCode *= 397;
            hashCode += (int)(m_nHash1 >> 32);
            return hashCode;
        }

        public override string ToString()
        {
            var strRet = m_nHash0.ToString("X8").PadLeft(16, '0') + m_nHash1.ToString("X8").PadLeft(16, '0');
            return strRet;
        }

        public static bool operator ==(HashStruct x, HashStruct y)
        {
            if (x.m_nHash0 != y.m_nHash0)
                return false;

            return (x.m_nHash1 == y.m_nHash1);
        }

        public static bool operator >(HashStruct x, HashStruct y)
        {
            if (x.m_nHash0 < y.m_nHash0) return false;
            if (x.m_nHash0 > y.m_nHash0) return true;

            if (x.m_nHash1 < y.m_nHash1) return false;
            if (x.m_nHash1 > y.m_nHash1) return true;
            return false;
        }

        public static bool operator !=(HashStruct x, HashStruct y) { return ((x == y) == false); }
        public static bool operator <(HashStruct x, HashStruct y) { return ((x >= y) == false); }
        public static bool operator >=(HashStruct x, HashStruct y) { return ((x > y) || (x == y)); }
        public static bool operator <=(HashStruct x, HashStruct y) { return ((x > y) == false); }

        readonly ulong m_nHash0;
        readonly ulong m_nHash1;
    }
}
