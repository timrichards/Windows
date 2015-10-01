using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace DoubleFile
{
    class LVitem_FilesVM : ListViewItemVM_Base
    {
        static internal bool ShowDuplicates = true;     // use-case: VolTreemap project

        public string Filename => FileLine[0];
        public string Created => FileLine[1];
        public DateTime CreatedRaw => ("" + Created).ToDateTime();
        public string Modified => FileLine[2];
        public DateTime ModifiedRaw => ("" + Modified).ToDateTime();
        public string Attributes => Util.DecodeAttributes(FileLine[3]);
        public ulong LengthRaw => 4 < FileLine.Count ? ("" + FileLine[4]).ToUlong() : 0;
        public string Length => LengthRaw.FormatSize();
        public string Error1 => 5 < FileLine.Count ? FileLine[5] : "";
        public string Error2 => 6 < FileLine.Count ? FileLine[6] : "";

        internal override string
            ExportLine => string.Join(" ", FileLine.Take(6)).Trim();

        public bool
            SameVolume
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

        public bool
            SolitaryAndNonEmpty =>
            ShowDuplicates && (0 == DuplicatesRaw) &&
                ((FileLine.Count <= FileParse.knColLengthLV) ||                     // doesn't happen
                string.IsNullOrWhiteSpace(FileLine[FileParse.knColLengthLV]) ||     // doesn't happen
                (0 < ("" + FileLine[FileParse.knColLengthLV]).ToUlong()));

        public string
            Duplicates => ((ShowDuplicates && (0 != DuplicatesRaw)) ? "" + DuplicatesRaw : "") + (_SameVolume ? " all on the same volume." : "");
        public int DuplicatesRaw { get; internal set; }

        public Visibility VisibilityOnDuplicates => (ShowDuplicates && (0 != DuplicatesRaw)) ? Visibility.Visible : Visibility.Collapsed;

        public ICommand Icmd_NextDuplicate { get; }

        public string
            Duplicate => LSdupDirFileLines?[DupIndex % LSdupDirFileLines.Count].Item2[1];
        public string
            In { get; private set; } = " in ";    // interned

        public string Parent
        {
            get
            {
                return LSdupDirFileLines?[DupIndex % LSdupDirFileLines.Count].Item2[0];
            }
        }

        internal override int NumCols => NumCols_;
        internal const int NumCols_ = 0;

        protected override IReadOnlyList<string> _propNames { get { return _propNamesA; } set { _propNamesA = value; } }
        static IReadOnlyList<string> _propNamesA = null;

        internal LVitem_FilesVM()
        {
            Icmd_NextDuplicate =
                new RelayCommand(() =>
            {
                ++DupIndex;
                RaisePropertyChanged("Duplicate");
                RaisePropertyChanged("Parent");
            });
        }

        internal IEnumerable<DupeFileDictionary.DuplicateStruct>
            LSduplicates = null;
        internal IReadOnlyList<string>
            FileLine;
        internal IReadOnlyList<Tuple<LVitemProject_Updater<bool>, IReadOnlyList<string>>>
            LSdupDirFileLines = null;
        internal int DupIndex = 0;
    }
}
