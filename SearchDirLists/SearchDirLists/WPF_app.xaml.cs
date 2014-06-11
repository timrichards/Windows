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
            VolumesTabVM context = new VolumesTabVM(app);
            app.DataContext = context;
            app.Show();
        }
    }
}
