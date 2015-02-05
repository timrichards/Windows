using System;

namespace DoubleFile
{
    partial class FileDictionary
    {
        class FileKey : IComparable
        {
            internal readonly HashStruct hash;
            internal readonly ulong nLength = 0;

            /// <summary>
            /// Parameter trickery allows one constructor and readonly member vars.
            /// </summary>
            /// <param name="deSerialize">when strLength is provided, this is strHash</param>
            /// <param name="strLength">only used when not deserializing</param>
            internal FileKey(string deSerialize, string strLength = null)
            {
                string strHash = deSerialize;

                if (strLength == null)
                {
                    var asKey = deSerialize.Split(' ');

                    strHash = asKey[0];
                    strLength = asKey[1];
                }

                hash = new HashStruct(strHash);
                nLength = ulong.Parse(strLength);

                // overflow mixes the bits
                m_nHashCode = 37;         // prime
                m_nHashCode *= 397;       // prime
                m_nHashCode += hash.GetHashCode();
                m_nHashCode *= 397;
                m_nHashCode += (int)nLength;
                m_nHashCode *= 397;
                m_nHashCode += (int)(nLength >> 32);
                m_nHashCode *= 397;
            }

            public int CompareTo(object obj)
            {
                var that = (FileKey)obj;

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
                return m_nHashCode;
            }

            public override string ToString()
            {
                return hash.ToString() + " " + nLength;
            }

            public static bool operator ==(FileKey x, FileKey y)
            {
                if (x.nLength != y.nLength)
                    return false;

                if (x.hash != y.hash)
                    return false;

                return true;
            }

            public static bool operator >(FileKey x, FileKey y)
            {
                if (x.nLength < y.nLength) return false;
                if (x.nLength > y.nLength) return true;

                if (x.hash < y.hash) return false;
                if (x.hash > y.hash) return true;

                return false;
            }

            public static bool operator !=(FileKey x, FileKey y) { return ((x == y) == false); }
            public static bool operator <(FileKey x, FileKey y) { return ((x >= y) == false); }
            public static bool operator >=(FileKey x, FileKey y) { return ((x > y) || (x == y)); }
            public static bool operator <=(FileKey x, FileKey y) { return ((x > y) == false); }

            int m_nHashCode = 0;
        }
    }
}
