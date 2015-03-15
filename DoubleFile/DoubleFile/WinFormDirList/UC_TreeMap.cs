using System.Windows.Forms;
using System.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Collections.Concurrent;

namespace DoubleFile
{
    [System.ComponentModel.DesignerCategory("Code")]
    class UC_TreeMap : UserControl
    {
        internal bool ToolTipActive { get; private set; }
        internal Control TooltipAnchor = null;

        public UC_TreeMap()
        {
            _toolTip.UseFading = true;
            _toolTip.UseAnimation = true;
            _timerAnim = new LocalTimer(33.0, () =>   // 30 FPS
            {
                if (_rectCenter != Rectangle.Empty)
                {
                    ++_nAnimFrame;

                    if (0 == _nInvalidateRef)
                        Invalidate(_rectCenter);
                }
            }).Start();

            DoubleBuffered = true;
            SetStyle(ControlStyles.DoubleBuffer |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint,
                true);
            BackgroundImageLayout = ImageLayout.Stretch;
            Dock = DockStyle.Fill;
            BackColor = Color.Transparent;
            //MouseDown += form_tmapUserCtl_MouseDown;		NO.
            //MouseUp += form_tmapUserCtl_MouseUp;

            if (null == TooltipAnchor)
                TooltipAnchor = this;
        }

        void InvalidatePushRef(Action action)
        {
            ++_nInvalidateRef;
            action();
            --_nInvalidateRef;

            if (0 == _nInvalidateRef)
                Invalidate();
        }

        void form_tmapUserCtl_MouseDown(object sender, MouseEventArgs e)
        {
            MBoxStatic.Assert(99895, false);
        }

        void form_tmapUserCtl_MouseUp(object sender, MouseEventArgs e)
        {
            MBoxStatic.Assert(99894, false);
        }
        internal void Clear()
        {
            _treeNode = null;
            _prevNode = null;
            _deepNode = null;
            _deepNodeDrawn = null;
            _toolTip.Tag = null;
            UtilProject.WriteLine(DateTime.Now + " Clear();");
            ClearSelection();
        }

        internal void ClearSelection(bool bKeepTooltipActive = false)
        {
            if (App.LocalExit)
                return;

            var ctl = TooltipAnchor;

            if ((null == ctl) || ctl.IsDisposed)
                ctl = this;

            if (ctl.IsDisposed)
                return;

            UtilProject.UIthread(() => _toolTip.Hide(ctl));

            if (false == bKeepTooltipActive)
            {
                ToolTipActive = false;
                UtilProject.WriteLine(DateTime.Now + " b ToolTipActive = false;");
            }

            InvalidatePushRef(() => _selRect = Rectangle.Empty);
        }

        protected override void Dispose(bool disposing)
        {
            if (_bg != null)
            {
                _bg.Dispose();
                _bg = null;
            }

            _toolTip.Dispose();
            _timerAnim.Dispose();
            ToolTipActive = false; UtilProject.WriteLine(DateTime.Now + " c ToolTipActive = false;");
            base.Dispose(disposing);
        }

