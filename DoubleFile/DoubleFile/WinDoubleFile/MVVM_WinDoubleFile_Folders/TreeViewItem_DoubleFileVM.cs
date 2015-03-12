﻿using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace DoubleFile
{
    partial class TreeViewItem_DoubleFileVM : ObservableObjectBase
    {
        public string Text { get { return ((string)_datum.Text).PadRight(200); } }
        public Brush Foreground { get { return _bSelected ? _SelectedForeground : FrontBrush; } }
        public Brush SelectedForeground { get { return _bSelected ? _SelectedForeground : FrontBrush; } }
        Brush _SelectedForeground { get { return Brushes.White; } set { } }

        public Brush Background { get { return UtilColor.ARGBtoBrush(_datum.BackColor); } }
        public FontWeight FontWeight { get { return _bSelected ? FontWeights.ExtraBold : FontWeights.Normal; } }

        internal readonly TreeViewItem_DoubleFileVM
            _Parent = null;
        internal readonly TreeView_DoubleFileVM
            _TVVM = null;
        internal readonly LocalTreeNode
            _datum = null;

        // turned these dependency property hacks into properties that are always -1
        internal double
            EphemeralExpandedPos { get { return -1; } set { } }
        internal int
            Index { get { return -1; } set { } }

        public ObservableCollection<TreeViewItem_DoubleFileVM> Items
        {
            get
            {
                if (null == _Items)
                {
                    var nIndex = -1;

                    _Items = new ObservableCollection<TreeViewItem_DoubleFileVM>
                    (
                        from
                            item
                            in _datum.Nodes.Keys
                            select new TreeViewItem_DoubleFileVM(_TVVM, item, this, ++nIndex)
                    );
                }

                return _Items;
            }
        }
        ObservableCollection<TreeViewItem_DoubleFileVM> _Items = null;

        public bool IsExpanded
        {
            get { return _bExpanded; }
            set
            {
                if (value == _bExpanded)
                {
                    return;
                }

                _bExpanded = value;

                if (_bExpanded)
                {
                    _TVVM._listExpanded.Add(this);
                }
                else
                {
                    _TVVM._listExpanded.Remove(this);
                }
            }
        }

        public bool IsSelected
        {
            get { return _bSelected; }
            set
            {
                if (value == _bSelected)
                {
                    return;
                }

                _bSelected = value;
                EphemeralExpandedPos = -1;
                _SelectedForeground = Brushes.White;

                if (_bSelected)
                {
                    _TVVM.SelectedItem = this;
                }
                else if (_TVVM.SelectedItem == this)
                {
                    _TVVM.SelectedItem = null;
                }

                DoTreeSelect();
            }
        }

        internal void SelectProgrammatic(bool bSelect)
        {
            if (_bSelected == bSelect)
            {
                return;
            }

            if (bSelect == false)
            {
                IsSelected = false;
                EphemeralExpandedPos = -1;
                return;
            }

            _TVVM.SelectedItem = this;

            var stackParents = new Stack<TreeViewItem_DoubleFileVM>(8);
            var listParents = new KeyList<TreeViewItem_DoubleFileVM>();
            var parentItem = _Parent;

            while (parentItem != null)
            {
                stackParents.Push(parentItem);
                listParents.Add(parentItem);
                parentItem = parentItem._Parent;
            }

            foreach (var tvivm in _TVVM._listExpanded.ToArray()
                .Where(tvivm => (stackParents.Contains(tvivm) == false) &&
                    tvivm._bExpanded))
            {
                tvivm.EphemeralExpandedPos = -1;
                tvivm._bExpanded = false;
                tvivm.RaisePropertyChanged("IsExpanded");
                _TVVM._listExpanded.Remove(tvivm);
            }

            while (false == stackParents.IsEmpty())
            {
                parentItem = stackParents.Pop();
                EphemeralExpandedPos += (parentItem.Index + 1);
                parentItem.EphemeralExpandedPos = EphemeralExpandedPos * _HeaderHeight;

                if (parentItem._bExpanded == false)
                {
                    parentItem._bExpanded = true;
                    parentItem.RaisePropertyChanged("IsExpanded");
                    _TVVM._listExpanded.Add(parentItem);
                }
            }

            EphemeralExpandedPos += (Index + 1);
            EphemeralExpandedPos *= _HeaderHeight;       // when implementing variable-height headers this calc will be wrong
            _TVVM._listExpanded = listParents;
            IsSelected = true;
        }

        internal TreeViewItem_DoubleFileVM(TreeView_DoubleFileVM tvvm, LocalTreeNode datum_in, int nIndex)
            : this(tvvm, datum_in, null, nIndex)
        { }

        TreeViewItem_DoubleFileVM(TreeView_DoubleFileVM tvvm, LocalTreeNode datum_in,
            TreeViewItem_DoubleFileVM parent, int nIndex)
        {
            _TVVM = tvvm;
            _datum = datum_in;
            _Parent = parent;
            Index = nIndex;
         //   datum.TVIVM = this;
         //   m_Foreground = SDLWPF._ForeClrToBrush(datum.ForeColor);
        }

        Brush FrontBrush
        {
            get
            {
                return
                    (UtilColor.Empty == _datum.ForeColor)
                    ? Brushes.Black
                    : UtilColor.ARGBtoBrush(_datum.ForeColor);
            }
        }

        static double
            _HeaderHeight { get { return -1; } }

        bool _bExpanded = false;
        bool _bSelected = false;
    }
}