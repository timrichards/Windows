using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;

namespace DoubleFile
{
    class UC_TreeMapVM : SliderVM_Base<ListViewItemVM_Base>, IDisposable
    {
        class TreeMapFolderRect
        {
            internal const int
                ScaleFactor = 1 << 2;
            internal double
                Area => _rc.Width * _rc.Height;
            internal GeometryDrawing
                GeometryDrawing => new GeometryDrawing(Fill, new Pen(Brushes.Black, .25), new RectangleGeometry(_rc.Scale(ScaleFactor)));

            internal TreeMapFolderRect(Rect rc, int? fill = null)
            {
                _rc = rc;
                _fill = fill;
            }

            Brush
                Fill
            {
                get
                {
                    switch (_fill)      // nullable type gets converted when not null (i.e. don't have to use .Value)
                    {
                        case null: return Brushes.Transparent;
                        case UtilColorcode.Transparent: return _brushSandyBrown;
                        case UtilColorcode.Solitary: return _brushSolitary;
                        case UtilColorcode.OneCopy: return _brushOneCopy;
                        case UtilColorcode.TreemapFolder: return _brushTreemapFolder;
                        case UtilColorcode.TreemapFile: return _brushTreemapFile;
                        default: return new RadialGradientBrush(_kCenterColor, UtilColorcode.Dark(UtilColorcode.FromArgb(_fill.Value))) { RadiusX = 1, RadiusY = 1 };
                    }
                }
            }
            static Brush _brushSandyBrown = null;
            static Brush _brushSolitary = null;
            static Brush _brushOneCopy = null;
            static Brush _brushTreemapFolder = null;
            static Brush _brushTreemapFile = null;

            static readonly Color _kCenterColor = Colors.PapayaWhip;

            static Brush Init(int color) => new RadialGradientBrush(_kCenterColor, UtilColorcode.Dark(UtilColorcode.FromArgb(color))) { RadiusX = 1, RadiusY = 1 };
            static internal void
                Init()
            {
                if (null != _brushSandyBrown)
                    return;

                Util.UIthread(99979, () =>
                {
                    _brushSandyBrown = new RadialGradientBrush(_kCenterColor, UtilColorcode.Dark(Colors.SandyBrown)) { RadiusX = 1, RadiusY = 1 };
                    _brushSolitary = Init(UtilColorcode.Solitary);
                    _brushOneCopy = Init(UtilColorcode.OneCopy);
                    _brushTreemapFolder = Init(UtilColorcode.TreemapFolder);
                    _brushTreemapFile = Init(UtilColorcode.TreemapFile);
                });
            }

            readonly Rect _rc = default(Rect);
            readonly int? _fill = null;
        }

        public Drawing
            TreeMapDrawing { get; private set; }
        public const double
            BitmapSize = 1 << 11;

        public double SelectionLeft { get; private set; }
        public double SelectionWidth { get; private set; }
        public double SelectionTop { get; private set; }
        public double SelectionHeight { get; private set; }
        LocalTreeNode
            SelChildNode
        {
            get { return _selChildNode; }
            set
            {
                var rect =
                    value?.NodeDatum.TreeMapRect.Scale(TreeMapFolderRect.ScaleFactor)
                    ?? default(Rect);

                SelectionLeft = rect.Left;
                SelectionTop = rect.Top;
                SelectionWidth = rect.Width;
                SelectionHeight = rect.Height;
                RaisePropertyChanged("SelectionLeft");
                RaisePropertyChanged("SelectionTop");
                RaisePropertyChanged("SelectionWidth");
                RaisePropertyChanged("SelectionHeight");
                _selChildNode = value;
            }
        }
        LocalTreeNode _selChildNode = null;

        internal override int NumCols => 0;

        internal override void
            GoTo(LocalTreeNode treeNode) => RenderA(treeNode, 99853);

        static internal IObservable<Tuple<string, int>>
            SelectedFile => _selectedFile;
        static readonly LocalSubject<string> _selectedFile = new LocalSubject<string>();
        static void SelectedFileOnNext(string value, int nInitiator) => _selectedFile.LocalOnNext(value, 99841, nInitiator);

        internal const int
            kSelRectAndTooltip = 99983;

        internal Window
            LocalOwner = null;

