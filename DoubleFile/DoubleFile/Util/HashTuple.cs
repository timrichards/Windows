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

        static internal HashTuple FactoryCreate(IReadOnlyList<byte> abHash_in)
        {
            var nIx = 16;
            var abHash = new byte[nIx];

            foreach (var b in abHash_in)
                abHash[--nIx] = b;

            return Create(abHash);
        }

        static internal HashTuple FactoryCreate(string strHash)
        {
            var abHash = new byte[16];

            try
            {
                for (var i = 0; i < 32; i += 2)
                    abHash[15 - (i >> 1)] = Convert.ToByte(strHash.Substring(i, 2), 16);

                return Create(abHash);
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

        static HashTuple Create(byte[] abHash)
        {
            unsafe
            {
                fixed (byte* n8 = &abHash[8])
                fixed (byte* n0 = &abHash[0])
                    return new HashTuple(*((ulong*)n8), *((ulong*)n0));
            }
        }

        public override string ToString()
        {
            return Item1.ToString("X8").PadLeft(16, '0') + Item2.ToString("X8").PadLeft(16, '0');
        }
    }
}
