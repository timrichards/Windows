namespace DoubleFile
{
    class LVitem_FileHash_FilesVM : ListViewItemVM_Base
    {
        internal System.Collections.Generic.IEnumerable<FileDictionary.DuplicateStruct>
            LSduplicates = null;

        internal string FileLine { get; set; }

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

        public bool SolitaryOrZero
        {
            get { return _SolitaryOrZero; }
            internal set
            {
                _SolitaryOrZero = value;
                RaisePropertyChanged("SolitaryOrZero");
            }
        }
        bool _SolitaryOrZero = false;

        public string Filename { get { return marr[0]; } internal set { SetProperty(0, value); } }
        public string Duplicates { get { return marr[1] + (_SameVolume ? " all on the same volume." : ""); } internal set { SetProperty(1, value); } }

        protected override string[] PropertyNames { get { return new[] { "Filename", "Duplicates" }; } }

        internal override int NumCols { get { return NumCols_; } }
        internal const int NumCols_ = 2;

        internal LVitem_FileHash_FilesVM(string[] arrStr = null)
            : base(null, arrStr)
        {
        }

        internal LVitem_FileHash_FilesVM(LVitem_FileHash_FilesVM lvItemTemp)
            : base(null, lvItemTemp.StringValues)
        {
        }
    }
}
