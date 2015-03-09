using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal static bool LocalActivated { get; private set; }
        internal static bool LocalExit { get; private set; }

        internal static bool DontFlashWindow
        {
            get { if (false == _DontFlashWindow) return false; _DontFlashWindow = false; return true; }
            private set { _DontFlashWindow = value; }
        }
        static bool _DontFlashWindow = false;

        public App()
        {
            LocalActivated = true;      // seemed to work but jic
            LocalExit = false;
        }

        private void Application_Activated(object sender, System.EventArgs e)
        {
            if (false == LocalActivated)
            {
                DontFlashWindow = true;
            }
            
            LocalActivated = true;
        }

        private void Application_Deactivated(object sender, System.EventArgs e)
        {
            LocalActivated = false;
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            LocalExit = true;
        }
    }
}
