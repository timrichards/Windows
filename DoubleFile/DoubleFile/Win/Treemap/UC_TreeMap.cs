using System.Windows.Forms;
using System.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Collections.Concurrent;
using System.IO;
using System.Reactive.Linq;

namespace DoubleFile
{
    [System.ComponentModel.DesignerCategory("Code")]
    class UC_TreeMap : UserControl
    {
        static internal IObservable<Tuple<string, int>>
            SelectedFile { get { return _selectedFile.AsObservable(); } }
        static readonly LocalSubject<string> _selectedFile = new LocalSubject<string>();
        static void SelectedFileOnNext(string value, int nInitiator) { _selectedFile.LocalOnNext(value, 99841, nInitiator); }

        internal const int
            kSelRectAndTooltip = 99983;

        internal System.Windows.Window
            LocalOwner = null;
        internal WinTreeMapVM
            TreeMapVM = null;

        public UC_TreeMap()
        {
            DoubleBuffered = true;
            SetStyle(ControlStyles.DoubleBuffer |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint,
                true);
            BackgroundImageLayout = ImageLayout.Stretch;
            Dock = DockStyle.Fill;
            BackColor = Color.Transparent;

            var bMouseDown = false;

            _lsDisposable.Add(Observable.FromEventPattern(this, "MouseDown")
                .Subscribe(x => bMouseDown = true));

            _lsDisposable.Add(Observable.FromEventPattern<MouseEventArgs>(this, "MouseUp")
                .Subscribe(args => { if (bMouseDown) { bMouseDown = false; form_tmapUserCtl_MouseUp(args.EventArgs.Location); } }));

            _lsDisposable.Add(TreeSelect.FolderDetailUpdated.Subscribe(initiatorTuple =>
            {
                if (LV_TreeListChildrenVM.kChildSelectedOnNext == initiatorTuple.Item2)
                    return;

                var tuple = initiatorTuple.Item1;

                Util.Write("M");
                RenderD(tuple.Item2, initiatorTuple.Item2);
                _bTreeSelect = false;
            }));

            _lsDisposable.Add(LV_TreeListChildrenVM.TreeListChildSelected.Subscribe(LV_TreeListChildrenVM_TreeListChildSelected));
            _lsDisposable.Add(LV_FilesVM.SelectedFileChanged.Subscribe(LV_FilesVM_SelectedFileChanged));
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _lsDisposable.Add(Observable.Timer(TimeSpan.Zero, TimeSpan.FromMilliseconds(33)).Timestamp()         // 30 FPS
                .Subscribe(x =>
            {
                if (Rectangle.Empty == _rectCenter)
                    return;         // from lambda

                ++_nAnimFrame;

                if (0 == _nInvalidateRef)
                    Invalidate(_rectCenter);
            }));

            _lsDisposable.Add(TreeMapVM.TreeNodeCallback.Subscribe(TreeMapVM_TreeNodeCallback));
        }

        void InvalidatePushRef(Action action)
        {
            ++_nInvalidateRef;
            action();
            --_nInvalidateRef;

            if (0 == _nInvalidateRef)
                Invalidate();
        }

        void form_tmapUserCtl_MouseUp(Point ptLocation)
        {
            var treeNode = ZoomOrTooltip(ptLocation);

            if (null == treeNode)
                return;

            LocalTV.SelectedNode = treeNode;
        }

        internal void Clear()
        {
            TreeMapVM.TreeNode = null;
            _prevNode = null;
            TreeMapVM.DeepNode = null;
            _deepNodeDrawn = null;
            WinTooltip.CloseTooltip();      // Tag
            ClearSelection();
        }

        internal void ClearSelection(bool bKeepTooltipActive = false)
        {
            if (App.LocalExit)
                return;

            if (_bClearingSelection)
                return;

            _bClearingSelection = true;

            if (false == bKeepTooltipActive)
                WinTooltip.CloseTooltip();  // CloseTooltip callback recurses here hence _bClearingSelection

            _selChildNode = null;

            if (0 == _nInvalidateRef)
                Invalidate();

            _bClearingSelection = false;
        }

