using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace DoubleFile
{
    static class UtilColor
    {
        readonly static internal int Empty = Color.Empty.ToArgb();
        readonly static internal int White = Color.White.ToArgb();
        readonly static internal int Blue = Color.Blue.ToArgb();
        readonly static internal int DarkBlue = Color.DarkBlue.ToArgb();
        readonly static internal int DarkGray = Color.DarkGray.ToArgb();
        readonly static internal int DarkOrange = Color.DarkOrange.ToArgb();
        readonly static internal int DarkRed = Color.DarkRed.ToArgb();
        readonly static internal int DarkSlateGray = Color.DarkSlateGray.ToArgb();
        readonly static internal int Firebrick = Color.Firebrick.ToArgb();
        readonly static internal int LightGoldenrodYellow = Color.LightGoldenrodYellow.ToArgb();
        readonly static internal int LightGray = Color.LightGray.ToArgb();
        readonly static internal int MediumSpringGreen = Color.MediumSpringGreen.ToArgb();
        readonly static internal int MediumVioletRed = Color.MediumVioletRed.ToArgb();
        readonly static internal int OliveDrab = Color.OliveDrab.ToArgb();
        readonly static internal int Red = Color.Red.ToArgb();
        readonly static internal int Snow = Color.Snow.ToArgb();
        readonly static internal int SteelBlue = Color.SteelBlue.ToArgb();

        static internal int Set_ARGB(int fg, int bg)
        {
            int ret = 0;

            SetFG_ARGB(ref ret, fg);
            SetBG_ARGB(ref ret, bg);
            return ret;
        }

        static internal int GetFG_ARGB(int n) { return CLUT[(n & knCLUT_FGmask)]; }
        static internal int GetBG_ARGB(int n) { return CLUT[(n & knCLUT_BGmask) >> 8]; }
        static internal int SetFG_ARGB(ref int n, int argb) { return n = (int)(n & knCLUT_BGmask) + RevCLUT[argb]; }
        static internal int SetBG_ARGB(ref int n, int argb) { return n = (int)(n & knCLUT_FGmask) + (RevCLUT[argb] << 8); }

        readonly static int[] CLUT = new int[knNumColors]
        {
            Empty, White, Blue, DarkBlue, DarkGray, DarkOrange, DarkRed, DarkSlateGray,
            Firebrick, LightGoldenrodYellow, LightGray, MediumSpringGreen, MediumVioletRed,
            OliveDrab, Red, Snow, SteelBlue
        };

        static UtilColor()
        {
            int nIx = 0;

            RevCLUT[Empty] = nIx++;
            RevCLUT[White] = nIx++;
            RevCLUT[Blue] = nIx++;
            RevCLUT[DarkBlue] = nIx++;
            RevCLUT[DarkGray] = nIx++;
            RevCLUT[DarkOrange] = nIx++;
            RevCLUT[DarkRed] = nIx++;
            RevCLUT[DarkSlateGray] = nIx++;
            RevCLUT[Firebrick] = nIx++;
            RevCLUT[LightGoldenrodYellow] = nIx++;
            RevCLUT[LightGray] = nIx++;
            RevCLUT[MediumSpringGreen] = nIx++;
            RevCLUT[MediumVioletRed] = nIx++;
            RevCLUT[OliveDrab] = nIx++;
            RevCLUT[Red] = nIx++;
            RevCLUT[Snow] = nIx++;
            RevCLUT[SteelBlue] = nIx++;

            MBoxStatic.Assert(0, nIx == knNumColors);
        }

        internal static System.Windows.Media.Brush ARGBtoBrush(int nFormsARGB)
        {
            var abARGB = BitConverter.GetBytes(nFormsARGB);

            return new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromArgb(abARGB[3], abARGB[2], abARGB[1], abARGB[0])
            );
        }

        const int knNumColors = 17;

        const uint knCLUT_FGmask = 0x000000FF;
        const uint knCLUT_BGmask = 0x0000FF00;

        static Dictionary<int, int> RevCLUT = new Dictionary<int, int>();
    }
}
