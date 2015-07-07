using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace DoubleFile
{
    partial class TreeViewItemVM : ObservableObjectBase
    {
        public string Text { get { return ("" + _datum.Text); } }
        public Brush Foreground { get { return _isSelected ? Brushes.Transparent : _datum.Foreground; } }
        public Brush SelectedForeground { get { return _isSelected ? Brushes.Transparent : _datum.Foreground; } }

        public Brush Background { get { return UtilColor.ARGBtoBrush(_datum.BackColor); } }
        public FontWeight FontWeight { get { return _isSelected ? FontWeights.ExtraBold : FontWeights.Normal; } }

        internal readonly TreeViewItemVM
            _Parent = null;
        internal readonly TreeViewVM
            _TVVM = null;
        internal readonly LocalTreeNode
            _datum = null;

        public ObservableCollection<TreeViewItemVM> Items
        {
            get
            {
                if ((null == _Items) &&
                    (null != _datum.Nodes))
                {
                    var nIndex = -1;

                    _Items = new ObservableCollection<TreeViewItemVM>
                    (
                        from
                            item
                            in _datum.Nodes
                            select new TreeViewItemVM(_TVVM, item, this, ++nIndex)
                    );
                }

                return _Items;
            }
        }
        ObservableCollection<TreeViewItemVM> _Items = null;

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (value == _isExpanded)
                    return;

                _isExpanded = value;

                if (_isExpanded)
                    _TVVM._listExpanded.Add(this, false);
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
                SelectedItem_AllTriggers(0);
            }
        }
        internal void SelectedItem_Set(bool value, int nInitiator)
        {
            if (value == _isSelected)
                return;

            if (false == value)
            {
                _isSelected = false;
                RaisePropertyChanged("IsSelected");
                return;
            }

            _TVVM.SelectedItem = this;

            var stackParents = new Stack<TreeViewItemVM>(8);
            var listParents = new Dictionary<TreeViewItemVM, bool>();
            var parentItem = _Parent;

            while (parentItem != null)
            {
                stackParents.Push(parentItem);
                listParents.Add(parentItem, false);
                parentItem = parentItem._Parent;
            }

            foreach (var tvivm in _TVVM._listExpanded.Keys.ToArray()
                .Where(tvivm => (false == stackParents.Contains(tvivm)) &&
                    tvivm._isExpanded))
            {
                tvivm._isExpanded = false;
                tvivm.RaisePropertyChanged("IsExpanded");
                _TVVM._listExpanded.Remove(tvivm);
            }

            while (0 < stackParents.Count)
            {
                parentItem = stackParents.Pop();

                if (parentItem._isExpanded == false)
                {
                    parentItem._isExpanded = true;
                    parentItem.RaisePropertyChanged("IsExpanded");
                    _TVVM._listExpanded.Add(parentItem, false);
                }
            }

            _TVVM._listExpanded = listParents;

            if (_isSelected)     // through the above IsExpanded logic
                return;

            _isSelected = true;

            RaisePropertyChanged("IsSelected");
            SelectedItem_AllTriggers(nInitiator);
        }
        void SelectedItem_AllTriggers(int nInitiator)
        {
            if (_isSelected)
                _TVVM.SelectedItem = this;
            else if (_TVVM.SelectedItem == this)
                _TVVM.SelectedItem = null;

            if (false == _isSelected)
                return;

            TreeSelect.DoThreadFactory(_datum, nInitiator);
        }
        bool _isSelected = false;

        internal TreeViewItemVM(TreeViewVM tvvm, LocalTreeNode datum_in, int nIndex)
            : this(tvvm, datum_in, null, nIndex)
        { }

        TreeViewItemVM(TreeViewVM tvvm, LocalTreeNode datum_in,
            TreeViewItemVM parent, int nIndex)
        {
            _TVVM = tvvm;
            _datum = datum_in;
            _Parent = parent;
        }
    }
}
