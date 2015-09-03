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

        // these need to remain properties - unless const
        internal const int Transparent         /*=> Colors.Transparent.ToArgb();            */  = unchecked(     0x00FFFFFF);
        internal const int LightBlue           /*=> Colors.LightBlue.ToArgb();              */  = unchecked((int)0xFFADD8E6);
        internal const int DarkGray            /*=> Colors.DarkGray.ToArgb();               */  = unchecked((int)0xFFA9A9A9);
        internal const int DarkKhaki           /*=> Colors.DarkKhaki.ToArgb();              */  = unchecked((int)0xFFBDB76B);
        internal const int DarkRedBG           /*=> Color.FromArgb(64, 64, 0, 0).ToArgb();  */  = unchecked(     0x40400000);
        internal const int Firebrick           /*=> Colors.Firebrick.ToArgb();              */  = unchecked((int)0xFFB22222);
        internal const int DarkYellowBG        /*=> Color.FromArgb(64, 0, 64, 64).ToArgb(); */  = unchecked(     0x40004040);
        internal const int LightGray           /*=> Colors.LightGray.ToArgb();              */  = unchecked((int)0xFFD3D3D3);
        internal const int MediumSpringGreen   /*=> Colors.MediumSpringGreen.ToArgb();      */  = unchecked((int)0xFF00FA9A);
        internal const int MediumVioletRed     /*=> Colors.MediumVioletRed.ToArgb();        */  = unchecked((int)0xFFC71585);
        internal const int OliveDrab           /*=> Colors.OliveDrab.ToArgb();              */  = unchecked((int)0xFF6B8E23);
        internal const int Red                 /*=> Color.FromArgb(255, 192, 0, 0).ToArgb();*/  = unchecked((int)0xFFC00000);
        internal const int SteelBlue           /*=> Colors.SteelBlue.ToArgb();              */  = unchecked((int)0xFF4682B4);

        internal const uint
            CLUT_Mask = 0xFF;
        static internal readonly int
            CLUT_Shift = (int)Math.Log(CLUT_Mask + 1, 2);

        static internal int Set_ARGB(int fg, int bg)
        {
            var ret = 0;

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
            var nIx = 0;
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