        internal TreeNode DoToolTip(Point pt_in)
        {
            UtilProject.WriteLine(DateTime.Now + " DoToolTip();");
            ClearSelection();

            if (_treeNode == null)
            {
                return null;
            }

            if (_rectCenter.Contains(pt_in))   // click once to hide goofball. Click again within 5 seconds to return to the deep node.
            {
                if (_dtHideGoofball == DateTime.MinValue)
                {
                    _dtHideGoofball = DateTime.Now;
                    return null;
                }
                else if (DateTime.Now - _dtHideGoofball < TimeSpan.FromSeconds(5))
                {
                    _dtHideGoofball = DateTime.MinValue;
                    return _deepNode;
                }
            }

            _dtHideGoofball = DateTime.MinValue;   // click anywhere else on the treemap and the goofball returns.

            var pt = Point.Ceiling(new PointF(pt_in.X / _sizeTranslate.Width, pt_in.Y / _sizeTranslate.Height));
            TreeNode nodeRet = null;
            var bImmediateFiles = false;
            var bVolumeView = false;

            UtilDirList.Closure(() =>
            {
                {
                    var nodeDatum = _treeNode.Tag as NodeDatum;

                    if (null == nodeDatum)      // added 2/13/15 as safety
                    {
                        MBoxStatic.Assert(99979, false);
                        return;
                    }

                    {
                        var rootNodeDatum = nodeDatum as RootNodeDatum;

                        bVolumeView = ((null != rootNodeDatum) && rootNodeDatum.VolumeView);
                    }

                    if ((false == bVolumeView) &&
                        (null != (nodeRet = FindMapNode(nodeDatum.TreeMapFiles, pt))))
                    {
                        bImmediateFiles = true;
                        return;
                    }
                }

                var m_prevNode_A = _prevNode ?? _treeNode;

                if (null != (nodeRet = FindMapNode(m_prevNode_A, pt)))
                {
                    return;
                }

                var nodeUplevel = 
                    (_prevNode != null)
                    ? _prevNode.Parent
                    : null;

                while (nodeUplevel != null)
                {
                    if ((nodeRet = FindMapNode(nodeUplevel, pt)) != null)
                    {
                        return;
                    }

                    nodeUplevel = nodeUplevel.Parent;
                }

                if ((nodeRet = FindMapNode(_treeNode, pt)) != null)
                {
                    return;
                }

                nodeRet = _treeNode;
            });

            if ((false == bVolumeView) &&
                (false == bImmediateFiles))
            {
                var nodeDatum = nodeRet.Tag as NodeDatum;

                if (null == nodeDatum)      // added 2/13/15 as safety
                {
                    MBoxStatic.Assert(99890, false);
                    return null;
                }

                var nodeRet_A = FindMapNode(nodeDatum.TreeMapFiles, pt);

                if ((null != nodeRet_A) &&
                    (nodeRet == _treeNode))
                {
                    nodeRet = nodeRet_A;
                    bImmediateFiles = true;
                }
            }

            if (nodeRet == _prevNode)
            {
                nodeRet = _treeNode;
                bImmediateFiles = false;
            }

            _toolTip.ToolTipTitle = nodeRet.Text;

            if (bImmediateFiles)
            {
                _toolTip.ToolTipTitle += " (immediate files)";
            }

            _toolTip.Tag = nodeRet;

            {
                var nodeDatum = nodeRet.Tag as NodeDatum;

                _selRect = nodeDatum.TreeMapRect;
                _toolTip.Show(UtilDirList.FormatSize(nodeDatum.TotalLength, bBytes: true),
                    TooltipAnchor,
                    new Point(0, 0));
                ToolTipActive = true; UtilProject.WriteLine(DateTime.Now + " a ToolTipActive = true; ------");
            }

            _prevNode = nodeRet;

            if (0 == _nInvalidateRef)   // jic
                Invalidate();

            return null;
        }

        static TreeNode FindMapNode(TreeNode treeNode_in, Point pt, bool bNextNode = false)
        {
            var treeNode = treeNode_in;

            if (treeNode == null)
            {
                return null;
            }

            do
            {
                var nodeDatum = treeNode.Tag as NodeDatum;

                if (null == nodeDatum)      // added 2/13/15 as safety
                {
                    MBoxStatic.Assert(99889, false);
                    return null;
                }

                if (false == nodeDatum.TreeMapRect.Contains(pt))
                {
                    continue;
                }

                if (bNextNode ||
                    (treeNode != treeNode_in))
                {
                    return treeNode;
                }

                if ((treeNode.Nodes == null) ||
                    (treeNode.Nodes.IsEmpty()))
                {
                    continue;
                }

                var foundNode = FindMapNode(treeNode.Nodes[0], pt, bNextNode: true);

                if (foundNode != null)
                {
                    return foundNode;
                }
            }
            while (bNextNode &&
                (null != (treeNode = treeNode.NextNode)));

            return null;
        }

