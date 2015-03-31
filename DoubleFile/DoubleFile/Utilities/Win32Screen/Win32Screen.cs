using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace DoubleFile
{
    static class Win32Screen
    {
        // size of a device name string
        const int CCHDEVICENAME = 32;       // Not used.

        const int MONITOR_CENTER = 0x0001;          // center rect to monitor 
        const int MONITOR_CLIP = 0x0000;            // clip rect to monitor 
        const int MONITOR_WORKAREA = 0x0002;        // use monitor work area 
        const int MONITOR_AREA = 0x0000;            // use monitor entire area 

        // 
        //  ClipOrCenterRectToMonitor 
        // 
        //  The most common problem apps have when running on a 
        //  multimonitor system is that they "clip" or "pin" windows 
        //  based on the SM_CXSCREEN and SM_CYSCREEN system metrics. 
        //  Because of app compatibility reasons these system metrics 
        //  return the size of the primary monitor. 
        // 
        //  This shows how you use the multi-monitor functions 
        //  to do the same thing. 
        // 
        static void ClipOrCenterRectToMonitor(ref RECT prc, uint flags)
        {
            IntPtr hMonitor;
            MONITORINFO mi = new MONITORINFO();
            RECT rc;
            int w = prc.right - prc.left;
            int h = prc.bottom - prc.top;

            // 
            // get the nearest monitor to the passed rect. 
            // 
            hMonitor = RECT.MonitorFromRect(ref prc, RECT.MONITOR_DEFAULTTONEAREST);

            // 
            // get the work area or entire monitor rect. 
            // 
            mi.cbSize = Marshal.SizeOf(mi);
            MONITORINFO.GetMonitorInfo(hMonitor, ref mi);

            if (0 != (flags & MONITOR_WORKAREA))
                rc = mi.rcWork;
            else
                rc = mi.rcMonitor;

            // 
            // center or clip the passed rect to the monitor rect 
            // 
            if (0 != (flags & MONITOR_CENTER))
            {
                prc.left = rc.left + (rc.right - rc.left - w) / 2;
                prc.top = rc.top + (rc.bottom - rc.top - h) / 2;
                prc.right = prc.left + w;
                prc.bottom = prc.top + h;
            }
            else
            {
                prc.left = Math.Max(rc.left, Math.Min(rc.right - w, prc.left));
                prc.top = Math.Max(rc.top, Math.Min(rc.bottom - h, prc.top));
                prc.right = prc.left + w;
                prc.bottom = prc.top + h;
            }
        }

        static void ClipOrCenterWindowToMonitor(IntPtr hwnd, uint flags)
        {
            RECT rc;

            RECT.GetWindowRect(hwnd, out rc);
            ClipOrCenterRectToMonitor(ref rc, flags);
            SWP.SetWindowPos(hwnd, IntPtr.Zero, rc.left, rc.top, 0, 0, SWP.NOSIZE | SWP.NOZORDER | SWP.NOACTIVATE);
        }

        internal static Rect GetOwnerMonitorRect(Window Owner)
        {
            RECT rcOwner;

            RECT.GetWindowRect(new WindowInteropHelper(Owner).Handle, out rcOwner);

            var strError = new Win32Exception(Marshal.GetLastWin32Error()).Message;
            var hMonitor = RECT.MonitorFromRect(ref rcOwner, RECT.MONITOR_MONITOR_DEFAULTTOPRIMARY);

            strError = new Win32Exception(Marshal.GetLastWin32Error()).Message;

            MONITORINFO mi = new MONITORINFO();

            mi.cbSize = Marshal.SizeOf(mi);

            var bSuccess = MONITORINFO.GetMonitorInfo(hMonitor, ref mi);

            MBoxStatic.Assert(99904, bSuccess, new Win32Exception(Marshal.GetLastWin32Error()).Message);

            var rc = mi.rcMonitor;

            return new Rect()
            {
                X = rc.left,
                Y = rc.top,
                Width = rc.right - rc.left,
                Height = rc.bottom - rc.top
            };
        }
    }
}
