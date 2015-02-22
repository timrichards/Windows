using System.Windows;
using System.Linq;
using System.Collections.Generic;
using System.Threading;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinSaveInProgress.xaml
    /// </summary>

    // Window_Closed() calls Dispose() on the LV_ProgressVM member.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    partial class WinProgress
    {
        internal BoolAction WindowClosingCallback = null;

        internal WinProgress()
        {
            InitializeComponent();
        }

        internal void InitProgress(IReadOnlyList<string> astrNicknames, IReadOnlyList<string> astrPaths)
        {
            if (astrNicknames.Count() != astrPaths.Count())
            {
                MBoxStatic.Assert(99932, false);
                return;
            }

            for (var i = 0; i < astrPaths.Count(); ++i)
            {
                m_lv.NewItem(new[] { astrNicknames.ElementAt(i), astrPaths.ElementAt(i) }, bQuiet: true);
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
                MBoxStatic.Assert(99931, false);
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
                MBoxStatic.Assert(99930, false);
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
                MBoxStatic.Assert(99929, false);
            }
        }

        internal bool Aborted { set; private get; }

        internal new void Close()
        {
            if (m_bClosing)
            {
                MBoxStatic.Assert(99928, false, bTraceOnly: true);
                return;     // some sort of lockup?
            }

            base.Close();
        }

        internal void CloseIfNatural()
        {
            if (m_bClosing)
            {
                return;     // get an error otherwise
            }

            if (Aborted)
            {
                return;     // don't close: there may be an error message
            }

            if (m_lv
                .ItemsCast
                .Any(lvItem => lvItem.Progress < 1))
            {
                return;
            }

            Aborted = true;
            UtilProject.UIthread(() => Close());
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            form_lvProgress.DataContext = m_lv;

            m_lv.SelectedOne = () => form_lvProgress.SelectedItems.HasOnlyOne();
            m_lv.SelectedAny = () => (false == form_lvProgress.SelectedItems.IsEmptyA());
            m_lv.Selected = () => form_lvProgress.SelectedItems.Cast<LVitem_ProgressVM>();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Aborted)
            {
                m_bClosing = true;
                return;     // close
            }

            if (null == WindowClosingCallback)
            {
                return;
            }

            e.Cancel = true;

            // Squib load: return without closing.
            // This is a bunch of bootstrapping stuff.
            // WindowClosingCallback needs to be called separate from the Window_Closing event;
            // otherwise when the process that called the progress window completes, and
            // WindowClosingCallback hasn't returned yet (from a messagebox), then it will
            // blockade and freeze up.

            var windowClosingCallback = WindowClosingCallback;

            WindowClosingCallback = null;

            new Thread(() =>
            {
                if ((bool)UtilProject.UIthread(windowClosingCallback))
                {
                    Aborted = true;
                    UtilProject.UIthread(() => Close());
                }
                else
                {
                    WindowClosingCallback = windowClosingCallback;
                }
            }).Start();
        }

        private void WinProgress_ContentRendered(object sender, System.EventArgs e)
        {
            MinHeight = ActualHeight;
            MaxHeight = ActualHeight;
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            m_lv.Dispose();
        }

        readonly LV_ProgressVM m_lv = new LV_ProgressVM();
        bool m_bClosing = false;
    }
}
