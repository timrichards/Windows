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
            Control dispatcher = ctl_in ?? GlobalData.static_MainWindow.Analysis_DirListForm;
            UtilAnalysis_DirList.UIthread(dispatcher, () =>
            {
                FLASHWINFO fInfo = new FLASHWINFO();

                fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));

                if (ctl_in != null)
                {
                    fInfo.hwnd = ctl_in.Handle;
                }
                else
                {
                    fInfo.hwnd = GlobalData.static_MainWindow.Analysis_DirListForm.Handle;
                }

                fInfo.dwFlags = FLASHW_ALL;
                fInfo.uCount = (uint)(Once ? 1 : 3);
                fInfo.dwTimeout = 0;
                FlashWindowEx(ref fInfo);
            });
        }


        internal static void Go(System.Windows.Window window_in)
        {
            System.Windows.Window window = window_in ?? GlobalData.static_MainWindow;

            FLASHWINFO fInfo = new FLASHWINFO();

            fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));
            fInfo.hwnd = new WindowInteropHelper(window).Handle;
            fInfo.dwFlags = FLASHW_ALL;
            fInfo.uCount = 5;
            fInfo.dwTimeout = 50;
            UtilProject.UIthread(() => FlashWindowEx(ref fInfo));
        }
    }
}
