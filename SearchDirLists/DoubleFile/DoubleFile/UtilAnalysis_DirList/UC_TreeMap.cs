using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Controls;
using System.Windows.Media;
using Media = System.Windows.Media;
using System.Windows.Markup;
using System.Xml;
using System.Windows;

using Forms = System.Windows.Forms;
using Drawing = System.Drawing;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing.Drawing2D;
using System.Windows.Threading;
using System.Windows.Forms;

namespace DoubleFile
{
    [System.ComponentModel.DesignerCategory("Code")]
    class UC_TreeMap : Forms.UserControl
    {
        public Forms.Control TooltipAnchor = null;

        Drawing.Rectangle m_rectBitmap = Drawing.Rectangle.Empty;
        Drawing.Rectangle m_selRect = Drawing.Rectangle.Empty;
        Drawing.Rectangle m_rectCenter = Drawing.Rectangle.Empty;
        Drawing.SizeF m_sizeTranslate = Drawing.SizeF.Empty;
        Drawing.BufferedGraphics m_bg = null;
        TreeNode m_treeNode = null;
        TreeNode m_prevNode = null;
        TreeNode m_deepNode = null;
        TreeNode m_deepNodeDrawn = null;
        readonly SDL_Timer m_timerAnim = new SDL_Timer();
        int m_nAnimFrame = 0;
        DateTime m_dtHideGoofball = DateTime.MinValue;
        readonly Forms.ToolTip m_toolTip = new Forms.ToolTip();

        public UC_TreeMap()
        {
            m_toolTip.UseFading = true;
            m_toolTip.UseAnimation = true;
            m_timerAnim.Interval = new TimeSpan(0, 0, 0, 0, 33);    // 30 FPS
            m_timerAnim.Tick += new EventHandler((Object sender, EventArgs e) =>
            {
                if (m_rectCenter != Drawing.Rectangle.Empty)
                {
                    ++m_nAnimFrame;
                    Invalidate(m_rectCenter);
                }
            });
            m_timerAnim.Start();

            SetStyle(Forms.ControlStyles.DoubleBuffer |
                Forms.ControlStyles.UserPaint |
                Forms.ControlStyles.AllPaintingInWmPaint,
                true);
        }

        internal void Clear()
        {
            m_treeNode = null;
            m_prevNode = null;
            m_deepNode = null;
            m_deepNodeDrawn = null;
            m_toolTip.Tag = null;
            ClearSelection();
        }

        internal void ClearSelection()
        {
            Forms.Control ctl = TooltipAnchor;
            if ((ctl == null) || ctl.IsDisposed) ctl = this;
            if ((ctl == null) || ctl.IsDisposed) { return; }

            m_toolTip.Hide(ctl);
            m_selRect = Drawing.Rectangle.Empty;
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
            base.Dispose(disposing);
        }

        internal TreeNode DoToolTip(Drawing.Point pt_in)
        {
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

            Drawing.Point pt = Drawing.Point.Ceiling(new Drawing.PointF(pt_in.X / m_sizeTranslate.Width, pt_in.Y / m_sizeTranslate.Height));
            TreeNode nodeRet = null;
            bool bImmediateFiles = false;
            bool bVolumeView = false;

            Utilities.Closure(() =>
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

                TreeNode m_prevNode_A = m_prevNode ?? m_treeNode;

                if ((nodeRet = FindMapNode(m_prevNode_A, pt)) != null)
                {
                    return;
                }

                TreeNode nodeUplevel = (TreeNode)((m_prevNode != null) ? m_prevNode.Parent : null);
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

                if ((nodeRet = FindMapNode(m_treeNode, pt)) != null)
                {
                    return;
                }

                nodeRet = m_treeNode;
            });

            if ((bVolumeView == false) && (bImmediateFiles == false))
            {
                TreeNode nodeRet_A = FindMapNode(((NodeDatum)nodeRet.Tag).TreeMapFiles, pt);

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
                m_toolTip.Show(Utilities.FormatSize(nodeDatum.nTotalLength, bBytes: true), TooltipAnchor, new Drawing.Point(0, 0));
            }

            m_prevNode = nodeRet;
            Invalidate();
            return null;
        }

        TreeNode FindMapNode(TreeNode treeNode_in, Drawing.Point pt, bool bNextNode = false)
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

                if ((treeNode.Nodes == null) || (treeNode.Nodes.Count <= 0))
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

