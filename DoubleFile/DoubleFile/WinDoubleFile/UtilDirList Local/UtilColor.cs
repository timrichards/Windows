﻿using System;
using System.Collections.Generic;
using System.Drawing;

namespace DoubleFile
{
    static class UtilColor
    {
        static internal Dictionary<int, string>
            Description = new Dictionary<int, string>();

        static internal int Empty { get { return Color.Empty.ToArgb(); } }
        static internal int White { get { return Color.White.ToArgb(); } }
        static internal int Blue { get { return Color.Blue.ToArgb(); } }
        static internal int DarkGray { get { return Color.DarkGray.ToArgb(); } }
        static internal int DarkKhaki { get { return Color.DarkKhaki.ToArgb(); } }
        static internal int DarkRed { get { return Color.DarkRed.ToArgb(); } }
        static internal int DarkSlateGray { get { return Color.DarkSlateGray.ToArgb(); } }
        static internal int Firebrick { get { return Color.Firebrick.ToArgb(); } }
        static internal int LightGoldenrodYellow { get { return Color.LightGoldenrodYellow.ToArgb(); } }
        static internal int LightGray { get { return Color.LightGray.ToArgb(); } }
        static internal int MediumSpringGreen { get { return Color.MediumSpringGreen.ToArgb(); } }
        static internal int MediumVioletRed { get { return Color.MediumVioletRed.ToArgb(); } }
        static internal int OliveDrab { get { return Color.OliveDrab.ToArgb(); } }
        static internal int Red { get { return Color.Red.ToArgb(); } }
        static internal int Snow { get { return Color.Snow.ToArgb(); } }
        static internal int SteelBlue { get { return Color.SteelBlue.ToArgb(); } }

        static internal uint
            CLUT_Mask { get { return 0x000000FF; } }
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
        static internal int SetFG_ARGB(ref int n, int argb) { return n = (int)(n & _knCLUT_BGmask) + _RevCLUT[argb]; }
        static internal int SetBG_ARGB(ref int n, int argb) { return n = (int)(n & _knCLUT_FGmask) + (_RevCLUT[argb] << (CLUT_Shift >> 1)); }

        static internal System.Windows.Media.Brush ARGBtoBrush(int nFormsARGB)
        {
            var abARGB = BitConverter.GetBytes(nFormsARGB);

            return new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromArgb(abARGB[3], abARGB[2], abARGB[1], abARGB[0])
            );
        }

        readonly static int[] CLUT = new int[_knNumColors]
        {
            Empty, White, Blue, DarkGray, DarkKhaki, DarkRed, DarkSlateGray,
            Firebrick, LightGoldenrodYellow, LightGray, MediumSpringGreen, MediumVioletRed,
            OliveDrab, Red, Snow, SteelBlue
        };

        static UtilColor()
        {
            int nIx = 0;

            _RevCLUT[Empty] = nIx++;
            _RevCLUT[White] = nIx++;
            _RevCLUT[Blue] = nIx++;
            _RevCLUT[DarkGray] = nIx++;
            _RevCLUT[DarkKhaki] = nIx++;
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
            MBoxStatic.Assert(99910, 0 == CLUT_Shift >> 4);          // 16 bits, not _knNumColors

            Description[Empty] = "";
            Description[White] = "";
            Description[Blue] = "This folder has multiple copies on at least two separate volumes.";
            Description[DarkGray] = "";             // ignore list
            Description[DarkKhaki] = "";            // Treemap: Folder containing files
            Description[DarkRed] = "";              // not used
            Description[DarkSlateGray] = "";        // LV marker item back color
            Description[Firebrick] = "All copies of this folder reside on one volume.";
            Description[LightGoldenrodYellow] = "This folder and its parent have a copy on a separate volume.";
            Description[LightGray] = "This folder has no data.";
            Description[MediumSpringGreen] = "";    // Treemap: Free space
            Description[MediumVioletRed] = "";      // Treemap
            Description[OliveDrab] = "";            // Treemap: File
            Description[Red] = "This folder has no exact copy.";
            Description[Snow] = "Contains folders that have no copy; or copies are on one volume.";
            Description[SteelBlue] = "This folder has a copy on a separate volume.";
        }

        const int
            _knNumColors = 16;
        const uint
            _knCLUT_FGmask = 0x0000000F;
        static readonly uint
            _knCLUT_BGmask = CLUT_Mask - _knCLUT_FGmask;
        static readonly Dictionary<int, int>
            _RevCLUT = new Dictionary<int, int>();
    }
}
