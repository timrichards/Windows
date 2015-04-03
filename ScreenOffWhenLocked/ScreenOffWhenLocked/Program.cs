using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;

namespace ScreenOffWhenLocked
{
    class Program
    {
        [STAThread]
        static void Main()
        {
            new Form1().ShowDialog();
        }
    }

    class Form1 : Form
    {
        internal Form1()
        {
            _timer = new System.Windows.Forms.Timer() { Interval = 60000 };
            _timer.Tick += (o, e) => SendMessage();

            Location = new Point(10000, 10000);
            Size = new Size(0, 0);
            FormBorderStyle = FormBorderStyle.None;
            ShowIcon = false;
            Opacity = 0;
            Icon = null;
            StartPosition = FormStartPosition.Manual;
            ShowInTaskbar = false;

            Load += (o, e) =>
            {
                Hide();
                Enabled = false;
            };

            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr
            SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        const Int32
            WM_SYSCOMMAND = 0x112;

        const UInt32
            SC_MONITORPOWER = 0xF170;

        void SendMessage()
        {
            //  1       Low power
            //  2       Off
            // -1       On

            SendMessage(Handle, WM_SYSCOMMAND, (IntPtr)SC_MONITORPOWER, (IntPtr)2);
        }

        void SystemEvents_SessionSwitch(object sender, Microsoft.Win32.SessionSwitchEventArgs e)
        {
            switch (e.Reason)
            {
                case SessionSwitchReason.SessionLock:
                {
                    SendMessage();
                    _timer.Start();
                    break;
                }

                case SessionSwitchReason.SessionUnlock:
                {
                    _timer.Stop();
                    break;
                }

                default:
                    return;
            }
        }

        System.Windows.Forms.Timer
            _timer = null;
    }
}
