using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;
using System.Collections.ObjectModel;

namespace DoubleFile
{
    class UC_TreemapVM : SliderVM_Base<ListViewItemVM_Base>, IDisposable
    {
        static string TooltipText(LocalTreeNode treeNode) => treeNode?.PathShort + " (" + treeNode?.NodeDatum.LengthTotal.FormatSize() + ")";

        public ObservableCollection<TreemapFrame>
            Frames { get; } = new ObservableCollection<TreemapFrame>();
        public class TreemapFrame
        {
            internal const int
                ScaleFactor = 1 << 2;
            internal double
                Area => _rc.Area();

            public double X => _rc.X;
            public double Y => _rc.Y;
            public double Width => _rc.Width;
            public double Height => _rc.Height;

            public string Tooltip { get; } = null;
            public Visibility VisibilityOnFile { get; } = Visibility.Collapsed;
            public Visibility VisibilityNotFile { get; } = Visibility.Visible;

            internal TreemapFrame(LocalTreeNode treeNode)
            {
                _rc = treeNode.TreemapRect.Scale(ScaleFactor);
                Tooltip = TooltipText(treeNode);

                if (treeNode.As<LocalTreemapNode>()?.IsFile ?? false)
                {
                    VisibilityOnFile = Visibility.Visible;
                    VisibilityNotFile = Visibility.Collapsed;
                }
            }

            protected readonly Rect _rc = default(Rect);
        }

        class TreemapFill : TreemapFrame
        {
            internal GeometryDrawing
                GeometryDrawing => new GeometryDrawing(Fill, new Pen(), new RectangleGeometry(_rc));

            internal TreemapFill(LocalTreeNode treeNode)
                : base(treeNode)
            {
                _fill = treeNode.ColorcodeFG;

                if (UtilColorcode.Transparent == _fill)
                    _fill = Colors.Wheat.ToArgb();
            }

            Brush
                Fill =>
                _brushes.TryGetValue(_fill)
                ?? new RadialGradientBrush(_kCenterColor, UtilColorcode.Dark(UtilColorcode.FromArgb(_fill))) { RadiusX = 1, RadiusY = 1 };
            static IDictionary<int, Brush> _brushes = null;
            static readonly Color _kCenterColor = Colors.PapayaWhip;
            static Brush Init(int color) => new RadialGradientBrush(_kCenterColor, UtilColorcode.Dark(UtilColorcode.FromArgb(color))) { RadiusX = 1, RadiusY = 1 };
            static internal void
                Init()
            {
                Util.UIthread(99979, () =>
                {
                    _brushes = new Dictionary<int, Brush>
                    {
                        { UtilColorcode.Transparent, new RadialGradientBrush(_kCenterColor, UtilColorcode.Dark(Colors.SandyBrown)) { RadiusX = 1, RadiusY = 1 } },
                        { UtilColorcode.SolitNoFilesDuped, Init(UtilColorcode.SolitNoFilesDuped) },
                        { UtilColorcode.SolitSomeFilesDuped, Init(UtilColorcode.SolitSomeFilesDuped) },
                        { UtilColorcode.OneCloneSepVolume, Init(UtilColorcode.OneCloneSepVolume) },
                        { UtilColorcode.OneOrTwoCloneSepVol, Init(UtilColorcode.OneOrTwoCloneSepVol) },
                        { UtilColorcode.TreemapFolder, Init(UtilColorcode.TreemapFolder) },
                        { UtilColorcode.TreemapDupeSepVol, Init(UtilColorcode.TreemapDupeSepVol) },
                        { UtilColorcode.TreemapUniqueFile, Init(UtilColorcode.TreemapUniqueFile) }
                    };
                });
            }

            readonly int _fill = -1;
        }

        public Visibility GoofballVisibility => (Rect.Empty != _rectDeepnode) ? Visibility.Visible : Visibility.Collapsed;
        public double GoofballX => _rectDeepnode.Scale(TreemapFrame.ScaleFactor).CenterX();
        public double GoofballY => _rectDeepnode.Scale(TreemapFrame.ScaleFactor).CenterY();

        public Drawing
            TreemapDrawing { get; private set; }
        public const double
            BitmapSize = 1 << 11;

        public string SelectionTooltip { get; private set; }
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
                    value?.TreemapRect.Scale(TreemapFrame.ScaleFactor)
                    ?? default(Rect);

