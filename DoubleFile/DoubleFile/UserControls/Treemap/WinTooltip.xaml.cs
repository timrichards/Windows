using System;
using System.Collections.Generic;
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

                Util.UIthread(99810, () => _winTooltip.Owner = winOwner);

                var rc = Win32Screen.GetWindowRect(winOwner);

                _winTooltip.Left = rc.Left;
                _winTooltip.Top = rc.Top;

                _lsDisposable.Add(Observable.FromEventPattern(winOwner, "Closed")
                    .LocalSubscribe(99691, argsA => CloseTooltip()));

                _lsDisposable.Add(Observable.FromEventPattern(winOwner, "LocationChanged")
                    .LocalSubscribe(99673, argsA => CloseTooltip()));

                _lsDisposable.Add(Observable.FromEventPattern(winOwner, "SizeChanged")
                    .LocalSubscribe(99638, argsA => CloseTooltip()));
            });
        }

        static void FactoryCreateOrUpdate(ArgsStruct args, object tag, Action clientSpecific)
        {
            if ((null == _winTooltip) ||
                _winTooltip.LocalIsClosing ||
                _winTooltip.LocalIsClosed)
            {
                Util.UIthread(99808, () => (_winTooltip = new WinTooltip()).Show());
                clientSpecific();
            }

            _winTooltip.Folder = args.strFolder;
            _winTooltip.Size = args.strSize;
            Util.UIthread(99807, () => _winTooltip.Tag = tag);
            _winTooltip._closingCallback = args.closingCallback;
            _winTooltip._clickCallback = args.clickCallback;
            Util.AssertNotNull(99964, tag);
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
                _winTooltip.Close();
            }

            Util.LocalDispose(_lsDisposable);
            _lsDisposable.Clear();
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
                .LocalSubscribe(99689, x => ++formTextBlock_folder.FontSize);

            var bMouseDown = false;

            Observable.FromEventPattern(this, "MouseDown")
                .LocalSubscribe(99688, x => bMouseDown = true);

            Observable.FromEventPattern(this, "MouseUp")
                .LocalSubscribe(99687, x => { if (bMouseDown) _clickCallback?.Invoke(); bMouseDown = false; });

            Observable.FromEventPattern(this, "Closing")
                .LocalSubscribe(99672, x => _closingCallback?.Invoke());
        }

        static WinTooltip
            _winTooltip = null;
        Action
            _closingCallback = null;
        Action
            _clickCallback = null;
        static bool
            _bClosingTooltip = false;
        static readonly IList<IDisposable>
            _lsDisposable = new List<IDisposable>();
    }
}
