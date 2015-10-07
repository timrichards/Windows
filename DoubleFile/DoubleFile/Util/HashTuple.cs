using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DoubleFile
{
    class HashTuple : Tuple<ulong, ulong>
    {
        static HashTuple()
        {
            LV_ProjectVM.Modified
                .LocalSubscribe(99612, x =>
            {
                _dictFileHashID = new ConcurrentDictionary<string, int>();
                _nFileHashID = -1;
            });
        }

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
        public override string
            ToString() => Item1.ToString("X8").PadLeft(16, '0') + Item2.ToString("X8").PadLeft(16, '0');

        static internal int
            FileIndexedIDFromString(string strHash) => _dictFileHashID.GetOrAdd(strHash, x => ++_nFileHashID);
        static ConcurrentDictionary<string, int>
            _dictFileHashID = new ConcurrentDictionary<string, int>();
        static int
            _nFileHashID = -1;
    }
}
