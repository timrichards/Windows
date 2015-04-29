using FirstFloor.ModernUI.Windows.Controls;
using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for ModernWindow1.xaml
    /// </summary>
    public partial class ModernWindow1 : ModernWindow
    {
        public ModernWindow1()
        {
            InitializeComponent();
            ((Window)_mainWindow).Show();
        }

        MainWindow _mainWindow = new MainWindow();
    }
}
