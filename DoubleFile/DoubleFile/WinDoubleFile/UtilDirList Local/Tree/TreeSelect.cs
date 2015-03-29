using System.Collections.Generic;
using System.Threading;

namespace DoubleFile
{
    partial class TreeSelect
    {
        internal TreeSelect(LocalTreeNode node,
            bool bCompareMode = false, bool bSecondComparePane = false)
        {
            _treeNode = node;

            if (null != LocalTreeNode.TreeView)
                _dictVolumeInfo = LocalTreeNode.TreeView._dictVolumeInfo;

            _bCompareMode = bCompareMode;
            _bSecondComparePane = bSecondComparePane;
        }

        internal Thread DoThreadFactory()
        {
            if (_treeNode is LocalTreeMapFileNode)     // does not support immediate file fake nodes
                return null;

            _thread = new Thread(Go) { IsBackground = true };
            _thread.Start();
            return _thread;
        }

        readonly LocalTreeNode
            _treeNode = null;
        readonly Dictionary<string, string>
            _dictVolumeInfo = null;
        Thread
            _thread = null;
        readonly bool
            _bCompareMode = false;
        readonly bool
            _bSecondComparePane = false;
    }
}
