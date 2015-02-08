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

        Rectangle m_rectBitmap = Rectangle.Empty;
        Rectangle m_selRect = Rectangle.Empty;
        Rectangle m_rectCenter = Rectangle.Empty;
        SizeF m_sizeTranslate = SizeF.Empty;
        BufferedGraphics m_bg = null;
        LocalTreeNode m_treeNode = null;
        LocalTreeNode m_prevNode = null;
        LocalTreeNode m_deepNode = null;
        LocalTreeNode m_deepNodeDrawn = null;
        readonly SDL_Timer m_timerAnim = new SDL_Timer();
        int m_nAnimFrame = 0;
        DateTime m_dtHideGoofball = DateTime.MinValue;
        readonly ToolTip m_toolTip = new ToolTip();

        internal UC_TreeMap()
        {
            m_toolTip.UseFading = true;
            m_toolTip.UseAnimation = true;
            m_timerAnim.Interval = new TimeSpan(0, 0, 0, 0, 33);    // 30 FPS
            m_timerAnim.Tick += (o, e) =>
            {
                if (m_rectCenter != Rectangle.Empty)
                {
                    ++m_nAnimFrame;
                    Invalidate(m_rectCenter);
                }
            };
            m_timerAnim.Start();

            SetStyle(ControlStyles.DoubleBuffer |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint,
                true);
        }

        internal void Clear()
        {
            m_treeNode = null;
            m_prevNode = null;
            m_deepNode = null;
            m_deepNodeDrawn = null;
            m_toolTip.Tag = null;
            UtilProject.WriteLine(DateTime.Now + " Clear();");
            ClearSelection();
        }

        internal void ClearSelection(bool bKeepTooltipActive = false)
        {
            Control ctl = TooltipAnchor;

            if ((ctl == null) || ctl.IsDisposed)
                ctl = this;

            if ((ctl == null) || ctl.IsDisposed)
                return;

            m_toolTip.Hide(ctl);

            if (bKeepTooltipActive == false)
            {
                ToolTipActive = false;
                UtilProject.WriteLine(DateTime.Now + " b ToolTipActive = false;");
            }

            m_selRect = Rectangle.Empty;
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (m_bg != null)
            {
                m_bg.Dispose();
                m_bg = null;
            }

            m_toolTip.Dispose();
            ToolTipActive = false; UtilProject.WriteLine(DateTime.Now + " c ToolTipActive = false;");
            base.Dispose(disposing);
        }

        internal LocalTreeNode DoToolTip(Point pt_in)
        {
            UtilProject.WriteLine(DateTime.Now + " DoToolTip();");
            ClearSelection();

            if (m_treeNode == null)
            {
                return null;
            }

            if (m_rectCenter.Contains(pt_in))   // click once to hide goofball. Click again within 5 seconds to return to the deep node.
            {
                if (m_dtHideGoofball == DateTime.MinValue)
                {
                    m_dtHideGoofball = DateTime.Now;
                    return null;
                }
                else if (DateTime.Now - m_dtHideGoofball < TimeSpan.FromSeconds(5))
                {
                    m_dtHideGoofball = DateTime.MinValue;
                    return m_deepNode;
                }
            }

            m_dtHideGoofball = DateTime.MinValue;   // click anywhere else on the treemap and the goofball returns.

            Point pt = Point.Ceiling(new PointF(pt_in.X / m_sizeTranslate.Width, pt_in.Y / m_sizeTranslate.Height));
            LocalTreeNode nodeRet = null;
            bool bImmediateFiles = false;
            bool bVolumeView = false;

            UtilAnalysis_DirList.Closure(() =>
            {
                {
                    NodeDatum nodeDatum = ((NodeDatum)m_treeNode.Tag);

                    bVolumeView = ((nodeDatum is RootNodeDatum) && ((RootNodeDatum)nodeDatum).VolumeView);

                    if ((bVolumeView == false) && ((nodeRet = FindMapNode(nodeDatum.TreeMapFiles, pt)) != null))
                    {
                        bImmediateFiles = true;
                        return;
                    }
                }

                LocalTreeNode m_prevNode_A = m_prevNode ?? m_treeNode;

                if ((nodeRet = FindMapNode(m_prevNode_A, pt)) != null)
                {
                    return;
                }

                LocalTreeNode nodeUplevel = (LocalTreeNode)((m_prevNode != null) ? m_prevNode.Parent : null);
                bool bFoundUplevel = false;

                while (nodeUplevel != null)
                {
                    if ((nodeRet = FindMapNode(nodeUplevel, pt)) != null)
                    {
                        bFoundUplevel = true;
                        return;
                    }

                    nodeUplevel = (LocalTreeNode)nodeUplevel.Parent;
                }

                if (bFoundUplevel)
                {
                    return;
                }

                if ((nodeRet = FindMapNode(m_treeNode, pt)) != null)
                {
                    return;
                }

                nodeRet = m_treeNode;
            });

            if ((bVolumeView == false) && (bImmediateFiles == false))
            {
                LocalTreeNode nodeRet_A = FindMapNode(((NodeDatum)nodeRet.Tag).TreeMapFiles, pt);

                if (nodeRet_A != null && (nodeRet == m_treeNode))
                {
                    nodeRet = nodeRet_A;
                    bImmediateFiles = true;
                }
            }

            if (nodeRet == m_prevNode)
            {
                nodeRet = m_treeNode;
                bImmediateFiles = false;
            }

            m_toolTip.ToolTipTitle = nodeRet.Text;

            if (bImmediateFiles)
            {
                m_toolTip.ToolTipTitle += " (immediate files)";
            }

            m_toolTip.Tag = nodeRet;

            {
                NodeDatum nodeDatum = (NodeDatum)nodeRet.Tag;

                m_selRect = nodeDatum.TreeMapRect;
                m_toolTip.Show(UtilAnalysis_DirList.FormatSize(nodeDatum.nTotalLength, bBytes: true), TooltipAnchor, new Point(0, 0));
                ToolTipActive = true; UtilProject.WriteLine(DateTime.Now + " a ToolTipActive = true; ------");
            }

            m_prevNode = nodeRet;
            Invalidate();
            return null;
        }

        LocalTreeNode FindMapNode(LocalTreeNode treeNode_in, Point pt, bool bNextNode = false)
        {
            LocalTreeNode treeNode = treeNode_in;

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

                if ((treeNode.Nodes == null) || (treeNode.Nodes.Count <= 0))
                {
                    continue;
                }

                LocalTreeNode foundNode = FindMapNode((LocalTreeNode)treeNode.Nodes[0], pt, bNextNode: true);

                if (foundNode != null)
                {
                    return foundNode;
                }
            }
            while (bNextNode && ((treeNode = (LocalTreeNode)treeNode.NextNode) != null));

            return null;
        }

        LocalTreeNode GetFileList(LocalTreeNode parent)
        {
            List<ulong> listLengths = new List<ulong>();
            List<string[]> listFiles = TreeSelect.GetFileList(parent, listLengths);

            if (listFiles == null)
            {
                return null;
            }

            LocalTreeNode nodeFileList = new LocalTreeNode(parent.Text);
            ulong nTotalLength = 0;
            List<ulong>.Enumerator iterUlong = listLengths.GetEnumerator();

            foreach (string[] arrLine in listFiles)
            {
                MBoxStatic.Assert(1302.3316, iterUlong.MoveNext());
                NodeDatum nodeDatum_A = new NodeDatum();

                nTotalLength += nodeDatum_A.nTotalLength = iterUlong.Current;

                if (iterUlong.Current <= 0)
                {
                    continue;
                }

                LocalTreeNode nodeFile = new LocalTreeNode(arrLine[0]);

                nodeFile.Tag = nodeDatum_A;
                nodeFile.ForeColor = Color.OliveDrab;
                nodeFileList.Nodes.Add(nodeFile);
            }

            if (nTotalLength <= 0)
            {
                return null;
            }

            NodeDatum nodeDatum = (NodeDatum)parent.Tag;
            NodeDatum nodeDatum_B = new NodeDatum();

            MBoxStatic.Assert(1302.3301, nTotalLength == nodeDatum.nLength);
            nodeDatum_B.nTotalLength = nTotalLength;
            nodeDatum_B.TreeMapRect = nodeDatum.TreeMapRect;
            nodeFileList.Tag = nodeDatum_B;
            MBoxStatic.Assert(1302.3302, nodeFileList.SelectedImageIndex == -1);              // sets the bitmap size
            nodeFileList.SelectedImageIndex = -1;
            return nodeFileList;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (m_selRect != Rectangle.Empty)
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(64, 0, 0, 0)), m_selRect.Scale(m_sizeTranslate));
            }

            if ((m_deepNodeDrawn == null) || (m_deepNodeDrawn == m_treeNode))
            {
                m_rectCenter = Rectangle.Empty;
                return;
            }

            if (m_dtHideGoofball != DateTime.MinValue)
            {
                return;
            }

            RectangleF r = (((NodeDatum)m_deepNodeDrawn.Tag).TreeMapRect).Scale(m_sizeTranslate);

            r.Inflate(-r.Width / 2 + 15, -r.Height / 2 + 15);
            m_rectCenter = Rectangle.Ceiling(r);

            GraphicsPath path = new GraphicsPath();

            path.AddEllipse(m_rectCenter);

            PathGradientBrush brush = new PathGradientBrush(path);

            brush.CenterColor = Color.White;
            brush.SurroundColors = new Color[] { Color.FromArgb(0, 0, 0, 0) };
            e.Graphics.FillEllipse(brush, m_rectCenter);
            r.Inflate(-r.Width / 5, -r.Height / 5);

            Rectangle r_A = Rectangle.Ceiling(r);
            int nAnimFrame = (m_nAnimFrame %= 6) * 30;

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
            m_prevNode = null;
            UtilProject.WriteLine(DateTime.Now + " OnSizeChanged();");
            ClearSelection();
        }

        internal void Render(LocalTreeNode treeNode)
        {
            if ((m_deepNode == null) || (m_deepNode.IsChildOf(treeNode) == false))
            {
                m_deepNode = treeNode;
            }

            int nPxPerSide = (treeNode.SelectedImageIndex < 0) ? 1024 : treeNode.SelectedImageIndex;

            if (nPxPerSide != m_rectBitmap.Size.Width)
            {
                DateTime dtStart_A = DateTime.Now;

                m_rectBitmap = new Rectangle(0, 0, nPxPerSide, nPxPerSide);
                BackgroundImage = new Bitmap(m_rectBitmap.Size.Width, m_rectBitmap.Size.Height);

                BufferedGraphicsContext bgcontext = BufferedGraphicsManager.Current;

                bgcontext.MaximumBuffer = m_rectBitmap.Size;

                if (m_bg != null)
                {
                    m_bg.Dispose();
                }

                m_bg = bgcontext.Allocate(Graphics.FromImage(BackgroundImage), m_rectBitmap);
                TranslateSize();
                UtilProject.WriteLine("Size bitmap " + nPxPerSide  + " " + (DateTime.Now - dtStart_A).TotalMilliseconds / 1000.0 + " seconds.");
            }

            DateTime dtStart = DateTime.Now;

            UtilProject.WriteLine(DateTime.Now + " Render();");
            ClearSelection();
            m_bg.Graphics.Clear(Color.DarkGray);
            m_treeNode = treeNode;
            DrawTreemap();
            m_bg.Graphics.DrawRectangle(new Pen(Brushes.Black, 10), m_rectBitmap);
            m_bg.Render();
            m_selRect = Rectangle.Empty;
            m_prevNode = null;
            Invalidate();
            m_dtHideGoofball = DateTime.MinValue;

            if ((DateTime.Now - dtStart) > TimeSpan.FromSeconds(1))
            {
                treeNode.SelectedImageIndex = Math.Max((int)
                    (((treeNode.SelectedImageIndex < 0) ? m_rectBitmap.Size.Width : treeNode.SelectedImageIndex)
                    * .75), 256);
            }
        }

        internal string Tooltip_Click()
        {
            if (m_toolTip.Tag == null)
            {
                return null;
            }

            LocalTreeNode treeNode_A = (LocalTreeNode)m_toolTip.Tag;

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
            SizeF sizeBitmap = m_rectBitmap.Size;
            SizeF size = Size;

            m_sizeTranslate = new SizeF(size.Width / sizeBitmap.Width, size.Height / sizeBitmap.Height);
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
            m_deepNodeDrawn = null;
            Graphics graphics = m_bg.Graphics;
            Rectangle rc = m_rectBitmap;

	        rc.Width--;
	        rc.Height--;

	        if (rc.Width <= 0 || rc.Height <= 0)
            {
		        return;
            }

            NodeDatum nodeDatum = (NodeDatum)m_treeNode.Tag;

            if (nodeDatum.nTotalLength > 0)
	        {
                RecurseDrawGraph(m_treeNode, rc, bStart: true);
	        }
	        else
	        {
                graphics.FillRectangle(Brushes.Wheat, rc);
            }
        }

        void RecurseDrawGraph(
	        LocalTreeNode item, 
	        Rectangle rc,
            bool bStart = false
        )
        {
            MBoxStatic.Assert(1302.3303, rc.Width >= 0);
            MBoxStatic.Assert(1302.3304, rc.Height >= 0);

            Graphics graphics = m_bg.Graphics;

	        if (rc.Width <= 0 || rc.Height <= 0)
	        {
		        return;
	        }

            if ((m_deepNode != null) &&
                ((item == m_deepNode) || (m_deepNode.IsChildOf(item))))
            {
                m_deepNodeDrawn = item;
            }

            NodeDatum nodeDatum = (NodeDatum)item.Tag;

            nodeDatum.TreeMapRect = rc;

            if (bStart && (nodeDatum.TreeMapFiles == null) && (item.TreeView != null))
            {
                nodeDatum.TreeMapFiles = GetFileList(item);
            }

            if (((item.Nodes.Count > 0) || (bStart && (nodeDatum.TreeMapFiles != null)))
                && KDirStat_DrawChildren(graphics, item, bStart))
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
            graphics.FillRectangle(brush, rc);
        }

         //My first approach was to make this member pure virtual and have three
         //classes derived from CTreemap. The disadvantage is then, that we cannot
         //simply have a member variable of type CTreemap but have to deal with
         //pointers, factory methods and explicit destruction. It's not worth.

         //I learned this squarification style from the KDirStat executable.
         //It's the most complex one here but also the clearest, imho.
        
        bool KDirStat_DrawChildren(Graphics graphics, LocalTreeNode parent_in, bool bStart = false)
        {
            List<LocalTreeNode> listChildren = null;
            LocalTreeNode parent = null;

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
                LocalTreeNode nodeFree = new LocalTreeNode(parent_in.Text + " (free space)");

                nodeDatumFree.nTotalLength = rootNodeDatum.VolumeFree;
                nodeFree.Tag = nodeDatumFree;
                nodeFree.ForeColor = Color.MediumSpringGreen;

                NodeDatum nodeDatumUnread = new NodeDatum();
                LocalTreeNode nodeUnread = new LocalTreeNode(parent_in.Text + " (unread data)");
                ulong nVolumeLength = rootNodeDatum.VolumeLength;
                long nUnreadLength = (long)nVolumeLength - (long)rootNodeDatum.VolumeFree - (long)rootNodeDatum.nTotalLength;

                if (nUnreadLength < 0)
                {
                    nVolumeLength = rootNodeDatum.VolumeFree + rootNodeDatum.nTotalLength;      // Faked length to make up for compression and hard links
                    nodeDatumUnread.nTotalLength = 0;
                }
                else
                {
                    nodeDatumUnread.nTotalLength = nVolumeLength - rootNodeDatum.VolumeFree - rootNodeDatum.nTotalLength;
                }

                nodeDatumUnread.nTotalLength = (ulong)nUnreadLength;
                nodeUnread.Tag = nodeDatumUnread;
                nodeUnread.ForeColor = Color.MediumVioletRed;
                listChildren = new List<LocalTreeNode>();
                listChildren.Add(parent_in);                                // parent added as child, with two other nodes:
                listChildren.Add(nodeFree);                                 // free space (color: spring green); and

                if (nUnreadLength > 0)
                {
                    listChildren.Add(nodeUnread);                           // unread guess, affected by compression and hard links (violet)
                }

                parent = new LocalTreeNode(parent_in.Text + " (volume)");

                NodeDatum nodeDatumVolume = new NodeDatum();

                nodeDatumVolume.nTotalLength = nVolumeLength;
                nodeDatumVolume.TreeMapRect = ((NodeDatum)parent_in.Tag).TreeMapRect;
                parent.Tag = nodeDatumVolume;
                bVolumeNode = true;
                rootNodeDatum.VolumeView = true;
            });

            if (bVolumeNode == false)
            {
                parent = parent_in;
                listChildren = parent.Nodes.Cast<LocalTreeNode>().Where(t => ((NodeDatum)t.Tag).nTotalLength > 0).ToList();
            }

            NodeDatum nodeDatum = (NodeDatum)parent.Tag;
            Rectangle rc = nodeDatum.TreeMapRect;
	        List<double> rows = new List<double>();	// Our rectangle is divided into rows, each of which gets this height (fraction of total height).
	        List<int> childrenPerRow = new List<int>();// childrenPerRow[i] = # of children in rows[i]

            if (bStart && (nodeDatum.TreeMapFiles != null))
            {
                listChildren.Add(nodeDatum.TreeMapFiles);
            }
            else if (nodeDatum.nLength > 0)
            {
                NodeDatum nodeFiles = new NodeDatum();

                nodeFiles.nTotalLength = nodeDatum.nLength;

                LocalTreeNode treeNode = new LocalTreeNode(parent.Text);

                treeNode.Tag = nodeFiles;
                treeNode.ForeColor = Color.OliveDrab;
                listChildren.Add(treeNode);
            }

            listChildren.Sort((y, x) => ((NodeDatum)x.Tag).nTotalLength.CompareTo(((NodeDatum)y.Tag).nTotalLength));

            if (listChildren.Count <= 0)
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
                    LocalTreeNode child = listChildren[c];
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
                        graphics.DrawRectangle(new Pen(Color.Black, 2), rcChild);
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

        double KDirStat_CalcutateNextRow(LocalTreeNode parent, int nextChild, double width, ref int childrenUsed, double[] arrChildWidth,
            List<LocalTreeNode> listChildren)
        {
            const double _minProportion = 0.4;
            MBoxStatic.Assert(1302.3308, _minProportion < 1);

            MBoxStatic.Assert(1302.3309, nextChild < listChildren.Count);
            MBoxStatic.Assert(1302.33101, width >= 1.0);

	        double mySize= (double)((NodeDatum)parent.Tag).nTotalLength;
	        ulong sizeUsed= 0;
	        double rowHeight= 0;
            int i = 0;

            for (i = nextChild; i < listChildren.Count; i++)
	        {
                ulong childSize = ((NodeDatum)listChildren[i].Tag).nTotalLength;
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
                double childSize = (double)((NodeDatum)listChildren[nextChild + i].Tag).nTotalLength;
		        double cw= childSize / rowSize;
                MBoxStatic.Assert(1302.3315, cw >= 0);
		        arrChildWidth[nextChild + i]= cw;
	        }

	        return rowHeight;
        }
    }
}
