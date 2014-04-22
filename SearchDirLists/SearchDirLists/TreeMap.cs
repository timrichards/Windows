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
using System.Diagnostics;
using System.Windows.Forms;

namespace SearchDirLists
{
	enum STYLE
	{
		SimpleStyle,		// This style is not used in WinDirStat (it's rather uninteresting).
		KDirStatStyle,		// Children are layed out in rows. Similar to the style used by KDirStat.
		SequoiaViewStyle	// The 'classical' squarification as described in at http://www.win.tue.nl/~vanwijk/.
	};

	struct Options
	{
        public Options (
		    STYLE style_in,
		    bool  grid_in,
		    Color gridColor_in,
		    double brightness_in,
		    double height_in,
		    double scaleFactor_in,
		    double ambientLight_in,
		    double lightSourceX_in,
		    double lightSourceY_in
        ) {
            style= style_in;
            grid= grid_in;
            gridColor = gridColor_in;
            brightness = brightness_in;
            height = height_in;
            scaleFactor = scaleFactor_in;
            ambientLight = ambientLight_in;
            lightSourceX = lightSourceX_in;
            lightSourceY = lightSourceY_in;
        }

		public readonly STYLE style;			// Squarification method
		public readonly bool  grid;				// Whether or not to draw grid lines
		public readonly Color gridColor;		// Color of grid lines
		public double brightness;		        // 0..1.0	(default = 0.84)
		public double height;			        // 0..oo	(default = 0.40)	Factor "H"
		public double scaleFactor;		        // 0..1.0	(default = 0.90)	Factor "F"
		public double ambientLight;	            // 0..1.0	(default = 0.15)	Factor "Ia"
		public double lightSourceX;	            // -4.0..+4.0 (default = -1.0), negative = left
		public double lightSourceY;	            // -4.0..+4.0 (default = -1.0), negative = top

		int GetBrightnessPercent()	            { return RoundDouble(brightness * 100); }
		int GetHeightPercent()		            { return RoundDouble(height * 100); }
		int GetScaleFactorPercent()	            { return RoundDouble(scaleFactor * 100); }
		int GetAmbientLightPercent()            { return RoundDouble(ambientLight * 100); }
		int GetLightSourceXPercent()            { return RoundDouble(lightSourceX * 100); }
		int GetLightSourceYPercent()            { return RoundDouble(lightSourceY * 100); }
		Point GetLightSourcePoint()             { return new Point(GetLightSourceXPercent(), GetLightSourceYPercent()); }

		void SetBrightnessPercent(int n)	    { brightness = n / 100.0; }
		void SetHeightPercent(int n)		    { height = n / 100.0; }
		void SetScaleFactorPercent(int n)	    { scaleFactor = n / 100.0; }
		void SetAmbientLightPercent(int n)	    { ambientLight = n / 100.0; }
		void SetLightSourceXPercent(int n)	    { lightSourceX = n / 100.0; }
		void SetLightSourceYPercent(int n)	    { lightSourceY = n / 100.0; }
		void SetLightSourcePoint(Point pt)      { SetLightSourceXPercent(pt.X); SetLightSourceYPercent(pt.Y); }

		int RoundDouble(double d) { return Math.Sign(d) * (int)(Math.Abs(d) + 0.5); }
	}

    class ColorSpace
    {
        // The EqualizeColors() method creates a palette with colors
        // all having the same brightness of 0.6
        // Later in RenderCushion() this number is used again to
        // scale the colors.

        double GetColorBrightness(Color color)
        {
	        return (color.R + color.G + color.B) / 255.0 / 3.0;
        }

        public static Color MakeBrightColor(Color color, double brightness)
        {
	        Debug.Assert(brightness >= 0.0);
	        Debug.Assert(brightness <= 1.0);

	        double dred= color.R / 255.0;
	        double dgreen= color.G / 255.0;
	        double dblue= color.B / 255.0;

	        double f= 3.0 * brightness / (dred + dgreen + dblue);
	        dred*= f;
	        dgreen*= f;
	        dblue*= f;

	        int red		= (int)(dred * 255);
	        int green	= (int)(dgreen * 255);
	        int blue	= (int)(dblue * 255);
	
	        NormalizeColor(ref red, ref green, ref blue);

	        return Color.FromArgb(red, green, blue);
        }

        public static void NormalizeColor(ref int red, ref int green, ref int blue)
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

