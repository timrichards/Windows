using System;
using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    abstract class LVitem_FilesVM_Base : ListViewItemVM_Base
    {
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
            FileLine { get { return (IReadOnlyList<string>)SubItems; } set { SubItems = (IList<string>)value; } }
        internal override string
            ExportLine => string.Join(" ", SubItems.Take(6)).Trim();

        internal override int NumCols => NumCols_;
        internal const int NumCols_ = 9;
    }
}
