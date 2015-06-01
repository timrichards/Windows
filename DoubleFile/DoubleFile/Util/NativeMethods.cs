using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows;
using System.Windows.Interop;

namespace DoubleFile
{
    internal class NativeWindow : IEquatable<NativeWindow>
    {
        static public implicit operator IntPtr(NativeWindow h) { return h.hwnd; }
        static public implicit operator NativeWindow(Window w) { return new NativeWindow { hwnd = new WindowInteropHelper(w).Handle }; }
        static public implicit operator NativeWindow(IntPtr hwnd) { return new NativeWindow { hwnd = hwnd }; }

        public bool Equals(NativeWindow other)
        {
            return hwnd == other.hwnd;
        }

        IntPtr hwnd = IntPtr.Zero;
    }

    static class NativeMethods
    {
        static internal int
            Command(IntPtr wParam)
        {
            return wParam.ToInt32() & 0xFFF0;
        }

        static internal T
            Call<T>(Func<T> action, bool bAssert = false, T errorValue = default(T), double nAssertLoc = -1)
        {
            var retVal = action();
            var strGetLastError = new Win32Exception(Marshal.GetLastWin32Error()).Message;

            MBoxStatic.Assert(nAssertLoc,
                (false == bAssert) ||
                (false == errorValue.Equals(retVal)));
            
            Util.WriteLine(action.Method.Name + ": " + retVal + " (" + strGetLastError +")");
            return retVal;
        }

        internal const int WM_SYSCOMMAND = 0x112;
        internal const int SC_MOVE = 0xF010;
        internal const int SC_MINIMIZE = 0xF020;
        internal const int SC_MAXIMIZE = 0xF030;
        internal const int SC_RESTORE = 0xF120;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static internal extern bool
            GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO lpmi);

        internal const uint MONITOR_DEFAULTTONULL = 0;
        internal const uint MONITOR_DEFAULTTOPRIMARY = 1;
        internal const uint MONITOR_DEFAULTTONEAREST = 2;

        [DllImport("user32.dll")]
        static internal extern IntPtr
            MonitorFromRect([In] ref RECT lprc, uint dwFlags);

        [DllImport("user32.dll")]
        static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);
        static internal IntPtr
            MonitorFromWindow(NativeWindow w, uint dwFlags) { return MonitorFromWindow((IntPtr)w, dwFlags); }

        [DllImport("user32.dll", SetLastError = true)]
        static internal extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);
        //static internal bool
        //    GetWindowRect(NativeWindow w, out RECT lpRect) { return GetWindowRect((IntPtr)w, out lpRect); }

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);
        static internal bool
            SetWindowPos(NativeWindow w, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags) { return SetWindowPos((IntPtr)w, hWndInsertAfter, X, Y, cx, cy, uFlags); }

        //[DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, BestFitMapping = false), SuppressUnmanagedCodeSecurity]
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        static internal extern SafeFileHandle
            CreateFile(string lpFileName, FileAccess dwDesiredAccess, FileShare dwShareMode, IntPtr lpSecurityAttributes, int dwCreationDisposition, FileAttributes dwFlagsAndAttributes, IntPtr hTemplateFile);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        internal struct
            DATUM : IComparable
        {
            internal FileAttributes fileAttributes;
            internal uint ftCreationTimeLow;
            internal uint ftCreationTimeHigh;
            internal uint ftLastAccessTimeLow;
            internal uint ftLastAccessTimeHigh;
            internal uint ftLastWriteTimeLow;
            internal uint ftLastWriteTimeHigh;
            internal uint nFileSizeHigh;
            internal uint nFileSizeLow;
            internal int dwReserved0;
            internal int dwReserved1;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            internal string strFileName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 14)]
            internal string strAltFileName;

            public int CompareTo(object obj)
            {
                return strFileName.CompareTo(((DATUM)obj).strFileName);
            }
        }

        internal enum
            IndexInfoLevels
        {
            FindExInfoStandard = 0,
            FindExInfoBasic,
            FindExInfoMaxInfoLevel
        };

        internal enum
            IndexSearchOps
        {
            FindExSearchNameMatch = 0,
            FindExSearchLimitToDirectories,
            FindExSearchLimitToDevices
        };

        internal const int
            FIND_FIRST_EX_LARGE_FETCH = 2;

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, ExactSpelling = true, BestFitMapping = false), SuppressUnmanagedCodeSecurity]
        static internal extern IntPtr
            FindFirstFileExW(string lpFileName, IndexInfoLevels infoLevel, out DATUM lpFindFileData, IndexSearchOps fSearchOp, IntPtr lpSearchFilter, int dwAdditionalFlag);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, BestFitMapping = false), SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        static internal extern bool
            FindNextFileW(IntPtr handle, out DATUM lpFindFileData);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static internal extern bool
            FindClose(IntPtr hFindFile);

        static internal readonly IntPtr
            InvalidHandleValue = new IntPtr(-1);

        internal const int GWL_STYLE = -16;
        internal const int WS_MAXIMIZEBOX = 0x10000;
        internal const int WS_MINIMIZEBOX = 0x20000;

        internal const int GWL_EXSTYLE = -20;
        internal const int WS_EX_NOACTIVATE = 0x08000000;

        [DllImport("user32.dll")]
        static internal extern int
            GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static internal extern int
            SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        internal const UInt32 FLASHW_ALL = 3;

        [StructLayout(LayoutKind.Sequential)]
        internal struct FLASHWINFO
        {
            public UInt32 cbSize;
            public IntPtr hwnd;
            public UInt32 dwFlags;
            public UInt32 uCount;
            public UInt32 dwTimeout;
        }

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static internal extern bool
            FlashWindowEx(ref FLASHWINFO pwfi);

        [DllImport("user32.dll")]
        static extern IntPtr GetTopWindow(IntPtr hWnd);
        static internal NativeWindow
            GetTopWindow(NativeWindow w) { return GetTopWindow((IntPtr)w); }

        [DllImport("user32.dll")]
        static extern bool BringWindowToTop(IntPtr hWnd);
        static internal bool
            BringWindowToTop(NativeWindow w) { return BringWindowToTop((IntPtr)w); }

        internal const uint GW_HWNDNEXT = 2;

        [DllImport("User32")]
        static extern IntPtr GetWindow(IntPtr hWnd, uint wCmd);
        static internal NativeWindow
            GetWindow(NativeWindow w, uint wCmd) { return GetWindow((IntPtr)w, wCmd); }
    }
}
