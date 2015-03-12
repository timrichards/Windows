using System;
using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinFormAnalysis_DirList.xaml
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

            if (null != winAnchor)
            {
                _winTooltip.Owner = winAnchor;
                _winTooltip.Left = winAnchor.Left;
                _winTooltip.Top = winAnchor.Top;
            }
            
            return _winTooltip;
        }

        internal static void CloseTooltip()
        {
            if ((null != _winTooltip) &&
                (false == _winTooltip.LocalIsClosing) &&
                (false == _winTooltip.LocalIsClosed))
            {
                _winTooltip.Close();
            }
        }

        WinTooltip()
        {
            InitializeComponent();
            Loaded += (o, e) => ++form_folder.FontSize;
            Deactivated += (o, e) => CloseTooltip();
            MouseDown += (o, e) => _bMouseDown = true;
            MouseUp += (o, e) => { if ((null != MouseClicked) && _bMouseDown) MouseClicked(); };
        }

        bool _bMouseDown = false;
        static WinTooltip _winTooltip = null;
    }
}
