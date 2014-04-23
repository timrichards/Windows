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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Windows.Forms;
using System.IO;
using System.Threading;

namespace SearchDirLists
{
    delegate void DrawTreeMapDelegate(BufferedGraphics bg);

    struct Options
	{
        internal Options (
		    bool  grid_in,
		    Color gridColor_in,
		    double brightness_in,
		    double height_in
        ) {
            grid= grid_in;
            gridColor = gridColor_in;
            brightness = brightness_in;
            height = height_in;
        }

		internal readonly bool  grid;				// Whether or not to draw grid lines
		internal readonly Color gridColor;		// Color of grid lines
		internal double brightness;		        // 0..1.0	(default = 0.84)
		internal double height;			        // 0..oo	(default = 0.40)	Factor "H"
	}

    class ColorSpace
    {
        internal static void NormalizeColor(ref int red, ref int green, ref int blue)
        {
	        Debug.Assert(red + green + blue <= 3 * 255);

	        if (red > 255)
	        {
		        DistributeFirst(ref red, ref green, ref blue);
	        }
	        else if (green > 255)
	        {
		        DistributeFirst(ref green, ref red, ref blue);
	        }
	        else if (blue > 255)
	        {
		        DistributeFirst(ref blue, ref red, ref green);
	        }
        }

        static void DistributeFirst(ref int first, ref int second, ref int third)
        {
            {
	            int h= (first - 255) / 2;
	            first= 255;
	            second+= h;
	            third+= h;
            }

	        if (second > 255)
	        {
		        int h= second - 255;
		        second= 255;
		        third+= h;
		        Debug.Assert(third <= 255);
	        }
	        else if (third > 255)
	        {
		        int h= third - 255;
		        third= 255;
		        second+= h;
		        Debug.Assert(second <= 255);
	        }
        }
    }

    class TreeMap : Utilities
    {
        Options m_options;

        Options _defaultOptions = new Options(
	        false,
	        Color.FromArgb(0,0,0),
	        0.88,
	        0.38);

        Options _defaultOptionsOld = new Options(
	        false,
	        Color.FromArgb(0,0,0),
	        0.85,
	        0.4);

        Options GetDefaultOptions()
        {
	        return _defaultOptions;
        }

        Options GetOldDefaultOptions()
        {
	        return _defaultOptionsOld;
        }

        internal TreeMap()
        {
	        SetOptions(_defaultOptions);
        }

        void SetOptions(Options options)
        {
	        m_options= options;
        }

        Options GetOptions()
        {
	        return m_options;
        }

        TreeNode m_rootNode = null;

