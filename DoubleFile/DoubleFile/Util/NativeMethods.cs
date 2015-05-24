using System;
using System.Runtime.InteropServices;

namespace DoubleFile
{
    static class NativeMethods
    {
        internal const int WM_SYSCOMMAND = 0x0112;
        internal const int SC_MOVE = 0xF010;

        internal static int Command(IntPtr wParam)
        {
            return wParam.ToInt32() & 0xFFF0;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static internal extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);
    }
}
