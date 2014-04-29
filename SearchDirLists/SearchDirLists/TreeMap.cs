using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;

namespace SearchDirLists
{
    class TreeMapUserControl : UserControl
    {
        public Control TooltipAnchor;

        bool m_bGrid = false;
        Rectangle m_rectBitmap = Rectangle.Empty;
        Rectangle m_selRect = Rectangle.Empty;
        Rectangle m_rectCenter = Rectangle.Empty;
        SizeF m_sizeTranslate = SizeF.Empty;
        BufferedGraphics m_bg = null;
        TreeNode m_treeNode = null;
        TreeNode m_fileNode = null;
        TreeNode m_prevNode = null;
        TreeNode m_deepNode = null;
        TreeNode m_deepNodeDrawn = null;
        System.Threading.Timer m_timerAnim = null;
        int m_nAnimFrame = 0;
        DateTime m_dtHideGoofball = DateTime.MinValue;
        ToolTip m_toolTip = new ToolTip();
        Thread m_thread = null;

        void TimerAnim_Callback(object state)
        {
            if (m_rectCenter != Rectangle.Empty)
            {
                Invalidate(m_rectCenter);
            }
        }

        public TreeMapUserControl()
        {
            m_toolTip.UseFading = true;
            m_toolTip.UseAnimation = true;
            m_timerAnim = new System.Threading.Timer(TimerAnim_Callback, null, 33, 33);    // 30 FPS
            this.SetStyle(ControlStyles.DoubleBuffer |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint,
                true);
        }

        internal void Clear()
        {
            m_treeNode = null;
            m_fileNode = null;
            m_prevNode = null;
            m_deepNode = null;
            m_deepNodeDrawn = null;
            m_toolTip.Tag = null;
        }

        internal void ClearSelection()
        {
            m_toolTip.Hide(TooltipAnchor ?? this);
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
            m_toolTip = null;
            base.Dispose(disposing);
        }

        TreeNode GetFileList(TreeNode parent)
        {
            List<ulong> listLength = new List<ulong>();
            List<String[]> listFiles = TreeSelect.GetFileList(parent, listLength);

            if (listFiles == null)
            {
                return null;
            }

            TreeNode nodeFileList = new TreeNode(parent.Text);
            ulong nTotalLength = 0;
            List<ulong>.Enumerator iterUlong = listLength.GetEnumerator();

            foreach (String[] arrLine in listFiles)
            {
                Debug.Assert(iterUlong.MoveNext());
                NodeDatum nodeDatum_A = new NodeDatum();

                nTotalLength += nodeDatum_A.nTotalLength = iterUlong.Current;

                if (iterUlong.Current <= 0)
                {
                    continue;
                }

                TreeNode nodeFile = new TreeNode(arrLine[0]);

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

            Debug.Assert(nTotalLength == nodeDatum.nLength);
            nodeDatum_B.nTotalLength = nTotalLength;
            nodeDatum_B.TreeMapRect = nodeDatum.TreeMapRect;
            nodeFileList.Tag = nodeDatum_B;
            return nodeFileList;
        }

        internal void DoThreadFactory(TreeNode treeNode)
        {
            if ((m_deepNode == null) || (m_deepNode.IsChildOf(treeNode) == false))
            {
                m_deepNode = treeNode;
            }

            m_treeNode = treeNode;
            DoThreadFactory();
            m_dtHideGoofball = DateTime.MinValue;
        }

        internal void DoThreadFactory()
        {
            ClearSelection();

            if (m_treeNode == null)
            {
                return;
            }

            if ((m_thread != null) && m_thread.IsAlive)
            {
                m_thread.Abort();
            }

            m_thread = new Thread(new ThreadStart(Go));
            m_thread.IsBackground = true;
            m_thread.Start();
        }

        internal String Tooltip_Click()
        {
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

        internal TreeNode DoToolTip(Point pt_in)
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

            Point pt = Point.Ceiling(new PointF(pt_in.X / m_sizeTranslate.Width, pt_in.Y / m_sizeTranslate.Height));
            TreeNode nodeRet = null;
            bool bImmediateFiles = false;

            do
            {
                if ((nodeRet = FindMapNode(((NodeDatum)m_treeNode.Tag).TreeMapFiles, pt)) != null)
                {
                    bImmediateFiles = true;
                    break;
                }

                TreeNode treeNode = m_treeNode;

                if (m_fileNode != null)
                {
                    Debug.Assert(m_fileNode.Text == m_treeNode.Text);
                    treeNode = m_fileNode;
                    m_prevNode = null;
                }

                TreeNode m_prevNode_A = m_prevNode ?? treeNode;

                if ((nodeRet = FindMapNode(m_prevNode_A, pt)) != null)
                {
                    break;
                }

                TreeNode nodeUplevel = (m_prevNode != null) ? m_prevNode.Parent : null;
                bool bFoundUplevel = false;

                while (nodeUplevel != null)
                {
                    if ((nodeRet = FindMapNode(nodeUplevel, pt)) != null)
                    {
                        bFoundUplevel = true;
                        break;
                    }

                    nodeUplevel = nodeUplevel.Parent;
                }

                if (bFoundUplevel)
                {
                    break;
                }

                if ((nodeRet = FindMapNode(m_treeNode, pt)) != null)
                {
                    break;
                }

                nodeRet = m_treeNode;
            }
            while (false);

            if (bImmediateFiles == false)
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

            NodeDatum nodeDatum = (NodeDatum)nodeRet.Tag;

            m_selRect = nodeDatum.TreeMapRect;
            m_toolTip.Show(Utilities.FormatSize(nodeDatum.nTotalLength, bBytes: true), TooltipAnchor, new Point(0, 0));
            m_prevNode = nodeRet;
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

                if ((treeNode.Nodes == null) || (treeNode.Nodes.Count == 0))
                {
                    continue;
                }

                TreeNode foundNode = FindMapNode(treeNode.Nodes[0], pt, bNextNode: true);

                if (foundNode != null)
                {
                    return foundNode;
                }
            }
            while (bNextNode && ((treeNode = treeNode.NextNode) != null));

            return null;
        }

