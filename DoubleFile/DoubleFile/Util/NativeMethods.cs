using Microsoft.Win32.SafeHandles;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;

namespace DoubleFile
{
    static class NativeMethods
    {
        internal static int
            Command(IntPtr wParam)
        {
            return wParam.ToInt32() & 0xFFF0;
        }

        internal static T
            Call<T>(Func<T> action, bool bAssert = false, T errorValue = default(T), double nAssertLoc = -1)
        {
            T retVal = action();
            var strGetLastError = new Win32Exception(Marshal.GetLastWin32Error()).Message;

            MBoxStatic.Assert(nAssertLoc,
                (false == bAssert) ||
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

        [DllImport("user32.dll", SetLastError = true)]
        static internal extern bool
            SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);

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
        internal static extern IntPtr
            FindFirstFileExW(string lpFileName, IndexInfoLevels infoLevel, out DATUM lpFindFileData, IndexSearchOps fSearchOp, IntPtr lpSearchFilter, int dwAdditionalFlag);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode, BestFitMapping = false), SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool
            FindNextFileW(IntPtr handle, out DATUM lpFindFileData);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool
            FindClose(IntPtr hFindFile);

        internal static readonly IntPtr
            InvalidHandleValue = new IntPtr(-1);
    }
}
