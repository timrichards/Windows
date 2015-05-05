using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace DoubleFile
{
    static class FlashWindowStatic
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [StructLayout(LayoutKind.Sequential)]
        public struct FLASHWINFO
        {
            public UInt32 cbSize;
            public IntPtr hwnd;
            public UInt32 dwFlags;
            public UInt32 uCount;
            public UInt32 dwTimeout;
        }

        public const UInt32 FLASHW_ALL = 3;

        static internal void Go(Window window_in, bool Once = false)
        {
            UtilProject.UIthread(() =>
            {
                var window = window_in ?? App.LocalMainWindow as Window;
                var fInfo = new FLASHWINFO
                {
                    hwnd = new WindowInteropHelper(window).Handle,
                    dwFlags = FLASHW_ALL,
                    uCount = (uint)(Once ? 1 : 7),
                    dwTimeout = 75
                };
                fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));

                FlashWindowEx(ref fInfo);
            });
        }
    }
}
