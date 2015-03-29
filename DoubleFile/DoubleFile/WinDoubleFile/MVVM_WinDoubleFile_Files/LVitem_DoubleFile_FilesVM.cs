using System.Collections.Generic;

namespace DoubleFile
{
    class LVitem_DoubleFile_FilesVM : ListViewItemVM_Base
    {
        internal System.Collections.Generic.IEnumerable<FileDictionary.DuplicateStruct>
            LSduplicates = null;

        internal int nDuplicates { get; set; }
        internal string[] FileLine { get; set; }

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
                    (0 == nDuplicates) &&
                        ((FileLine.Length <= FileParse.knColLengthLV) ||                    // doesn't happen
                        string.IsNullOrWhiteSpace(FileLine[FileParse.knColLengthLV]) ||     // doesn't happen
                        ulong.Parse(FileLine[FileParse.knColLengthLV]) > 0);
            }
        }

        public string Filename { get { return FileLine[0]; } }

        public string Duplicates
        {
            get
            {
                return
                    ((0 != nDuplicates) ? "" + nDuplicates : "") +
                    (_SameVolume ? " all on the same volume." : "");
            }
        }

        public string Created { get { return FileLine[1]; } }
        public string Modified { get { return FileLine[2]; } }
        public string Attributes { get { return UtilDirList.DecodeAttributes(FileLine[3]); } }
        public string Length { get { return 4 < FileLine.Length ? UtilDirList.FormatSize(FileLine[4]) : ""; } }
        public string Error1 { get { return 5 < FileLine.Length ? FileLine[5] : ""; } }
        public string Error2 { get { return 6 < FileLine.Length ? FileLine[6] : ""; } }

        protected override string[] PropertyNames { get { return new[] { "Filename", "Duplicates", "Created", "Modified", "Attributes", "Length", "Error1", "Error2" }; } }

        internal override int NumCols { get { return NumCols_; } }
        internal const int NumCols_ = 8;

        internal LVitem_DoubleFile_FilesVM(IEnumerable<string> ieString = null)
            : base(null, ieString)
        {
            marr = null;
        }

        internal LVitem_DoubleFile_FilesVM(LVitem_DoubleFile_FilesVM lvItemTemp)
            : this(lvItemTemp.StringValues)
        {
        }
    }
}
