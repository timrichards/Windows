using System;
using System.Runtime.InteropServices;
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

        internal static void Go(Control ctl_in = null, bool Once = false)
        {
            UtilAnalysis_DirList.UIthread(ctl_in ?? GlobalData.static_MainWindow.Analysis_DirListForm, () =>
            {
                var fInfo = new FLASHWINFO
                {
                    hwnd = (ctl_in != null)
                        ? ctl_in.Handle
                        : GlobalData.static_MainWindow.Analysis_DirListForm.Handle,
                    dwFlags = FLASHW_ALL,
                    uCount = (uint)(Once ? 1 : 3),
                    dwTimeout = 0
                };
                fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));

                FlashWindowEx(ref fInfo);
            });
        }

        internal static void Go(System.Windows.Window window_in, bool Once = false)
        {
            var window = window_in ?? GlobalData.static_MainWindow;
            var fInfo = new FLASHWINFO
            {
                hwnd = new WindowInteropHelper(window).Handle,
                dwFlags = FLASHW_ALL,
                uCount = (uint)(Once ? 1 : 7),
                dwTimeout = 75
            };
            fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));

            UtilProject.UIthread(() => FlashWindowEx(ref fInfo));
        }
    }
}
