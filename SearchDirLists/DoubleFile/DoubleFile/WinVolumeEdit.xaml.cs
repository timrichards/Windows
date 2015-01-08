using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinVolumeEdit.xaml
    /// </summary>
    partial class WinVolumeEdit : Window, IWinVolumeEdit
    {
        public string[] StringValues
        {
            get
            {
                return uc_VolumeEdit.StringValues;
            }

            set
            {
                uc_VolumeEdit.StringValues = value;
            }
        }

        public bool? ShowDialog(System.Windows.Window me) { Owner = me; return base.ShowDialog(); }

        public WinVolumeEdit()
        {
            InitializeComponent();
        }
    }
}
