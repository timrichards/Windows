using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace DoubleFile
{
    class LVitem_FilesVM : ListViewItemVM_Base
    {
        static internal bool ShowDuplicatesA = true;                // VolTreemap assembly
        internal virtual bool ShowDuplicates => ShowDuplicatesA;    // compare view

        public string Filename => SubItems[0];
        public string Created => SubItems[1];
        public DateTime CreatedRaw => ("" + Created).ToDateTime();
        public string Modified => SubItems[2];
        public DateTime ModifiedRaw => ("" + Modified).ToDateTime();
        public string Attributes => Util.DecodeAttributes(SubItems[3]);
        public ulong LengthRaw => 4 < SubItems.Count ? ("" + SubItems[4]).ToUlong() : 0;
        public string Length => LengthRaw.FormatSize();
        public string Error1 => 5 < SubItems.Count ? SubItems[5] : "";
        public string Error2 => 6 < SubItems.Count ? SubItems[6] : "";

        internal IReadOnlyList<string>
            FileLine { get { return (IReadOnlyList<string>)SubItems; } set { SubItems = (IList<string>)value; }}
        internal override string
            ExportLine => string.Join(" ", SubItems.Take(6)).Trim();

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
                ((SubItems.Count <= FileParse.knColLengthLV) ||                     // doesn't happen
                string.IsNullOrWhiteSpace(SubItems[FileParse.knColLengthLV]) ||     // doesn't happen
                (0 < ("" + SubItems[FileParse.knColLengthLV]).ToUlong()));

        public string
            Duplicates => ((ShowDuplicates && (0 < DuplicatesRaw)) ? "" + DuplicatesRaw : "") + (_SameVolume ? " all on the same volume." : "");
        public int DuplicatesRaw { get; internal set; }

        public Visibility
            VisibilityOnDuplicates => (ShowDuplicates && (0 < DuplicatesRaw)) ? Visibility.Visible : Visibility.Collapsed;

        public ICommand Icmd_NextDuplicate { get; }

        public string
            Duplicate => LSdupDirFileLines?[DupIndex % LSdupDirFileLines.Count].Item2[1];
        public string
            In { get; private set; } = " in ";    // interned

        public string
            Parent => LSdupDirFileLines?[DupIndex % LSdupDirFileLines.Count].Item2[0];

        internal override int NumCols => NumCols_;
        internal const int NumCols_ = 9;

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
        internal IReadOnlyList<Tuple<LVitemProject_Updater<bool>, IReadOnlyList<string>>>
            LSdupDirFileLines = null;
        internal int DupIndex = 0;
    }
}