        public UC_TreeMapVM()
        {
            _lsDisposable.Add(TreeSelect.FolderDetailUpdated.Observable.LocalSubscribe(99696, initiatorTuple =>
            {
                if (LV_TreeListChildrenVM.kChildSelectedOnNext == initiatorTuple.Item2)
                    return;

                var folderDetail = initiatorTuple.Item1;

                Util.Write("M");
                RenderD(folderDetail.treeNode, initiatorTuple.Item2);
                _bTreeSelect = false;
            }));

            _lsDisposable.Add(LV_TreeListChildrenVM.TreeListChildSelected.LocalSubscribe(99695, LV_TreeListChildrenVM_TreeListChildSelected));
            _lsDisposable.Add(LV_FilesVM.SelectedFileChanged.Observable.LocalSubscribe(99694, LV_FilesVM_SelectedFileChanged));
            TreeMapFolderRect.Init();
        }

        public void Dispose()
        {
            Util.LocalDispose(_lsDisposable);
            Util.ThreadMake(WinTooltip.CloseTooltip);
        }

        internal void MouseUp(Point ptLocation)
        {
            var treeNode = ZoomOrTooltip(new Point(ptLocation.X * BitmapSize, ptLocation.Y * BitmapSize));

            if (null != treeNode)
                LocalTV.SelectedNode = treeNode;
        }

        internal void ClearSelection(bool bDontCloseTooltip = false)
        {
            if (Application.Current?.Dispatcher.HasShutdownStarted ?? true)
                return;

            if (_bClearingSelection)
                return;

            _bClearingSelection = true;

            if (false == bDontCloseTooltip)
                WinTooltip.CloseTooltip();  // CloseTooltip callback recurses here hence _bClearingSelection

            SelChildNode = null;
            _bClearingSelection = false;
            _prevNode = null;
        }

        internal LocalTreeNode ZoomOrTooltip(Point pt)
        {
            if (_bTreeSelect ||
                _bSelRecAndTooltip)
            {
                return null;
            }

            var prevNode = _prevNode;

            ClearSelection(bDontCloseTooltip: true);
            _prevNode = prevNode;

            if (null == TreeNode)
                return null;

            if (_rectCenter.Contains(pt))   // click once to hide goofball. Click again within 5 seconds to return to the deep node.
            {
                if (DateTime.MinValue == _dtHideGoofball)
                {
                    _dtHideGoofball = DateTime.Now;
                    return null;
                }
                else if (DateTime.Now - _dtHideGoofball < TimeSpan.FromSeconds(5))
                {
                    _dtHideGoofball = DateTime.MinValue;
                    return DeepNode;
                }
            }

            _dtHideGoofball = DateTime.MinValue;   // click anywhere else on the treemap and the goofball returns.

            LocalTreeNode nodeRet = null;
            var bFilesHere = false;
            var bVolumeView = false;

            Util.Closure(() =>
            {
                {
                    var nodeDatum = TreeNode.NodeDatum;

                    if (null == nodeDatum)      // added 2/13/15
                    {
                        Util.Assert(99967, false);
                        return;     // from lambda
                    }

                    var rootNodeDatum = nodeDatum.As<RootNodeDatum>();

                    bVolumeView = rootNodeDatum?.VolumeView ?? false;

                    if ((false == bVolumeView) &&
                        (null !=
                        (nodeRet = FindMapNode(nodeDatum.TreeMapFiles, pt))))
                    {
                        bFilesHere = true;
                        return;     // from lambda
                    }
                }

                var prevNode_A = _prevNode ?? TreeNode;

                if (null != (nodeRet = FindMapNode(prevNode_A, pt)))
                    return;         // from lambda

                if (_prevNode?.IsChildOf(TreeNode) ?? false)
                {
                    var nodeUplevel = _prevNode.Parent;

                    while (nodeUplevel?.IsChildOf(TreeNode) ?? false)
                    {
                        if (null != (nodeRet = FindMapNode(nodeUplevel, pt)))
                            return;     // from lambda

                        nodeUplevel = nodeUplevel.Parent;
                    }
                }

                Util.Assert(99882,
                    (null == _prevNode) ||
                    (false == TreeNode.IsChildOf(_prevNode)));

                if (null != (nodeRet = FindMapNode(TreeNode, pt)))
                    return;         // from lambda

                nodeRet = TreeNode;

                if (bVolumeView)
                    return;         // from lambda

                var nodeDatum_A = nodeRet.NodeDatum;

                if (null == nodeDatum_A)      // added 2/13/15
                {
                    Util.Assert(99923, false);
                    return;
                }

                var nodeRet_A = FindMapNode(nodeDatum_A.TreeMapFiles, pt);

                if (null != nodeRet_A)
                {
                    nodeRet = nodeRet_A;
                    bFilesHere = true;
                }
            });

            if (ReferenceEquals(nodeRet, _prevNode))
            {
                nodeRet = TreeNode;
                bFilesHere = false;
            }

            SelRectAndTooltip(nodeRet, kSelRectAndTooltip /* UI Initiator */, bFilesHere);
            return null;
        }