        static TreeNode GetFileList(TreeNode parent)
        {
            var listLengths = new List<ulong>();
            var listFiles = TreeSelect.GetFileList(parent, listLengths);

            if (listFiles == null)
            {
                return null;
            }

            var nodeFileList = new TreeNode(parent.Text);
            ulong nTotalLength = 0;
            var enumerator = listLengths.GetEnumerator();

            foreach (var arrLine in listFiles)
            {
                var nodeDatum_A = new NodeDatum();
                var bMoveNext = enumerator.MoveNext();

                MBoxStatic.Assert(99888, bMoveNext);
                nTotalLength += nodeDatum_A.TotalLength = enumerator.Current;

                if (enumerator.Current == 0)
                    continue;

                nodeFileList.Nodes.Add(new TreeNode(arrLine[0])
                {
                    Tag = nodeDatum_A,
                    ForeColor = Color.OliveDrab
                });
            }

            if (nTotalLength == 0)
            {
                return null;
            }

            var nodeDatum = parent.Tag as NodeDatum;
            var nodeDatum_B = new NodeDatum();

            MBoxStatic.Assert(99887, nTotalLength == nodeDatum.Length);
            nodeDatum_B.TotalLength = nTotalLength;
            nodeDatum_B.TreeMapRect = nodeDatum.TreeMapRect;
            nodeFileList.Tag = nodeDatum_B;
            MBoxStatic.Assert(99886, nodeFileList.SelectedImageIndex == -1);              // sets the bitmap size
            nodeFileList.SelectedImageIndex = -1;
            return nodeFileList;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_selRect != Rectangle.Empty)
            {
                e.Graphics.FillRectangle(
                    new SolidBrush(Color.FromArgb(64, 0, 0, 0)),
                    _selRect.Scale(_sizeTranslate));
            }

            if ((null == _deepNodeDrawn) ||
                (_deepNodeDrawn == _treeNode))
            {
                _rectCenter = Rectangle.Empty;
                return;
            }

            if (_dtHideGoofball != DateTime.MinValue)
            {
                return;
            }

            var nodeDatum = _deepNodeDrawn.Tag as NodeDatum;

            if (null == nodeDatum)      // added 2/13/15 as safety
            {
                MBoxStatic.Assert(99885, false);
                return;
            }

            RectangleF r = (nodeDatum.TreeMapRect)
                .Scale(_sizeTranslate);

            r.Inflate(-r.Width / 2 + 15, -r.Height / 2 + 15);
            _rectCenter = Rectangle.Ceiling(r);

            var path = new GraphicsPath();

            path.AddEllipse(_rectCenter);

            var brush = new PathGradientBrush(path)
            {
                CenterColor = Color.White,
                SurroundColors = new Color[] {Color.FromArgb(0, 0, 0, 0)}
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
            UtilProject.WriteLine(DateTime.Now + " OnSizeChanged();");
            ClearSelection();
        }

        internal void Render(TreeNode treeNode)
        {
            if ((null == _deepNode) ||
                (false == _deepNode.IsChildOf(treeNode)))
            {
                _deepNode = treeNode;
            }

            var nPxPerSide = (treeNode.SelectedImageIndex < 0)
                ? 1024
                : treeNode.SelectedImageIndex;

            if (nPxPerSide != _rectBitmap.Size.Width)
            {
                var dtStart_A = DateTime.Now;

                _rectBitmap = new Rectangle(0, 0, nPxPerSide, nPxPerSide);
                BackgroundImage = new Bitmap(_rectBitmap.Size.Width, _rectBitmap.Size.Height);

                var bgcontext = BufferedGraphicsManager.Current;

                bgcontext.MaximumBuffer = _rectBitmap.Size;

                if (_bg != null)
                    _bg.Dispose();

                _bg = bgcontext.Allocate(Graphics.FromImage(BackgroundImage), _rectBitmap);
                TranslateSize();
                UtilProject.WriteLine("Size bitmap " + nPxPerSide + " " + (DateTime.Now - dtStart_A).TotalMilliseconds / 1000.0 + " seconds.");
            }

            var dtStart = DateTime.Now;

            ClearSelection();
            _treeNode = treeNode;
            DrawTreemap();              // populates _lsRenderActions

            UtilProject.UIthread(() =>
            {
                _bg.Graphics.Clear(Color.DarkGray);

                var lsFrames = new List<RenderAction>();

                foreach (var stroke in _lsRenderActions)
                {
                    if (stroke is DrawRectangle)
                        lsFrames.Add(stroke);
                    else
                        stroke.Stroke(_bg.Graphics);
                }

                foreach (var stroke in lsFrames)
                    stroke.Stroke(_bg.Graphics);

                _lsRenderActions = null;
                _bg.Graphics.DrawRectangle(new Pen(Brushes.Black, 10), _rectBitmap);
                _bg.Render();
            });

            _selRect = Rectangle.Empty;
            _prevNode = null;
            _dtHideGoofball = DateTime.MinValue;

            if ((DateTime.Now - dtStart) > TimeSpan.FromSeconds(1))
            {
                treeNode.SelectedImageIndex = Math.Max((int)
                    (((treeNode.SelectedImageIndex < 0) ? _rectBitmap.Size.Width : treeNode.SelectedImageIndex)
                    * .75), 256);
            }
        }

