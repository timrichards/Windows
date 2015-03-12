﻿using System.Windows;
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
        internal System.Func<bool> WindowClosingCallback { get; set; }

        internal WinProgress()
        {
            InitializeComponent();
            form_grid.Loaded += Grid_Loaded;
            ContentRendered += WinProgress_ContentRendered;
            Closing += Window_Closing;
            Closed += Window_Closed;
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
                _lv.NewItem(new[] { astrNicknames.ElementAt(i), astrPaths.ElementAt(i) }, bQuiet: true);
            }
        }

        internal void SetProgress(string strPath, double nProgress)
        {
            var lvItem = (_lv[strPath] as LVitem_ProgressVM);

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
            var lvItem = (_lv[strPath] as LVitem_ProgressVM);

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
            var lvItem = (_lv[strPath] as LVitem_ProgressVM);

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

        internal void CloseIfNatural()
        {
            if (_bClosing)
            {
                return;     // get an error otherwise
            }

            if (Aborted)
            {
                return;     // don't close: there may be an error message
            }

            if (_lv
                .ItemsCast
                .Any(lvItem => lvItem.Progress < 1))
            {
                return;
            }

            Aborted = true;
            UtilProject.UIthread(Close);
        }

        #region form_handlers
        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            form_lvProgress.DataContext = _lv;

            _lv.SelectedOne = () => form_lvProgress.SelectedItems.HasOnlyOne();
            _lv.SelectedAny = () => (false == form_lvProgress.SelectedItems.IsEmptyA());
            _lv.Selected = () => form_lvProgress.SelectedItems.Cast<LVitem_ProgressVM>();
        }

        private void WinProgress_ContentRendered(object sender, System.EventArgs e)
        {
            MinHeight = ActualHeight;
            MaxHeight = ActualHeight;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (Aborted)
            {
                _bClosing = true;
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
                if (UtilProject.UIthread(windowClosingCallback))
                {
                    Aborted = true;
                    UtilProject.UIthread(Close);
                }
                else
                {
                    WindowClosingCallback = windowClosingCallback;
                }
            }).Start();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {
            _lv.Dispose();
        }
        #endregion form_handlers

        readonly LV_ProgressVM _lv = new LV_ProgressVM();
        bool _bClosing = false;
    }
}