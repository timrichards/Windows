
namespace DoubleFile
{
    class LVitem_FileHashVM : ListViewItemVM_Base
    {
        internal System.Collections.Generic.IEnumerable<FileDictionary.DuplicateStruct>
            LSduplicates = null;

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

        public bool Solitary
        {
            get { return _Solitary; }
            internal set
            {
                _Solitary = value;
                RaisePropertyChanged("Solitary");
            }
        }
        bool _Solitary = false;

        public string Filename { get { return marr[0]; } internal set { SetProperty(0, value); } }
        public string Duplicates { get { return marr[1]; } internal set { SetProperty(1, value); } }

        protected override string[] PropertyNames { get { return new[] { "Filename", "Duplicates" }; } }

        internal override int NumCols { get { return NumCols_; } }
        internal const int NumCols_ = 2;

        internal LVitem_FileHashVM(string[] arrStr = null)
            : base(null, arrStr)
        {
        }

        internal LVitem_FileHashVM(LVitem_FileHashVM lvItemTemp)
            : base(null, lvItemTemp.StringValues)
        {
        }
    }
}
