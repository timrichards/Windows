using System;
using System.Collections.Generic;

namespace DoubleFile
{
    class LVitem_FilesVM : ListViewItemVM_Base
    {
        internal IEnumerable<FileDictionary.DuplicateStruct>
            LSduplicates = null;
        internal string[]
            FileLine { get; set; }

        public bool SameVolume
        {
            get { return _SameVolume; }
            internal set
            {
                _SameVolume = value;
                RaisePropertyChanged("SameVolume");
            }
        }
        bool _SameVolume = false;

        public bool SolitaryAndNonEmpty
        {
            get
            {
                return
                    (0 == DuplicatesRaw) &&
                        ((FileLine.Length <= FileParse.knColLengthLV) ||                    // doesn't happen
                        string.IsNullOrWhiteSpace(FileLine[FileParse.knColLengthLV]) ||     // doesn't happen
                        (0 < ulong.Parse(FileLine[FileParse.knColLengthLV])));
            }
        }

        public string Filename { get { return FileLine[0]; } }

        public string Duplicates
        {
            get
            {
                return
                    ((0 != DuplicatesRaw) ? "" + DuplicatesRaw : "") +
                    (_SameVolume ? " all on the same volume." : "");
            }
        }
        public int DuplicatesRaw { get; internal set; }

        public string Created { get { return FileLine[1]; } }
        public DateTime CreatedRaw { get { return DateTime.Parse(FileLine[1]); } }
        public string Modified { get { return FileLine[2]; } }
        public DateTime ModifiedRaw { get { return DateTime.Parse(FileLine[1]); } }
        public string Attributes { get { return Util.DecodeAttributes(FileLine[3]); } }
        public string Length { get { return 4 < FileLine.Length ? Util.FormatSize(FileLine[4]) : ""; } }
        public ulong LengthRaw { get { return 4 < FileLine.Length ? ulong.Parse(FileLine[4]) : 0; } }
        public string Error1 { get { return 5 < FileLine.Length ? FileLine[5] : ""; } }
        public string Error2 { get { return 6 < FileLine.Length ? FileLine[6] : ""; } }

        internal override int NumCols { get { return NumCols_; } }
        internal const int NumCols_ = 8;

        protected override string[] _propNames { get { return _propNamesA; } set { _propNamesA = value; } }
        static string[] _propNamesA = null;

        internal LVitem_FilesVM(IList<string> lsStr = null)
            : base(null, lsStr)
        {
            marr = null;
        }

        internal LVitem_FilesVM(LVitem_FilesVM lvItemTemp)
            : this(lvItemTemp.StringValues)
        {
        }
    }
}
