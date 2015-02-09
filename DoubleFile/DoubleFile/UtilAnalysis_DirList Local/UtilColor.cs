using System;
using System.Drawing;
using System.Linq;

namespace DoubleFile
{
    static class UtilColor
    {
        readonly static internal int Empty = Color.Empty.ToArgb();
        readonly static internal int White = Color.White.ToArgb();
        readonly static internal int Blue = Color.Blue.ToArgb();
        readonly static internal int DarkBlue = Color.DarkBlue.ToArgb();
        readonly static internal int DarkGray = Color.DarkGray.ToArgb();
        readonly static internal int DarkOrange = Color.DarkOrange.ToArgb();
        readonly static internal int DarkRed = Color.DarkRed.ToArgb();
        readonly static internal int DarkSlateGray = Color.DarkSlateGray.ToArgb();
        readonly static internal int Firebrick = Color.Firebrick.ToArgb();
        readonly static internal int LightGoldenrodYellow = Color.LightGoldenrodYellow.ToArgb();
        readonly static internal int LightGray = Color.LightGray.ToArgb();
        readonly static internal int MediumSpringGreen = Color.MediumSpringGreen.ToArgb();
        readonly static internal int MediumVioletRed = Color.MediumVioletRed.ToArgb();
        readonly static internal int OliveDrab = Color.OliveDrab.ToArgb();
        readonly static internal int Red = Color.Red.ToArgb();
        readonly static internal int Snow = Color.Snow.ToArgb();
        readonly static internal int SteelBlue = Color.SteelBlue.ToArgb();

        internal static System.Windows.Media.Brush ARGBtoBrush(int nFormsARGB)
        {
            var abARGB = BitConverter.GetBytes(nFormsARGB);

            return new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromArgb(abARGB[3], abARGB[2], abARGB[1], abARGB[0])
            );
        }
    }
}
