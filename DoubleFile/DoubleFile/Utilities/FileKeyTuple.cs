using System;

namespace DoubleFile
{
    internal class FileKeyTuple : Tuple<HashTuple, ulong>
    {
        internal FileKeyTuple(string strHash, string strLength = null)
            : base(HashTuple.FactoryCreate(strHash), ulong.Parse(strLength))
        {
        }
    }
 }
