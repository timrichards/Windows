using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media;
using System.Linq;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinTooltip.xaml
    /// </summary>
    public partial class WinTooltip
    {
        internal string Folder { set { form_folder.Text = value; } }
        internal string Size { set { form_size.Text = value; } }

        static internal LocalTreeNode LocalTreeNode { get { return (null == _winTooltip) ? null : _winTooltip.Tag as LocalTreeNode; } }

        internal struct ArgsStruct
        {
            internal ArgsStruct(
                string strFolder_in,
                string strSize_in,
                LocalWindow winOwner_in,
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
            readonly internal LocalWindow winOwner;
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

                _winTooltip.Owner = winOwner;
                _winTooltip.Left = winOwner.Left;
                _winTooltip.Top = winOwner.Top + winOwner.Height;

                _winOwnerClosedObserver = Observable.FromEventPattern(winOwner, "Closed")
                    .Subscribe(argsA => CloseTooltip());

                Observable.FromEventPattern<SizeChangedEventArgs>(_winTooltip, "SizeChanged")
                    .Subscribe(argsA => _winTooltip.WinTooltip_SizeChanged(argsA.EventArgs.NewSize));
            });
        }

        static void FactoryCreateOrUpdate(ArgsStruct args, object tag, Action clientSpecific)
        {
            if ((null == _winTooltip) ||
                _winTooltip.LocalIsClosing ||
                _winTooltip.LocalIsClosed)
            {
                _winTooltip = new WinTooltip { WindowStartupLocation = WindowStartupLocation.Manual };
                _winTooltip.Show();
                clientSpecific();
            }

            _winTooltip.Folder = args.strFolder;
            _winTooltip.Size = args.strSize;
            _winTooltip.Tag = tag;
            _winTooltip._closingCallback = args.closingCallback;
            _winTooltip._clickCallback = args.clickCallback;

            MBoxStatic.Assert(99964, null != tag);
        }

        static internal void CloseTooltip()
        {
            if (_bClosingTooltip)
                return;

            if (null != _winTooltip)
                _winTooltip.Tag = null;

            _bClosingTooltip = true;

            if ((null != _winTooltip) &&
                (false == _winTooltip.LocalIsClosing) &&
                (false == _winTooltip.LocalIsClosed))
            {
                if (null != _winTooltip._closingCallback)
                    _winTooltip._closingCallback();

                _winTooltip.Close();
            }

            LocalWindow winOwner = null;

            if (null != _winTooltip)
                winOwner = _winTooltip.Owner as LocalWindow;

            if (null != _winOwnerClosedObserver)
            {
                _winOwnerClosedObserver.Dispose();
                _winOwnerClosedObserver = null;
            }

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
                .Subscribe(args => ++form_folder.FontSize);

            var bMouseDown = false;

            Observable.FromEventPattern(this, "MouseDown")
                .Subscribe(args => bMouseDown = true);

            Observable.FromEventPattern(this, "MouseUp")
                .Subscribe(args => { if (bMouseDown && (null != _clickCallback)) _clickCallback(); bMouseDown = false; });
        }

        void WinTooltip_SizeChanged(Size newSize)
        {
            var winOwner = Owner as LocalWindow;

            if (null == winOwner)
                return;

            var nOwnerRight = winOwner.Left + winOwner.Width;
            var nOwnerBot = winOwner.Top + winOwner.Height;

            var rcTooltip = new Rect()
            {
                X = nOwnerRight - newSize.Width,
                Y = nOwnerBot,
                Width = newSize.Width,
                Height = newSize.Height
            };

            if (WindowState.Maximized == winOwner.WindowState)
            {
                rcTooltip.X = (SystemParameters.PrimaryScreenWidth - rcTooltip.Width) / 2.0;
                rcTooltip.Y = 0;
            }

            var rcMonitor = Win32Screen.GetOwnerMonitorRect(Owner);

            if (false == (rcMonitor.Contains(rcTooltip)))
            {
                rcTooltip.X = winOwner.Left + winOwner.Width - rcTooltip.Width;
                rcTooltip.Y = winOwner.Top - rcTooltip.Height;
            }

            if (rcMonitor.Left > rcTooltip.X)
                rcTooltip.X = winOwner.Left;

            if (rcMonitor.Top > rcTooltip.Y)
                rcTooltip.Y = winOwner.Top;

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
