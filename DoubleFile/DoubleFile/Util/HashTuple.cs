using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace DoubleFile
{
    class HashTuple : Tuple<ulong, ulong>
    {
        HashTuple(ulong nHash0, ulong nHash1)
            : base(nHash0, nHash1)
        {
        }

        static HashTuple()
        {
            LV_ProjectVM.Modified.LocalSubscribe(99597, x =>
            {
                _dictLookup = new ConcurrentDictionary<Tuple<ulong, ulong, ulong>, int>();
                _nFileIndexedID = 0;
            });
        }

        static internal HashTuple
            FactoryCreate(IReadOnlyList<byte> abHash_in)    // only called by SaveDirListings
        {
            var nIx = 16;
            var abHash = new byte[nIx];

            foreach (var b in abHash_in)
                abHash[--nIx] = b;

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
            FileIndexedIDfromString(string strHash, string strFileLength) =>
            FileIndexedIDfromString(strHash, ("" + strFileLength).ToUlong());
        static internal int
            FileIndexedIDfromString(string strHash, ulong nFileLength)
        {
            try
            {
                var abHash_in = new byte[16];

                for (var i = 0; i < 32; i += 2)
                    abHash_in[15 - (i >> 1)] = Convert.ToByte(strHash.Substring(i, 2), 16);

                var nIx = 16;
                var abHash = new byte[nIx];

                foreach (var b in abHash_in)
                    abHash[--nIx] = b;

                unsafe
                {
                    fixed (byte* n8 = &abHash[8])
                    fixed (byte* n0 = &abHash[0])
                        return _dictLookup.GetOrAdd(Tuple.Create(*((ulong*)n8), *((ulong*)n0), nFileLength), Interlocked.Increment(ref _nFileIndexedID));
                }
            }
            catch (ArgumentException)
            {
                Util.Assert(99935, false);
            }
            catch (FormatException)
            {
                Util.Assert(99935, false);
            }

            return 0;
        }

        static ConcurrentDictionary<Tuple<ulong, ulong, ulong>, int>
            _dictLookup = new ConcurrentDictionary<Tuple<ulong, ulong, ulong>, int>();
        static int
            _nFileIndexedID = 0;        // first file ID is 1: zero is not valid: no unchecked/overflow/wraparound (1.5M is reasonable)
    }
}