        internal void Go()
        {
            lock (this)
            {
                m_bg.Graphics.Clear(Color.DarkGray);
                m_fileNode = DrawTreemap(m_bg.Graphics, m_rectBitmap);
                m_bg.Graphics.DrawRectangle(new Pen(Brushes.Black, 10), m_rectBitmap);
                m_bg.Render();
                m_selRect = Rectangle.Empty;
                m_prevNode = null;
                Invalidate();
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if ((Site != null) && Site.DesignMode)    // prevent bitmap from getting into the pesky resx
            {
                BackColor = Color.Turquoise;          // litmus
                return;
            }

            BackColor = Color.Empty;
            m_rectBitmap = new Rectangle(0, 0, 1024, 1024);
            BackgroundImage = new Bitmap(m_rectBitmap.Size.Width, m_rectBitmap.Size.Height);

            BufferedGraphicsContext bgcontext = BufferedGraphicsManager.Current;

            bgcontext.MaximumBuffer = m_rectBitmap.Size;
            m_bg = bgcontext.Allocate(Graphics.FromImage(BackgroundImage), m_rectBitmap);
            TranslateSize();
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
            brush.CenterColor = Color.White;
            brush.SurroundColors = new Color[] { Color.Black };
            r.Inflate(-r.Width / 5, -r.Height / 5);

            Rectangle r_A = Rectangle.Ceiling(r);
            int nAnimFrame = (m_nAnimFrame %= 6) * 30;

            e.Graphics.FillPie(brush, r_A, 90 + nAnimFrame, 90);
            e.Graphics.FillPie(brush, r_A, 270 + nAnimFrame, 90);
            brush.CenterColor = Color.Black;
            brush.SurroundColors = new Color[] { Color.FromArgb(128, 255, 255, 255) };
            e.Graphics.FillPie(brush, r_A, 0 + nAnimFrame, 90);
            e.Graphics.FillPie(brush, r_A, 180 + nAnimFrame, 90);
            ++m_nAnimFrame;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            TranslateSize();
            m_prevNode = null;
            ClearSelection();
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
        
        internal TreeNode DrawTreemap(Graphics graphics, Rectangle rc, bool bGrid = false)
        {
            m_deepNodeDrawn = null;
            m_bGrid = bGrid;

	        if (bGrid)
	        {
                lock (graphics)
                {
                    graphics.FillRectangle(new SolidBrush(Color.Black), rc);
                }
	        }
	        else
	        {
		        // We shrink the rectangle here, too.
		        // If we didn't do this, the layout of the treemap would
		        // change, when grid is switched on and off.
                Pen pen = new Pen(SystemColors.ButtonShadow, 1);
                lock (graphics)
                {
                    { int nLine = rc.Right - 1; graphics.DrawLine(pen, new Point(nLine, rc.Top), new Point(nLine, rc.Bottom)); }
                    { int nLine = rc.Bottom - 1; graphics.DrawLine(pen, new Point(rc.Left, nLine), new Point(rc.Right, nLine)); }
                }
	        }

	        rc.Width--;
	        rc.Height--;

	        if (rc.Width <= 0 || rc.Height <= 0)
            {
		        return null;
            }

            NodeDatum nodeDatum = (NodeDatum)m_treeNode.Tag;

            if (nodeDatum.nTotalLength > 0)
	        {
                return RecurseDrawGraph(graphics, m_treeNode, rc, bStart: true);
	        }
	        else
	        {
                lock (graphics)
                {
                    graphics.FillRectangle(Brushes.Wheat, rc);
                }
	        }

            return null;
        }

        TreeNode RecurseDrawGraph(
	        Graphics graphics,
	        TreeNode item, 
	        Rectangle rc,
            bool bStart = false
        )
        {
            Debug.Assert(rc.Width >= 0);
	        Debug.Assert(rc.Height >= 0);

	        int gridWidth= m_bGrid ? 1 : 0;

	        if (rc.Width <= gridWidth || rc.Height <= gridWidth)
	        {
		        return null;
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
                return null;
            }

            if (m_bGrid)
            {
                rc.Offset(1, 1);

                if (rc.Width <= 0 || rc.Height <= 0)
                    return null;
            }

            Color col = (item.ForeColor == Color.Empty) ? Color.SandyBrown : item.ForeColor;
            GraphicsPath path = new GraphicsPath();
            Rectangle r = rc;

            r.Inflate(r.Width / 2, r.Height / 2);
            path.AddEllipse(r);

            PathGradientBrush brush = new PathGradientBrush(path);

            brush.CenterColor = Color.Wheat;
            brush.SurroundColors = new Color[] { ControlPaint.Dark(col) };

            while (true)
            {
                lock (graphics)
                {
                    try
                    {
                        graphics.FillRectangle(brush, rc);
                        break;
                    }
                    catch (InvalidOperationException)
                    {
                        Console.WriteLine("RenderLeaf() InvalidOperationException");
                        Thread.Sleep(10);
                    }
                }
            }

            return null;
        }

         //My first approach was to make this member pure virtual and have three
         //classes derived from CTreemap. The disadvantage is then, that we cannot
         //simply have a member variable of type CTreemap but have to deal with
         //pointers, factory methods and explicit destruction. It's not worth.

         //I learned this squarification style from the KDirStat executable.
         //It's the most complex one here but also the clearest, imho.
        
        bool KDirStat_DrawChildren(Graphics graphics, TreeNode parent_in, bool bStart = false)
        {
            List<TreeNode> listChildren = null;
            TreeNode parent = null;

            bool bVolumeNode = false;

            do
            {
                if ((bStart == false) || ((parent_in.Tag is RootNodeDatum) == false))
                {
                    break;
                }

                RootNodeDatum rootNodeDatum = (RootNodeDatum)parent_in.Tag;

                if (rootNodeDatum.VolumeView == false)
                {
                    break;
                }

                NodeDatum nodeDatumFree = new NodeDatum();
                TreeNode nodeFree = new TreeNode(parent_in.Text + " (free space)");

                nodeDatumFree.nTotalLength = rootNodeDatum.VolumeFree;
                nodeFree.Tag = nodeDatumFree;
                nodeFree.ForeColor = Color.MediumSpringGreen;

                NodeDatum nodeDatumUnread = new NodeDatum();
                TreeNode nodeUnread = new TreeNode(parent_in.Text + " (unread data)");         // TODO: incorporate error list
                long nUnreadLength = (long)rootNodeDatum.VolumeLength - (long)rootNodeDatum.VolumeFree - (long)rootNodeDatum.nTotalLength;
                ulong nVolumeLength = rootNodeDatum.VolumeLength;

                if (nUnreadLength < 0)
                {
                    // doesn't equal (long)rootNodeDatum.VolumeFree.
                    // compare nTotalLength to end of file length.
                    // compressed drive? hard links?

                    nVolumeLength = rootNodeDatum.VolumeFree + rootNodeDatum.nTotalLength;
                    nUnreadLength = 0;

                    if (rootNodeDatum.VolumeFree <= 0)
                    {
                        break;
                    }
                }

                nodeDatumUnread.nTotalLength = (ulong)nUnreadLength;
                nodeUnread.Tag = nodeDatumUnread;
                nodeUnread.ForeColor = Color.MediumVioletRed;
                listChildren = new List<TreeNode>();
                listChildren.Add(parent_in);                            // parent added as child
                listChildren.Add(nodeFree);

                if (nUnreadLength > 0)
                {
                    listChildren.Add(nodeUnread);
                }

                parent = new TreeNode(parent_in.Text + " (volume)");       // parent reassigned as volume
                NodeDatum nodeDatumVolume = new NodeDatum();
                nodeDatumVolume.nTotalLength = nVolumeLength;
                nodeDatumVolume.TreeMapRect = ((NodeDatum)parent_in.Tag).TreeMapRect;
                parent.Tag = nodeDatumVolume;
                bVolumeNode = true;
                rootNodeDatum.VolumeView = true;
            }
            while (false);

            if (bVolumeNode == false)
            {
                parent = parent_in;
                listChildren = parent.Nodes.Cast<TreeNode>().Where(t => ((NodeDatum)t.Tag).nTotalLength > 0).ToList();
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

                TreeNode treeNode = new TreeNode(parent.Text);

                treeNode.Tag = nodeFiles;
                treeNode.ForeColor = Color.OliveDrab;
                listChildren.Add(treeNode);
            }

            listChildren.Sort((x, y) => ((NodeDatum)y.Tag).nTotalLength.CompareTo(((NodeDatum)x.Tag).nTotalLength));

            if (listChildren.Count == 0)
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
			        Debug.Assert(childWidth[c] >= 0);
			        double fRight= left + childWidth[c] * width;
			        int right= (int)fRight;

			        bool lastChild = (i == childrenPerRow[row] - 1 || childWidth[c + 1] == 0);

			        if (lastChild)
				        right= horizontalRows ? rc.Right : rc.Bottom;

			        Rectangle rcChild = 
			            (horizontalRows)
                        ? new Rectangle((int)left, (int)top, right-(int)left, bottom-(int)top)
                        : new Rectangle((int)top, (int)left, bottom-(int)top, right-(int)left);
			
			        RecurseDrawGraph(graphics, child, rcChild);

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
		        // This asserts due to rounding error: Debug.Assert(left == (horizontalRows ? rc.Right : rc.Bottom));
		        top= fBottom;
	        }
	        // This asserts due to rounding error: Debug.Assert(top == (horizontalRows ? rc.Bottom : rc.Right));
            return true;
        }

        double KDirStat_CalcutateNextRow(TreeNode parent, int nextChild, double width, ref int childrenUsed, double[] arrChildWidth,
            List<TreeNode> listChildren)
        {
            const double _minProportion = 0.4;
	        Debug.Assert(_minProportion < 1);

            Debug.Assert(nextChild < listChildren.Count);
	        Debug.Assert(width >= 1.0);

	        double mySize= (double)((NodeDatum)parent.Tag).nTotalLength;
	        ulong sizeUsed= 0;
	        double rowHeight= 0;
            int i = 0;

            for (i = nextChild; i < listChildren.Count; i++)
	        {
                ulong childSize = ((NodeDatum)listChildren[i].Tag).nTotalLength;
		        sizeUsed+= childSize;
		        double virtualRowHeight= sizeUsed / mySize;
		        Debug.Assert(virtualRowHeight > 0);
		        Debug.Assert(virtualRowHeight <= 1);
		
		        // Rectangle(mySize)    = width * 1.0
		        // Rectangle(childSize) = childWidth * virtualRowHeight
		        // Rectangle(childSize) = childSize / mySize * width

		        double childWidth= childSize / mySize * width / virtualRowHeight;

		        if (childWidth / virtualRowHeight < _minProportion)
		        {
			        Debug.Assert(i > nextChild); // because width >= 1 and _minProportion < 1.
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

	        Debug.Assert(i > nextChild);

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
		        Debug.Assert(cw >= 0);
		        arrChildWidth[nextChild + i]= cw;
	        }

	        return rowHeight;
        }
    }
}