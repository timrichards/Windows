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

            AddTreeToList Go(IEnumerable<LocalTreeNode> lsNodes)
            {
                foreach (var treeNode in lsNodes)
                    Go(treeNode, bNextNode: false);

                return this;
            }

            void Go(LocalTreeNode treeNode_in, bool bCloneOK = false, bool bNextNode = true)
            {
                if (treeNode_in == null)
                {
                    Util.Assert(1305.6302m, false);
                    return;
                }

                var treeNode = treeNode_in;

                do
                {
                    _lsAllNodes.Add(treeNode);

                    var nodeDatum = treeNode.NodeDatum;

                    if (null == nodeDatum)
                    {
                        Util.Assert(1305.6303m, false);
                        continue;
                    }

                    if ((treeNode.ForeColor == UtilColorcode.AllOnOneVolume) &&
                        (treeNode == nodeDatum.Clones[0]))
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

                    if (treeNode.FirstNode != null)
                    {
                        Go(treeNode.FirstNode, bCloneOK ||
                            (new[] { UtilColorcode.OneCopy, UtilColorcode.MultipleCopies }.Contains(treeNode.ForeColor)));
                    }
                }
                while (bNextNode && ((treeNode = treeNode.NextNode) != null));
            }

            IList<LocalTreeNode>
                _lsAllNodes = null;
            IList<LocalTreeNode>
                _lsSameVol = null;
        }
    }
}
