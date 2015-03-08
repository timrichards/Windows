using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal static bool AppActivated { get; private set; }

        private void Application_Activated(object sender, System.EventArgs e)
        {
            AppActivated = true;
        }

        private void Application_Deactivated(object sender, System.EventArgs e)
        {
            AppActivated = false;
        }
    }
}
