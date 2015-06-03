using System;

namespace DoubleFile
{
    internal class FileKeyTuple : Tuple<HashTuple, ulong>
    {
        internal FileKeyTuple(string strHash, ulong nLength = 0)
            : base(HashTuple.FactoryCreate(strHash), nLength)
        {
        }
    }
 }
