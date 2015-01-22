using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DoubleFile
{
    static partial class ExtensionMethods
    {
        internal static bool IsChildOf(this TreeNode child, TreeNode treeNode)
        {
            if (child.Level <= treeNode.Level)
            {
                return false;
            }

            TreeNode parentNode = (TreeNode)child.Parent;

            while (parentNode != null)
            {
                if (parentNode == treeNode)
                {
                    return true;
                }

                parentNode = (TreeNode)parentNode.Parent;
            }

            return false;
        }

        internal static TreeNode Root(this TreeNode treeNode)
        {
            TreeNode nodeParent = treeNode;

            while (nodeParent.Parent != null)
            {
                nodeParent = (TreeNode)nodeParent.Parent;
            }

            return nodeParent;
        }

        internal static Rectangle Scale(this Rectangle rc_in, SizeF scale)
        {
            RectangleF rc = rc_in;

            rc.X *= scale.Width;
            rc.Y *= scale.Height;
            rc.Width *= scale.Width;
            rc.Height *= scale.Height;
            return Rectangle.Ceiling(rc);
        }
    }
}
