﻿using System.Drawing;

namespace DoubleFile
{
    static partial class ExtensionMethodsStatic
    {
        static internal RectangleF Scale(this Rectangle rc_in, SizeF scale)
        {
            RectangleF rc = rc_in;

            rc.X *= scale.Width;
            rc.Y *= scale.Height;
            rc.Width *= scale.Width;
            rc.Height *= scale.Height;
            return rc;
        }
    }
}