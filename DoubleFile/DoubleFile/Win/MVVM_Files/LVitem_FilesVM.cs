using System;
using System.Collections.Generic;

namespace DoubleFile
{
    class LVitem_FilesVM : ListViewItemVM_Base
    {
        static internal bool ShowDuplicates = true;     // use-case: VolTreeMap project

        public string Filename => FileLine[0];
        public string Created => FileLine[1];
        public DateTime CreatedRaw => ("" + Created).ToDateTime();
        public string Modified => FileLine[2];
        public DateTime ModifiedRaw => ("" + Modified).ToDateTime();
        public string Attributes => Util.DecodeAttributes(FileLine[3]);
        public ulong LengthRaw => 4 < FileLine.Length ? ("" + FileLine[4]).ToUlong() : 0;
        public string Length => Util.FormatSize(LengthRaw);
        public string Error1 => 5 < FileLine.Length ? FileLine[5] : "";
        public string Error2 => 6 < FileLine.Length ? FileLine[6] : "";

        public bool SameVolume
        {
            get { return _SameVolume; }
            internal set
            {
                if (false == ShowDuplicates)
                    return;     // from lambda

                _SameVolume = value;
                RaisePropertyChanged("SameVolume");
            }
        }
        bool _SameVolume = false;

        public bool SolitaryAndNonEmpty =>
            ShowDuplicates &&
            (0 == DuplicatesRaw) &&
                ((FileLine.Length <= FileParse.knColLengthLV) ||                    // doesn't happen
                string.IsNullOrWhiteSpace(FileLine[FileParse.knColLengthLV]) ||     // doesn't happen
                (0 < ("" + FileLine[FileParse.knColLengthLV]).ToUlong()));

        public string Duplicates =>
            ((ShowDuplicates && (0 != DuplicatesRaw))
            ? "" + DuplicatesRaw : "") + (_SameVolume ? " all on the same volume." : "");
        public int DuplicatesRaw { get; internal set; }

        internal override int NumCols => NumCols_;
        internal const int NumCols_ = 0;

        protected override string[] _propNames { get { return _propNamesA; } set { _propNamesA = value; } }
        static string[] _propNamesA = null;

        internal IEnumerable<FileDictionary.DuplicateStruct>
            LSduplicates = null;
        internal string[]
            FileLine;
    }
}
