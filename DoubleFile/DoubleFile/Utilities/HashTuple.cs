using System;
using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    class HashTuple : Tuple<ulong, ulong>
    {
        HashTuple(ulong nHash0, ulong nHash1)
            : base(nHash0, nHash1)
        {
        }

        internal static HashTuple FactoryCreate(IEnumerable<byte> abHash_in)
        {
            var abHash = abHash_in
                .Reverse()
                .ToArray();

            MBoxStatic.Assert(99955, abHash.Length == 16);

            return new HashTuple(BitConverter.ToUInt64(abHash, 8),
                BitConverter.ToUInt64(abHash, 0));
        }

        internal static HashTuple FactoryCreate(string strHash)
        {
            return FactoryCreate(Enumerable.Range(0, strHash.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(strHash.Substring(x, 2), 16)));
        }

        public override string ToString()
        {
            return Item1.ToString("X8").PadLeft(16, '0') + Item2.ToString("X8").PadLeft(16, '0');
        }
    }
}