        internal string Tooltip_Click()
        {
            if (null == _toolTip.Tag)
            {
                return null;
            }

            var treeNode_A = (_toolTip.Tag as TreeNode);

            if (null == treeNode_A)      // added 2/13/15 as safety
            {
                MBoxStatic.Assert(99884, false);
                return null;
            }

            if (treeNode_A.TreeView != null)    // null if fake file treenode (NodeDatum.TreeMapFiles)
            {
                var rootNodeDatum = treeNode_A.Tag as RootNodeDatum;

                if (rootNodeDatum != null)
                {
                    rootNodeDatum.VolumeView = (rootNodeDatum.VolumeView == false);
                    treeNode_A.TreeView.SelectedNode = null;    // to kick in a change selection event
                }

                treeNode_A.TreeView.SelectedNode = treeNode_A;
            }
            else
            {
                return treeNode_A.Text;
            }

            return null;
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
        
        internal void DrawTreemap()
        {
            _deepNodeDrawn = null;
            var rc = _rectBitmap;

	        rc.Width--;
	        rc.Height--;

	        if (rc.Width <= 0 || rc.Height <= 0)
            {
		        return;
            }

            var nodeDatum = _treeNode.Tag as NodeDatum;

            if (null == nodeDatum)      // added 2/13/15 as safety
            {
                MBoxStatic.Assert(99883, false);
                return;
            }

            if (nodeDatum.TotalLength > 0)
                _lsRenderActions = new Recurse().Render(_treeNode, rc, _deepNode, out _deepNodeDrawn);
            else
                _lsRenderActions = new ConcurrentBag<RenderAction> { new FillRectangle() { Brush = Brushes.Wheat, rc = rc } };
        }

        class Recurse
        {
            internal ConcurrentBag<RenderAction> Render(
                TreeNode item,
                Rectangle rc,
                TreeNode deepNode,
                out TreeNode deepNodeDrawn_out)
            {
                _lsRenderActions = new ConcurrentBag<RenderAction>();
                _deepNode = deepNode;
                RecurseDrawGraph(item, rc, true);

                while (_nWorkerCount > 0)
                    Thread.Sleep(20);

                deepNodeDrawn_out = _deepNodeDrawn;
                return _lsRenderActions;
            }
            
            void RecurseDrawGraph(
                TreeNode item,
                Rectangle rc,
                bool bStart = false)
            {
#if (DEBUG)
                MBoxStatic.Assert(1302.33035, rc.Width >= 0);
                MBoxStatic.Assert(1302.33045, rc.Height >= 0);
#endif
                var nodeDatum = item.Tag as NodeDatum;

                if (null == nodeDatum)      // added 2/13/15 as safety
                {
                    MBoxStatic.Assert(99962.5, false);
                    return;
                }

                nodeDatum.TreeMapRect = rc;

                if (rc.Width < 1 ||
                    rc.Height < 1)
                {
                    return;
                }

                if (rc.Width < 4 ||
                    rc.Height < 4)
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
                    (null != item.TreeView))
                {
                    nodeDatum.TreeMapFiles = GetFileList(item);
                }

                if ((false == item.Nodes.IsEmpty()) ||
                    (bStart && (null != nodeDatum.TreeMapFiles)))
                {
                    List<TreeNode> listChildren = null;
                    TreeNode parent = null;
                    var bVolumeNode = false;

                    UtilDirList.Closure(() =>
                    {
                        var rootNodeDatum = item.Tag as RootNodeDatum;

                        if ((false == bStart) ||
                            (null == rootNodeDatum))
                        {
                            return;     // from lambda
                        }

                        if (false == rootNodeDatum.VolumeView)
                        {
                            return;     // from lambda
                        }

                        var nodeDatumFree = new NodeDatum();
                        var nodeFree = new TreeNode(item.Text + " (free space)");

                        nodeDatumFree.TotalLength = rootNodeDatum.VolumeFree;
                        nodeFree.Tag = nodeDatumFree;
                        nodeFree.ForeColor = Color.MediumSpringGreen;

                        var nodeDatumUnread = new NodeDatum();
                        var nodeUnread = new TreeNode(item.Text + " (unread data)");
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

                        nodeUnread.Tag = nodeDatumUnread;
                        nodeUnread.ForeColor = Color.MediumVioletRed;

                        // parent added as child, with two other nodes:
                        // free space (color: spring green); and...
                        listChildren = new List<TreeNode> { item, nodeFree };

                        if (nUnreadLength > 0)
                        {
                            // ...unread guess, affected by compression and hard links (violet)
                            listChildren.Add(nodeUnread);
                        }

                        parent = new TreeNode(item.Text + " (volume)");

                        var nodeDatumVolume = new NodeDatum
                        {
                            TotalLength = nVolumeLength,
                            TreeMapRect = rootNodeDatum.TreeMapRect
                        };

                        parent.Tag = nodeDatumVolume;
                        bVolumeNode = true;
                    });

                    if (bVolumeNode == false)
                    {
                        parent = item;

                        listChildren =
                            item.Nodes
                            .Cast<TreeNode>()
                            .Where(t => 0 < (t.Tag as NodeDatum).TotalLength)
                            .ToList();
                    }

                    // returns true if there are children
                    if (KDirStat_DrawChildren(parent, listChildren, bStart))
                    {
                        // example scenario: empty folder when there are immediate files and bStart is not true
                        return;
                    }
                }

                // There are no children. Draw a file or an empty folder.
                DrawNode(item, rc);
            }

            void DrawNode(TreeNode item, Rectangle rc)
            {
                var path = new GraphicsPath();
                var r = rc;

                r.Inflate(r.Width / 2, r.Height / 2);
                path.AddEllipse(r);

                var brush = new PathGradientBrush(path)
                {
                    CenterColor = Color.Wheat,
                    SurroundColors = new[] { ControlPaint.Dark(
                        (item.ForeColor == Color.Empty)
                        ? Color.SandyBrown
                        : item.ForeColor
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

            bool KDirStat_DrawChildren(TreeNode parent, List<TreeNode> listChildren, bool bStart)
            {
                var nodeDatum = parent.Tag as NodeDatum;
                var rc = nodeDatum.TreeMapRect;
                var rows = new List<RowStruct>();

                if (bStart &&
                    (null != nodeDatum.TreeMapFiles))
                {
                    listChildren.Add(nodeDatum.TreeMapFiles);
                }
                else if (nodeDatum.Length > 0)
                {
                    listChildren.Add(new TreeNode(parent.Text)
                    {
                        Tag = new NodeDatum { TotalLength = nodeDatum.Length },
                        ForeColor = Color.OliveDrab
                    });
                }

                listChildren.Sort((y, x) => (x.Tag as NodeDatum).TotalLength.CompareTo((y.Tag as NodeDatum).TotalLength));

                if (listChildren.IsEmpty())
                {
                    // any files are zero in length
                    return false;
                }

                Interlocked.Add(ref _nWorkerCount, listChildren.Count);

                var anChildWidth = // Widths of the children (fraction of row width).
                    new Double[listChildren.Count];

                var horizontalRows = (rc.Width >= rc.Height);
                var width_A = 1.0;
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

                    for (var nextChild = 0; nextChild < listChildren.Count; nextChild += childrenUsed)
                    {
                        rows.Add(new RowStruct
                        {
                            RowHeight = KDirStat_CalculateNextRow(parent, nextChild, width_A, out childrenUsed, anChildWidth, listChildren),
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

                    if (object.ReferenceEquals(row, rows[rows.Count - 1]))
                        bottom = horizontalRows ? rc.Bottom : rc.Right;

                    double left = horizontalRows ? rc.Left : rc.Top;

                    for (var i = 0; i < row.ChildrenPerRow; i++, c++)
                    {
                        var child = listChildren[c];
                        MBoxStatic.Assert(1302.33055, anChildWidth[c] >= 0);
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
                            new WaitCallback(state =>
                            {
                                var param = (Tuple<TreeNode, Rectangle>)state;

                                RecurseDrawGraph(param.Item1, param.Item2);
                                Interlocked.Decrement(ref _nWorkerCount);
                            }),
                            new Tuple<TreeNode, Rectangle>(child, rcChild)
                        );

                        if (bStart)
                            _lsRenderActions.Add(new DrawRectangle { rc = rcChild });

                        if (lastChild)
                        {
                            i++;
                            c++;

                            if (i < row.ChildrenPerRow)
                                (listChildren[c].Tag as NodeDatum).TreeMapRect = new Rectangle(-1, -1, -1, -1);

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

            static double KDirStat_CalculateNextRow(TreeNode parent, int nextChild, double width,
                out int childrenUsed, double[] anChildWidth,
                IReadOnlyList<TreeNode> listChildren)
            {
                childrenUsed = 0;
                const double kdMinProportion = 0.4;
                MBoxStatic.Assert(1302.33085, kdMinProportion < 1);

                MBoxStatic.Assert(1302.33095, nextChild < listChildren.Count);
                MBoxStatic.Assert(1302.331015, width >= 1.0);

                var nodeDatum = parent.Tag as NodeDatum;

                if (null == nodeDatum)      // added 2/13/15 as safety
                {
                    MBoxStatic.Assert(99961.5, false);
                    return 0;
                }

                var mySize = (double)nodeDatum.TotalLength;
                ulong sizeUsed = 0;
                double rowHeight = 0;
                var i = 0;

                for (i = nextChild; i < listChildren.Count; i++)
                {
                    var childSize = (listChildren[i].Tag as NodeDatum).TotalLength;
                    sizeUsed += childSize;
                    var virtualRowHeight = sizeUsed / mySize;
                    MBoxStatic.Assert(1302.33115, virtualRowHeight > 0);
                    MBoxStatic.Assert(1302.33125, virtualRowHeight <= 1);

                    // Rectangle(mySize)    = width * 1.0
                    // Rectangle(childSize) = childWidth * virtualRowHeight
                    // Rectangle(childSize) = childSize / mySize * width

                    double childWidth = childSize / mySize * width / virtualRowHeight;

                    if (childWidth / virtualRowHeight < kdMinProportion)
                    {
                        MBoxStatic.Assert(1302.33135, i > nextChild); // because width >= 1 and _minProportion < 1.
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

                MBoxStatic.Assert(1302.33145, i > nextChild);

                // Now i-1 is the last child used
                // and rowHeight is the height of the row.

                childrenUsed = i - nextChild;

                // Now as we know the rowHeight, we compute the widths of our children.
                for (i = 0; i < childrenUsed; i++)
                {
                    // Rectangle(1.0 * 1.0) = mySize
                    var rowSize = mySize * rowHeight;
                    var nodeDatum_A = listChildren[nextChild + i].Tag as NodeDatum;

                    if (null == nodeDatum_A)      // added 2/13/15 as safety
                    {
                        MBoxStatic.Assert(99960.5, false);
                        return 0;
                    }

                    var childSize = (double)nodeDatum_A.TotalLength;
                    var cw = childSize / rowSize;
                    MBoxStatic.Assert(1302.33155, cw >= 0);
                    anChildWidth[nextChild + i] = cw;
                }

                return rowHeight;
            }

            ConcurrentBag<RenderAction>
                _lsRenderActions = null;
            TreeNode
                _deepNode = null;
            TreeNode
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
            DrawRectangle : RenderAction { static Pen Pen = new Pen(Color.Black, 2); internal override void Stroke(Graphics g) { g.DrawRectangle(Pen, rc); } }

        ConcurrentBag<RenderAction>
            _lsRenderActions = null;
        TreeNode
            _deepNode = null;
        TreeNode
            _deepNodeDrawn = null;

        Rectangle
            _rectBitmap = Rectangle.Empty;
        Rectangle
            _selRect = Rectangle.Empty;
        Rectangle
            _rectCenter = Rectangle.Empty;
        SizeF
            _sizeTranslate = SizeF.Empty;
        BufferedGraphics
            _bg = null;
        TreeNode
            _treeNode = null;
        TreeNode
            _prevNode = null;
        readonly LocalTimer
            _timerAnim = null;
        int
            _nAnimFrame = 0;
        DateTime
            _dtHideGoofball = DateTime.MinValue;
        readonly ToolTip
            _toolTip = new ToolTip();
        int
            _nInvalidateRef = 0;
    }
}
