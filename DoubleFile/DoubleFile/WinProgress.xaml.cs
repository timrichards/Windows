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
        internal BoolAction WindowClosingCallback = null;
        internal string WindowTitle { set { Title = value; } }

        internal WinProgress()
        {
            InitializeComponent();
        }

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

        internal void SetError(string strPath, string strError)
        {
            var lvItem = (m_lv[strPath] as LVitem_ProgressVM);

            if (lvItem != null)
            {
                lvItem.SetError(strError);
            }
            else
            {
                MBox.Assert(0, false);
            }
        }

        internal bool Aborted { set; private get; }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            form_lvProgress.DataContext = m_lv;

            m_lv.SelectedOne = () => { return form_lvProgress.SelectedItems.Count == 1; };
            m_lv.SelectedAny = () => { return form_lvProgress.SelectedItems.Count > 0; };
            m_lv.Selected = () => { return form_lvProgress.SelectedItems.Cast<LVitem_ProgressVM>(); };
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Aborted)
            {
                return;
            }

            if (WindowClosingCallback != null)
            {
                e.Cancel = (false == WindowClosingCallback());
            }
        }

        private void WinProgress_ContentRendered(object sender, System.EventArgs e)
        {
            MinHeight = ActualHeight;
            MaxHeight = ActualHeight;
        }

        LV_ProgressVM m_lv = new LV_ProgressVM();
    }
}
