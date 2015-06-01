using System.Runtime.InteropServices;
using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// The RECT structure defines the coordinates of the upper-left and lower-right corners of a rectangle.
    /// </summary>
    /// <see cref="http://msdn.microsoft.com/en-us/library/dd162897%28VS.85%29.aspx"/>
    /// <remarks>
    /// By convention, the right and bottom edges of the rectangle are normally considered exclusive.
    /// In other words, the pixel whose coordinates are ( right, bottom ) lies immediately outside of the the rectangle.
    /// For example, when RECT is passed to the FillRect function, the rectangle is filled up to, but not including,
    /// the right column and bottom row of pixels. This structure is identical to the RECTL structure.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    struct RECT
    {
        internal int Width { get { return Right - Left; } }
        internal int Height { get { return Bottom - Top; } }

        static public implicit operator Rect(RECT value)
        {
            return new Rect
            {
                X = value.Left,
                Y = value.Top,
                Width = value.Width,
                Height = value.Height
            };
        }

        /// <summary>
        /// The x-coordinate of the upper-left corner of the rectangle.
        /// </summary>
        internal int Left;

        /// <summary>
        /// The y-coordinate of the upper-left corner of the rectangle.
        /// </summary>
        internal int Top;

        /// <summary>
        /// The x-coordinate of the lower-right corner of the rectangle.
        /// </summary>
        internal int Right;

        /// <summary>
        /// The y-coordinate of the lower-right corner of the rectangle.
        /// </summary>
        internal int Bottom;
    }
}
