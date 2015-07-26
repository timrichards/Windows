using System.Windows;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Reactive.Linq;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace DoubleFile
{
    interface IWinProgressClosing
    {
        bool ConfirmClose();
    }

    // Window_Closed() calls Dispose() on the LV_ProgressVM member.
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    partial class WinProgress : IModalWindow
    {
        internal WeakReference<IWinProgressClosing>
            WindowClosingCallback { set; private get; }

        internal WinProgress
            AllowSubsequentProcess() { _bAllowSubsequentProcess = true; return this; }
        bool _bAllowSubsequentProcess = false;

        internal WinProgress
            Abort() { _bAborted = true; return this; }
        bool _bAborted = false;

        internal WinProgress(IEnumerable<string> astrBigLabels, IEnumerable<string> astrSmallKeyLabels, Action<WinProgress> initClient)
        {
            WithWinProgress(w =>
            {
                if (w.LocalIsClosed ||
                    w._bAllowSubsequentProcess)
                {
                    return false;   // from lambda
                }

                Util.Assert(99792, false);
                w.Abort().Close();
                return false;       // from lambda
            });

            _wr.SetTarget(this);
            InitializeComponent();
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;

            var rc = Win32Screen.GetWindowRect(Application.Current.MainWindow);

            Left = rc.Left;
            Width = rc.Width;

            Observable.FromEventPattern(this, "SourceInitialized")
                .LocalSubscribe(x => ResizeMode = ResizeMode.NoResize);

            Observable.FromEventPattern(this, "Loaded")
                .LocalSubscribe(x => formLV_Progress.DataContext = _lv);

            Observable.FromEventPattern(this, "ContentRendered")
                .LocalSubscribe(x =>
            {
                MinHeight = MaxHeight = ActualHeight;
                initClient(this);
            });

            Observable.FromEventPattern(formBtn_Cancel, "Click")
                .LocalSubscribe(x => base.Close());

            Observable.FromEventPattern<CancelEventArgs>(this, "Closing")
                .LocalSubscribe(args => Window_Closing(args.EventArgs));

            _lv.Add(astrBigLabels.Zip(astrSmallKeyLabels, (a, b) => Tuple.Create(a, b)));
        }

        internal WinProgress
            SetProgress(string strPath, double nProgress)
        {
            if (false ==
                _lv[strPath]
                .FirstOnlyAssert(lvItem => lvItem.Progress = nProgress - double.Epsilon))
            {
                Util.Assert(99969, false);
            }

            return this;
        }

        internal WinProgress
            SetCompleted(string strPath)
        {
            if (false ==
                _lv[strPath]
                .FirstOnlyAssert(lvItem =>
            {
                lvItem.SetCompleted();

                if (_lv.ItemsCast.All(lvItemA => 1 == lvItemA.Progress))
                {
                    Util.UIthread(99827, () => formBtn_Cancel.ToolTip = "Process completed. You may now close the window");

                    if (_bAllowSubsequentProcess)
                        GoModeless();
                }
            }))
            {
                Util.Assert(99968, false);
            }

            return this;
        }

        internal WinProgress
            SetError(string strSmallKeyLabel, string strError)
        {
            if (false == _lv[strSmallKeyLabel].FirstOnlyAssert(lvItem => lvItem.SetError(strError)))
                Util.Assert(99956, false);

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
            base.Close();
            return this;
        }

        void
            Window_Closing(CancelEventArgs e)
        {
            IWinProgressClosing windowClosing = null;

            _bClosing = Util.Closure(() =>
            {
                if (_bAborted)
                {
                    WindowClosingCallback = null;
                    return true;    // from lambda
                }

                WindowClosingCallback?.TryGetTarget(out windowClosing);

                if (null == windowClosing)
                    return true;    // from lambda

                return false;       // from lambda
            });

            if (_bClosing)
            {
                _lv.Dispose();
                return;
            }

            e.Cancel = true;

            // Squib load: return without closing.
            // This is a bunch of bootstrapping stuff.
            // WindowClosingCallback needs to be called separate from the Window_Closing event;
            // otherwise when the process that called the progress window completes, and
            // WindowClosingCallback hasn't returned yet (from a messagebox), then it will
            // blockade and freeze up.

            WindowClosingCallback = null;

            Util.ThreadMake(() =>
            {
                bool bConfirmClose = false;

                Util.UIthread(99824, () => bConfirmClose = windowClosing.ConfirmClose());

                if (bConfirmClose)
                {
                    _bAborted = true;
                    base.Close();
                }
                else
                {
                    WindowClosingCallback = new WeakReference<IWinProgressClosing>(windowClosing);
                }
            });
        }

        internal static T
            WithWinProgress<T>(Func<WinProgress, T> doSomethingWith) => _wr.Get(o => doSomethingWith(o));
        static WeakReference<WinProgress> _wr = new WeakReference<WinProgress>(null);
        
        readonly LV_ProgressVM
            _lv = new LV_ProgressVM();
        bool
            _bClosing = false;
    }
}
