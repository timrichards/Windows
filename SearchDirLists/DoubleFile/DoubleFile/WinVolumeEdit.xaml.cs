using System.Windows;

namespace DoubleFile
{
    /// <summary>
    /// Interaction logic for WinVolumeEdit.xaml
    /// </summary>
    public partial class WinVolumeEdit : Window
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

        public WinVolumeEdit()
        {
            InitializeComponent();
        }
    }
}