        void LV_TreeListChildrenVM_TreeListChildSelected(Tuple<LocalTreeNode, int> initiatorTuple)
        {
            var treeNodeChild = initiatorTuple.Item1;

            Util.Write("N");
            if (_bTreeSelect ||
                _bSelRecAndTooltip)
            {
                return;
            }

            if (false == ReferenceEquals(TreeNode, treeNodeChild.Parent))
                return;

            SelRectAndTooltip(treeNodeChild, initiatorTuple.Item2, bFile: false);
        }

        void LV_FilesVM_SelectedFileChanged(Tuple<LV_FilesVM.SelectedFileChanged, int> initiatorTuple)
        {
            var tuple = initiatorTuple.Item1;

            Util.Write("O");
            if (_bTreeSelect ||
                _bSelRecAndTooltip ||
                (null == tuple))
            {
                return;
            }

            if (null == tuple.treeNode.NodeDatum.TreeMapFiles)     // TODO: Why would this be null?
                return;

            bool bCloseTooltip = true;

            tuple.fileLine.FirstOrDefault(strFile =>
                tuple.treeNode.NodeDatum.TreeMapFiles.Nodes
                .Where(treeNodeA => treeNodeA.Text == strFile)
                .Where(treeNodeA => treeNodeA is LocalTreeMapFileNode)
                .FirstOnlyAssert(fileNode =>
            {
                SelRectAndTooltip(fileNode, initiatorTuple.Item2, bFile: true);
                bCloseTooltip = false;
            }));

            if (bCloseTooltip)                                  // TODO: Why would this occur?
                WinTooltip.CloseTooltip();
        }

        void SelRectAndTooltip(LocalTreeNode treeNodeChild, int nInitiator, bool bFile)
        {
            if (_bTreeSelect ||
                _bSelRecAndTooltip)
            {
                return;
            }

            if (SelChildNode == treeNodeChild)
                return;

            _bSelRecAndTooltip = true;

            var nodeDatum = treeNodeChild.NodeDatum;
            var strFolder = treeNodeChild.Text;
            var nodeTreeSelect = treeNodeChild;

            if (bFile)
            {
                strFolder += " (file)";
                nodeTreeSelect = treeNodeChild.Parent.Parent;   // Parent is TreeMapFileListNode
                SelectedFileOnNext(treeNodeChild.Text, nInitiator);
            }

            WinTooltip.ShowTooltip(
                new WinTooltip.ArgsStruct(
                    strFolder,
                    Util.FormatSize(nodeDatum.TotalLength, bBytes: true),
                    LocalOwner,
                    Tooltip_Click,
                    () => ClearSelection(bDontCloseTooltip: true)),
                treeNodeChild);

            SelChildNode = treeNodeChild;
            _prevNode = treeNodeChild;

            if (LV_TreeListChildrenVM.kChildSelectedOnNext != nInitiator)
                _bTreeSelect = TreeSelect.DoThreadFactory(nodeTreeSelect, nInitiator);

            _bSelRecAndTooltip = false;
        }

        static LocalTreeNode FindMapNode(LocalTreeNode treeNode_in, Point pt)
        {
            pt.X /= TreeMapFolderRect.ScaleFactor;
            pt.Y /= TreeMapFolderRect.ScaleFactor;
            return FindMapNode_(treeNode_in, pt);
        }

