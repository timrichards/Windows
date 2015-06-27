using System;

namespace DoubleFile
{
    internal class FileKeyTuple : Tuple<HashTuple, ulong>
    {
        FileKeyTuple(HashTuple hashTuple, ulong nLength = 0)
            : base(hashTuple, nLength)
        {
        }

        static internal FileKeyTuple FactoryCreate(string strHash, ulong nLength = 0)
        {
            var hashTuple = HashTuple.FactoryCreate(strHash);

            if (null == hashTuple)
                return null;

            return new FileKeyTuple(hashTuple, nLength);
        }
    }
 }
