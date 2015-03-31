using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Media;

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
        static internal TreeNode TreeNode { get { return (null == _winTooltip) ? null : _winTooltip.Tag as TreeNode; } }

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

        internal static void ShowTooltip(ArgsStruct args, Point ptAnchor, TreeNode treeNode)
        {
            if ((null != _winTooltip) &&
                (_winTooltip.Tag is LocalTreeNode))  // the other type
            {
                CloseTooltip();
            }

            FactoryCreateOrUpdate(args, treeNode, () =>
            {
                _winTooltip.Left = ptAnchor.X;
                _winTooltip.Top = ptAnchor.Y;

                if (null != args.winOwner)
                {
                    _winTooltip.Owner = args.winOwner;
                    args.winOwner.Closed += CloseTooltip;
                }

                _winTooltip._closingCallback = args.closingCallback;
            });
        }

        internal static void ShowTooltip(ArgsStruct args, LocalTreeNode treeNode)
        {
            if ((null != _winTooltip) &&
                (_winTooltip.Tag is TreeNode))  // the other type
            {
                CloseTooltip();
            }

            FactoryCreateOrUpdate(args, treeNode, () =>
            {
                var winOwner = args.winOwner;

                if (null == winOwner)
                    return; // from lambda

                _winTooltip.Owner = winOwner;
                _winTooltip.Left = winOwner.Left;
                _winTooltip.Top = winOwner.Top + winOwner.Height;
                winOwner.Closed += CloseTooltip;
                _winTooltip.SizeChanged += _winTooltip.WinTooltip_SizeChanged;
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

        internal static void CloseTooltip(object sender = null, EventArgs e = null)
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

            if ((null != winOwner) &&
                (false == winOwner.LocalIsClosing) &&
                (false == winOwner.LocalIsClosed))
            {
                winOwner.Closed -= CloseTooltip;
            }

            _winTooltip = null;
            _bClosingTooltip = false;
        }

        static WinTooltip()
        {
            //    App.DeactivateDidOccur += () => CloseTooltip();
        }

        WinTooltip()
        {
            InitializeComponent();
            WindowStyle = WindowStyle.None;
            SizeToContent = SizeToContent.WidthAndHeight;
            ResizeMode = ResizeMode.NoResize;
            Background = Brushes.LightYellow;
            Loaded += (o, e) => ++form_folder.FontSize;
            MouseDown += (o, e) => _bMouseDown = true;
            MouseUp += (o, e) => { if (_bMouseDown) _clickCallback(); _bMouseDown = false; };
        }

        void WinTooltip_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var winOwner = Owner as LocalWindow;

            if (null == winOwner)
                return;

            var nOwnerRight = winOwner.Left + winOwner.Width;
            var nOwnerBot = winOwner.Top + winOwner.Height;

            var rcTooltip = new Rect()
            {
                X = nOwnerRight - e.NewSize.Width,
                Y = nOwnerBot,
                Width = e.NewSize.Width,
                Height = e.NewSize.Height
            };

            //          var screen = Screen.
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

            if (0 > rcTooltip.X)
                rcTooltip.X = winOwner.Left;

            if (0 > rcTooltip.Y)
                rcTooltip.Y = winOwner.Top;

            Left = rcTooltip.X;
            Top = rcTooltip.Y;
        }

        bool
            _bMouseDown = false;
        static WinTooltip
            _winTooltip = null;
        Action
            _closingCallback = null;
        Action
            _clickCallback = null;
        static bool
            _bClosingTooltip = false;
    }
}
