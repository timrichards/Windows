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

        internal static bool CanFlashWindow_ResetsIt
        {
            get
            {
                if (_canFlashWindow_ResetsIt)
                    return true;

                _canFlashWindow_ResetsIt = true;
                return false;
            }
        }
        static bool _canFlashWindow_ResetsIt = true;

        public App()
        {
            LocalActivated = true;      // seemed to work but jic
            LocalExit = false;
        }

        private void Application_Activated(object sender, System.EventArgs e)
        {
            if (false == LocalActivated)
            {
                _canFlashWindow_ResetsIt = false;
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
