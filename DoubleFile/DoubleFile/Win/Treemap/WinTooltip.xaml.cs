using System;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinTooltip.xaml
    /// </summary>
    public partial class WinTooltip : ICantBeTopWindow
    {
        internal string Folder { set { Util.UIthread(99822, () => formTextBlock_folder.Text = value); } }
        internal string Size { set { Util.UIthread(99821, () => formTextBlock_size.Text = value); } }

        static internal LocalTreeNode LocalTreeNode => _winTooltip?.Tag.As<LocalTreeNode>();

        internal struct ArgsStruct
        {
            internal ArgsStruct(
                string strFolder_in,
                string strSize_in,
                Window winOwner_in,
                Action clickCallback_in,
                Action closingCallback_in)
            {
                strFolder = strFolder_in;
                strSize = strSize_in;
                winOwner = winOwner_in;
                clickCallback = clickCallback_in;
                closingCallback = closingCallback_in;
            }

            readonly internal string strFolder;
            readonly internal string strSize;
            readonly internal Window winOwner;
            readonly internal Action clickCallback;
            readonly internal Action closingCallback;
        }

        static internal void ShowTooltip(ArgsStruct args, LocalTreeNode treeNode)
        {
            FactoryCreateOrUpdate(args, treeNode, () =>
            {
                var winOwner = args.winOwner;

                if (null == winOwner)
                    return; // from lambda

                Util.UIthread(99810, () =>
                {
                    _winTooltip.Owner = winOwner;
                    _winTooltip.Left = winOwner.Left;
                    _winTooltip.Top = winOwner.Top + winOwner.Height;
                });

                _winOwnerClosedObserver = Observable.FromEventPattern(winOwner, "Closed")
                    .LocalSubscribe(argsA => CloseTooltip());

                Observable.FromEventPattern<SizeChangedEventArgs>(_winTooltip, "SizeChanged")
                    .LocalSubscribe(argsA => _winTooltip.WinTooltip_SizeChanged(argsA.EventArgs.NewSize));
            });
        }

        static void FactoryCreateOrUpdate(ArgsStruct args, object tag, Action clientSpecific)
        {
            if ((null == _winTooltip) ||
                _winTooltip.LocalIsClosing ||
                _winTooltip.LocalIsClosed)
            {
                Util.UIthread(99808, () =>
                {
                    (_winTooltip = new WinTooltip { WindowStartupLocation = WindowStartupLocation.Manual })
                        .Show();

                    NativeMethods.SetWindowPos(_winTooltip, _winTooltip.Owner, 0, 0, 0, 0, SWP.NOSIZE | SWP.NOMOVE | SWP.NOACTIVATE);
                });

                clientSpecific();
            }

            _winTooltip.Folder = args.strFolder;
            _winTooltip.Size = args.strSize;
            Util.UIthread(99807, () => _winTooltip.Tag = tag);
            _winTooltip._closingCallback = args.closingCallback;
            _winTooltip._clickCallback = args.clickCallback;
            Util.AssertNutNull(99964, tag);
        }

        static internal void CloseTooltip()
        {
            if (Application.Current?.Dispatcher.HasShutdownStarted ?? true)
                return;

            if (_bClosingTooltip)
                return;

            Util.UIthread(99773, () =>
            {
                if (null != _winTooltip)
                    _winTooltip.Tag = null;
            });

            _bClosingTooltip = true;

            if ((null != _winTooltip) &&
                (false == _winTooltip.LocalIsClosing) &&
                (false == _winTooltip.LocalIsClosed))
            {
                _winTooltip._closingCallback?.Invoke();
                _winTooltip.Close();
            }

            _winOwnerClosedObserver?.Dispose();
            _winOwnerClosedObserver = null;
            _winTooltip = null;
            _bClosingTooltip = false;
        }

        WinTooltip()
        {
            InitializeComponent();
            WindowStyle = WindowStyle.None;
            SizeToContent = SizeToContent.WidthAndHeight;
            ResizeMode = ResizeMode.NoResize;
            Background = Brushes.LightYellow;

            Observable.FromEventPattern(this, "Loaded")
                .LocalSubscribe(x => ++formTextBlock_folder.FontSize);

            var bMouseDown = false;

            Observable.FromEventPattern(this, "MouseDown")
                .LocalSubscribe(x => bMouseDown = true);

            Observable.FromEventPattern(this, "MouseUp")
                .LocalSubscribe(x => { if (bMouseDown) _clickCallback?.Invoke(); bMouseDown = false; });
        }

        void WinTooltip_SizeChanged(Size newSize)
        {
            if (null == Owner)
                return;

            var nOwnerRight = Owner.Left + Owner.Width;
            var nOwnerBot = Owner.Top + Owner.Height;

            var rcTooltip = new Rect
            {
                X = nOwnerRight - newSize.Width,
                Y = nOwnerBot,
                Width = newSize.Width,
                Height = newSize.Height
            };

            if (WindowState.Maximized == Owner.WindowState)
            {
                rcTooltip.X = (SystemParameters.PrimaryScreenWidth - rcTooltip.Width) / 2d;
                rcTooltip.Y = 0;
            }

            Rect rcMonitor = Win32Screen.GetWindowMonitorInfo(Owner).rcMonitor;

            if (false == (rcMonitor.Contains(rcTooltip)))
            {
                rcTooltip.X = Owner.Left + Owner.Width - rcTooltip.Width;
                rcTooltip.Y = Owner.Top - rcTooltip.Height;
            }

            if (rcMonitor.Left > rcTooltip.X)
                rcTooltip.X = Owner.Left;

            if (rcMonitor.Top > rcTooltip.Y)
                rcTooltip.Y = Owner.Top;

            Left = rcTooltip.X;
            Top = rcTooltip.Y;
        }

        static WinTooltip
            _winTooltip = null;
        Action
            _closingCallback = null;
        Action
            _clickCallback = null;
        static bool
            _bClosingTooltip = false;
        static IDisposable
            _winOwnerClosedObserver = null;
    }
}
