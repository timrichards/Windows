using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DoubleFile;
using System.Collections.Concurrent;

namespace Local
{
    partial class Collate
    {
        internal Collate(GlobalData_Base gd,
            ConcurrentDictionary<FolderKeyTuple, UList<LocalTreeNode>> dictNodes,
            LocalTV tvBrowse,
            LocalLV lvClones,
            LocalLV lvSameVol,
            LocalLV lvUnique,
            List<LocalTreeNode> lsRootNodes,
            UList<LocalTreeNode> lsTreeNodes,
            List<LocalLVitem> lsLVignore,
            bool bLoose)
        {
            _gd = gd;
            _static_this = this;
            _dictNodes = dictNodes;
            _tvBrowse = tvBrowse;
            _lvClones = lvClones;
            _lvSameVol = lvSameVol;
            _lvUnique = lvUnique;
            _lsRootNodes = lsRootNodes;
            _lsTreeNodes = lsTreeNodes;
            _lsLVignore = lsLVignore;
            _bLoose = bLoose;
        }

        internal static void ClearMem()
        {
            Abort();
            _static_this = null;
        }

        internal static void Abort()
        {
            if (_static_this != null)
            {
                _static_this._bThreadAbort = true;
            }
        }

        internal static void InsertSizeMarkers(List<LocalLVitem> listLVitems)
        {
            if (listLVitems.IsEmpty())
            {
                return;
            }

            var bUnique = (null != listLVitems[0].LocalTreeNode);
            var nCount = listLVitems.Count;
            var nInterval = (nCount < 100) ? 10 : (nCount < 1000) ? 25 : 50;

            InsertSizeMarkerStatic.Go(listLVitems, nCount - 1, bUnique, bAdd: true);

            var nInitial = nCount % nInterval;

            if (nInitial == 0)
            {
                nInitial = nInterval;
            }

            if (nCount - nInitial > nInterval / 2)
            {
                for (var i = nCount - nInitial; i > nInterval / 2; i -= nInterval)
                {
                    InsertSizeMarkerStatic.Go(listLVitems, i, bUnique);
                }
            }

            InsertSizeMarkerStatic.Go(listLVitems, 0, bUnique);            // Enter the Zeroth
        }

