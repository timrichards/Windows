using System;

namespace DoubleFile
{
    internal class FileKeyTuple : Tuple<HashTuple, ulong>
    {
        internal FileKeyTuple(string strHash, ulong? nLength = null)
            : base(HashTuple.FactoryCreate(strHash), (null != nLength) ? nLength.Value : 0)
        {
        }
    }
 }
