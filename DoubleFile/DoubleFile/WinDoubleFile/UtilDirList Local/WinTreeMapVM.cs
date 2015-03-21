using System;
using System.Windows.Media;

namespace DoubleFile
{
    class WinTreeMapVM : ObservableObject_OwnerWindow
    {
        internal Action<LocalTreeNode> TreeNodeCallback = null;

        public double Maximum { get { return _maximum; } private set { _maximum = value; RaisePropertyChanged("Maximum"); } }
        double _maximum;

        public double Value
        {
            get { return _value; }

            set
            {
                if (_bSettingValue)
                    return;

                if (_value == value)
                    return;

                var treeNode = _deepNode;

                for (var nCount = 0; nCount < value; ++nCount)
                    treeNode = treeNode.Parent;

                TreeNodeCallback(treeNode);
                _value = value;
            }
        }
        double _value;

        internal LocalTreeNode DeepNode
        {
            get { return _deepNode; }

            set
            {
                if (value == _deepNode)
                    return;

                var nMaximum = 0;

                for (var treeNode = value.Parent;
                    null != treeNode;
                    treeNode = treeNode.Parent)
                {
                    ++nMaximum;
                }

                Maximum = nMaximum;
                _deepNode = value;
            }
        }
        LocalTreeNode _deepNode = null;

        internal LocalTreeNode TreeNode
        {
            get { return _treeNode; }

            set
            {
                if (_bSettingValue)
                    return;

                _bSettingValue = true;

                var nCount = 0;

                for (var treeNode = _deepNode;
                    ((null != treeNode) && (false == ReferenceEquals(value, treeNode)));
                    treeNode = treeNode.Parent)
                {
                    ++nCount;
                }

                _value = nCount;
                RaisePropertyChanged("Value");
                _bSettingValue = false;
                _treeNode = value;
            }
        }
        LocalTreeNode _treeNode = null;

        bool _bSettingValue = false;
    }
}
