﻿using System.Windows.Forms;
using System.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Collections.Concurrent;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;

namespace DoubleFile
{
    class UC_TreeMapVM : SliderVM_Base<ListViewItemVM_Base>, IDisposable
    {
        public const double BitmapSize = 2048;

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
                var sz = new SizeF((float)BitmapSize / _rectBitmap.Width, (float)BitmapSize / _rectBitmap.Height);
                var rect = value?.NodeDatum.TreeMapRect.Scale(sz) ?? default(Rectangle);

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

        [DllImport("gdi32")]
        static extern int DeleteObject(IntPtr o);

        public System.Windows.Media.ImageSource
            TreeMapImage
        {
            get
            {
                if (null == _BackgroundImage)
                    return null;

                var hBitmap =  _BackgroundImage.GetHbitmap();

                var imageSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    System.Windows.Int32Rect.Empty,
                    System.Windows.Media.Imaging.BitmapSizeOptions.FromWidthAndHeight(_BackgroundImage.Width, _BackgroundImage.Height));

                DeleteObject(hBitmap);
                return imageSource;
            }
        }
        Bitmap _BackgroundImage;

        void Invalidate(Rectangle r = default(Rectangle)) => RaisePropertyChanged("TreeMapImage");

        internal override int NumCols => 0;

        internal IObservable<Tuple<LocalTreeNode, int>>
            TreeNodeCallback => _treeNodeCallback;
        readonly LocalSubject<LocalTreeNode> _treeNodeCallback = new LocalSubject<LocalTreeNode>();
        internal override void GoTo(LocalTreeNode treeNode) => _treeNodeCallback.LocalOnNext(treeNode, 99853);

        static internal IObservable<Tuple<string, int>>
            SelectedFile => _selectedFile;
        static readonly LocalSubject<string> _selectedFile = new LocalSubject<string>();
        static void SelectedFileOnNext(string value, int nInitiator) => _selectedFile.LocalOnNext(value, 99841, nInitiator);

        internal const int
            kSelRectAndTooltip = 99983;

        internal System.Windows.Window
            LocalOwner = null;

        public UC_TreeMapVM()
        {
            _lsDisposable.Add(TreeSelect.FolderDetailUpdated.Observable.LocalSubscribe(99696, initiatorTuple =>
            {
                if (LV_TreeListChildrenVM.kChildSelectedOnNext == initiatorTuple.Item2)
                    return;

                var tuple = initiatorTuple.Item1;

                Util.Write("M");
                RenderD(tuple.treeNode, initiatorTuple.Item2);
                _bTreeSelect = false;
            }));

            _lsDisposable.Add(LV_TreeListChildrenVM.TreeListChildSelected.LocalSubscribe(99695, LV_TreeListChildrenVM_TreeListChildSelected));
            _lsDisposable.Add(LV_FilesVM.SelectedFileChanged.Observable.LocalSubscribe(99694, LV_FilesVM_SelectedFileChanged));
            _lsDisposable.Add(TreeNodeCallback.LocalSubscribe(99692, TreeMapVM_TreeNodeCallback));
        }

        public void Dispose()
        {
            Util.LocalDispose(_lsDisposable);

            Util.ThreadMake(() =>
            {
                _bg?.Dispose();
                WinTooltip.CloseTooltip();
            });
        }

        void InvalidatePushRef(Action action)
        {
            ++_nInvalidateRef;
            action();
            --_nInvalidateRef;

            if (0 == _nInvalidateRef)
                Invalidate();
        }

        internal void form_tmapUserCtl_MouseUp(System.Windows.Point ptLocation)
        {
            var treeNode = ZoomOrTooltip(new Point((int)(ptLocation.X * _rectBitmap.Width), (int)(ptLocation.Y * _rectBitmap.Height)));

            if (null != treeNode)
                LocalTV.SelectedNode = treeNode;
        }

        internal void ClearSelection(bool bDontCloseTooltip = false)
        {
            if (System.Windows.Application.Current?.Dispatcher.HasShutdownStarted ?? true)
                return;

            if (_bClearingSelection)
                return;

            _bClearingSelection = true;

            if (false == bDontCloseTooltip)
                WinTooltip.CloseTooltip();  // CloseTooltip callback recurses here hence _bClearingSelection

            SelChildNode = null;

            if (0 == _nInvalidateRef)
                Invalidate();

            _bClearingSelection = false;
        }

