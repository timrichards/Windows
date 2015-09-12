using System;

namespace DoubleFile
{
    internal class FileKeyTuple : Tuple<HashTuple, ulong>
    {
        FileKeyTuple(HashTuple hashTuple, ulong nLength)
            : base(hashTuple, nLength)
        {
        }

        static internal FileKeyTuple FactoryCreate(string strHash, ulong nLength)
        {
            var hashTuple = HashTuple.FactoryCreate(strHash);

            if (0 == nLength)
                return null;

            return
                (null != hashTuple)
                ? new FileKeyTuple(hashTuple, nLength)
                : null;
        }
    }
 }
