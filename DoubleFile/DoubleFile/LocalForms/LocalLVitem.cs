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
        internal LocalLVitem(LocalLV listView = null)
        {
            ListView = listView;
            SubItems = new LocalLVitemCollection(ListView);
        }

        internal LocalLVitem(string strContent, LocalLV listView = null) : this(listView) { Text = strContent; }

        internal LocalLVitem(IReadOnlyList<string> asString, LocalLV listView = null)
            : this(listView)
        {
            Text = asString[0];
            SubItems.Add(this);

            var i = 1;

            foreach (var s in asString.Skip(1))
            {
                SubItems.Add(new LocalLVitem(asString[i++], listView));
            }
        }

        internal string Text = null;
        internal string Name = null;
        internal object Tag = null;
        internal void Select(bool bSel = true) { }
  //      internal bool Focused;
        internal int Index = -1;
        internal LocalLVitemCollection SubItems = null;
        internal void EnsureVisible() { }
        internal LocalLV ListView = null;

        // Only used for colors and bold font weight, not subitems, in Collate.cs InsertSizeMarker(). Size 18 to show obvious fault in interpretation.
        internal object Clone() { var lvItem = (LocalLVitem)MemberwiseClone(); lvItem.Font = (System.Drawing.Font)Font.Clone(); return lvItem; }
        internal System.Drawing.Font Font { get { return new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold); } set { FontWeight = System.Windows.FontWeights.Bold; } }
        internal System.Windows.FontWeight FontWeight = System.Windows.FontWeights.Normal;
    }
}
