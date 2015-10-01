using System.Reactive.Linq;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinTreemap.xaml
    /// </summary>
    public partial class UC_ExtraWindow
    {
        internal ILocalWindow WindowExtra = null;

        public UC_ExtraWindow()
        {
            InitializeComponent();

            Observable.FromEventPattern(formBtn_Activate, "Click")
                .LocalSubscribe(99758, x => WindowExtra?.Activate());
        }
    }
}
