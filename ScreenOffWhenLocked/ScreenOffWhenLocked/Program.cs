using Microsoft.Win32;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;

namespace ScreenOffWhenLocked
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            _form = new Form();
            _form.Load += (o, e) => SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
            _form.Location = new Point(10000, 10000);
            _form.StartPosition = FormStartPosition.Manual;
            _form.ShowInTaskbar = false;
            _form.ShowDialog();
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr
            SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = false)]
        static extern IntPtr
            GetDesktopWindow();

        const Int32
            WM_SYSCOMMAND = 0x112;

        const UInt32
            SC_MONITORPOWER = 0xF170;

        static void SystemEvents_SessionSwitch(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
        {
            if (e.Reason != SessionSwitchReason.SessionLock)
                return;

            //  1       Low power
            //  2       Off
            // -1       On
            if (null == _form)
                _form = new Form();

            SendMessage(_form.Handle, WM_SYSCOMMAND, (IntPtr)SC_MONITORPOWER, (IntPtr)2);
        }

        static Form _form = null;
    }
}