        internal void Step1(Action<double> reportProgress)
        {
            double nProgressNumerator = 0;
            double nProgressDenominator = 0;
            double nProgressItem = 0;
            const double nTotalProgressItems = 6;

            var treeView = new LocalTV();     // sets Level and NextNode

            if (_lsRootNodes.IsEmpty())
            {
                MBoxStatic.Assert(1305.6314, false);
                return;
            }

            if (_lsRootNodes[0].TreeView == null)
            {
                treeView.Nodes.AddRange(_lsRootNodes.ToArray());
            }

            if (false == _lsLVignore.IsEmpty())
            {
                var dtStart = DateTime.Now;
                var nMaxLevel = _lsLVignore.Max(i => int.Parse(i.SubItems[1].Text) - 1);
                var sbMatch = new StringBuilder();

                foreach (var lvItem in _lsLVignore)
                {
                    sbMatch.AppendLine(lvItem.Text);
                }

                IgnoreNodeQuery(sbMatch.ToString().ToLower(), nMaxLevel, _lsRootNodes[0]);
                UtilProject.WriteLine("IgnoreNode " + (DateTime.Now - dtStart).TotalMilliseconds / 1000.0 + " seconds."); dtStart = DateTime.Now;
            }

            var dictIgnoreMark = new Dictionary<LocalTreeNode, LocalLVitem>();
            var dictNodes = new SortedDictionary<FolderKeyTuple, List<LocalTreeNode>>();

            foreach (var kvp in _dictNodes)                     // clone to remove ignored
            {                                                   // m_ vs local check is via List vs UList
                dictNodes.Add(kvp.Key, kvp.Value.ToList());     // clone pair.Value to remove ignored, using ToList() 
            }

            nProgressDenominator += _dictIgnoreNodes.Count;
            ++nProgressItem;

            foreach (var kvp in _dictIgnoreNodes)
            {
                reportProgress(++nProgressNumerator / nProgressDenominator * nProgressItem/nTotalProgressItems);

                var treeNode = kvp.Key;
                var nodeDatum = treeNode.NodeDatum;

                if (null == nodeDatum)
                {
                    MBoxStatic.Assert(99900, false);    // This check is new 3/7/15 as safety
                }

                List<LocalTreeNode> lsTreeNodes = null;

                if (false == dictNodes.TryGetValue(nodeDatum.Key, out lsTreeNodes))
                {
                    continue;
                }

                if (_bLoose)
                {
                    foreach (var treeNode_A in lsTreeNodes)
                    {
                        dictIgnoreMark.Add(treeNode_A, kvp.Value);
                    }

                    dictNodes.Remove(nodeDatum.Key);
                }
                else if (lsTreeNodes.Contains(treeNode))
                {
                    dictIgnoreMark.Add(treeNode, kvp.Value);
                    lsTreeNodes.Remove(treeNode);

                    if (lsTreeNodes.IsEmpty())
                    {
                        dictNodes.Remove(nodeDatum.Key);
                    }
                }
            }

            var dictUnique = new SortedDictionary<FolderKeyTuple, LocalTreeNode>();
            nProgressDenominator += dictNodes.Count;
            ++nProgressItem;

            foreach (var kvp in dictNodes)
            {
                reportProgress(++nProgressNumerator / nProgressDenominator * nProgressItem / nTotalProgressItems);

                if (_bThreadAbort || _gd.WindowClosed)
                {
                    return;
                }

                var listNodes = kvp.Value;

                if (listNodes.Count < 1)
                {
                    MBoxStatic.Assert(1305.6315, false);
                    continue;
                }
                
                if (listNodes.Count > 1)
                {
                    // Parent folder may contain only its clone subfolder, in which case unmark the subfolder

                    var listKeep = new UList<LocalTreeNode>();

                    foreach (var treeNode_A in listNodes)
                    {
                        if (_bThreadAbort || _gd.WindowClosed)
                        {
                            return;
                        }

                        if (null == treeNode_A)
                        {
                            MBoxStatic.Assert(99978, false);
                            continue;
                        }

                        var nodeDatum = treeNode_A.NodeDatum;

                        if (null == nodeDatum)      // added 2/13/15 as safety
                        {
                            MBoxStatic.Assert(99977, false);
                            return;
                        }

                        MBoxStatic.Assert(1305.6316, nodeDatum.TotalLength > 100 * 1024);

                        if (listNodes.Contains(treeNode_A.Parent) == false)
                        {
                            listKeep.Add(treeNode_A);
                        }
                    }

                    if (listKeep.Count > 1)
                    {
                        foreach (var treeNode_A in listKeep)
                        {
                            var nodeDatum = treeNode_A.NodeDatum;

                            if (null == nodeDatum)      // added 2/13/15 as safety
                            {
                                MBoxStatic.Assert(99976, false);
                                return;
                            }

                            nodeDatum.Clones = listKeep;
                        }
                    }
                    else
                    {
                        listNodes = listKeep.ToList();  // kick off "else" logic below after deleting child clones
                    }
                }

                if (listNodes.Count == 1)               // "else"
                {
                    var treeNode = listNodes[0];

                    var nodeDatum = treeNode.NodeDatum;

                    if (null == nodeDatum)      // added 2/13/15 as safety
                    {
                        MBoxStatic.Assert(99975, false);
                        return;
                    }

                    if (nodeDatum.ImmediateFiles > 0)
                    {
                        dictUnique.Add(kvp.Key, treeNode);
                    }
                }
            }

            var dictClones = new SortedDictionary<FolderKeyTuple, UList<LocalTreeNode>>();

            nProgressDenominator += _lsRootNodes.Count;
            ++nProgressItem;

            foreach (var treeNode in _lsRootNodes)
            {
                reportProgress(++nProgressNumerator / nProgressDenominator * nProgressItem / nTotalProgressItems);
                
                if (_bThreadAbort || _gd.WindowClosed)
                {
                    return;
                }

                DifferentVolsQuery(dictClones, treeNode);
            }

            _lsRootNodes.Sort((x, y) => string.Compare(x.Text, y.Text));
            nProgressDenominator += dictClones.Count;
            ++nProgressItem;

            foreach (var listNodes in dictClones)
            {
                reportProgress(++nProgressNumerator / nProgressDenominator * nProgressItem / nTotalProgressItems);

                // load up listLVdiffVol

                if (_bThreadAbort || _gd.WindowClosed)
                {
                    return;
                }

                var nClones = listNodes.Value.Count;

                if (nClones == 0)
                {
                    MBoxStatic.Assert(1305.6317, false);
                    continue;
                }

                if (nClones == 1)
                {
                    continue;       // keep the same-vol
                }

                string str_nClones = null;

                if (nClones > 2)        // includes the subject node: only note three clones or more
                {
                    str_nClones = nClones.ToString("###,###");

                    foreach (var node in listNodes.Value)
                    {
                        node.ForeColor = UtilColor.Blue;
                    }
                }

                var lvItem = new LocalLVitem(new[] {string.Empty, str_nClones})
                {
                    TreeNodes = listNodes.Value,
                    ForeColor = listNodes.Value[0].ForeColor
                };

                LocalTreeNode nameNode = null;
                var nLevel = int.MaxValue;

                foreach (var treeNode in listNodes.Value)
                {
                    if (_bThreadAbort || _gd.WindowClosed)
                    {
                        return;
                    }

                    if (treeNode.Level < nLevel)
                    {
                        nLevel = treeNode.Level;
                        nameNode = treeNode;
                    }

                    var nodeDatum = treeNode.NodeDatum;

                    if (null == nodeDatum)      // added 2/13/15 as safety
                    {
                        MBoxStatic.Assert(99974, false);
                        return;
                    }

                    nodeDatum.LVitem = lvItem;
                }

                lvItem.Text = nameNode.Text;
                MBoxStatic.Assert(1305.6318, false == string.IsNullOrWhiteSpace(lvItem.Text));
                _lsLVdiffVol.Add(lvItem);
            }

            nProgressDenominator += dictIgnoreMark.Count;
            ++nProgressItem;

            foreach (var kvp in dictIgnoreMark)
            {
                reportProgress(++nProgressNumerator / nProgressDenominator * nProgressItem / nTotalProgressItems);

                var treeNode = kvp.Key;
                var lvIgnoreItem = kvp.Value;

                treeNode.ForeColor = UtilColor.DarkGray;
                treeNode.BackColor = UtilColor.Empty;

                var nodeDatum = treeNode.NodeDatum;

                if (null == nodeDatum)      // added 2/13/15 as safety
                {
                    MBoxStatic.Assert(99973, false);
                    return;
                }

                nodeDatum.LVitem = lvIgnoreItem;
                MBoxStatic.Assert(1305.6319, nodeDatum.LVitem != null);

                if (null != nodeDatum.Clones)
                    nodeDatum.Clones.Remove(treeNode);
            }

            InsertSizeMarkers(_lsLVdiffVol);
            nProgressDenominator += dictUnique.Count;

            foreach (var kvp in dictUnique)
            {
                reportProgress(++nProgressNumerator / nProgressDenominator);

                if (_bThreadAbort || _gd.WindowClosed)
                {
                    return;
                }

                var treeNode = kvp.Value;

                MBoxStatic.Assert(1305.6321, false == string.IsNullOrWhiteSpace(treeNode.Text));

                var lvItem = new LocalLVitem(treeNode.Text) { LocalTreeNode = treeNode };
                var nodeDatum = treeNode.NodeDatum;

                if (null == nodeDatum)      // added 2/13/15 as safety
                {
                    MBoxStatic.Assert(99972, false);
                    return;
                }

                MBoxStatic.Assert(1305.6322, nodeDatum.ImmediateFiles > 0);
                SnowUniqueParents(treeNode);

                if (treeNode.ForeColor != UtilColor.DarkOrange)
                {
                    MBoxStatic.Assert(1305.6323, treeNode.ForeColor == UtilColor.Empty);
                    treeNode.ForeColor = UtilColor.Red;
                }

                lvItem.ForeColor = treeNode.ForeColor;
                _lsLVunique.Add(lvItem);
                MBoxStatic.Assert(1305.6324, nodeDatum.LVitem == null);
                nodeDatum.LVitem = lvItem;
            }

            InsertSizeMarkers(_lsLVunique);

            var listSameVol = new List<LocalTreeNode>();

            if (false == _lsRootNodes.IsEmpty())
            {
                var nCount = CountNodes.Go(_lsRootNodes);
                var nCount_A = new AddTreeToList(_lsTreeNodes, listSameVol).Go(_lsRootNodes).Count;

                MBoxStatic.Assert(1305.6325, nCount_A == nCount);
                MBoxStatic.Assert(1305.6326, _lsTreeNodes.Count == nCount);
                UtilProject.WriteLine("Step1_OnThread " + nCount);
            }

            listSameVol.Sort((y, x) => x.NodeDatum.TotalLength.CompareTo(y.NodeDatum.TotalLength));
            nProgressDenominator += listSameVol.Count;
            ++nProgressItem;

            foreach (var treeNode in listSameVol)
            {
                reportProgress(++nProgressNumerator / nProgressDenominator * nProgressItem / nTotalProgressItems);

                if (_bThreadAbort || _gd.WindowClosed)
                {
                    return;
                }

                SnowUniqueParents(treeNode);

                var nodeDatum = treeNode.NodeDatum;

                if (null == nodeDatum)      // added 2/13/15 as safety
                {
                    MBoxStatic.Assert(99971, false);
                    return;
                }

                int nClones = 0;

                if (null != nodeDatum.Clones)
                    nClones = nodeDatum.Clones.Count;

                if (nClones == 0)
                {
                    MBoxStatic.Assert(1305.6328, false);
                }

                string str_nClones = null;

                if (nClones > 2)
                {
                    str_nClones = nClones.ToString("###,###");
                }

                MBoxStatic.Assert(1305.6329, false == string.IsNullOrWhiteSpace(treeNode.Text));

                var lvItem = new LocalLVitem(new[] {(string)treeNode.Text, str_nClones})
                {
                    TreeNodes = nodeDatum.Clones,
                    ForeColor = UtilColor.Firebrick,
                    BackColor = treeNode.BackColor
                };

                _lsLVsameVol.Add(lvItem);
                nodeDatum.LVitem = lvItem;
            }

            InsertSizeMarkers(_lsLVsameVol);
            treeView.Nodes.Clear();                             // prevents destroy nodes
        }

