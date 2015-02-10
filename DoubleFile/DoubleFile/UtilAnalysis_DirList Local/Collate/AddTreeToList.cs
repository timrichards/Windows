﻿using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using DoubleFile;
using System.Windows.Media;

namespace Local
{
    partial class Collate
    {
        // can't be struct because it has an auto-implemented property
        class AddTreeToList
        {
            internal int Count { get; private set; }

            internal AddTreeToList(UList<LocalTreeNode> listTreeNodes, List<LocalTreeNode> listSameVol)
            {
                m_listTreeNodes = listTreeNodes;
                m_listSameVol = listSameVol;
            }

            internal AddTreeToList Go(IEnumerable<LocalTreeNode> listNodes)
            {
                foreach (LocalTreeNode treeNode in listNodes)
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
                }

                LocalTreeNode treeNode = treeNode_in;

                do
                {
                    if (m_listTreeNodes.Contains(treeNode))
                    {
                        var kvp_A = (m_listTreeNodes as Dictionary<LocalTreeNode, object>)
                            .Where(kvp => kvp.Key == treeNode)
                            .ToArray()[0];

                        UtilProject.WriteLine(kvp_A.Key.ToString());
                        MBoxStatic.Assert(0, false);
                    }

                    m_listTreeNodes.Add(treeNode);
                    ++Count;

                    NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;

                    if (nodeDatum == null)
                    {
                        MBoxStatic.Assert(1305.6303, false);
                        continue;
                    }

                    if ((treeNode.ForeColor == UtilColor.Firebrick) && (treeNode == nodeDatum.m_listClones[0]))
                    {
                        MBoxStatic.Assert(1305.6304, (nodeDatum.m_listClones.Count > 0) && (nodeDatum.m_bDifferentVols == false));
                        m_listSameVol.Add(treeNode);
                    }

                    if (bCloneOK)
                    {
                        treeNode.BackColor = UtilColor.LightGoldenrodYellow;

                        if ((nodeDatum.m_lvItem != null) && (nodeDatum.m_lvItem.ListView == null))  // ignore LV
                        {
                            nodeDatum.m_lvItem.BackColor = treeNode.BackColor;
                        }
                    }

                    if (treeNode.FirstNode != null)
                    {
                        Go((LocalTreeNode)treeNode.FirstNode, bCloneOK || (new int[] { UtilColor.SteelBlue, UtilColor.DarkBlue }.Contains(treeNode.ForeColor)));
                    }
                }
                while (bNextNode && ((treeNode = (LocalTreeNode)treeNode.NextNode) != null));
            }

            UList<LocalTreeNode> m_listTreeNodes = null;
            List<LocalTreeNode> m_listSameVol = null;
        }
    }
}
