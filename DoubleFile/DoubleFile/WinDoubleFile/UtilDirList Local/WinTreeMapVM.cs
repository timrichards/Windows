using System;
using System.Windows.Media;

namespace DoubleFile
{
    class WinTreeMapVM : ObservableObject_OwnerWindow
    {
        internal Action<LocalTreeNode> TreeNodeCallback = null;

        public double Maximum { get { return _maximum; } internal set { _maximum = value; RaisePropertyChanged("Maximum"); } }
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

                for (var nCount = 0; nCount++ < value; ++nCount)
                    treeNode = treeNode.Parent;

                TreeNodeCallback(treeNode);
            }
        }
        double _value;

        internal LocalTreeNode DeepNode
        {
            set
            {
                if (value == _deepNode)
                    return;

                var nMaximum = 0;
                var treeNode = value.Parent;

                while (null != treeNode)
                {
                    ++nMaximum;
                    treeNode = treeNode.Parent;
                }

                Maximum = nMaximum;
                _deepNode = value;
            }
        }
        LocalTreeNode _deepNode = null;

        internal LocalTreeNode TreeNode
        {
            set
            {
                if (_bSettingValue)
                    return;

                _bSettingValue = true;

                var nCount = 0;
                var treeNode = _deepNode;

                while (false == ReferenceEquals(value, treeNode))
                {
                    treeNode = treeNode.Parent;
                    ++nCount;
                }

                _value = nCount; RaisePropertyChanged("Value");
                _bSettingValue = false;
            }
        }

        bool _bSettingValue = false;
    }
}
