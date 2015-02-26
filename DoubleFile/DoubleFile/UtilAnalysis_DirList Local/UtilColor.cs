using System;
using System.Collections.Generic;
using System.Drawing;

namespace DoubleFile
{
    static class UtilColor
    {
        internal static int Empty { get { return Color.Empty.ToArgb(); } }
        internal static int White { get { return Color.White.ToArgb(); } }
        internal static int Blue { get { return Color.Blue.ToArgb(); } }
        internal static int DarkBlue { get { return Color.DarkBlue.ToArgb(); } }
        internal static int DarkGray { get { return Color.DarkGray.ToArgb(); } }
        internal static int DarkOrange { get { return Color.DarkOrange.ToArgb(); } }
        internal static int DarkRed { get { return Color.DarkRed.ToArgb(); } }
        internal static int DarkSlateGray { get { return Color.DarkSlateGray.ToArgb(); } }
        internal static int Firebrick { get { return Color.Firebrick.ToArgb(); } }
        internal static int LightGoldenrodYellow { get { return Color.LightGoldenrodYellow.ToArgb(); } }
        internal static int LightGray { get { return Color.LightGray.ToArgb(); } }
        internal static int MediumSpringGreen { get { return Color.MediumSpringGreen.ToArgb(); } }
        internal static int MediumVioletRed { get { return Color.MediumVioletRed.ToArgb(); } }
        internal static int OliveDrab { get { return Color.OliveDrab.ToArgb(); } }
        internal static int Red { get { return Color.Red.ToArgb(); } }
        internal static int Snow { get { return Color.Snow.ToArgb(); } }
        internal static int SteelBlue { get { return Color.SteelBlue.ToArgb(); } }

        internal static uint
            CLUT_Mask { get { return 0x000003FF; } }
        internal static int
            CLUT_Shift { get { return (int)Math.Log(CLUT_Mask + 1, 2); } }

        internal static int Set_ARGB(int fg, int bg)
        {
            int ret = 0;

            SetFG_ARGB(ref ret, fg);
            SetBG_ARGB(ref ret, bg);
            return ret;
        }

        internal static int GetFG_ARGB(int n) { return CLUT[(n & _knCLUT_FGmask)]; }
        internal static int GetBG_ARGB(int n) { return CLUT[(n & _knCLUT_BGmask) >> 5]; }
        internal static int SetFG_ARGB(ref int n, int argb) { return n = (int)(n & _knCLUT_BGmask) + _RevCLUT[argb]; }
        internal static int SetBG_ARGB(ref int n, int argb) { return n = (int)(n & _knCLUT_FGmask) + (_RevCLUT[argb] << 5); }

        internal static System.Windows.Media.Brush ARGBtoBrush(int nFormsARGB)
        {
            var abARGB = BitConverter.GetBytes(nFormsARGB);

            return new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromArgb(abARGB[3], abARGB[2], abARGB[1], abARGB[0])
            );
        }

        readonly static int[] CLUT = new int[_knNumColors]
        {
            Empty, White, Blue, DarkBlue, DarkGray, DarkOrange, DarkRed, DarkSlateGray,
            Firebrick, LightGoldenrodYellow, LightGray, MediumSpringGreen, MediumVioletRed,
            OliveDrab, Red, Snow, SteelBlue
        };

        static UtilColor()
        {
            int nIx = 0;

            _RevCLUT[Empty] = nIx++;
            _RevCLUT[White] = nIx++;
            _RevCLUT[Blue] = nIx++;
            _RevCLUT[DarkBlue] = nIx++;
            _RevCLUT[DarkGray] = nIx++;
            _RevCLUT[DarkOrange] = nIx++;
            _RevCLUT[DarkRed] = nIx++;
            _RevCLUT[DarkSlateGray] = nIx++;
            _RevCLUT[Firebrick] = nIx++;
            _RevCLUT[LightGoldenrodYellow] = nIx++;
            _RevCLUT[LightGray] = nIx++;
            _RevCLUT[MediumSpringGreen] = nIx++;
            _RevCLUT[MediumVioletRed] = nIx++;
            _RevCLUT[OliveDrab] = nIx++;
            _RevCLUT[Red] = nIx++;
            _RevCLUT[Snow] = nIx++;
            _RevCLUT[SteelBlue] = nIx++;

            MBoxStatic.Assert(99957, nIx == _knNumColors);
            MBoxStatic.Assert(99910, 16 > CLUT_Shift);
        }

        const int
            _knNumColors = 17;
        const uint
            _knCLUT_FGmask = 0x0000001F;
        static readonly uint
            _knCLUT_BGmask = CLUT_Mask - _knCLUT_FGmask;
        static Dictionary<int, int>
            _RevCLUT = new Dictionary<int, int>();
    }
}
