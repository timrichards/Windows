using System.Windows.Media;

namespace DoubleFile
{
    interface ILocalColorItemBase
    {
        int ForeColor { get; set; }
        int BackColor { get; set; }
    }

    class LocalColorItemBase_ClassObject : LocalColorItemBase
    {
        internal int Datum8bits_ClassObject { get { return Datum8bits; } set { Datum8bits = value; } }
        internal int Datum16bits_ClassObject { get { return Datum16bits; } set { Datum16bits = value; } }
    }

    abstract class LocalColorItemBase : ILocalColorItemBase
    {
        public int ForeColor { get { return UtilColor.GetFG_ARGB(Color); } set { int c = Color; Color = UtilColor.SetFG_ARGB(ref c, value); } }
        public int BackColor { get { return UtilColor.GetBG_ARGB(Color); } set { int c = Color; Color = UtilColor.SetBG_ARGB(ref c, value); } }

        internal Brush Foreground
        {
            get
            {
                return
                    (UtilColor.Empty == ForeColor)
                    ? Brushes.White
                    : UtilColor.ARGBtoBrush(ForeColor);
            }
        }

        internal Brush Background => UtilColor.ARGBtoBrush(BackColor);

        internal LocalColorItemBase()
        {
            Color = UtilColor.Set_ARGB(UtilColor.Empty, UtilColor.Empty);
        }

        protected int Datum8bits
        {
            get { return (int)(_datum & _knDatum8bitMask) >> UtilColor.CLUT_Shift; }
            set { _datum = (int)(_datum & (-1 - _knDatum8bitMask)) + (value << UtilColor.CLUT_Shift); }
        }

        protected int Datum16bits
        {
            get { return (int)(_datum & _knDatum16bitMask) >> 16; }
            set { _datum = (int)(_datum & (-1 - _knDatum16bitMask)) + (value << 16); }
        }

        int Color
        {
            get { return (int)(_datum & UtilColor.CLUT_Mask); }
            set { _datum = (int)(_datum & (-1 - UtilColor.CLUT_Mask)) + value; }
        }

        static readonly uint
            _knDatum8bitMask =  0xFFFF - UtilColor.CLUT_Mask;
        const uint
            _knDatum16bitMask = 0xFFFF0000;

        int _datum = 0;
    }
}