        static LocalTreeNode FindMapNode_(LocalTreeNode treeNode_in, Point pt, bool bNextNode = false)
        {
            var treeNode = treeNode_in;

            if (null == treeNode)
                return null;

            do
            {
                var nodeDatum = treeNode.NodeDatum;

                if (null == nodeDatum)      // added 2/13/15
                {
                    Util.Assert(99966, false);
                    return null;
                }

                if (false == nodeDatum.TreeMapRect.Contains(pt))
                    continue;

                if (bNextNode ||
                    (treeNode != treeNode_in))
                {
                    return treeNode;
                }

                if (0 == (treeNode.Nodes?.Count ?? 0))
                    continue;

                var foundNode = FindMapNode_(treeNode.Nodes[0], pt, bNextNode: true);

                if (null != foundNode)
                    return foundNode;
            }
            while (bNextNode &&
                (null !=
                (treeNode = treeNode.NextNode)));

            return null;
        }

        static LocalTreeNode GetFileList(LocalTreeNode parent)
        {
            var nodeDatum = parent.NodeDatum;
            var rootNodeDatum = parent.Root.NodeDatum.As<RootNodeDatum>();

            if ((null == nodeDatum) ||
                (0 == nodeDatum.LineNo) ||
                (null == rootNodeDatum))
            {
                return null;
            }

            var nPrevDir = (int)nodeDatum.PrevLineNo;

            if (0 == nPrevDir)
                return null;

            var nLineNo = (int)nodeDatum.LineNo;

            if (1 >= (nLineNo - nPrevDir))  // dir has no files
                return null;

            ulong nLengthDebug = 0;
            var strListingFile = rootNodeDatum.LVitemProjectVM.ListingFile;
            var lsFiles = new List<Tuple<string, ulong>>();

            foreach (var asFileLine
                in strListingFile.ReadLinesWait(99650)
                .Skip(nPrevDir)
                .Take((nLineNo - nPrevDir - 1))
                .Select(s =>
                    s
                    .Split('\t')
                    .Skip(3)                    // makes this an LV line: knColLengthLV
                    .ToArray()))
            {
                ulong nLength = 0;

                if ((asFileLine.Length > FileParse.knColLengthLV) &&
                    (false == string.IsNullOrWhiteSpace(asFileLine[FileParse.knColLengthLV])))
                {
                    nLengthDebug += nLength = ("" + asFileLine[FileParse.knColLengthLV]).ToUlong();
                    asFileLine[FileParse.knColLengthLV] = Util.FormatSize(asFileLine[FileParse.knColLengthLV]);
                }

                lsFiles.Add(Tuple.Create(asFileLine[0], nLength));
            }

            Util.Assert(1301.2313m, nLengthDebug == nodeDatum.Length);

            ulong nTotalLength = 0;
            var lsNodes = new List<LocalTreeMapFileNode>();

            foreach (var tuple in lsFiles)
            {
                if (0 == tuple.Item2)
                    continue;

                nTotalLength += tuple.Item2;

                lsNodes.Add(new LocalTreeMapFileNode(tuple.Item1)
                {
                    NodeDatum = new NodeDatum { TotalLength = tuple.Item2 },
                    ForeColor = UtilColorcode.TreemapFile
                });
            }

            if (0 == nTotalLength)
                return null;

            Util.Assert(1302.3301m, nTotalLength == parent.NodeDatum.Length);

            return new LocalTreeMapFileListNode(parent, lsNodes)
            {
                NodeDatum = new NodeDatum { TotalLength = nTotalLength, TreeMapRect = parent.NodeDatum.TreeMapRect }
            };
        }

        internal void Tooltip_Click()
        {
            var treeNode = WinTooltip.LocalTreeNode;

            if (treeNode is LocalTreeMapFileNode)
                return;     // file fake node

            WinTooltip.CloseTooltip();
            RenderA(treeNode, nInitiator: 0);
        }

        void RenderA(LocalTreeNode treeNode, int nInitiator)
        {
            RenderD(treeNode, nInitiator);
            _bTreeSelect = TreeSelect.DoThreadFactory(treeNode, nInitiator);
        }

        void RenderD(LocalTreeNode treeNode, int nInitiator)
        {
            if (_bSelRecAndTooltip ||
                _bTreeSelect)
            {
                return;
            }

            Render(treeNode);
        }

        void Render(LocalTreeNode treeNode)
        {
            if (false == (DeepNode?.IsChildOf(treeNode) ?? false))
                DeepNode = treeNode;

            ClearSelection();
            TreeNode = treeNode;

            var timer = Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1)).Timestamp()
                .LocalSubscribe(0, x => Util.WriteLine(DateTime.Now.Ticks + " " + treeNode + " DrawTreemap"));

