using System.Reactive.Linq;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinTreeMap.xaml
    /// </summary>
    public partial class WinExtraWindow
    {
        internal ILocalWindow WindowExtra = null;

        public WinExtraWindow()
        {
            InitializeComponent();

            Observable.FromEventPattern(formBtn_Activate, "Click")
                .LocalSubscribe(99758, x => WindowExtra?.Activate());
        }
    }
}
