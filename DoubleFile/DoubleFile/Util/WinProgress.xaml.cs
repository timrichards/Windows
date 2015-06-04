using System.Windows;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Reactive.Linq;
using System;

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
        internal bool Aborted { set; private get; }
        internal bool AllowNewProcess { set; private get; }

        internal WeakReference<IWinProgressClosing> WindowClosingCallback{ get; set; }

        internal WinProgress()
        {
            InitializeComponent();

            var rc = MainWindow.WithMainWindow(Win32Screen.GetWindowRect);

            Left = rc.Left;
            Width = rc.Width;

            Observable.FromEventPattern(this, "SourceInitialized")
                .Subscribe(x => ResizeMode = ResizeMode.NoResize);

            Observable.FromEventPattern(form_grid, "Loaded")
                .Subscribe(x =>
            {
                formLV_Progress.DataContext = _lv;
                _lv.SelectedOne = () => formLV_Progress.SelectedItems.HasOnlyOne();
                _lv.SelectedAny = () => (false == formLV_Progress.SelectedItems.IsEmptyA());
                _lv.Selected = () => formLV_Progress.SelectedItems.Cast<LVitem_ProgressVM>();

                var lastWindow = _lsNativeWindow.LastOrDefault();

                if (null != lastWindow)
                    NativeMethods.BringWindowToTop(lastWindow);

                _lsNativeWindow.Add(this);
            });

            Observable.FromEventPattern(this, "ContentRendered")
                .Subscribe(x => MinHeight = MaxHeight = ActualHeight);

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
            _lv.Add(astrNicknames.Zip(astrPaths, (a, b) => new[] { a, b }), bQuiet: true);
            return this;
        }

        internal void SetProgress(string strPath, double nProgress)
        {
            var lvItem = _lv[strPath].FirstOrDefault();

            if (null != lvItem)
                lvItem.Progress = nProgress;
            else
                MBoxStatic.Assert(99931, false);
        }

        internal void SetCompleted(string strPath)
        {
            var lvItem = _lv[strPath];

            if (null != lvItem)
                lvItem.FirstOrDefault().SetCompleted();
            else
                MBoxStatic.Assert(99930, false);

            if (_lv.ItemsCast.All(lvItemA => 1 == lvItemA.Progress) &&
                AllowNewProcess)
            {
                StopSimulatingModal();
            }
        }

        internal void SetError(string strPath, string strError)
        {
            var lvItem = _lv[strPath];

            if (null != lvItem)
                lvItem.FirstOrDefault().SetError(strError);
            else
                MBoxStatic.Assert(99929, false);
        }

        internal void CloseIfNatural()
        {
            if (_bClosing)
                return;     // get an error otherwise

            if (Aborted)
                return;     // don't close: there may be an error message

            if (_lv
                .ItemsCast
                .Any(lvItem => 1 > lvItem.Progress))
            {
                return;
            }

            Aborted = true;
            Util.UIthread(Close);
        }

        private void Window_Closing(System.ComponentModel.CancelEventArgs e)
        {
            if (Aborted)
            {
                WindowClosingCallback = null;
                _bClosing = true;

                _lsNativeWindow
                    .Where(w => w.Equals(this))
                    .ToArray()
                    .ForEach(w => _lsNativeWindow.Remove(w));

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

        readonly LV_ProgressVM
            _lv = new LV_ProgressVM();
        bool
            _bClosing = false;
        static List<NativeWindow>
            _lsNativeWindow = new List<NativeWindow>();
    }
}
