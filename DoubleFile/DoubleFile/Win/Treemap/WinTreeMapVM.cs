using System;
using System.Reactive.Linq;

namespace DoubleFile
{
    class WinTreeMapVM : SliderVM_Base<ListViewItemVM_Base>
    {
        internal override int NumCols => 0;

        internal IObservable<Tuple<LocalTreeNode, int>>
            TreeNodeCallback => _treeNodeCallback;
        readonly LocalSubject<LocalTreeNode> _treeNodeCallback = new LocalSubject<LocalTreeNode>();
        internal override void GoTo(LocalTreeNode treeNode) { _treeNodeCallback.LocalOnNext(treeNode, 99853); }
    }
}
