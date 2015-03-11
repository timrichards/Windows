using System.Windows.Forms;
using System.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing.Drawing2D;

namespace DoubleFile
{
    [System.ComponentModel.DesignerCategory("Code")]
    class UC_TreeMap : UserControl
    {
        internal bool ToolTipActive { get; private set; }
        internal Control TooltipAnchor = null;

        internal UC_TreeMap()
        {
            _toolTip.UseFading = true;
            _toolTip.UseAnimation = true;
            _timerAnim = new SDL_Timer(33.0, () =>   // 30 FPS
            {
                if (_rectCenter != Rectangle.Empty)
                {
                    ++_nAnimFrame;
                    Invalidate(_rectCenter);
                }
            }).Start();

            SetStyle(ControlStyles.DoubleBuffer |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint,
                true);
            BackgroundImageLayout = ImageLayout.Stretch;
            Dock = DockStyle.Fill;
            BackColor = Color.Transparent;

            if (null == TooltipAnchor)
                TooltipAnchor = this;
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

            Control ctl = TooltipAnchor;

            if ((ctl == null) || ctl.IsDisposed)
                ctl = this;

            if (ctl.IsDisposed)
                return;

            _toolTip.Hide(ctl);

            if (bKeepTooltipActive == false)
            {
                ToolTipActive = false;
                UtilProject.WriteLine(DateTime.Now + " b ToolTipActive = false;");
            }

            _selRect = Rectangle.Empty;
            Invalidate();
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

            Point pt = Point.Ceiling(new PointF(pt_in.X / _sizeTranslate.Width, pt_in.Y / _sizeTranslate.Height));
            TreeNode nodeRet = null;
            bool bImmediateFiles = false;
            bool bVolumeView = false;

            UtilAnalysis_DirList.Closure(() =>
            {
                {
                    var nodeDatum = (_treeNode.Tag as NodeDatum);

                    if (null == nodeDatum)      // added 2/17/15 as safety
                    {
                        MBoxStatic.Assert(99979, false);
                        return;
                    }

                    var rootNodeDatum = (_treeNode.Tag as RootNodeDatum);

                    bVolumeView = (null != rootNodeDatum) &&
                        (rootNodeDatum.VolumeView);

                    if ((bVolumeView == false) &&
                        ((nodeRet = FindMapNode(nodeDatum.TreeMapFiles, pt)) != null))
                    {
                        bImmediateFiles = true;
                        return;
                    }
                }

                TreeNode m_prevNode_A = _prevNode ?? _treeNode;

                if ((nodeRet = FindMapNode(m_prevNode_A, pt)) != null)
                {
                    return;
                }

                TreeNode nodeUplevel = (TreeNode)((_prevNode != null) ? _prevNode.Parent : null);
                bool bFoundUplevel = false;

                while (nodeUplevel != null)
                {
                    if ((nodeRet = FindMapNode(nodeUplevel, pt)) != null)
                    {
                        bFoundUplevel = true;
                        return;
                    }

                    nodeUplevel = (TreeNode)nodeUplevel.Parent;
                }

                if (bFoundUplevel)
                {
                    return;
                }

                if ((nodeRet = FindMapNode(_treeNode, pt)) != null)
                {
                    return;
                }

                nodeRet = _treeNode;
            });

            if ((bVolumeView == false) && (bImmediateFiles == false))
            {
                TreeNode nodeRet_A = FindMapNode(((NodeDatum)nodeRet.Tag).TreeMapFiles, pt);

                if (nodeRet_A != null && (nodeRet == _treeNode))
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
                NodeDatum nodeDatum = (NodeDatum)nodeRet.Tag;

                _selRect = nodeDatum.TreeMapRect;
                _toolTip.Show(UtilAnalysis_DirList.FormatSize(nodeDatum.TotalLength, bBytes: true), TooltipAnchor, new Point(0, 0));
                ToolTipActive = true; UtilProject.WriteLine(DateTime.Now + " a ToolTipActive = true; ------");
            }

            _prevNode = nodeRet;
            Invalidate();
            return null;
        }

