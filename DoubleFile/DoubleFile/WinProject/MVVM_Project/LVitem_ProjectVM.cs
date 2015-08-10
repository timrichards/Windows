using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DoubleFile
{
    class LVitem_ProjectVM : ListViewItemVM_Base
    {
        public string Nickname { get { return SubItems[0]; } internal set { SetProperty(0, value); } }
        public string SourcePath { get { return SubItems[1]; } internal set { SetProperty(1, value); } }

        public string ListingFile { get { return SubItems[2]; } set { SetProperty(2, value); } }
        public string ListingFileNoPath => Path.GetFileName(SubItems[2]);

        public string Status { get { return SubItems[3]; } internal set { SetProperty(3, value); } }
        public string IncludeYN { get { return SubItems[4]; } private set { SetProperty(4, value); } }
        public string VolumeGroup { get { return SubItems[5]; } internal set { SetProperty(5, value); } }
        public string DriveModel { get { return SubItems[6]; } internal set { SetProperty(6, value); } }
        public string DriveSerial { get { return SubItems[7]; } internal set { SetProperty(7, value); } }

        public string ScannedLength { get { return Util.FormatSize(SubItems[8]); } internal set { SetProperty(8, value); } }
        public ulong ScannedLengthRaw => ("" + SubItems[8]).ToUlong();

        internal override int NumCols => NumCols_;
        internal const int NumCols_ = 9;

        protected override IReadOnlyList<string> _propNames { get { return _propNamesA; } set { _propNamesA = value; } }
        static IReadOnlyList<string> _propNamesA = null;

        internal LVitem_ProjectVM(IList<string> lsStr = null)
            : base(null, lsStr)
        {
            if (null == Status)
                Status = FileParse.ksNotSaved;
        }

        internal LVitem_ProjectVM(LVitem_ProjectVM lvItemTemp)
            : this(lvItemTemp?.SubItems.ToList() ?? null)
        {
        }

        internal bool Include
        {
            get { return (IncludeYN == FileParse.ksIncludeYes); }
            set { IncludeYN = (value ? FileParse.ksIncludeYes : FileParse.ksIncludeNo); } 
        }

        internal bool WouldSave => (false == _ksFileExistsCheck.Contains(Status));
        internal bool CanLoad => (Include && _ksFileExistsCheck.Contains(Status) && (FileParse.ksError != Status));

        internal string Volume
        {
            get
            {
                if (false == string.IsNullOrWhiteSpace(VolumeGroup))
                    return VolumeGroup;

                if (false == string.IsNullOrWhiteSpace(Nickname))
                    return RootText;

                return SourcePath;
            }
        }

        internal string
            RootText
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Nickname))
                    return SourcePath;

                var sourcePathNoSlash = SourcePath.TrimEnd('\\');

                if (("" + Nickname).EndsWith(sourcePathNoSlash))
                    return Nickname;

                return Nickname + " (" + sourcePathNoSlash + ")";
            }
        }

        internal void SetSaved()
        {
            LVitem_ProjectVM lvItem = null;

            if (FileParse.ReadHeader(ListingFile, ref lvItem))
            {
                SubItems = lvItem.SubItems;
                LinesTotal = lvItem.LinesTotal;
                HashV2 = lvItem.HashV2;
                Status = FileParse.ksSaved;
            }
            else
            {
                Status = FileParse.ksError;
            }
        }

        internal int LinesTotal;
        internal bool HashV2;

        readonly string _ksFileExistsCheck = FileParse.ksUsingFile + FileParse.ksSaved;
    }
}