        public static void DistributeFirst(ref int first, ref int second, ref int third)
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


/////////////////////////////////////////////////////////////////////////////

    class TreeMap
    {
        static double PALETTE_BRIGHTNESS = 0.6;

        const uint COLORFLAG_DARKER = 0x01000000;
	    const uint COLORFLAG_LIGHTER = 0x02000000;
	    const uint COLORFLAG_MASK	 = 0x03000000;

        Options m_options;
	    double m_Lx;			// Derived parameters
	    double m_Ly;
	    double m_Lz;

        Options _defaultOptions = new Options(
	        STYLE.KDirStatStyle,
	        false,
	        Color.FromArgb(0,0,0),
	        0.88,
	        0.38,
	        0.91,
	        0.13,
	        -1.0,
	        -1.0);

        Options _defaultOptionsOld = new Options(
            STYLE.KDirStatStyle,
	        false,
	        Color.FromArgb(0,0,0),
	        0.85,
	        0.4,
	        0.9,
	        0.15,
	        -1.0,
	        -1.0);

        Color[] _defaultCushionColors = new Color[] {
	        Color.FromArgb(0, 0, 255),
	        Color.FromArgb(255, 0, 0),
	        Color.FromArgb(0, 255, 0),
	        Color.FromArgb(0, 255, 255),
	        Color.FromArgb(255, 0, 255),
	        Color.FromArgb(255, 255, 0),
	        Color.FromArgb(150, 150, 255),
	        Color.FromArgb(255, 150, 150),
	        Color.FromArgb(150, 255, 150),
	        Color.FromArgb(150, 255, 255),
	        Color.FromArgb(255, 150, 255),
	        Color.FromArgb(255, 255, 150),
	        Color.FromArgb(255, 255, 255)
        };

        void GetDefaultPalette(ref Color[] palette)
        {
		    EqualizeColors(_defaultCushionColors, _defaultCushionColors.Length, ref palette);
        }

        void EqualizeColors(Color[] colors, int count, ref Color[] palette)
        {
	        for (int i=0; i < count; i++)
	        {
		        palette[i] = ColorSpace.MakeBrightColor(colors[i], PALETTE_BRIGHTNESS);
	        }
        }

        Options GetDefaultOptions()
        {
	        return _defaultOptions;
        }

        Options GetOldDefaultOptions()
        {
	        return _defaultOptionsOld;
        }

        public TreeMap()
        {
	        SetOptions(_defaultOptions);
        }

        void SetOptions(Options options)
        {
	        m_options= options;

	        // Derive normalized vector here for performance
	        double lx = m_options.lightSourceX;			// negative = left
	        double ly = m_options.lightSourceY;			// negative = top
	        double lz = 10;

	        double len= Math.Sqrt(lx*lx + ly*ly + lz*lz);
	        m_Lx = lx / len;
	        m_Ly = ly / len;
	        m_Lz = lz / len;
        }

        Options GetOptions()
        {
	        return m_options;
        }

        // BufferedGraphics
        public void DrawTreemap(Graphics graphics, Rectangle rc, TreeNode root, Options? options = null)
        {
            if (options != null)
            {
                SetOptions(options.Value);
            }

	        if (rc.Width <= 0 || rc.Height <= 0)
            {
                Debug.Assert(false);
		        return;
            }

	        if (m_options.grid)
	        {
		        graphics.FillRectangle(new SolidBrush(m_options.gridColor), rc);
	        }
	        else
	        {
		        // We shrink the rectangle here, too.
		        // If we didn't do this, the layout of the treemap would
		        // change, when grid is switched on and off.
                Pen pen = new Pen(SystemColors.ButtonShadow, 1);
                { int nLine = rc.Right - 1; graphics.DrawLine(pen, new Point(nLine, rc.Top), new Point(nLine, rc.Bottom)); }
                { int nLine = rc.Bottom - 1; graphics.DrawLine(pen, new Point(rc.Left, nLine), new Point(rc.Right, nLine)); }
	        }

	        rc.Width--;
	        rc.Height--;

	        if (rc.Width <= 0 || rc.Height <= 0)
            {
		        return;
            }

	        if (((NodeDatum)root.Tag).nTotalLength > 0)
	        {
		        double[] surface = new double[4];

		        RecurseDrawGraph(graphics, root, rc, true, surface, m_options.height, 0);

        #if _DEBUG
		        for (int x=rc.Left; x < rc.Right - m_options.grid; x++)
		        for (int y=rc.Top; y < rc.Bottom - m_options.grid; y++)
			        Debug.Assert(FindItemByPoint(root, CPoint(x, y)) != null);
        #endif

	        }
	        else
	        {
		        graphics.FillRectangle(Brushes.Black, rc);
	        }
        }

