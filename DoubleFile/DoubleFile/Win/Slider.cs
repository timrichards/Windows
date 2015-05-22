using System;
using System.Reactive.Linq;

namespace DoubleFile
{
    abstract class Slider<T> : ListViewVM_Base<T>
        where T : ListViewItemVM_Base
    {
        internal abstract void GoTo(LocalTreeNode treeNode);

        public double Maximum { get; private set; }
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
                RaisePropertyChanged("Maximum");
                _deepNode = value;
            }
        }
        LocalTreeNode _deepNode = null;

        public double Value { get; set; }
        internal LocalTreeNode TreeNode
        {
            get { return _treeNode; }
            set
            {
                var nCount = 0;

                for (var treeNode = _deepNode;
                    ((null != treeNode) && (false == ReferenceEquals(value, treeNode)));
                    treeNode = treeNode.Parent)
                {
                    ++nCount;
                }

                Value = nCount;
                RaisePropertyChanged("Value");
                _treeNode = value;
            }
        }
        LocalTreeNode _treeNode = null;

        internal void LostMouseCapture()
        {
            if (null == _deepNode)
                return;

            var treeNode = _deepNode;

            for (var nCount = 0; nCount < Value; ++nCount)
                treeNode = treeNode.Parent;

            GoTo(treeNode);
        }
    }
}
