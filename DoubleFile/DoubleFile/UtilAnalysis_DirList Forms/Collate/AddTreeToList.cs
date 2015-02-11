using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace DoubleFile
{
    partial class Collate
    {
        // can't be struct because it has an auto-implemented property
        class AddTreeToList
        {
            internal int Count { get; private set; }

            internal AddTreeToList(UList<TreeNode> listTreeNodes, List<TreeNode> listSameVol)
            {
                m_listTreeNodes = listTreeNodes;
                m_listSameVol = listSameVol;
            }

            internal AddTreeToList Go(IReadOnlyList<TreeNode> listNodes)
            {
                foreach (var treeNode in listNodes)
                {
                    Go(treeNode, bNextNode: false);
                }

                return this;
            }

            void Go(TreeNode treeNode_in, bool bCloneOK = false, bool bNextNode = true)
            {
                if (treeNode_in == null)
                {
                    MBoxStatic.Assert(1305.6302, false);
                }

                var treeNode = treeNode_in;

                do
                {
                    m_listTreeNodes.Add(treeNode);
                    ++Count;

                    NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;

                    if (nodeDatum == null)
                    {
                        MBoxStatic.Assert(1305.6303, false);
                        continue;
                    }

                    if ((treeNode.ForeColor == Color.Firebrick) && (treeNode == nodeDatum.m_listClones[0]))
                    {
                        MBoxStatic.Assert(1305.6304, (false == nodeDatum.m_listClones.IsEmpty()) && (nodeDatum.m_bDifferentVols == false));
                        m_listSameVol.Add(treeNode);
                    }

                    if (bCloneOK)
                    {
                        treeNode.BackColor = Color.LightGoldenrodYellow;

                        if ((nodeDatum.m_lvItem != null) && (nodeDatum.m_lvItem.ListView == null))  // ignore LV
                        {
                            nodeDatum.m_lvItem.BackColor = treeNode.BackColor;
                        }
                    }

                    if (treeNode.FirstNode != null)
                    {
                        Go((TreeNode)treeNode.FirstNode, bCloneOK || (new Color[] { Color.SteelBlue, Color.DarkBlue }.Contains(treeNode.ForeColor)));
                    }
                }
                while (bNextNode && ((treeNode = (TreeNode)treeNode.NextNode) != null));
            }

            UList<TreeNode> m_listTreeNodes = null;
            List<TreeNode> m_listSameVol = null;
        }
    }
}
