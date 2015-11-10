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

    interface IDimForMessagebox
    {
        ProgressOverlay Go(Action showMessagebox);
    }

    public partial class UC_Progress
    {
        public UC_Progress()
        {
            InitializeComponent();
        }
    }

    // Window_Closed() calls Dispose() on the LV_ProgressVM member.
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    class
        ProgressOverlay : IDimForMessagebox
    {
        internal string
            Title { get; set; }
        internal bool
            LocalIsClosed => null == _vm;

        internal WeakReference<IProgressOverlayClosing>
            WindowClosingCallback { private get; set; }

        internal ProgressOverlay
            AllowSubsequentProcess() { _bAllowSubsequentProcess = true; return this; }
        bool _bAllowSubsequentProcess = false;
        void GoModeless() { _bWentModeless = true; _dispatcherFrame.Continue = false; }
        bool _bWentModeless = false;

        static internal T
            WithProgressOverlay<T>(Func<ProgressOverlay, T> doSomethingWith) => _wr.Get(o => doSomethingWith(o));
        static readonly WeakReference<ProgressOverlay> _wr = new WeakReference<ProgressOverlay>(null);

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
            _ieStr = astrBigLabels.Zip(astrSmallKeyLabels, (a, b) => Tuple.Create(a, b));
        }

        internal ProgressOverlay
            ShowOverlay(LocalModernWindowBase window = null) { _window = window; return ShowOverlay(); }
        ProgressOverlay ShowOverlay()
        {
            _lsDisposable.Add(_vm = new LV_ProgressVM());
            _vm.Add(_ieStr);

            Util.UIthread(99729, () =>
            {
                if (null == _window)
                    _window = (LocalModernWindowBase)Application.Current.MainWindow;

                _window.ProgressCtl.DataContext = _vm;
                _vm.Init();
                _window.Progress_Darken();
            });

            _lsDisposable.Add(Observable.FromEventPattern(_window.ProgressCtl.formBtn_Cancel, "Click")
                .LocalSubscribe(99621, x => Close()));

            Util.ThreadMake(() => _initClient?.Invoke(this));
            _dispatcherFrame.PushFrameTrue();

            if (false == _bWentModeless)
                LocalDispose();

            return this;
        }

        ProgressOverlay
            LocalDispose()
        {
            Util.LocalDispose(_lsDisposable);
            _wr.SetTarget(null);
            _vm = null;

            Util.UIthread(99622, () =>
            {
                _window.Progress_Undarken();
                _window.ProgressCtl.DataContext = null;
            });

            return this;
        }

        ProgressOverlay
            IDimForMessagebox.Go(Action showMessagebox)
        {
            _window.ProgressCtl.LocalHide(99637);
            showMessagebox();
            _window.ProgressCtl.LocalShow(99637);
            return this;
        }

        internal ProgressOverlay
            Close()
        {
            var cancelEventArgs = new CancelEventArgs { Cancel = false };

            Window_Closing(cancelEventArgs);

            if (false == cancelEventArgs.Cancel)
                _dispatcherFrame.Continue = false;

            if (_bWentModeless)
                LocalDispose();

            return this;
        }

        static internal ProgressOverlay
            CloseForced() => WithProgressOverlay(w =>
        {
            return
                (w.LocalIsClosed)
                ? w
                : w.Abort().Close();
        });
        internal ProgressOverlay
            Abort() { _bAborted = true; return this; }
        bool _bAborted = false;

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
            Close();
            return this;
        }

        internal ProgressOverlay
            SetProgress(string strSmallKeyLabel, double nProgress)
        {
            if (false ==
                _vm[strSmallKeyLabel]
                .FirstOnlyAssert(lvItem => lvItem.Progress = nProgress))
            {
                Util.Assert(99969, false);
            }

            return this;
        }

        internal ProgressOverlay
            ResetEstimate(string strSmallKeyLabel)
        {
            if (false ==
                _vm[strSmallKeyLabel]
                .FirstOnlyAssert(lvItem => lvItem.ResetEstimate()))
            {
                Util.Assert(99873, false);
            }

            return this;
        }

        internal ProgressOverlay
            SetCompleted(string strSmallKeyLabel)
        {
            if (false ==
                _vm[strSmallKeyLabel]
                .FirstOnlyAssert(lvItem =>
            {
                lvItem.SetCompleted();
                GoModelessIfNotRunning();
            }))
            {
                Util.Assert(99968, false);
            }

            return this;
        }

        internal ProgressOverlay
            SetError(string strSmallKeyLabel, string strError)
        {
            if (false == _vm?[strSmallKeyLabel].FirstOnlyAssert(lvItem =>
            {
                lvItem.SetError(strError);
                GoModelessIfNotRunning();
            }))
            {
                Util.Assert(99956, false);
            }

            return this;
        }

        void GoModelessIfNotRunning()
        {
            if (_vm?.ItemsCast.All(lvItemA => false == lvItemA.IsRunning) ?? false)
            {
                Util.UIthread(99827, () => _window.ProgressCtl.formBtn_Cancel.ToolTip = "Process completed. You may now close the window");
                StopShowingConfirmMessage();

                if (_bAllowSubsequentProcess)
                    GoModeless();
            }
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

            if (null != MBoxStatic.FailUp)
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

                if (_vm.ItemsCast.All(lvItemA => false == lvItemA.IsRunning))
                    return true;    // from lambda

                return (null == windowClosing);     // from lambda
            });

            if (_bClosing)
            {
                if (DateTime.MinValue != _dtConfirmingClose)
                    UC_Messagebox.Kill();

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
                    Close();
                }
                else
                {
                    WindowClosingCallback = new WeakReference<IProgressOverlayClosing>(windowClosing);
                }
            });
        }

        LocalDispatcherFrame
            _dispatcherFrame = new LocalDispatcherFrame(99730);
        LV_ProgressVM
            _vm = null;
        Action<ProgressOverlay>
            _initClient = null;
        IEnumerable<Tuple<string, string>>
            _ieStr = null;
        LocalModernWindowBase
            _window = null;
        readonly IList<IDisposable>
            _lsDisposable = new List<IDisposable> { };
        bool
            _bClosing = false;
        DateTime
            _dtConfirmingClose = DateTime.MinValue;
    }
}