        internal void Step2()
        {
            if (_tvBrowse.Enabled == false)
            {
                _tvBrowse.Enabled = true;

                var nCount = CountNodes.Go(_lsRootNodes);

                UtilAnalysis_DirList.Write("A");
                _tvBrowse.Nodes.AddRange(_lsRootNodes.ToArray());
                UtilProject.WriteLine("A");

                var nCount_A = CountNodes.Go(_lsRootNodes);

                MBoxStatic.Assert(1305.6331, nCount_A == nCount);
                MBoxStatic.Assert(1305.6332, _tvBrowse.GetNodeCount(includeSubTrees: true) == nCount);
                UtilProject.WriteLine("Step2_OnForm_A " + nCount);
            }

            if (_bThreadAbort || _gd.WindowClosed)
            {
                return;
            }

            MBoxStatic.Assert(1305.6333, _lvClones.Items.IsEmpty());
            UtilAnalysis_DirList.Write("B");
            _lvClones.Items.AddRange(_lsLVdiffVol.ToArray());
            _lvClones.Invalidate();
            UtilProject.WriteLine("B");

            if (_bThreadAbort || _gd.WindowClosed)
            {
                return;
            }

            MBoxStatic.Assert(1305.6334, _lvUnique.Items.IsEmpty());
            UtilAnalysis_DirList.Write("C");
            _lvUnique.Items.AddRange(_lsLVunique.ToArray());
            _lvUnique.Invalidate();
            UtilProject.WriteLine("C");

            if (_bThreadAbort || _gd.WindowClosed)
            {
                return;
            }

            MBoxStatic.Assert(1305.6335, _lvSameVol.Items.IsEmpty());
            UtilAnalysis_DirList.Write("D");
            _lvSameVol.Items.AddRange(_lsLVsameVol.ToArray());
            _lvSameVol.Invalidate();
            UtilProject.WriteLine("D");

            if (_tvBrowse.SelectedNode != null)      // gd.m_bPutPathInFindEditBox is set in TreeDoneCallback()
            {
                var treeNode = _tvBrowse.SelectedNode;

                _tvBrowse.SelectedNode = null;
                _tvBrowse.SelectedNode = treeNode;   // reselect in repopulated collation listviewers
            }
            else
            {
                _tvBrowse.SelectedNode = _lsRootNodes[0];
            }

            _static_this = null;
        }

