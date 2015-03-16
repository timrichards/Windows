using System;
using System.Windows;
using System.Windows.Forms;
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

        static internal event Action MouseClicked;
        static internal LocalTreeNode LocalTreeNode { get { return (null == _winTooltip) ? null : _winTooltip.Tag as LocalTreeNode; } }
        static internal TreeNode TreeNode { get { return (null == _winTooltip) ? null : _winTooltip.Tag as TreeNode; } }
        static internal TreeNodeProxy TreeNodeProxy { get { return (null == _winTooltip) ? null : _winTooltip.Tag as TreeNodeProxy; } }

        internal static void ShowTooltip(string strFolder, string strSize,
            Window winOwner, Point ptAnchor, TreeNode treeNode, Action closingCallback = null)
        {
            if ((null != _winTooltip) &&
                (_winTooltip.Tag is LocalTreeNode))  // the other type
            {
                CloseTooltip();
            }

            FactoryCreateOrUpdate(strFolder, strSize, treeNode, closingCallback, () =>
            {
                _winTooltip.Left = ptAnchor.X;
                _winTooltip.Top = ptAnchor.Y;

                if (null != winOwner)
                {
                    _winTooltip.Owner = winOwner;
                    winOwner.Closed += CloseTooltip;
                }

                _winTooltip._closingCallback = closingCallback;
            });
        }

        internal static void ShowTooltip(string strFolder, string strSize,
            Window winAnchor, LocalTreeNode treeNode, Action closingCallback = null)
        {
            if ((null != _winTooltip) &&
                (_winTooltip.Tag is TreeNode))  // the other type
            {
                CloseTooltip();
            }

            FactoryCreateOrUpdate(strFolder, strSize, treeNode, closingCallback, () =>
            {
                var winOwner = winAnchor as LocalWindow;

                if (null != winOwner)
                {
                    _winTooltip.Owner = winOwner;
                    _winTooltip.Left = winOwner.Left;
                    _winTooltip.Top = winOwner.Top + winOwner.Height;
                    winOwner.Closed += CloseTooltip;
                    _winTooltip.SizeChanged += _winTooltip.WinTooltip_SizeChanged;
                }
            });
        }

        static WinTooltip FactoryCreateOrUpdate(string strFolder, string strSize, object tag, Action closingCallback,
            Action clientSpecific)
        {
            if ((null == _winTooltip) ||
                _winTooltip.LocalIsClosing ||
                _winTooltip.LocalIsClosed)
            {
                _winTooltip = new WinTooltip();
                _winTooltip.WindowStartupLocation = WindowStartupLocation.Manual;
                _winTooltip.Show();
                clientSpecific();
            }

            _winTooltip.Folder = strFolder;
            _winTooltip.Size = strSize;
            _winTooltip.Tag = tag;
            _winTooltip._closingCallback = closingCallback;
            return _winTooltip;
        }

        internal static void CloseTooltip(object sender = null, EventArgs e = null)
        {
            if (null != _winTooltip)
                _winTooltip.Tag = null;

            if (_bClosingTooltip)
                return;

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
            MouseUp += (o, e) => { if ((null != MouseClicked) && _bMouseDown) MouseClicked(); };
        }

        void WinTooltip_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var winOwner = Owner as LocalWindow;

            if (null != winOwner)
            {
                var nWidth = e.NewSize.Width;
                var nHeight = e.NewSize.Height;
                var nLeft = winOwner.Left;
                var nTop = winOwner.Top + winOwner.Height;

                if (WindowState.Maximized == winOwner.WindowState)
                {
                    nLeft = (SystemParameters.PrimaryScreenWidth - nWidth) / 2.0;
                    nTop = 0;
                }

                if (SystemParameters.PrimaryScreenHeight < nTop + nHeight)
                {
                    nLeft = winOwner.Left + winOwner.Width;
                    nTop = winOwner.Top;
                }

                if (SystemParameters.PrimaryScreenWidth < nLeft + nWidth)
                {
                    nLeft = winOwner.Left + winOwner.Width - nWidth;
                    nTop = winOwner.Top - nHeight;
                }

                if (0 > nLeft)
                    nLeft = winOwner.Left;

                if (0 > nTop)
                    nTop = winOwner.Top;

                Left = nLeft;
                Top = nTop;
            }
        }

        bool
            _bMouseDown = false;
        static WinTooltip
            _winTooltip = null;
        Action
            _closingCallback = null;
        static bool
            _bClosingTooltip = false;
    }
}
