using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinVolumeNew.xaml
    /// </summary>
    partial class WinVolumeNew : Window
    {
        public string[] StringValues
        {
            get
            {
                return uc_VolumeEdit.StringValues;
            }
        }

        public WinVolumeNew()
        {
            InitializeComponent();
        }
    }
}
