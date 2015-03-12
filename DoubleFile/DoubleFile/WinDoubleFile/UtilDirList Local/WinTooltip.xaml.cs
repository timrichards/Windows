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
            CloseTooltip();
            _winTooltip = new WinTooltip();
            _winTooltip.Folder = strFolder;
            _winTooltip.Size = strSize;
            _winTooltip.Show();

            var winOwner = winAnchor as LocalWindow;

            if (null != winOwner)
            {
                _winTooltip.Owner = winOwner;
                _winTooltip.Left = winOwner.Left;
                _winTooltip.Top = winOwner.Top + winOwner.Height;

                winOwner.MouseDown += CloseTooltip;
                winOwner.Closed += CloseTooltip;
            }
            
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
                winOwner.MouseDown -= CloseTooltip;
                winOwner.Closed -= CloseTooltip;

                winOwner.Activate();
            }
        }

        WinTooltip()
        {
            InitializeComponent();
  //          Visibility = System.Windows.Visibility.Hidden;
            Loaded += (o, e) => ++form_folder.FontSize;
            ContentRendered += WinTooltip_ContentRendered;
            MouseDown += (o, e) => _bMouseDown = true;
            MouseUp += (o, e) => { if ((null != MouseClicked) && _bMouseDown) MouseClicked(); };
        }

        void WinTooltip_ContentRendered(object sender, EventArgs e)
        {
            if (Top + Height > System.Windows.SystemParameters.PrimaryScreenHeight)
            {
                var winOwner = Owner as LocalWindow;

                if (null != winOwner)
                {
                    Top = winOwner.Top;
                    Left = winOwner.Left + winOwner.Width;

                    if (Left + Width > System.Windows.SystemParameters.PrimaryScreenWidth)
                    {
                        Top = winOwner.Top - Height;
                        Left = winOwner.Left;
                    }

                    if (Top < 0)
                        Top = winOwner.Top;
                }
            }

            Visibility = System.Windows.Visibility.Visible;
        }

        bool _bMouseDown = false;
        static WinTooltip _winTooltip = null;
    }
}
