using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace DoubleFile
{
    static class UtilColor
    {
        static internal readonly IReadOnlyDictionary<int, string>
            Descriptions = new Dictionary<int, string>
        {
            {Transparent,       ""},
            {LightBlue,         "This folder has multiple copies on at least two separate volumes."},
            {DarkGray,          ""},						            // ignore list
            {DarkKhaki,         ""},						            // Treemap: Folder containing files
            {DarkRedBG,         "Contains folders that have no copy, or copies are on one volume."},
            {Firebrick,         "All copies of this folder reside on one volume."},
            {DarkYellowBG,      "This folder and its parent have a copy on a separate volume."},
            {LightGray,         "This folder has no data."},
            {MediumSpringGreen, ""},						            // Treemap: Free space
            {MediumVioletRed,   ""},						            // Treemap
            {OliveDrab,         ""},						            // Treemap: File
            {Red,               "This folder has no exact copy."},
            {SteelBlue,         "This folder has a copy on a separate volume."}
        };

        static internal int
            ToArgb(this Color color) => (color.A << 24) | (color.R << 16) | (color.G << 8) | color.B;
        static internal Color
            FromArgb(int argb) => Color.FromArgb((byte)(argb >> 24), (byte)(argb >> 16), (byte)(argb >> 8), (byte)(argb));
        static internal Color
            Dark(this Color color_)
        {
            var color = color_;

            color.R = (byte)(color.R * .3);
            color.G = (byte)(color.G * .3);
            color.B = (byte)(color.B * .3);
            return color;
        }

        static internal int Transparent => Colors.Transparent.ToArgb();
        static internal int LightBlue => Colors.LightBlue.ToArgb();
        static internal int DarkGray => Colors.DarkGray.ToArgb();
        static internal int DarkKhaki => Colors.DarkKhaki.ToArgb();
        static internal int DarkRedBG => Color.FromArgb(64, 64, 0, 0).ToArgb();
        static internal int Firebrick => Colors.Firebrick.ToArgb();
        static internal int DarkYellowBG => Color.FromArgb(64, 0, 64, 64).ToArgb();
        static internal int LightGray => Colors.LightGray.ToArgb();
        static internal int MediumSpringGreen => Colors.MediumSpringGreen.ToArgb();
        static internal int MediumVioletRed => Colors.MediumVioletRed.ToArgb();
        static internal int OliveDrab => Colors.OliveDrab.ToArgb();
        static internal int Red => Color.FromArgb(255, 192, 0, 0).ToArgb();
        static internal int SteelBlue => Colors.SteelBlue.ToArgb();

        internal const uint
            CLUT_Mask = 0xFF;
        static internal readonly int
            CLUT_Shift = (int)Math.Log(CLUT_Mask + 1, 2);

        static internal int Set_ARGB(int fg, int bg)
        {
            int ret = 0;

            SetFG_ARGB(ref ret, fg);
            SetBG_ARGB(ref ret, bg);
            return ret;
        }

        static internal int GetFG_ARGB(int n) => CLUT[(int)(n & _knCLUT_FGmask)];
        static internal int GetBG_ARGB(int n) => CLUT[(int)(n & _knCLUT_BGmask) >> (CLUT_Shift >> 1)];
        static internal int SetFG_ARGB(ref int n, int argb) => n = (int)(n & _knCLUT_BGmask) + _revCLUT[argb];
        static internal int SetBG_ARGB(ref int n, int argb) => n = (int)(n & _knCLUT_FGmask) + (_revCLUT[argb] << (CLUT_Shift >> 1));

        static internal Brush ARGBtoBrush(int nFormsARGB) => new SolidColorBrush(FromArgb(nFormsARGB));

        readonly static IReadOnlyList<int>
            CLUT = new int[_knNumColors]
        {
            Transparent, LightBlue, DarkGray, DarkKhaki, DarkRedBG,
            Firebrick, DarkYellowBG, LightGray, MediumSpringGreen, MediumVioletRed,
            OliveDrab, Red, SteelBlue
        };

        static UtilColor()
        {
            int nIx = 0;
            var revClut = new Dictionary<int, int>();

            revClut[Transparent] = nIx++;
            revClut[LightBlue] = nIx++;
            revClut[DarkGray] = nIx++;
            revClut[DarkKhaki] = nIx++;
            revClut[DarkRedBG] = nIx++;
            revClut[Firebrick] = nIx++;
            revClut[DarkYellowBG] = nIx++;
            revClut[LightGray] = nIx++;
            revClut[MediumSpringGreen] = nIx++;
            revClut[MediumVioletRed] = nIx++;
            revClut[OliveDrab] = nIx++;
            revClut[Red] = nIx++;
            revClut[SteelBlue] = nIx++;
            _revCLUT = revClut;
            Util.Assert(99957, nIx == _knNumColors);
            Util.Assert(99910, 0 == CLUT_Shift >> 4);          // 16 bits, not _knNumColors
        }

        const int
            _knNumColors = 13;
        const uint
            _knCLUT_FGmask = 0xF;
        static readonly uint
            _knCLUT_BGmask = CLUT_Mask - _knCLUT_FGmask;
        static readonly IReadOnlyDictionary<int, int>
            _revCLUT = null;
    }
}