        // If an outer directory is cloned then all the inner ones are part of the outer clone and their clone status is redundant.
        // Breadth-first.
        void DifferentVolsQuery(
            IDictionary<FolderKeyTuple, UList<LocalTreeNode>> dictClones,
            LocalTreeNode treeNode,
            LocalTreeNode rootClone = null)
        {
            // neither rootClone nor nMaxLength are used at all (rootClone is used as a bool).
            // provisional.

            var nodeDatum = treeNode.NodeDatum;

            if (null == nodeDatum)
            {
                MBoxStatic.Assert(99913, false);    // This check is new 2/23/15 as safety
                return;
            }

            var listClones = nodeDatum.Clones;
            var nLength = nodeDatum.TotalLength;

            if (nLength <= 100 * 1024)
            {
                treeNode.ForeColor = UtilColor.LightGray;
                nodeDatum.Clones = null;
            }

            if ((null != listClones) &&
                (false == listClones.IsEmpty()) &&
                (null == rootClone))
            {
                rootClone = treeNode;

                UList<LocalTreeNode> lsTreeNodes = null;

                if (dictClones.TryGetValue(nodeDatum.Key, out lsTreeNodes))
                {
                    MBoxStatic.Assert(1305.6305, lsTreeNodes == listClones);
                    MBoxStatic.Assert(1305.6306, lsTreeNodes[0].NodeDatum.SeparateVols == nodeDatum.SeparateVols);
                    MBoxStatic.Assert(1305.6307, lsTreeNodes[0].ForeColor == treeNode.ForeColor);
                }
                else
                {
                    dictClones.Add(nodeDatum.Key, listClones);

                    // Test to see if clones are on separate volumes.

                    var rootNode = treeNode.Root();
                    var rootNodeDatum = rootNode.NodeDatum as RootNodeDatum;

                    if (null == rootNodeDatum)      // added 2/13/15 as safety
                    {
                        MBoxStatic.Assert(99970, false);
                        return;
                    }


                    MBoxStatic.Assert(1305.6308, UtilColor.Empty == treeNode.ForeColor);
                    treeNode.ForeColor = UtilColor.Firebrick;

                    var bDifferentVols = false;

                    foreach (var subnode in listClones)
                    {
                        if (_bThreadAbort || _gd.WindowClosed)
                        {
                            return;
                        }

                        MBoxStatic.Assert(1305.6309, subnode.NodeDatum.Key.Equals(nodeDatum.Key));

                        var rootNode_A = subnode.Root();

                        if (rootNode == rootNode_A)
                        {
                            continue;
                        }

                        var rootNodeDatum_A = (rootNode_A.NodeDatum as RootNodeDatum);

                        if (null == rootNodeDatum_A)      // added 2/13/15 as safety
                        {
                            MBoxStatic.Assert(99969, false);
                            return;
                        }

                        if (false == string.IsNullOrWhiteSpace(rootNodeDatum.VolumeGroup) &&
                            (rootNodeDatum.VolumeGroup == rootNodeDatum_A.VolumeGroup))
                        {
                            continue;
                        }

                        MBoxStatic.Assert(1305.6311, treeNode.ForeColor == UtilColor.Firebrick);
                        treeNode.ForeColor = UtilColor.SteelBlue;

                        bDifferentVols = true;
                        break;
                    }

                    foreach (var subNode in listClones)
                    {
                        var nodeDatum_A = subNode.NodeDatum;

                        if (null == nodeDatum_A)      // added 2/13/15 as safety
                        {
                            MBoxStatic.Assert(99968, false);
                            return;
                        }

                        nodeDatum_A.SeparateVols = bDifferentVols;
                        subNode.ForeColor = treeNode.ForeColor;
                    }
                }
            }

            foreach (var subNode in treeNode.Nodes)
            {
                if (_bThreadAbort || _gd.WindowClosed)
                {
                    return;
                }

                DifferentVolsQuery(dictClones, subNode, rootClone);
            }
        }

