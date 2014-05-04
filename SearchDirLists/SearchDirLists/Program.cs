using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace SearchDirLists
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (Environment.OSVersion.Version.Major >= 6) SetProcessDPIAware();
//            if (Environment.OSVersion.Version.Major >= 6) SetProcessDpiAwareness(2);    // 2: per-monitor DPI awareness
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();

        //[System.Runtime.InteropServices.DllImport("shcore.dll")]
        //private static extern bool SetProcessDpiAwareness(long value);
    }
}


//<assembly xmlns="urn:schemas-microsoft-com:asm.v1" manifestVersion="1.0" xmlns:asmv3="urn:schemas-microsoft-com:asm.v3" >
//  <asmv3:application>
//    <asmv3:windowsSettings xmlns="http://schemas.microsoft.com/SMI/2005/WindowsSettings">
//      <dpiAware>true</dpiAware>
//    </asmv3:windowsSettings>
//  </asmv3:application>
//</assembly>
