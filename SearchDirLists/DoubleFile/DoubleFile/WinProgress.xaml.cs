using System.Windows;
using System.Linq;
using System.Collections.Generic;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinSaveInProgress.xaml
    /// </summary>
    partial class WinProgress : LocalWindow
    {
        internal string WindowTitle { set { Title = value; } }

        public WinProgress()
        {
            InitializeComponent();
        }

        LV_ProgressVM m_lv = new LV_ProgressVM();

        internal void InitProgress(IEnumerable<string> astrNicknames, IEnumerable<string> astrPaths)
        {
            if (astrNicknames.Count() != astrPaths.Count())
            {
                MBox.Assert(0, false);
                return;
            }

            for (int i = 0; i < astrPaths.Count(); ++i)
            {
                m_lv.NewItem(new string[] { astrNicknames.ElementAt(i), astrPaths.ElementAt(i) }, bQuiet: true);
            }
        }

        internal void SetProgress(string strPath, double nProgress)
        {
            var lvItem = (m_lv[strPath] as LVitem_ProgressVM);

            if (lvItem != null)
            {
                lvItem.Progress = nProgress;
            }
            else
            {
                MBox.Assert(0, false);
            }
        }

        internal void SetCompleted(string strPath)
        {
            var lvItem = (m_lv[strPath] as LVitem_ProgressVM);

            if (lvItem != null)
            {
                lvItem.SetCompleted();
            }
            else
            {
                MBox.Assert(0, false);
            }
        }

        System.Random rand = new System.Random();
        void Demo(string strNickname, string strDemoPath)
        {
            double nProgress = 0;
            var nIncrement = rand.NextDouble() / 10.0;

            if (nIncrement < .03)
            {
                return;     // demo indeterminate state
            }

            var act = new System.Action<double>(d => { (m_lv[strDemoPath] as LVitem_ProgressVM).Progress = d; });
            var timer = new System.Windows.Threading.DispatcherTimer();

            timer.Tick += new System.EventHandler((s, e) =>
            {
                nProgress += nIncrement;
                 
                if (nProgress > 1)
                {
                    (m_lv[strDemoPath] as LVitem_ProgressVM).SetCompleted();
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
            form_lvProgress.DataContext = m_lv;

            m_lv.SelectedOne = () => { return form_lvProgress.SelectedItems.Count == 1; };
            m_lv.SelectedAny = () => { return form_lvProgress.SelectedItems.Count > 0; };
            m_lv.Refresh = () => { }; // { form_lvProgress.Items.Refresh(); };
            m_lv.Selected = () => { return form_lvProgress.SelectedItems.Cast<LVitem_ProgressVM>(); };

            m_lv.GetWindow = () => { return this; };

            //Demo(m_lv, "Stuff at work", @"C:\My Documents");
            //Demo(m_lv, "", @"E:\Test");
            //Demo(m_lv, "Another test", @"C:\My Documents2");
            //Demo(m_lv, "", @"E:\Test2");
            //Demo(m_lv, "More stuff", @"E:\Test3");
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var worker = GlobalData.GetInstance().m_saveDirListings;

            if (worker == null)
            {
                return;
            }

            if (worker.IsAborted)
            {
                return;
            }

            if (MBox.ShowDialog("Do you want to cancel?", "Saving Directory Listings",
                MessageBoxButton.YesNo) ==
                MessageBoxResult.Yes)
            {
                worker.EndThread();
            }
            else
            {
                e.Cancel = true;
            }
        }
    }
}
