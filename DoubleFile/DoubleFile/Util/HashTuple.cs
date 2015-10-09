using System;
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
        public override string
            ToString() => Item1.ToString("X8").PadLeft(16, '0') + Item2.ToString("X8").PadLeft(16, '0');

        static internal int
            HashCodeFromString(string strHash, string strFileLength) =>
            HashCodeFromString(strHash, ("" + strFileLength).ToUlong());
        static internal int
            HashCodeFromString(string strHash, ulong nFileLength)
        {
            try
            {
                var abHash = new byte[16];

                for (var i = 0; i < 32; i += 2)
                    abHash[15 - (i >> 1)] = Convert.ToByte(strHash.Substring(i, 2), 16);

                if (null == abHash)
                    return 0;

                unsafe
                {
                    fixed (byte* n12 = &abHash[12])
                    fixed (byte* n8 = &abHash[8])
                    fixed (byte* n4 = &abHash[4])
                    fixed (byte* n0 = &abHash[0])
                        return *((int*)n12) + *((int*)n8) + *((int*)n4) + *((int*)n0) + (int)nFileLength * 37;
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
    }
}
