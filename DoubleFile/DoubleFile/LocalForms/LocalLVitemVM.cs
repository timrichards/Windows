﻿using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace DoubleFile
{
    class LocalLVitemVM : ListViewItemVM_Base, ILocalColorItemBase
    {
        internal string
            Folder { get { return SubItems[0]; } set { SubItems[0] = value; } }
        internal string
            Clones { get { return SubItems[1]; } }

        public Brush Foreground { get { return _classObject.Foreground; } }
        public Brush Background { get { return _classObject.Background; } }

        internal FontWeight FontWeight
        {
            get { return (_classObject.Datum8bits_ClassObject != 0) ? FontWeights.Bold : FontWeights.Normal; }
            set { _classObject.Datum8bits_ClassObject = (value == FontWeights.Normal) ? 0 : -1; }
        }

        public int ForeColor { get { return _classObject.ForeColor; } set { _classObject.ForeColor = value; } }
        public int BackColor { get { return _classObject.BackColor; } set { _classObject.BackColor = value; } }

        protected override string[] _propNames { get { return _propNamesA; } set { _propNamesA = value; } }
        static string[] _propNamesA = null;

        internal override int NumCols { get { return NumCols_; } }
        internal const int NumCols_ = 2;

        internal TabledString<Tabled_Folders>
            Name { get; set; }
        //internal LocalLVitem
        //    IgnoreLVItem { get; set; }
        internal IList<LocalTreeNode>
            TreeNodes = new LocalTreeNode[0];

        internal LocalLVVM
            ListView { get; set; }

        internal int
            Index { get { return _classObject.Datum16bits_ClassObject; } set { _classObject.Datum16bits_ClassObject = value; } }

        internal LocalLVitemVM(IList<string> asString, LocalLVVM listView = null)
            : base(null, asString)
        {
            ListView = listView;
        }

        LocalColorItemBase_ClassObject
            _classObject = new LocalColorItemBase_ClassObject();
    }
}
