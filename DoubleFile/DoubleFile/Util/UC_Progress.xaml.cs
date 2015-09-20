using System.Linq;
using System.Collections.Generic;
using System.Reactive.Linq;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows;

namespace DoubleFile
{
    interface IProgressOverlayClosing
    {
        bool ConfirmClose();
    }

    public partial class UC_Progress
    {
        public UC_Progress()
        {
            InitializeComponent();
        }
    }

    class _ProgressAsOverlay : ILocalWindow
    {
        internal string Title { get; set; }

        protected Action<CancelEventArgs> OKtoClose = null;
        internal ILocalWindow Close()
        {
            var cancelEventArgs = new CancelEventArgs { Cancel = false };

            OKtoClose?.Invoke(cancelEventArgs);

            if (false == cancelEventArgs.Cancel)
                _dispatcherFrame.Continue = false;

            return this;
        }

        protected void GoModeless() { _bWentModeless = true; _dispatcherFrame.Continue = false; }
        bool _bWentModeless = false;

        public bool Activate() { return false; }
        public bool LocalIsClosed => (false == _vm.Any());
        public bool SimulatingModal { get; set; } = true;

        LocalDispatcherFrame _dispatcherFrame = new LocalDispatcherFrame(99730);

        protected LV_ProgressVM
            _vm = null;

        internal _ProgressAsOverlay()
        {
            _vm = new LV_ProgressVM();
            _vm.Cancel_Action = () => Close();
        }

        protected Action<ProgressOverlay> _initClient = null;

        internal void
            ShowOverlay(LocalModernWindowBase window = null)
        {
            UC_Progress ucProgress = null;

            Util.UIthread(99729, () =>
            {
                if (null == window)
                    window = ((LocalModernWindowBase)Application.Current.MainWindow);

                window.Progress_Darken();
                _vm.Init();
                ucProgress = window.GetProgressCtl();
                ucProgress.DataContext = _vm;
                ucProgress.LocalShow();
            });

            Util.ThreadMake(() => _initClient?.Invoke((ProgressOverlay)this));
            _dispatcherFrame.PushFrameTrue();

            if (_bWentModeless)
                return;

            Util.UIthread(99635, () =>
            {
                ucProgress.LocalHide();
                window.Progress_Undarken();
            });

            _vm.Dispose();
        }
    }

    // Window_Closed() calls Dispose() on the LV_ProgressVM member.
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    partial class
        ProgressOverlay : _ProgressAsOverlay
    {
        internal WeakReference<IProgressOverlayClosing>
            WindowClosingCallback { private get; set; }

        internal ProgressOverlay
            AllowSubsequentProcess() { _bAllowSubsequentProcess = true; return this; }
        bool _bAllowSubsequentProcess = false;

        internal static ProgressOverlay
            CloseForced() => WithProgressOverlay(w =>
        {
            return
                (w.LocalIsClosed)
                ? w
                : (ProgressOverlay) w.Abort().Close();
        });
        internal ProgressOverlay
            Abort() { _bAborted = true; return this; }
        bool _bAborted = false;

        internal
            ProgressOverlay(IEnumerable<string> astrBigLabels, IEnumerable<string> astrSmallKeyLabels, Action<ProgressOverlay> initClient)
        {
            WithProgressOverlay(w =>
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
            OKtoClose = Window_Closing;
            _vm.Add(astrBigLabels.Zip(astrSmallKeyLabels, (a, b) => Tuple.Create(a, b)));
        }

        internal ProgressOverlay
            SetProgress(string strPath, double nProgress)
        {
            if (false ==
                _vm[strPath]
                .FirstOnlyAssert(lvItem => lvItem.Progress = nProgress))
            {
                Util.Assert(99969, false);
            }

            return this;
        }

        internal ProgressOverlay
            SetCompleted(string strPath)
        {
            if (false ==
                _vm[strPath]
                .FirstOnlyAssert(lvItem =>
            {
                lvItem.SetCompleted();

                if (_vm.ItemsCast.All(lvItemA => lvItemA.IsCompleted))
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

        internal ProgressOverlay
            SetError(string strSmallKeyLabel, string strError)
        {
            if (false == _vm[strSmallKeyLabel].FirstOnlyAssert(lvItem => lvItem.SetError(strError)))
                Util.Assert(99956, false);

            return this;
        }

        internal ProgressOverlay
            CloseIfNatural()
        {
            if (_bClosing)
                return this;     // get an error otherwise

            StopShowingConfirmMessage();

            if (_bAborted)
                return this;     // don't close: there may be an error message

            if (_vm.ItemsCast
                .Any(lvItem => 1 > lvItem.Progress))
            {
                return this;
            }

            _bAborted = true;
            base.Close();
            return this;
        }

        ProgressOverlay
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
                _vm.Dispose();
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
                    WindowClosingCallback = new WeakReference<IProgressOverlayClosing>(windowClosing);
                }
            });
        }

        internal static T
            WithProgressOverlay<T>(Func<ProgressOverlay, T> doSomethingWith) => _wr.Get(o => doSomethingWith(o));

        static readonly WeakReference<ProgressOverlay> _wr = new WeakReference<ProgressOverlay>(null);

        bool
            _bClosing = false;
        DateTime
            _dtConfirmingClose = DateTime.MinValue;
    }
}
