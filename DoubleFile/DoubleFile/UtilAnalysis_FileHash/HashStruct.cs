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
            n0 = BitConverter.ToUInt64(abHash, 8);
            n1 = BitConverter.ToUInt64(abHash, 0);
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
            if ((obj is HashStruct) == false)
                return false;

            return (((HashStruct)obj) == this);
        }

        public override int GetHashCode()
        {
            var hashCode = (int)n0;
            hashCode *= 397;        // prime
            hashCode += (int)(n0 >> 32);
            hashCode *= 397;
            hashCode += (int)n1;
            hashCode *= 397;
            hashCode += (int)(n1 >> 32);
            return hashCode;
        }

        public override string ToString()
        {
            var strRet = n0.ToString("X8").PadLeft(16, '0') + n1.ToString("X8").PadLeft(16, '0');
            return strRet;
        }

        public static bool operator ==(HashStruct x, HashStruct y)
        {
            if (x.n0 != y.n0)
                return false;

            if (x.n1 != y.n1)
                return false;

            return true;
        }

        public static bool operator >(HashStruct x, HashStruct y)
        {
            if (x.n0 < y.n0) return false;
            if (x.n0 > y.n0) return true;

            if (x.n1 < y.n1) return false;
            if (x.n1 > y.n1) return true;
            return false;
        }

        public static bool operator !=(HashStruct x, HashStruct y) { return ((x == y) == false); }
        public static bool operator <(HashStruct x, HashStruct y) { return ((x >= y) == false); }
        public static bool operator >=(HashStruct x, HashStruct y) { return ((x > y) || (x == y)); }
        public static bool operator <=(HashStruct x, HashStruct y) { return ((x > y) == false); }

        ulong n0;
        ulong n1;
    }
}
