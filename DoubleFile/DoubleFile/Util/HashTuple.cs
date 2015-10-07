using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DoubleFile
{
    class HashTuple : Tuple<ulong, ulong>
    {
        HashTuple(ulong nHash0, ulong nHash1)
            : base(nHash0, nHash1)
        {
        }

        static internal HashTuple
            FactoryCreate(IReadOnlyList<byte> abHash_in)    // only called by SaveDirListings
        {
            var nIx = 16;
            var abHash = new byte[nIx];

            foreach (var b in abHash_in)
                abHash[--nIx] = b;

            if (null == abHash)
                return null;

            unsafe
            {
                fixed (byte* n8 = &abHash[8])
                fixed (byte* n0 = &abHash[0])
                    return new HashTuple(*((ulong*)n8), *((ulong*)n0));
            }
        }

        static internal int
            HashcodeFromString(string strHash) =>           // called by TreeRootNodeBuilder; FileKeyTuple; and UC_CompareVM
            _dictFileHashID.GetOrAdd(strHash, x => ++_nFileHashID);

        static byte[]
            ConvertToByte(string strHash)
        {
            try
            {
                var abHash = new byte[16];

                for (var i = 0; i < 32; i += 2)
                    abHash[15 - (i >> 1)] = Convert.ToByte(strHash.Substring(i, 2), 16);

                return abHash;
            }
            catch (ArgumentException)
            {
                Util.Assert(99935, false);
            }
            catch (FormatException)
            {
                Util.Assert(99935, false);
            }

            return null;
        }

        public override string ToString() =>
            Item1.ToString("X8").PadLeft(16, '0') + Item2.ToString("X8").PadLeft(16, '0');

        static ConcurrentDictionary<string, int>
            _dictFileHashID = new ConcurrentDictionary<string, int>();
        static int
            _nFileHashID = -1;
    }
}
