using System.Windows;
using System.Linq;
using System.Collections.Generic;
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

    class _ProgressAsOverlay : ILocalWindow
    {
        internal string Title { get; set; }

        protected Action<CancelEventArgs> OKtoClose = null;
        internal ILocalWindow Close()
        {
            CancelEventArgs cancelEventArgs = new CancelEventArgs { Cancel = false };

            OKtoClose?.Invoke(cancelEventArgs);

            if (false == cancelEventArgs.Cancel)
                _dispatcherFrame.Continue = false;

            return this;
        }

        protected void GoModeless() { _bWentModeless = true; _dispatcherFrame.Continue = false; }
        bool _bWentModeless = false;

        public bool Activate() { return false; }
        public bool LocalIsClosed => (false == _lv.Any());
        public bool SimulatingModal { get; set; } = true;

        LocalDispatcherFrame _dispatcherFrame = new LocalDispatcherFrame(0);

        protected LV_ProgressVM
            _lv = null;

        internal _ProgressAsOverlay()
        {
            _lv = MainWindow.WithMainWindow(w => w.DataContext.As<LV_ProgressVM>());
        }

        protected Action<WinProgress> _initClient = null;

        internal void ShowDialog()
        {
            var mainWindow = MainWindow.WithMainWindow(w => w);
            
            mainWindow.ShowProgress();

            Observable.Timer(TimeSpan.FromMilliseconds(50)).Timestamp()
                .LocalSubscribe(0, x => Util.UIthread(0, () => _initClient?.Invoke((WinProgress)this)));

            _dispatcherFrame.PushFrameTrue();

            if (_bWentModeless)
                return;

            mainWindow.Progress_Close();
            _lv.ClearItems();
        }
    }

    // Window_Closed() calls Dispose() on the LV_ProgressVM member.
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    partial class
        WinProgress : _ProgressAsOverlay
    {
        internal WeakReference<IWinProgressClosing>
            WindowClosingCallback { private get; set; }

        internal WinProgress
            AllowSubsequentProcess() { _bAllowSubsequentProcess = true; return this; }
        bool _bAllowSubsequentProcess = false;

        internal static WinProgress
            CloseForced() => WithWinProgress(w =>
        {
            return
                (w.LocalIsClosed)
                ? w
                : (WinProgress) w.Abort().Close();
        });
        internal WinProgress
            Abort() { _bAborted = true; return this; }
        bool _bAborted = false;

        internal
            WinProgress(IEnumerable<string> astrBigLabels, IEnumerable<string> astrSmallKeyLabels, Action<WinProgress> initClient)
        {
            WithWinProgress(w =>
            {
                if (w.LocalIsClosed ||
                    w._bAllowSubsequentProcess)
                {
                    return w;   // from lambda
                }

   //             Util.Assert(99792, false);

                return
                    w           // from lambda
                    .Abort()
                    .Close();
            });

            _wr.SetTarget(this);
            _initClient = initClient;
            _lv.Cancel_Action = () => base.Close();
            OKtoClose = Window_Closing;
            _lv.Add(astrBigLabels.Zip(astrSmallKeyLabels, (a, b) => Tuple.Create(a, b)));
        }

        internal WinProgress
            SetProgress(string strPath, double nProgress)
        {
            if (false ==
                _lv[strPath]
                .FirstOnlyAssert(lvItem => lvItem.Progress = nProgress))
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

                if (_lv.ItemsCast.All(lvItemA => lvItemA.IsCompleted))
                {
 //                   Util.UIthread(99827, () => formBtn_Cancel.ToolTip = "Process completed. You may now close the window");
                    StopShowingConfirmMessage();

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

            StopShowingConfirmMessage();

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

        WinProgress
            StopShowingConfirmMessage()
        {
            if (DateTime.MinValue == _dtConfirmingClose)
                return this;

            //if (TimeSpan.FromSeconds(2) < DateTime.Now - _dtConfirmingClose)
            //    return this;        // don't yank out the message box before they can read it

            if (_bAborted)
                return this;        // don't close: there may be an error message

            if (MBoxStatic.AssertUp)
                return this;        // don't close: there Is an error message

            //Util.UIthread(99774, () =>
            //    OwnedWindows
            //    .Cast<Window>()
            //    .ToList()
            //    .FirstOnlyAssert(w =>
            //{
            //    w.Close();
            //    _dtConfirmingClose = DateTime.MinValue;
            //}));

            return this;
        }

        void
            Window_Closing(CancelEventArgs e)
        {
            var windowClosing = WindowClosingCallback?.Get(w => w);

            _bClosing = Util.Closure(() =>
            {
                if (_bAborted)
                {
                    WindowClosingCallback = null;
                    return true;    // from lambda
                }

                return (null == windowClosing);     // from lambda
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

                _dtConfirmingClose = DateTime.Now;
                Util.UIthread(99824, () => bConfirmClose = windowClosing.ConfirmClose());
                _dtConfirmingClose = DateTime.MinValue;

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

        static readonly WeakReference<WinProgress> _wr = new WeakReference<WinProgress>(null);

        bool
            _bClosing = false;
        DateTime
            _dtConfirmingClose = DateTime.MinValue;
    }
}
