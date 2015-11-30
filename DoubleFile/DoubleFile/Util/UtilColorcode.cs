using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace DoubleFile
{
    static class UtilColorcode
    {
        static internal string 
            SameVolumeText => DupeFileDictionary.IsDeletedVolumeView ? "one or two volumes." : "one volume.";
        static string _fnStrOnMoreThan => " on more than " + (DupeFileDictionary.IsDeletedVolumeView ? "two volumes." : "one volume.");
        static string _fnStrTwo => DupeFileDictionary.IsDeletedVolumeView ? "three" : "two";
        static string _fnStrThisFolderHas => "This folder has " + (DupeFileDictionary.IsDeletedVolumeView ? "zero or one" : "no exact") + " copy";
        static string _fnStrA => DupeFileDictionary.IsDeletedVolumeView ? "more than one" : "a";
        static internal readonly IReadOnlyDictionary<int, Func<string>>
            Descriptions = new Dictionary<int, Func<string>>
        {
            { Transparent,          () => "" },
            { ParentCloned,         () => "" },        // decription would look redundant in detailed info view
            { ParentClonedBG,       () => "This folder and its parent have a copy on " + _fnStrA + " separate volume." },
            { ChildClonedSepVolume, () => "A child of this folder has one or more clones on " + _fnStrA + " separate volume." },
            { ChildAllOnOneVolume,  () => "All copies of a child of this folder reside on " + SameVolumeText },
            { ManyClonesSepVolume,  () => "This folder has multiple copies on at least " + _fnStrTwo + " separate volumes." },
            { OneCloneSepVolume,    () => "This folder has a copy on " + _fnStrA + " separate volume." },
            { AllOnOneVolume,       () => "All copies of this folder reside on " + SameVolumeText },
            { OneVolumeDupesSepVol, () => "All files are on " + _fnStrTwo + " or more volumes though all copies of this folder are on " + SameVolumeText },
            { SolitNoFilesDuped,    () => "No file in this folder is duplicated, or all are on " + SameVolumeText },
            { SolitSomeFilesDuped,  () => "One or more file in this folder is duplicated on " + _fnStrA + " volume." },
            { SolitAllDupesOneVol,  () => "All files in this folder are duplicated, at least one on " + SameVolumeText },
            { SolitAllDupesSepVol,  () => "All files in this folder are duplicated," + _fnStrOnMoreThan },
            { SolitaryHasClones,    () => _fnStrThisFolderHas + ", yet it contains folders with more copies." },
            { SolitAllClonesOneVol, () => "All folders and any files here are duplicated, at least one on " + SameVolumeText },
            { SolitAllClonesSepVol, () => "All folders and any files here are duplicated, all" + _fnStrOnMoreThan },
            { SolitaryClonedParent, () => _fnStrThisFolderHas + ", yet its parent does." },
            { SolitaryOneVolParent, () => _fnStrThisFolderHas + ", yet its parent does, on only " + SameVolumeText },
            { ContainsSolitaryBG,   () => "Contains folders that have no copy, or copies are on " + SameVolumeText },
            { FolderHasNoHashes,    () => "Could not create hashcodes for any file in this folder." },
            { ZeroLengthFolder,     () => "This folder has no files." },
            { TreemapFolder,        () => "" },						            // Treemap: Folder containing files
            { TreemapFreespace,     () => "" },						            // Treemap: Free space
            { TreemapUnreadspace,   () => "" },						            // Treemap: Unread space
            { TreemapDupeOneVol,    () => "" },						            // Treemap: Duplicate File
            { TreemapDupeSepVol,    () => "" },						            // Treemap: Duplicate File
            { TreemapUniqueFile,    () => "" },						            // Treemap: Unique File
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
        internal const int OneVolumeDupesSepVol = unchecked((int)0xFF4787B7);   // => Colors.SteelBlue.ToArgb();                // SteelBlue
        internal const int SolitNoFilesDuped    = unchecked((int)0xFFC80808);   // => Color.FromArgb(255, 192, 0, 0).ToArgb();  // Red
        internal const int SolitSomeFilesDuped  = unchecked((int)0xFFC90909);   // => Color.FromArgb(255, 192, 0, 0).ToArgb();  // Red
        internal const int SolitAllDupesOneVol  = unchecked((int)0xFFFACADA);
        internal const int SolitAllDupesSepVol  = unchecked((int)0xFF7B5BDB);
        internal const int SolitaryHasClones    = unchecked((int)0xFFCC2C2C);
        internal const int SolitAllClonesOneVol = unchecked((int)0xFFDDCDDD);
        internal const int SolitAllClonesSepVol = unchecked((int)0xFFAECEDE);
        internal const int SolitaryClonedParent = unchecked((int)0xFF4F7FAF);
        internal const int SolitaryOneVolParent = unchecked((int)0xFFB03030);
        internal const int ContainsSolitaryBG   =                0x40410101;    // => Color.FromArgb(64, 64, 0, 0).ToArgb();    // DarkRedBG
        internal const int FolderHasNoHashes    = unchecked((int)0xFFA20202);
        internal const int ZeroLengthFolder     = unchecked((int)0xFFD3D3D3);   // => Colors.LightGray.ToArgb();                // LightGray
        internal const int TreemapFolder        = unchecked((int)0xFF64A444);
        internal const int TreemapFreespace     = unchecked((int)0xFF05F595);   // => Colors.MediumSpringGreen.ToArgb();        // MediumSpringGreen
        internal const int TreemapUnreadspace   = unchecked((int)0xFFC61686);   // => Colors.MediumVioletRed.ToArgb();          // MediumVioletRed
        internal const int TreemapDupeSepVol    = unchecked((int)0xFF678707);   // => Colors.OliveDrab.ToArgb();                // OliveDrab
        internal const int TreemapDupeOneVol    = unchecked((int)0xFFE82828);
        internal const int TreemapUniqueFile    = unchecked((int)0xFFC91919);

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
            TreemapDupeSepVol, SolitNoFilesDuped, SolitaryClonedParent, SolitaryOneVolParent, OneCloneSepVolume,
            TreemapUniqueFile, SolitAllDupesOneVol, ChildClonedSepVolume, ChildAllOnOneVolume, FolderHasNoHashes,
            SolitAllClonesOneVol, SolitAllDupesSepVol, TreemapDupeOneVol, SolitAllClonesSepVol, OneVolumeDupesSepVol,
            SolitSomeFilesDuped
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
            revClut[SolitNoFilesDuped] = nIx++;
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
            revClut[OneVolumeDupesSepVol] = nIx++;
            revClut[SolitSomeFilesDuped] = nIx++;
            _revCLUT = revClut;
            Util.Assert(99957, nIx == _knNumColors);
            Util.Assert(99910, 0 == CLUT_Shift >> 4);          // 16 bits, not _knNumColors
        }

        const int
            _knNumColors = 27;
        const uint
            _knCLUT_FGmask = (1 << (CLUT_Shift >> 1)) - 1;
        static readonly uint
            _knCLUT_BGmask = CLUT_Mask - _knCLUT_FGmask;
        static readonly IReadOnlyDictionary<int, int>
            _revCLUT = null;
    }
}
