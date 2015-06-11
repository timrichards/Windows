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
        internal WeakReference<IWinProgressClosing>
            WindowClosingCallback { set; private get; }

        internal WinProgress
            AllowSubsequentProcess() { _bAllowSubsequentProcess = true; return this; }
        bool _bAllowSubsequentProcess = false;

        internal WinProgress
            SetAborted() { _bAborted = true; return this; }
        bool _bAborted = false;

        internal
            WinProgress()
        {
            InitializeComponent();

            var rc = MainWindow.WithMainWindow(Win32Screen.GetWindowRect);

            Left = rc.Left;
            Width = rc.Width;

            Observable.FromEventPattern(this, "SourceInitialized")
                .Subscribe(x => ResizeMode = ResizeMode.NoResize);

            Observable.FromEventPattern(form_grid, "Loaded")
                .Subscribe(x => formLV_Progress.DataContext = _lv);

            Observable.FromEventPattern(this, "ContentRendered")
                .Subscribe(x =>
            {
                MinHeight = MaxHeight = ActualHeight;
                MBoxStatic.Restart();
            });

            Observable.FromEventPattern(formBtn_Cancel, "Click")
                .Subscribe(x => Close());

            Observable.FromEventPattern<System.ComponentModel.CancelEventArgs>(this, "Closing")
                .Subscribe(args => Window_Closing(args.EventArgs));
        }

        internal
            WinProgress(IEnumerable<string> astrNicknames, IEnumerable<string> astrPaths)
            : this()
        {
            InitProgress(astrNicknames, astrPaths);
        }

        internal WinProgress
            InitProgress(IEnumerable<string> astrNicknames, IEnumerable<string> astrPaths)
        {
            _lv.Add(astrNicknames.Zip(astrPaths, (a, b) => Tuple.Create(a, b)));
            return this;
        }

        internal WinProgress
            SetProgress(string strPath, double nProgress)
        {
            if (false == _lv[strPath].FirstOnlyAssert(lvItem => lvItem.Progress = nProgress))
                MBoxStatic.Assert(99969, false);

            return this;
        }

        internal WinProgress
            SetCompleted(string strPath)
        {
            if (false == _lv[strPath].FirstOnlyAssert(lvItem =>
            {
                lvItem.SetCompleted();

                if (_lv.ItemsCast.All(lvItemA => 1 == lvItemA.Progress) &&
                    _bAllowSubsequentProcess)
                {
                    Util.UIthread(() => formBtn_Cancel.ToolTip = "Process completed. You can close the window now");
                    GoModeless();
                }
            }))
            {
                MBoxStatic.Assert(99968, false);
            }

            return this;
        }

        internal WinProgress
            SetError(string strPath, string strError)
        {
            if (false == _lv[strPath].FirstOnlyAssert(lvItem => lvItem.SetError(strError)))
                MBoxStatic.Assert(99956, false);

            return this;
        }

        internal WinProgress
            CloseIfNatural()
        {
            if (_bClosing)
                return this;     // get an error otherwise

            if (_bAborted)
                return this;     // don't close: there may be an error message

            if (_lv.ItemsCast
                .Any(lvItem => 1 > lvItem.Progress))
            {
                return this;
            }

            _bAborted = true;
            Util.UIthread(Close);
            return this;
        }

        void
            Window_Closing(System.ComponentModel.CancelEventArgs e)
        {
            if (_bAborted)
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
                    _bAborted = true;
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
    }
}
