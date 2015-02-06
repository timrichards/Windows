using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DoubleFile
{
    static partial class ExtensionMethodsStatic
    {
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