        TreeNode GetFileList(TreeNode parent)
        {
            List<ulong> listLengths = new List<ulong>();
            List<string[]> listFiles = TreeSelect.GetFileList(parent, listLengths);

            if (listFiles == null)
            {
                return null;
            }

            TreeNode nodeFileList = new TreeNode(parent.Text);
            ulong nTotalLength = 0;
            List<ulong>.Enumerator iterUlong = listLengths.GetEnumerator();

            foreach (string[] arrLine in listFiles)
            {
                MBox.Assert(1302.3316, iterUlong.MoveNext());
                NodeDatum nodeDatum_A = new NodeDatum();

                nTotalLength += nodeDatum_A.nTotalLength = iterUlong.Current;

                if (iterUlong.Current <= 0)
                {
                    continue;
                }

                TreeNode nodeFile = new TreeNode(arrLine[0]);

                nodeFile.Tag = nodeDatum_A;
                nodeFile.ForeColor = Drawing.Color.OliveDrab;
                nodeFileList.Nodes.Add(nodeFile);
            }

            if (nTotalLength <= 0)
            {
                return null;
            }

            NodeDatum nodeDatum = (NodeDatum)parent.Tag;
            NodeDatum nodeDatum_B = new NodeDatum();

            MBox.Assert(1302.3301, nTotalLength == nodeDatum.nLength);
            nodeDatum_B.nTotalLength = nTotalLength;
            nodeDatum_B.TreeMapRect = nodeDatum.TreeMapRect;
            nodeFileList.Tag = nodeDatum_B;
            MBox.Assert(1302.3302, nodeFileList.SelectedImageIndex == -1);              // sets the bitmap size
            nodeFileList.SelectedImageIndex = -1;
            return nodeFileList;
        }

        protected override void OnPaint(Forms.PaintEventArgs e)
        {
            base.OnPaint(e);

            if (m_selRect != Drawing.Rectangle.Empty)
            {
                e.Graphics.FillRectangle(new Drawing.SolidBrush(Drawing.Color.FromArgb(64, 0, 0, 0)), m_selRect.Scale(m_sizeTranslate));
            }

            if ((m_deepNodeDrawn == null) || (m_deepNodeDrawn == m_treeNode))
            {
                m_rectCenter = Drawing.Rectangle.Empty;
                return;
            }

            if (m_dtHideGoofball != DateTime.MinValue)
            {
                return;
            }

            Drawing.RectangleF r = (((NodeDatum)m_deepNodeDrawn.Tag).TreeMapRect).Scale(m_sizeTranslate);

            r.Inflate(-r.Width / 2 + 15, -r.Height / 2 + 15);
            m_rectCenter = Drawing.Rectangle.Ceiling(r);

            GraphicsPath path = new GraphicsPath();

            path.AddEllipse(m_rectCenter);

            PathGradientBrush brush = new PathGradientBrush(path);

            brush.CenterColor = Drawing.Color.White;
            brush.SurroundColors = new Drawing.Color[] { Drawing.Color.FromArgb(0, 0, 0, 0) };
            e.Graphics.FillEllipse(brush, m_rectCenter);
            r.Inflate(-r.Width / 5, -r.Height / 5);

            Drawing.Rectangle r_A = Drawing.Rectangle.Ceiling(r);
            int nAnimFrame = (m_nAnimFrame %= 6) * 30;

            brush.CenterColor = Drawing.Color.White;
            brush.SurroundColors = new Drawing.Color[] { Drawing.Color.Black };
            e.Graphics.FillPie(brush, r_A, 90 + nAnimFrame, 90);
            e.Graphics.FillPie(brush, r_A, 270 + nAnimFrame, 90);
            brush.CenterColor = Drawing.Color.Black;
            brush.SurroundColors = new Drawing.Color[] { Drawing.Color.White };
            e.Graphics.FillPie(brush, r_A, 0 + nAnimFrame, 90);
            e.Graphics.FillPie(brush, r_A, 180 + nAnimFrame, 90);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            TranslateSize();
            m_prevNode = null;
            ClearSelection();
        }

