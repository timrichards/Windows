using System.Windows.Media;

namespace DoubleFile
{
    abstract class LocalColorItemBase
    {
        public int ForeColor { get { return UtilColorcode.GetFG_ARGB(Color); } set { int c = Color; Color = UtilColorcode.SetFG_ARGB(ref c, value); } }
        public int BackColor { get { return UtilColorcode.GetBG_ARGB(Color); } set { int c = Color; Color = UtilColorcode.SetBG_ARGB(ref c, value); } }

        internal Brush Foreground =>
            (UtilColorcode.Transparent == ForeColor)
            ? Brushes.White
            : UtilColorcode.ARGBtoBrush(ForeColor);

        internal Brush Background => UtilColorcode.ARGBtoBrush(BackColor);

        internal LocalColorItemBase()
        {
            Color = UtilColorcode.Set_ARGB(UtilColorcode.Transparent, UtilColorcode.Transparent);
        }

        protected int Datum8bits
        {
            get { return (int)(_datum & _knDatum8bitMask) >> UtilColorcode.CLUT_Shift; }
            set { _datum = (int)(_datum & (-1 - _knDatum8bitMask)) + (value << UtilColorcode.CLUT_Shift); }
        }

        protected int Datum16bits
        {
            get { return (int)(_datum & _knDatum16bitMask) >> 16; }
            set { _datum = (int)(_datum & (-1 - _knDatum16bitMask)) + (value << 16); }
        }

        int Color
        {
            get { return (int)(_datum & UtilColorcode.CLUT_Mask); }
            set { _datum = (int)(_datum & (-1 - UtilColorcode.CLUT_Mask)) + value; }
        }

        static readonly uint
            _knDatum8bitMask =  0xFFFF - UtilColorcode.CLUT_Mask;
        const uint
            _knDatum16bitMask = 0xFFFF0000;

        int _datum = 0;
    }
}
