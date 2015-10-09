using System;

namespace DoubleFile
{
    internal class FileKeyTuple : Tuple<int, ulong>
    {
        FileKeyTuple(int nHash, ulong nLength)
            : base(nHash, nLength)
        {
        }

        static internal FileKeyTuple FactoryCreate(string strHash, ulong nLength)
        {
            if (0 == nLength)
                return null;

            return new FileKeyTuple(HashTuple.HashCodeFromString(strHash, nLength), nLength);
        }
    }
 }
