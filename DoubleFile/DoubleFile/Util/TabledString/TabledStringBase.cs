namespace DoubleFile
{
    abstract class TabledStringBase
    {
        internal virtual int
            Set(string str_in) { Util.Assert(99919, false); return -1; }
        internal abstract int
            CompareTo(int nIx, int thatIx);
        internal abstract string
            Get(int index);
    }
}
