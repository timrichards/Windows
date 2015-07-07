﻿using System.Collections.Generic;

namespace DoubleFile
{
    class LVitem_ProjectVM : ListViewItemVM_Base
    {
        public string Nickname { get { return marr[0]; } internal set { SetProperty(0, value); } }
        public string SourcePath { get { return marr[1]; } internal set { SetProperty(1, value); } }

        public string ListingFile { get { return marr[2]; } set { SetProperty(2, value); } }
        public string ListingFileNoPath { get { return System.IO.Path.GetFileName(marr[2]); } }

        public string Status { get { return marr[3]; } internal set { SetProperty(3, value); } }
        public string IncludeYN { get { return marr[4]; } private set { SetProperty(4, value); } }
        public string VolumeGroup { get { return marr[5]; } internal set { SetProperty(5, value); } }
        public string DriveModel { get { return marr[6]; } internal set { SetProperty(6, value); } }
        public string DriveSerial { get { return marr[7]; } internal set { SetProperty(7, value); } }

        public string ScannedLength { get { return Util.FormatSize(marr[8]); } internal set { SetProperty(8, value); } }
        public ulong ScannedLengthRaw { get { return ("" + marr[8]).ToUlong(); } }

        internal int LinesTotal { get; set; }
        internal bool HashV2 { get; set; }

        internal override int NumCols { get { return NumCols_; } }
        internal const int NumCols_ = 9;

        protected override string[] _propNames { get { return _propNamesA; } set { _propNamesA = value; } }
        static string[] _propNamesA = null;

        internal LVitem_ProjectVM(IList<string> lsStr = null)
            : base(null, lsStr)
        {
            if (null == Status)
                Status = FileParse.ksNotSaved;
        }

        internal LVitem_ProjectVM(LVitem_ProjectVM lvItemTemp)
            : this(lvItemTemp.StringValues)
        {
        }

        internal bool Include
        {
            get { return (IncludeYN == FileParse.ksIncludeYes); }
            set { IncludeYN = (value ? FileParse.ksIncludeYes : FileParse.ksIncludeNo); } 
        }

        internal bool WouldSave
        {
            get { return (false == (_ksFileExistsCheck + FileParse.ksError).Contains(Status)); }
        }

        internal bool CanLoad
        {
            get { return (Include && _ksFileExistsCheck.Contains(Status) && (FileParse.ksError != Status)); }
        }

        internal string Volume
        {
            get
            {
                if (false == string.IsNullOrWhiteSpace(VolumeGroup))
                    return VolumeGroup;

                if (false == string.IsNullOrWhiteSpace(Nickname))
                    return RootText(Nickname, SourcePath);

                return SourcePath;
            }
        }

        static internal string RootText(string strNickname, string strSourcePath)
        {
            if (string.IsNullOrWhiteSpace(strSourcePath))
                return strNickname;

            if (("" + strNickname).EndsWith(strSourcePath))
                return strNickname;

            return strNickname + " (" + strSourcePath + ")";
        }

        internal void SetSaved()
        {
            LVitem_ProjectVM lvItem = null;

            if (FileParse.ReadHeader(ListingFile, out lvItem))
            {
                StringValues = lvItem.StringValues;
                LinesTotal = lvItem.LinesTotal;
                HashV2 = lvItem.HashV2;
                Status = FileParse.ksSaved;
            }
            else
            {
                Status = FileParse.ksError;
            }
        }

        readonly string _ksFileExistsCheck = FileParse.ksUsingFile + FileParse.ksSaved;
    }
}
