using System.Windows;


namespace SearchDirLists
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow app = new MainWindow();
            new VolumesTabVM(app);
            new BrowseTabVM(app);
            app.Show();
        }
    }
}
