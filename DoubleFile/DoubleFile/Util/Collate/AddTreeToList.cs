using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    using static UtilColorcode;

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

                    if (0 < treeNode.NodeDatum.LengthTotal)
                    {
                        if ((AllOnOneVolume == treeNode.ColorcodeFG) &&
                            (treeNode == treeNode.Clones[0]))
                        {
                            _lsSameVol.Add(treeNode);
                        }

                        if (bCloneOK)
                        {
                            treeNode.ColorcodeBG = ParentClonedBG;

                            if (false ==
                                new[] { SolitaryClonedParent, SolitaryOneVolParent, SolitAllDupesOneVol, ManyClonesSepVolume, ZeroLengthFolder }
                                .Contains(treeNode.ColorcodeFG))
                            {
                                var bExpected =
                                    new[] { Solitary, Transparent,
                                    ChildAllOnOneVolume, ChildClonedSepVolume,
                                    OneCloneSepVolume,
                                    AllOnOneVolume }      // A Useful Find
                                    .Contains(treeNode.ColorcodeFG);

                                Util.Assert(99859, bExpected, bIfDefDebug: true);

                                if (bExpected)
                                    treeNode.ColorcodeFG = ParentCloned;
                            }

                            //if ((nodeDatum.LVitem != null) && (nodeDatum.LVitem.ListView == null))  // ignore LV
                            //{
                            //    nodeDatum.LVitem.BackColor = treeNode.BackColor;
                            //}
                        }
                    }

                    if (null != treeNode.Nodes)
                        Go(treeNode.Nodes, bCloneOK || new[] { OneCloneSepVolume, ManyClonesSepVolume }.Contains(treeNode.ColorcodeFG));
                }
            }

            IList<LocalTreeNode>
                _lsAllNodes = null;
            IList<LocalTreeNode>
                _lsSameVol = null;
        }
    }
}
