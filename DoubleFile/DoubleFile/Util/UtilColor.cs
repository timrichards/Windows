using System;
using System.Collections.Generic;
using System.Drawing;

namespace DoubleFile
{
    static class UtilColor
    {
        static internal IReadOnlyDictionary<int, string>
            Descriptions = new Dictionary<int, string>
        {
            {Empty,             ""},
            {White,             ""},
            {LightBlue,         "This folder has multiple copies on at least two separate volumes."},
            {DarkGray,          ""},						            // ignore list
            {DarkKhaki,         ""},						            // Treemap: Folder containing files
            {DarkRedBG,         "Contains folders that have no copy, or copies are on one volume."},
            {DarkSlateGray,     ""},						            // LV marker item back color
            {Firebrick,         "All copies of this folder reside on one volume."},
            {DarkYellowBG,      "This folder and its parent have a copy on a separate volume."},
            {LightGray,         "This folder has no data."},
            {MediumSpringGreen, ""},						            // Treemap: Free space
            {MediumVioletRed,   ""},						            // Treemap
            {OliveDrab,         ""},						            // Treemap: File
            {Red,               "This folder has no exact copy."},
            {SteelBlue,         "This folder has a copy on a separate volume."}
        };

        static internal int Empty { get { return Color.Empty.ToArgb(); } }
        static internal int White { get { return Color.White.ToArgb(); } }
        static internal int LightBlue { get { return Color.LightBlue.ToArgb(); } }
        static internal int DarkGray { get { return Color.DarkGray.ToArgb(); } }
        static internal int DarkKhaki { get { return Color.DarkKhaki.ToArgb(); } }
        static internal int DarkRedBG { get { return Color.FromArgb(64, 64, 0, 0).ToArgb(); } }
        static internal int DarkSlateGray { get { return Color.DarkSlateGray.ToArgb(); } }
        static internal int Firebrick { get { return Color.Firebrick.ToArgb(); } }
        static internal int DarkYellowBG { get { return Color.FromArgb(64, 0, 64, 64).ToArgb(); } }
        static internal int LightGray { get { return Color.LightGray.ToArgb(); } }
        static internal int MediumSpringGreen { get { return Color.MediumSpringGreen.ToArgb(); } }
        static internal int MediumVioletRed { get { return Color.MediumVioletRed.ToArgb(); } }
        static internal int OliveDrab { get { return Color.OliveDrab.ToArgb(); } }
        static internal int Red { get { return Color.FromArgb(255, 192, 0, 0).ToArgb(); } }
        static internal int SteelBlue { get { return Color.SteelBlue.ToArgb(); } }

        static internal uint
            CLUT_Mask { get { return 0xFF; } }
        static internal int
            CLUT_Shift { get { return (int)Math.Log(CLUT_Mask + 1, 2); } }

        static internal int Set_ARGB(int fg, int bg)
        {
            int ret = 0;

            SetFG_ARGB(ref ret, fg);
            SetBG_ARGB(ref ret, bg);
            return ret;
        }

        static internal int GetFG_ARGB(int n) { return CLUT[(n & _knCLUT_FGmask)]; }
        static internal int GetBG_ARGB(int n) { return CLUT[(n & _knCLUT_BGmask) >> (CLUT_Shift >> 1)]; }
        static internal int SetFG_ARGB(ref int n, int argb) { return n = (int)(n & _knCLUT_BGmask) + _revCLUT[argb]; }
        static internal int SetBG_ARGB(ref int n, int argb) { return n = (int)(n & _knCLUT_FGmask) + (_revCLUT[argb] << (CLUT_Shift >> 1)); }

        static internal System.Windows.Media.Brush ARGBtoBrush(int nFormsARGB)
        {
            var abARGB = BitConverter.GetBytes(nFormsARGB);

            return new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromArgb(abARGB[3], abARGB[2], abARGB[1], abARGB[0])
            );
        }

        readonly static int[] CLUT = new int[_knNumColors]
        {
            Empty, White, LightBlue, DarkGray, DarkKhaki, DarkRedBG, DarkSlateGray,
            Firebrick, DarkYellowBG, LightGray, MediumSpringGreen, MediumVioletRed,
            OliveDrab, Red, SteelBlue
        };

        static UtilColor()
        {
            int nIx = 0;
            var revClut = new Dictionary<int, int>();

            revClut[Empty] = nIx++;
            revClut[White] = nIx++;
            revClut[LightBlue] = nIx++;
            revClut[DarkGray] = nIx++;
            revClut[DarkKhaki] = nIx++;
            revClut[DarkRedBG] = nIx++;
            revClut[DarkSlateGray] = nIx++;
            revClut[Firebrick] = nIx++;
            revClut[DarkYellowBG] = nIx++;
            revClut[LightGray] = nIx++;
            revClut[MediumSpringGreen] = nIx++;
            revClut[MediumVioletRed] = nIx++;
            revClut[OliveDrab] = nIx++;
            revClut[Red] = nIx++;
            revClut[SteelBlue] = nIx++;
            _revCLUT = revClut;
            MBoxStatic.Assert(99957, nIx == _knNumColors);
            MBoxStatic.Assert(99910, 0 == CLUT_Shift >> 4);          // 16 bits, not _knNumColors
        }

        const int
            _knNumColors = 15;
        const uint
            _knCLUT_FGmask = 0xF;
        static readonly uint
            _knCLUT_BGmask = CLUT_Mask - _knCLUT_FGmask;
        static readonly IReadOnlyDictionary<int, int>
            _revCLUT = null;
    }
}
