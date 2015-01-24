using System;
using System.Windows;
using System.Windows.Forms;

namespace DoubleFile
{
    abstract class GlobalData_Base
    {
   //     GlobalData gd;

        internal abstract bool WindowClosed { get; }
    }

    class GlobalData_Window : GlobalData_Base
    {
        internal GlobalData_Window(Window mainWindow_in) { MainWindow = mainWindow_in; }
        internal Window MainWindow { get; private set; }
        internal override bool WindowClosed
        {
            get
            {
                if (MainWindow == null)
                {
                    return true;
                }

                return (bool) UtilProject.CheckAndInvoke(new BoolAction(() => { return (false == MainWindow.IsLoaded); }));
            }
        }
    }

    class GlobalData_Form : GlobalData_Base
    {
        internal GlobalData_Form(Form mainForm_in) { MainForm = mainForm_in; }
        internal Form MainForm { get; private set; }
        internal override bool WindowClosed { get { return (MainForm == null) || (MainForm.IsDisposed); } }
    }
}
