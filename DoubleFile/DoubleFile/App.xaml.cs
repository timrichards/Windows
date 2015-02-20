using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        internal static event System.Action OnAppActivated = null;

        private void Application_Activated(object sender, System.EventArgs e)
        {
            if (null != OnAppActivated)
            {
                OnAppActivated();
            }
        }
    }
}
