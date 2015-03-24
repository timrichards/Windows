using System.Collections.Generic;
using System.Linq;
using DoubleFile;

namespace DoubleFile
{
    partial class Collate
    {
        // can't be struct because it has an auto-implemented property
        class AddTreeToList
        {
            internal int Count { get; private set; }

            internal AddTreeToList(KeyList<LocalTreeNode> listTreeNodes, List<LocalTreeNode> listSameVol)
            {
                m_listTreeNodes = listTreeNodes;
                m_listSameVol = listSameVol;
            }

            internal AddTreeToList Go(IReadOnlyList<LocalTreeNode> listNodes)
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
                    MBoxStatic.Assert(1305.6302, false);
                    return;
                }

                var treeNode = treeNode_in;

                do
                {
                    m_listTreeNodes.Add(treeNode);
                    ++Count;

                    var nodeDatum = treeNode.NodeDatum;

                    if (nodeDatum == null)
                    {
                        MBoxStatic.Assert(1305.6303, false);
                        continue;
                    }

                    if ((treeNode.ForeColor == UtilColor.Firebrick) &&
                        (treeNode == nodeDatum.Clones[0]))
                    {
                        MBoxStatic.Assert(1305.6304, false == nodeDatum.SeparateVols);
                        m_listSameVol.Add(treeNode);
                    }

                    if (bCloneOK)
                    {
                        treeNode.BackColor = UtilColor.LightGoldenrodYellow;

                        if ((nodeDatum.LVitem != null) && (nodeDatum.LVitem.ListView == null))  // ignore LV
                        {
                            nodeDatum.LVitem.BackColor = treeNode.BackColor;
                        }
                    }

                    if (treeNode.FirstNode != null)
                    {
                        Go(treeNode.FirstNode, bCloneOK ||
                            (new[] { UtilColor.SteelBlue, UtilColor.Blue }.Contains(treeNode.ForeColor)));
                    }
                }
                while (bNextNode && ((treeNode = treeNode.NextNode) != null));
            }

            readonly KeyList<LocalTreeNode> m_listTreeNodes = null;
            readonly List<LocalTreeNode> m_listSameVol = null;
        }
    }
}