        void IgnoreNodeAndSubnodes(LocalLVitem lvItem, LocalTreeNode treeNode_in, bool bContinue = false)
        {
            var treeNode = treeNode_in;

            do
            {
                if (_dictIgnoreNodes.ContainsKeyA(treeNode))
                {
                    continue;
                }

                MBoxStatic.Assert(1305.6312, lvItem != null);
                _dictIgnoreNodes.Add(treeNode, lvItem);

                if (false == treeNode.Nodes.IsEmpty())
                {
                    IgnoreNodeAndSubnodes(lvItem, treeNode.Nodes[0], bContinue: true);
                }
            }
            while (bContinue && ((treeNode = treeNode.NextNode) != null));
        }

        void IgnoreNodeQuery(string sbMatch, int nMaxLevel, LocalTreeNode treeNode_in)
        {
            if (treeNode_in.Level > nMaxLevel)
            {
                return;
            }

            var treeNode = treeNode_in;

            do
            {
                if (_bThreadAbort || _gd.WindowClosed)
                {
                    return;
                }

                if (sbMatch.Contains(treeNode.Text.ToLower()))
                {
                    foreach (var lvItem
                        in _lsLVignore
                        .Where(lvItem => treeNode.Level == (int.Parse(lvItem.SubItems[1].Text) - 1))
                        .Where(lvItem => ((string)lvItem.Text).Equals(treeNode.Text,
                            StringComparison.InvariantCultureIgnoreCase)))
                    {
                        MBoxStatic.Assert(99898, false);    // replace the Tag field with an LVitem
                  //      IgnoreNodeAndSubnodes((LocalLVitem)lvItem.Tag, treeNode);
                        break;
                    }
                }

                if (false == treeNode.Nodes.IsEmpty())
                {
                    IgnoreNodeQuery(sbMatch, nMaxLevel, treeNode.Nodes[0]);
                }
            }
            while ((treeNode = treeNode.NextNode) != null);
        }

