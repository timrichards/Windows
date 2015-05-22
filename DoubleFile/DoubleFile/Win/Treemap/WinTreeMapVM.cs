using System;
using System.Reactive.Linq;

namespace DoubleFile
{
    class WinTreeMapVM : Slider<ListViewItemVM_Base>
    {
        internal override int NumCols { get { return 0; } }

        internal IObservable<Tuple<LocalTreeNode, int>>
            TreeNodeCallback { get { return _treeNodeCallback.AsObservable(); } }
        readonly LocalSubject<LocalTreeNode> _treeNodeCallback = new LocalSubject<LocalTreeNode>();
        internal override void GoTo(LocalTreeNode treeNode) { _treeNodeCallback.LocalOnNext(treeNode, 99982); }
    }
}
