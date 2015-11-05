namespace DoubleFile
{
    abstract class TabledStringBase
    {
        internal virtual int
            Set(string str_in) => Util.Assert(99919, false, bIfDefDebug: true) ? -1 : 0;
        internal virtual int
            CompareTo(int nIx, int thatIx) => Util.Assert(99918, false, bIfDefDebug: true) ? -1 : 0;
        internal virtual string
            Get(int nIndex) => "" + Util.Assert(99917, false, bIfDefDebug: true);
        internal abstract int
            IndexOf(string str);
    }
}
