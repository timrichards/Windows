using System;
using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinFormDirList.xaml
    /// </summary>
    public partial class WinTooltip
    {
        static internal event Action MouseClicked;
        internal string Folder { set { form_folder.Text = value; } }
        internal string Size { set { form_size.Text = value; } }

        internal static WinTooltip ShowTooltip(string strFolder, string strSize, Window winAnchor)
        {
            if ((null == _winTooltip) ||
                _winTooltip.LocalIsClosing ||
                _winTooltip.LocalIsClosed)
            {
                _winTooltip = new WinTooltip();

                _winTooltip.WindowStartupLocation = WindowStartupLocation.Manual;
                _winTooltip.Show();

                var winOwner = winAnchor as LocalWindow;

                if (null != winOwner)
                {
                    _winTooltip.Owner = winOwner;
                    _winTooltip.Left = winOwner.Left;
                    _winTooltip.Top = winOwner.Top + winOwner.Height;

                    winOwner.Closed += CloseTooltip;
                }
            }

            _winTooltip.Folder = strFolder;
            _winTooltip.Size = strSize;
            return _winTooltip;
        }

        internal static void CloseTooltip(object sender = null, EventArgs e = null)
        {
            if ((null != _winTooltip) &&
                (false == _winTooltip.LocalIsClosing) &&
                (false == _winTooltip.LocalIsClosed))
            {
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

                winOwner.Activate();
            }
        }

        WinTooltip()
        {
            InitializeComponent();
            Loaded += (o, e) => ++form_folder.FontSize;
            MouseDown += (o, e) => _bMouseDown = true;
            MouseUp += (o, e) => { if ((null != MouseClicked) && _bMouseDown) MouseClicked(); };
            SizeChanged += WinTooltip_SizeChanged;
        }

        void WinTooltip_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var winOwner = Owner as LocalWindow;

            if (null != winOwner)
            {
                var nWidth = e.NewSize.Width;
                var nHeight = e.NewSize.Height;

                var bMaxd = (winOwner.WindowState == WindowState.Maximized);

                var nLeft = bMaxd
                    ? (SystemParameters.PrimaryScreenWidth - nWidth) / 2.0
                    : winOwner.Left;

                var nTop = bMaxd ? 0 : winOwner.Top + winOwner.Height;

                if (nTop + nHeight > SystemParameters.PrimaryScreenHeight)
                {
                    nTop = winOwner.Top;
                    nLeft = winOwner.Left + winOwner.Width;
                }

                if (nLeft + nWidth > SystemParameters.PrimaryScreenWidth)
                {
                    nTop = winOwner.Top - nHeight;
                    nLeft = winOwner.Left + winOwner.Width - nWidth;
                }

                if (nLeft < 0)
                    nLeft = bMaxd ? 0 : winOwner.Left;

                if (nTop < 0)
                    nTop = bMaxd ? 0 : winOwner.Top;

                Left = nLeft;
                Top = nTop;
            }
        }

        bool _bMouseDown = false;
        static WinTooltip _winTooltip = null;
    }
}