        TreeNode FindItemByPoint(TreeNode item, Point point)
        {
	        Debug.Assert(item != null);

	        Rectangle rc = ((NodeDatum)item.Tag).TreeMapRect;

	        if (rc.Contains(point) == false)
	        {
		        // The only case that this function returns null is that
		        // point is not inside the rectangle of item.
		        //
		        // Take notice of
		        // (a) the very right an bottom lines, which can be "grid" and
		        //     are not covered by the root rectangle,
		        // (b) the fact, that WM_MOUSEMOVEs can occur after WM_SIZE but
		        //     before WM_PAINT.
		        //
		        return null;
	        }

	        Debug.Assert(rc.Contains(point));

	        TreeNode ret = null;

	        int gridWidth = m_options.grid ? 1: 0;

	        if (rc.Width <= gridWidth || rc.Height <= gridWidth)
	        {
		        ret= item;
	        }
	        else if (item.Nodes.Count == 0)
	        {
		        ret= item;
	        }
	        else
	        {
		        Debug.Assert(((NodeDatum)item.Tag).nTotalLength > 0);
		        Debug.Assert(item.Nodes.Count > 0);

		        for (int i=0; i < item.Nodes.Count; i++)
		        {
			        TreeNode child= item.Nodes[i];

			        Debug.Assert(((NodeDatum)child.Tag).nTotalLength > 0);

        #if _DEBUG
			        Rectangle rcChild= ((NodeDatum)child.Tag).TreeMapRect;
			        Debug.Assert(rcChild.Right >= rcChild.Left);
			        Debug.Assert(rcChild.Bottom >= rcChild.Top);
			        Debug.Assert(rcChild.Left >= rc.Left);
			        Debug.Assert(rcChild.Right <= rc.Right);
			        Debug.Assert(rcChild.Top >= rc.Top);
			        Debug.Assert(rcChild.Bottom <= rc.Bottom);
        #endif
			        if (((NodeDatum)child.Tag).TreeMapRect.Contains(point))
			        {
				        ret= FindItemByPoint(child, point);
				        Debug.Assert(ret != null);
        #if STRONGDEBUG
        #if _DEBUG
				        for (i++; i < item.Nodes.Count; i++)
				        {
					        child= item.Nodes[i);
					
					        if (((NodeDatum)child.Tag).nTotalLength == 0)
						        break;

					        rcChild= ((NodeDatum)child.Tag).TreeMapRect;
					        if (rcChild.Left == -1)
					        {
						        Debug.Assert(rcChild.Top == -1);
						        Debug.Assert(rcChild.Right == -1);
						        Debug.Assert(rcChild.Bottom == -1);
						        break;
					        }
					
					        Debug.Assert(rcChild.Right >= rcChild.Left);
					        Debug.Assert(rcChild.Bottom >= rcChild.Top);
					        Debug.Assert(rcChild.Left >= rc.Left);
					        Debug.Assert(rcChild.Right <= rc.Right);
					        Debug.Assert(rcChild.Top >= rc.Top);
					        Debug.Assert(rcChild.Bottom <= rc.Bottom);
				        }
        #endif
        #endif

				        break;
			        }
		        }
	        }

	        Debug.Assert(ret != null);

	        if (ret == null)
	        {
		        ret= item;
	        }

	        return ret;
        }

        void DrawColorPreview(Graphics graphics, Rectangle rc, Color color, Options options)
        {
		    double[] surface = new double[4];

	        AddRidge(rc, surface, m_options.height * m_options.scaleFactor);

            RenderRectangle(graphics, rc, surface, color);

	        if (m_options.grid)
	        {
                graphics.DrawRectangle(new Pen(m_options.gridColor, 1), rc);
	        }
        }

