using System;
using System.Linq;

namespace DoubleFile
{
    partial class FileDictionary
    {
        class FileKey : IComparable
        {
            internal byte[] abHash;
            internal ulong nLength;
            int nHashCode = 0;

            internal FileKey(string deSerialize)
            {
                var asKey = deSerialize.Split(' ');

                Init(asKey[0], asKey[1]);
            }

            internal FileKey(string strHash, string strLength)
            {
                Init(strHash, strLength);
            }

            public int CompareTo(object obj)
            {
                FileKey that = (FileKey)obj;

                if (this > that) return -1;             // reverse sort
                if (this == that) return 0;
                return 1;
            }

            public override bool Equals(object obj)
            {
                if ((obj is FileKey) == false)
                    return false;

                return (((FileKey)obj) == this);
            }

            public override int GetHashCode()
            {
                return nHashCode;
            }

            public override string ToString()
            {
                return DRDigit.Fast.ToHexString(abHash) + " " + nLength;
            }

            public static bool operator ==(FileKey x, FileKey y)
            {
                if (x.nLength != y.nLength)
                    return false;

                for (int i = 0; i < x.abHash.Length; ++i)
                {
                    if (x.abHash[i] != y.abHash[i])
                        return false;
                }

                return true;
            }

            public static bool operator >(FileKey x, FileKey y)
            {
                for (int i = 0; i < x.abHash.Length; ++i)
                {
                    if (x.abHash[i] < y.abHash[i])
                        return false;

                    if (x.abHash[i] > y.abHash[i])
                        return true;
                }

                if (x.nLength < y.nLength) return false;
                if (x.nLength > y.nLength) return true;
                return false;
            }

            public static bool operator !=(FileKey x, FileKey y) { return ((x == y) == false); }
            public static bool operator <(FileKey x, FileKey y) { return ((x >= y) == false); }
            public static bool operator >=(FileKey x, FileKey y) { return ((x > y) || (x == y)); }
            public static bool operator <=(FileKey x, FileKey y) { return ((x > y) == false); }

            void Init(string strHash, string strLength)
            {
                MBox.Assert(0, strHash.Length == 32);

                abHash = Enumerable.Range(0, strHash.Length)
                    .Where(x => x % 2 == 0)
                    .Select(x => Convert.ToByte(strHash.Substring(x, 2), 16))
                    .ToArray();
                nLength = ulong.Parse(strLength);

                // overflow mixes the bits
                nHashCode = 37;         // prime
                nHashCode *= 397;       // prime

                foreach (var b in abHash)
                {
                    nHashCode += b;
                    nHashCode *= 397;
                }

                nHashCode += (int)nLength;
                nHashCode *= 397;
                nHashCode += (int)(nLength >> 32);
                nHashCode *= 397;
            }
        }
    }
}
