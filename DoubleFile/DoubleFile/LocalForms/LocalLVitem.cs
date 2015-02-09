﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace DoubleFile
{
    class LocalLVitem
    {
        internal LocalLVitem(LocalLV listView = null) { SubItems = new LocalLVitemCollection(ListView); }
        internal LocalLVitem(string strContent, LocalLV listView = null) : this(listView) { Text = strContent; }

        internal LocalLVitem(string[] arrString, LocalLV listView = null)
            : this(listView)
        {
            Text = arrString[0];
            SubItems.Add(this);

            for (int i = 1; i < arrString.Length; ++i)
            {
                SubItems.Add(new LocalLVitem(arrString[i], listView));
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

        internal int BackColor = UtilColor.Empty;
        internal int ForeColor = UtilColor.Empty;

        // Only used for colors and bold font weight, not subitems, in Collate.cs InsertSizeMarker(). Size 18 to show obvious fault in interpretation.
        internal object Clone() { LocalLVitem lvItem = (LocalLVitem)MemberwiseClone(); lvItem.Font = (System.Drawing.Font)Font.Clone(); return lvItem; }
        internal System.Drawing.Font Font { get { return new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold); } set { FontWeight = System.Windows.FontWeights.Bold; } }
        internal System.Windows.FontWeight FontWeight = System.Windows.FontWeights.Normal;
    }
}
