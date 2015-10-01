using System.Windows;
using DoubleFile;

namespace VolTreemap
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            _statics = new Statics();
        }

        Statics
            _statics = null;
    }
}
