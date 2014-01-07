using System;
using System.Collections.Generic;
using System.Windows.Forms;

using System.Diagnostics;       // Process
using Microsoft.Win32;          // Registry
using System.IO;                // File

using System.Drawing;           // Point
using System.Text.RegularExpressions;

namespace RegexReplace
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 

        static public Rectangle mrFormMainBounds;
        static public bool mbFuncsExpanded;
        static public string msSearch;
        static public string msMath;
        static public string msFormat;
        static public string msReplace;
        static public int miRegexOptions;

        static RegistryKey keyMRX = null;
        const string csKeyMRX = "{EFBFD812-2A40-4EA0-AD8F-67AD7D298307}";
        const string csInstallLocation = "MRX_InstallLocation";
        const string csWindowLocationX = "MRX_WindowLocationX";
        const string csWindowLocationY = "MRX_WindowLocationY";
        const string csWindowSizeX = "MRX_WindowSizeX";
        const string csWindowSizeY = "MRX_WindowSizeY";
        const string csWindowMaximized = "MRX_Maximized";
        const string csTopMost = "MRX_TopMost";
        const string csFuncsExpanded = "MRX_FuncsExpanded";
        const string csSearch = "MRX_Search";
        const string csRegexOptions = "MRX_RegexOptions";
        const string csMath = "MRX_Math";
        const string csFormat = "MRX_Format";
        const string csReplace = "MRX_Replace";
        const string csNokillPID = "MRX_NokillPID";

        static void SingletonOnly()
        {
            int pid = Process.GetCurrentProcess().Id, noKillPid = 0, noKillSetup = 0;
            {
                Process[] p = Process.GetProcessesByName("Math Regex Setup");
                if (p.Length == 1) noKillSetup = p[0].Id;
            }

            if (keyMRX is RegistryKey) noKillPid = (int)keyMRX.GetValue(csNokillPID, 0);

            {                                                               // Run only one /form/ at a time
                Process[] p = Process.GetProcessesByName("MathRegex");
                for (ushort i = 0; i < p.Length; ++i)
                    if (p[i].Id != pid && p[i].Id != noKillPid && p[i].Id != noKillSetup) p[i].CloseMainWindow();
            }
            System.Threading.Thread.Sleep(500);
            {                                                               // make sure to kill it, even if they have an alert box open
                Process[] p = Process.GetProcessesByName("MathRegex");
                for (ushort i = 0; i < p.Length; ++i)
                    if (p[i].Id != pid && p[i].Id != noKillPid && p[i].Id != noKillSetup) p[i].Kill();
            }
        }

        static void RunApp()
        {
            SingletonOnly();
            Form_Main form = new Form_Main();
            {
                Rectangle rLocation = Rectangle.Empty;
                if (keyMRX is RegistryKey)
                {
                    int x = int.MinValue, y = int.MinValue;
                    x = Convert.ToInt32(keyMRX.GetValue(csWindowLocationX, int.MinValue));
                    y = Convert.ToInt32(keyMRX.GetValue(csWindowLocationY, int.MinValue));
                    int h = int.MinValue, v = int.MinValue;
                    h = Convert.ToInt32(keyMRX.GetValue(csWindowSizeX, int.MinValue));
                    v = Convert.ToInt32(keyMRX.GetValue(csWindowSizeY, int.MinValue));
                    if (x != int.MinValue && y != int.MinValue && h != int.MinValue && v != int.MinValue)
                        form.StartLocation = new Rectangle(x, y, h, v);
                    form.TopMost = Convert.ToBoolean(keyMRX.GetValue(csTopMost, 0));
                    mbFuncsExpanded = Convert.ToBoolean(keyMRX.GetValue(csFuncsExpanded, 0));
                    msSearch = (string)keyMRX.GetValue(csSearch, @"(?<A>0\.\d+)");
                    miRegexOptions = Convert.ToInt32(keyMRX.GetValue(csRegexOptions, 0));
                    msMath = (string)keyMRX.GetValue(csMath, @"sqrt( ${A} * .98 )");
                    msFormat = (string)keyMRX.GetValue(csFormat, @"0.00000");
                    msReplace = (string)keyMRX.GetValue(csReplace, @"${A} is the original. ${Math} is the result.");
                }
                mrFormMainBounds = form.Bounds;
            }
            Application.Run(form);

            try
            {
                if (keyMRX is RegistryKey)
                {
                    keyMRX.SetValue(csWindowLocationX, mrFormMainBounds.X, RegistryValueKind.DWord);
                    keyMRX.SetValue(csWindowLocationY, mrFormMainBounds.Y, RegistryValueKind.DWord);
                    keyMRX.SetValue(csWindowSizeX, mrFormMainBounds.Width, RegistryValueKind.DWord);
                    keyMRX.SetValue(csWindowSizeY, mrFormMainBounds.Height, RegistryValueKind.DWord);
                    keyMRX.SetValue(csTopMost, form.TopMost ? 1 : 0, RegistryValueKind.DWord);
                    keyMRX.SetValue(csFuncsExpanded, mbFuncsExpanded ? 1 : 0, RegistryValueKind.DWord);
                    keyMRX.SetValue(csSearch, msSearch);
                    keyMRX.SetValue(csRegexOptions, miRegexOptions, RegistryValueKind.DWord);
                    keyMRX.SetValue(csMath, msMath);
                    keyMRX.SetValue(csFormat, msFormat);
                    keyMRX.SetValue(csReplace, msReplace);
                }
            }
            catch { }
        }

        static void OpenKeyMRX(RegistryKey keyHive)
        { keyMRX = keyHive.OpenSubKey("MathRegex", RegistryKeyPermissionCheck.ReadWriteSubTree, System.Security.AccessControl.RegistryRights.FullControl); }

        static public void TryDelete(FileSystemInfo fsi) { if (fsi.Exists) try { fsi.Delete(); } catch { } }

        static Form kludgeForm = null;
        static public void TopMostMessageBox_Show(string sText, string sCaption) { MessageBox.Show(kludgeForm, sText, sCaption); }

        static void submain(string[] args)
        {
            RegistryKey keyHive = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\Tim Richards", RegistryKeyPermissionCheck.ReadWriteSubTree);
            if (!(keyMRX is RegistryKey)) keyHive.CreateSubKey("MathRegex");
            OpenKeyMRX(keyHive);

            if (args.Length != 1 && args.Length != 2) { RunApp(); return; }

            switch (args[0])
            {
                case "iExpress_Install":          // From an iExpress temp dir
                    if (args.Length != 1) { RunApp(); return; }
                    keyMRX.SetValue(csNokillPID, Process.GetCurrentProcess().Id);
                    SingletonOnly();
                    {
                        Process p = new Process();
                        p.StartInfo.FileName = "msiexec.exe";
                        p.StartInfo.Arguments = "/qn /uninstall " + csKeyMRX;
                        p.Start();
                        p.WaitForExit();
                    }
                    {
                        Process p = new Process();
                        p.StartInfo.FileName = "Setup.exe";
                        p.Start();
                        p.WaitForExit();        // runs installed copy of this app with "CustomAction_Install", below, so we get the install path
                    }
                    {
                        OpenKeyMRX(keyHive);    // keyMRX will only have freshly been installed by setup.exe, open it now to refresh it
                        if (!(keyMRX is RegistryKey)) return;                       // they cancelled the install

                        keyMRX.DeleteValue(csNokillPID, false);

                        if (null == keyMRX.GetValue("Publisher", null)) return;     // they cancelled the install

                        object o = keyMRX.GetValue(csInstallLocation, null);

                        if (o == null) return;
                        string sAppDir = o.ToString();

                        //string sFile = string.Empty;
                        //string sInstalledText = Verifier.Verify(Screen.PrimaryScreen.Bounds, sAppDir, ref sFile, false)
                        //    ? "successfully attached to your Terminal.\nPlease restart the terminal before running Math Regex for the first time.\nYou will be asked to run the Math Regex script each"
                        //    : "could not find your terminal.\nYou will be asked to find it next";

                        //try { File.Delete(sAppDir + @"\*.jpg"); }
                        //catch { }

                        TopMostMessageBox_Show("Math Regex installed.", "Math Regex Installed.");
                    }

                    return;

                case "CustomAction_Install":         // the app, in the install folder
                    if (args.Length != 1) { RunApp(); return; }
                    // Do /not/ run as singleton: case iExpress_Install, in a temp folder, is still awaiting this info.
                    keyMRX.SetValue(csInstallLocation, Application.StartupPath);
                    return;

                case "User_Uninstall":               // the app, in the install folder
                    if (args.Length != 1) { RunApp(); return; }
                    SingletonOnly();
                    {
                        // Don't put any uninstall stuff here. This is only hit when a user uninstalls. Use CustomAction_Uninstall, invoked by msiexec.
                        Process p = new Process();
                        p.StartInfo.FileName = "msiexec.exe";
                        p.StartInfo.Arguments = "/uninstall " + csKeyMRX;
                        p.Start();              // Don't wait for exit: Let msi delete the app, would be killed anyway by case CustomAction_Uninstall
                    }
                    return;

                case "CustomAction_Uninstall":      // the app, in the install folder
                    if (args.Length != 1) { RunApp(); return; }
                    SingletonOnly();

                    //if (Process.GetProcessesByName("terminal").Length == 1)
                    //    TopMostMessageBox_Show("Math Regex is being uninstalled.\nIt is recommended that you close your Terminal before proceeding.", "Please close Terminal");

                    if (keyMRX is RegistryKey && keyMRX.GetValue(csInstallLocation, null) != null)
                    {
                        keyMRX.DeleteValue(csInstallLocation);
                        //{
                        //    string sEX4_InstalledFile = Application.StartupPath + @"\EX4_Installed.txt";
                        //    FileInfo fi = new FileInfo(sEX4_InstalledFile);
                        //    if (fi.Exists)
                        //    {
                        //        string sEX4path; { StreamReader sr = new StreamReader(sEX4_InstalledFile); sEX4path = sr.ReadLine(); sr.Close(); }
                        //        TryDelete(fi);
                        //        TryDelete(new FileInfo(sEX4path + @"\experts\scripts\Math Regex.ex4"));

                        //        TryDelete(new FileInfo(sEX4path + @"\experts\scripts\Math Regex.mq4"));     // jic!
                        //        TryDelete(new FileInfo(sEX4path + @"\experts\files\Math Regex.TAR"));
                        //        TryDelete(new FileInfo(sEX4path + @"\experts\files\Math Regex0.txt"));
                        //    }
                        //}
                        File.Copy(Application.StartupPath + @"\Math Regex.exe", System.IO.Path.GetTempPath() + @"\MRX Uninstall.exe", true);

                        Process p = new Process();
                        p.StartInfo.FileName = System.IO.Path.GetTempPath() + @"\MRX Uninstall.exe";
                        p.StartInfo.Arguments = "tempuninstall \"" + Application.StartupPath + '"';
                        try { p.Start(); }  // Don't wait for exit. From here...
                        catch { goto case "tempuninstall"; }
                    }
                    else goto case "tempuninstall";
                    return;

                case "tempuninstall":               // System.IO.Path.GetTempPath() + @"\Math Regex Setup.exe";
                    // Don't use SingletonOnly here: case iExpress_Install may still be alive.
                    //              ...we land here, and the msi will be done and the app will have been deleted and the dir emptied.

                    System.Threading.Thread.Sleep(2000);    // Wait 2 seconds for msi to exit completely so we can clean up after it.
                    SingletonOnly();                    // do this before wiping out the node just below!

                    //if (keyMRX is RegistryKey && keyMRX.ValueCount < 6)     // Wipes out WindowLocation.
                    //    keyHive.DeleteSubKey(csKeyMRX);

                    // This doesn't work because the exe is locked while it is running
                    //TryDelete(new FileInfo(Application.StartupPath + @"\Math Regex.exe"));

                    //DirectoryInfo di;
                    //if (args.Length == 2) di = new DirectoryInfo(args[1]); else di = new DirectoryInfo(Application.StartupPath);
                    //DirectoryInfo diTAR = di.Parent;
                    //TryDelete(di);
                    //if (diTAR.Exists && diTAR.GetDirectories().Length == 0) TryDelete(diTAR);
                    return;

                default:
                    RunApp();
                    return;
            }
        }

        [STAThread]
        static void Main(string[] args)
        {
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                kludgeForm = new Form();

                kludgeForm.Size = new Size(1, 1);
                kludgeForm.StartPosition = FormStartPosition.Manual;
                kludgeForm.Location = new Point(SystemInformation.VirtualScreen.Right + 16, SystemInformation.VirtualScreen.Bottom + 16);
                kludgeForm.Show();
                kludgeForm.BringToFront();
                kludgeForm.TopMost = true;
                kludgeForm.ShowInTaskbar = false;
                kludgeForm.ShowIcon = false;
                kludgeForm.Visible = false;
            }
            submain(args);
            {
                kludgeForm.Dispose();
            }
        }
    }
}