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
                if ((obj is FileKey) == false) return false;
                return (((FileKey)obj) == this);
            }

            public override int GetHashCode()
            {
                // any overflow mixes the bits a bit better.
                int result = 37;    // prime
                result *= 397;      // also prime

                foreach (var b in abHash)
                {
                    result += b;
                    result *= 397;
                }

                result += nLength.GetHashCode();
                result *= 397;
                return result;
            }

            public override string ToString()
            {
                return DRDigit.Fast.ToHexString(abHash) + " " + nLength;
            }

            public static bool operator ==(FileKey x, FileKey y)
            {
                var q = from a in x.abHash
                        join b in y.abHash on a equals b
                        select a;

                return (x.abHash.Length == y.abHash.Length) &&
                    (q.Count() == x.abHash.Length) &&
                    (x.nLength == y.nLength);
            }

            public static bool operator >(FileKey x, FileKey y)
            {
                if (x.abHash.Length < y.abHash.Length) return false;
                if (x.abHash.Length > y.abHash.Length) return true;

                for (int n = 0; n < x.abHash.Length; ++n)
                {
                    if (x.abHash[n] < y.abHash[n])
                        return false;

                    if (x.abHash[n] > y.abHash[n])
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
                abHash = Enumerable.Range(0, strHash.Length)
                    .Where(x => x % 2 == 0)
                    .Select(x => Convert.ToByte(strHash.Substring(x, 2), 16))
                    .ToArray();
                nLength = ulong.Parse(strLength);
            }
        }
    }
}