        void RecurseDrawGraph(
	        Graphics graphics,
	        TreeNode item, 
	        Rectangle rc,
	        bool asroot,
	        double[] psurface,
	        double h,
	        uint flags
        )
        {
	        Debug.Assert(rc.Width >= 0);
	        Debug.Assert(rc.Height >= 0);
	        Debug.Assert(((NodeDatum)item.Tag).nTotalLength > 0);

	        ((NodeDatum)item.Tag).TreeMapRect = rc;

	        int gridWidth= m_options.grid ? 1 : 0;

	        if (rc.Width <= gridWidth || rc.Height <= gridWidth)
	        {
		        return;
	        }

	        double[] surface = new double[4];

            Debug.Assert(psurface.Length == 4);

	        if (IsCushionShading())
	        {
		        for (int i=0; i < 4; i++)
                {
			        surface[i]= psurface[i];
                }

		        if (!asroot)
                {
			        AddRidge(rc, surface, h);
                }
	        }

	        if (item.Nodes.Count == 0)
	        {
		        RenderLeaf(graphics, item, surface);
	        }
	        else
	        {
		        Debug.Assert(item.Nodes.Count > 0);
		        Debug.Assert(((NodeDatum)item.Tag).nTotalLength > 0);

		        DrawChildren(graphics, item, surface, h, flags);
	        }
        }

        // My first approach was to make this member pure virtual and have three
        // classes derived from CTreemap. The disadvantage is then, that we cannot
        // simply have a member variable of type CTreemap but have to deal with
        // pointers, factory methods and explicit destruction. It's not worth.

        void DrawChildren(
	        Graphics graphics, 
	        TreeNode parent, 
	        double[] surface,
	        double h,
	        uint flags
        )
        {
	        switch (m_options.style)
	        {
	        case STYLE.KDirStatStyle:
		        KDirStat_DrawChildren(graphics, parent, surface, h, flags);
		        break;

	        case STYLE.SequoiaViewStyle:
		        SequoiaView_DrawChildren(graphics, parent, surface, h, flags);
		        break;

	        case STYLE.SimpleStyle:
		        Simple_DrawChildren(graphics, parent, surface, h, flags);
		        break;
	        }
        }


        // I learned this squarification style from the KDirStat executable.
        // It's the most complex one here but also the clearest, imho.
        //
        void KDirStat_DrawChildren(Graphics graphics, TreeNode parent, double[] surface, double h, uint flags)
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
			
			        RecurseDrawGraph(graphics, child, rcChild, false, surface, h * m_options.scaleFactor, 0);

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
            if (((NodeDatum)parent.Tag).nTotalLength == 0)
	        {
		        rows.Add(1.0);
                childrenPerRow.Add(listChildren.Count);
                for (int i = 0; i < listChildren.Count; i++)
                    childWidth[i] = 1.0 / listChildren.Count;
		        return true;
	        }

	        bool horizontalRows= (((NodeDatum)parent.Tag).TreeMapRect.Width >= ((NodeDatum)parent.Tag).TreeMapRect.Height);

