using System;
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
            if (MessageBox.Show("Run WPF?", "Run WPF?", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                new App().Run();
            }
#if (WPF == false)
            else
            {
                if (Environment.OSVersion.Version.Major >= 6) SetProcessDPIAware();
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form1());
            }
#endif
        }   

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetProcessDPIAware();
    }
}
