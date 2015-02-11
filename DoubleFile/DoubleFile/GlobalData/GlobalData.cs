using System;
using System.Windows;
using System.Windows.Forms;

namespace DoubleFile
{
    abstract class GlobalData_Base
    {
        internal bool WindowClosed;

        internal FileDictionary FileDictionary = null;
    }

    class GlobalData_Window : GlobalData_Base
    {
        internal readonly MainWindow Main_Window = null;

        internal GlobalData_Window(MainWindow mainWindow_in)
        {
            Main_Window = mainWindow_in;

            EventHandler closedEvent = null;
            EventHandler closedEvent_ = (o, e) =>
            {
                WindowClosed = true;
                FileDictionary.Dispose();
                FileDictionary = null;
                Main_Window.Closed -= closedEvent;
            };

            closedEvent = closedEvent_;
            Main_Window.Closed += closedEvent;

            FileDictionary = new FileDictionary();
        }
    }

    class GlobalData_Form : GlobalData_Base
    {
        internal readonly FormAnalysis_DirList Main_Form = null;

        internal GlobalData_Form(FormAnalysis_DirList mainForm_in)
        {
            Main_Form = mainForm_in;

            EventHandler closedEvent = null;
            EventHandler closedEvent_ = (o, e) =>
            {
                WindowClosed = true;
                Main_Form.Closed -= closedEvent;
            };

            closedEvent = closedEvent_;
            Main_Form.Closed += closedEvent;
        }
    }
}