        internal TreeNode DrawTreemap(Graphics graphics, Rectangle rc, TreeNode root, Options? options = null)
        {
            m_rootNode = root;

            if (options != null)
            {
                SetOptions(options.Value);
            }

	        if (rc.Width <= 0 || rc.Height <= 0)
            {
                Debug.Assert(false);
		        return null;
            }

	        if (m_options.grid)
	        {
                lock (graphics)
                {
                    graphics.FillRectangle(new SolidBrush(m_options.gridColor), rc);
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

	        if (((NodeDatum)root.Tag).nTotalLength > 0)
	        {
		        double[] surface = new double[4];

		        return RecurseDrawGraph(graphics, root, rc, bStart: true);

        #if _DEBUG
		        for (int x=rc.Left; x < rc.Right - m_options.grid; x++)
		        for (int y=rc.Top; y < rc.Bottom - m_options.grid; y++)
			        Debug.Assert(FindItemByPoint(root, CPoint(x, y)) != null);
        #endif

	        }
	        else
	        {
                lock (graphics)
                {
                    graphics.FillRectangle(Brushes.Black, rc);
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
	        Debug.Assert(((NodeDatum)item.Tag).nTotalLength > 0);

	        ((NodeDatum)item.Tag).TreeMapRect = rc;

	        int gridWidth= m_options.grid ? 1 : 0;

	        if (rc.Width <= gridWidth || rc.Height <= gridWidth)
	        {
		        return null;
	        }

            bool bDoFileList = false;

	        if (item.Nodes.Count == 0)
	        {
                if (bStart == false)
                {
                    RenderLeaf(graphics, item);
                    return null;
                }

                bDoFileList = true;
                item = DoFileList(item);

                if (item == null)
                {
                    return null;
                }
	        }
	        
		    Debug.Assert(item.Nodes.Count > 0);
		    Debug.Assert(((NodeDatum)item.Tag).nTotalLength > 0);

            KDirStat_DrawChildren(graphics, item);

            if (bDoFileList)
            {
                return item;    // the file list as a fake cloned treenode for tooltip search
            }

            return null;
        }

        TreeNode DoFileList(TreeNode parent)
        {
            TreeNode treeNode = (TreeNode)parent.Clone();

            TreeNode rootNode = TreeSelect.GetParentRoot(parent);
            String strFile = (String)((RootNodeDatum)rootNode.Tag).StrFile;

            if (File.Exists(strFile) == false)
            {
                Debug.Assert(false);
                return null;
            }

            if ((parent.Tag is NodeDatum) == false)
            {
                return null;
            }

            NodeDatum nodeDatum = (NodeDatum)parent.Tag;

            if (nodeDatum.nLineNo <= 0)
            {
                return null;
            }

            long nPrevDir = nodeDatum.nPrevLineNo;
            long nLineNo = nodeDatum.nLineNo;

            if (nPrevDir <= 0)
            {
                return null;
            }

            if ((nLineNo - nPrevDir) <= 1)  // dir has no files
            {
                return null;
            }

            DateTime dtStart = DateTime.Now;
            List<String> listLines = File.ReadLines(strFile)
                .Skip((int)nPrevDir)
                .Take((int)(nLineNo - nPrevDir - 1))
                .ToList();

            if (listLines.Count <= 0)
            {
                return null;
            }

            foreach (String strFileLine in listLines)
            {
                String[] strArrayFiles = strFileLine.Split('\t').Skip(3).ToArray();
                ulong nLength = 0;

                if ((strArrayFiles.Length > nColLENGTH_LV) && StrValid(strArrayFiles[nColLENGTH_LV]))
                {
                    nLength = ulong.Parse(strArrayFiles[nColLENGTH_LV]);
                    strArrayFiles[nColLENGTH_LV] = FormatSize(strArrayFiles[nColLENGTH_LV]);
                }

                NodeDatum nodeDatum_A = new NodeDatum(0, 0, 0);
                TreeNode treeNode_A = new TreeNode(strArrayFiles[0]);

                nodeDatum_A.nTotalLength = nLength;
                treeNode_A.Tag = nodeDatum_A;
                treeNode_A.ForeColor = Color.OliveDrab;
                treeNode.Nodes.Add(treeNode_A);
            }

            return treeNode;
        }       

         //My first approach was to make this member pure virtual and have three
         //classes derived from CTreemap. The disadvantage is then, that we cannot
         //simply have a member variable of type CTreemap but have to deal with
         //pointers, factory methods and explicit destruction. It's not worth.

         //I learned this squarification style from the KDirStat executable.
         //It's the most complex one here but also the clearest, imho.
        
        void KDirStat_DrawChildren(Graphics graphics, TreeNode parent)
        {
	        Debug.Assert(parent.Nodes.Count > 0);

	        Rectangle rc= ((NodeDatum)parent.Tag).TreeMapRect;

	        List<double> rows = new List<double>();	// Our rectangle is divided into rows, each of which gets this height (fraction of total height).
	        List<int> childrenPerRow = new List<int>();// childrenPerRow[i] = # of children in rows[i]
            List<TreeNode> listChildren = parent.Nodes.Cast<TreeNode>().Where(t => ((NodeDatum)t.Tag).nTotalLength > 0).ToList();

            listChildren.Sort((x, y) => ((NodeDatum)y.Tag).nTotalLength.CompareTo(((NodeDatum)x.Tag).nTotalLength));
            listChildren = listChildren.ToList();

            double[] childWidth = // Widths of the children (fraction of row width).
             new Double[listChildren.Count];

	        bool horizontalRows= KDirStat_ArrangeChildren(parent, childWidth, rows, childrenPerRow, listChildren);

	        int width= horizontalRows ? rc.Width : rc.Height;
	        int height= horizontalRows ? rc.Height : rc.Width;
	        Debug.Assert(width >= 0);
	        Debug.Assert(height >= 0);

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

			        #if _DEBUG
			        if (rcChild.Width > 0 && rcChild.Height > 0)
			        {
				        Rectangle test;
				        test.IntersectRect(((NodeDatum)parent.Tag).TreeMapRect, rcChild);
				        Debug.Assert(test == rcChild);
			        }
			        #endif			
			
			        RecurseDrawGraph(graphics, child, rcChild);

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
        }


        // return: whether the rows are horizontal.
        //
        bool KDirStat_ArrangeChildren(
	        TreeNode parent,
	        double[] childWidth,
	        List<double> rows,
	        List<int> childrenPerRow,
            List<TreeNode> listChildren
        )
        {
            NodeDatum nodeDatum = (NodeDatum)parent.Tag;

            if (nodeDatum.nTotalLength == 0)
	        {
		        rows.Add(1.0);
                childrenPerRow.Add(listChildren.Count);
                for (int i = 0; i < listChildren.Count; i++)
                    childWidth[i] = 1.0 / listChildren.Count;
		        return true;
	        }

            bool horizontalRows = (nodeDatum.TreeMapRect.Width >= nodeDatum.TreeMapRect.Height);

	        double width= 1.0;
	        if (horizontalRows)
	        {
                if (nodeDatum.TreeMapRect.Height > 0)
                    width = (double)nodeDatum.TreeMapRect.Width / nodeDatum.TreeMapRect.Height;
	        }
	        else
	        {
                if (nodeDatum.TreeMapRect.Width > 0)
                    width = (double)nodeDatum.TreeMapRect.Height / nodeDatum.TreeMapRect.Width;
	        }
            
            int nextChild = 0;
            while (nextChild < listChildren.Count)
	        {
		        int childrenUsed = 0;

		        rows.Add(KDirStat_CalcutateNextRow(parent, nextChild, width, ref childrenUsed, childWidth, listChildren));
		        childrenPerRow.Add(childrenUsed);
		        nextChild+= childrenUsed;
	        }

	        return horizontalRows;
        }

        double KDirStat_CalcutateNextRow(TreeNode parent, int nextChild, double width, ref int childrenUsed, double[] arrChildWidth
            , List<TreeNode> listChildren)
        {
	        const double _minProportion = 0.4;
	        Debug.Assert(_minProportion < 1);

            Debug.Assert(nextChild < listChildren.Count);
	        Debug.Assert(width >= 1.0);

	        double mySize= (double)((NodeDatum)parent.Tag).nTotalLength;

	        Debug.Assert(mySize > 0);

	        ulong sizeUsed= 0;
	        double rowHeight= 0;
            int i = 0;

            for (i = nextChild; i < listChildren.Count; i++)
	        {
                ulong childSize = ((NodeDatum)listChildren[i].Tag).nTotalLength;
		        if (childSize == 0)
		        {
			        Debug.Assert(i > nextChild);	// first child has size > 0
			        break;
		        }

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

	        // We add the rest of the children, if their size is 0.
            while (i < listChildren.Count && ((NodeDatum)listChildren[i].Tag).nTotalLength == 0)
            {
		        i++;
            }

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

        void RenderLeaf(Graphics graphics, TreeNode item)
        {
	        Rectangle rc= ((NodeDatum)item.Tag).TreeMapRect;

	        if (m_options.grid)
	        {
                rc.Offset(1, 1);

                if (rc.Width <= 0 || rc.Height <= 0)
			        return;
	        }

            DrawCushion(graphics, rc, (item.ForeColor == Color.Empty) ? Color.SandyBrown : item.ForeColor);
        }

        void DrawCushion(Graphics graphics, Rectangle rc, Color col)
        {
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
                    catch (System.InvalidOperationException)
                    {
                        Thread.Sleep(10);
                    }
                }
            }
        }
    }

    class DrawTreeMap : IDisposable
    {
        BufferedGraphics m_bg = null;
        Size m_size;
        Graphics m_graphics;
        Rectangle m_rect;
        DrawTreeMapDelegate m_callback = null;
        TreeNode m_treeNode = null;
        TreeNode m_fileNode = null;
        TreeNode m_prevNode = null;
        ToolTip m_toolTip = null;
        Thread m_thread = null;

        public void Dispose()
        {
            if (m_bg != null)
            {
                m_bg.Dispose();
            }

            if (m_toolTip != null)
            {
                m_toolTip.Dispose();
            }
        }

        internal DrawTreeMap(DrawTreeMapDelegate callback, TreeNode treeNode)
        {
            m_callback = callback;
            m_treeNode = treeNode;
        }

        internal void Go()
        {
            lock (this)
            {
                if (m_bg == null)
                {
                    BufferedGraphicsContext bgcontext = BufferedGraphicsManager.Current;

                    bgcontext.MaximumBuffer = m_size;
                    m_bg = bgcontext.Allocate(m_graphics, m_rect);
                }

                m_fileNode = new TreeMap().DrawTreemap(m_bg.Graphics, m_rect, m_treeNode);
                m_callback(m_bg);
            }
        }

        internal DrawTreeMap DoThreadFactory(PictureBox pictureBox)
        {
            m_size = pictureBox.ClientRectangle.Size;
            m_graphics = pictureBox.CreateGraphics();
            m_rect = pictureBox.ClientRectangle;

            if ((m_thread != null) && m_thread.IsAlive)
            {
                m_thread.Abort();
            }

            m_thread = new Thread(new ThreadStart(Go));
            m_thread.IsBackground = true;
            m_thread.Start();
            return this;
        }

        TreeNode FindMapNode(TreeNode treeNode_in, Point pt, bool bNextNode = true)
        {
            TreeNode treeNode = treeNode_in;

            do
            {
                NodeDatum nodeDatum = (NodeDatum)treeNode.Tag;

                if (nodeDatum.TreeMapRect.Contains(pt) == false)
                {
                    continue;
                }

                if (treeNode != treeNode_in)
                {
                    return treeNode;
                }

                if ((treeNode.Nodes == null) || (treeNode.Nodes.Count == 0))
                {
                    continue;
                }

                TreeNode foundNode = FindMapNode(treeNode.Nodes[0], pt);

                if (foundNode != null)
                {
                    return foundNode;
                }
            }
            while (bNextNode && ((treeNode = treeNode.NextNode) != null));

            return null;
        }

        internal void DoToolTip(PictureBox pictureBox, Point pt)
        {
            if (m_bg != null)
            {
                m_bg.Render();
            }

            TreeNode treeNode = m_prevNode;

            if (m_fileNode != null)
            {
                Debug.Assert(m_fileNode.Text == treeNode.Text);
                treeNode = m_fileNode;
                m_prevNode = null;
            }

            if ((Cursor.Current == Cursors.Arrow) && (m_prevNode != null))        // hack: clicked in tooltip
            {
                m_toolTip.Active = false;
                m_prevNode.TreeView.SelectedNode = m_prevNode;
                return;
            }

            m_prevNode = FindMapNode(m_prevNode ?? treeNode, pt);

            if (m_prevNode == null)
            {
                m_prevNode = FindMapNode(treeNode, pt);
            }

            if (m_prevNode == null)
            {
                return;
            }

            if (m_toolTip == null)
            {
                m_toolTip = new ToolTip();
                m_toolTip.UseFading = true;
            }

            NodeDatum nodeDatum = (NodeDatum)m_prevNode.Tag;

            m_toolTip.Active = true;
            m_toolTip.ToolTipTitle = m_prevNode.Text;
            m_toolTip.Show(Utilities.FormatSize(nodeDatum.nTotalLength, bBytes: true), pictureBox, pt);

            Rectangle rc = nodeDatum.TreeMapRect;

            rc.Inflate(-rc.Width/2 + 1, -rc.Height/2 + 1);
            ControlPaint.DrawSelectionFrame(pictureBox.CreateGraphics(), true, nodeDatum.TreeMapRect, rc, Color.White);
        }

        internal void ClearToolTip()
        {
            if (m_toolTip != null)
            {
                m_toolTip.Dispose();
                m_toolTip = null;
            }
        }
    }

    partial class Form1
    {
        DrawTreeMap m_DrawTreeMap = null;
    }
}