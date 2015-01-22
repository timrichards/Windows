using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace DoubleFile
{
    class FlashWindow
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
            Control dispatcher = ctl_in ?? GlobalData.static_MainWindow.SearchDirListsForm;
            UtilAnalysis_DirList.CheckAndInvoke(dispatcher, new Action(() =>
            {
                FLASHWINFO fInfo = new FLASHWINFO();

                fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));

                if (ctl_in != null)
                {
                    fInfo.hwnd = ctl_in.Handle;
                }
                else
                {
                    fInfo.hwnd = GlobalData.static_MainWindow.SearchDirListsForm.Handle;
                }

                fInfo.dwFlags = FLASHW_ALL;
                fInfo.uCount = (uint)(Once ? 1 : 3);
                fInfo.dwTimeout = 0;
                FlashWindowEx(ref fInfo);
            }));
        }
    }
}
