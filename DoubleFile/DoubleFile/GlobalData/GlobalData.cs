using System;
using System.Windows;
using System.Windows.Forms;

namespace DoubleFile
{
    abstract class GlobalData_Base
    {
        internal abstract bool WindowClosed { get; }

        internal FileDictionary FileDictionary = null;
        internal WPF_TreeView WPF_TreeView = new WPF_TreeView();
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
                FileDictionary.Dispose();
                FileDictionary = null;
                Main_Window.Closed -= closedEvent;
            };

            closedEvent = closedEvent_;
            Main_Window.Closed += closedEvent;

            FileDictionary = new FileDictionary();
        }

        internal override bool WindowClosed
        {
            get
            {
                if (Main_Window == null)
                {
                    return true;
                }

                return (bool) UtilProject.CheckAndInvoke(new BoolAction(() => { return (false == Main_Window.IsLoaded); }));
            }
        }
    }

    class GlobalData_Form : GlobalData_Base
    {
        internal readonly FormAnalysis_DirList Main_Form = null;

        internal GlobalData_Form(FormAnalysis_DirList mainForm_in) { Main_Form = mainForm_in; }
        internal override bool WindowClosed { get { return (Main_Form == null) || (Main_Form.IsDisposed); } }
    }
}
