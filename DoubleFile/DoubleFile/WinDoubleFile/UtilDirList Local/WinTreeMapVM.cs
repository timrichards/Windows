using System;
using System.Reactive.Subjects;
using System.Reactive.Linq;

namespace DoubleFile
{
    class WinTreeMapVM : Observable_OwnerWindowBase
    {
        internal IObservable<Tuple<LocalTreeNode, int>>
            TreeNodeCallback { get { return _treeNodeCallback.AsObservable(); } }
        readonly LocalSubject<LocalTreeNode> _treeNodeCallback = new LocalSubject<LocalTreeNode>();
        internal void GoTo(LocalTreeNode treeNode) { _treeNodeCallback.LocalOnNext(treeNode, 99853); }

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