        internal LocalTreeNode ZoomOrTooltip(Point pt)
        {
            if (_bTreeSelect ||
                _bSelRecAndTooltip)
            {
                return null;
            }

            ClearSelection(bDontCloseTooltip: true);

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

                //The following shows that you have to convert to int to compare to int.
                //var a = treeNode.Nodes?.Count;
                //var b = treeNode.Nodes?.Count ?? 0;

                //if (0 == treeNode.Nodes?.Count)
                //    continue;

                if (0 == (treeNode.Nodes?.Count ?? 0))
                    continue;

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
                    ForeColor = UtilColor.OliveDrab
                });
            }

            if (0 == nTotalLength)
                return null;

            Util.Assert(1302.3301m, nTotalLength == parent.NodeDatum.Length);

            return new LocalTreeMapFileListNode(parent, lsNodes)
            {
                NodeDatum = new NodeDatum { TotalLength = nTotalLength, TreeMapRect = parent.NodeDatum.TreeMapRect },
                SelectedImageIndex = -1
            };
        }

        internal void OnSizeChanged(EventArgs e)
        {
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

        void TreeMapVM_TreeNodeCallback(Tuple<LocalTreeNode, int> initiatorTuple) =>
            Util.UIthread(99825, () => RenderA(initiatorTuple.Item1, initiatorTuple.Item2));

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
            if (false == (DeepNode?.IsChildOf(treeNode) ?? false))
                DeepNode = treeNode;

            var nPxPerSide = (treeNode.SelectedImageIndex < 0)
                ? (int)BitmapSize
                : treeNode.SelectedImageIndex;

            if ((null == _bg?.Graphics) ||
                (nPxPerSide != _rectBitmap.Size.Width))
            {
                var dtStart_A = DateTime.Now;

                _rectBitmap = new Rectangle(0, 0, nPxPerSide, nPxPerSide);
                _BackgroundImage = new Bitmap(_rectBitmap.Size.Width, _rectBitmap.Size.Height);

                var bgcontext = BufferedGraphicsManager.Current;

                bgcontext.MaximumBuffer = _rectBitmap.Size;
                _bg?.Dispose();
                _bg = bgcontext.Allocate(Graphics.FromImage(_BackgroundImage), _rectBitmap);
                Util.WriteLine("Size bitmap " + nPxPerSide + " " + (DateTime.Now - dtStart_A).TotalMilliseconds / 1000d + " seconds.");
            }

            var dtStart = DateTime.Now;

            ClearSelection();
            TreeNode = treeNode;
            _ieRenderActions = DrawTreemap();

            Util.UIthread(99823, () =>
            {
               if (null != LocalOwner)
                    LocalOwner.Title = "Double File";

               try
               {
                   _bg.Graphics.Clear(Color.DarkGray);

                   if (null == _ieRenderActions)
                       return;     // from lambda

                   foreach (var stroke in _ieRenderActions)
                       stroke.Stroke(_bg.Graphics);

                   _ieRenderActions = null;
                   _bg.Graphics.DrawRectangle(new Pen(Brushes.Black, 10), _rectBitmap);
                   _bg.Render();

                   if (null != LocalOwner)
                       LocalOwner.Title = treeNode.Text;
               }
               catch (ArgumentException) { Util.Assert(99979, false); }
            });

            SelChildNode = null;
            _prevNode = null;
            _dtHideGoofball = DateTime.MinValue;

            if ((DateTime.Now - dtStart) > TimeSpan.FromSeconds(1))
            {
                treeNode.SelectedImageIndex = Math.Max((int)
                    (((treeNode.SelectedImageIndex < 0) ? _rectBitmap.Size.Width : treeNode.SelectedImageIndex)
                    * .75), 256);
            }
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

            var nodeDatum = TreeNode.NodeDatum;

            if (null == nodeDatum)      // added 2/13/15
            {
                Util.Assert(99963, false);
                return null;
            }

            return
                (nodeDatum.TotalLength > 0)
                ? new Recurse().Render(TreeNode, rc, DeepNode, out _deepNodeDrawn)
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

                if (0 < _nWorkerCount)
                    _blockingFrame.PushFrameTrue();

                deepNodeDrawn_out = _deepNodeDrawn;
                return _lsRenderActions.Concat(_lsFrames);
            }
            
            void RecurseDrawGraph(
                LocalTreeNode item,
                Rectangle rc,
                bool bStart = false)
            {
#if (DEBUG)
                Util.Assert(1302.3303m, rc.Width >= 0);
                Util.Assert(1302.3304m, rc.Height >= 0);
#endif
                var nodeDatum = item.NodeDatum;

                if (null == nodeDatum)      // added 2/13/15
                {
                    Util.Assert(99962, false);
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

                if ((item == _deepNode) ||
                    (_deepNode?.IsChildOf(item) ?? false))
                {
                    _deepNodeDrawn = item;
                }

                if (bStart &&
                    (null == nodeDatum.TreeMapFiles) &&
                    (false == item is LocalTreeMapFileNode))
                {
                    nodeDatum.TreeMapFiles = GetFileList(item);
                }

                if ((0 < (item.Nodes?.Count ?? 0)) ||
                    (bStart && (null != nodeDatum.TreeMapFiles)))
                {
                    IEnumerable<LocalTreeNode> ieChildren = null;
                    LocalTreeNode parent = null;
                    var bVolumeNode = false;

                    Util.Closure(() =>
                    {
                        var rootNodeDatum = item.NodeDatum.As<RootNodeDatum>();

                        if ((false == bStart) ||
                            (null == rootNodeDatum))
                        {
                            return;     // from lambda
                        }

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

                // could do array here but it's slightly less efficient because element sizes have to be shrunk
                var lsChildren =
                    ieChildren
                    .OrderByDescending(x => x.NodeDatum.TotalLength)
                    .ToList();

                var nCount = lsChildren.Count;

                if (0 == nCount)
                {
                    // any files are zero in length
                    return false;
                }

                Interlocked.Add(ref _nWorkerCount, nCount);

                var anChildWidth = // Widths of the children (fraction of row width).
                    new Double[nCount];

                var horizontalRows = (rc.Width >= rc.Height);
                var width_A = 1d;

                if (horizontalRows)
                {
                    if (rc.Height > 0)
                        width_A = rc.Width / (double)rc.Height;
                }
                else
                {
                    if (rc.Width > 0)
                        width_A = rc.Height / (double)rc.Width;
                }

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
                double top = horizontalRows ? rc.Top : rc.Left;
                var lastRow = rows[rows.Count - 1];

                rows.ForEach(row =>
                {
                    var fBottom = top + row.RowHeight * height;
                    var bottom = (int)fBottom;

                    if (ReferenceEquals(row, lastRow))
                        bottom = horizontalRows ? rc.Bottom : rc.Right;

                    double left = horizontalRows ? rc.Left : rc.Top;

                    for (var i = 0; i < row.ChildrenPerRow; i++, c++)
                    {
                        var child = lsChildren[c];
                        Util.Assert(1302.3305m, anChildWidth[c] >= 0);
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

                                if (0 == _nWorkerCount)
                                    _blockingFrame.Continue = false;
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
                var nCount = listChildren.Count;

                Util.Assert(1302.3308m, kdMinProportion < 1);

                Util.Assert(1302.3309m, nextChild < nCount);
                Util.Assert(1302.33101m, width >= 1d);

                var nodeDatum = parent.NodeDatum;

                if (null == nodeDatum)      // added 2/13/15
                {
                    Util.Assert(99961, false);
                    return 0;
                }

                var mySize = (double)nodeDatum.TotalLength;
                ulong sizeUsed = 0;
                double rowHeight = 0;
                var i = 0;

                for (i = nextChild; i < nCount; i++)
                {
                    var childSize = listChildren[i].NodeDatum.TotalLength;
                    sizeUsed += childSize;
                    var virtualRowHeight = sizeUsed / mySize;
                    Util.Assert(1302.3311m, virtualRowHeight > 0);
                    Util.Assert(1302.3312m, virtualRowHeight <= 1);

                    // Rectangle(mySize)    = width * 1d
                    // Rectangle(childSize) = childWidth * virtualRowHeight
                    // Rectangle(childSize) = childSize / mySize * width

                    double childWidth = childSize / mySize * width / virtualRowHeight;

                    if (childWidth / virtualRowHeight < kdMinProportion)
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
                for (i = 0; i < childrenUsed; i++)
                {
                    // Rectangle(1d * 1d) = mySize
                    var rowSize = mySize * rowHeight;
                    var nodeDatum_A = listChildren[nextChild + i].NodeDatum;

                    if (null == nodeDatum_A)      // added 2/13/15
                    {
                        Util.Assert(99960, false);
                        return 0;
                    }

                    var childSize = (double)nodeDatum_A.TotalLength;
                    var cw = childSize / rowSize;
                    Util.Assert(1302.3315m, cw >= 0);
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
            LocalDispatcherFrame
                _blockingFrame = new LocalDispatcherFrame(99850);
        }

        abstract class
            RenderAction { internal Rectangle rc; internal abstract void Stroke(Graphics g); }
        class
            FillRectangle : RenderAction { internal Brush Brush; internal override void Stroke(Graphics g) => g.FillRectangle(Brush, rc); }
        class
            DrawRectangle : RenderAction { static readonly Pen Pen = new Pen(Color.Black, 2); internal override void Stroke(Graphics g) => g.DrawRectangle(Pen, rc); }

        BufferedGraphics
            _bg = null;
        Rectangle
            _rectBitmap = Rectangle.Empty;
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
            _ieRenderActions = null;
        LocalTreeNode
            _deepNodeDrawn = null;

        // goofball
        Rectangle
            _rectCenter = Rectangle.Empty;
        DateTime
            _dtHideGoofball = DateTime.MinValue;

        LocalTreeNode
            _prevNode = null;

        readonly IList<IDisposable>
            _lsDisposable = new List<IDisposable>();
    }
}
