using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DoubleFile
{
    class LocalLVitem : LocalColorItemBase
    {
        internal UString
            _Text = null;
        internal UString
            _Name = null;
        internal object
            _Tag = null;
        internal LocalLVitemCollection
            _SubItems = null;
        internal LocalLV
            _ListView = null;

//      internal bool Focused;

        internal void
            Select(bool bSel = true) { }
        internal int
            Index { get { return Datum16bits; } set { Datum16bits = value; } }
        internal void
            EnsureVisible() { }

        internal LocalLVitem(LocalLV listView = null)
        {
            _ListView = listView;
            _SubItems = new LocalLVitemCollection(_ListView);
        }

        internal LocalLVitem(string strContent, LocalLV listView = null) : this(listView) { _Text = strContent; Index = -1; }

        internal LocalLVitem(IReadOnlyList<string> asString, LocalLV listView = null)
            : this(listView)
        {
            _Text = asString[0];
            _SubItems.Add(this);

            var i = 1;

            foreach (var s in asString.Skip(1))
            {
                _SubItems.Add(new LocalLVitem(asString[i++], listView));
            }
        }

        // Only used for colors and bold font weight, not subitems, in Collate.cs InsertSizeMarker(). Size 18 to show obvious fault in interpretation.
        internal object Clone() { var lvItem = (LocalLVitem)MemberwiseClone(); lvItem.FontWeight = FontWeight; return lvItem; }
        internal System.Windows.FontWeight FontWeight
        {
            get { return (Datum6bits != 0) ? System.Windows.FontWeights.Bold : System.Windows.FontWeights.Normal; }
            set { Datum6bits = (value == System.Windows.FontWeights.Normal) ? 0 : -1; }
        }
    }
}