        TreeNode FindMapNode(TreeNode treeNode_in, Point pt, bool bNextNode = false)
        {
            TreeNode treeNode = treeNode_in;

            if (treeNode == null)
            {
                return null;
            }

            do
            {
                NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;

                if (nodeDatum.TreeMapRect.Contains(pt) == false)
                {
                    continue;
                }

                if (bNextNode || (treeNode != treeNode_in))
                {
                    return treeNode;
                }

                if ((treeNode.Nodes == null) || (treeNode.Nodes.IsEmpty()))
                {
                    continue;
                }

                TreeNode foundNode = FindMapNode((TreeNode)treeNode.Nodes[0], pt, bNextNode: true);

                if (foundNode != null)
                {
                    return foundNode;
                }
            }
            while (bNextNode && ((treeNode = (TreeNode)treeNode.NextNode) != null));

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

            NodeDatum nodeDatum = (NodeDatum)parent.Tag;
            NodeDatum nodeDatum_B = new NodeDatum();

            MBoxStatic.Assert(1302.3301, nTotalLength == nodeDatum.Length);
            nodeDatum_B.TotalLength = nTotalLength;
            nodeDatum_B.TreeMapRect = nodeDatum.TreeMapRect;
            nodeFileList.Tag = nodeDatum_B;
            MBoxStatic.Assert(1302.3302, nodeFileList.SelectedImageIndex == -1);              // sets the bitmap size
            nodeFileList.SelectedImageIndex = -1;
            return nodeFileList;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_selRect != Rectangle.Empty)
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(64, 0, 0, 0)), _selRect.Scale(_sizeTranslate));
            }

            if ((_deepNodeDrawn == null) || (_deepNodeDrawn == _treeNode))
            {
                _rectCenter = Rectangle.Empty;
                return;
            }

            if (_dtHideGoofball != DateTime.MinValue)
            {
                return;
            }

            RectangleF r = (((NodeDatum)_deepNodeDrawn.Tag).TreeMapRect).Scale(_sizeTranslate);

            r.Inflate(-r.Width / 2 + 15, -r.Height / 2 + 15);
            _rectCenter = Rectangle.Ceiling(r);

            GraphicsPath path = new GraphicsPath();

            path.AddEllipse(_rectCenter);

            PathGradientBrush brush = new PathGradientBrush(path);

            brush.CenterColor = Color.White;
            brush.SurroundColors = new Color[] { Color.FromArgb(0, 0, 0, 0) };
            e.Graphics.FillEllipse(brush, _rectCenter);
            r.Inflate(-r.Width / 5, -r.Height / 5);

            Rectangle r_A = Rectangle.Ceiling(r);
            int nAnimFrame = (_nAnimFrame %= 6) * 30;

            brush.CenterColor = Color.White;
            brush.SurroundColors = new Color[] { Color.Black };
            e.Graphics.FillPie(brush, r_A, 90 + nAnimFrame, 90);
            e.Graphics.FillPie(brush, r_A, 270 + nAnimFrame, 90);
            brush.CenterColor = Color.Black;
            brush.SurroundColors = new Color[] { Color.White };
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
            if ((_deepNode == null) || (_deepNode.IsChildOf(treeNode) == false))
            {
                _deepNode = treeNode;
            }

            int nPxPerSide = (treeNode.SelectedImageIndex < 0) ? 1024 : treeNode.SelectedImageIndex;

            if (nPxPerSide != _rectBitmap.Size.Width)
            {
                DateTime dtStart_A = DateTime.Now;

                _rectBitmap = new Rectangle(0, 0, nPxPerSide, nPxPerSide);
                BackgroundImage = new Bitmap(_rectBitmap.Size.Width, _rectBitmap.Size.Height);

                BufferedGraphicsContext bgcontext = BufferedGraphicsManager.Current;

                bgcontext.MaximumBuffer = _rectBitmap.Size;

                if (_bg != null)
                {
                    _bg.Dispose();
                }

                _bg = bgcontext.Allocate(Graphics.FromImage(BackgroundImage), _rectBitmap);
                TranslateSize();
                UtilProject.WriteLine("Size bitmap " + nPxPerSide  + " " + (DateTime.Now - dtStart_A).TotalMilliseconds / 1000.0 + " seconds.");
            }

            DateTime dtStart = DateTime.Now;

            UtilProject.WriteLine(DateTime.Now + " Render();");
            ClearSelection();
            _bg.Graphics.Clear(Color.DarkGray);
            _treeNode = treeNode;
            DrawTreemap();
            _bg.Graphics.DrawRectangle(new Pen(Brushes.Black, 10), _rectBitmap);
            _bg.Render();
            _selRect = Rectangle.Empty;
            _prevNode = null;
            Invalidate();
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
            if (_toolTip.Tag == null)
            {
                return null;
            }

            TreeNode treeNode_A = (TreeNode)_toolTip.Tag;

            if (treeNode_A.TreeView != null)    // null if fake file treenode (NodeDatum.TreeMapFiles)
            {
                if (treeNode_A.Tag is RootNodeDatum)
                {
                    ((RootNodeDatum)treeNode_A.Tag).VolumeView = (((RootNodeDatum)treeNode_A.Tag).VolumeView == false);
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
            Rectangle rc = _rectBitmap;

	        rc.Width--;
	        rc.Height--;

	        if (rc.Width <= 0 || rc.Height <= 0)
            {
		        return;
            }

            NodeDatum nodeDatum = (NodeDatum)_treeNode.Tag;

            if (nodeDatum.TotalLength > 0)
	        {
                RecurseDrawGraph(_treeNode, rc, bStart: true);
	        }
	        else
	        {
                _bg.Graphics.FillRectangle(Brushes.Wheat, rc);
            }
        }

        void RecurseDrawGraph(
	        TreeNode item, 
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

            if ((_deepNode != null) &&
                ((item == _deepNode) || (_deepNode.IsChildOf(item))))
            {
                _deepNodeDrawn = item;
            }

            NodeDatum nodeDatum = (NodeDatum)item.Tag;

            nodeDatum.TreeMapRect = rc;

            if (bStart && (nodeDatum.TreeMapFiles == null) && (item.TreeView != null))
            {
                nodeDatum.TreeMapFiles = GetFileList(item);
            }

            if (((false == item.Nodes.IsEmpty()) || (bStart && (nodeDatum.TreeMapFiles != null)))
                && KDirStat_DrawChildren(item, bStart))
            {
                // example scenario: empty folder when there are immediate files and bStart is not true
                return;
            }

            GraphicsPath path = new GraphicsPath();
            Rectangle r = rc;

            r.Inflate(r.Width / 2, r.Height / 2);
            path.AddEllipse(r);

            PathGradientBrush brush = new PathGradientBrush(path);

            brush.CenterColor = Color.Wheat;
            brush.SurroundColors = new Color[] { ControlPaint.Dark((item.ForeColor == Color.Empty) ? Color.SandyBrown : item.ForeColor) };
            _bg.Graphics.FillRectangle(brush, rc);
        }

         //My first approach was to make this member pure virtual and have three
         //classes derived from CTreemap. The disadvantage is then, that we cannot
         //simply have a member variable of type CTreemap but have to deal with
         //pointers, factory methods and explicit destruction. It's not worth.

         //I learned this squarification style from the KDirStat executable.
         //It's the most complex one here but also the clearest, imho.
        
        bool KDirStat_DrawChildren(TreeNode parent_in, bool bStart = false)
        {
            List<TreeNode> listChildren = null;
            TreeNode parent = null;

            bool bVolumeNode = false;

            UtilAnalysis_DirList.Closure(() =>
            {
                if ((bStart == false) || ((parent_in.Tag is RootNodeDatum) == false))
                {
                    return;
                }

                RootNodeDatum rootNodeDatum = (RootNodeDatum)parent_in.Tag;

                if (rootNodeDatum.VolumeView == false)
                {
                    return;
                }

                NodeDatum nodeDatumFree = new NodeDatum();
                TreeNode nodeFree = new TreeNode(parent_in.Text + " (free space)");

                nodeDatumFree.TotalLength = rootNodeDatum.VolumeFree;
                nodeFree.Tag = nodeDatumFree;
                nodeFree.ForeColor = Color.MediumSpringGreen;

                NodeDatum nodeDatumUnread = new NodeDatum();
                TreeNode nodeUnread = new TreeNode(parent_in.Text + " (unread data)");
                ulong nVolumeLength = rootNodeDatum.VolumeLength;
                long nUnreadLength = (long)nVolumeLength - (long)rootNodeDatum.VolumeFree - (long)rootNodeDatum.TotalLength;

                if (nUnreadLength < 0)
                {
                    nVolumeLength = rootNodeDatum.VolumeFree + rootNodeDatum.TotalLength;      // Faked length to make up for compression and hard links
                    nodeDatumUnread.TotalLength = 0;
                }
                else
                {
                    nodeDatumUnread.TotalLength = nVolumeLength - rootNodeDatum.VolumeFree - rootNodeDatum.TotalLength;
                }

                nodeDatumUnread.TotalLength = (ulong)nUnreadLength;
                nodeUnread.Tag = nodeDatumUnread;
                nodeUnread.ForeColor = Color.MediumVioletRed;
                listChildren = new List<TreeNode>();
                listChildren.Add(parent_in);                                // parent added as child, with two other nodes:
                listChildren.Add(nodeFree);                                 // free space (color: spring green); and

                if (nUnreadLength > 0)
                {
                    listChildren.Add(nodeUnread);                           // unread guess, affected by compression and hard links (violet)
                }

                parent = new TreeNode(parent_in.Text + " (volume)");

                NodeDatum nodeDatumVolume = new NodeDatum();

                nodeDatumVolume.TotalLength = nVolumeLength;
                nodeDatumVolume.TreeMapRect = ((NodeDatum)parent_in.Tag).TreeMapRect;
                parent.Tag = nodeDatumVolume;
                bVolumeNode = true;
                rootNodeDatum.VolumeView = true;
            });

            if (bVolumeNode == false)
            {
                parent = parent_in;
                listChildren = parent.Nodes.Cast<TreeNode>().Where(t => ((NodeDatum)t.Tag).TotalLength > 0).ToList();
            }

            NodeDatum nodeDatum = (NodeDatum)parent.Tag;
            Rectangle rc = nodeDatum.TreeMapRect;
	        List<double> rows = new List<double>();	// Our rectangle is divided into rows, each of which gets this height (fraction of total height).
	        List<int> childrenPerRow = new List<int>();// childrenPerRow[i] = # of children in rows[i]

            if (bStart && (nodeDatum.TreeMapFiles != null))
            {
                listChildren.Add(nodeDatum.TreeMapFiles);
            }
            else if (nodeDatum.Length > 0)
            {
                NodeDatum nodeFiles = new NodeDatum();

                nodeFiles.TotalLength = nodeDatum.Length;

                TreeNode treeNode = new TreeNode(parent.Text);

                treeNode.Tag = nodeFiles;
                treeNode.ForeColor = Color.OliveDrab;
                listChildren.Add(treeNode);
            }

            listChildren.Sort((y, x) => ((NodeDatum)x.Tag).TotalLength.CompareTo(((NodeDatum)y.Tag).TotalLength));

            if (listChildren.IsEmpty())
            {
                // any files are zero in length
                return false;
            }

            double[] childWidth = // Widths of the children (fraction of row width).
                new Double[listChildren.Count];

            bool horizontalRows = (rc.Width >= rc.Height);
            double width_A = 1.0;
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

            int nextChild = 0;
            while (nextChild < listChildren.Count)
            {
                int childrenUsed = 0;

                rows.Add(KDirStat_CalcutateNextRow(parent, nextChild, width_A, ref childrenUsed, childWidth, listChildren));
                childrenPerRow.Add(childrenUsed);
                nextChild += childrenUsed;
            }

	        int width= horizontalRows ? rc.Width : rc.Height;
	        int height= horizontalRows ? rc.Height : rc.Width;

	        int c = 0;
	        double top= horizontalRows ? rc.Top : rc.Left;
	        for (int row=0; row < rows.Count; row++)
	        {
		        double fBottom= top + rows[row] * height;
		        int bottom= (int)fBottom;
		        if (row == rows.Count - 1)
			        bottom= horizontalRows ? rc.Bottom : rc.Right;
		        double left= horizontalRows ? rc.Left : rc.Top;

                for (int i=0; i < childrenPerRow[row]; i++, c++)
		        {
                    TreeNode child = listChildren[c];
                    MBoxStatic.Assert(1302.3305, childWidth[c] >= 0);
			        double fRight= left + childWidth[c] * width;
			        int right= (int)fRight;

			        bool lastChild = (i == childrenPerRow[row] - 1 || childWidth[c + 1] == 0);

			        if (lastChild)
				        right= horizontalRows ? rc.Right : rc.Bottom;

			        Rectangle rcChild = 
			            (horizontalRows)
                        ? new Rectangle((int)left, (int)top, right-(int)left, bottom-(int)top)
                        : new Rectangle((int)top, (int)left, bottom-(int)top, right-(int)left);
			
			        RecurseDrawGraph(child, rcChild);

                    if (bStart)
                    {
                        _bg.Graphics.DrawRectangle(new Pen(Color.Black, 2), rcChild);
                    }
                    
                    if (lastChild)
			        {
				        i++;
                        c++;

				        if (i < childrenPerRow[row])
                            ((NodeDatum)listChildren[c].Tag).TreeMapRect = new Rectangle(-1, -1, -1, -1);
				
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

        double KDirStat_CalcutateNextRow(TreeNode parent, int nextChild, double width, ref int childrenUsed, double[] arrChildWidth,
            List<TreeNode> listChildren)
        {
            const double _minProportion = 0.4;
            MBoxStatic.Assert(1302.3308, _minProportion < 1);

            MBoxStatic.Assert(1302.3309, nextChild < listChildren.Count);
            MBoxStatic.Assert(1302.33101, width >= 1.0);

	        double mySize= (double)((NodeDatum)parent.Tag).TotalLength;
	        ulong sizeUsed= 0;
	        double rowHeight= 0;
            int i = 0;

            for (i = nextChild; i < listChildren.Count; i++)
	        {
                ulong childSize = ((NodeDatum)listChildren[i].Tag).TotalLength;
		        sizeUsed+= childSize;
		        double virtualRowHeight= sizeUsed / mySize;
                MBoxStatic.Assert(1302.3311, virtualRowHeight > 0);
                MBoxStatic.Assert(1302.3312, virtualRowHeight <= 1);
		
		        // Rectangle(mySize)    = width * 1.0
		        // Rectangle(childSize) = childWidth * virtualRowHeight
		        // Rectangle(childSize) = childSize / mySize * width

		        double childWidth= childSize / mySize * width / virtualRowHeight;

		        if (childWidth / virtualRowHeight < _minProportion)
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
		        double rowSize= mySize * rowHeight;
                double childSize = (double)((NodeDatum)listChildren[nextChild + i].Tag).TotalLength;
		        double cw= childSize / rowSize;
                MBoxStatic.Assert(1302.3315, cw >= 0);
		        arrChildWidth[nextChild + i]= cw;
	        }

	        return rowHeight;
        }

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
        TreeNode
            _deepNode = null;
        TreeNode
            _deepNodeDrawn = null;
        readonly SDL_Timer
            _timerAnim = null;
        int
            _nAnimFrame = 0;
        DateTime
            _dtHideGoofball = DateTime.MinValue;
        readonly ToolTip
            _toolTip = new ToolTip();
    }
}
