﻿using System.Windows;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Reactive.Linq;
using System;
using System.Windows.Interop;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace DoubleFile
{
    interface IWinProgressClosing
    {
        bool ConfirmClose();
    }

    // Window_Closed() calls Dispose() on the LV_ProgressVM member.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    partial class WinProgress
    {
        internal WeakReference<IWinProgressClosing> WindowClosingCallback{ get; set; }

        internal WinProgress()
        {
            InitializeComponent();

            MainWindow.WithMainWindow(mainWindow =>
            {
                var rc = default(RECT);

                NativeMethods.Call(() => NativeMethods
                    .GetWindowRect(new WindowInteropHelper(mainWindow).Handle, out rc));

                Left = rc.Left;
                Width = rc.Width;
                return false;
            });

            Observable.FromEventPattern(this, "SourceInitialized")
                .Subscribe(x => ResizeMode = ResizeMode.NoResize);

            Observable.FromEventPattern(form_grid, "Loaded")
                .Subscribe(x => Grid_Loaded());

            Observable.FromEventPattern(this, "ContentRendered")
                .Subscribe(x => WinProgress_ContentRendered());

            Observable.FromEventPattern(formBtn_Cancel, "Click")
                .Subscribe(x => Close());

            Observable.FromEventPattern<System.ComponentModel.CancelEventArgs>(this, "Closing")
                .Subscribe(args => Window_Closing(args.EventArgs));
        }

        internal WinProgress(IEnumerable<string> astrNicknames, IEnumerable<string> astrPaths)
            : this()
        {
            InitProgress(astrNicknames, astrPaths);
        }

        internal WinProgress InitProgress(IEnumerable<string> astrNicknames, IEnumerable<string> astrPaths)
        {
            if (astrNicknames.Count() != astrPaths.Count())
            {
                MBoxStatic.Assert(99932, false);
                return this;
            }

            for (var i = 0; i < astrPaths.Count(); ++i)
                _lv.Add(new[] { astrNicknames.ElementAt(i), astrPaths.ElementAt(i) }, bQuiet: true);

            return this;
        }

        internal void SetProgress(string strPath, double nProgress)
        {
            var lvItem = _lv[strPath].FirstOrDefault();

            if (lvItem != null)
                lvItem.Progress = nProgress;
            else
                MBoxStatic.Assert(99931, false);
        }

        internal void SetCompleted(string strPath)
        {
            var lvItem = _lv[strPath];

            if (lvItem != null)
                lvItem.FirstOrDefault().SetCompleted();
            else
                MBoxStatic.Assert(99930, false);
        }

        internal void SetError(string strPath, string strError)
        {
            var lvItem = _lv[strPath];

            if (lvItem != null)
                lvItem.FirstOrDefault().SetError(strError);
            else
                MBoxStatic.Assert(99929, false);
        }

        internal bool Aborted { set; private get; }

        internal void CloseIfNatural()
        {
            if (_bClosing)
                return;     // get an error otherwise

            if (Aborted)
                return;     // don't close: there may be an error message

            if (_lv
                .ItemsCast
                .Any(lvItem => lvItem.Progress < 1))
            {
                return;
            }

            Aborted = true;
            Util.UIthread(Close);
        }

        #region form_handlers
        private void Grid_Loaded()
        {
            formLV_Progress.DataContext = _lv;

            _lv.SelectedOne = () => formLV_Progress.SelectedItems.HasOnlyOne();
            _lv.SelectedAny = () => (false == formLV_Progress.SelectedItems.IsEmptyA());
            _lv.Selected = () => formLV_Progress.SelectedItems.Cast<LVitem_ProgressVM>();
        }

        private void WinProgress_ContentRendered()
        {
            MinHeight = ActualHeight;
            MaxHeight = ActualHeight;
        }

        private void Window_Closing(System.ComponentModel.CancelEventArgs e)
        {
            if (Aborted)
            {
                WindowClosingCallback = null;
                _bClosing = true;
                return;     // close
            }

            if (null == WindowClosingCallback)
                return;

            IWinProgressClosing windowClosing = null;
            
            WindowClosingCallback.TryGetTarget(out windowClosing);

            if (null == windowClosing)
                return;

            e.Cancel = true;

            // Squib load: return without closing.
            // This is a bunch of bootstrapping stuff.
            // WindowClosingCallback needs to be called separate from the Window_Closing event;
            // otherwise when the process that called the progress window completes, and
            // WindowClosingCallback hasn't returned yet (from a messagebox), then it will
            // blockade and freeze up.

            WindowClosingCallback = null;

            new Thread(() =>
            {
                if (Util.UIthread(() => windowClosing.ConfirmClose()))
                {
                    Aborted = true;
                    Util.UIthread(Close);
                }
                else
                {
                    WindowClosingCallback = new WeakReference<IWinProgressClosing>(windowClosing);
                }
            }).Start();
        }

        #endregion form_handlers

        readonly LV_ProgressVM _lv = new LV_ProgressVM();
        bool _bClosing = false;
    }
}