        protected override void Dispose(bool disposing)
        {
            Util.LocalDispose(_lsDisposable);

            Util.ThreadMake(() =>
            {
                if (null != _bg)
                    _bg.Dispose();

                _bg = null;
                WinTooltip.CloseTooltip();
                _deepNodeDrawn = null;
                _selChildNode = null;
                _prevNode = null;
            });

            base.Dispose(disposing);
        }

        internal LocalTreeNode ZoomOrTooltip(Point pt_in)
        {
            if (_bTreeSelect ||
                _bSelRecAndTooltip)
            {
                return null;
            }

            ClearSelection(bKeepTooltipActive: true);

            if (null == TreeMapVM.TreeNode)
                return null;

            if (_rectCenter.Contains(pt_in))   // click once to hide goofball. Click again within 5 seconds to return to the deep node.
            {
                if (DateTime.MinValue == _dtHideGoofball)
                {
                    _dtHideGoofball = DateTime.Now;
                    return null;
                }
                else if (DateTime.Now - _dtHideGoofball < TimeSpan.FromSeconds(5))
                {
                    _dtHideGoofball = DateTime.MinValue;
                    return TreeMapVM.DeepNode;
                }
            }

            _dtHideGoofball = DateTime.MinValue;   // click anywhere else on the treemap and the goofball returns.

            var pt = Point.Ceiling(new PointF(pt_in.X / _sizeTranslate.Width, pt_in.Y / _sizeTranslate.Height));
            LocalTreeNode nodeRet = null;
            var bFilesHere = false;
            var bVolumeView = false;

            Util.Closure(() =>
            {
                {
                    var nodeDatum = TreeMapVM.TreeNode.NodeDatum;

                    if (null == nodeDatum)      // added 2/13/15
                    {
                        MBoxStatic.Assert(99967, false);
                        return;     // from lambda
                    }

                    var rootNodeDatum = nodeDatum as RootNodeDatum;

                    bVolumeView =
                        ((null != rootNodeDatum) &&
                        rootNodeDatum.VolumeView);

                    if ((false == bVolumeView) &&
                        (null != (nodeRet = FindMapNode(nodeDatum.TreeMapFiles, pt))))
                    {
                        bFilesHere = true;
                        return;     // from lambda
                    }
                }

                var prevNode_A = _prevNode ?? TreeMapVM.TreeNode;

                if (null != (nodeRet = FindMapNode(prevNode_A, pt)))
                    return;         // from lambda

                if ((null != _prevNode) &&
                    _prevNode.IsChildOf(TreeMapVM.TreeNode))
                {
                    var nodeUplevel = _prevNode.Parent;

                    while ((null != nodeUplevel) &&
                        nodeUplevel.IsChildOf(TreeMapVM.TreeNode))
                    {
                        if (null != (nodeRet = FindMapNode(nodeUplevel, pt)))
                            return;     // from lambda

                        nodeUplevel = nodeUplevel.Parent;
                    }
                }

                MBoxStatic.Assert(99882,
                    (null == _prevNode) ||
                    (false == TreeMapVM.TreeNode.IsChildOf(_prevNode)));

                if (null != (nodeRet = FindMapNode(TreeMapVM.TreeNode, pt)))
                    return;         // from lambda

                nodeRet = TreeMapVM.TreeNode;

                if (bVolumeView)
                    return;         // from lambda

                var nodeDatum_A = nodeRet.NodeDatum;

                if (null == nodeDatum_A)      // added 2/13/15
                {
                    MBoxStatic.Assert(99923, false);
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
                nodeRet = TreeMapVM.TreeNode;
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

            if (false == ReferenceEquals(TreeMapVM.TreeNode, treeNodeChild.Parent))
                return;

            SelRectAndTooltip(treeNodeChild, initiatorTuple.Item2, bFile: false);
        }

        void LV_FilesVM_SelectedFileChanged(Tuple<Tuple<IEnumerable<FileDictionary.DuplicateStruct>, IEnumerable<string>, LocalTreeNode>, int> initiatorTuple)
        {
            var tuple = initiatorTuple.Item1;

            Util.Write("O");
            if (_bTreeSelect ||
                _bSelRecAndTooltip ||
                (null == tuple))
            {
                return;
            }

            if (null == tuple.Item3.NodeDatum.TreeMapFiles)     // TODO: Why would this be null?
                return;

            bool bCloseTooltip = true;

            tuple.Item2.First(strFile =>
                tuple.Item3.NodeDatum.TreeMapFiles.Nodes
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

            if (_selChildNode == treeNodeChild)
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
                    () => ClearSelection()),
                treeNodeChild);

            _selChildNode = treeNodeChild;
            _prevNode = treeNodeChild;

            if (0 == _nInvalidateRef)   // jic
                Invalidate();

            if (LV_TreeListChildrenVM.kChildSelectedOnNext != nInitiator)
                _bTreeSelect = TreeSelect.DoThreadFactory(nodeTreeSelect, nInitiator);

            _bSelRecAndTooltip = false;
        }

        static LocalTreeNode FindMapNode(LocalTreeNode treeNode_in, Point pt, bool bNextNode = false)
        {
            var treeNode = treeNode_in;

            if (null == treeNode)
                return null;

            do
            {
                var nodeDatum = treeNode.NodeDatum;

                if (null == nodeDatum)      // added 2/13/15
                {
                    MBoxStatic.Assert(99966, false);
                    return null;
                }

                if (false == nodeDatum.TreeMapRect.Contains(pt))
                    continue;

                if (bNextNode ||
                    (treeNode != treeNode_in))
                {
                    return treeNode;
                }

                if ((null == treeNode.Nodes) ||
                    (treeNode.Nodes.IsEmpty()))
                {
                    continue;
                }

                var foundNode = FindMapNode(treeNode.Nodes[0], pt, bNextNode: true);

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
            var rootNodeDatum = parent.Root().NodeDatum as RootNodeDatum;

            if ((null == nodeDatum) ||
                (nodeDatum.LineNo == 0) ||
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
            var strListingFile = rootNodeDatum.ListingFile;
            var lsFiles = new List<Tuple<string, ulong>>();

            foreach (var asFileLine
                in File.ReadLines(strListingFile)
                .Skip(nPrevDir)
                .Take((nLineNo - nPrevDir - 1))
                .Select(s =>
                    s
                    .Split('\t')
                    .Skip(3)                    // makes this an LV line: knColLengthLV
                    .ToArray()))
            {
                ulong nLength = 0;

                if ((asFileLine.Length > Util.knColLengthLV) &&
                    (false == string.IsNullOrWhiteSpace(asFileLine[Util.knColLengthLV])))
                {
                    nLengthDebug += nLength = ulong.Parse(asFileLine[Util.knColLengthLV]);
                    asFileLine[Util.knColLengthLV] = Util.FormatSize(asFileLine[Util.knColLengthLV]);
                }

                lsFiles.Add(Tuple.Create(asFileLine[0], nLength));
            }

            MBoxStatic.Assert(1301.2313, nLengthDebug == nodeDatum.Length);

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
                    ForeColor = UtilColor.OliveDrab
                });
            }

            if (0 == nTotalLength)
                return null;

            MBoxStatic.Assert(1302.3301, nTotalLength == parent.NodeDatum.Length);

            return new LocalTreeMapFileListNode(parent, lsNodes)
            {
                NodeDatum = new NodeDatum { TotalLength = nTotalLength, TreeMapRect = parent.NodeDatum.TreeMapRect },
                SelectedImageIndex = -1
            };
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (null != _selChildNode)
            {
                e.Graphics.FillRectangle(
                    new SolidBrush(Color.FromArgb(64, 0, 0, 0)),
                    _selChildNode.NodeDatum.TreeMapRect.Scale(_sizeTranslate));
            }

            if ((null == _deepNodeDrawn) ||
                ReferenceEquals(_deepNodeDrawn, TreeMapVM.TreeNode))
            {
                _rectCenter = Rectangle.Empty;
                return;
            }

            if (DateTime.MinValue != _dtHideGoofball)
                return;

            var nodeDatum = _deepNodeDrawn.NodeDatum;

            if (null == nodeDatum)      // added 2/13/15
            {
                MBoxStatic.Assert(99965, false);
                return;
            }

            RectangleF r = 
                nodeDatum.TreeMapRect
                .Scale(_sizeTranslate);

            r.Inflate(-r.Width / 2 + 15, -r.Height / 2 + 15);
            _rectCenter = Rectangle.Ceiling(r);

            var path = new GraphicsPath();

            path.AddEllipse(_rectCenter);

            var brush = new PathGradientBrush(path)
            {
                CenterColor = Color.White,
                SurroundColors = new[] { Color.FromArgb(0, 0, 0, 0) }
            };

            e.Graphics.FillEllipse(brush, _rectCenter);
            r.Inflate(-r.Width / 5, -r.Height / 5);

            var r_A = Rectangle.Ceiling(r);
            var nAnimFrame = (_nAnimFrame %= 6) * 30;

            brush.CenterColor = Color.White;
            brush.SurroundColors = new[] { Color.Black };
            e.Graphics.FillPie(brush, r_A, 90 + nAnimFrame, 90);
            e.Graphics.FillPie(brush, r_A, 270 + nAnimFrame, 90);
            brush.CenterColor = Color.Black;
            brush.SurroundColors = new[] { Color.White };
            e.Graphics.FillPie(brush, r_A, 0 + nAnimFrame, 90);
            e.Graphics.FillPie(brush, r_A, 180 + nAnimFrame, 90);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            TranslateSize();
            _prevNode = null;
            ClearSelection();
        }

        internal void Tooltip_Click()
        {
            var treeNode = WinTooltip.LocalTreeNode;

            if (treeNode is LocalTreeMapFileNode)
                return;     // file fake node

            WinTooltip.CloseTooltip();
            RenderA(treeNode, nInitiator: 0);
        }

        void TreeMapVM_TreeNodeCallback(Tuple<LocalTreeNode, int> initiatorTuple)
        {
            Util.UIthread(() => RenderA(initiatorTuple.Item1, initiatorTuple.Item2));
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

            InvalidatePushRef(() => Render(treeNode));
        }

        void Render(LocalTreeNode treeNode)
        {
            if ((null == TreeMapVM.DeepNode) ||
                (false == TreeMapVM.DeepNode.IsChildOf(treeNode)))
            {
                TreeMapVM.DeepNode = treeNode;
            }

            var nPxPerSide = (treeNode.SelectedImageIndex < 0)
                ? 2048
                : treeNode.SelectedImageIndex;

            if ((null == _bg) ||
                (null == _bg.Graphics) ||
                (nPxPerSide != _rectBitmap.Size.Width))
            {
                var dtStart_A = DateTime.Now;

                _rectBitmap = new Rectangle(0, 0, nPxPerSide, nPxPerSide);
                BackgroundImage = new Bitmap(_rectBitmap.Size.Width, _rectBitmap.Size.Height);

                var bgcontext = BufferedGraphicsManager.Current;

                bgcontext.MaximumBuffer = _rectBitmap.Size;

                if (null != _bg)
                    _bg.Dispose();

                _bg = bgcontext.Allocate(Graphics.FromImage(BackgroundImage), _rectBitmap);
                TranslateSize();
                Util.WriteLine("Size bitmap " + nPxPerSide + " " + (DateTime.Now - dtStart_A).TotalMilliseconds / 1000d + " seconds.");
            }

            var dtStart = DateTime.Now;

            ClearSelection();
            TreeMapVM.TreeNode = treeNode;
            _lsRenderActions = DrawTreemap();

            Util.UIthread(() =>
            {
               if (null != LocalOwner)
                    LocalOwner.Title = "Double File";

               try
               {
                   _bg.Graphics.Clear(Color.DarkGray);

                   if (null == _lsRenderActions)
                       return;     // from lambda

                   foreach (var stroke in _lsRenderActions)
                       stroke.Stroke(_bg.Graphics);

                   _lsRenderActions = null;
                   _bg.Graphics.DrawRectangle(new Pen(Brushes.Black, 10), _rectBitmap);
                   _bg.Render();

                   if (null != LocalOwner)
                       LocalOwner.Title = treeNode.Text;
               }
               catch (ArgumentException) { MBoxStatic.Assert(99979, false); }
            });

            _selChildNode = null;
            _prevNode = null;
            _dtHideGoofball = DateTime.MinValue;

            if ((DateTime.Now - dtStart) > TimeSpan.FromSeconds(1))
            {
                treeNode.SelectedImageIndex = Math.Max((int)
                    (((treeNode.SelectedImageIndex < 0) ? _rectBitmap.Size.Width : treeNode.SelectedImageIndex)
                    * .75), 256);
            }
        }

        void TranslateSize()
        {
            SizeF sizeBitmap = _rectBitmap.Size;
            SizeF size = Size;

            _sizeTranslate = new SizeF(size.Width / sizeBitmap.Width, size.Height / sizeBitmap.Height);
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
        
        IEnumerable<RenderAction> DrawTreemap()
        {
            _deepNodeDrawn = null;

            var rc = _rectBitmap;

            rc.Width--;
            rc.Height--;

            if (rc.Width <= 0 || rc.Height <= 0)
                return null;

            var nodeDatum = TreeMapVM.TreeNode.NodeDatum;

            if (null == nodeDatum)      // added 2/13/15
            {
                MBoxStatic.Assert(99963, false);
                return null;
            }

            return
                (nodeDatum.TotalLength > 0)
                ? new Recurse().Render(TreeMapVM.TreeNode, rc, TreeMapVM.DeepNode, out _deepNodeDrawn)
                : new[] { new FillRectangle { Brush = Brushes.Wheat, rc = rc } };
        }

        class Recurse
        {
            internal IEnumerable<RenderAction>
                Render(LocalTreeNode item, Rectangle rc, LocalTreeNode deepNode, out LocalTreeNode deepNodeDrawn_out)
            {
                _lsRenderActions = new ConcurrentBag<RenderAction>();
                _lsFrames = new ConcurrentBag<RenderAction>();
                _deepNode = deepNode;
                RecurseDrawGraph(item, rc, true);

                while (_nWorkerCount > 0)
                    Util.Block(20);

                deepNodeDrawn_out = _deepNodeDrawn;
                return _lsRenderActions.Concat(_lsFrames);
            }
            
            void RecurseDrawGraph(
                LocalTreeNode item,
                Rectangle rc,
                bool bStart = false)
            {
#if (DEBUG)
                MBoxStatic.Assert(1302.3303, rc.Width >= 0);
                MBoxStatic.Assert(1302.3304, rc.Height >= 0);
#endif
                var nodeDatum = item.NodeDatum;

                if (null == nodeDatum)      // added 2/13/15
                {
                    MBoxStatic.Assert(99962, false);
                    return;
                }

                nodeDatum.TreeMapRect = rc;

                if (rc.Width < 1 ||
                    rc.Height < 1)
                {
                    return;
                }

                if (rc.Width < 32 ||
                    rc.Height < 32)
                {
                    // Speedup. Draw an "empty" folder in place of too much detail
                    DrawNode(item, rc);
                    return;
                }

                if ((null != _deepNode) &&
                    ((item == _deepNode) || (_deepNode.IsChildOf(item))))
                {
                    _deepNodeDrawn = item;
                }

                if (bStart &&
                    (null == nodeDatum.TreeMapFiles) &&
                    (false == item is LocalTreeMapFileNode))
                {
                    nodeDatum.TreeMapFiles = GetFileList(item);
                }

                if ((null != item.Nodes) && (false == item.Nodes.IsEmpty()) ||
                    (bStart && (null != nodeDatum.TreeMapFiles)))
                {
                    IEnumerable<LocalTreeNode> ieChildren = null;
                    LocalTreeNode parent = null;
                    var bVolumeNode = false;

                    Util.Closure(() =>
                    {
                        if ((false == bStart) ||
                            (false == item.NodeDatum is RootNodeDatum))
                        {
                            return;     // from lambda
                        }

                        var rootNodeDatum = (RootNodeDatum)item.NodeDatum;

                        if (false == rootNodeDatum.VolumeView)
                            return;     // from lambda

                        var nodeDatumFree = new NodeDatum { TotalLength = rootNodeDatum.VolumeFree };

                        var nodeFree = new LocalTreeMapFileNode(item.Text + " (free space)")
                        {
                            NodeDatum = nodeDatumFree,
                            ForeColor = UtilColor.MediumSpringGreen
                        };

                        var nodeDatumUnread = new NodeDatum();
                        var nVolumeLength = rootNodeDatum.VolumeLength;
                        var nUnreadLength = (long)nVolumeLength -
                            (long)rootNodeDatum.VolumeFree -
                            (long)rootNodeDatum.TotalLength;

                        if (nUnreadLength > 0)
                        {
                            nodeDatumUnread.TotalLength = (ulong)nUnreadLength;
                        }
                        else
                        {
                            // Faked length to make up for compression and hard links
                            nVolumeLength = rootNodeDatum.VolumeFree +
                                rootNodeDatum.TotalLength;

                            nodeDatumUnread.TotalLength = 0;
                        }

                        var nodeUnread = new LocalTreeMapFileNode(item.Text + " (unread data)")
                        {
                            NodeDatum = nodeDatumUnread,
                            ForeColor = UtilColor.MediumVioletRed
                        };

                        // parent added as child, with two other nodes:
                        // free space (color: spring green); and...
                        var lsChildren = new List<LocalTreeNode> { item, nodeFree };

                        if (nUnreadLength > 0)
                        {
                            // ...unread guess, affected by compression and hard links (violet)
                            lsChildren.Add(nodeUnread);
                        }

                        ieChildren = lsChildren;
                        parent = new LocalTreeMapFileNode(item.Text + " (volume)");

                        var nodeDatumVolume = new NodeDatum
                        {
                            TotalLength = nVolumeLength,
                            TreeMapRect = rootNodeDatum.TreeMapRect
                        };

                        parent.NodeDatum = nodeDatumVolume;
                        bVolumeNode = true;
                    });

                    if ((bVolumeNode == false) &&
                        (null != item.Nodes))
                    {
                        parent = item;

                        ieChildren =
                            item.Nodes
                            .Cast<LocalTreeNode>()
                            .Where(t => 0 < t.NodeDatum.TotalLength);
                    }

                    if ((null == ieChildren) &&
                        (null != nodeDatum.TreeMapFiles))
                    {
                        parent = item;
                        ieChildren = new List<LocalTreeNode>();
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
                DrawNode(item, rc);
            }

            void DrawNode(LocalTreeNode item, Rectangle rc)
            {
                var path = new GraphicsPath();
                var r = rc;

                r.Inflate(r.Width >> 1, r.Height >> 1);
                path.AddEllipse(r);

                var brush = new PathGradientBrush(path)
                {
                    CenterColor = Color.Wheat,
                    SurroundColors = new[] { ControlPaint.Dark(
                        (UtilColor.Empty == item.ForeColor)
                        ? Color.SandyBrown
                        : Color.FromArgb(item.ForeColor)
                    )}
                };

                _lsRenderActions.Add(new FillRectangle { Brush = brush, rc = rc });
            }

            //My first approach was to make this member pure virtual and have three
            //classes derived from CTreemap. The disadvantage is then, that we cannot
            //simply have a member variable of type CTreemap but have to deal with
            //pointers, factory methods and explicit destruction. It's not worth.

            //I learned this squarification style from the KDirStat executable.
            //It's the most complex one here but also the clearest, imho.

            bool KDirStat_DrawChildren(LocalTreeNode parent, IEnumerable<LocalTreeNode> ieChildren, bool bStart)
            {
                var nodeDatum = parent.NodeDatum;
                var rc = nodeDatum.TreeMapRect;
                var rows = new List<RowStruct>();

                if (bStart &&
                    (null != nodeDatum.TreeMapFiles))
                {
                    ieChildren = ieChildren.Concat(new[] { nodeDatum.TreeMapFiles });
                }
                else if (nodeDatum.Length > 0)
                {
                    ieChildren = ieChildren.Concat(new[] { new LocalTreeMapFileNode(parent.Text)
                    {
                        NodeDatum = new NodeDatum { TotalLength = nodeDatum.Length },
                        ForeColor = UtilColor.DarkKhaki
                    }});
                }

                var lsChildren =
                    ieChildren
                    .OrderByDescending(x => x.NodeDatum.TotalLength)
                    .ToList();

                if (lsChildren.IsEmpty())
                {
                    // any files are zero in length
                    return false;
                }

                Interlocked.Add(ref _nWorkerCount, lsChildren.Count);

                var anChildWidth = // Widths of the children (fraction of row width).
                    new Double[lsChildren.Count];

                var horizontalRows = (rc.Width >= rc.Height);
                var width_A = 1d;

                if (horizontalRows)
                {
                    if (rc.Height > 0)
                        width_A = (double)rc.Width / rc.Height;
                }
                else
                {
                    if (rc.Width > 0)
                        width_A = (double)rc.Height / rc.Width;
                }

                {
                    var childrenUsed = 0;

                    for (var nextChild = 0; nextChild < lsChildren.Count; nextChild += childrenUsed)
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
                double top = horizontalRows ? rc.Top : rc.Left;

                rows.ForEach(row =>
                {
                    var fBottom = top + row.RowHeight * height;
                    var bottom = (int)fBottom;

                    if (ReferenceEquals(row, rows[rows.Count - 1]))
                        bottom = horizontalRows ? rc.Bottom : rc.Right;

                    double left = horizontalRows ? rc.Left : rc.Top;

                    for (var i = 0; i < row.ChildrenPerRow; i++, c++)
                    {
                        var child = lsChildren[c];
                        MBoxStatic.Assert(1302.3305, anChildWidth[c] >= 0);
                        var fRight = left + anChildWidth[c] * width;
                        var right = (int)fRight;

                        var lastChild = 
                            ((i == row.ChildrenPerRow - 1) ||
                            anChildWidth[c + 1].Equals(0));

                        if (lastChild)
                            right = horizontalRows ? rc.Right : rc.Bottom;

                        var rcChild =
                            horizontalRows
                            ? new Rectangle((int)left, (int)top, right - (int)left, bottom - (int)top)
                            : new Rectangle((int)top, (int)left, bottom - (int)top, right - (int)left);

                        ThreadPool.QueueUserWorkItem(
                            state =>
                            {
                                var param = (Tuple<LocalTreeNode, Rectangle>)state;

                                RecurseDrawGraph(param.Item1, param.Item2);
                                Interlocked.Decrement(ref _nWorkerCount);
                            },
                            Tuple.Create(child, rcChild)
                        );

                        if (bStart)
                            _lsFrames.Add(new DrawRectangle { rc = rcChild });

                        if (lastChild)
                        {
                            i++;
                            c++;

                            if (i < row.ChildrenPerRow)
                                lsChildren[c].NodeDatum.TreeMapRect = new Rectangle(-1, -1, -1, -1);

                            c += row.ChildrenPerRow - i;
                            break;
                        }

                        left = fRight;
                    }
                    // This asserts due to rounding error: Utilities.Assert(1302.3306, left == (horizontalRows ? rc.Right : rc.Bottom));
                    top = fBottom;
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
                MBoxStatic.Assert(1302.3308, kdMinProportion < 1);

                MBoxStatic.Assert(1302.3309, nextChild < listChildren.Count);
                MBoxStatic.Assert(1302.33101, width >= 1d);

                var nodeDatum = parent.NodeDatum;

                if (null == nodeDatum)      // added 2/13/15
                {
                    MBoxStatic.Assert(99961, false);
                    return 0;
                }

                var mySize = (double)nodeDatum.TotalLength;
                ulong sizeUsed = 0;
                double rowHeight = 0;
                var i = 0;

                for (i = nextChild; i < listChildren.Count; i++)
                {
                    var childSize = listChildren[i].NodeDatum.TotalLength;
                    sizeUsed += childSize;
                    var virtualRowHeight = sizeUsed / mySize;
                    MBoxStatic.Assert(1302.3311, virtualRowHeight > 0);
                    MBoxStatic.Assert(1302.3312, virtualRowHeight <= 1);

                    // Rectangle(mySize)    = width * 1d
                    // Rectangle(childSize) = childWidth * virtualRowHeight
                    // Rectangle(childSize) = childSize / mySize * width

                    double childWidth = childSize / mySize * width / virtualRowHeight;

                    if (childWidth / virtualRowHeight < kdMinProportion)
                    {
                        MBoxStatic.Assert(1302.3313, i > nextChild); // because width >= 1 and _minProportion < 1.
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

                MBoxStatic.Assert(1302.3314, i > nextChild);

                // Now i-1 is the last child used
                // and rowHeight is the height of the row.

                childrenUsed = i - nextChild;

                // Now as we know the rowHeight, we compute the widths of our children.
                for (i = 0; i < childrenUsed; i++)
                {
                    // Rectangle(1d * 1d) = mySize
                    var rowSize = mySize * rowHeight;
                    var nodeDatum_A = listChildren[nextChild + i].NodeDatum;

                    if (null == nodeDatum_A)      // added 2/13/15
                    {
                        MBoxStatic.Assert(99960, false);
                        return 0;
                    }

                    var childSize = (double)nodeDatum_A.TotalLength;
                    var cw = childSize / rowSize;
                    MBoxStatic.Assert(1302.3315, cw >= 0);
                    anChildWidth[nextChild + i] = cw;
                }

                return rowHeight;
            }

            ConcurrentBag<RenderAction>
                _lsRenderActions = null;
            ConcurrentBag<RenderAction>
                _lsFrames = null;
            LocalTreeNode
                _deepNode = null;
            LocalTreeNode
                _deepNodeDrawn = null;

            struct
                RowStruct { internal double RowHeight; internal int ChildrenPerRow; }
            int
                _nWorkerCount = 0;
        }

        abstract class
            RenderAction { internal Rectangle rc; internal abstract void Stroke(Graphics g); }
        class
            FillRectangle : RenderAction { internal Brush Brush; internal override void Stroke(Graphics g) { g.FillRectangle(Brush, rc); } }
        class
            DrawRectangle : RenderAction { static readonly Pen Pen = new Pen(Color.Black, 2); internal override void Stroke(Graphics g) { g.DrawRectangle(Pen, rc); } }

        BufferedGraphics
            _bg = null;
        Rectangle
            _rectBitmap = Rectangle.Empty;
        SizeF
            _sizeTranslate = SizeF.Empty;
        int
            _nInvalidateRef = 0;
        bool
            _bClearingSelection = false;
        bool
            _bTreeSelect = false;
        bool
            _bSelRecAndTooltip = false;

        // Recurse class
        IEnumerable<RenderAction>
            _lsRenderActions = null;
        LocalTreeNode
            _deepNodeDrawn = null;

        // goofball
        Rectangle
            _rectCenter = Rectangle.Empty;
        DateTime
            _dtHideGoofball = DateTime.MinValue;
        int
            _nAnimFrame = 0;

        // selection
        LocalTreeNode
            _selChildNode = null;
        LocalTreeNode
            _prevNode = null;

        List<IDisposable>
            _lsDisposable = new List<IDisposable>();
    }
}