        static void SnowUniqueParents(LocalTreeNode treeNode)
        {
            LocalTreeNode parentNode = treeNode.Parent;

            while (parentNode != null)
            {
                parentNode.BackColor = UtilColor.Snow;

                var nodeDatum = parentNode.NodeDatum;

                if (nodeDatum.LVitem != null)
                {
                    nodeDatum.LVitem.BackColor = parentNode.BackColor;
                }

                if (parentNode.ForeColor != UtilColor.DarkOrange)
                {
                    MBoxStatic.Assert(1305.6313,
                        (parentNode.ForeColor == UtilColor.Empty) ==
                        (nodeDatum.LVitem == null));
                }

                parentNode = parentNode.Parent;
            }
        }

        // the following are form vars referenced internally, thus keeping their form_ and m_ prefixes
        readonly ConcurrentDictionary<FolderKeyTuple, UList<LocalTreeNode>>
            _dictNodes = null;
        readonly LocalTV _tvBrowse = null;
        readonly LocalLV _lvClones = null;
        readonly LocalLV _lvSameVol = null;
        readonly LocalLV _lvUnique = null;
        readonly List<LocalTreeNode> _lsRootNodes = null;
        readonly UList<LocalTreeNode> _lsTreeNodes = null;
        readonly List<LocalLVitem> _lsLVignore = null;

        // the following are "local" to this object, and do not have m_ prefixes because they do not belong to the form.
        readonly List<LocalLVitem> _lsLVunique = new List<LocalLVitem>();
        readonly List<LocalLVitem> _lsLVsameVol = new List<LocalLVitem>();
        readonly List<LocalLVitem> _lsLVdiffVol = new List<LocalLVitem>();
        readonly Dictionary<LocalTreeNode, LocalLVitem> _dictIgnoreNodes = new Dictionary<LocalTreeNode, LocalLVitem>();
        readonly bool _bLoose = false;

        bool _bThreadAbort = false;
        static Collate _static_this = null;
        readonly GlobalData_Base _gd = null;
    }
}
