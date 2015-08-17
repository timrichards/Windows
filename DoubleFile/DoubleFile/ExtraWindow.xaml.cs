using FirstFloor.ModernUI.Windows.Controls;
using System.Reactive.Linq;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for ExtraWindow.xaml
    /// </summary>
    public partial class ExtraWindow
    {
        public ExtraWindow()
        {
            InitializeComponent();

            //Observable.FromEventPattern(this, "Loaded")
            //    .LocalSubscribe(0, x => form_Overlay.IsOverlayContentVisible = true);
        }
    }
}
