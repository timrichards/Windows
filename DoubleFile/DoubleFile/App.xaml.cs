using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal static bool LocalActivated { get; private set; }

        private void Application_Activated(object sender, System.EventArgs e)
        {
            LocalActivated = true;
        }

        private void Application_Deactivated(object sender, System.EventArgs e)
        {
            LocalActivated = false;
        }
    }
}
