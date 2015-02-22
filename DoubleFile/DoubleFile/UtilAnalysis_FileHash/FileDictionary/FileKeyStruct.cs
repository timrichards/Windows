using System;

namespace DoubleFile
{
    partial class FileDictionary
    {
        struct FileKeyStruct : IComparable
        {
            /// <summary>
            /// Parameter trickery allows one constructor and readonly member vars.
            /// </summary>
            /// <param name="deSerialize">when strLength is provided, this is strHash</param>
            /// <param name="strLength">only used when not deserializing</param>
            internal FileKeyStruct(string deSerialize, string strLength = null)
            {
                var strHash = deSerialize;

                if (strLength == null)
                {
                    var asKey = deSerialize.Split(' ');

                    strHash = asKey[0];
                    strLength = asKey[1];
                }

                m_hash = new HashStruct(strHash);
                m_nLength = ulong.Parse(strLength);
            }

            public int CompareTo(object obj)
            {
                var that = (FileKeyStruct)obj;

                if (this > that) return -1;             // reverse sort
                if (this == that) return 0;
                return 1;
            }

            public override bool Equals(object obj)
            {
                // The as operator must be used with a reference type or nullable type
                // ('DoubleFile.FileDictionary.FileKeyStruct' is a non-nullable value type)

                if ((obj is FileKeyStruct) == false)
                    return false;

                return (((FileKeyStruct)obj) == this);
            }

            public override int GetHashCode()
            {
                // overflow mixes the bits
                int nHashCode = 37;         // prime
                nHashCode *= 397;       // prime
                nHashCode += m_hash.GetHashCode();
                nHashCode *= 397;
                nHashCode += (int)m_nLength;
                nHashCode *= 397;
                nHashCode += (int)(m_nLength >> 32);
                nHashCode *= 397;
                return nHashCode;
            }

            public override string ToString()
            {
                return m_hash + " " + m_nLength;
            }

            public static bool operator ==(FileKeyStruct x, FileKeyStruct y)
            {
                if (x.m_nLength != y.m_nLength)
                    return false;

                return (x.m_hash == y.m_hash);
            }

            public static bool operator >(FileKeyStruct x, FileKeyStruct y)
            {
                if (x.m_nLength < y.m_nLength) return false;
                if (x.m_nLength > y.m_nLength) return true;

                if (x.m_hash < y.m_hash) return false;
                if (x.m_hash > y.m_hash) return true;

                return false;
            }

            public static bool operator !=(FileKeyStruct x, FileKeyStruct y) { return ((x == y) == false); }
            public static bool operator <(FileKeyStruct x, FileKeyStruct y) { return ((x >= y) == false); }
            public static bool operator >=(FileKeyStruct x, FileKeyStruct y) { return ((x > y) || (x == y)); }
            public static bool operator <=(FileKeyStruct x, FileKeyStruct y) { return ((x > y) == false); }

            readonly HashStruct m_hash;
            readonly ulong m_nLength;
        }
    }
}
