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

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            form_lvProgress.DataContext = m_lv;

            m_lv.SelectedOne = () => { return form_lvProgress.SelectedItems.Count == 1; };
            m_lv.SelectedAny = () => { return form_lvProgress.SelectedItems.Count > 0; };
            m_lv.Selected = () => { return form_lvProgress.SelectedItems.Cast<LVitem_ProgressVM>(); };
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
