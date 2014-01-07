using System;
using System.Collections.Generic;
using System.Text;

using System.Drawing;                   // Icon;
using System.Runtime.InteropServices;   // Icon Extractor   

namespace NS_MyData
{
        [StructLayout(LayoutKind.Sequential)]
        public struct SHFILEINFO
        {
            public IntPtr hIcon;
            public IntPtr iIcon;
            public uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        };

        class Win32
        {
            public const uint SHGFI_ICON = 0x100;
            public const uint SHGFI_LARGEICON = 0x0;    // 'Large icon
            public const uint SHGFI_SMALLICON = 0x1;    // 'Small icon

            [DllImport("shell32.dll")]
            public static extern IntPtr SHGetFileInfo(string pszPath,
                                        uint dwFileAttributes,
                                        ref SHFILEINFO psfi,
                                        uint cbSizeFileInfo,
                                        uint uFlags);

        //http://www.codeproject.com/cs/miscctrl/FilesListBox.asp?df=100&forumid=250672&exp=0&select=1346471&tid=1344705#xx1346471xx
        public static Icon GetFileIcon(string fileName, uint iconSize)
        {
            try
            {
                SHFILEINFO shinfo = new SHFILEINFO();
                Win32.SHGetFileInfo(fileName, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), Win32.SHGFI_ICON | iconSize);
                return Icon.FromHandle(shinfo.hIcon);
            }
            catch { return null; }
        }
    }
}
