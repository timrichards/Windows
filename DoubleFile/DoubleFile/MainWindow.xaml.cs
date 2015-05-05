using FirstFloor.ModernUI.Windows.Controls;
using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for ModernWindow1.xaml
    /// </summary>
    public partial class ModernWindow1
    {
        public ModernWindow1()
        {
            InitializeComponent();
            App.LocalMainWindow = this;
            App.TopWindow = this;
        }
    }
}
