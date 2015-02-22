
namespace DoubleFile
{
    abstract class LocalColorItemBase
    {
        internal int BackColor { get { return UtilColor.GetBG_ARGB(_color); } set { UtilColor.SetBG_ARGB(ref _color, value); } }
        internal int ForeColor { get { return UtilColor.GetFG_ARGB(_color); } set { UtilColor.SetFG_ARGB(ref _color, value); } }

        int _color = UtilColor.Set_ARGB(UtilColor.Empty, UtilColor.Empty);
    }
}
