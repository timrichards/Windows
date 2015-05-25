using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace DoubleFile
{
    static class NativeMethods
    {
        internal static int
            Command(IntPtr wParam)
        {
            return wParam.ToInt32() & 0xFFF0;
        }

        internal static T Call<T>(Func<T> action, T errorValue = default(T), double nAssertLoc = -1)
        {
            T retVal = action();
            var strGetLastError = new Win32Exception(Marshal.GetLastWin32Error()).Message;

            MBoxStatic.Assert(nAssertLoc,
                (default(T).Equals(errorValue)) ||
                (false == errorValue.Equals(retVal)));

            Util.WriteLine(strGetLastError);
            return retVal;
        }

        internal const int WM_SYSCOMMAND = 0x112;
        internal const int SC_MOVE = 0xF010;
        internal const int SC_MAXIMIZE = 0xF030;
        internal const int SC_RESTORE = 0xF120;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static internal extern bool
            GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        internal const uint MONITOR_MONITOR_DEFAULTTONULL = 0;
        internal const uint MONITOR_MONITOR_DEFAULTTOPRIMARY = 1;
        internal const uint MONITOR_DEFAULTTONEAREST = 2;

        [DllImport("user32.dll")]
        static internal extern IntPtr
            MonitorFromRect([In] ref RECT lprc, uint dwFlags);

        [DllImport("user32.dll", SetLastError = true)]
        static internal extern bool
            GetWindowRect(IntPtr hwnd, out RECT lpRect);
    }
}
