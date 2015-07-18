using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace DoubleFile
{
    class LocalLVitem : ListViewItemVM_Base, ILocalColorItemBase
    {
        public int ForeColor { get { return _classObject.ForeColor; } set { _classObject.ForeColor = value; } }
        public int BackColor { get { return _classObject.BackColor; } set { _classObject.BackColor = value; } }

        protected override string[] _propNames { get { return _propNamesA; } set { _propNamesA = value; } }
        static string[] _propNamesA = null;

        internal override int NumCols { get { return NumCols_; } }
        internal const int NumCols_ = 9;

        internal TabledString<Tabled_Folders>
            Text { get; set; }
        internal TabledString<Tabled_Folders>
            Name { get; set; }
        //internal LocalLVitem
        //    IgnoreLVItem { get; set; }
        internal List<LocalTreeNode>
            TreeNodes { get; set; }
        internal LocalTreeNode
            LocalTreeNode { get; set; }

        internal LocalLVitem[]
            SubItems { get; set; }
        internal LocalLV
            ListView { get; set; }

//      internal bool Focused;

        internal void
            Select(bool bSel = true) { }
        internal int
            Index { get { return _classObject.Datum16bits_ClassObject; } set { _classObject.Datum16bits_ClassObject = value; } }
        internal void
            EnsureVisible() { }

        internal LocalLVitem(LocalLV listView = null)
            : base(null, null)
        {
            ListView = listView;
        }

        internal LocalLVitem(string strContent, LocalLV listView = null)
            : this(listView) { Text = strContent; Index = -1; }

        internal LocalLVitem(IEnumerable<string> asString, LocalLV listView = null)
            : this(listView)
        {
            var lsLVItems = new List<LocalLVitem>();

            Text = asString.FirstOrDefault();
            lsLVItems.Add(this);

            foreach (var s in asString.Skip(1))
                lsLVItems.Add(new LocalLVitem(s, listView));

            SubItems = lsLVItems.ToArray();
        }

        // Only used for colors and bold font weight, not subitems, in Collate.cs InsertSizeMarker(). Size 18 to show obvious fault in interpretation.
        internal object Clone() { var lvItem = (LocalLVitem)MemberwiseClone(); lvItem.FontWeight = FontWeight; return lvItem; }

        internal FontWeight FontWeight
        {
            get { return (_classObject.Datum8bits_ClassObject != 0) ? FontWeights.Bold : FontWeights.Normal; }
            set { _classObject.Datum8bits_ClassObject = (value == FontWeights.Normal) ? 0 : -1; }
        }

        LocalColorItemBase_ClassObject _classObject = new LocalColorItemBase_ClassObject();
    }
}