                SelectionTooltip = TooltipText(value);
                SelectionLeft = rect.Left;
                SelectionTop = rect.Top;
                SelectionWidth = rect.Width;
                SelectionHeight = rect.Height;
                RaisePropertyChanged("SelectionTooltip");
                RaisePropertyChanged("SelectionLeft");
                RaisePropertyChanged("SelectionTop");
                RaisePropertyChanged("SelectionWidth");
                RaisePropertyChanged("SelectionHeight");
                _selChildNode = value;
            }
        }
        LocalTreeNode _selChildNode = null;

        internal override int NumCols => 0;

        internal override object
            GoTo(LocalTreeNode treeNode) => TreeSelect.DoThreadFactory(treeNode, 99853);

        static internal IObservable<Tuple<string, decimal>>
            SelectedFile => _selectedFile;
        static readonly LocalSubject<string> _selectedFile = new LocalSubject<string>();
        static void SelectedFileOnNext(string value, decimal nInitiator) => _selectedFile.LocalOnNext(value, 99841, nInitiator);

        internal const int
            kSelRectAndTooltip = 99983;

        internal Window
            LocalOwner = null;

        internal UC_TreemapVM
            Init()
        {
            _lsDisposable.Add(TreeSelect.FolderDetailUpdated.Observable.LocalSubscribe(99696, initiatorTuple =>
            {
                if (LV_TreeListChildrenVM.kChildSelectedOnNext == initiatorTuple.Item2)
                    return;

                var folderDetail = initiatorTuple.Item1;

                Util.Write("M");
                Render(folderDetail.treeNode);
                _bTreeSelect = false;
            }));

            _lsDisposable.Add(LV_TreeListChildrenVM.TreeListChildSelected.LocalSubscribe(99695, LV_TreeListChildrenVM_TreeListChildSelected));
            _lsDisposable.Add(LV_FilesVM.SelectedFileChanged.Observable.LocalSubscribe(99694, LV_FilesVM_SelectedFileChanged));
            TreemapFill.Init();

            var folderDetailA = LocalTV.TreeSelect_FolderDetail;

            if (null != folderDetailA)
                Render(folderDetailA.treeNode);

            return this;
        }

        public void Dispose()
        {
            Util.LocalDispose(_lsDisposable);
            Util.ThreadMake(WinTooltip.CloseTooltip);

            if (null != LocalOwner)
                Util.UIthread(99938, () => LocalOwner.Title = Util.Localized("Title"));
        }

        internal void Tooltip_Click()
        {
            var treeNode = WinTooltip.LocalTreeNode;

            if (treeNode is LocalTreemapNode)
                return;     // file fake node

            if (treeNode is LocalTreemapFileListNode)
                treeNode = treeNode.Parent;

            WinTooltip.CloseTooltip();
            _bTooltipVolumeView = false;
            GoTo(treeNode);
        }

        internal void MouseUp(Point ptLocation) =>
            ShowTooltip(new Point(ptLocation.X * BitmapSize, ptLocation.Y * BitmapSize));

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

        internal LocalTreeNode ShowTooltip(Point pt)
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

            LocalTreeNode nodeRet = null;
            var bVolumeView = false;

            Util.Closure(() =>
            {
                {
                    bVolumeView = TreeNode.NodeDatum.As<RootNodeDatum>()?.VolumeView ?? false;

                    Util.Write("A");
                    if ((false == bVolumeView) &&
                        (null !=
                        (nodeRet = FindMapNode(TreeNode.TreemapFiles, pt))))
                    {
                        Util.Write("B");                // file list node
                        return;     // from lambda
                    }
                }

                var prevNode_A = _prevNode ?? TreeNode;

                Util.Write("C");
                if (null != (nodeRet = FindMapNode(prevNode_A, pt)))
                    return;         // from lambda
                Util.Write("D");

                if (_prevNode?.IsChildOf(TreeNode) ?? false)
                {
                    var nodeUplevel = _prevNode.Parent;

                    while (nodeUplevel?.IsChildOf(TreeNode) ?? false)
                    {
                        Util.Write("E");
                        if (null != (nodeRet = FindMapNode(nodeUplevel, pt)))
                            return;     // from lambda
                        Util.Write("F");

                        nodeUplevel = nodeUplevel.Parent;
                    }
                }

                Util.Assert(99882,
                    (null == _prevNode) ||
                    (false == TreeNode.IsChildOf(_prevNode)));

                Util.Write("G");
                if (null != (nodeRet = FindMapNode(TreeNode, pt)))
                    return;         // from lambda
                Util.Write("H");

                nodeRet = TreeNode;

                if (bVolumeView)
                    return;         // from lambda

                Util.Write("I");
                var nodeRet_A = FindMapNode(nodeRet.TreemapFiles, pt);
                Util.Write("J");

                if (null != nodeRet_A)
                    nodeRet = nodeRet_A;        // file list node
            });

            if (ReferenceEquals(nodeRet, _prevNode))
                nodeRet = TreeNode;             // file list node

            SelRectAndTooltip(nodeRet, /* UI Initiator */ kSelRectAndTooltip);
            return null;
        }

        void LV_TreeListChildrenVM_TreeListChildSelected(Tuple<LocalTreeNode, decimal> initiatorTuple)
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

            SelRectAndTooltip(treeNodeChild, initiatorTuple.Item2);
        }

        void LV_FilesVM_SelectedFileChanged(Tuple<LV_FilesVM.SelectedFileChanged, decimal> initiatorTuple)
        {
            var tuple = initiatorTuple.Item1;

            Util.Write("O");
            if (_bTreeSelect ||
                _bSelRecAndTooltip ||
                (null == tuple))
            {
                return;
            }

            if (null == tuple.treeNode.TreemapFiles)     // TODO: Why would this be null?
                return;

            bool bCloseTooltip = true;

            tuple.fileLine.FirstOrDefault(strFile =>
                tuple.treeNode.TreemapFiles.Nodes
                .Where(treeNodeA => treeNodeA.PathShort == strFile)
                .Where(treeNodeA => treeNodeA is LocalTreemapNode)
                .FirstOnlyAssert(fileNode =>
            {
                SelRectAndTooltip(fileNode, initiatorTuple.Item2);
                bCloseTooltip = false;
            }));

            if (bCloseTooltip)                                  // TODO: Why would this occur?
                WinTooltip.CloseTooltip();
        }

        void SelRectAndTooltip(LocalTreeNode treeNode, decimal nInitiator)
        {
            if (_bTreeSelect ||
                _bSelRecAndTooltip)
            {
                return;
            }

            if (SelChildNode == treeNode)
                return;

            _bSelRecAndTooltip = true;

            var nodeDatum = treeNode.NodeDatum;
            var strFolder = treeNode.PathShort;
            var nodeTreeSelect = treeNode;

            if (nodeTreeSelect is LocalTreemapNode)
            {
                strFolder += " (file)";
                nodeTreeSelect = treeNode.Parent.Parent;   // Parent is TreemapFileListNode
                SelectedFileOnNext(treeNode.PathShort, nInitiator);
            }
            else if (nodeTreeSelect is LocalTreemapFileListNode)
            {
                nodeTreeSelect = nodeTreeSelect.Parent;
            }

            WinTooltip.ShowTooltip(
                new WinTooltip.ArgsStruct(
                    strFolder,
                    nodeDatum.LengthTotal.FormatSize(bytes: true),
                    LocalOwner,
                    Tooltip_Click,
                    () => ClearSelection(bDontCloseTooltip: true)),
                treeNode);

            SelChildNode = treeNode;
            _prevNode = treeNode;

            if (LV_TreeListChildrenVM.kChildSelectedOnNext != nInitiator)
                _bTreeSelect = TreeSelect.DoThreadFactory(nodeTreeSelect, nInitiator);

            _bSelRecAndTooltip = false;
        }

        LocalTreeNode
            FindMapNode(LocalTreeNode treeNode, Point pt)
        {
            if (null == treeNode?.Nodes)
                treeNode = treeNode?.Parent;

            if (null == treeNode)
                return null;

            pt.X /= TreemapFrame.ScaleFactor;
            pt.Y /= TreemapFrame.ScaleFactor;

            for (var testNode = treeNode; null != testNode; testNode = testNode.Parent)
            {
                foreach (var subNode in testNode.Nodes)
                {
                    if (subNode.TreemapRect.Contains(pt))
                        return subNode;
                }

                if (testNode.TreemapFiles?.TreemapRect.Contains(pt) ?? false)
                    return testNode.TreemapFiles;

                if (treeNode is ITreemapNode)
                    return null;

                if (ReferenceEquals(treeNode, TreeNode))
                    return null;
            }

            return null;
        }

        void Render(LocalTreeNode treeNode)
        {
            if (_bSelRecAndTooltip ||
                _bTreeSelect)
            {
                return;
            }

            if (false == (DeepNode?.IsChildOf(treeNode) ?? false))
                DeepNode = treeNode;

            ClearSelection();
            TreeNode = treeNode;    // Must follow setting DeepNode above

            if (null == TreeNode.Parent)
                TreeNode.RootNodeDatum.VolumeView = _bTooltipVolumeView;

            _bTooltipVolumeView = true;

            var timer = Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1)).Timestamp()
                .LocalSubscribe(99906, x => Util.WriteLine(DateTime.Now.Ticks + " " + treeNode + " DrawTreemap"));

            IEnumerable<TreemapFrame> ieFrames = null;
            var ieFills = DrawTreemap(out ieFrames);

            timer.Dispose();

            Util.UIthread(99823, () =>
            {
                if (null != LocalOwner)
                    LocalOwner.Title = Util.Localized("Title");

                TreemapDrawing = null;
                Frames.Clear();

                if (null == ieFills)
                    return;     // from lambda

                var drawingGroup = new DrawingGroup();

                drawingGroup.Children.Add(
                    new GeometryDrawing(
                    new SolidColorBrush(Color.FromRgb(193, 176, 139)), new Pen(), new RectangleGeometry(new Rect(0, 0, BitmapSize, BitmapSize))));

                foreach (var render in ieFills.OrderByDescending(r => r.Area).Take(1 << 11))
                    drawingGroup.Children.Add(render.GeometryDrawing);

                drawingGroup.Freeze();
                TreemapDrawing = drawingGroup;

                if (null != ieFrames)
                {
                    foreach (var frame in ieFrames.OrderByDescending(r => r.Area).Take(1 << 10))
                        Frames.Add(frame);
                }

                if (null != LocalOwner)
                    LocalOwner.Title = treeNode.PathShort;
            });

            RaisePropertyChanged("TreemapDrawing");
            RaisePropertyChanged("Frames");

            if ((null == _deepNodeDrawn) ||
                ReferenceEquals(_deepNodeDrawn, TreeNode))
            {
                _rectDeepnode = Rect.Empty;
            }
            else
            {
                _rectDeepnode =
                    _deepNodeDrawn.TreemapRect;
            }

            RaisePropertyChanged("GoofballVisibility");
            RaisePropertyChanged("GoofballX");
            RaisePropertyChanged("GoofballY");
            SelChildNode = null;
            _prevNode = null;
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
        
        IEnumerable<TreemapFill>
            DrawTreemap(out IEnumerable<TreemapFrame> ieFrames)
        {
            _deepNodeDrawn = null;
            ieFrames = null;

            TreeNode.TreemapRect = new Rect(0, 0, BitmapSize, BitmapSize).Scale(1d / TreemapFrame.ScaleFactor);

            return
                (0 < TreeNode.NodeDatum.LengthTotal)
                ? new Recurse().Render(TreeNode, DeepNode, out _deepNodeDrawn, out ieFrames)
                : new[] { new TreemapFill(TreeNode) };
        }

        class
            Recurse
        {
            internal IEnumerable<TreemapFill>
                Render(LocalTreeNode treeNode, LocalTreeNode deepNode,
                out LocalTreeNode deepNodeDrawn_out, out IEnumerable<TreemapFrame> ieFrames)
            {
                _lsFills = new ConcurrentBag<TreemapFill>();
                _lsFrames = new ConcurrentBag<TreemapFrame>();
                _deepNode = deepNode;
                RecurseDrawGraph(treeNode, bStart: true);

                using (Observable.Timer(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1)).Timestamp()
                    .LocalSubscribe(99881, x => Util.WriteLine(_nWorkerCount + " " + treeNode + " DrawTreemap")))
                using (Observable.Timer(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100)).Timestamp()
                    .LocalSubscribe(99825, x =>
                {
                    if (0 < _nWorkerCount)
                        return;     // from lambda

                    _blockingFrame.Continue = false;
                    Util.WriteLine(DateTime.Now.Ticks + " " + treeNode + " DrawTreemap - cleared stuck worker count during 100ms check");
                }))
                {
                    if (0 < _nWorkerCount)
                        _blockingFrame.PushFrameTrue();
                }

                deepNodeDrawn_out = _deepNodeDrawn;
                ieFrames = _lsFrames;
                return _lsFills;
            }
            
            void
                RecurseDrawGraph(LocalTreeNode treeNode, bool bStart = false)
            {
                if ((treeNode == _deepNode) || (_deepNode?.IsChildOf(treeNode) ?? false))
                    _deepNodeDrawn = treeNode;

                if (1 > treeNode.TreemapRect.Area())
                    return;

                Action<LocalTreeNode> addTreenodeToRender = (treeNodeA) =>
                {
                    var element = new TreemapFill(treeNodeA);

                    _lsFills.Add(element);

                    if (treeNodeA is LocalTreemapNode)
                        _lsFrames.Add(element);
                };

                if (1 << 10 > treeNode.TreemapRect.Area())
                {
                    // Speedup. Draw an "empty" folder in place of too much detail
                    addTreenodeToRender(treeNode);
                    return;
                }

                if ((false == treeNode is LocalTreemapNode) &&
                    (0 < treeNode.NodeDatum.LengthHere))
                {
                    if (null == treeNode.TreemapFiles)
                        treeNode.TreemapFiles = new LocalTreemapFileListNode(treeNode);

                    if (bStart)
                        treeNode.TreemapFiles.GetFileList();

                    treeNode.TreemapFiles.Start = bStart;
                }

                IEnumerable<LocalTreeNode> ieChildren = null;
                LocalTreeNode parent = treeNode;
                var rootNodeDatum = treeNode.NodeDatum.As<RootNodeDatum>();

                if (bStart && (rootNodeDatum?.VolumeView ?? false))
                {
                    var nodeFree = new LocalTreemapNode(treeNode, rootNodeDatum.VolumeFree, "free space", UtilColorcode.TreemapFreespace);
                    var nVolumeLength = rootNodeDatum.VolumeLength;

                    var nUnreadLength =
                        (long)nVolumeLength -
                        (long)rootNodeDatum.VolumeFree -
                        (long)rootNodeDatum.LengthTotal;

                    // parent added as child, with two other nodes: free space (color: spring green); and...
                    ieChildren = new[] { treeNode, nodeFree };

                    if (0 < nUnreadLength)
                    {
                        // ...unread guess, affected by compression and hard links (violet)
                        ieChildren = ieChildren.Concat(new[]
                        {
                            new LocalTreemapNode(treeNode, (ulong)nUnreadLength,
                                "unread data (estimate affected by compression and hard links)", UtilColorcode.TreemapUnreadspace)
                        });
                    }
                    else
                    {
                        // Faked length to make up for compression and hard links
                        nVolumeLength = rootNodeDatum.VolumeFree + rootNodeDatum.LengthTotal;
                    }

                    parent = new LocalTreemapNode(treeNode, nVolumeLength, treeNode.PathShort + " (volume)", UtilColorcode.Transparent)
                    {
                        TreemapRect = treeNode.TreemapRect
                    };
                }
                else if (null != treeNode.Nodes)
                {
                    ieChildren = treeNode.Nodes;
                }
                else if (null != treeNode.TreemapFiles)
                {
                    ieChildren = new List<LocalTreeNode> { };
                }
                else
                {
                    // There are no children. Draw a file or an empty folder.
                    addTreenodeToRender(treeNode);
                    return;
                }

                if (null != parent.TreemapFiles)
                    ieChildren = ieChildren.Concat(new[] { parent.TreemapFiles });

                var ieOrderedChildren =
                    ieChildren
                    .OrderByDescending(treeNodeA => treeNodeA.NodeDatum.LengthTotal);

                foreach (var treeNodeB in ieOrderedChildren.Where(treeNodeA => 0 == treeNodeA.NodeDatum.LengthTotal))
                    treeNodeB.TreemapRect = new Rect();

                var lsOrderedChildren = ieOrderedChildren.Where(treeNodeA => 0 < treeNodeA.NodeDatum.LengthTotal)
                    .ToList();

                if (0 < lsOrderedChildren.Count)
                    KDirStat_DrawChildren(parent, lsOrderedChildren, bStart);
            }

            //My first approach was to make this member pure virtual and have three
            //classes derived from CTreemap. The disadvantage is then, that we cannot
            //simply have a member variable of type CTreemap but have to deal with
            //pointers, factory methods and explicit destruction. It's not worth.

            //I learned this squarification style from the KDirStat executable.
            //It's the most complex one here but also the clearest, imho.

            void
                KDirStat_DrawChildren(LocalTreeNode parent, IReadOnlyList<LocalTreeNode> lsOrderedChildren, bool bStart)
            {
                var nCount = lsOrderedChildren.Count;

                Interlocked.Add(ref _nWorkerCount, nCount);

                var anChildWidth = new double[nCount];      // Widths of the children (fraction of row width).
                var rc = parent.TreemapRect;
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

                for (int nextChild = 0, childrenUsed = 0; nextChild < nCount; nextChild += childrenUsed)
                {
                    rows.Add(new RowStruct
                    {
                        RowHeight = KDirStat_CalculateNextRow(parent, nextChild, width_A, out childrenUsed, anChildWidth, lsOrderedChildren),
                        ChildrenPerRow = childrenUsed
                    });
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

                    var left = horizontalRows ? rc.Left : rc.Top;

                    for (var i = 0; i < row.ChildrenPerRow; i++, c++)
                    {
                        var child = lsOrderedChildren[c];

                        Util.Assert(1302.3305m, 0 <= anChildWidth[c]);

                        var right = left + anChildWidth[c] * width;

                        var lastChild = 
                            ((row.ChildrenPerRow - 1 == i) ||
                            anChildWidth[c + 1].Equals(0));

                        if (lastChild)
                            right = horizontalRows ? rc.Right() : rc.Bottom();

                        child.TreemapRect =
                            horizontalRows
                            ? new Rect(left, top, right - left, bottom - top)
                            : new Rect(top, left, bottom - top, right - left);

                        ThreadPool.QueueUserWorkItem(
                            state =>
                            {
                                RecurseDrawGraph((LocalTreeNode)state);
                                Interlocked.Decrement(ref _nWorkerCount);

                                if (0 == _nWorkerCount)
                                    _blockingFrame.Continue = false;
                            }, child
                        );

                        if (bStart)
                            _lsFrames.Add(new TreemapFrame(child));

                        if (lastChild)
                        {
                            i++;
                            c++;

                            if (i < row.ChildrenPerRow)
                                lsOrderedChildren[c].TreemapRect = new Rect(-1, -1, -1, -1);

                            c += row.ChildrenPerRow - i;
                            break;
                        }

                        left = right;
                    }
                    // This asserts due to rounding error: Utilities.Assert(1302.3306, left == (horizontalRows ? rc.Right : rc.Bottom));
                    top = bottom;
                });
                // This asserts due to rounding error: Utilities.Assert(1302.3307, top == (horizontalRows ? rc.Bottom : rc.Right));
            }

            static double KDirStat_CalculateNextRow(LocalTreeNode parent, int nextChild, double width,
                out int childrenUsed, double[] anChildWidth,
                IReadOnlyList<LocalTreeNode> listChildren)
            {
                childrenUsed = 0;

                const double kdMinProportion = 0.4;
                var nCount = listChildren.Count;

                Util.Assert(99967, 1 > kdMinProportion);
                Util.Assert(99966, nextChild < nCount);
                Util.Assert(99963, 1 <= width);

                double mySize = parent.NodeDatum.LengthTotal;
                ulong sizeUsed = 0;
                double rowHeight = 0;
                var i = 0;

                for (i = nextChild; i < nCount; ++i)
                {
                    var childSize = listChildren[i].NodeDatum.LengthTotal;

                    sizeUsed += childSize;

                    var virtualRowHeight = sizeUsed / mySize;

                    Util.Assert(99962, virtualRowHeight > 0);
                    Util.Assert(99961, virtualRowHeight <= 1);

                    // Rect(mySize)    = width * 1d
                    // Rect(childSize) = childWidth * virtualRowHeight
                    // Rect(childSize) = childSize / mySize * width

                    var childWidth =
                        childSize / mySize *
                        width / virtualRowHeight;

                    if (kdMinProportion > childWidth / virtualRowHeight)
                    {
                        Util.Assert(99923, i > nextChild); // because width >= 1 and _minProportion < 1.
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
                    var childSize = (double)listChildren[nextChild + i].NodeDatum.LengthTotal;
                    var cw = childSize / rowSize;

                    Util.Assert(99960, 0 <= cw);
                    anChildWidth[nextChild + i] = cw;
                }

                return rowHeight;
            }

            ConcurrentBag<TreemapFill>
                _lsFills = null;
            ConcurrentBag<TreemapFrame>
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
        bool
            _bTooltipVolumeView = true;

        // Recurse class
        LocalTreeNode
            _deepNodeDrawn = null;

        // goofball
        Rect
            _rectDeepnode = Rect.Empty;

        LocalTreeNode
            _prevNode = null;

        readonly IList<IDisposable>
            _lsDisposable = new List<IDisposable> { };
    }
}
