using System.Collections.Generic;
using System.Linq;

namespace DoubleFile
{
    partial class Collate
    {
        // can't be struct because it has an auto-implemented property
        class AddTreeToList
        {
            internal int Count { get; private set; }

            internal AddTreeToList(List<LocalTreeNode> listTreeNodes, List<LocalTreeNode> listSameVol)
            {
                _listTreeNodes = listTreeNodes;
                _listSameVol = listSameVol;
            }

            internal AddTreeToList Go(IEnumerable<LocalTreeNode> listNodes)
            {
                foreach (var treeNode in listNodes)
                {
                    Go(treeNode, bNextNode: false);
                }

                return this;
            }

            void Go(LocalTreeNode treeNode_in, bool bCloneOK = false, bool bNextNode = true)
            {
                if (treeNode_in == null)
                {
                    MBoxStatic.Assert(1305.6302m, false);
                    return;
                }

                var treeNode = treeNode_in;

                do
                {
                    _listTreeNodes.Add(treeNode);
                    ++Count;

                    var nodeDatum = treeNode.NodeDatum;

                    if (nodeDatum == null)
                    {
                        MBoxStatic.Assert(1305.6303m, false);
                        continue;
                    }

                    if ((treeNode.ForeColor == UtilColor.Firebrick) &&
                        (treeNode == nodeDatum.Clones[0]))
                    {
                        _listSameVol.Add(treeNode);
                    }

                    if (bCloneOK)
                    {
                        treeNode.BackColor = UtilColor.DarkYellowBG;

                        if ((nodeDatum.LVitem != null) && (nodeDatum.LVitem.ListView == null))  // ignore LV
                        {
                            nodeDatum.LVitem.BackColor = treeNode.BackColor;
                        }
                    }

                    if (treeNode.FirstNode != null)
                    {
                        Go(treeNode.FirstNode, bCloneOK ||
                            (new[] { UtilColor.SteelBlue, UtilColor.LightBlue }.Contains(treeNode.ForeColor)));
                    }
                }
                while (bNextNode && ((treeNode = treeNode.NextNode) != null));
            }

            readonly List<LocalTreeNode>
                _listTreeNodes = null;
            readonly List<LocalTreeNode>
                _listSameVol = null;
        }
    }
}
