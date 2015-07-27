using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Windows;
using System.Windows.Interop;

namespace DoubleFile
{
    internal class NativeWindow : IEquatable<NativeWindow>, IWin32Window, System.Windows.Forms.IWin32Window
    {
        static public implicit operator
            IntPtr(NativeWindow h) => h.Handle;

        static public implicit operator
            NativeWindow(Window w) =>
            new NativeWindow
            {
                Handle = (null != w) ? new WindowInteropHelper(w).Handle : (IntPtr)0xBAD00 + nBadCount++,
                Window = w
            };

        static public implicit operator
            NativeWindow(IntPtr hwnd) => new NativeWindow { Handle = hwnd };

        public IntPtr Handle { get; private set; } = IntPtr.Zero;
        internal Window Window = null;

        // can't override == and != operator because of the implicit operator IntPtr above
        public bool Equals(NativeWindow other) => Handle == other?.Handle;

        static int nBadCount = 0;
    }

    static class NativeMethods
    {
        static internal int
            Command(IntPtr wParam)
        {
            return wParam.ToInt32() & 0xFFF0;
        }

        static internal T
            Call<T>(Func<T> action, bool bAssert = false, T errorValue = default(T), decimal nAssertLoc = -1)
        {
            var retVal = action();
            var strGetLastError = new Win32Exception(Marshal.GetLastWin32Error()).Message;

            Util.Assert(nAssertLoc,
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
            MonitorFromWindow(NativeWindow w, uint dwFlags) => MonitorFromWindow((IntPtr)w, dwFlags);

        [DllImport("user32.dll", SetLastError = true)]
        static internal extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);
        //static internal bool
        //    GetWindowRect(NativeWindow w, out RECT lpRect) => GetWindowRect((IntPtr)w, out lpRect);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, int uFlags);
        static internal bool
            SetWindowPos(NativeWindow w, NativeWindow insertAfter, int X, int Y, int cx, int cy, int uFlags) =>
            SetWindowPos((IntPtr)w, (IntPtr)insertAfter, X, Y, cx, cy, uFlags);

        internal const int OPEN_EXISTING = 3;
        internal const int FILE_FLAG_RANDOM_ACCESS = 0x10000000;

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode), SuppressUnmanagedCodeSecurity]
        static internal extern SafeFileHandle
            CreateFile(string lpFileName, FileAccess dwDesiredAccess, FileShare dwShareMode, IntPtr lpSecurityAttributes, int dwCreationDisposition, FileAttributes dwFlagsAndAttributes, IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError=true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static internal extern bool CloseHandle(IntPtr hObject);
        
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
                return strAltFileName.CompareTo(((DATUM)obj).strAltFileName);
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
        internal const int WS_EX_NOACTIVATE = 0x8000000;    // 7th place (six zeros)
        internal const int WS_EX_TOOLWINDOW = 0x80;

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);
        static internal int
            GetWindowLong(NativeWindow w, int nIndex) => GetWindowLong((IntPtr)w, nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        static internal int
            SetWindowLong(NativeWindow w, int nIndex, int dwNewLong) => SetWindowLong((IntPtr)w, nIndex, dwNewLong);

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
            GetTopWindow(NativeWindow w) => GetTopWindow((IntPtr)w);

        [DllImport("user32.dll")]
        static extern bool BringWindowToTop(IntPtr hWnd);
        static internal void
            BringWindowToTop(NativeWindow w) => BringWindowToTop((IntPtr)w);

        internal const uint GW_HWNDNEXT = 2;
        internal const uint GW_OWNER = 4;

        [DllImport("User32")]
        static extern IntPtr GetWindow(IntPtr hWnd, uint wCmd);
        static internal NativeWindow
            GetWindow(NativeWindow w, uint wCmd) => GetWindow((IntPtr)w, wCmd);

        internal static IList<NativeWindow> GetAllWindowsWithTitleOf(string title)
        {
            var searchData = new SearchData { Title = title };

            _lsRet = new List<NativeWindow>();
            EnumWindows(EnumProcTitleAll, ref searchData);
            return _lsRet;
        }

        static bool EnumProcTitleAll(IntPtr hWnd, ref SearchData searchData)
        {
            var sb = new StringBuilder(1024);

            GetWindowText(hWnd, sb, sb.Capacity);

            if (sb.ToString().StartsWith(searchData.Title))
                _lsRet.Add(hWnd);    // Found the wnd, keep enumerating though

            return true;
        }

        static IList<NativeWindow> _lsRet = null;
        public class SearchData { public string Title; }
        private delegate bool EnumWindowsProc(IntPtr hWnd, ref SearchData data);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, ref SearchData data);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);
    }
}
