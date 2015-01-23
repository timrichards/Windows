using System.Windows;
using System.Windows.Forms;

namespace DoubleFile
{
    abstract class GlobalDataBase
    {
   //     GlobalData gd;

        internal abstract bool WindowClosed { get; }
    }

    class GlobalData_Window : GlobalDataBase
    {
        internal GlobalData_Window(Window maainWindow_in) { MainWindow = maainWindow_in; }
        internal Window MainWindow { get; private set; }
        internal override bool WindowClosed { get { return (MainWindow == null) || (false == MainWindow.IsLoaded); } }
    }

    class GlobalData_Form : GlobalDataBase
    {
        internal GlobalData_Form(Form mainForm_in) { MainForm = mainForm_in; }
        internal Form MainForm { get; private set; }
        internal override bool WindowClosed { get { return (MainForm == null) || (MainForm.IsDisposed); } }
    }
}
