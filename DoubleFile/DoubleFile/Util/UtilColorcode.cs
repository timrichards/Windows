using System.Collections.Generic;
using System.Windows.Media;

namespace DoubleFile
{
    static class UtilColorcode
    {
        static internal readonly IReadOnlyDictionary<int, string>
            Descriptions = new Dictionary<int, string>
        {
            {Transparent,           ""},
            {ParentCloned,          ""},        // decription would look redundant in detailed info view
            {ParentClonedBG,        "This folder and its parent have a copy on a separate volume."},
            {ManyClonesSepVolume,   "This folder has multiple copies on at least two separate volumes."},
            {OneCloneSepVolume,     "This folder has a copy on a separate volume."},
            {AllOnOneVolume,        "All copies of this folder reside on one volume."},
            {Solitary,              "This folder has no exact copy."},
            {SolitaryHasClones,     "This folder has no exact copy, yet it contains folders that do."},
            {SolitaryClonedParent,  "This folder has no exact copy, yet its parent does."},
            {SolitaryOneVolParent,  "This folder has no exact copy, yet its parent does, on only one volume."},
            {ContainsSolitaryBG,    "Contains folders that have no copy, or copies are on one volume."},
            {ZeroLengthFolder,      "This folder has no data."},
            {TreemapFolder,         ""},						            // Treemap: Folder containing files
            {TreemapFreespace,      ""},						            // Treemap: Free space
            {TreemapUnreadspace,    ""},						            // Treemap: Unread space
            {TreemapDupeFile,       ""},						            // Treemap: Duplicate File
            {TreemapUniqueFile,     ""},						            // Treemap: Unique File
            {SolitaryHasAllDupes,   "This folder has no exact copy, yet all its files are duplicated."}
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

        // "Clones" and "Solitary" refer to folders whereas "Dupes" and "Unique" refer to files.
        // these need to remain properties - unless const
        internal const int Transparent          =                0x00FFFFFF;    // => Colors.Transparent.ToArgb();              // Transparent
        internal const int ParentClonedBG       =                0x40004040;    // => Color.FromArgb(64, 0, 64, 64).ToArgb();   // DarkYellowBG
        internal const int ParentCloned         = unchecked((int)0xFFEEEECC);
        internal const int ManyClonesSepVolume  = unchecked((int)0xFFADD8E6);   // => Colors.LightBlue.ToArgb();                // LightBlue
        internal const int OneCloneSepVolume    = unchecked((int)0xFF4682B4);   // => Colors.SteelBlue.ToArgb();                // SteelBlue
        internal const int AllOnOneVolume       = unchecked((int)0xFFB22222);   // => Colors.Firebrick.ToArgb();                // Firebrick
        internal const int Solitary             = unchecked((int)0xFFC00000);   // => Color.FromArgb(255, 192, 0, 0).ToArgb();  // Red
        internal const int SolitaryHasClones    = unchecked((int)0xFFC02222);
        internal const int SolitaryClonedParent = unchecked((int)0xFF4477AA);
        internal const int SolitaryOneVolParent = unchecked((int)0xFFBB3333);
        internal const int ContainsSolitaryBG   =                0x40400000;    // => Color.FromArgb(64, 64, 0, 0).ToArgb();    // DarkRedBG
        internal const int ZeroLengthFolder     = unchecked((int)0xFFD3D3D3);   // => Colors.LightGray.ToArgb();                // LightGray
        internal const int TreemapFolder        = unchecked((int)0xFF66AA44);
        internal const int TreemapFreespace     = unchecked((int)0xFF00FA9A);   // => Colors.MediumSpringGreen.ToArgb();        // MediumSpringGreen
        internal const int TreemapUnreadspace   = unchecked((int)0xFFC71585);   // => Colors.MediumVioletRed.ToArgb();          // MediumVioletRed
        internal const int TreemapDupeFile      = unchecked((int)0xFF6B8E23);   // => Colors.OliveDrab.ToArgb();                // OliveDrab
        internal const int TreemapUniqueFile    = unchecked((int)0xFFC00001);
        internal const int SolitaryHasAllDupes  = unchecked((int)0xFF4682B3);

        internal const int
            CLUT_Mask = (1 << CLUT_Shift) - 1;
        internal const int
            CLUT_Shift = 10;

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
            Transparent, ManyClonesSepVolume, SolitaryHasClones, TreemapFolder, ContainsSolitaryBG,
            AllOnOneVolume, ParentClonedBG, ParentCloned, ZeroLengthFolder, TreemapFreespace, TreemapUnreadspace,
            TreemapDupeFile, Solitary, SolitaryClonedParent, SolitaryOneVolParent, OneCloneSepVolume,
            TreemapUniqueFile, SolitaryHasAllDupes
        };

        static UtilColorcode()
        {
            Util.Assert(99587, 0 == CLUT_Shift % 2);

            var nIx = 0;
            var revClut = new Dictionary<int, int>();

            revClut[Transparent] = nIx++;
            revClut[ManyClonesSepVolume] = nIx++;
            revClut[SolitaryHasClones] = nIx++;
            revClut[TreemapFolder] = nIx++;
            revClut[ContainsSolitaryBG] = nIx++;
            revClut[AllOnOneVolume] = nIx++;
            revClut[ParentClonedBG] = nIx++;
            revClut[ParentCloned] = nIx++;
            revClut[ZeroLengthFolder] = nIx++;
            revClut[TreemapFreespace] = nIx++;
            revClut[TreemapUnreadspace] = nIx++;
            revClut[TreemapDupeFile] = nIx++;
            revClut[Solitary] = nIx++;
            revClut[SolitaryClonedParent] = nIx++;
            revClut[SolitaryOneVolParent] = nIx++;
            revClut[OneCloneSepVolume] = nIx++;
            revClut[TreemapUniqueFile] = nIx++;
            revClut[SolitaryHasAllDupes] = nIx++;
            _revCLUT = revClut;
            Util.Assert(99957, nIx == _knNumColors);
            Util.Assert(99910, 0 == CLUT_Shift >> 4);          // 16 bits, not _knNumColors
        }

        const int
            _knNumColors = 18;
        const uint
            _knCLUT_FGmask = (1 << (CLUT_Shift >> 1)) - 1;
        static readonly uint
            _knCLUT_BGmask = CLUT_Mask - _knCLUT_FGmask;
        static readonly IReadOnlyDictionary<int, int>
            _revCLUT = null;
    }
}
