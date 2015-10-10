using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    partial class Collate
    {
        // can't be struct because it has an auto-implemented property
        class AddTreeToList
        {
            static internal void Go(IList<LocalTreeNode> lsAllNodes, IList<LocalTreeNode> lsSameVol,
                IEnumerable<LocalTreeNode> lsNodes)
            {
                new AddTreeToList
                {
                    _lsAllNodes = lsAllNodes,
                    _lsSameVol = lsSameVol,
                }
                    .Go(lsNodes);
            }

            void Go(IEnumerable<LocalTreeNode> lsNodes, bool bCloneOK = false)
            {
                foreach (var treeNode in lsNodes)
                {
                    _lsAllNodes.Add(treeNode);

                    if ((treeNode.ForeColor == UtilColorcode.AllOnOneVolume) &&
                        (treeNode == treeNode.NodeDatum.Clones[0]))
                    {
                        _lsSameVol.Add(treeNode);
                    }

                    if (bCloneOK)
                    {
                        treeNode.BackColor = UtilColorcode.ParentCloned;

                        //if ((nodeDatum.LVitem != null) && (nodeDatum.LVitem.ListView == null))  // ignore LV
                        //{
                        //    nodeDatum.LVitem.BackColor = treeNode.BackColor;
                        //}
                    }

                    if (null != treeNode.Nodes)
                    {
                        Go(treeNode.Nodes, bCloneOK ||
                            (new[] { UtilColorcode.OneCopy, UtilColorcode.MultipleCopies }.Contains(treeNode.ForeColor)));
                    }
                }
            }

            IList<LocalTreeNode>
                _lsAllNodes = null;
            IList<LocalTreeNode>
                _lsSameVol = null;
        }
    }
}
