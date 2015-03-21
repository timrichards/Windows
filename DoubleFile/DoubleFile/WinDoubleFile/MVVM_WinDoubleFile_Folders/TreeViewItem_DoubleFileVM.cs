using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace DoubleFile
{
    partial class TreeViewItem_DoubleFileVM : ObservableObjectBase
    {
        public string Text { get { return ((string)_datum.Text).PadRight(200); } }
        public Brush Foreground { get { return _isSelected ? Brushes.White : FrontBrush; } }
        public Brush SelectedForeground { get { return _isSelected ? Brushes.White : FrontBrush; } }

        public Brush Background { get { return UtilColor.ARGBtoBrush(_datum.BackColor); } }
        public FontWeight FontWeight { get { return _isSelected ? FontWeights.ExtraBold : FontWeights.Normal; } }

        internal readonly TreeViewItem_DoubleFileVM
            _Parent = null;
        internal readonly TreeView_DoubleFileVM
            _TVVM = null;
        internal readonly LocalTreeNode
            _datum = null;

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
            get { return _isExpanded; }
            set
            {
                if (value == _isExpanded)
                    return;

                _isExpanded = value;

                if (_isExpanded)
                    _TVVM._listExpanded.Add(this);
                else
                    _TVVM._listExpanded.Remove(this);
            }
        }
        bool _isExpanded = false;

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value == _isSelected)
                    return;

                _isSelected = value;
                DoTreeSelect();
                SelectedItem_AllTriggers();
            }
        }
        internal void SelectedItem_Set(bool value = true)
        {
            if (value == _isSelected)
                return;

            _isSelected = value;

            if (false == _isSelected)
            {
                RaisePropertyChanged("IsSelected");
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
                    tvivm._isExpanded))
            {
                tvivm._isExpanded = false;
                tvivm.RaisePropertyChanged("IsExpanded");
                _TVVM._listExpanded.Remove(tvivm);
            }

            while (false == stackParents.IsEmpty())
            {
                parentItem = stackParents.Pop();

                if (parentItem._isExpanded == false)
                {
                    parentItem._isExpanded = true;
                    parentItem.RaisePropertyChanged("IsExpanded");
                    _TVVM._listExpanded.Add(parentItem);
                }
            }

            _TVVM._listExpanded = listParents;
            RaisePropertyChanged("IsSelected");
            SelectedItem_AllTriggers();
        }
        void SelectedItem_AllTriggers()
        {
            if (_isSelected)
                _TVVM.SelectedItem = this;
            else if (_TVVM.SelectedItem == this)
                _TVVM.SelectedItem = null;
        }
        bool _isSelected = false;

        internal TreeViewItem_DoubleFileVM(TreeView_DoubleFileVM tvvm, LocalTreeNode datum_in, int nIndex)
            : this(tvvm, datum_in, null, nIndex)
        { }

        TreeViewItem_DoubleFileVM(TreeView_DoubleFileVM tvvm, LocalTreeNode datum_in,
            TreeViewItem_DoubleFileVM parent, int nIndex)
        {
            _TVVM = tvvm;
            _datum = datum_in;
            _Parent = parent;
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
    }
}
