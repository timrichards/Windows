using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace DoubleFile
{
    class LVitem_FilesVM : LVitem_FilesVM_Base
    {
        static internal bool ShowDuplicatesA = true;                // VolTreemap assembly
        internal virtual bool ShowDuplicates => ShowDuplicatesA;    // compare view

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
            Duplicates => ((ShowDuplicates && (0 < DuplicatesRaw)) ? "" + DuplicatesRaw : "") +
            (_SameVolume ? " all on " + UtilColorcode.SameVolumeText : "");
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

        internal LVitem_FilesVM()
        {
            if (ShowDuplicates)
            {
                Icmd_NextDuplicate =
                    new RelayCommand(() =>
                {
                    ++DupIndex;
                    RaisePropertyChanged("Duplicate");
                    RaisePropertyChanged("Parent");
                });
            }
        }

        internal IEnumerable<DupeFileDictionary.DuplicateStruct>
            LSduplicates = null;
        internal IReadOnlyList<Tuple<LVitemProject_Updater<bool>, IReadOnlyList<string>>>
            LSdupDirFileLines = null;
        internal int DupIndex = 0;

        protected override IReadOnlyList<string> _propNames { get { return _propNamesA; } set { _propNamesA = value; } }
        static IReadOnlyList<string> _propNamesA = null;
    }
}
