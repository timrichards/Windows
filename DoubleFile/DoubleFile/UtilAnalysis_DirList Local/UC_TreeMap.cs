using System.Windows.Forms;
using System.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing.Drawing2D;
using DoubleFile;

namespace Local
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
            _timerAnim = new SDL_Timer(33.0, () =>   // 30 FPS
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
            Local.TreeSelect.FolderDetailUpdated += TreeSelect_FolderDetailUpdated;
            MouseDown += form_tmapUserCtl_MouseDown;
            MouseUp += form_tmapUserCtl_MouseUp;

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
            _bMouseDown = true;
        }

        void form_tmapUserCtl_MouseUp(object sender, MouseEventArgs e)
        {
            if (_bMouseDown == false)
            {
                return;
            }

            _bMouseDown = false;

            var treeNode = DoToolTip(e.Location);

            if (treeNode == null)
            {
                return;
            }

            //gd.m_bPutPathInFindEditBox = true;
            //gd.m_bTreeViewIndirectSelChange = true;
            //treeNode.TreeView.SelectedNode = treeNode;
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
            InvalidatePushRef(() =>
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

                _selRect = Rectangle.Empty;
            });
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
            Local.TreeSelect.FolderDetailUpdated -= TreeSelect_FolderDetailUpdated;
            base.Dispose(disposing);
        }

        internal LocalTreeNode DoToolTip(Point pt_in)
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
            LocalTreeNode nodeRet = null;
            var bImmediateFiles = false;
            var bVolumeView = false;

            UtilAnalysis_DirList.Closure(() =>
            {
                {
                    var nodeDatum = _treeNode.NodeDatum;

                    if (null == nodeDatum)      // this check is new 2/13/15 and has never been hit
                    {
                        MBoxStatic.Assert(99967, false);
                        return;
                    }

                    {
                        var rootNodeDatum = nodeDatum as RootNodeDatum;

                        bVolumeView = ((rootNodeDatum != null) && rootNodeDatum.VolumeView);
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
                var nodeDatum = nodeRet.NodeDatum;

                if (null == nodeDatum)      // this check is new 2/13/15 and has never been hit
                {
                    MBoxStatic.Assert(99923, false);
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
                var nodeDatum = nodeRet.NodeDatum;

                _selRect = nodeDatum.TreeMapRect;
                _toolTip.Show(UtilAnalysis_DirList.FormatSize(nodeDatum.TotalLength, bBytes: true),
                    TooltipAnchor,
                    new Point(0, 0));
                ToolTipActive = true; UtilProject.WriteLine(DateTime.Now + " a ToolTipActive = true; ------");
            }

            _prevNode = nodeRet;

            if (0 == _nInvalidateRef)   // jic
                Invalidate();

            return null;
        }

        static LocalTreeNode FindMapNode(LocalTreeNode treeNode_in, Point pt, bool bNextNode = false)
        {
            var treeNode = treeNode_in;

            if (treeNode == null)
            {
                return null;
            }

            do
            {
                var nodeDatum = treeNode.NodeDatum;

                if (null == nodeDatum)      // this check is new 2/13/15 and has never been hit
                {
                    MBoxStatic.Assert(99966, false);
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

        static LocalTreeNode GetFileList(LocalTreeNode parent)
        {
            var listLengths = new List<ulong>();
            var listFiles = TreeSelect.GetFileList(parent, listLengths);

            if (listFiles == null)
            {
                return null;
            }

            var nodeFileList = new LocalTreeMapNode(parent.Text);
            ulong nTotalLength = 0;
            var iterUlong = listLengths.GetEnumerator();

            foreach (var arrLine in listFiles)
            {
                MBoxStatic.Assert(1302.3316, iterUlong.MoveNext());
                var nodeDatum_A = new NodeDatum();

                nTotalLength += nodeDatum_A.TotalLength = iterUlong.Current;

                if (iterUlong.Current == 0)
                {
                    continue;
                }

                nodeFileList.Nodes.Add(new LocalTreeMapNode(arrLine[0])
                {
                    NodeDatum = nodeDatum_A,
                    ForeColor = UtilColor.OliveDrab
                });
            }

            if (nTotalLength == 0)
            {
                return null;
            }

            var nodeDatum = parent.NodeDatum;
            var nodeDatum_B = new NodeDatum();

            MBoxStatic.Assert(1302.3301, nTotalLength == nodeDatum.Length);
            nodeDatum_B.TotalLength = nTotalLength;
            nodeDatum_B.TreeMapRect = nodeDatum.TreeMapRect;
            nodeFileList.NodeDatum = nodeDatum_B;
            MBoxStatic.Assert(1302.3302, nodeFileList.SelectedImageIndex == -1);              // sets the bitmap size
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

            var nodeDatum = _deepNodeDrawn.NodeDatum;

            if (null == nodeDatum)      // this check is new 2/13/15 and has never been hit
            {
                MBoxStatic.Assert(99965, false);
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

        void TreeSelect_FolderDetailUpdated(IEnumerable<string[]> lasDetail, LocalTreeNode treeNode)
        {
            InvalidatePushRef(() => Render(treeNode));
        }

        void Render(LocalTreeNode treeNode)
        {
            if ((null == _deepNode) ||
                (false == _deepNode.IsChildOf(treeNode)))
            {
                _deepNode = treeNode;
            }

            var nPxPerSide = (treeNode.SelectedImageIndex < 0) ?
                1024 :
                treeNode.SelectedImageIndex;

            if (nPxPerSide != _rectBitmap.Size.Width)
            {
                var dtStart_A = DateTime.Now;

                _rectBitmap = new Rectangle(0, 0, nPxPerSide, nPxPerSide);
                BackgroundImage = new Bitmap(_rectBitmap.Size.Width, _rectBitmap.Size.Height);

                var bgcontext = BufferedGraphicsManager.Current;

                bgcontext.MaximumBuffer = _rectBitmap.Size;

                if (_bg != null)
                {
                    _bg.Dispose();
                }

                _bg = bgcontext.Allocate(Graphics.FromImage(BackgroundImage), _rectBitmap);
                TranslateSize();
                UtilProject.WriteLine("Size bitmap " + nPxPerSide + " " + (DateTime.Now - dtStart_A).TotalMilliseconds / 1000.0 + " seconds.");
            }

            var dtStart = DateTime.Now;

            ClearSelection();
            _treeNode = treeNode;
            _lsRenderActions.Clear();
            DrawTreemap();

            UtilProject.UIthread(() =>
            {
                _bg.Graphics.Clear(Color.DarkGray);

                foreach (var stroke in _lsRenderActions)
                    stroke.Stroke(_bg.Graphics);

                _lsRenderActions.Clear();
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

            var treeNode_A = (_toolTip.Tag as LocalTreeNode);

            if (null == treeNode_A)      // this check is new 2/13/15 and has never been hit
            {
                MBoxStatic.Assert(99964, false);
                return null;
            }

            if (treeNode_A.TreeView != null)    // null if fake file treenode (NodeDatum.TreeMapFiles)
            {
                var rootNodeDatum = treeNode_A.NodeDatum as RootNodeDatum;

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

            var nodeDatum = _treeNode.NodeDatum;

            if (null == nodeDatum)      // this check is new 2/13/15 and has never been hit
            {
                MBoxStatic.Assert(99963, false);
                return;
            }

            if (nodeDatum.TotalLength > 0)
                RecurseDrawGraph(_treeNode, rc, bStart: true);
            else
                _lsRenderActions.Add(new FillRectangle() { Brush = Brushes.Wheat, rc = rc });
        }

        void RecurseDrawGraph(
	        LocalTreeNode item, 
	        Rectangle rc,
            bool bStart = false
        )
        {
            MBoxStatic.Assert(1302.3303, rc.Width >= 0);
            MBoxStatic.Assert(1302.3304, rc.Height >= 0);

	        if (rc.Width <= 0 || rc.Height <= 0)
	        {
		        return;
	        }

            if ((null != _deepNode) &&
                ((item == _deepNode) || (_deepNode.IsChildOf(item))))
            {
                _deepNodeDrawn = item;
            }

            var nodeDatum = item.NodeDatum;

            if (null == nodeDatum)      // this check is new 2/13/15 and has never been hit
            {
                MBoxStatic.Assert(99962, false);
                return;
            }

            nodeDatum.TreeMapRect = rc;

            if (bStart &&
                (null == nodeDatum.TreeMapFiles) &&
                (null != item.TreeView))
            {
                nodeDatum.TreeMapFiles = GetFileList(item);
            }

            if (((false == item.Nodes.IsEmpty()) || (bStart && (null != nodeDatum.TreeMapFiles))) &&
                KDirStat_DrawChildren(item, bStart))
            {
                // example scenario: empty folder when there are immediate files and bStart is not true
                return;
            }

            var path = new GraphicsPath();
            var r = rc;

            r.Inflate(r.Width / 2, r.Height / 2);
            path.AddEllipse(r);

            var brush = new PathGradientBrush(path)
            {
                CenterColor = Color.Wheat,
                SurroundColors = new[] { ControlPaint.Dark((item.ForeColor == UtilColor.Empty) ?
                    Color.SandyBrown :
                    Color.FromArgb(item.ForeColor)
                )}
            };

            _lsRenderActions.Add(new FillRectangle() { Brush = brush, rc = rc });
        }

         //My first approach was to make this member pure virtual and have three
         //classes derived from CTreemap. The disadvantage is then, that we cannot
         //simply have a member variable of type CTreemap but have to deal with
         //pointers, factory methods and explicit destruction. It's not worth.

         //I learned this squarification style from the KDirStat executable.
         //It's the most complex one here but also the clearest, imho.
        
        bool KDirStat_DrawChildren(LocalTreeNode parent_in, bool bStart = false)
        {
            List<LocalTreeNode> listChildren = null;
            LocalTreeNode parent = null;

            var bVolumeNode = false;

            UtilAnalysis_DirList.Closure(() =>
            {
                var rootNodeDatum = parent_in.NodeDatum as RootNodeDatum;

                if ((false == bStart) ||
                    (null == rootNodeDatum))
                {
                    return;
                }

                if (rootNodeDatum.VolumeView == false)
                {
                    return;
                }

                var nodeDatumFree = new NodeDatum();
                var nodeFree = new LocalTreeMapNode(parent_in.Text + " (free space)");

                nodeDatumFree.TotalLength = rootNodeDatum.VolumeFree;
                nodeFree.NodeDatum = nodeDatumFree;
                nodeFree.ForeColor = UtilColor.MediumSpringGreen;

                var nodeDatumUnread = new NodeDatum();
                var nodeUnread = new LocalTreeMapNode(parent_in.Text + " (unread data)");
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

                nodeUnread.NodeDatum = nodeDatumUnread;
                nodeUnread.ForeColor = UtilColor.MediumVioletRed;

                // parent added as child, with two other nodes:
                // free space (color: spring green); and...
                listChildren = new List<LocalTreeNode> { parent_in, nodeFree };

                if (nUnreadLength > 0)
                {
                    // ...unread guess, affected by compression and hard links (violet)
                    listChildren.Add(nodeUnread);
                }

                parent = new LocalTreeMapNode(parent_in.Text + " (volume)");

                var nodeDatumVolume = new NodeDatum
                {
                    TotalLength = nVolumeLength,
                    TreeMapRect = rootNodeDatum.TreeMapRect
                };

                parent.NodeDatum = nodeDatumVolume;
                bVolumeNode = true;
            });

            if (bVolumeNode == false)
            {
                parent = parent_in;
                listChildren =
                    parent
                    .Nodes
                    .Cast<LocalTreeNode>()
                    .Where(t => 0 < t.NodeDatum.TotalLength)
                    .ToList();
            }

            var nodeDatum = parent.NodeDatum;
            var rc = nodeDatum.TreeMapRect;
	        var rows = new List<double>();	// Our rectangle is divided into rows, each of which gets this height (fraction of total height).
	        var childrenPerRow = new List<int>();// childrenPerRow[i] = # of children in rows[i]

            if (bStart &&
                (null != nodeDatum.TreeMapFiles))
            {
                listChildren.Add(nodeDatum.TreeMapFiles);
            }
            else if (nodeDatum.Length > 0)
            {
                listChildren.Add(new LocalTreeMapNode(parent.Text)
                {
                    NodeDatum = new NodeDatum { TotalLength = nodeDatum.Length },
                    ForeColor = UtilColor.OliveDrab
                });
            }

            listChildren.Sort((y, x) => 
                x.NodeDatum.TotalLength.CompareTo(
                y.NodeDatum.TotalLength));

            if (listChildren.IsEmpty())
            {
                // any files are zero in length
                return false;
            }

            var childWidth = // Widths of the children (fraction of row width).
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

            var nextChild = 0;
            while (nextChild < listChildren.Count)
            {
                var childrenUsed = 0;

                rows.Add(KDirStat_CalcutateNextRow(parent, nextChild, width_A, ref childrenUsed, childWidth, listChildren));
                childrenPerRow.Add(childrenUsed);
                nextChild += childrenUsed;
            }

	        var width= horizontalRows ? rc.Width : rc.Height;
	        var height= horizontalRows ? rc.Height : rc.Width;

	        var c = 0;
	        double top= horizontalRows ? rc.Top : rc.Left;
	        for (var row=0; row < rows.Count; row++)
	        {
		        var fBottom= top + rows[row] * height;
		        var bottom= (int)fBottom;
		        if (row == rows.Count - 1)
			        bottom= horizontalRows ? rc.Bottom : rc.Right;
		        double left= horizontalRows ? rc.Left : rc.Top;

                for (var i=0; i < childrenPerRow[row]; i++, c++)
		        {
                    var child = listChildren[c];
                    MBoxStatic.Assert(1302.3305, childWidth[c] >= 0);
			        var fRight= left + childWidth[c] * width;
			        var right= (int)fRight;

			        var lastChild = (
                        (i == childrenPerRow[row] - 1) ||
                        childWidth[c + 1].Equals(0));

			        if (lastChild)
				        right= horizontalRows ? rc.Right : rc.Bottom;

			        var rcChild = 
			            (horizontalRows)
                        ? new Rectangle((int)left, (int)top, right-(int)left, bottom-(int)top)
                        : new Rectangle((int)top, (int)left, bottom-(int)top, right-(int)left);

                    RecurseDrawGraph(child, rcChild);

                    if (bStart)
                        _lsRenderActions.Add(new DrawRectangle() { rc = rc });
                    
                    if (lastChild)
			        {
				        i++;
                        c++;

				        if (i < childrenPerRow[row])
                            listChildren[c].NodeDatum.TreeMapRect = new Rectangle(-1, -1, -1, -1);
				
				        c+= childrenPerRow[row] - i;
				        break;
			        }

			        left= fRight;
		        }
                // This asserts due to rounding error: Utilities.Assert(1302.3306, left == (horizontalRows ? rc.Right : rc.Bottom));
		        top= fBottom;
	        }
            // This asserts due to rounding error: Utilities.Assert(1302.3307, top == (horizontalRows ? rc.Bottom : rc.Right));
            return true;
        }

        static double KDirStat_CalcutateNextRow(LocalTreeNode parent, int nextChild, double width, ref int childrenUsed, double[] arrChildWidth,
            List<LocalTreeNode> listChildren)
        {
            const double kdMinProportion = 0.4;
            MBoxStatic.Assert(1302.3308, kdMinProportion < 1);

            MBoxStatic.Assert(1302.3309, nextChild < listChildren.Count);
            MBoxStatic.Assert(1302.33101, width >= 1.0);

            var nodeDatum = parent.NodeDatum;

            if (null == nodeDatum)      // this check is new 2/13/15 and has never been hit
            {
                MBoxStatic.Assert(99961, false);
                return 0;
            }

            var mySize = (double)nodeDatum.TotalLength;
	        ulong sizeUsed= 0;
	        double rowHeight= 0;
            var i = 0;

            for (i = nextChild; i < listChildren.Count; i++)
	        {
                var childSize = listChildren[i].NodeDatum.TotalLength;
		        sizeUsed+= childSize;
		        var virtualRowHeight= sizeUsed / mySize;
                MBoxStatic.Assert(1302.3311, virtualRowHeight > 0);
                MBoxStatic.Assert(1302.3312, virtualRowHeight <= 1);
		
		        // Rectangle(mySize)    = width * 1.0
		        // Rectangle(childSize) = childWidth * virtualRowHeight
		        // Rectangle(childSize) = childSize / mySize * width

		        double childWidth= childSize / mySize * width / virtualRowHeight;

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
		        rowHeight= virtualRowHeight;
	        }

            MBoxStatic.Assert(1302.3314, i > nextChild);

	        // Now i-1 is the last child used
	        // and rowHeight is the height of the row.

	        childrenUsed= i - nextChild;

	        // Now as we know the rowHeight, we compute the widths of our children.
	        for (i=0; i < childrenUsed; i++)
	        {
		        // Rectangle(1.0 * 1.0) = mySize
		        var rowSize= mySize * rowHeight;
                var nodeDatum_A = listChildren[nextChild + i].NodeDatum;

                if (null == nodeDatum_A)      // this check is new 2/13/15 and has never been hit
                {
                    MBoxStatic.Assert(99960, false);
                    return 0;
                }

                var childSize = (double)nodeDatum_A.TotalLength;
		        var cw= childSize / rowSize;
                MBoxStatic.Assert(1302.3315, cw >= 0);
		        arrChildWidth[nextChild + i]= cw;
	        }

	        return rowHeight;
        }

        abstract class RenderAction { internal Rectangle rc; internal abstract void Stroke(Graphics g); }
        class FillRectangle : RenderAction { internal Brush Brush; internal override void Stroke(Graphics g) { g.FillRectangle(Brush, rc); } }
        class DrawRectangle : RenderAction { internal static Pen Pen = new Pen(Color.Black, 2); internal override void Stroke(Graphics g) { g.DrawRectangle(Pen, rc); } }
        List<RenderAction> _lsRenderActions = new List<RenderAction>();

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
        LocalTreeNode
            _treeNode = null;
        LocalTreeNode
            _prevNode = null;
        LocalTreeNode
            _deepNode = null;
        LocalTreeNode
            _deepNodeDrawn = null;
        readonly SDL_Timer
            _timerAnim = null;
        int
            _nAnimFrame = 0;
        DateTime
            _dtHideGoofball = DateTime.MinValue;
        readonly ToolTip
            _toolTip = new ToolTip();
        bool
            _bMouseDown = false;
        int
            _nInvalidateRef = 0;
    }
}
