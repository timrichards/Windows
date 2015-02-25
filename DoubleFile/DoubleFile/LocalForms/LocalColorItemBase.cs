
namespace DoubleFile
{
    abstract class LocalColorItemBase
    {
        internal int BackColor { get { return UtilColor.GetBG_ARGB(Color); } set { int c = Color; Color = UtilColor.SetBG_ARGB(ref c, value); } }
        internal int ForeColor { get { return UtilColor.GetFG_ARGB(Color); } set { int c = Color; Color = UtilColor.SetFG_ARGB(ref c, value); } }

        internal LocalColorItemBase()
        {
            Color = UtilColor.Set_ARGB(UtilColor.Empty, UtilColor.Empty);
        }

        protected int Datum6bits
        {
            get { return (int)(_datum & _knDatum6bitMask) >> 10; }
            set { _datum = (int)(_datum & (-1 - _knDatum6bitMask)) + (value << 10); }
        }

        protected int Datum16bits
        {
            get { return (int)(_datum & _knDatum16bitMask) >> 16; }
            set { _datum = (int)(_datum & (-1 - _knDatum16bitMask)) + (value << 16); }
        }

        int Color
        {
            get { return (int)(_datum & UtilColor.knCLUTmask); }
            set { _datum = (int)(_datum & (-1 - UtilColor.knCLUTmask)) + value; }
        }

        const uint _knDatum6bitMask =  0x0000FFFF - UtilColor.knCLUTmask;
        const uint _knDatum16bitMask = 0xFFFF0000;

        int _datum = 0;
    }
}
