using System.Collections.Generic;
using System.Windows.Media;

namespace DoubleFile
{
    static class UtilColorcode
    {
        static internal readonly IReadOnlyDictionary<int, string>
            Descriptions = new Dictionary<int, string>
        {
            { Transparent,          "" },
            { ParentCloned,         "" },        // decription would look redundant in detailed info view
            { ParentClonedBG,       "This folder and its parent have a copy on a separate volume." },
            { ChildClonedSepVolume, "A child of this folder has one or more clones on a separate volume." },
            { ChildAllOnOneVolume,  "All copies of a child of this folder reside on one volume." },
            { ManyClonesSepVolume,  "This folder has multiple copies on at least two separate volumes." },
            { OneCloneSepVolume,    "This folder has a copy on a separate volume." },
            { AllOnOneVolume,       "All copies of this folder reside on one volume." },
            { Solitary,             "This folder has no exact copy." },
            { SolitAllDupesOneVol,  "This folder has no exact copy, yet all its files are duplicated, on just one volume." },
            { SolitAllDupesSepVol,  "This folder has no exact copy, yet all its files are duplicated, on more than one volume." },
            { SolitaryHasClones,    "This folder has no exact copy, yet it contains folders that do." },
            { SolitAllClonesOneVol, "This folder has no exact copy, yet all its folders and any files here are duplicated, at least one on one volume." },
            { SolitAllClonesSepVol, "This folder has no exact copy, yet all its folders and any files here are duplicated, all on more than one volume." },
            { SolitaryClonedParent, "This folder has no exact copy, yet its parent does." },
            { SolitaryOneVolParent, "This folder has no exact copy, yet its parent does, on only one volume." },
            { ContainsSolitaryBG,   "Contains folders that have no copy, or copies are on one volume." },
            { FolderHasNoHashes,    "Couldn't create hashcodes for any file in this folder." },
            { ZeroLengthFolder,     "This folder has no data." },
            { TreemapFolder,        "" },						            // Treemap: Folder containing files
            { TreemapFreespace,     "" },						            // Treemap: Free space
            { TreemapUnreadspace,   "" },						            // Treemap: Unread space
            { TreemapDupeOneVol,    "" },						            // Treemap: Duplicate File
            { TreemapDupeSepVol,    "" },						            // Treemap: Duplicate File
            { TreemapUniqueFile,    "" },						            // Treemap: Unique File
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
        internal const int ParentCloned         = unchecked((int)0xFFE1E1C1);
        internal const int ChildClonedSepVolume = unchecked((int)0xFF5282B2);
        internal const int ChildAllOnOneVolume  = unchecked((int)0xFFB34343);
        internal const int ManyClonesSepVolume  = unchecked((int)0xFFA4D4E4);   // => Colors.LightBlue.ToArgb();                // LightBlue
        internal const int OneCloneSepVolume    = unchecked((int)0xFF4585B5);   // => Colors.SteelBlue.ToArgb();                // SteelBlue
        internal const int AllOnOneVolume       = unchecked((int)0xFFB62626);   // => Colors.Firebrick.ToArgb();                // Firebrick
        internal const int Solitary             = unchecked((int)0xFFC70707);   // => Color.FromArgb(255, 192, 0, 0).ToArgb();  // Red
        internal const int SolitAllDupesOneVol  = unchecked((int)0xFFF8C8D8);
        internal const int SolitAllDupesSepVol  = unchecked((int)0xFF7959D9);
        internal const int SolitaryHasClones    = unchecked((int)0xFFCA2A2A);
        internal const int SolitAllClonesOneVol = unchecked((int)0xFFFBCBDB);
        internal const int SolitAllClonesSepVol = unchecked((int)0xFFACCCDC);
        internal const int SolitaryClonedParent = unchecked((int)0xFF4D7DAD);
        internal const int SolitaryOneVolParent = unchecked((int)0xFFBE3E3E);
        internal const int ContainsSolitaryBG   =                0x404F0F0F;    // => Color.FromArgb(64, 64, 0, 0).ToArgb();    // DarkRedBG
        internal const int FolderHasNoHashes    = unchecked((int)0xFFA00000);
        internal const int ZeroLengthFolder     = unchecked((int)0xFFD1D1D1);   // => Colors.LightGray.ToArgb();                // LightGray
        internal const int TreemapFolder        = unchecked((int)0xFF62A242);
        internal const int TreemapFreespace     = unchecked((int)0xFF03F393);   // => Colors.MediumSpringGreen.ToArgb();        // MediumSpringGreen
        internal const int TreemapUnreadspace   = unchecked((int)0xFFC41484);   // => Colors.MediumVioletRed.ToArgb();          // MediumVioletRed
        internal const int TreemapDupeSepVol    = unchecked((int)0xFF658505);   // => Colors.OliveDrab.ToArgb();                // OliveDrab
        internal const int TreemapDupeOneVol    = unchecked((int)0xFFE62626);
        internal const int TreemapUniqueFile    = unchecked((int)0xFFC71717);

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
            TreemapDupeSepVol, Solitary, SolitaryClonedParent, SolitaryOneVolParent, OneCloneSepVolume,
            TreemapUniqueFile, SolitAllDupesOneVol, ChildClonedSepVolume, ChildAllOnOneVolume, FolderHasNoHashes,
            SolitAllClonesOneVol, SolitAllDupesSepVol, TreemapDupeOneVol, SolitAllClonesSepVol
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
            revClut[TreemapDupeSepVol] = nIx++;
            revClut[Solitary] = nIx++;
            revClut[SolitaryClonedParent] = nIx++;
            revClut[SolitaryOneVolParent] = nIx++;
            revClut[OneCloneSepVolume] = nIx++;
            revClut[TreemapUniqueFile] = nIx++;
            revClut[SolitAllDupesOneVol] = nIx++;
            revClut[ChildClonedSepVolume] = nIx++;
            revClut[ChildAllOnOneVolume] = nIx++;
            revClut[FolderHasNoHashes] = nIx++;
            revClut[SolitAllClonesOneVol] = nIx++;
            revClut[SolitAllDupesSepVol] = nIx++;
            revClut[TreemapDupeOneVol] = nIx++;
            revClut[SolitAllClonesSepVol] = nIx++;
            _revCLUT = revClut;
            Util.Assert(99957, nIx == _knNumColors);
            Util.Assert(99910, 0 == CLUT_Shift >> 4);          // 16 bits, not _knNumColors
        }

        const int
            _knNumColors = 25;
        const uint
            _knCLUT_FGmask = (1 << (CLUT_Shift >> 1)) - 1;
        static readonly uint
            _knCLUT_BGmask = CLUT_Mask - _knCLUT_FGmask;
        static readonly IReadOnlyDictionary<int, int>
            _revCLUT = null;
    }
}
