using System;

namespace DoubleFile
{
    internal struct FileKeyStruct : IComparable
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

                _hash = new HashStruct(strHash);
                _nLength = ulong.Parse(strLength);
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
                nHashCode += _hash.GetHashCode();
                nHashCode *= 397;
                nHashCode += (int)_nLength;
                nHashCode *= 397;
                nHashCode += (int)(_nLength >> 32);
                nHashCode *= 397;
                return nHashCode;
            }

            public override string ToString()
            {
                return _hash + " " + _nLength;
            }

            public static bool operator ==(FileKeyStruct x, FileKeyStruct y)
            {
                if (x._nLength != y._nLength)
                    return false;

                return (x._hash == y._hash);
            }

            public static bool operator >(FileKeyStruct x, FileKeyStruct y)
            {
                if (x._nLength < y._nLength) return false;
                if (x._nLength > y._nLength) return true;

                if (x._hash < y._hash) return false;
                if (x._hash > y._hash) return true;

                return false;
            }

            public static bool operator !=(FileKeyStruct x, FileKeyStruct y) { return ((x == y) == false); }
            public static bool operator <(FileKeyStruct x, FileKeyStruct y) { return ((x >= y) == false); }
            public static bool operator >=(FileKeyStruct x, FileKeyStruct y) { return ((x > y) || (x == y)); }
            public static bool operator <=(FileKeyStruct x, FileKeyStruct y) { return ((x > y) == false); }

            readonly HashStruct _hash;
            readonly ulong _nLength;
        }
}
