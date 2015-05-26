using System;
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
            // 
            // get the nearest monitor to the passed rect. 
            //

            var prcA = prc;

            var hMonitor = NativeMethods.Call(() => NativeMethods
                .MonitorFromRect(ref prcA, NativeMethods.MONITOR_DEFAULTTONEAREST));

            prc = prcA;

            // 
            // get the work area or entire monitor rect. 
            // 
            var mi = new MONITORINFO { cbSize = Marshal.SizeOf(typeof(MONITORINFO)) };

            NativeMethods.Call(() => NativeMethods
                .GetMonitorInfo(hMonitor, ref mi));

            var rc = 
                (0 != (flags & MONITOR_WORKAREA))
                ? mi.rcWork
                : mi.rcMonitor;

            // 
            // center or clip the passed rect to the monitor rect 
            // 
            var origWidth = prc.Width;
            var origHeight = prc.Height;

            if (0 != (flags & MONITOR_CENTER))
            {
                prc.Left = rc.Left + ((rc.Width - origWidth) >> 1);
                prc.Top = rc.Top + ((rc.Height - origHeight) >> 1);
                prc.Right = prc.Left + origWidth;
                prc.Bottom = prc.Top + origHeight;
            }
            else
            {
                prc.Left = Math.Max(rc.Left, Math.Min(rc.Right - origWidth, prc.Left));
                prc.Top = Math.Max(rc.Top, Math.Min(rc.Bottom - origHeight, prc.Top));
                prc.Right = prc.Left + origWidth;
                prc.Bottom = prc.Top + origHeight;
            }
        }

        static void ClipOrCenterWindowToMonitor(IntPtr hwnd, uint flags)
        {
            var rc = default(RECT);

            NativeMethods.Call(() => NativeMethods
                .GetWindowRect(hwnd, out rc));

            ClipOrCenterRectToMonitor(ref rc, flags);

            NativeMethods.Call(() => NativeMethods
                .SetWindowPos(hwnd, IntPtr.Zero, rc.Left, rc.Top, 0, 0, SWP.NOSIZE | SWP.NOZORDER | SWP.NOACTIVATE));
        }

        static internal Rect GetWindowRect(Window window)
        {
            switch (window.WindowState)
            {
                case WindowState.Normal:
                {
                    return new Rect(window.Left, window.Top, window.Width, window.Height);
                }

                case WindowState.Minimized:
                {
                    return default(Rect);
                }
            }

            // maximized

            var rcWindow = default(RECT);

            NativeMethods.Call(() => NativeMethods
                .GetWindowRect(new WindowInteropHelper(window).Handle, out rcWindow));

            var rcMonitor = GetWindowMonitorRect(window);

            return new Rect
            {
                X = rcWindow.Left,
                Y = rcWindow.Top,
                Width = rcMonitor.Right - rcWindow.Left,
                Height = rcMonitor.Bottom - rcWindow.Top
            };
        }

        static internal Rect GetWindowMonitorRect(Window window)
        {
            var rcOwner = new RECT
            {
                Left = (int)window.Left,
                Top = (int)window.Top,
                Right = (int)(window.Left + window.Width),
                Bottom = (int)(window.Top + window.Height)
            };

            var hMonitor = NativeMethods.Call(() => NativeMethods
                .MonitorFromRect(ref rcOwner, NativeMethods.MONITOR_DEFAULTTOPRIMARY));

            var mi = new MONITORINFO { cbSize = Marshal.SizeOf(typeof(MONITORINFO)) };

            NativeMethods.Call(() => NativeMethods
                .GetMonitorInfo(hMonitor, ref mi), true, false, 99904);

            var rc = mi.rcMonitor;

            return new Rect
            {
                X = rc.Left,
                Y = rc.Top,
                Width = rc.Width,
                Height = rc.Height
            };
        }
    }
}
