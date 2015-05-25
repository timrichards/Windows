using System;
using System.Runtime.InteropServices;

namespace DoubleFile
{
    static class NativeMethods
    {
        internal const int WM_SYSCOMMAND = 0x0112;
        internal const int SC_MOVE = 0xF010;
        internal const int SC_MAXIMIZE = 0xF030;
        internal const int SC_RESTORE = 0xF120;

        internal static int Command(IntPtr wParam)
        {
            return wParam.ToInt32() & 0xFFF0;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static internal extern bool
            GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        internal const uint MONITOR_MONITOR_DEFAULTTONULL = 0x00000000;
        internal const uint MONITOR_MONITOR_DEFAULTTOPRIMARY = 0x00000001;
        internal const uint MONITOR_DEFAULTTONEAREST = 0x00000002;

        [DllImport("user32.dll")]
        static internal extern IntPtr
            MonitorFromRect([In] ref RECT lprc, uint dwFlags);

        [DllImport("user32.dll", SetLastError = true)]
        static internal extern bool
            GetWindowRect(IntPtr hwnd, out RECT lpRect);
    }
}
