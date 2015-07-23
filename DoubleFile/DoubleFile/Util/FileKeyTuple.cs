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

            return (null != hashTuple)
                ? new FileKeyTuple(hashTuple, nLength)
                : null;
        }
    }
 }