	        double width= 1.0;
	        if (horizontalRows)
	        {
		        if (((NodeDatum)parent.Tag).TreeMapRect.Height > 0)
			        width= (double)((NodeDatum)parent.Tag).TreeMapRect.Width / ((NodeDatum)parent.Tag).TreeMapRect.Height;
	        }
	        else
	        {
		        if (((NodeDatum)parent.Tag).TreeMapRect.Width > 0)
			        width= (double)((NodeDatum)parent.Tag).TreeMapRect.Height / ((NodeDatum)parent.Tag).TreeMapRect.Width;
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


        // The classical squarification method.
        //
        void SequoiaView_DrawChildren(Graphics graphics, TreeNode parent, double[] surface, double h, uint flags)
        {
	        // Rest rectangle to fill
	        Rectangle remaining = ((NodeDatum)parent.Tag).TreeMapRect;
	
	        Debug.Assert(remaining.Width > 0);
	        Debug.Assert(remaining.Height > 0);

	        // Size of rest rectangle
	        ulong remainingSize = ((NodeDatum)parent.Tag).nTotalLength;
	        Debug.Assert(remainingSize > 0);

	        // Scale factor
	        double sizePerSquarePixel= (double)((NodeDatum)parent.Tag).nTotalLength / remaining.Width / remaining.Height;

	        // First child for next row
	        int head = 0;

	        // At least one child left
	        while (head < parent.Nodes.Count)
	        {
		        Debug.Assert(remaining.Width > 0);
		        Debug.Assert(remaining.Height > 0);

		        // How we devide the remaining rectangle 
		        bool horizontal = (remaining.Width >= remaining.Height);

		        // Height of the new row
		        int height = horizontal ? remaining.Height : remaining.Width;

		        // Square of height in size scale for ratio formula
		        double hh= (height * height) * sizePerSquarePixel;
		        Debug.Assert(hh > 0);

		        // Row will be made up of child(rowBegin)...child(rowEnd - 1)
		        int rowBegin = head;
		        int rowEnd = head;

		        // Worst ratio so far
		        double worst = double.MaxValue;

		        // Maxmium size of children in row
		        ulong rmax= ((NodeDatum)parent.Nodes[rowBegin].Tag).nTotalLength;

		        // Sum of sizes of children in row
		        ulong sum= 0;

		        // This condition will hold at least once.
		        while (rowEnd < parent.Nodes.Count)
		        {
			        // We check a virtual row made up of child(rowBegin)...child(rowEnd) here.

			        // Minimum size of child in virtual row
			        ulong rmin= ((NodeDatum)parent.Nodes[rowEnd].Tag).nTotalLength;

			        // If sizes of the rest of the children is zero, we add all of them
			        if (rmin == 0)
			        {
				        rowEnd= parent.Nodes.Count;
				        break;
			        }

			        // Calculate the worst ratio in virtual row.
			        // Formula taken from the "Squarified Treemaps" paper.
			        // (http://http://www.win.tue.nl/~vanwijk/)

			        double ss= ((double)sum + rmin) * ((double)sum + rmin);
			        double ratio1 = hh * rmax / ss;
			        double ratio2 = ss / hh / rmin;

			        double nextWorst = Math.Max(ratio1, ratio2);

			        // Will the ratio get worse?
			        if (nextWorst > worst)
			        {
				        // Yes. Don't take the virtual row, but the
				        // real row (child(rowBegin)..child(rowEnd - 1))
				        // made so far.
				        break;
			        }

			        // Here we have decided to add child(rowEnd) to the row.
			        sum+= rmin;
			        rowEnd++;

			        worst= nextWorst;
		        }

		        // Row will be made up of child(rowBegin)...child(rowEnd - 1).
		        // sum is the size of the row.

		        // As the size of parent is greater than zero, the size of
		        // the first child must have been greater than zero, too.
		        Debug.Assert(sum > 0);

		        // Width of row
		        int width = (horizontal ? remaining.Width : remaining.Height);
		        Debug.Assert(width > 0);

		        if (sum < remainingSize)
			        width= (int)((double)sum / remainingSize * width);
		        // else: use up the whole width
		        // width may be 0 here.

		        // Build the rectangles of children.
                int rc_l = 0, rc_t = 0, rc_r = 0, rc_b = 0;
		        double fBegin;

		        if (horizontal)
		        {
			        rc_l= remaining.Left;
			        rc_r= remaining.Left + width;
			        fBegin= remaining.Top;
		        }
		        else
		        {
			        rc_t= remaining.Top;
			        rc_b= remaining.Top + width;
			        fBegin= remaining.Left;
		        }

		        // Now put the children into their places
		        for (int i=rowBegin; i < rowEnd; i++)
		        {
			        int begin= (int)fBegin;
			        double fraction= (double)(((NodeDatum)parent.Nodes[i].Tag).nTotalLength) / sum;
			        double fEnd= fBegin + fraction * height;
			        int end= (int)fEnd;
			        bool lastChild = (i == rowEnd - 1 || ((NodeDatum)parent.Nodes[i+1].Tag).nTotalLength == 0);

			        if (lastChild)
			        {
				        // Use up the whole height
				        end= (horizontal ? remaining.Top + height : remaining.Left + height);
			        }
		
			        if (horizontal)
			        {
				        rc_t= begin;
				        rc_b= end;
			        }
			        else
			        {
				        rc_l= begin;
				        rc_r= end;
			        }

                    Rectangle rc = new Rectangle(rc_l, rc_t, rc_r - rc_l, rc_t - rc_b);

			        Debug.Assert(rc.Left <= rc.Right);
			        Debug.Assert(rc.Top <= rc.Bottom);

			        Debug.Assert(rc.Left >= remaining.Left);
			        Debug.Assert(rc.Right <= remaining.Right);
			        Debug.Assert(rc.Top >= remaining.Top);
			        Debug.Assert(rc.Bottom <= remaining.Bottom);

			        RecurseDrawGraph(graphics, parent.Nodes[i], rc, false, surface, h * m_options.scaleFactor, 0);

			        if (lastChild)
                    {
				        break;
                    }

			        fBegin= fEnd;
		        }

		        // Put the next row into the rest of the rectangle
		        if (horizontal)
			        remaining.Offset(width, 0);
		        else
			        remaining.Offset(0, width);

		        remainingSize-= sum;
		
		        Debug.Assert(remaining.Left <= remaining.Right);
		        Debug.Assert(remaining.Top <= remaining.Bottom);

		        Debug.Assert(remainingSize >= 0);

		        head+= (rowEnd - rowBegin);

		        if (remaining.Width <= 0 || remaining.Height <= 0)
		        {
			        if (head < parent.Nodes.Count)
				        ((NodeDatum)parent.Nodes[head].Tag).TreeMapRect = new Rectangle(-1, -1, -1, -1);

			        break;
		        }
	        }
	        Debug.Assert(remainingSize == 0);
	        Debug.Assert(remaining.Left == remaining.Right || remaining.Top == remaining.Bottom);
        }


        // No squarification. Children are arranged alternately horizontally and vertically.
        //
        void Simple_DrawChildren(Graphics graphics, TreeNode parent, double[] surface, double h, uint flags)
        {
        #if true
	        Debug.Assert(false); // Not used in Windirstat.


        #else
	        Debug.Assert(parent.Nodes.Count > 0);
	        Debug.Assert(((NodeDatum)parent.Tag).nTotalLength > 0);

	        Rectangle rc= ((NodeDatum)parent.Tag).TreeMapRect;

	        bool horizontal = (flags == 0);

	        int width = horizontal ? rc.Width : rc.Height;
	        Debug.Assert(width >= 0);

	        double fBegin = horizontal ? rc.Left : rc.Top;
	        int veryEnd = horizontal ? rc.Right : rc.Bottom;

	        for (int i=0; i < parent.Nodes.Count; i++)
	        {
		        double fraction = (double)(parent.Nodes[i).TmiGetSize()) / ((NodeDatum)parent.Tag).nTotalLength;

		        double fEnd = fBegin + fraction * width;

		        bool lastChild = (i == parent.Nodes.Count - 1 || parent.Nodes[i + 1).TmiGetSize() == 0);

		        if (lastChild)
			        fEnd = veryEnd;

		        int begin= (int)fBegin;
		        int end= (int)fEnd;

		        Debug.Assert(begin <= end);
		        Debug.Assert(end <= veryEnd);

		        Rectangle rcChild;
		        if (horizontal)
		        {
			        rcChild.Left= begin;
			        rcChild.Right= end;
			        rcChild.Top= rc.Top;
			        rcChild.Bottom= rc.Bottom;
		        }
		        else
		        {
			        rcChild.Top= begin;
			        rcChild.Bottom= end;
			        rcChild.Left= rc.Left;
			        rcChild.Right= rc.Right;
		        }

		        RecurseDrawGraph(
			        graphics, 
			        parent.Nodes[i), 
			        rcChild,
			        false,
			        surface,
			        h * m_options.scaleFactor,
			        flags == 0 ? 1 : 0
		        );

		        if (lastChild)
		        {
			        i++;
			        break;
		        }

		        fBegin= fEnd;
	        }
	        if (i < parent.Nodes.Count)
		        parent.Nodes[i).TmiSetRectangle(Rectangle(-1, -1, -1, -1));
        #endif
        }

        bool IsCushionShading()
        {
	        return m_options.ambientLight < 1.0
		        && m_options.height > 0.0
		        && m_options.scaleFactor > 0.0;
        }

        void RenderLeaf(Graphics graphics, TreeNode item, double[] surface)
        {
	        Rectangle rc= ((NodeDatum)item.Tag).TreeMapRect;

	        if (m_options.grid)
	        {
                rc.Offset(1, 1);

                if (rc.Width <= 0 || rc.Height <= 0)
			        return;
	        }

	        RenderRectangle(graphics, rc, surface, item.ForeColor == Color.Empty ? Color.SandyBrown : item.ForeColor);
        }

        void RenderRectangle(Graphics graphics, Rectangle rc, double[] surface, Color color)
        {
	        double brightness = m_options.brightness;

	        if ((color.ToArgb() & COLORFLAG_MASK) != 0)
	        {
		        uint flags = (uint) (color.ToArgb() & COLORFLAG_MASK);
		        color= ColorSpace.MakeBrightColor(color, PALETTE_BRIGHTNESS);

		        if ((flags & COLORFLAG_DARKER) != 0)
		        {
			        brightness*= 0.66;
		        }
		        else
		        {
			        brightness*= 1.2;
			        if (brightness > 1.0)
				        brightness= 1.0;
		        }
	        }

	        if (IsCushionShading())
	        {
		        DrawCushion(graphics, rc, surface, color, brightness);
	        }
	        else
	        {
		        DrawSolidRect(graphics, rc, color, brightness);
	        }
        }

        void DrawSolidRect(Graphics graphics, Rectangle rc, Color col, double brightness)
        {
	        int red = col.R;
	        int green = col.G;
	        int blue = col.B;
	
	        double factor = brightness / PALETTE_BRIGHTNESS;

	        red= (int)(red * factor);
	        green= (int)(green * factor);
	        blue= (int)(blue * factor);

	        ColorSpace.NormalizeColor(ref red, ref green, ref blue);

	        graphics.FillRectangle(new SolidBrush(Color.FromArgb(red, green, blue)), rc);
        }

        void DrawCushion(Graphics graphics, Rectangle rc, double[] surface, Color col, double brightness)
        {
	        // Cushion parameters
	        double Ia = m_options.ambientLight;

	        // Derived parameters
	        double Is = 1 - Ia;			// shading

	        double colR= col.R;
	        double colG= col.G;
	        double colB= col.B;

	        for (int iy = rc.Top; iy < rc.Bottom; iy++)
            {
	            for (int ix = rc.Left; ix < rc.Right; ix++)
	            {
		            double nx= -(2 * surface[0] * (ix + 0.5) + surface[2]);
		            double ny= -(2 * surface[1] * (iy + 0.5) + surface[3]);
		            double cosa= (nx*m_Lx + ny*m_Ly + m_Lz) / Math.Sqrt(nx*nx + ny*ny + 1.0);
		            if (cosa > 1.0)
			            cosa= 1.0;
		
		            double pixel= Is * cosa;
		            if (pixel < 0)
			            pixel= 0;
		
		            pixel+= Ia;
		            Debug.Assert(pixel <= 1.0);

		            // Now, pixel is the brightness of the pixel, 0...1.0.

		            // Apply contrast.
		            // Not implemented.
		            // Costs performance and nearly the same effect can be
		            // made width the m_options.ambientLight parameter.
		            // pixel= pow(pixel, m_options.contrast);

		            // Apply "brightness"
		            pixel*= brightness / PALETTE_BRIGHTNESS;

		            // Make color value
		            int red		= (int)(colR * pixel);
		            int green	= (int)(colG * pixel);
		            int blue	= (int)(colB * pixel);

		            ColorSpace.NormalizeColor(ref red, ref green, ref blue);

		            // ... and set!
		            graphics.DrawLine(new Pen(Color.FromArgb(red, green, blue)), new Point(ix, iy), new Point(ix+1, iy+1));
	            }
            }
        }

        void AddRidge(Rectangle rc, double[] surface, double h)
        {
	        /* 
	        Unoptimized:

	        if (rc.Width > 0)
	        {
		        surface[2]+= 4 * h * (rc.Right + rc.Left) / (rc.Right - rc.Left);
		        surface[0]-= 4 * h / (rc.Right - rc.Left);
	        }

	        if (rc.Height > 0)
	        {
		        surface[3]+= 4 * h * (rc.Bottom + rc.Top) / (rc.Bottom - rc.Top);
		        surface[1]-= 4 * h / (rc.Bottom - rc.Top);
	        }
	        */
	
	        // Optimized (gained 15 ms of 1030):

	        int width= rc.Width;
	        int height= rc.Height;

	        Debug.Assert(width > 0 && height > 0);

	        double h4= 4 * h;

	        double wf= h4 / width;
	        surface[2]+= wf * (rc.Right + rc.Left);
	        surface[0]-= wf;

	        double hf= h4 / height;
	        surface[3]+= hf * (rc.Bottom + rc.Top);
	        surface[1]-= hf;
        }
    }
}