            var ieRenderActions = DrawTreemap();

            timer.Dispose();

            Util.UIthread(99823, () =>
            {
                if (null != LocalOwner)
                    LocalOwner.Title = "Double File";

                TreeMapDrawing = null;

                if (null == ieRenderActions)
                    return;     // from lambda

                var drawingGroup = new DrawingGroup();

                drawingGroup.Children.Add(new GeometryDrawing(new SolidColorBrush(Color.FromRgb(193, 176, 139)), new Pen(), new RectangleGeometry(new Rect(0, 0, BitmapSize, BitmapSize))));

                foreach (var render in ieRenderActions.OrderByDescending(r => r.Area).Take(1 << 11))
                    drawingGroup.Children.Add(render.GeometryDrawing);

                TreeMapDrawing = drawingGroup;

                if (null != LocalOwner)
                    LocalOwner.Title = treeNode.Text;
            });

            RaisePropertyChanged("TreeMapDrawing");
            SelChildNode = null;
            _prevNode = null;
            _dtHideGoofball = DateTime.MinValue;
        }

        // treemap.cpp	- Implementation of CColorSpace, CTreemap and CTreemapPreview
        //
        // WinDirStat - Directory Statistics
        // Copyright (C) 2003-2004 Bernhard Seifert
        //
        // This program is free software; you can redistribute it and/or modify
        // it under the terms of the GNU General Public License as published by
        // the Free Software Foundation; either version 2 of the License, or
        // (at your option) any later version.
        //
        // This program is distributed in the hope that it will be useful,
        // but WITHOUT ANY WARRANTY; without even the implied warranty of
        // MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
        // GNU General Public License for more details.
        //
        // You should have received a copy of the GNU General Public License
        // along with this program; if not, write to the Free Software
        // Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
        //
        // Author: bseifert@users.sourceforge.net, bseifert@daccord.net
        //
        // Last modified: $Date: 2004/11/05 16:53:08 $
        
        IEnumerable<TreeMapFolderRect> 
            DrawTreemap()
        {
            _deepNodeDrawn = null;

            var nodeDatum = TreeNode.NodeDatum;

            if (null == nodeDatum)      // added 2/13/15
            {
                Util.Assert(99963, false);
                return null;
            }

            var rc = new Rect(0, 0, BitmapSize, BitmapSize).Scale(1d / TreeMapFolderRect.ScaleFactor);

            return
                (0 < nodeDatum.TotalLength)
                ? new Recurse().Render(TreeNode, rc, DeepNode, out _deepNodeDrawn)
                : new[] { new TreeMapFolderRect(rc, Colors.Wheat.ToArgb()) };
        }

        class
            Recurse
        {
            internal IEnumerable<TreeMapFolderRect>
                Render(LocalTreeNode treeNode, Rect rc, LocalTreeNode deepNode, out LocalTreeNode deepNodeDrawn_out)
            {
                _lsRenderActions = new ConcurrentBag<TreeMapFolderRect> { };
                _lsFrames = new ConcurrentBag<TreeMapFolderRect> { };
                _deepNode = deepNode;
                RecurseDrawGraph(treeNode, rc, true);

                var timerA = Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1)).Timestamp()
                    .LocalSubscribe(0, x => Util.WriteLine(_nWorkerCount + " " + treeNode + " DrawTreemap"));

                var timer = Observable.Timer(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100)).Timestamp()
                    .LocalSubscribe(99825, x =>
                {
                    if (0 < _nWorkerCount)
                        return;     // from lambda

                    _blockingFrame.Continue = false;
                    Util.WriteLine(DateTime.Now.Ticks + " " + treeNode + " DrawTreemap - cleared stuck worker count during 100ms check");
                });

                if (0 < _nWorkerCount)
                    _blockingFrame.PushFrameTrue();

                timer.Dispose();
                timerA.Dispose();

                deepNodeDrawn_out = _deepNodeDrawn;
                return _lsRenderActions.Concat(_lsFrames);
            }
            
            void
                RecurseDrawGraph(LocalTreeNode treeNode, Rect rc, bool bStart = false)
            {
                var nodeDatum = treeNode.NodeDatum;

                if (null == nodeDatum)      // added 2/13/15
                {
                    Util.Assert(99962, false);
                    return;
                }

                nodeDatum.TreeMapRect = rc;

                if ((1 > rc.Width) ||
                    (1 > rc.Height))
                {
                    return;
                }

                if ((32 > rc.Width) ||
                    (32 > rc.Height))
                {
                    // Speedup. Draw an "empty" folder in place of too much detail
                    _lsRenderActions.Add(new TreeMapFolderRect(rc, treeNode.ForeColor));
                    return;
                }

                if ((treeNode == _deepNode) ||
                    (_deepNode?.IsChildOf(treeNode) ?? false))
                {
                    _deepNodeDrawn = treeNode;
                }

                if (bStart &&
                    (null == nodeDatum.TreeMapFiles) &&
                    (false == treeNode is LocalTreeMapFileNode))
                {
                    nodeDatum.TreeMapFiles = GetFileList(treeNode);
                }

                if ((0 < (treeNode.Nodes?.Count ?? 0)) ||
                    (bStart && (null != nodeDatum.TreeMapFiles)))
                {
                    IEnumerable<LocalTreeNode> ieChildren = null;
                    LocalTreeNode parent = null;
                    var bVolumeNode = false;

                    Util.Closure(() =>
                    {
                        var rootNodeDatum = treeNode.NodeDatum.As<RootNodeDatum>();

                        if ((false == bStart) ||
                            (null == rootNodeDatum))
                        {
                            return;     // from lambda
                        }

                        if (false == rootNodeDatum.VolumeView)
                            return;     // from lambda

                        var nodeDatumFree = new NodeDatum { TotalLength = rootNodeDatum.VolumeFree };

                        var nodeFree = new LocalTreeMapFileNode(treeNode.Text + " (free space)")
                        {
                            NodeDatum = nodeDatumFree,
                            ForeColor = UtilColorcode.TreemapFreespace
                        };

                        var nodeDatumUnread = new NodeDatum();
                        var nVolumeLength = rootNodeDatum.VolumeLength;

                        var nUnreadLength =
                            (long)nVolumeLength -
                            (long)rootNodeDatum.VolumeFree -
                            (long)rootNodeDatum.TotalLength;

                        if (0 < nUnreadLength)
                        {
                            nodeDatumUnread.TotalLength = (ulong)nUnreadLength;
                        }
                        else
                        {
                            // Faked length to make up for compression and hard links
                            nVolumeLength =
                                rootNodeDatum.VolumeFree + rootNodeDatum.TotalLength;

                            nodeDatumUnread.TotalLength = 0;
                        }

                        var nodeUnread = new LocalTreeMapFileNode(treeNode.Text + " (unread data)")
                        {
                            NodeDatum = nodeDatumUnread,
                            ForeColor = UtilColorcode.TreemapUnreadspace
                        };

                        // parent added as child, with two other nodes:
                        // free space (color: spring green); and...
                        var lsChildren = new List<LocalTreeNode> { treeNode, nodeFree };

                        if (0 < nUnreadLength)
                        {
                            // ...unread guess, affected by compression and hard links (violet)
                            lsChildren.Add(nodeUnread);
                        }

                        ieChildren = lsChildren;
                        parent = new LocalTreeMapFileNode(treeNode.Text + " (volume)");

                        var nodeDatumVolume = new NodeDatum
                        {
                            TotalLength = nVolumeLength,
                            TreeMapRect = rootNodeDatum.TreeMapRect
                        };

                        parent.NodeDatum = nodeDatumVolume;
                        bVolumeNode = true;
                    });

                    if ((false == bVolumeNode) &&
                        (null != treeNode.Nodes))
                    {
                        parent = treeNode;

                        ieChildren =
                            treeNode.Nodes
                            .Where(t => 0 < t.NodeDatum.TotalLength);
                    }

                    if ((null == ieChildren) &&
                        (null != nodeDatum.TreeMapFiles))
                    {
                        parent = treeNode;
                        ieChildren = new List<LocalTreeNode> { };
                    }

                    // returns true if there are children
                    if ((null != ieChildren) &&
                        KDirStat_DrawChildren(parent, ieChildren, bStart))
                    {
                        // example scenario: empty folder when there are files here and bStart is not true
                        return;
                    }
                }

                // There are no children. Draw a file or an empty folder.
                _lsRenderActions.Add(new TreeMapFolderRect(rc, treeNode.ForeColor));
            }

            //My first approach was to make this member pure virtual and have three
            //classes derived from CTreemap. The disadvantage is then, that we cannot
            //simply have a member variable of type CTreemap but have to deal with
            //pointers, factory methods and explicit destruction. It's not worth.

            //I learned this squarification style from the KDirStat executable.
            //It's the most complex one here but also the clearest, imho.

            bool
                KDirStat_DrawChildren(LocalTreeNode parent, IEnumerable<LocalTreeNode> ieChildren, bool bStart)
            {
                var nodeDatum = parent.NodeDatum;

                if (bStart &&
                    (null != nodeDatum.TreeMapFiles))
                {
                    ieChildren = ieChildren.Concat(new[] { nodeDatum.TreeMapFiles });
                }
                else if (0 < nodeDatum.Length)
                {
                    ieChildren = ieChildren.Concat(new[] { new LocalTreeMapFileNode(parent.Text)
                    {
                        NodeDatum = new NodeDatum { TotalLength = nodeDatum.Length },
                        ForeColor = UtilColorcode.TreemapFolder
                    }});
                }

                // could do array here but it's slightly less efficient because element sizes have to be shrunk
                var lsChildren =
                    ieChildren
                    .OrderByDescending(treeNode => treeNode.NodeDatum.TotalLength)
                    .ToList();

                var nCount = lsChildren.Count;

                if (0 == nCount)
                {
                    // any files are zero in length
                    return false;
                }

                Interlocked.Add(ref _nWorkerCount, nCount);

                var anChildWidth = // Widths of the children (fraction of row width).
                    new double[nCount];

                var rc = nodeDatum.TreeMapRect;
                var horizontalRows = (rc.Width >= rc.Height);
                double width_A = 1;

                if (horizontalRows)
                {
                    if (0 < rc.Height)
                        width_A = rc.Width / rc.Height;
                }
                else
                {
                    if (0 < rc.Width)
                        width_A = rc.Height / rc.Width;
                }

                var rows = new List<RowStruct> { };

                {
                    var childrenUsed = 0;

                    for (var nextChild = 0; nextChild < nCount; nextChild += childrenUsed)
                    {
                        rows.Add(new RowStruct
                        {
                            RowHeight = KDirStat_CalculateNextRow(parent, nextChild, width_A, out childrenUsed, anChildWidth, lsChildren),
                            ChildrenPerRow = childrenUsed
                        });
                    }
                }

                var width = horizontalRows ? rc.Width : rc.Height;
                var height = horizontalRows ? rc.Height : rc.Width;
                var c = 0;
                var top = horizontalRows ? rc.Top : rc.Left;
                var lastRow = rows[rows.Count - 1];

                rows.ForEach(row =>
                {
                    var bottom = top + row.RowHeight * height;

                    if (ReferenceEquals(row, lastRow))
                        bottom = horizontalRows ? rc.Bottom() : rc.Right();

                    double left = horizontalRows ? rc.Left : rc.Top;

                    for (var i = 0; i < row.ChildrenPerRow; i++, c++)
                    {
                        var child = lsChildren[c];

                        Util.Assert(1302.3305m, 0 <= anChildWidth[c]);

                        var right = left + anChildWidth[c] * width;

                        var lastChild = 
                            ((row.ChildrenPerRow - 1 == i) ||
                            anChildWidth[c + 1].Equals(0));

                        if (lastChild)
                            right = horizontalRows ? rc.Right() : rc.Bottom();

                        var rcChild =
                            horizontalRows
                            ? new Rect(left, top, right - left, bottom - top)
                            : new Rect(top, left, bottom - top, right - left);

                        ThreadPool.QueueUserWorkItem(
                            state =>
                            {
                                var param = (Tuple<LocalTreeNode, Rect>)state;

                                RecurseDrawGraph(param.Item1, param.Item2);
                                Interlocked.Decrement(ref _nWorkerCount);

                                if (0 == _nWorkerCount)
                                    _blockingFrame.Continue = false;
                            },

                            Tuple.Create(child, rcChild)
                        );

                        if (bStart)
                            _lsFrames.Add(new TreeMapFolderRect(rcChild));

                        if (lastChild)
                        {
                            i++;
                            c++;

                            if (i < row.ChildrenPerRow)
                                lsChildren[c].NodeDatum.TreeMapRect = new Rect(-1, -1, -1, -1);

                            c += row.ChildrenPerRow - i;
                            break;
                        }

                        left = right;
                    }
                    // This asserts due to rounding error: Utilities.Assert(1302.3306, left == (horizontalRows ? rc.Right : rc.Bottom));
                    top = bottom;
                });
                // This asserts due to rounding error: Utilities.Assert(1302.3307, top == (horizontalRows ? rc.Bottom : rc.Right));
                return true;
            }

            static double KDirStat_CalculateNextRow(LocalTreeNode parent, int nextChild, double width,
                out int childrenUsed, double[] anChildWidth,
                IReadOnlyList<LocalTreeNode> listChildren)
            {
                childrenUsed = 0;

                const double kdMinProportion = 0.4;
                var nCount = listChildren.Count;

                Util.Assert(1302.3308m, 1 > kdMinProportion);
                Util.Assert(1302.3309m, nextChild < nCount);
                Util.Assert(1302.33101m, 1 <= width);

                var nodeDatum = parent.NodeDatum;

                if (null == nodeDatum)      // added 2/13/15
                {
                    Util.Assert(99961, false);
                    return 0;
                }

                double mySize = nodeDatum.TotalLength;
                ulong sizeUsed = 0;
                double rowHeight = 0;
                var i = 0;

                for (i = nextChild; i < nCount; ++i)
                {
                    var childSize = listChildren[i].NodeDatum.TotalLength;

                    sizeUsed += childSize;

                    var virtualRowHeight = sizeUsed / mySize;

                    Util.Assert(1302.3311m, virtualRowHeight > 0);
                    Util.Assert(1302.3312m, virtualRowHeight <= 1);

                    // Rect(mySize)    = width * 1d
                    // Rect(childSize) = childWidth * virtualRowHeight
                    // Rect(childSize) = childSize / mySize * width

                    double childWidth =
                        childSize / mySize *
                        width / virtualRowHeight;

                    if (kdMinProportion > childWidth / virtualRowHeight)
                    {
                        Util.Assert(1302.3313m, i > nextChild); // because width >= 1 and _minProportion < 1.
                        // For the first child we have:
                        // childWidth / rowHeight
                        // = childSize / mySize * width / rowHeight / rowHeight
                        // = childSize * width / sizeUsed / sizeUsed * mySize
                        // > childSize * mySize / sizeUsed / sizeUsed
                        // > childSize * childSize / childSize / childSize 
                        // = 1 > _minProportion.
                        break;
                    }

                    rowHeight = virtualRowHeight;
                }

                Util.Assert(1302.3314m, i > nextChild);

                // Now i-1 is the last child used
                // and rowHeight is the height of the row.

                childrenUsed = i - nextChild;

                // Now as we know the rowHeight, we compute the widths of our children.
                for (i = 0; i < childrenUsed; ++i)
                {
                    // Rect(1d * 1d) = mySize
                    var rowSize = mySize * rowHeight;
                    var nodeDatum_A = listChildren[nextChild + i].NodeDatum;

                    if (null == nodeDatum_A)      // added 2/13/15
                    {
                        Util.Assert(99960, false);
                        return 0;
                    }

                    var childSize = (double)nodeDatum_A.TotalLength;
                    var cw = childSize / rowSize;

                    Util.Assert(1302.3315m, 0 <= cw);
                    anChildWidth[nextChild + i] = cw;
                }

                return rowHeight;
            }

            ConcurrentBag<TreeMapFolderRect>
                _lsRenderActions = null;
            ConcurrentBag<TreeMapFolderRect>
                _lsFrames = null;
            LocalTreeNode
                _deepNode = null;
            LocalTreeNode
                _deepNodeDrawn = null;

            struct
                RowStruct { internal double RowHeight; internal int ChildrenPerRow; }
            int
                _nWorkerCount = 0;
            LocalDispatcherFrame
                _blockingFrame = new LocalDispatcherFrame(99850);
        }

        bool
            _bClearingSelection = false;
        bool
            _bTreeSelect = false;
        bool
            _bSelRecAndTooltip = false;

        // Recurse class
        LocalTreeNode
            _deepNodeDrawn = null;

        // goofball
        Rect
            _rectCenter = Rect.Empty;
        DateTime
            _dtHideGoofball = DateTime.MinValue;

        LocalTreeNode
            _prevNode = null;

        readonly IList<IDisposable>
            _lsDisposable = new List<IDisposable>();
    }
}