        internal void Render(TreeNode treeNode)
        {
            if ((m_deepNode == null) || (m_deepNode.IsChildOf(treeNode) == false))
            {
                m_deepNode = treeNode;
            }

            int nPxPerSide = (treeNode.SelectedImageIndex < 0) ? 1024 : treeNode.SelectedImageIndex;

            if (nPxPerSide != m_rectBitmap.Size.Width)
            {
                DateTime dtStart_A = DateTime.Now;

                m_rectBitmap = new Drawing.Rectangle(0, 0, nPxPerSide, nPxPerSide);
                BackgroundImage = new Drawing.Bitmap(m_rectBitmap.Size.Width, m_rectBitmap.Size.Height);

                Drawing.BufferedGraphicsContext bgcontext = Drawing.BufferedGraphicsManager.Current;

                bgcontext.MaximumBuffer = m_rectBitmap.Size;

                if (m_bg != null)
                {
                    m_bg.Dispose();
                }

                m_bg = bgcontext.Allocate(Drawing.Graphics.FromImage(BackgroundImage), m_rectBitmap);
                TranslateSize();
                UtilProject.WriteLine("Size bitmap " + nPxPerSide  + " " + (DateTime.Now - dtStart_A).TotalMilliseconds / 1000.0 + " seconds.");
            }

            DateTime dtStart = DateTime.Now;

            ClearSelection();
            m_bg.Graphics.Clear(Drawing.Color.DarkGray);
            m_treeNode = treeNode;
            DrawTreemap();
            m_bg.Graphics.DrawRectangle(new Drawing.Pen(Drawing.Brushes.Black, 10), m_rectBitmap);
            m_bg.Render();
            m_selRect = Drawing.Rectangle.Empty;
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

            TreeNode treeNode_A = (TreeNode)m_toolTip.Tag;

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
            Drawing.SizeF sizeBitmap = m_rectBitmap.Size;
            Drawing.SizeF size = Size;

            m_sizeTranslate = new Drawing.SizeF(size.Width / sizeBitmap.Width, size.Height / sizeBitmap.Height);
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
            Drawing.Graphics graphics = m_bg.Graphics;
            Drawing.Rectangle rc = m_rectBitmap;

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
                graphics.FillRectangle(Drawing.Brushes.Wheat, rc);
            }
        }

        void RecurseDrawGraph(
	        TreeNode item, 
	        Drawing.Rectangle rc,
            bool bStart = false
        )
        {
            MBox.Assert(1302.3303, rc.Width >= 0);
            MBox.Assert(1302.3304, rc.Height >= 0);

            Drawing.Graphics graphics = m_bg.Graphics;

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
            Drawing.Rectangle r = rc;

            r.Inflate(r.Width / 2, r.Height / 2);
            path.AddEllipse(r);

            PathGradientBrush brush = new PathGradientBrush(path);

            brush.CenterColor = Drawing.Color.Wheat;
            brush.SurroundColors = new Drawing.Color[] { Forms.ControlPaint.Dark((item.ForeColor == Drawing.Color.Empty) ? Drawing.Color.SandyBrown : item.ForeColor) };
            graphics.FillRectangle(brush, rc);
        }

         //My first approach was to make this member pure virtual and have three
         //classes derived from CTreemap. The disadvantage is then, that we cannot
         //simply have a member variable of type CTreemap but have to deal with
         //pointers, factory methods and explicit destruction. It's not worth.

         //I learned this squarification style from the KDirStat executable.
         //It's the most complex one here but also the clearest, imho.
        
        bool KDirStat_DrawChildren(Drawing.Graphics graphics, TreeNode parent_in, bool bStart = false)
        {
            List<TreeNode> listChildren = null;
            TreeNode parent = null;

            bool bVolumeNode = false;

            Utilities.Closure(new Action(() =>
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

                nodeDatumFree.nTotalLength = rootNodeDatum.VolumeFree;
                nodeFree.Tag = nodeDatumFree;
                nodeFree.ForeColor = Drawing.Color.MediumSpringGreen;

                NodeDatum nodeDatumUnread = new NodeDatum();
                TreeNode nodeUnread = new TreeNode(parent_in.Text + " (unread data)");
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
                nodeUnread.ForeColor = Drawing.Color.MediumVioletRed;
                listChildren = new List<TreeNode>();
                listChildren.Add(parent_in);                                // parent added as child, with two other nodes:
                listChildren.Add(nodeFree);                                 // free space (color: spring green); and

                if (nUnreadLength > 0)
                {
                    listChildren.Add(nodeUnread);                           // unread guess, affected by compression and hard links (violet)
                }

                parent = new TreeNode(parent_in.Text + " (volume)");

                NodeDatum nodeDatumVolume = new NodeDatum();

                nodeDatumVolume.nTotalLength = nVolumeLength;
                nodeDatumVolume.TreeMapRect = ((NodeDatum)parent_in.Tag).TreeMapRect;
                parent.Tag = nodeDatumVolume;
                bVolumeNode = true;
                rootNodeDatum.VolumeView = true;
            }));

            if (bVolumeNode == false)
            {
                parent = parent_in;
                listChildren = parent.Nodes.Cast<TreeNode>().Where(t => ((NodeDatum)t.Tag).nTotalLength > 0).ToList();
            }

            NodeDatum nodeDatum = (NodeDatum)parent.Tag;
            Drawing.Rectangle rc = nodeDatum.TreeMapRect;
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

                TreeNode treeNode = new TreeNode(parent.Text);

                treeNode.Tag = nodeFiles;
                treeNode.ForeColor = Drawing.Color.OliveDrab;
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
                    TreeNode child = listChildren[c];
                    MBox.Assert(1302.3305, childWidth[c] >= 0);
			        double fRight= left + childWidth[c] * width;
			        int right= (int)fRight;

			        bool lastChild = (i == childrenPerRow[row] - 1 || childWidth[c + 1] == 0);

			        if (lastChild)
				        right= horizontalRows ? rc.Right : rc.Bottom;

			        Drawing.Rectangle rcChild = 
			            (horizontalRows)
                        ? new Drawing.Rectangle((int)left, (int)top, right-(int)left, bottom-(int)top)
                        : new Drawing.Rectangle((int)top, (int)left, bottom-(int)top, right-(int)left);
			
			        RecurseDrawGraph(child, rcChild);

                    if (bStart)
                    {
                        graphics.DrawRectangle(new Drawing.Pen(Drawing.Color.Black, 2), rcChild);
                    }
                    
                    if (lastChild)
			        {
				        i++;
                        c++;

				        if (i < childrenPerRow[row])
                            ((NodeDatum)listChildren[c].Tag).TreeMapRect = new Drawing.Rectangle(-1, -1, -1, -1);
				
				        c+= childrenPerRow[row] - i;
				        break;
			        }

			        left= fRight;
		        }
                // This asserts due to rounding error: MBox.Assert(1302.3306, left == (horizontalRows ? rc.Right : rc.Bottom));
		        top= fBottom;
	        }
            // This asserts due to rounding error: MBox.Assert(1302.3307, top == (horizontalRows ? rc.Bottom : rc.Right));
            return true;
        }

        double KDirStat_CalcutateNextRow(TreeNode parent, int nextChild, double width, ref int childrenUsed, double[] arrChildWidth,
            List<TreeNode> listChildren)
        {
            const double _minProportion = 0.4;
            MBox.Assert(1302.3308, _minProportion < 1);

            MBox.Assert(1302.3309, nextChild < listChildren.Count);
            MBox.Assert(1302.33101, width >= 1.0);

	        double mySize= (double)((NodeDatum)parent.Tag).nTotalLength;
	        ulong sizeUsed= 0;
	        double rowHeight= 0;
            int i = 0;

            for (i = nextChild; i < listChildren.Count; i++)
	        {
                ulong childSize = ((NodeDatum)listChildren[i].Tag).nTotalLength;
		        sizeUsed+= childSize;
		        double virtualRowHeight= sizeUsed / mySize;
                MBox.Assert(1302.3311, virtualRowHeight > 0);
                MBox.Assert(1302.3312, virtualRowHeight <= 1);
		
		        // Drawing.Rectangle(mySize)    = width * 1.0
		        // Drawing.Rectangle(childSize) = childWidth * virtualRowHeight
		        // Drawing.Rectangle(childSize) = childSize / mySize * width

		        double childWidth= childSize / mySize * width / virtualRowHeight;

		        if (childWidth / virtualRowHeight < _minProportion)
		        {
                    MBox.Assert(1302.3313, i > nextChild); // because width >= 1 and _minProportion < 1.
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

            MBox.Assert(1302.3314, i > nextChild);

	        // Now i-1 is the last child used
	        // and rowHeight is the height of the row.

	        childrenUsed= i - nextChild;

	        // Now as we know the rowHeight, we compute the widths of our children.
	        for (i=0; i < childrenUsed; i++)
	        {
		        // Drawing.Rectangle(1.0 * 1.0) = mySize
		        double rowSize= mySize * rowHeight;
                double childSize = (double)((NodeDatum)listChildren[nextChild + i].Tag).nTotalLength;
		        double cw= childSize / rowSize;
                MBox.Assert(1302.3315, cw >= 0);
		        arrChildWidth[nextChild + i]= cw;
	        }

	        return rowHeight;
        }
    }
}
