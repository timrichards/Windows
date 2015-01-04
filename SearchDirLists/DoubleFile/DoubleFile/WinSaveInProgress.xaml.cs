using System.Windows;
using System.Linq;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinSaveInProgress.xaml
    /// </summary>
    partial class WinSaveInProgress : Window
    {
        public WinSaveInProgress()
        {
            InitializeComponent();
        }

        System.Random rand = new System.Random();
        void Demo(LV_ProgressVM lv, string strNickname, string strDemoPath)
        {
            lv.NewItem(new string[] { strNickname, strDemoPath });

            double nProgress = 0;
            var nIncrement = rand.NextDouble() / 10.0;

            if (nIncrement < .03)
            {
                return;     // demo indeterminate state
            }

            var act = new System.Action<double>(d => { (lv[strDemoPath] as LVitem_ProgressVM).SetProgress(d); });
            var timer = new System.Windows.Threading.DispatcherTimer();

            timer.Tick += new System.EventHandler((s, e) =>
            {
                nProgress += nIncrement;
                 
                if (nProgress > 1)
                {
                    (lv[strDemoPath] as LVitem_ProgressVM).SetCompleted();
                    timer.Stop();
                }

                Dispatcher.Invoke(act, System.Windows.Threading.DispatcherPriority.Background,
                    new object[] { nProgress });
            });

            timer.Interval = new System.TimeSpan(0, 0, 1);
            timer.Start();
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            var lv = new LV_ProgressVM();

            form_lvProgress.DataContext = lv;

            lv.SelectedOne = () => { return form_lvProgress.SelectedItems.Count == 1; };
            lv.SelectedAny = () => { return form_lvProgress.SelectedItems.Count > 0; };
            lv.Refresh = () => { }; // { form_lvProgress.Items.Refresh(); };
            lv.Selected = () => { return form_lvProgress.SelectedItems.Cast<LVitem_ProgressVM>(); };

            lv.GetWindow = () => { return this; };

            Demo(lv, "Stuff at work", @"C:\My Documents");
            Demo(lv, "", @"E:\Test");
            Demo(lv, "Another test", @"C:\My Documents2");
            Demo(lv, "", @"E:\Test2");
            Demo(lv, "More stuff", @"E:\Test3");
        }
    }
}
